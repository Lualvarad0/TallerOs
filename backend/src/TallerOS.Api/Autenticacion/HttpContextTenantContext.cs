using TallerOS.Application.Comun;

namespace TallerOS.Api.Autenticacion;

/// <summary>Reemplaza a TenantContextPorDefecto (Infrastructure) una vez autenticado: lee empresa_id
/// y es_super_admin de los claims del JWT de sesión ya validado por el middleware JwtBearer. Si el
/// request no está autenticado, o trae un token pre-auth (sin esos claims), se comporta igual que el
/// default: sin empresa, sin privilegios.</summary>
public sealed class HttpContextTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid? EmpresaId
    {
        get
        {
            var valor = httpContextAccessor.HttpContext?.User.FindFirst(JwtClaims.EmpresaId)?.Value;
            return Guid.TryParse(valor, out var empresaId) ? empresaId : null;
        }
    }

    public bool EsSuperAdmin =>
        httpContextAccessor.HttpContext?.User.FindFirst(JwtClaims.EsSuperAdmin)?.Value == "true";
}
