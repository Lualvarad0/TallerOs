using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Application.Seguridad.Roles.Commands.CrearRol;

public sealed class CrearRolCommandHandler(IAppDbContext db, ITenantContext tenant) : IRequestHandler<CrearRolCommand, Guid>
{
    public async Task<Guid> Handle(CrearRolCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        var nombreEnUso = await db.Roles.AnyAsync(
            r => r.EmpresaId == empresaId && r.Nombre == request.Nombre && r.EliminadoEn == null, cancellationToken);
        if (nombreEnUso)
        {
            throw new ConflictoDeNegocioException($"Ya existe un rol llamado '{request.Nombre}'.");
        }

        var rol = new Rol(empresaId, request.Nombre);
        db.Roles.Add(rol);
        await db.SaveChangesAsync(cancellationToken);

        return rol.Id;
    }
}
