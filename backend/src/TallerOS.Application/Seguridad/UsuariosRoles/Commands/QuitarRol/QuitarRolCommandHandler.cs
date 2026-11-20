using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Seguridad.UsuariosRoles.Commands.QuitarRol;

public sealed class QuitarRolCommandHandler(IAppDbContext db) : IRequestHandler<QuitarRolCommand, Unit>
{
    public async Task<Unit> Handle(QuitarRolCommand request, CancellationToken cancellationToken)
    {
        var usuarioRol = await db.UsuarioRoles.FirstOrDefaultAsync(ur => ur.Id == request.UsuarioRolId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("UsuarioRol", request.UsuarioRolId);

        db.UsuarioRoles.Remove(usuarioRol);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
