using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using TallerOS.Application.Comun;

namespace TallerOS.Api.Autenticacion;

/// <summary>Construye al vuelo una policy por cada código de permiso ("permiso:{codigo}"), en vez de
/// tener que registrar una por módulo en Program.cs. También exige token de sesión (no preauth), así
/// que [Authorize(Policy = PermisoPolicy.Para(...))] funciona solo con el [Authorize(Policy = "Sesion")]
/// del controller, o solo, sin depender de cuál se evalúe primero.</summary>
public sealed class PermisoPolicyProvider(IOptions<AuthorizationOptions> opciones) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _default = new(opciones);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PermisoPolicy.Prefijo, StringComparison.Ordinal))
        {
            return _default.GetPolicyAsync(policyName);
        }

        var codigo = policyName[PermisoPolicy.Prefijo.Length..];
        var policy = new AuthorizationPolicyBuilder()
            .RequireClaim(JwtClaims.TipoToken, JwtClaims.TipoTokenSesion)
            .AddRequirements(new PermisoRequirement(codigo))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _default.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _default.GetFallbackPolicyAsync();
}
