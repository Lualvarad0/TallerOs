using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TallerOS.Application.Catalogos.Marcas;
using TallerOS.Application.Catalogos.Marcas.Commands.CrearMarca;
using TallerOS.Application.Catalogos.Marcas.Queries.ListarMarcas;
using TallerOS.Application.Catalogos.Modelos;
using TallerOS.Application.Catalogos.Modelos.Queries.ListarModelos;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Api.Controllers;

[ApiController]
[Route("api/marcas")]
[Authorize(Policy = "Sesion")]
public sealed class MarcasController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MarcaDto>>> Listar([FromQuery] short? tipoVehiculoId, CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarMarcasQuery(tipoVehiculoId), cancellationToken);
        return Ok(lista);
    }

    [HttpGet("{marcaId:guid}/modelos")]
    public async Task<ActionResult<IReadOnlyList<ModeloDto>>> ListarModelos(Guid marcaId, CancellationToken cancellationToken)
    {
        var lista = await sender.Send(new ListarModelosQuery(marcaId), cancellationToken);
        return Ok(lista);
    }

    // Solo Admin/Supervisor (o quien tenga este permiso asignado): catálogo global, afecta a todas
    // las empresas, así que no es algo que cualquier usuario autenticado pueda agregar.
    [HttpPost]
    [Authorize(Policy = PermisoPolicy.Prefijo + "catalogo_vehiculo.gestionar")]
    public async Task<ActionResult<Guid>> Crear(CrearMarcaCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Ok(new { id });
    }
}
