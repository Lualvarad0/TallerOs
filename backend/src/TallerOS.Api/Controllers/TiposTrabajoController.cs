using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.OrdenesTrabajo.TiposTrabajo.Commands.CrearTipoTrabajo;
using TallerOS.Application.OrdenesTrabajo.TiposTrabajo.Queries.ListarTiposTrabajo;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/tipos-trabajo")]
[Authorize(Policy = "Sesion")]
public sealed class TiposTrabajoController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TipoTrabajoDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarTiposTrabajoQuery(), cancellationToken);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear(CrearTipoTrabajoCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }
}
