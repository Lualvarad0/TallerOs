using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Vehiculos.Commands.RegistrarVehiculo;
using TallerOS.Domain.Catalogos;
using TallerOS.Domain.Clientes;
using TallerOS.Domain.Vehiculos;
using TallerOS.Infrastructure.Persistence;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>RegistrarVehiculoCommandTests.Registros_simultaneos... prueba que la carrera de VIN
/// duplicado lanza DbUpdateException a nivel de Application; eso demuestra la restricción de
/// unicidad, pero no que la Api la convierta en algo entendible. Esta prueba cubre lo que falta,
/// ahora de punta a punta: WebApplicationFactory&lt;Program&gt; levanta la Api real completa (todo
/// Program.cs: autenticación JWT, ManejoDeExcepcionesMiddleware, VehiculosController), con login de
/// verdad por HTTP (ver SesionDePrueba) en vez de un token fabricado a mano o un host mínimo que se
/// salte la autenticación.</summary>
public sealed class ManejoDeExcepcionesMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ManejoDeExcepcionesMiddlewareTests(WebApplicationFactory<Program> factory)
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

    private static AppDbContext CrearContexto(ITenantContext tenant) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString())
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new TenantSessionInterceptor(tenant))
            .Options, tenant);

    // Mismo generador de VIN de prueba que RegistrarVehiculoCommandTests, duplicado a propósito
    // (ver el comentario de esa clase sobre VinDePrueba): cada archivo de prueba se mantiene
    // independiente de los demás, sin acoplarse a los privados de otro.
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

    private sealed record RespuestaId(Guid Id);

    private sealed record ProblemaDeConflicto(int Status);

    [Fact]
    public async Task Carrera_de_vin_duplicado_a_traves_de_la_api_real_responde_409_no_500()
    {
        var client = _factory.CreateClient();
        var superAdmin = new TenantContextFijo(empresaId: null, esSuperAdmin: true);
        var dbSuperAdmin = CrearContexto(superAdmin);
        try
        {
            await dbSuperAdmin.ElevarPrivilegiosSinEmpresaAsync(CancellationToken.None);
            var sesion = await SesionDePrueba.CrearAsync(client, dbSuperAdmin, "Empresa (prueba 409 real)");
            try
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);

                var (marcaId, modeloId) = await SembrarMarcaYModeloAsync(dbSuperAdmin);
                var cliente1 = await CrearClienteViaHttpAsync(client, "1710034065");
                var cliente2 = await CrearClienteViaHttpAsync(client, "1710034073");
                var vinCompartido = VinDePrueba();

                var comando1 = new RegistrarVehiculoCommand(
                    cliente1, vinCompartido, PlacaDePrueba(), marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);
                var comando2 = new RegistrarVehiculoCommand(
                    cliente2, vinCompartido, PlacaDePrueba(), marcaId, modeloId, 2020, null, RolClienteVehiculo.Propietario);

                // Dos peticiones HTTP concurrentes de verdad: cada una resuelve en su propio scope de
                // DI (su propio AppDbContext/conexión, igual que dos requests reales), así que la
                // carrera contra el índice único de Postgres es auténtica, no simulada.
                var tarea1 = client.PostAsJsonAsync("/api/vehiculos", comando1, SesionDePrueba.JsonWeb);
                var tarea2 = client.PostAsJsonAsync("/api/vehiculos", comando2, SesionDePrueba.JsonWeb);
                var respuestas = await Task.WhenAll(tarea1, tarea2);

                // La ganadora es un 200 normal; la perdedora es el punto de esta prueba: debe llegar
                // como 409 con ProblemDetails, nunca como 500 (lo que pasaría si
                // ManejoDeExcepcionesMiddleware no reconociera DbUpdateException+23505).
                Assert.Single(respuestas, r => r.IsSuccessStatusCode);
                var perdedora = Assert.Single(respuestas, r => !r.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.Conflict, perdedora.StatusCode);

                var problema = await perdedora.Content.ReadFromJsonAsync<ProblemaDeConflicto>(SesionDePrueba.JsonWeb);
                Assert.Equal(409, problema?.Status);
            }
            finally
            {
                // Orden exigido por las llaves foráneas: cliente_vehiculo (FK a cliente) antes que
                // cliente (FK a empresa) antes que lo que borra SesionDePrueba.LimpiarAsync.
                // Vehiculo/VehiculoPlaca quedan huérfanos (sin política RLS de DELETE, ver
                // RegistrarVehiculoCommandTests), igual que en el resto de estas pruebas.
                var clienteVehiculos = await dbSuperAdmin.ClienteVehiculos.Where(cv => cv.EmpresaId == sesion.Empresa.Id).ToListAsync();
                dbSuperAdmin.ClienteVehiculos.RemoveRange(clienteVehiculos);
                await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);

                var clientes = await dbSuperAdmin.Clientes.Where(c => c.EmpresaId == sesion.Empresa.Id).ToListAsync();
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
