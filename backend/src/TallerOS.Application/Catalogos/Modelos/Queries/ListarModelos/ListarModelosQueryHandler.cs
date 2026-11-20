using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Catalogos.Modelos.Queries.ListarModelos;

public sealed class ListarModelosQueryHandler(IAppDbContext db) : IRequestHandler<ListarModelosQuery, IReadOnlyList<ModeloDto>>
{
    public async Task<IReadOnlyList<ModeloDto>> Handle(ListarModelosQuery request, CancellationToken cancellationToken)
    {
        return await db.Modelos
            .Where(m => m.MarcaId == request.MarcaId)
            .OrderBy(m => m.Nombre)
            .Select(m => new ModeloDto(m.Id, m.MarcaId, m.Nombre))
            .ToListAsync(cancellationToken);
    }
}
