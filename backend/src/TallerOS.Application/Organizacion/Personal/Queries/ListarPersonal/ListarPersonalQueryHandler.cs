using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Personal.Queries.ListarPersonal;

public sealed class ListarPersonalQueryHandler(IAppDbContext db) : IRequestHandler<ListarPersonalQuery, IReadOnlyList<PersonalDto>>
{
    public async Task<IReadOnlyList<PersonalDto>> Handle(ListarPersonalQuery request, CancellationToken cancellationToken)
    {
        return await db.Personal
            .Where(p => p.AreaId == request.AreaId && p.EliminadoEn == null)
            .OrderBy(p => p.Nombre)
            .Select(p => new PersonalDto(p.Id, p.AreaId, p.Nombre, p.TarifaHora, p.TieneAcceso))
            .ToListAsync(cancellationToken);
    }
}
