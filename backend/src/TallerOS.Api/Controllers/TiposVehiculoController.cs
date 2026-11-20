using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Catalogos.TiposVehiculo;
using TallerOS.Application.Catalogos.TiposVehiculo.Queries.ListarTiposVehiculo;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/tipos-vehiculo")]
[Authorize(Policy = "Sesion")]
public sealed class TiposVehiculoController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TipoVehiculoDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarTiposVehiculoQuery(), cancellationToken);
        return Ok(lista);
    }
}
