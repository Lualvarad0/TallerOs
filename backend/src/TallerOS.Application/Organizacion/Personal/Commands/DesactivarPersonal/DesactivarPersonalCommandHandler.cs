using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Personal.Commands.DesactivarPersonal;

public sealed class DesactivarPersonalCommandHandler(IAppDbContext db) : IRequestHandler<DesactivarPersonalCommand, Unit>
{
    public async Task<Unit> Handle(DesactivarPersonalCommand request, CancellationToken cancellationToken)
    {
        var personal = await db.Personal.FirstOrDefaultAsync(p => p.Id == request.PersonalId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Personal", request.PersonalId);

        personal.Desactivar();
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
