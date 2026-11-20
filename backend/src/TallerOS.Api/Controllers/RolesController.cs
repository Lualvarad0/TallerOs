using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Seguridad.Roles;
using TallerOS.Application.Seguridad.Roles.Commands.AsignarPermisos;
using TallerOS.Application.Seguridad.Roles.Commands.CrearRol;
using TallerOS.Application.Seguridad.Roles.Queries.ListarPermisos;
using TallerOS.Application.Seguridad.Roles.Queries.ListarRoles;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Policy = "Sesion")]
public sealed class RolesController(ISender sender) : ControllerBase
{
    public sealed record AsignarPermisosRequest(IReadOnlyList<string> Codigos);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RolDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarRolesQuery(), cancellationToken);
        return Ok(lista);
    }

    // Catálogo de permisos disponibles (global, no por empresa): lo usa el frontend para armar el
    // formulario de AsignarPermisos.
    [HttpGet("permisos")]
    public async Task<ActionResult<IReadOnlyList<PermisoDto>>> ListarPermisos(CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarPermisosQuery(), cancellationToken);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear(CrearRolCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{rolId:guid}/permisos")]
    public async Task<IActionResult> AsignarPermisos(Guid rolId, AsignarPermisosRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new AsignarPermisosCommand(rolId, request.Codigos), cancellationToken);
        return NoContent();
    }
}
