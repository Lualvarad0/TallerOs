using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.Queries.ListarEstadosOt;

public sealed class ListarEstadosOtQueryHandler(IAppDbContext db) : IRequestHandler<ListarEstadosOtQuery, IReadOnlyList<EstadoOtDto>>
{
    public async Task<IReadOnlyList<EstadoOtDto>> Handle(ListarEstadosOtQuery request, CancellationToken cancellationToken)
    {
        return await db.EstadosOt
            .OrderBy(e => e.Orden)
            .Select(e => new EstadoOtDto(e.Id, e.Nombre, e.Orden, e.EsFinal, e.ColorToken))
            .ToListAsync(cancellationToken);
    }
}
