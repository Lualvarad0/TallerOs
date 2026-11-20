using System.Net.Http.Json;
using System.Text.Json;
using TallerOS.Application.Seguridad.Autenticacion.Commands.IniciarSesion;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Seguridad;
using TallerOS.Infrastructure.Persistence;
using TallerOS.Infrastructure.Seguridad;

namespace TallerOS.Api.Tests.Integracion;

/// <summary>Crea una empresa + usuario + rol de prueba y hace login de verdad por HTTP (POST
/// /api/auth/iniciar-sesion), en vez de fabricar un JWT a mano: así las pruebas que usan esto
/// ejercitan el mismo camino de autenticación que un cliente real (AddJwtBearer + la policy
/// "Sesion"), no solo el contrato de los controllers. El usuario queda con una sola empresa a
/// propósito (ver IniciarSesionCommandHandler): con una sola, el login devuelve el token de sesión
/// directo, sin el paso intermedio de seleccionar-empresa.</summary>
internal static class SesionDePrueba
{
    public const string Password = "clave-de-prueba-123!";

    public static readonly JsonSerializerOptions JsonWeb = new(JsonSerializerDefaults.Web);

    public sealed record Sesion(Empresa Empresa, Usuario Usuario, Rol Rol, UsuarioRol UsuarioRol, string Token);

    /// <summary>dbSuperAdmin debe venir ya elevado (ElevarPrivilegiosSinEmpresaAsync): crear una
    /// empresa, un usuario global y un rol/usuario_rol de otra empresa necesita leer y escribir sin
    /// el filtro normal, igual que CrearEmpresaCommand/IniciarSesionCommand en producción.</summary>
    public static async Task<Sesion> CrearAsync(HttpClient client, AppDbContext dbSuperAdmin, string nombreEmpresa)
    {
        var rucDePrueba = $"999{Random.Shared.Next(0, 1_000_000_000):D10}";
        var empresa = new Empresa(nombreEmpresa, rucDePrueba);
        dbSuperAdmin.Empresas.Add(empresa);

        var email = $"prueba-{Guid.NewGuid():N}@correo-de-prueba.test";
        var usuario = new Usuario(email, new HashService().Hash(Password));
        dbSuperAdmin.Usuarios.Add(usuario);

        var rol = new Rol(empresa.Id, "Rol de prueba");
        dbSuperAdmin.Roles.Add(rol);

        var usuarioRol = new UsuarioRol(usuario.Id, empresa.Id, rol.Id, sucursalId: null);
        dbSuperAdmin.UsuarioRoles.Add(usuarioRol);

        await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);

        var respuesta = await client.PostAsJsonAsync("/api/auth/iniciar-sesion", new { Email = email, Password }, JsonWeb);
        respuesta.EnsureSuccessStatusCode();
        var resultado = await respuesta.Content.ReadFromJsonAsync<IniciarSesionResultado>(JsonWeb)
            ?? throw new InvalidOperationException("iniciar-sesion no devolvió ningún cuerpo.");

        return new Sesion(empresa, usuario, rol, usuarioRol, resultado.TokenSesion
            ?? throw new InvalidOperationException("iniciar-sesion no devolvió un token de sesión (¿más de una empresa?)."));
    }

    /// <summary>Usa las mismas instancias que CrearAsync dejó rastreadas en dbSuperAdmin (el mismo
    /// contexto): un Remove sobre una entidad ya rastreada no necesita IgnoreQueryFilters ni volver
    /// a consultarla, a diferencia de limpiar desde un contexto nuevo.</summary>
    public static async Task LimpiarAsync(AppDbContext dbSuperAdmin, Sesion sesion)
    {
        dbSuperAdmin.UsuarioRoles.Remove(sesion.UsuarioRol);
        dbSuperAdmin.Roles.Remove(sesion.Rol);
        dbSuperAdmin.Usuarios.Remove(sesion.Usuario);
        dbSuperAdmin.Empresas.Remove(sesion.Empresa);
        await dbSuperAdmin.SaveChangesAsync(CancellationToken.None);
    }
}
