namespace TallerOS.Application.Comun;

/// <summary>Prefijo de policy de ASP.NET Core para exigir un permiso puntual (ver PermisoPolicyProvider
/// y PermisoAuthorizationHandler, Api): [Authorize(Policy = PermisoPolicy.Prefijo + "catalogo_vehiculo.gestionar")].
/// Tiene que ser un literal/const porque los argumentos de atributo se evalúan en tiempo de
/// compilación. Cualquier módulo nuevo reutiliza esto mismo, no inventa su propio mecanismo.</summary>
public static class PermisoPolicy
{
    public const string Prefijo = "permiso:";
}
