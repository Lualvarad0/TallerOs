using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Catalogos.Marcas.Queries.ListarMarcas;

public sealed class ListarMarcasQueryHandler(IAppDbContext db) : IRequestHandler<ListarMarcasQuery, IReadOnlyList<MarcaDto>>
{
    public async Task<IReadOnlyList<MarcaDto>> Handle(ListarMarcasQuery request, CancellationToken cancellationToken)
    {
        var consulta = db.Marcas.AsQueryable();
        if (request.TipoVehiculoId is not null)
        {
            consulta = consulta.Where(m => m.TipoVehiculoId == request.TipoVehiculoId);
        }

        return await consulta
            .OrderBy(m => m.Nombre)
            .Select(m => new MarcaDto(m.Id, m.TipoVehiculoId, m.Nombre))
            .ToListAsync(cancellationToken);
    }
}
