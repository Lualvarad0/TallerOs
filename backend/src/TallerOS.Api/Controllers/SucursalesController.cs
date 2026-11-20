using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Organizacion.Sucursales;
using TallerOS.Application.Organizacion.Sucursales.Commands.ActualizarSucursal;
using TallerOS.Application.Organizacion.Sucursales.Commands.CrearSucursal;
using TallerOS.Application.Organizacion.Sucursales.Commands.DesactivarSucursal;
using TallerOS.Application.Organizacion.Sucursales.Queries.ListarSucursales;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/sucursales")]
[Authorize(Policy = "Sesion")]
public sealed class SucursalesController(ISender sender) : ControllerBase
{
    public sealed record ActualizarSucursalRequest(string Nombre, string? Direccion);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SucursalDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarSucursalesQuery(), cancellationToken);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear(CrearSucursalCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{sucursalId:guid}")]
    public async Task<IActionResult> Actualizar(Guid sucursalId, ActualizarSucursalRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarSucursalCommand(sucursalId, request.Nombre, request.Direccion), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{sucursalId:guid}")]
    public async Task<IActionResult> Desactivar(Guid sucursalId, CancellationToken cancellationToken)
    {
        await sender.Send(new DesactivarSucursalCommand(sucursalId), cancellationToken);
        return NoContent();
    }
}
