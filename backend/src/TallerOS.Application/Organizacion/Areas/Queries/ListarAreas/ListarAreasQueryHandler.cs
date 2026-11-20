using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Areas.Queries.ListarAreas;

public sealed class ListarAreasQueryHandler(IAppDbContext db) : IRequestHandler<ListarAreasQuery, IReadOnlyList<AreaDto>>
{
    public async Task<IReadOnlyList<AreaDto>> Handle(ListarAreasQuery request, CancellationToken cancellationToken)
    {
        return await db.Areas
            .Where(a => a.SucursalId == request.SucursalId && a.EliminadoEn == null)
            .OrderBy(a => a.Nombre)
            .Select(a => new AreaDto(a.Id, a.SucursalId, a.Nombre))
            .ToListAsync(cancellationToken);
    }
}
