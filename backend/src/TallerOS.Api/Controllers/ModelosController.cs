using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Catalogos.Modelos.Commands.CrearModelo;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/modelos")]
[Authorize(Policy = "Sesion")]
public sealed class ModelosController(ISender sender) : ControllerBase
{
    // Ver el comentario de MarcasController.Crear: mismo permiso, mismo catálogo global.
    [HttpPost]
    [Authorize(Policy = PermisoPolicy.Prefijo + "catalogo_vehiculo.gestionar")]
    public async Task<ActionResult<Guid>> Crear(CrearModeloCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }
}
