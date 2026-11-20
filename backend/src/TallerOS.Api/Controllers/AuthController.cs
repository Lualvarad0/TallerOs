using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Seguridad.Autenticacion.Commands.IniciarSesion;
using TallerOS.Application.Seguridad.Autenticacion.Commands.SeleccionarEmpresa;
using TallerOS.Application.Seguridad.Autenticacion.Commands.VerificarPin;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    public sealed record IniciarSesionRequest(string Email, string Password);

    public sealed record SeleccionarEmpresaRequest(Guid EmpresaId);

    public sealed record VerificarPinRequest(string TokenDispositivo, string Pin);

    [HttpPost("iniciar-sesion")]
    [AllowAnonymous]
    public async Task<ActionResult<IniciarSesionResultado>> IniciarSesion(IniciarSesionRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new IniciarSesionCommand(request.Email, request.Password), cancellationToken);
        return Ok(resultado);
    }

    // UsuarioId sale del token pre-auth validado por [Authorize], nunca del body: evita que alguien
    // pida un token de sesión para otro usuario mandando cualquier id.
    [HttpPost("seleccionar-empresa")]
    [Authorize(Policy = "PreAuth")]
    public async Task<ActionResult<SeleccionarEmpresaResultado>> SeleccionarEmpresa(SeleccionarEmpresaRequest request, CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirst(JwtClaims.UsuarioId)!.Value);
        var resultado = await sender.Send(new SeleccionarEmpresaCommand(usuarioId, request.EmpresaId), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("verificar-pin")]
    [AllowAnonymous]
    public async Task<ActionResult<VerificarPinResultado>> VerificarPin(VerificarPinRequest request, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new VerificarPinCommand(request.TokenDispositivo, request.Pin), cancellationToken);
        return Ok(resultado);
    }
}
