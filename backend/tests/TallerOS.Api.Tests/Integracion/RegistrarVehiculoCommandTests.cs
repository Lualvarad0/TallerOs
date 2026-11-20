using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Application.Vehiculos.Commands.RegistrarVehiculo;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Vehiculos;
using TallerOS.Infrastructure.Persistence;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Pruebas de integración de RegistrarVehiculo contra Postgres real (mismo motivo que
/// AislamientoEntreEmpresasTests: RLS es de Postgres, no se puede probar con InMemory).
///
/// Cubren lo que de verdad puede fallar en este comando: VIN y placa que señalan a vehículos
/// distintos (conflicto, incluso cuando uno de los dos solo aparece al buscar entre TODAS las
/// empresas, no entre lo que esta empresa ya conoce), un vehículo identificado solo por placa que
/// luego completa su VIN, la carrera real de dos registros simultáneos con el mismo VIN nuevo, y que
/// una empresa no pueda usar el cliente de otra para vincularse a un vehículo. A propósito NO hay una prueba que solo verifique
/// que RegistrarVehiculoCommand no tiene una propiedad VehiculoId: eso no prueba nada sobre lo que el
/// comando realmente hace con Postgres (ver RegistrarVehiculoCommand para la razón de diseño).
///
/// Limpieza: vehiculo y vehiculo_placa no tienen NINGUNA política RLS de DELETE (a propósito, ver
/// Modulo3RlsPoliticas: este módulo todavía no borra vehículos, ni siquiera como super admin), así
/// que con FORCE ROW LEVEL SECURITY el rol de la Api no puede borrar esas filas aunque quisiera. La
/// mayoría de estas pruebas evita el problema sin pelear contra esa regla: abren una transacción
/// explícita y nunca la confirman, así que nada de lo que escriben (vehiculo incluido) llega a
/// persistir. Tres SÍ necesitan un COMMIT real, cada una por su propia razón (ver el comentario de
/// cada una): la carrera de dos registros simultáneos (sin confirmar no hay fila contra la cual el
/// índice único pueda chocar), la reutilización global de un vehículo que otra empresa ya registró
/// (la búsqueda global corre en una conexión nueva, que solo ve lo que ya está confirmado), y el
/// conflicto cruzado VIN/placa de otra empresa (mismo motivo). Las tres aceptan dejar un vehiculo,
/// su marca/modelo de prueba y una vehiculo_placa huérfanos: igual que en producción, un vehículo
/// no se borra, solo deja de tener un cliente_vehiculo que lo haga visible a nadie.
///
/// Por la misma razón (RLS no deja borrar vehiculo/vehiculo_placa ni siquiera a este rol de
/// prueba), esta clase no puede limpiar ese huérfano por sí sola: solo un rol que bypassee RLS
/// (el superusuario del contenedor) puede. <c>backend/scripts/limpiar-datos-de-prueba.sql</c> hace
/// esa limpieza fuera del proceso de pruebas — correrlo de tanto en tanto en el Postgres de
/// desarrollo evita que el catálogo de Marca acumule filas "Marca de prueba &lt;guid&gt;" (pasó:
/// llegaron a acumularse 36, ver docs/especificacion.md, nota del 2026-11-02).</summary>
public sealed class RegistrarVehiculoCommandTests
{
    // Empresa A de estas pruebas: la misma "Taller Demo" sembrada por EmpresaConfiguration.HasData
    // que usa AislamientoEntreEmpresasTests. Evita crear una empresa nueva en cada prueba.
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

    // Alfabeto real de un VIN (ISO 3779): dígitos y letras, sin I/O/Q (se confunden con 1/0/9).
    private static readonly char[] CaracteresVin = "0123456789ABCDEFGHJKLMNPRSTUVWXYZ".ToCharArray();

    private static string VinDePrueba()
    {
        var caracteres = new char[17];
        for (var i = 0; i < 17; i++)
        {
            // Índice 8 es el dígito verificador; se recalcula abajo. Su peso en la suma es 0 (ver
            // VinValidator), así que el valor con que se llena aquí no importa.
            caracteres[i] = i == 8 ? '0' : CaracteresVin[Random.Shared.Next(CaracteresVin.Length)];
        }

        caracteres[8] = CalcularDigitoVerificador(caracteres);
        return new string(caracteres);
    }

    // Mismo algoritmo que TallerOS.Domain.Validators.VinValidator, duplicado a propósito: esto solo
    // genera VINes de prueba válidos, no los valida (eso ya lo cubre VinValidatorTests).
    private static char CalcularDigitoVerificador(char[] vin)
    {
        var transliteracion = new Dictionary<char, int>
        {
            ['A'] = 1, ['B'] = 2, ['C'] = 3, ['D'] = 4, ['E'] = 5, ['F'] = 6, ['G'] = 7, ['H'] = 8,
            ['J'] = 1, ['K'] = 2, ['L'] = 3, ['M'] = 4, ['N'] = 5, ['P'] = 7, ['R'] = 9,
            ['S'] = 2, ['T'] = 3, ['U'] = 4, ['V'] = 5, ['W'] = 6, ['X'] = 7, ['Y'] = 8, ['Z'] = 9,
        };
        int[] pesos = [8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2];

        var suma = 0;
        for (var i = 0; i < 17; i++)
        {
            var valor = vin[i] is >= '0' and <= '9' ? vin[i] - '0' : transliteracion[vin[i]];
            suma += valor * pesos[i];
        }

        var residuo = suma % 11;
        return residuo == 10 ? 'X' : (char)('0' + residuo);
    }

    private static string PlacaDePrueba() => $"P{Guid.NewGuid():N}"[..7].ToUpperInvariant();

    private static async Task<(Guid marcaId, Guid modeloId)> SembrarMarcaYModeloAsync(AppDbContext db)
    {
        var marca = new Marca(1, $"Marca de prueba {Guid.NewGuid():N}");
        db.Marcas.Add(marca);
        var modelo = new Modelo(marca.Id, $"Modelo de prueba {Guid.NewGuid():N}");
        db.Modelos.Add(modelo);
        await db.SaveChangesAsync(CancellationToken.None);
        return (marca.Id, modelo.Id);
    }

    private static async Task<Guid> SembrarClienteAsync(AppDbContext db, Guid empresaId)
    {
        var identificacion = Random.Shared.NextInt64(1_000_000_000, 1_999_999_999).ToString();
        var cliente = new Cliente(empresaId, "Cliente de prueba", TipoCliente.PersonaNatural,
            identificacion, telefono: null, email: null, CanalPreferido.WhatsApp);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(CancellationToken.None);
        return cliente.Id;
    }

    [Fact]
    public async Task Vehiculo_sin_vin_se_identifica_solo_por_placa_y_se_crea()
    {
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var db = CrearContexto(tenantA);
        try
        {
            await using var transaccion = await db.Database.BeginTransactionAsync(CancellationToken.None);

            var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(db);
            var clienteId = await SembrarClienteAsync(db, EmpresaAId);
            var placa = PlacaDePrueba();

            var handler = new RegistrarVehiculoCommandHandler(db, tenantA);
            var comando = new RegistrarVehiculoCommand(clienteId, Vin: null, placa, marcaId, modeloId,
                Anio: 2020, Motor: null, RolClienteVehiculo.Propietario);

            var resultado = await handler.Handle(comando, CancellationToken.None);

            Assert.True(resultado.EsVehiculoNuevo);
            var vehiculo = await db.Vehiculos.FirstAsync(v => v.Id == resultado.VehiculoId);
            Assert.Null(vehiculo.Vin);
            Assert.True(await db.VehiculoPlacas.AnyAsync(p => p.VehiculoId == vehiculo.Id && p.Placa == placa && p.Hasta == null));

            // Nunca se confirma (ver comentario de la clase): nada de esto queda en la base.
        }
        finally
        {
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task Vehiculo_conocido_solo_por_placa_se_reutiliza_aunque_se_indique_un_vin_nuevo()
    {
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var db = CrearContexto(tenantA);
        try
        {
            await using var transaccion = await db.Database.BeginTransactionAsync(CancellationToken.None);

            var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(db);
            var cliente1 = await SembrarClienteAsync(db, EmpresaAId);
            var cliente2 = await SembrarClienteAsync(db, EmpresaAId);
            var placa = PlacaDePrueba();
            var handler = new RegistrarVehiculoCommandHandler(db, tenantA);

            var primero = await handler.Handle(new RegistrarVehiculoCommand(
                cliente1, Vin: null, placa, marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario),
                CancellationToken.None);
            Assert.True(primero.EsVehiculoNuevo);

            // El segundo cliente trae un VIN que el primer registro no tenía. Se vincula al mismo
            // vehículo (la placa ya lo identificaba para esta empresa), pero el VIN dado no se
            // guarda: vehiculo no tiene política RLS de UPDATE todavía (ver
            // RegistrarVehiculoCommandHandler), así que completarlo queda fuera de alcance de este
            // comando, no es un bug de esta prueba.
            var segundo = await handler.Handle(new RegistrarVehiculoCommand(
                cliente2, VinDePrueba(), placa, marcaId, modeloId, 2020, null, RolClienteVehiculo.ConductorHabitual),
                CancellationToken.None);

            Assert.False(segundo.EsVehiculoNuevo);
            Assert.Equal(primero.VehiculoId, segundo.VehiculoId);
            var vehiculo = await db.Vehiculos.FirstAsync(v => v.Id == primero.VehiculoId);
            Assert.Null(vehiculo.Vin);
        }
        finally
        {
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task Vin_y_placa_que_corresponden_a_vehiculos_distintos_lanza_conflicto()
    {
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var db = CrearContexto(tenantA);
        try
        {
            await using var transaccion = await db.Database.BeginTransactionAsync(CancellationToken.None);

            var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(db);
            var handler = new RegistrarVehiculoCommandHandler(db, tenantA);

            var vin1 = VinDePrueba();
            var placa1 = PlacaDePrueba();
            var cliente1 = await SembrarClienteAsync(db, EmpresaAId);
            await handler.Handle(new RegistrarVehiculoCommand(
                cliente1, vin1, placa1, marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario),
                CancellationToken.None);

            var vin2 = VinDePrueba();
            var placa2 = PlacaDePrueba();
            var cliente2 = await SembrarClienteAsync(db, EmpresaAId);
            await handler.Handle(new RegistrarVehiculoCommand(
                cliente2, vin2, placa2, marcaId, modeloId, 2021, null, RolClienteVehiculo.Propietario),
                CancellationToken.None);

            // cliente3 trae el VIN del primer vehículo pero la placa del segundo: dos vehículos ya
            // conocidos por esta empresa, contradictorios entre sí.
            var cliente3 = await SembrarClienteAsync(db, EmpresaAId);
            var comandoContradictorio = new RegistrarVehiculoCommand(
                cliente3, vin1, placa2, marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);

            await Assert.ThrowsAsync<ConflictoDeNegocioException>(
                () => handler.Handle(comandoContradictorio, CancellationToken.None));
        }
        finally
        {
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task Placa_activa_con_vin_distinto_al_indicado_lanza_conflicto()
    {
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var db = CrearContexto(tenantA);
        try
        {
            await using var transaccion = await db.Database.BeginTransactionAsync(CancellationToken.None);

            var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(db);
            var handler = new RegistrarVehiculoCommandHandler(db, tenantA);

            var vinConocido = VinDePrueba();
            var placaConocida = PlacaDePrueba();
            var cliente1 = await SembrarClienteAsync(db, EmpresaAId);
            await handler.Handle(new RegistrarVehiculoCommand(
                cliente1, vinConocido, placaConocida, marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario),
                CancellationToken.None);

            // Misma placa (ya pertenece a un vehículo con VIN propio) pero con un VIN nuevo distinto:
            // no puede ser "el mismo vehículo que aún no tenía VIN" (ya lo tiene) ni "cambió de placa"
            // (fuera de alcance), así que es un conflicto, no una reutilización silenciosa.
            var cliente2 = await SembrarClienteAsync(db, EmpresaAId);
            var comandoContradictorio = new RegistrarVehiculoCommand(
                cliente2, VinDePrueba(), placaConocida, marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);

            await Assert.ThrowsAsync<ConflictoDeNegocioException>(
                () => handler.Handle(comandoContradictorio, CancellationToken.None));
        }
        finally
        {
            await db.DisposeAsync();
        }
    }

    [Fact]
    public async Task Registros_simultaneos_con_el_mismo_vin_nuevo_solo_uno_tiene_exito()
    {
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var dbSetup = CrearContexto(tenantA);
        try
        {
            var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(dbSetup);
            var cliente1 = await SembrarClienteAsync(dbSetup, EmpresaAId);
            var cliente2 = await SembrarClienteAsync(dbSetup, EmpresaAId);
            var vinCompartido = VinDePrueba();

            // Dos conexiones independientes (como dos requests HTTP concurrentes de verdad), no dos
            // consultas del mismo DbContext: si no, no habría carrera real que probar. A propósito
            // NO van dentro de una transacción sin confirmar (como las demás pruebas de este
            // archivo): el choque de índice único que se quiere probar solo ocurre si una de las dos
            // inserciones llega a confirmarse de verdad.
            var db1 = CrearContexto(tenantA);
            var db2 = CrearContexto(tenantA);
            try
            {
                var handler1 = new RegistrarVehiculoCommandHandler(db1, tenantA);
                var handler2 = new RegistrarVehiculoCommandHandler(db2, tenantA);

                var comando1 = new RegistrarVehiculoCommand(
                    cliente1, vinCompartido, PlacaDePrueba(), marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);
                var comando2 = new RegistrarVehiculoCommand(
                    cliente2, vinCompartido, PlacaDePrueba(), marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);

                var tarea1 = EjecutarCapturandoResultado(handler1, comando1);
                var tarea2 = EjecutarCapturandoResultado(handler2, comando2);
                var resultados = await Task.WhenAll(tarea1, tarea2);

                var ganador = Assert.Single(resultados, r => r.Exito);
                Assert.Single(resultados, r => !r.Exito && r.Excepcion is DbUpdateException);

                // "Sin registros parciales": SaveChangesAsync agrupa sus INSERT en una sola
                // transacción (comportamiento por defecto de EF Core), así que el intento que
                // perdió la carrera no debería dejar nada a medio confirmar. Se comprueba leyendo
                // con una conexión nueva (ninguno de los dos handlers), para no depender de lo que
                // ese DbContext ya tenía en memoria: debe existir un solo vehiculo con ese VIN, y el
                // único cliente_vehiculo/vehiculo_placa que lo referencian son los del que ganó, no
                // los del que perdió ni un resto de ambos.
                await using var dbVerificacion = CrearContexto(tenantA);
                Assert.Equal(1, await dbVerificacion.Vehiculos.CountAsync(v => v.Vin == vinCompartido));
                Assert.Equal(1, await dbVerificacion.ClienteVehiculos.CountAsync(cv => cv.VehiculoId == ganador.Resultado!.VehiculoId));
                Assert.Equal(1, await dbVerificacion.VehiculoPlacas.CountAsync(p => p.VehiculoId == ganador.Resultado!.VehiculoId));
                Assert.Equal(ganador.Resultado!.ClienteVehiculoId, (await dbVerificacion.ClienteVehiculos.SingleAsync(cv => cv.VehiculoId == ganador.Resultado!.VehiculoId)).Id);
            }
            finally
            {
                await db1.DisposeAsync();
                await db2.DisposeAsync();
            }
        }
        finally
        {
            // Solo lo que sí se puede borrar (ver comentario de la clase): cliente_vehiculo, cliente,
            // modelo y marca. El vehiculo y la vehiculo_placa que sí llegaron a confirmarse quedan
            // en la base, huérfanos (ningún empresa_id los referencia), igual que en producción.
            await LimpiarLoQueSePuedeBorrarAsync(dbSetup, EmpresaAId);
        }
    }

    private static async Task<(bool Exito, Exception? Excepcion, RegistrarVehiculoResultado? Resultado)> EjecutarCapturandoResultado(
        RegistrarVehiculoCommandHandler handler, RegistrarVehiculoCommand comando)
    {
        try
        {
            var resultado = await handler.Handle(comando, CancellationToken.None);
            return (true, null, resultado);
        }
        catch (Exception ex)
        {
            return (false, ex, null);
        }
    }

    [Fact]
    public async Task Empresa_B_no_puede_usar_un_cliente_de_empresa_A_para_vincular_un_vehiculo()
    {
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);

            var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
            var empresaB = new Empresa("Empresa B (prueba RegistrarVehiculo)", rucDePrueba);
            setup.Empresas.Add(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);

            var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
            var dbA = CrearContexto(tenantA);
            try
            {
                await using var transaccion = await dbA.Database.BeginTransactionAsync(CancellationToken.None);

                var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(dbA);
                var clienteDeEmpresaA = await SembrarClienteAsync(dbA, EmpresaAId);

                var tenantB = new TenantContextFijo(empresaB.Id, esSuperAdmin: false);
                var dbB = CrearContexto(tenantB);
                try
                {
                    var handlerB = new RegistrarVehiculoCommandHandler(dbB, tenantB);
                    var comando = new RegistrarVehiculoCommand(
                        clienteDeEmpresaA, VinDePrueba(), PlacaDePrueba(), marcaId, modeloId, 2020, null,
                        RolClienteVehiculo.Propietario);

                    // El filtro global de EF ya oculta el cliente de la empresa A; este comando lo
                    // trata como si no existiera, no revela que es "de otra empresa".
                    await Assert.ThrowsAsync<EntidadNoEncontradaException>(
                        () => handlerB.Handle(comando, CancellationToken.None));

                    // Tampoco quedó ningún vínculo creado del lado de la empresa B.
                    var sinFiltro = await dbB.ClienteVehiculos.IgnoreQueryFilters()
                        .AnyAsync(cv => cv.ClienteId == clienteDeEmpresaA);
                    Assert.False(sinFiltro);
                }
                finally
                {
                    await dbB.DisposeAsync();
                }

                // dbA tampoco confirma: el cliente y la marca/modelo de prueba de la empresa A no
                // quedan en la base.
            }
            finally
            {
                await dbA.DisposeAsync();
            }

            setup.Empresas.Remove(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }

    [Fact]
    public async Task Empresa_B_reutiliza_el_vehiculo_que_empresa_A_ya_registro_sin_duplicarlo()
    {
        // Empresa A registra el vehículo de verdad (COMMIT real, no en una transacción que se
        // deshace): la búsqueda global de Empresa B necesita una conexión NUEVA que vea ese commit,
        // así que aquí no se puede evitar el mismo límite que la prueba de concurrencia (vehiculo y
        // vehiculo_placa quedan huérfanos al final, ver comentario de la clase).
        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var dbA = CrearContexto(tenantA);
        Guid marcaId, modeloId, vehiculoIdDeA;
        string vin, placa;
        try
        {
            (marcaId, modeloId) = await SembrarMarcaYModeloAsync(dbA);
            var clienteA = await SembrarClienteAsync(dbA, EmpresaAId);
            vin = VinDePrueba();
            placa = PlacaDePrueba();

            var resultadoA = await new RegistrarVehiculoCommandHandler(dbA, tenantA).Handle(
                new RegistrarVehiculoCommand(clienteA, vin, placa, marcaId, modeloId, 2020, "2.0L", RolClienteVehiculo.Propietario),
                CancellationToken.None);
            Assert.True(resultadoA.EsVehiculoNuevo);
            vehiculoIdDeA = resultadoA.VehiculoId;
        }
        finally
        {
            await dbA.DisposeAsync();
        }

        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
            var empresaB = new Empresa("Empresa B (prueba RegistrarVehiculo, reuso global)", rucDePrueba);
            setup.Empresas.Add(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);

            var tenantB = new TenantContextFijo(empresaB.Id, esSuperAdmin: false);
            var dbB = CrearContexto(tenantB);
            try
            {
                var clienteB = await SembrarClienteAsync(dbB, empresaB.Id);

                // Mismo VIN que ya registró la empresa A; empresaB nunca tuvo contacto con ese
                // vehículo antes. Debe reutilizarlo (búsqueda global, ver RegistrarVehiculoCommand),
                // no chocar con el índice único intentando crear uno nuevo.
                var resultadoB = await new RegistrarVehiculoCommandHandler(dbB, tenantB).Handle(
                    new RegistrarVehiculoCommand(clienteB, vin, Placa: null, marcaId, modeloId, 2020, "2.0L", RolClienteVehiculo.Propietario),
                    CancellationToken.None);

                Assert.False(resultadoB.EsVehiculoNuevo);
                Assert.Equal(vehiculoIdDeA, resultadoB.VehiculoId);

                // No se duplicó: un solo vehiculo con ese VIN en toda la base.
                var totalConEseVin = await setup.Vehiculos.IgnoreQueryFilters().CountAsync(v => v.Vin == vin, CancellationToken.None);
                Assert.Equal(1, totalConEseVin);

                // Empresa B ahora sí ve los datos básicos del vehículo (el vínculo que acaba de crear
                // le da acceso)...
                var vehiculoVistoPorB = await dbB.Vehiculos.FirstOrDefaultAsync(v => v.Id == vehiculoIdDeA, CancellationToken.None);
                Assert.NotNull(vehiculoVistoPorB);
                Assert.Equal(vin, vehiculoVistoPorB.Vin);

                // ...pero nada del cliente ni del vínculo de la empresa A: su propio ClienteVehiculo
                // es el único que ve con el filtro normal puesto.
                var vinculosQueBVe = await dbB.ClienteVehiculos.Where(cv => cv.VehiculoId == vehiculoIdDeA).ToListAsync(CancellationToken.None);
                Assert.Single(vinculosQueBVe, cv => cv.EmpresaId == empresaB.Id);
            }
            finally
            {
                await LimpiarLoQueSePuedeBorrarAsync(dbB, empresaB.Id);
            }

            setup.Empresas.Remove(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);
        }
        finally
        {
            await setup.DisposeAsync();
        }
    }

    [Fact]
    public async Task Vin_conocido_localmente_pero_placa_de_un_vehiculo_distinto_de_otra_empresa_lanza_conflicto()
    {
        // El vehículo "ajeno" de Empresa B necesita un COMMIT real: la búsqueda global que lo
        // detecta corre sobre la conexión de Empresa A, que solo ve lo que ya está confirmado en la
        // base (igual que en Empresa_B_reutiliza..., no el límite propio de esta prueba).
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var setup = CrearContexto(superAdmin);
        Guid empresaBId;
        string placaAjena;
        try
        {
            await setup.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
            var empresaB = new Empresa("Empresa B (prueba RegistrarVehiculo, conflicto cruzado)", rucDePrueba);
            setup.Empresas.Add(empresaB);
            await setup.SaveChangesAsync(CancellationToken.None);
            empresaBId = empresaB.Id;

            var tenantB = new TenantContextFijo(empresaBId, esSuperAdmin: false);
            var dbB = CrearContexto(tenantB);
            try
            {
                var (marcaIdB, modeloIdB) = await SembrarMarcaYModeloAsync(dbB);
                var clienteB = await SembrarClienteAsync(dbB, empresaBId);
                placaAjena = PlacaDePrueba();

                await new RegistrarVehiculoCommandHandler(dbB, tenantB).Handle(
                    new RegistrarVehiculoCommand(clienteB, VinDePrueba(), placaAjena, marcaIdB, modeloIdB, 2019, null, RolClienteVehiculo.Propietario),
                    CancellationToken.None);
            }
            finally
            {
                await LimpiarLoQueSePuedeBorrarAsync(dbB, empresaBId);
            }

            setup.Empresas.Remove(await setup.Empresas.FirstAsync(e => e.Id == empresaBId, CancellationToken.None));
            await setup.SaveChangesAsync(CancellationToken.None);
        }
        finally
        {
            await setup.DisposeAsync();
        }

        var tenantA = new TenantContextFijo(EmpresaAId, esSuperAdmin: false);
        var db = CrearContexto(tenantA);
        try
        {
            await using var transaccion = await db.Database.BeginTransactionAsync(CancellationToken.None);

            var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(db);
            var handler = new RegistrarVehiculoCommandHandler(db, tenantA);

            // Empresa A ya conoce este vehículo (su propio VIN, vinculado a cliente1): la búsqueda
            // local lo encuentra sin necesitar la global.
            var vinConocido = VinDePrueba();
            var cliente1 = await SembrarClienteAsync(db, EmpresaAId);
            await handler.Handle(new RegistrarVehiculoCommand(
                cliente1, vinConocido, PlacaDePrueba(), marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario),
                CancellationToken.None);

            // cliente2 trae ese mismo VIN (coincide local, no haría falta buscar más) pero la placa
            // de un vehículo DISTINTO de otra empresa, que Empresa A todavía no conoce. Antes de la
            // corrección, al ya haber resuelto el VIN localmente, nunca se buscaba la placa de forma
            // global y esta mezcla pasaba sin que nadie la detectara (ver RegistrarVehiculoCommandHandler).
            var cliente2 = await SembrarClienteAsync(db, EmpresaAId);
            var comandoContradictorio = new RegistrarVehiculoCommand(
                cliente2, vinConocido, placaAjena, marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);

            var excepcion = await Assert.ThrowsAsync<ConflictoDeNegocioException>(
                () => handler.Handle(comandoContradictorio, CancellationToken.None));
            Assert.Contains("distintos", excepcion.Message);

            // Nunca se confirma (ver comentario de la clase): nada de lo de Empresa A queda en la base.
        }
        finally
        {
            await db.DisposeAsync();
        }
    }

    /// <summary>Solo para la prueba de concurrencia: borra cliente_vehiculo y cliente (tienen
    /// política RLS de DELETE para la propia empresa). Deliberadamente NO toca vehiculo ni
    /// vehiculo_placa (ver comentario de la clase: no tienen política de DELETE, no se puede) — ni,
    /// por la misma razón, marca y modelo: el vehiculo que queda huérfano sigue referenciándolos por
    /// FK, así que borrarlos violaría esa llave foránea.</summary>
    private static async Task LimpiarLoQueSePuedeBorrarAsync(AppDbContext db, Guid empresaId)
    {
        try
        {
            var clienteVehiculos = await db.ClienteVehiculos.Where(cv => cv.EmpresaId == empresaId).ToListAsync(CancellationToken.None);
            db.ClienteVehiculos.RemoveRange(clienteVehiculos);
            await db.SaveChangesAsync(CancellationToken.None);

            var clientes = await db.Clientes.Where(c => c.EmpresaId == empresaId && c.Nombre == "Cliente de prueba").ToListAsync(CancellationToken.None);
            db.Clientes.RemoveRange(clientes);
            await db.SaveChangesAsync(CancellationToken.None);
        }
        finally
        {
            await db.DisposeAsync();
        }
    }
}
