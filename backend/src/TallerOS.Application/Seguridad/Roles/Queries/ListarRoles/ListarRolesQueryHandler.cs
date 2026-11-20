using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.Roles.Queries.ListarRoles;

public sealed class ListarRolesQueryHandler(IAppDbContext db) : IRequestHandler<ListarRolesQuery, IReadOnlyList<RolDto>>
{
    public async Task<IReadOnlyList<RolDto>> Handle(ListarRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await db.Roles
            .Where(r => r.EliminadoEn == null)
            .OrderBy(r => r.Nombre)
            .Select(r => new { r.Id, r.Nombre })
            .ToListAsync(cancellationToken);

        var permisosPorRol = await db.RolPermisos
            .Where(rp => roles.Select(r => r.Id).Contains(rp.RolId))
            .ToListAsync(cancellationToken);

        return roles
            .Select(r => new RolDto(r.Id, r.Nombre, permisosPorRol.Where(rp => rp.RolId == r.Id).Select(rp => rp.PermisoCodigo).ToList()))
            .ToList();
    }
}
