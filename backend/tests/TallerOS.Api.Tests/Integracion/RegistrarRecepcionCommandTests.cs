using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.OrdenesTrabajo.Commands.RegistrarRecepcion;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.OrdenesTrabajo;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Vehiculos;
using TallerOS.Infrastructure.Persistence;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Pruebas de integración de RegistrarRecepcion contra Postgres real (mismo motivo que
/// AislamientoEntreEmpresasTests). Además del camino feliz, cubre la condición de numeración que
/// puso el usuario al aprobar el Backend del Módulo 4: la numeración de OT debe ser atómica por
/// empresa — dos recepciones simultáneas para la misma empresa deben obtener números DISTINTOS, sin
/// que el índice único (empresa_id, numero) llegue a chocar. A diferencia de
/// Registros_simultaneos_con_el_mismo_vin_nuevo_solo_uno_tiene_exito (RegistrarVehiculoCommandTests),
/// aquí la carrera no tiene un "perdedor": SiguienteNumeroOtAsync usa UPDATE...RETURNING sobre
/// contador_numero_ot, que Postgres serializa fila por fila, así que ambas peticiones deben tener
/// éxito.</summary>
public sealed class RegistrarRecepcionCommandTests
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

    /// <summary>Crea, con COMMIT real (no en una transacción que se deshace): marca/modelo/vehículo,
    /// cliente y el vínculo cliente_vehiculo que RegistrarRecepcionCommandHandler exige. Necesita
    /// commit real porque la prueba de concurrencia usa DOS conexiones nuevas que solo ven lo que ya
    /// está confirmado.</summary>
    private static async Task<Guid> SembrarSucursalAsync(AppDbContext db, Guid empresaId)
    {
        var sucursal = new Sucursal(empresaId, $"Sucursal de prueba {Guid.NewGuid():N}", direccion: null);
        db.Sucursales.Add(sucursal);
        await db.SaveChangesAsync(CancellationToken.None);
        return sucursal.Id;
    }

    // historial_estado_ot tiene una FK (no compuesta) a usuario: no vale cualquier Guid, tiene que
    // ser un usuario real. usuario es un catálogo global (sin empresa_id, igual que vehiculo), así
    // que crearlo no depende del tenant de la sesión.
    private static async Task<Guid> SembrarUsuarioAsync(AppDbContext db)
    {
        var usuario = new TallerOS.Domain.Seguridad.Usuario($"usuario.prueba.{Guid.NewGuid():N}@talleros-test.local", "hash-no-usado");
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(CancellationToken.None);
        return usuario.Id;
    }

    private static async Task<(Guid vehiculoId, Guid clienteId)> SembrarVehiculoClienteYVinculoAsync(AppDbContext db, Guid empresaId)
    {
        var marca = new Marca(1, $"Marca de prueba {Guid.NewGuid():N}");
        db.Marcas.Add(marca);
        var modelo = new Modelo(marca.Id, $"Modelo de prueba {Guid.NewGuid():N}");
        db.Modelos.Add(modelo);
        var vehiculo = new Vehiculo(vin: null, marca.Id, modelo.Id, anio: 2020, motor: null);
        db.Vehiculos.Add(vehiculo);

        var identificacion = Random.Shared.NextInt64(1_000_000_000, 1_999_999_999).ToString();
        var cliente = new Cliente(empresaId, "Cliente de prueba (RegistrarRecepcion)", TipoCliente.PersonaNatural,
            identificacion, telefono: null, email: null, CanalPreferido.WhatsApp);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(CancellationToken.None);

        db.ClienteVehiculos.Add(new ClienteVehiculo(empresaId, cliente.Id, vehiculo.Id, RolClienteVehiculo.Propietario));
        await db.SaveChangesAsync(CancellationToken.None);

        return (vehiculo.Id, cliente.Id);
    }

    /// <summary>Borra lo que sí tiene política RLS de DELETE para su propia empresa (cliente_vehiculo
    /// y cliente). A propósito NO toca vehiculo/marca/modelo, por el mismo motivo documentado en
    /// RegistrarVehiculoCommandTests. orden_trabajo/recepcion/danio_previo/historial_estado_ot SÍ
    /// tienen política de DELETE (usan la misma aislamiento_empresa general que cliente, no la
    /// ausencia deliberada de vehiculo), así que esos se borran también.</summary>
    private static async Task LimpiarAsync(Guid empresaId, Guid clienteId, IReadOnlyList<Guid> otIds,
        Guid? sucursalId = null, IReadOnlyList<Guid>? usuarioIds = null)
    {
        var tenantA = new TenantContextFijo(empresaId, esSuperAdmin: false);
        var db = CrearContexto(tenantA);
        try
        {
            db.HistorialEstadoOt.RemoveRange(await db.HistorialEstadoOt.Where(h => otIds.Contains(h.OtId)).ToListAsync(CancellationToken.None));
            db.DaniosPrevios.RemoveRange(await db.DaniosPrevios.Where(d => db.Recepciones.Any(r => otIds.Contains(r.OtId) && r.Id == d.RecepcionId)).ToListAsync(CancellationToken.None));
            db.Recepciones.RemoveRange(await db.Recepciones.Where(r => otIds.Contains(r.OtId)).ToListAsync(CancellationToken.None));
            db.OrdenesTrabajo.RemoveRange(await db.OrdenesTrabajo.Where(o => otIds.Contains(o.Id)).ToListAsync(CancellationToken.None));
            await db.SaveChangesAsync(CancellationToken.None);

            db.ClienteVehiculos.RemoveRange(await db.ClienteVehiculos.Where(cv => cv.ClienteId == clienteId).ToListAsync(CancellationToken.None));
            db.Clientes.RemoveRange(await db.Clientes.Where(c => c.Id == clienteId).ToListAsync(CancellationToken.None));
            if (sucursalId is { } id)
            {
                db.Sucursales.RemoveRange(await db.Sucursales.Where(s => s.Id == id).ToListAsync(CancellationToken.None));
            }

            await db.SaveChangesAsync(CancellationToken.None);

            if (usuarioIds is { Count: > 0 })
            {
                db.Usuarios.RemoveRange(await db.Usuarios.Where(u => usuarioIds.Contains(u.Id)).ToListAsync(CancellationToken.None));
                await db.SaveChangesAsync(CancellationToken.None);
            }
        }
        finally
        {
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task Registrar_recepcion_crea_la_ot_en_el_primer_estado_y_registra_el_historial_inicial()
    {
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var dbSetup = CrearContexto(tenantA);
        var (vehiculoId, clienteId) = await SembrarVehiculoClienteYVinculoAsync(dbSetup, EmpresaAId);
        var sucursalId = await SembrarSucursalAsync(dbSetup, EmpresaAId);
        var usuarioId = await SembrarUsuarioAsync(dbSetup);
        await dbSetup.DisposeAsync();

        var db = CrearContexto(tenantA);
        var otId = Guid.Empty;
        try
        {
            var handler = new RegistrarRecepcionCommandHandler(db, tenantA);
            var comando = new RegistrarRecepcionCommand(
                sucursalId, vehiculoId, clienteId, PrioridadOt.Normal, KmIngreso: 45_000,
                AsesorPersonalId: null, Combustible: 4, Objetos: "Gato, llave de ruedas",
                Observaciones: null,
                DaniosPrevios: [new DanioPrevioInput("puerta_delantera_izquierda", TipoDanio.Rayon, "Rayón leve", null)],
                UsuarioId: usuarioId);

            var resultado = await handler.Handle(comando, CancellationToken.None);
            otId = resultado.OtId;

            var estadoInicial = await db.EstadosOt.OrderBy(e => e.Orden).FirstAsync(CancellationToken.None);
            Assert.Equal(estadoInicial.Id, resultado.EstadoId);
            Assert.True(resultado.Numero > 0);

            var ot = await db.OrdenesTrabajo.FirstAsync(o => o.Id == otId, CancellationToken.None);
            Assert.Equal(estadoInicial.Id, ot.EstadoId);
            Assert.Equal(resultado.Numero, ot.Numero);

            var recepcion = await db.Recepciones.SingleAsync(r => r.OtId == otId, CancellationToken.None);
            Assert.Equal((short)4, recepcion.Combustible);

            var danios = await db.DaniosPrevios.Where(d => d.RecepcionId == recepcion.Id).ToListAsync(CancellationToken.None);
            Assert.Single(danios, d => d.Zona == "puerta_delantera_izquierda");

            var historial = await db.HistorialEstadoOt.SingleAsync(h => h.OtId == otId, CancellationToken.None);
            Assert.Null(historial.DeEstadoId);
            Assert.Equal(estadoInicial.Id, historial.AEstadoId);
            Assert.Equal(usuarioId, historial.UsuarioId);
        }
        finally
        {
            await db.DisposeAsync();
            await LimpiarAsync(EmpresaAId, clienteId, otId != default ? [otId] : [], sucursalId, [usuarioId]);
        }
    }

    [Fact]
    public async Task Dos_recepciones_simultaneas_de_la_misma_empresa_obtienen_numeros_distintos()
    {
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var dbSetup = CrearContexto(tenantA);
        var (vehiculo1Id, cliente1Id) = await SembrarVehiculoClienteYVinculoAsync(dbSetup, EmpresaAId);
        var (vehiculo2Id, cliente2Id) = await SembrarVehiculoClienteYVinculoAsync(dbSetup, EmpresaAId);
        var sucursalId = await SembrarSucursalAsync(dbSetup, EmpresaAId);
        var usuario1Id = await SembrarUsuarioAsync(dbSetup);
        var usuario2Id = await SembrarUsuarioAsync(dbSetup);
        await dbSetup.DisposeAsync();

        // Dos conexiones independientes, como dos requests HTTP concurrentes de verdad (mismo motivo
        // que Registros_simultaneos_con_el_mismo_vin_nuevo_solo_uno_tiene_exito en
        // RegistrarVehiculoCommandTests): si no, no habría carrera real que probar.
        var db1 = CrearContexto(tenantA);
        var db2 = CrearContexto(tenantA);
        var otIds = new List<Guid>();
        try
        {
            var handler1 = new RegistrarRecepcionCommandHandler(db1, tenantA);
            var handler2 = new RegistrarRecepcionCommandHandler(db2, tenantA);

            var comando1 = new RegistrarRecepcionCommand(
                sucursalId, vehiculo1Id, cliente1Id, PrioridadOt.Normal, 10_000, null,
                Combustible: 2, Objetos: null, Observaciones: null, DaniosPrevios: [], UsuarioId: usuario1Id);
            var comando2 = new RegistrarRecepcionCommand(
                sucursalId, vehiculo2Id, cliente2Id, PrioridadOt.Urgente, 20_000, null,
                Combustible: 6, Objetos: null, Observaciones: null, DaniosPrevios: [], UsuarioId: usuario2Id);

            var tarea1 = handler1.Handle(comando1, CancellationToken.None);
            var tarea2 = handler2.Handle(comando2, CancellationToken.None);
            var resultados = await Task.WhenAll(tarea1, tarea2);

            otIds.AddRange(resultados.Select(r => r.OtId));

            // La carrera real: ninguna de las dos debe fallar, y los números asignados deben ser
            // distintos (y por lo tanto, el índice único (empresa_id, numero) nunca choca).
            Assert.NotEqual(resultados[0].Numero, resultados[1].Numero);

            await using var verificacion = CrearContexto(tenantA);
            var numerosEnLaBase = await verificacion.OrdenesTrabajo
                .Where(o => otIds.Contains(o.Id)).Select(o => o.Numero).ToListAsync(CancellationToken.None);
            Assert.Equal(2, numerosEnLaBase.Distinct().Count());
        }
        finally
        {
            await db1.DisposeAsync();
            await db2.DisposeAsync();
            await LimpiarAsync(EmpresaAId, cliente1Id, otIds, sucursalId, [usuario1Id, usuario2Id]);
            await LimpiarAsync(EmpresaAId, cliente2Id, []);
        }
    }
}
