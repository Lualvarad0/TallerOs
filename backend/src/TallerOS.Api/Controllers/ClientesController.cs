using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Clientes;
using TallerOS.Application.Clientes.Commands.ActualizarCliente;
using TallerOS.Application.Clientes.Commands.CrearCliente;
using TallerOS.Application.Clientes.Commands.DesactivarCliente;
using TallerOS.Application.Clientes.Queries.ListarClientes;
using TallerOS.Application.Clientes.Queries.ObtenerCliente;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Domain.Clientes;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(Policy = "Sesion")]
public sealed class ClientesController(ISender sender) : ControllerBase
{
    public sealed record ActualizarClienteRequest(string Nombre, string? Telefono, string? Email, CanalPreferido CanalPreferido);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteDto>>> Listar([FromQuery] string? busqueda, CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarClientesQuery(busqueda), cancellationToken);
        return Ok(lista);
    }

    [HttpGet("{clienteId:guid}")]
    public async Task<ActionResult<ClienteDetalleDto>> Obtener(Guid clienteId, CancellationToken cancellationToken)
    {
        var detalle = await sender.Send(new ObtenerClienteQuery(clienteId), cancellationToken);
        return Ok(detalle);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear(CrearClienteCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{clienteId:guid}")]
    public async Task<IActionResult> Actualizar(Guid clienteId, ActualizarClienteRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarClienteCommand(clienteId, request.Nombre, request.Telefono, request.Email, request.CanalPreferido), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{clienteId:guid}")]
    public async Task<IActionResult> Desactivar(Guid clienteId, CancellationToken cancellationToken)
    {
        await sender.Send(new DesactivarClienteCommand(clienteId), cancellationToken);
        return NoContent();
    }
}
