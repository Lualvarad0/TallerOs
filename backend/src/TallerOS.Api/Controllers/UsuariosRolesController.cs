using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Seguridad.UsuariosRoles;
using TallerOS.Application.Seguridad.UsuariosRoles.Commands.InvitarUsuario;
using TallerOS.Application.Seguridad.UsuariosRoles.Commands.QuitarRol;
using TallerOS.Application.Seguridad.UsuariosRoles.Queries.ListarUsuarios;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = "Sesion")]
public sealed class UsuariosRolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioRolDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarUsuariosQuery(), cancellationToken);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Invitar(InvitarUsuarioCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }

    [HttpDelete("roles/{usuarioRolId:guid}")]
    public async Task<IActionResult> QuitarRol(Guid usuarioRolId, CancellationToken cancellationToken)
    {
        await sender.Send(new QuitarRolCommand(usuarioRolId), cancellationToken);
        return NoContent();
    }
}
