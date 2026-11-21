using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.TiposTrabajo.Queries.ListarTiposTrabajo;

public sealed class ListarTiposTrabajoQueryHandler(IAppDbContext db) : IRequestHandler<ListarTiposTrabajoQuery, IReadOnlyList<TipoTrabajoDto>>
{
    public async Task<IReadOnlyList<TipoTrabajoDto>> Handle(ListarTiposTrabajoQuery request, CancellationToken cancellationToken)
    {
        return await db.TiposTrabajo
            .Where(t => t.EliminadoEn == null)
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoTrabajoDto(t.Id, t.Nombre, t.TiempoEstandarMin, t.IntervaloKm, t.IntervaloMeses))
            .ToListAsync(cancellationToken);
    }
}
