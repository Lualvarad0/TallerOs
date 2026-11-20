using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Roles.Queries.ListarPermisos;

public sealed class ListarPermisosQueryHandler(IAppDbContext db) : IRequestHandler<ListarPermisosQuery, IReadOnlyList<PermisoDto>>
{
    public async Task<IReadOnlyList<PermisoDto>> Handle(ListarPermisosQuery request, CancellationToken cancellationToken)
    {
        return await db.Permisos
            .OrderBy(p => p.Codigo)
            .Select(p => new PermisoDto(p.Codigo, p.Nombre, p.Descripcion))
            .ToListAsync(cancellationToken);
    }
}
