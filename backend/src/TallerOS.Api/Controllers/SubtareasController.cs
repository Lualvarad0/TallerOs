using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.OrdenesTrabajo.Subtareas.Commands.ActualizarSubtarea;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/subtareas")]
[Authorize(Policy = "Sesion")]
public sealed class SubtareasController(ISender sender) : ControllerBase
{
    public sealed record ActualizarSubtareaRequest(Guid? ResponsablePersonalId, EstadoSubtarea Estado);

    [HttpPut("{subtareaId:guid}")]
    public async Task<IActionResult> Actualizar(Guid subtareaId, ActualizarSubtareaRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarSubtareaCommand(subtareaId, request.ResponsablePersonalId, request.Estado), cancellationToken);
        return NoContent();
    }
}
