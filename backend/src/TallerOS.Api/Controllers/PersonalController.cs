using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Organizacion.Personal;
using TallerOS.Application.Organizacion.Personal.Commands.ActualizarPersonal;
using TallerOS.Application.Organizacion.Personal.Commands.CrearPersonal;
using TallerOS.Application.Organizacion.Personal.Commands.DesactivarPersonal;
using TallerOS.Application.Organizacion.Personal.Queries.ListarPersonal;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/personal")]
[Authorize(Policy = "Sesion")]
public sealed class PersonalController(ISender sender) : ControllerBase
{
    public sealed record ActualizarPersonalRequest(Guid AreaId, string Nombre, decimal? TarifaHora);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PersonalDto>>> Listar([FromQuery] Guid areaId, CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarPersonalQuery(areaId), cancellationToken);
        return Ok(lista);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Crear(CrearPersonalCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{personalId:guid}")]
    public async Task<IActionResult> Actualizar(Guid personalId, ActualizarPersonalRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarPersonalCommand(personalId, request.AreaId, request.Nombre, request.TarifaHora), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{personalId:guid}")]
    public async Task<IActionResult> Desactivar(Guid personalId, CancellationToken cancellationToken)
    {
        await sender.Send(new DesactivarPersonalCommand(personalId), cancellationToken);
        return NoContent();
    }
}
