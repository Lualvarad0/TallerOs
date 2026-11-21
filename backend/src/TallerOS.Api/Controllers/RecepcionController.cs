using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.OrdenesTrabajo.Commands.RegistrarRecepcion;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/recepcion")]
[Authorize(Policy = "Sesion")]
public sealed class RecepcionController(ISender sender) : ControllerBase
{
    public sealed record RecepcionRequest(
        Guid SucursalId,
        Guid VehiculoId,
        Guid ClienteId,
        PrioridadOt Prioridad,
        int KmIngreso,
        Guid? AsesorPersonalId,
        short Combustible,
        string? Objetos,
        string? Observaciones,
        IReadOnlyList<DanioPrevioInput> DaniosPrevios);

    // UsuarioId sale del token de sesión (igual que en AuthController.SeleccionarEmpresa), nunca
    // del body: queda en historial_estado_ot como quién registró el ingreso.
    [HttpPost]
    public async Task<ActionResult<RegistrarRecepcionResultado>> Registrar(RecepcionRequest request, CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirst(JwtClaims.UsuarioId)!.Value);
        var command = new RegistrarRecepcionCommand(
            request.SucursalId, request.VehiculoId, request.ClienteId, request.Prioridad, request.KmIngreso,
            request.AsesorPersonalId, request.Combustible, request.Objetos, request.Observaciones,
            request.DaniosPrevios, usuarioId);

        var resultado = await sender.Send(command, cancellationToken);
        return Ok(resultado);
    }
}
