using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.OrdenesTrabajo.Subtareas.Commands.ActualizarSubtarea;

public sealed class ActualizarSubtareaCommandHandler(IAppDbContext db) : IRequestHandler<ActualizarSubtareaCommand, Unit>
{
    public async Task<Unit> Handle(ActualizarSubtareaCommand request, CancellationToken cancellationToken)
    {
        var subtarea = await db.Subtareas.FirstOrDefaultAsync(s => s.Id == request.SubtareaId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Subtarea", request.SubtareaId);

        subtarea.AsignarResponsable(request.ResponsablePersonalId);
        subtarea.CambiarEstado(request.Estado);

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
