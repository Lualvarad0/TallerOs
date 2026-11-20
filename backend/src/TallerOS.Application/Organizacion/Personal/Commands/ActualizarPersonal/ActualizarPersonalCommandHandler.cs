using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Personal.Commands.ActualizarPersonal;

public sealed class ActualizarPersonalCommandHandler(IAppDbContext db) : IRequestHandler<ActualizarPersonalCommand, Unit>
{
    public async Task<Unit> Handle(ActualizarPersonalCommand request, CancellationToken cancellationToken)
    {
        var personal = await db.Personal.FirstOrDefaultAsync(p => p.Id == request.PersonalId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Personal", request.PersonalId);

        var existeArea = await db.Areas.AnyAsync(a => a.Id == request.AreaId && a.EliminadoEn == null, cancellationToken);
        if (!existeArea)
        {
            throw new EntidadNoEncontradaException("Area", request.AreaId);
        }

        personal.ActualizarDatos(request.AreaId, request.Nombre, request.TarifaHora);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
