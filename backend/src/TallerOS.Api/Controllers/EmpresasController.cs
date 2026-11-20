using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Organizacion.Empresas;
using TallerOS.Application.Organizacion.Empresas.Commands.ActualizarEmpresa;
using TallerOS.Application.Organizacion.Empresas.Commands.CambiarEstadoEmpresa;
using TallerOS.Application.Organizacion.Empresas.Commands.CrearEmpresa;
using TallerOS.Application.Organizacion.Empresas.Queries.ObtenerEmpresa;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/empresas")]
[Authorize(Policy = "Sesion")]
public sealed class EmpresasController(ISender sender) : ControllerBase
{
    public sealed record ActualizarEmpresaRequest(string Nombre, string Ruc);

    public sealed record CambiarEstadoEmpresaRequest(EstadoEmpresa NuevoEstado);

    // Registro público de un taller nuevo en TallerOS: todavía no hay sesión.
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CrearEmpresaResultado>> Crear(CrearEmpresaCommand command, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(command, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{empresaId:guid}")]
    public async Task<ActionResult<EmpresaDto>> Obtener(Guid empresaId, CancellationToken cancellationToken)
    {
        var dto = await sender.Send(new ObtenerEmpresaQuery(empresaId), cancellationToken);
        return Ok(dto);
    }

    [HttpPut("{empresaId:guid}")]
    public async Task<IActionResult> Actualizar(Guid empresaId, ActualizarEmpresaRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new ActualizarEmpresaCommand(empresaId, request.Nombre, request.Ruc), cancellationToken);
        return NoContent();
    }

    [HttpPut("{empresaId:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid empresaId, CambiarEstadoEmpresaRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new CambiarEstadoEmpresaCommand(empresaId, request.NuevoEstado), cancellationToken);
        return NoContent();
    }
}
