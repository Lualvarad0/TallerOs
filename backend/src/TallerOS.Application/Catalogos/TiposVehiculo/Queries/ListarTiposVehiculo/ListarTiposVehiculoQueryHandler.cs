using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Catalogos.TiposVehiculo.Queries.ListarTiposVehiculo;

public sealed class ListarTiposVehiculoQueryHandler(IAppDbContext db)
    : IRequestHandler<ListarTiposVehiculoQuery, IReadOnlyList<TipoVehiculoDto>>
{
    public async Task<IReadOnlyList<TipoVehiculoDto>> Handle(ListarTiposVehiculoQuery request, CancellationToken cancellationToken)
    {
        // Catálogo global y fijo: sin filtro de empresa (ver docs/convenciones.md § Seguridad).
        return await db.TiposVehiculo
            .OrderBy(t => t.Id)
            .Select(t => new TipoVehiculoDto(t.Id, t.Nombre))
            .ToListAsync(cancellationToken);
    }
}
