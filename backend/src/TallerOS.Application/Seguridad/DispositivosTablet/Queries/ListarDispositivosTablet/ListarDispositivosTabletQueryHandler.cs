using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.DispositivosTablet.Queries.ListarDispositivosTablet;

public sealed class ListarDispositivosTabletQueryHandler(IAppDbContext db)
    : IRequestHandler<ListarDispositivosTabletQuery, IReadOnlyList<DispositivoTabletDto>>
{
    public async Task<IReadOnlyList<DispositivoTabletDto>> Handle(ListarDispositivosTabletQuery request, CancellationToken cancellationToken)
    {
        return await db.DispositivosTablet
            .OrderBy(d => d.Nombre)
            .Select(d => new DispositivoTabletDto(d.Id, d.SucursalId, d.Nombre, d.Activo, d.UltimoUsoEn))
            .ToListAsync(cancellationToken);
    }
}
