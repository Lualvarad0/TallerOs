using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Seguridad.UsuariosRoles.Queries.ListarUsuarios;

public sealed class ListarUsuariosQueryHandler(IAppDbContext db) : IRequestHandler<ListarUsuariosQuery, IReadOnlyList<UsuarioRolDto>>
{
    public async Task<IReadOnlyList<UsuarioRolDto>> Handle(ListarUsuariosQuery request, CancellationToken cancellationToken)
    {
        var asignaciones = await db.UsuarioRoles.ToListAsync(cancellationToken);
        var usuarios = await db.Usuarios
            .Where(u => asignaciones.Select(a => a.UsuarioId).Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);
        var roles = await db.Roles
            .Where(r => asignaciones.Select(a => a.RolId).Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        return asignaciones
            .Select(a => new UsuarioRolDto(a.Id, a.UsuarioId, usuarios[a.UsuarioId].Email, a.RolId, roles[a.RolId].Nombre, a.SucursalId))
            .ToList();
    }
}
