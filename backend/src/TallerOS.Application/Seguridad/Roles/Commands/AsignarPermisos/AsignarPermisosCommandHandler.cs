using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Application.Seguridad.Roles.Commands.AsignarPermisos;

public sealed class AsignarPermisosCommandHandler(IAppDbContext db) : IRequestHandler<AsignarPermisosCommand, Unit>
{
    public async Task<Unit> Handle(AsignarPermisosCommand request, CancellationToken cancellationToken)
    {
        var existeRol = await db.Roles.AnyAsync(r => r.Id == request.RolId && r.EliminadoEn == null, cancellationToken);
        if (!existeRol)
        {
            throw new EntidadNoEncontradaException("Rol", request.RolId);
        }

        var codigosValidos = await db.Permisos.Select(p => p.Codigo).ToHashSetAsync(cancellationToken);
        var desconocidos = request.Codigos.Where(c => !codigosValidos.Contains(c)).ToList();
        if (desconocidos.Count > 0)
        {
            throw new EntidadNoEncontradaException("Permiso", string.Join(", ", desconocidos));
        }

        var actuales = await db.RolPermisos.Where(rp => rp.RolId == request.RolId).ToListAsync(cancellationToken);
        db.RolPermisos.RemoveRange(actuales);
        db.RolPermisos.AddRange(request.Codigos.Distinct().Select(codigo => new RolPermiso(request.RolId, codigo)));

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
