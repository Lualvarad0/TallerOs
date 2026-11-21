using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.OrdenesTrabajo.Queries.ListarEstadosOt;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/estados-ot")]
[Authorize(Policy = "Sesion")]
public sealed class EstadosOtController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EstadoOtDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarEstadosOtQuery(), cancellationToken);
        return Ok(lista);
    }
}
