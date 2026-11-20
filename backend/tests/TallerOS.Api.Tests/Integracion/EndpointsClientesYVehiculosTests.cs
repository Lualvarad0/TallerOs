using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Vehiculos;
using TallerOS.Application.Vehiculos.Commands.RegistrarVehiculo;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Validators;
using TallerOS.Domain.Vehiculos;
using TallerOS.Infrastructure.Persistence;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Pruebas HTTP de punta a punta para ClientesController y VehiculosController sobre la
/// Api real (WebApplicationFactory&lt;Program&gt;, ver ManejoDeExcepcionesMiddlewareTests): sin
/// token no se pasa de la autenticación, y el aislamiento entre empresas se prueba contra los
/// endpoints de verdad (JwtBearer + policy "Sesion" + filtro global de EF + RLS juntos), no solo
/// contra el handler en memoria como en RegistrarVehiculoCommandTests.
///
/// Cada petición HTTP es su propio COMMIT real (no hay una transacción de prueba que envuelva al
/// handler, como sí pueden hacer las de RegistrarVehiculoCommandTests): las tres pruebas que llaman
/// a SembrarMarcaYModeloAsync dejan, a propósito, un vehiculo + su marca/modelo de prueba huérfanos
/// (mismo motivo de RLS, ver el comentario de esa clase). Limpieza periódica:
/// backend/scripts/limpiar-datos-de-prueba.sql.</summary>
public sealed class EndpointsClientesYVehiculosTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EndpointsClientesYVehiculosTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private sealed class TenantContextFijo(Guid? empresaId, bool esSuperAdmin) : ITenantContext
    {
        public Guid? EmpresaId { get; } = empresaId;
        public bool EsSuperAdmin { get; } = esSuperAdmin;
    }

    private static string ConnectionString() =>
        Environment.GetEnvironmentVariable("ConnectionStrings__App")
            ?? throw new InvalidOperationException(
                "Esta prueba necesita Postgres local con las migraciones aplicadas y la variable " +
                "de entorno ConnectionStrings__App (la misma cadena que usa la Api; ver .env).");

    private static AppDbContext CrearContextoSuperAdmin() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString())
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new TenantSessionInterceptor(new TenantContextFijo(empresaId: null, esSuperAdmin: true)))
            .Options, new TenantContextFijo(empresaId: null, esSuperAdmin: true));

    // Mismo generador de VIN de prueba que RegistrarVehiculoCommandTests, duplicado a propósito
    // (ver el comentario de esa clase): cada archivo de prueba se mantiene independiente.
    private static readonly char[] CaracteresVin = "0123456789ABCDEFGHJKLMNPRSTUVWXYZ".ToCharArray();

    private static string VinDePrueba()
    {
        var caracteres = new char[17];
        for (var i = 0; i < 17; i++)
        {
            caracteres[i] = i == 8 ? '0' : CaracteresVin[Random.Shared.Next(CaracteresVin.Length)];
        }

        caracteres[8] = CalcularDigitoVerificador(caracteres);
        return new string(caracteres);
    }

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

    private sealed record RespuestaId(Guid Id);

    private static async Task<Guid> CrearClienteViaHttpAsync(HttpClient client, string identificacion)
    {
        var comando = new
        {
            Nombre = "Cliente de prueba",
            Tipo = TipoCliente.PersonaNatural,
            Identificacion = identificacion,
            Telefono = (string?)null,
            Email = (string?)null,
            CanalPreferido = CanalPreferido.WhatsApp,
        };
        var respuesta = await client.PostAsJsonAsync("/api/clientes", comando, SesionDePrueba.JsonWeb);
        respuesta.EnsureSuccessStatusCode();
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaId>(SesionDePrueba.JsonWeb);
        return cuerpo!.Id;
    }

    private HttpClient ClienteAutenticado(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Peticion_sin_token_a_clientes_responde_401()
    {
        var client = _factory.CreateClient();

        var respuesta = await client.GetAsync("/api/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Empresa_B_no_puede_ver_ni_editar_el_cliente_de_empresa_A_por_http()
    {
        var clienteAnonimo = _factory.CreateClient();
        var dbSuperAdmin = CrearContextoSuperAdmin();
        try
        {
            await dbSuperAdmin.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var sesionA = await SesionDePrueba.CrearAsync(clienteAnonimo, dbSuperAdmin, "Empresa A (aislamiento cliente http)");
            var sesionB = await SesionDePrueba.CrearAsync(clienteAnonimo, dbSuperAdmin, "Empresa B (aislamiento cliente http)");
            try
            {
                var clientA = ClienteAutenticado(sesionA.Token);
                var clientB = ClienteAutenticado(sesionB.Token);

                var clienteAId = await CrearClienteViaHttpAsync(clientA, "1710034065");

                // El filtro global de EF (Cliente) ya acota esto a la empresa de la sesión: para B,
                // este cliente no existe, igual que si de verdad no existiera (no revela que es de
                // otra empresa, ver EntidadNoEncontradaException/ManejoDeExcepcionesMiddleware).
                var respuestaGetB = await clientB.GetAsync($"/api/clientes/{clienteAId}");
                Assert.Equal(HttpStatusCode.NotFound, respuestaGetB.StatusCode);

                var respuestaPutB = await clientB.PutAsJsonAsync(
                    $"/api/clientes/{clienteAId}",
                    new { Nombre = "Nombre que B no debería poder poner", Telefono = (string?)null, Email = (string?)null, CanalPreferido = CanalPreferido.Correo },
                    SesionDePrueba.JsonWeb);
                Assert.Equal(HttpStatusCode.NotFound, respuestaPutB.StatusCode);

                var respuestaDeleteB = await clientB.DeleteAsync($"/api/clientes/{clienteAId}");
                Assert.Equal(HttpStatusCode.NotFound, respuestaDeleteB.StatusCode);

                // Control: A sí puede ver su propio cliente (si esto fallara, el 404 de arriba no
                // probaría aislamiento, probaría que el endpoint está roto para todos).
                var respuestaGetA = await clientA.GetAsync($"/api/clientes/{clienteAId}");
                Assert.Equal(HttpStatusCode.OK, respuestaGetA.StatusCode);
            }
            finally
            {
                foreach (var sesion in new[] { sesionA, sesionB })
                {
                    var clientes = await dbSuperAdmin.Clientes.Where(c => c.EmpresaId == sesion.Empresa.Id).ToListAsync(CancellationToken.None);
                    dbSuperAdmin.Clientes.RemoveRange(clientes);
                }

                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);
                await SesionDePrueba.LimpiarAsync(dbSuperAdmin, sesionA);
                await SesionDePrueba.LimpiarAsync(dbSuperAdmin, sesionB);
            }
        }
        finally
        {
            await dbSuperAdmin.DisposeAsync();
        }
    }

    [Fact]
    public async Task Empresa_B_no_puede_ver_el_vehiculo_de_empresa_A_por_http()
    {
        var clienteAnonimo = _factory.CreateClient();
        var dbSuperAdmin = CrearContextoSuperAdmin();
        try
        {
            await dbSuperAdmin.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var sesionA = await SesionDePrueba.CrearAsync(clienteAnonimo, dbSuperAdmin, "Empresa A (aislamiento vehiculo http)");
            var sesionB = await SesionDePrueba.CrearAsync(clienteAnonimo, dbSuperAdmin, "Empresa B (aislamiento vehiculo http)");
            try
            {
                var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(dbSuperAdmin);
                var clientA = ClienteAutenticado(sesionA.Token);
                var clientB = ClienteAutenticado(sesionB.Token);

                var clienteA = await CrearClienteViaHttpAsync(clientA, "1710034065");
                var comando = new RegistrarVehiculoCommand(
                    clienteA, VinDePrueba(), PlacaDePrueba(), marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);
                var respuestaRegistro = await clientA.PostAsJsonAsync("/api/vehiculos", comando, SesionDePrueba.JsonWeb);
                respuestaRegistro.EnsureSuccessStatusCode();
                var resultado = await respuestaRegistro.Content.ReadFromJsonAsync<RegistrarVehiculoResultado>(SesionDePrueba.JsonWeb);

                // Empresa B nunca vinculó este vehículo: el filtro global de Vehiculo exige un
                // ClienteVehiculo propio para verlo (ver AppDbContext), así que esto da 404, no un
                // vehículo con datos de otra empresa.
                var respuestaB = await clientB.GetAsync($"/api/vehiculos/{resultado!.VehiculoId}");
                Assert.Equal(HttpStatusCode.NotFound, respuestaB.StatusCode);

                var respuestaA = await clientA.GetAsync($"/api/vehiculos/{resultado.VehiculoId}");
                Assert.Equal(HttpStatusCode.OK, respuestaA.StatusCode);
            }
            finally
            {
                foreach (var sesion in new[] { sesionA, sesionB })
                {
                    var clienteVehiculos = await dbSuperAdmin.ClienteVehiculos.Where(cv => cv.EmpresaId == sesion.Empresa.Id).ToListAsync(CancellationToken.None);
                    dbSuperAdmin.ClienteVehiculos.RemoveRange(clienteVehiculos);
                }

                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);

                foreach (var sesion in new[] { sesionA, sesionB })
                {
                    var clientes = await dbSuperAdmin.Clientes.Where(c => c.EmpresaId == sesion.Empresa.Id).ToListAsync(CancellationToken.None);
                    dbSuperAdmin.Clientes.RemoveRange(clientes);
                }

                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);
                await SesionDePrueba.LimpiarAsync(dbSuperAdmin, sesionA);
                await SesionDePrueba.LimpiarAsync(dbSuperAdmin, sesionB);
            }
        }
        finally
        {
            await dbSuperAdmin.DisposeAsync();
        }
    }

    [Fact]
    public async Task Clientes_vinculados_de_un_vehiculo_compartido_solo_muestran_el_de_la_empresa_que_consulta()
    {
        // Empresa A registra el vehículo (nuevo); Empresa B lo reutiliza (misma búsqueda global que
        // RegistrarVehiculoCommandTests.Empresa_B_reutiliza..., pero aquí de punta a punta por HTTP,
        // con cada registro como su propia petición ya confirmada, sin el límite de transacciones
        // sin confirmar que tiene esa prueba). Cada empresa, al pedir la ficha del vehículo, debe
        // ver únicamente SU vínculo en "Clientes" — nunca el de la otra, aunque comparten el mismo
        // vehículo físico.
        var clienteAnonimo = _factory.CreateClient();
        var dbSuperAdmin = CrearContextoSuperAdmin();
        try
        {
            await dbSuperAdmin.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var sesionA = await SesionDePrueba.CrearAsync(clienteAnonimo, dbSuperAdmin, "Empresa A (vehiculo compartido http)");
            var sesionB = await SesionDePrueba.CrearAsync(clienteAnonimo, dbSuperAdmin, "Empresa B (vehiculo compartido http)");
            try
            {
                var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(dbSuperAdmin);
                var clientA = ClienteAutenticado(sesionA.Token);
                var clientB = ClienteAutenticado(sesionB.Token);

                var clienteA = await CrearClienteViaHttpAsync(clientA, "1710034065");
                var clienteB = await CrearClienteViaHttpAsync(clientB, "1710034065");
                var vin = VinDePrueba();

                var comandoA = new RegistrarVehiculoCommand(clienteA, vin, PlacaDePrueba(), marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);
                var respuestaA = await clientA.PostAsJsonAsync("/api/vehiculos", comandoA, SesionDePrueba.JsonWeb);
                respuestaA.EnsureSuccessStatusCode();
                var resultadoA = await respuestaA.Content.ReadFromJsonAsync<RegistrarVehiculoResultado>(SesionDePrueba.JsonWeb);

                var comandoB = new RegistrarVehiculoCommand(clienteB, vin, Placa: null, marcaId, modeloId, 2020, null, RolClienteVehiculo.ConductorHabitual);
                var respuestaB = await clientB.PostAsJsonAsync("/api/vehiculos", comandoB, SesionDePrueba.JsonWeb);
                respuestaB.EnsureSuccessStatusCode();
                var resultadoB = await respuestaB.Content.ReadFromJsonAsync<RegistrarVehiculoResultado>(SesionDePrueba.JsonWeb);

                Assert.False(resultadoB!.EsVehiculoNuevo);
                Assert.Equal(resultadoA!.VehiculoId, resultadoB.VehiculoId);

                var detalleParaA = await (await clientA.GetAsync($"/api/vehiculos/{resultadoA.VehiculoId}"))
                    .Content.ReadFromJsonAsync<VehiculoDetalleDto>(SesionDePrueba.JsonWeb);
                Assert.Single(detalleParaA!.Clientes, c => c.ClienteId == clienteA);

                var detalleParaB = await (await clientB.GetAsync($"/api/vehiculos/{resultadoA.VehiculoId}"))
                    .Content.ReadFromJsonAsync<VehiculoDetalleDto>(SesionDePrueba.JsonWeb);
                Assert.Single(detalleParaB!.Clientes, c => c.ClienteId == clienteB);
            }
            finally
            {
                foreach (var sesion in new[] { sesionA, sesionB })
                {
                    var clienteVehiculos = await dbSuperAdmin.ClienteVehiculos.Where(cv => cv.EmpresaId == sesion.Empresa.Id).ToListAsync(CancellationToken.None);
                    dbSuperAdmin.ClienteVehiculos.RemoveRange(clienteVehiculos);
                }

                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);

                foreach (var sesion in new[] { sesionA, sesionB })
                {
                    var clientes = await dbSuperAdmin.Clientes.Where(c => c.EmpresaId == sesion.Empresa.Id).ToListAsync(CancellationToken.None);
                    dbSuperAdmin.Clientes.RemoveRange(clientes);
                }

                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);
                await SesionDePrueba.LimpiarAsync(dbSuperAdmin, sesionA);
                await SesionDePrueba.LimpiarAsync(dbSuperAdmin, sesionB);
            }
        }
        finally
        {
            await dbSuperAdmin.DisposeAsync();
        }
    }

    // Perturba un carácter de peso no nulo (posición 0, peso 8) de un VIN válido hasta que el
    // dígito verificador deje de cerrar. El bucle existe porque esa posición alterna entre dígitos
    // y letras que a veces transliteran al mismo valor (p. ej. '1' y 'A' valen 1): si el nuevo
    // carácter cae en esa coincidencia el cambio no alcanzaría a invalidar el VIN.
    private static string VinConDigitoVerificadorInvalido()
    {
        string vin;
        do
        {
            var caracteres = VinDePrueba().ToCharArray();
            caracteres[0] = caracteres[0] == '0' ? '1' : '0';
            vin = new string(caracteres);
        } while (VinValidator.EsValido(vin));

        return vin;
    }

    [Fact]
    public async Task Vehiculo_con_vin_de_digito_verificador_invalido_se_registra_con_advertencia_por_http()
    {
        // Cambio de contrato del 2026-11-02 (ver docs/especificacion.md y
        // RegistrarVehiculoCommandValidator): un VIN con dígito verificador incorrecto ya no
        // bloquea el registro (antes esto respondía 400). Ahora se guarda tal cual se envió y
        // RegistrarVehiculoResultado.AdvertenciaVin avisa de eso al llamante.
        var clienteAnonimo = _factory.CreateClient();
        var dbSuperAdmin = CrearContextoSuperAdmin();
        try
        {
            await dbSuperAdmin.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var sesion = await SesionDePrueba.CrearAsync(clienteAnonimo, dbSuperAdmin, "Empresa (VIN inválido http)");
            try
            {
                var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(dbSuperAdmin);
                var client = ClienteAutenticado(sesion.Token);
                var clienteId = await CrearClienteViaHttpAsync(client, "1710034065");
                var vinInvalido = VinConDigitoVerificadorInvalido();

                var comando = new RegistrarVehiculoCommand(
                    clienteId, vinInvalido, Placa: null, marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);
                var respuesta = await client.PostAsJsonAsync("/api/vehiculos", comando, SesionDePrueba.JsonWeb);

                respuesta.EnsureSuccessStatusCode();
                var resultado = await respuesta.Content.ReadFromJsonAsync<RegistrarVehiculoResultado>(SesionDePrueba.JsonWeb);
                Assert.NotNull(resultado!.AdvertenciaVin);

                var vehiculo = await dbSuperAdmin.Vehiculos.FirstAsync(v => v.Id == resultado.VehiculoId, CancellationToken.None);
                Assert.Equal(vinInvalido, vehiculo.Vin);
            }
            finally
            {
                var clienteVehiculos = await dbSuperAdmin.ClienteVehiculos.Where(cv => cv.EmpresaId == sesion.Empresa.Id).ToListAsync(CancellationToken.None);
                dbSuperAdmin.ClienteVehiculos.RemoveRange(clienteVehiculos);
                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);

                var clientes = await dbSuperAdmin.Clientes.Where(c => c.EmpresaId == sesion.Empresa.Id).ToListAsync(CancellationToken.None);
                dbSuperAdmin.Clientes.RemoveRange(clientes);
                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);

                await SesionDePrueba.LimpiarAsync(dbSuperAdmin, sesion);
            }
        }
        finally
        {
            await dbSuperAdmin.DisposeAsync();
        }
    }

    [Fact]
    public async Task Crear_cliente_con_identificacion_duplicada_por_http_responde_409()
    {
        var clienteAnonimo = _factory.CreateClient();
        var dbSuperAdmin = CrearContextoSuperAdmin();
        try
        {
            await dbSuperAdmin.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var sesion = await SesionDePrueba.CrearAsync(clienteAnonimo, dbSuperAdmin, "Empresa (409 cliente http)");
            try
            {
                var client = ClienteAutenticado(sesion.Token);
                const string identificacion = "1710034065";
                await CrearClienteViaHttpAsync(client, identificacion);

                var comando = new
                {
                    Nombre = "Otro cliente, misma identificación",
                    Tipo = TipoCliente.PersonaNatural,
                    Identificacion = identificacion,
                    Telefono = (string?)null,
                    Email = (string?)null,
                    CanalPreferido = CanalPreferido.WhatsApp,
                };
                var segunda = await client.PostAsJsonAsync("/api/clientes", comando, SesionDePrueba.JsonWeb);

                Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
            }
            finally
            {
                var clientes = await dbSuperAdmin.Clientes.Where(c => c.EmpresaId == sesion.Empresa.Id).ToListAsync(CancellationToken.None);
                dbSuperAdmin.Clientes.RemoveRange(clientes);
                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);
                await SesionDePrueba.LimpiarAsync(dbSuperAdmin, sesion);
            }
        }
        finally
        {
            await dbSuperAdmin.DisposeAsync();
        }
    }
}
