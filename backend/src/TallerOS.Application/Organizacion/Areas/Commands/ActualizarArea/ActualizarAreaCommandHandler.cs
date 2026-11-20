using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Areas.Commands.ActualizarArea;

public sealed class ActualizarAreaCommandHandler(IAppDbContext db) : IRequestHandler<ActualizarAreaCommand, Unit>
{
    public async Task<Unit> Handle(ActualizarAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await db.Areas.FirstOrDefaultAsync(a => a.Id == request.AreaId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Area", request.AreaId);

        area.Renombrar(request.Nombre);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
