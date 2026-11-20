namespace TallerOS.Application.Comun;

/// <summary>Nombres de claims usados tanto al emitir el JWT (Infrastructure.TokenService) como al
/// leerlo (Api.HttpContextTenantContext, controllers). Centralizados aquí para que ambos lados usen
/// exactamente el mismo literal.</summary>
public static class JwtClaims
{
    public const string UsuarioId = "usuario_id";
    public const string EmpresaId = "empresa_id";
    public const string EsSuperAdmin = "es_super_admin";

    /// <summary>Un claim por cada código de permiso que el usuario tiene en la empresa activa
    /// (ver PermisoAuthorizationHandler, Api). Solo en el token de sesión; vacío para Super Admin,
    /// que salta el chequeo por el claim EsSuperAdmin en vez de llevar todos los códigos.</summary>
    public const string Permiso = "permiso";

    /// <summary>"preauth" (solo identifica al usuario, usado únicamente para elegir empresa) o
    /// "sesion" (acceso real a la API, con empresa y privilegios ya resueltos).</summary>
    public const string TipoToken = "tipo_token";
    public const string TipoTokenPreAuth = "preauth";
    public const string TipoTokenSesion = "sesion";
}
