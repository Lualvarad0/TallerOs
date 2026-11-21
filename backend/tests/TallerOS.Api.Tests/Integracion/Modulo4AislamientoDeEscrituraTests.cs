using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.OrdenesTrabajo;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Vehiculos;
using TallerOS.Infrastructure.Persistence;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Prueba de integración contra Postgres real (mismo motivo que AislamientoEntreEmpresasTests).
///
/// AislamientoEntreEmpresasTests ya prueba las dos barreras de LECTURA (filtro de EF + RLS de
/// Postgres). Esta clase cubre lo que esa prueba no cubre y que el usuario pidió explícitamente al
/// aprobar el Backend del Módulo 4: que las políticas RLS de las tablas nuevas también protegen
/// ESCRITURAS, no solo lecturas. Dos formas de escritura, cada una probada en orden_trabajo (tabla
/// principal del módulo) y en estado_ot (una tabla hija, para no depender de un solo caso):
///
/// 1. INSERT: la cláusula WITH CHECK debe rechazar una fila cuyo empresa_id no coincide con la
///    empresa de la sesión, aunque la fila sea perfectamente válida para la OTRA empresa (claves
///    foráneas compuestas incluidas) — si no fuera por RLS, el INSERT no tendría ningún motivo para
///    fallar.
/// 2. UPDATE: la cláusula USING debe ocultar una fila que de verdad pertenece a otra empresa,
///    incluso cuando la consulta la busca con IgnoreQueryFilters() (es decir, aunque el filtro de EF
///    ya no la esconda): el UPDATE debe afectar 0 filas, no lanzar una excepción ni "tener éxito"
///    modificando algo.</summary>
public sealed class Modulo4AislamientoDeEscrituraTests
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

    private static async Task<Empresa> CrearEmpresaBAsync(AppDbContext setup)
    {
        var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
        var empresaB = new Empresa("Empresa B (prueba aislamiento de escritura)", rucDePrueba);
        setup.Empresas.Add(empresaB);
        await setup.SaveChangesAsync(CancellationToken.None);
        return empresaB;
    }

    [Fact]
    public async Task EmpresaA_no_puede_insertar_un_estado_ot_cuyo_empresa_id_es_de_empresa_B()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var empresaB = await CrearEmpresaBAsync(setup);
            try
            {
                var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
                var db = CrearContexto(tenantA);
                try
                {
                    await using var transaccion = await db.Database.BeginTransactionAsync(CancellationToken.None);

                    // La fila es internamente consistente (el estado le pertenece a empresaB): si el
                    // INSERT falla, es solo porque la SESIÓN está autenticada como empresa A, que es
                    // justo lo que WITH CHECK debe impedir.
                    var estadoDeB = new EstadoOt(empresaB.Id, "Estado colado (prueba de escritura)", orden: 50, esFinal: false, colorToken: "sf3");
                    db.EstadosOt.Add(estadoDeB);

                    await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(CancellationToken.None));
                }
                finally
                {
                    await db.DisposeAsync();
                }
            }
            finally
            {
                setup.Empresas.Remove(empresaB);
                await setup.SaveChangesAsync(CancellationToken.None);
            }
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }

    [Fact]
    public async Task EmpresaA_no_puede_actualizar_un_estado_ot_que_de_verdad_pertenece_a_empresa_B()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var empresaB = await CrearEmpresaBAsync(setup);
            var estadoDeB = new EstadoOt(empresaB.Id, "Estado de B (prueba de escritura)", orden: 50, esFinal: false, colorToken: "sf3");
            setup.EstadosOt.Add(estadoDeB);
            await setup.SaveChangesAsync(CancellationToken.None);

            try
            {
                var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
                var db = CrearContexto(tenantA);
                try
                {
                    // IgnoreQueryFilters a propósito: si no, el filtro de EF ya le ocultaría la fila
                    // ANTES de llegar a Postgres, y esta prueba terminaría comprobando la primera
                    // barrera (ya cubierta por AislamientoEntreEmpresasTests), no la segunda.
                    var filasAfectadas = await db.EstadosOt.IgnoreQueryFilters()
                        .Where(e => e.Id == estadoDeB.Id)
                        .ExecuteUpdateAsync(s => s.SetProperty(e => e.Nombre, "Modificado por A (no debe pasar)"), CancellationToken.None);

                    Assert.Equal(0, filasAfectadas);
                }
                finally
                {
                    await db.DisposeAsync();
                }

                // Confirma además que el nombre original sigue intacto (no solo que "0 filas" se
                // reportó, sino que de verdad no cambió nada).
                await using var verificacion = CrearContexto(superAdmin);
                var estadoSinCambios = await verificacion.EstadosOt.IgnoreQueryFilters()
                    .SingleAsync(e => e.Id == estadoDeB.Id, CancellationToken.None);
                Assert.Equal("Estado de B (prueba de escritura)", estadoSinCambios.Nombre);
            }
            finally
            {
                setup.EstadosOt.Remove(estadoDeB);
                setup.Empresas.Remove(empresaB);
                await setup.SaveChangesAsync(CancellationToken.None);
            }
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }

    [Fact]
    public async Task EmpresaA_no_puede_insertar_una_orden_trabajo_cuyo_empresa_id_es_de_empresa_B()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var empresaB = await CrearEmpresaBAsync(setup);
            var sucursalB = new Sucursal(empresaB.Id, "Sucursal B (prueba de escritura)", direccion: null);
            setup.Sucursales.Add(sucursalB);
            var estadoB = new EstadoOt(empresaB.Id, "Estado B (prueba de escritura)", orden: 1, esFinal: false, colorToken: "sf3");
            setup.EstadosOt.Add(estadoB);
            var marca = new Marca(1, $"Marca de prueba {Guid.NewGuid():N}");
            setup.Marcas.Add(marca);
            var modelo = new Modelo(marca.Id, $"Modelo de prueba {Guid.NewGuid():N}");
            setup.Modelos.Add(modelo);
            var vehiculo = new Vehiculo(vin: null, marca.Id, modelo.Id, anio: 2020, motor: null);
            setup.Vehiculos.Add(vehiculo);
            var identificacionB = Random.Shared.NextInt64(1_000_000_000, 1_999_999_999).ToString();
            var clienteB = new TallerOS.Domain.Clientes.Cliente(empresaB.Id, "Cliente B (prueba de escritura)",
                TallerOS.Domain.Clientes.TipoCliente.PersonaNatural, identificacionB, telefono: null, email: null,
                TallerOS.Domain.Clientes.CanalPreferido.WhatsApp);
            setup.Clientes.Add(clienteB);
            await setup.SaveChangesAsync(CancellationToken.None);

            try
            {
                var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
                var db = CrearContexto(tenantA);
                try
                {
                    await using var transaccion = await db.Database.BeginTransactionAsync(CancellationToken.None);

                    // Todo el resto de la fila (sucursal, cliente, estado, vehículo) es consistente
                    // para empresaB: la única cosa que puede hacer fallar este INSERT bajo la sesión
                    // de empresa A es RLS, no la clave foránea compuesta (ya probada por separado en
                    // Modulo4FkCompuestaRechazaCrucesDeEmpresaTests).
                    var otColada = new OrdenTrabajo(empresaB.Id, sucursalB.Id, 1, vehiculo.Id, clienteB.Id,
                        estadoB.Id, PrioridadOt.Normal, 10_000, asesorPersonalId: null);
                    db.OrdenesTrabajo.Add(otColada);

                    await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(CancellationToken.None));
                }
                finally
                {
                    await db.DisposeAsync();
                }
            }
            finally
            {
                setup.Clientes.Remove(clienteB);
                setup.EstadosOt.Remove(estadoB);
                setup.Sucursales.Remove(sucursalB);
                setup.Empresas.Remove(empresaB);
                await setup.SaveChangesAsync(CancellationToken.None);
            }
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }

    [Fact]
    public async Task EmpresaA_no_puede_actualizar_una_orden_trabajo_que_de_verdad_pertenece_a_empresa_B()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var empresaB = await CrearEmpresaBAsync(setup);
            var sucursalB = new Sucursal(empresaB.Id, "Sucursal B (prueba de escritura, update)", direccion: null);
            setup.Sucursales.Add(sucursalB);
            var estadoB = new EstadoOt(empresaB.Id, "Estado B (prueba de escritura, update)", orden: 1, esFinal: false, colorToken: "sf3");
            setup.EstadosOt.Add(estadoB);
            var marca = new Marca(1, $"Marca de prueba {Guid.NewGuid():N}");
            setup.Marcas.Add(marca);
            var modelo = new Modelo(marca.Id, $"Modelo de prueba {Guid.NewGuid():N}");
            setup.Modelos.Add(modelo);
            var vehiculo = new Vehiculo(vin: null, marca.Id, modelo.Id, anio: 2020, motor: null);
            setup.Vehiculos.Add(vehiculo);
            var identificacionB = Random.Shared.NextInt64(1_000_000_000, 1_999_999_999).ToString();
            var clienteB = new TallerOS.Domain.Clientes.Cliente(empresaB.Id, "Cliente B (prueba de escritura, update)",
                TallerOS.Domain.Clientes.TipoCliente.PersonaNatural, identificacionB, telefono: null, email: null,
                TallerOS.Domain.Clientes.CanalPreferido.WhatsApp);
            setup.Clientes.Add(clienteB);
            await setup.SaveChangesAsync(CancellationToken.None);

            var otDeB = new OrdenTrabajo(empresaB.Id, sucursalB.Id, 1, vehiculo.Id, clienteB.Id,
                estadoB.Id, PrioridadOt.Normal, 10_000, asesorPersonalId: null);
            setup.OrdenesTrabajo.Add(otDeB);
            await setup.SaveChangesAsync(CancellationToken.None);

            try
            {
                var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
                var db = CrearContexto(tenantA);
                try
                {
                    var filasAfectadas = await db.OrdenesTrabajo.IgnoreQueryFilters()
                        .Where(o => o.Id == otDeB.Id)
                        .ExecuteUpdateAsync(s => s.SetProperty(o => o.KmIngreso, 999_999), CancellationToken.None);

                    Assert.Equal(0, filasAfectadas);
                }
                finally
                {
                    await db.DisposeAsync();
                }

                await using var verificacion = CrearContexto(superAdmin);
                var otSinCambios = await verificacion.OrdenesTrabajo.IgnoreQueryFilters()
                    .SingleAsync(o => o.Id == otDeB.Id, CancellationToken.None);
                Assert.Equal(10_000, otSinCambios.KmIngreso);
            }
            finally
            {
                setup.OrdenesTrabajo.Remove(otDeB);
                setup.Clientes.Remove(clienteB);
                setup.EstadosOt.Remove(estadoB);
                setup.Sucursales.Remove(sucursalB);
                setup.Empresas.Remove(empresaB);
                await setup.SaveChangesAsync(CancellationToken.None);
            }
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }
}
