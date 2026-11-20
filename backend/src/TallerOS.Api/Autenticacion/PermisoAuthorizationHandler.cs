using Microsoft.AspNetCore.Authorization;
using TallerOS.Application.Comun;

namespace TallerOS.Api.Autenticacion;

public sealed class PermisoAuthorizationHandler : AuthorizationHandler<PermisoRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermisoRequirement requirement)
    {
        var esSuperAdmin = context.User.FindFirst(JwtClaims.EsSuperAdmin)?.Value == "true";
        if (esSuperAdmin || context.User.HasClaim(JwtClaims.Permiso, requirement.Codigo))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
