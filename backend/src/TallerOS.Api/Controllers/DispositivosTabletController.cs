using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Seguridad.DispositivosTablet;
using TallerOS.Application.Seguridad.DispositivosTablet.Commands.RegistrarDispositivoTablet;
using TallerOS.Application.Seguridad.DispositivosTablet.Commands.RevocarDispositivoTablet;
using TallerOS.Application.Seguridad.DispositivosTablet.Queries.ListarDispositivosTablet;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/dispositivos-tablet")]
[Authorize(Policy = "Sesion")]
public sealed class DispositivosTabletController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DispositivoTabletDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarDispositivosTabletQuery(), cancellationToken);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<RegistrarDispositivoTabletResultado>> Registrar(RegistrarDispositivoTabletCommand command, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(command, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("{dispositivoId:guid}/revocar")]
    public async Task<IActionResult> Revocar(Guid dispositivoId, CancellationToken cancellationToken)
    {
        await sender.Send(new RevocarDispositivoTabletCommand(dispositivoId), cancellationToken);
        return NoContent();
    }
}
