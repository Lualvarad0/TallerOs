using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.OrdenesTrabajo.Commands.CambiarEstadoOt;
using TallerOS.Application.OrdenesTrabajo.Queries.ListarTablero;
using TallerOS.Application.OrdenesTrabajo.Queries.ObtenerOrdenTrabajo;
using TallerOS.Application.OrdenesTrabajo.Subtareas.Commands.CrearSubtarea;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/ordenes-trabajo")]
[Authorize(Policy = "Sesion")]
public sealed class OrdenesTrabajoController(ISender sender) : ControllerBase
{
    public sealed record CambiarEstadoRequest(Guid NuevoEstadoId);

    public sealed record CrearSubtareaRequest(Guid TipoTrabajoId, Guid? ResponsablePersonalId, int TiempoEstimadoMin, short Orden);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TableroOtDto>>> ListarTablero(
        [FromQuery] Guid? sucursalId,
        [FromQuery] Guid? asesorPersonalId,
        [FromQuery] PrioridadOt? prioridad,
        [FromQuery] Guid? estadoId,
        CancellationToken cancellationToken)
    {
        var tablero = await sender.Send(new ListarTableroQuery(sucursalId, asesorPersonalId, prioridad, estadoId), cancellationToken);
        return Ok(tablero);
    }

    [HttpGet("{otId:guid}")]
    public async Task<ActionResult<OrdenTrabajoDetalleDto>> Obtener(Guid otId, CancellationToken cancellationToken)
    {
        var detalle = await sender.Send(new ObtenerOrdenTrabajoQuery(otId), cancellationToken);
        return Ok(detalle);
    }

    [HttpPut("{otId:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid otId, CambiarEstadoRequest request, CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirst(JwtClaims.UsuarioId)!.Value);
        await sender.Send(new CambiarEstadoOtCommand(otId, request.NuevoEstadoId, usuarioId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{otId:guid}/subtareas")]
    public async Task<ActionResult<Guid>> CrearSubtarea(Guid otId, CrearSubtareaRequest request, CancellationToken cancellationToken)
    {
        var command = new CrearSubtareaCommand(otId, request.TipoTrabajoId, request.ResponsablePersonalId, request.TiempoEstimadoMin, request.Orden);
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }
}
