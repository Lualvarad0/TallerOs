using Microsoft.AspNetCore.Authorization;

namespace TallerOS.Api.Autenticacion;

/// <summary>Exige que la sesión tenga el claim de permiso con este código exacto (ver JwtClaims.Permiso),
/// o que sea Super Admin. La crea PermisoPolicyProvider a partir del nombre de policy
/// "permiso:{codigo}" (PermisoPolicy.Para).</summary>
public sealed class PermisoRequirement(string codigo) : IAuthorizationRequirement
{
    public string Codigo { get; } = codigo;
}
