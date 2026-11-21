using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.OrdenesTrabajo;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Vehiculos;
using TallerOS.Infrastructure.Persistence;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Prueba de integración contra Postgres real (mismo motivo que AislamientoEntreEmpresasTests).
///
/// Cubre la condición más importante que puso el usuario al aprobar el Backend del Módulo 4: que
/// tener empresa_id en las tablas dependientes y tener RLS en ellas NO basta para impedir que una
/// orden_trabajo de la empresa A apunte a una sucursal/cliente/estado_ot/personal de la empresa B.
/// Eso lo impide la clave foránea COMPUESTA (empresa_id, id) de cada relación (ver
/// OrdenTrabajoConfiguration): PostgreSQL rechaza el INSERT con una violación de FK incluso si RLS
/// nunca llegó a evaluarse (de hecho estas pruebas corren como la propia empresa A, sin elevar
/// privilegios, así que RLS ni entra en juego). Las 4 pruebas cubren los 4 campos de orden_trabajo
/// que usan ese patrón, cada una violando solo ESE campo y dejando los demás válidos para A; el
/// resto de tablas del módulo (historial_estado_ot, recepcion, danio_previo, subtarea) generan su
/// clave compuesta con el mismo método de Fluent API, así que no repiten el caso.</summary>
public sealed class Modulo4FkCompuestaRechazaCrucesDeEmpresaTests
{
    private static readonly Guid EmpresaAId = Guid.Parse("e598ae6b-3823-446a-b9b3-3229deb3d134");

    private sealed class TenantContextFijo(Guid? empresaId, bool esSuperAdmin) : ITenantContext
    {
        public Guid? EmpresaId { get; } = empresaId;
        public bool EsSuperAdmin { get; } = esSuperAdmin;
    }

    private static AppDbContext CrearContexto(ITenantContext tenant)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__App")
            ?? throw new InvalidOperationException(
                "Esta prueba necesita Postgres local con las migraciones aplicadas y la variable " +
                "de entorno ConnectionStrings__App (la misma cadena que usa la Api; ver .env).");

        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new TenantSessionInterceptor(tenant))
            .Options;

        return new AppDbContext(opciones, tenant);
    }

    /// <summary>Crea la empresa B y, con el super admin ya elevado, una sucursal/cliente/estado/
    /// personal propios de B — las cuatro cosas que las pruebas de abajo intentarán colar en una
    /// orden_trabajo de la empresa A.</summary>
    private static async Task<(Empresa empresa, Guid sucursalId, Guid clienteId, Guid estadoId, Guid personalId)>
        CrearEmpresaBConDependenciasAsync(AppDbContext setup)
    {
        var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
        var empresaB = new Empresa("Empresa B (prueba FK compuesta Módulo 4)", rucDePrueba);
        setup.Empresas.Add(empresaB);

        var sucursalB = new Sucursal(empresaB.Id, "Sucursal B (prueba FK compuesta)", direccion: null);
        setup.Sucursales.Add(sucursalB);

        var estadoB = new EstadoOt(empresaB.Id, "Estado B (prueba FK compuesta)", orden: 1, esFinal: false, colorToken: "sf3");
        setup.EstadosOt.Add(estadoB);

        await setup.SaveChangesAsync(CancellationToken.None);

        var areaB = new Area(empresaB.Id, sucursalB.Id, "Área B (prueba FK compuesta)");
        setup.Areas.Add(areaB);
        await setup.SaveChangesAsync(CancellationToken.None);

        var personalB = new Personal(empresaB.Id, areaB.Id, "Personal B (prueba FK compuesta)",
            tarifaHora: null, tieneAcceso: false, usuarioId: null, pinHash: null);
        setup.Personal.Add(personalB);

        var identificacionB = Random.Shared.NextInt64(1_000_000_000, 1_999_999_999).ToString();
        var clienteB = new Cliente(empresaB.Id, "Cliente B (prueba FK compuesta)", TipoCliente.PersonaNatural,
            identificacionB, telefono: null, email: null, CanalPreferido.WhatsApp);
        setup.Clientes.Add(clienteB);

        await setup.SaveChangesAsync(CancellationToken.None);

        return (empresaB, sucursalB.Id, clienteB.Id, estadoB.Id, personalB.Id);
    }

    private static async Task LimpiarEmpresaBAsync(AppDbContext setup, Guid empresaBId)
    {
        setup.Clientes.RemoveRange(await setup.Clientes.IgnoreQueryFilters().Where(c => c.EmpresaId == empresaBId).ToListAsync(CancellationToken.None));
        setup.Personal.RemoveRange(await setup.Personal.IgnoreQueryFilters().Where(p => p.EmpresaId == empresaBId).ToListAsync(CancellationToken.None));
        setup.EstadosOt.RemoveRange(await setup.EstadosOt.IgnoreQueryFilters().Where(e => e.EmpresaId == empresaBId).ToListAsync(CancellationToken.None));
        setup.Areas.RemoveRange(await setup.Areas.IgnoreQueryFilters().Where(a => a.EmpresaId == empresaBId).ToListAsync(CancellationToken.None));
        setup.Sucursales.RemoveRange(await setup.Sucursales.IgnoreQueryFilters().Where(s => s.EmpresaId == empresaBId).ToListAsync(CancellationToken.None));
        await setup.SaveChangesAsync(CancellationToken.None);

        setup.Empresas.Remove(await setup.Empresas.IgnoreQueryFilters().FirstAsync(e => e.Id == empresaBId, CancellationToken.None));
        await setup.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>Arma, con datos válidos para la empresa A, todo lo que una orden_trabajo necesita
    /// (marca/modelo/vehículo, cliente, sucursal, el estado inicial ya sembrado), le entrega esos
    /// cuatro ids válidos a <paramref name="armarOt"/> para que la prueba cambie exactamente UNO por
    /// el de la empresa B, y comprueba que Postgres rechaza el INSERT. Todo corre dentro de una
    /// transacción de A que nunca se confirma: ni siquiera lo válido llega a persistir.</summary>
    private static async Task AssertOtEsRechazadaAsync(
        Func<(Guid sucursalId, Guid vehiculoId, Guid clienteId, Guid estadoId, Guid? asesorPersonalId), OrdenTrabajo> armarOt)
    {
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var db = CrearContexto(tenantA);
        try
        {
            await using var transaccion = await db.Database.BeginTransactionAsync(CancellationToken.None);

            var marca = new Marca(1, $"Marca de prueba {Guid.NewGuid():N}");
            db.Marcas.Add(marca);
            var modelo = new Modelo(marca.Id, $"Modelo de prueba {Guid.NewGuid():N}");
            db.Modelos.Add(modelo);
            var vehiculo = new Vehiculo(vin: null, marca.Id, modelo.Id, anio: 2020, motor: null);
            db.Vehiculos.Add(vehiculo);

            var identificacion = Random.Shared.NextInt64(1_000_000_000, 1_999_999_999).ToString();
            var cliente = new Cliente(EmpresaAId, "Cliente de prueba (FK compuesta)", TipoCliente.PersonaNatural,
                identificacion, telefono: null, email: null, CanalPreferido.WhatsApp);
            db.Clientes.Add(cliente);

            var sucursalA = new Sucursal(EmpresaAId, "Sucursal A (prueba FK compuesta)", direccion: null);
            db.Sucursales.Add(sucursalA);

            await db.SaveChangesAsync(CancellationToken.None);

            var estadoA = await db.EstadosOt.OrderBy(e => e.Orden).FirstAsync(CancellationToken.None);

            var ot = armarOt((sucursalA.Id, vehiculo.Id, cliente.Id, estadoA.Id, null));
            db.OrdenesTrabajo.Add(ot);

            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(CancellationToken.None));
        }
        finally
        {
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task Orden_trabajo_con_sucursal_de_otra_empresa_es_rechazada_por_fk_compuesta()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var (empresaB, sucursalBId, _, _, _) = await CrearEmpresaBConDependenciasAsync(setup);
            try
            {
                await AssertOtEsRechazadaAsync(v => new OrdenTrabajo(
                    EmpresaAId, sucursalBId, 1, v.vehiculoId, v.clienteId, v.estadoId, PrioridadOt.Normal, 50_000, v.asesorPersonalId));
            }
            finally
            {
                await LimpiarEmpresaBAsync(setup, empresaB.Id);
            }
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }

    [Fact]
    public async Task Orden_trabajo_con_cliente_de_otra_empresa_es_rechazada_por_fk_compuesta()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var (empresaB, _, clienteBId, _, _) = await CrearEmpresaBConDependenciasAsync(setup);
            try
            {
                await AssertOtEsRechazadaAsync(v => new OrdenTrabajo(
                    EmpresaAId, v.sucursalId, 1, v.vehiculoId, clienteBId, v.estadoId, PrioridadOt.Normal, 50_000, v.asesorPersonalId));
            }
            finally
            {
                await LimpiarEmpresaBAsync(setup, empresaB.Id);
            }
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }

    [Fact]
    public async Task Orden_trabajo_con_estado_de_otra_empresa_es_rechazada_por_fk_compuesta()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var (empresaB, _, _, estadoBId, _) = await CrearEmpresaBConDependenciasAsync(setup);
            try
            {
                await AssertOtEsRechazadaAsync(v => new OrdenTrabajo(
                    EmpresaAId, v.sucursalId, 1, v.vehiculoId, v.clienteId, estadoBId, PrioridadOt.Normal, 50_000, v.asesorPersonalId));
            }
            finally
            {
                await LimpiarEmpresaBAsync(setup, empresaB.Id);
            }
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }

    [Fact]
    public async Task Orden_trabajo_con_asesor_de_otra_empresa_es_rechazada_por_fk_compuesta()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var (empresaB, _, _, _, personalBId) = await CrearEmpresaBConDependenciasAsync(setup);
            try
            {
                await AssertOtEsRechazadaAsync(v => new OrdenTrabajo(
                    EmpresaAId, v.sucursalId, 1, v.vehiculoId, v.clienteId, v.estadoId, PrioridadOt.Normal, 50_000, personalBId));
            }
            finally
            {
                await LimpiarEmpresaBAsync(setup, empresaB.Id);
            }
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }
}
