using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Areas.Commands.DesactivarArea;

public sealed class DesactivarAreaCommandHandler(IAppDbContext db) : IRequestHandler<DesactivarAreaCommand, Unit>
{
    public async Task<Unit> Handle(DesactivarAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await db.Areas.FirstOrDefaultAsync(a => a.Id == request.AreaId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Area", request.AreaId);

        area.Desactivar();
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
