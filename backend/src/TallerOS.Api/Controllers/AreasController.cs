using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Organizacion.Areas;
using TallerOS.Application.Organizacion.Areas.Commands.ActualizarArea;
using TallerOS.Application.Organizacion.Areas.Commands.CrearArea;
using TallerOS.Application.Organizacion.Areas.Commands.DesactivarArea;
using TallerOS.Application.Organizacion.Areas.Queries.ListarAreas;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/areas")]
[Authorize(Policy = "Sesion")]
public sealed class AreasController(ISender sender) : ControllerBase
{
    public sealed record ActualizarAreaRequest(string Nombre);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AreaDto>>> Listar([FromQuery] Guid sucursalId, CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarAreasQuery(sucursalId), cancellationToken);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear(CrearAreaCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{areaId:guid}")]
    public async Task<IActionResult> Actualizar(Guid areaId, ActualizarAreaRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarAreaCommand(areaId, request.Nombre), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{areaId:guid}")]
    public async Task<IActionResult> Desactivar(Guid areaId, CancellationToken cancellationToken)
    {
        await sender.Send(new DesactivarAreaCommand(areaId), cancellationToken);
        return NoContent();
    }
}
