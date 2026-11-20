using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Vehiculos;
using TallerOS.Application.Vehiculos.Commands.RegistrarVehiculo;
using TallerOS.Application.Vehiculos.Queries.ObtenerVehiculo;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/vehiculos")]
[Authorize(Policy = "Sesion")]
public sealed class VehiculosController(ISender sender) : ControllerBase
{
    [HttpGet("{vehiculoId:guid}")]
    public async Task<ActionResult<VehiculoDetalleDto>> Obtener(Guid vehiculoId, CancellationToken cancellationToken)
    {
        var detalle = await sender.Send(new ObtenerVehiculoQuery(vehiculoId), cancellationToken);
        return Ok(detalle);
    }

    [HttpPost]
    public async Task<ActionResult<RegistrarVehiculoResultado>> Registrar(RegistrarVehiculoCommand command, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(command, cancellationToken);
        return Ok(resultado);
    }
}
