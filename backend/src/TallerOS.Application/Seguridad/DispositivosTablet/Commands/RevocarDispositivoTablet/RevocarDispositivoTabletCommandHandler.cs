using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Seguridad.DispositivosTablet.Commands.RevocarDispositivoTablet;

public sealed class RevocarDispositivoTabletCommandHandler(IAppDbContext db) : IRequestHandler<RevocarDispositivoTabletCommand, Unit>
{
    public async Task<Unit> Handle(RevocarDispositivoTabletCommand request, CancellationToken cancellationToken)
    {
        var dispositivo = await db.DispositivosTablet.FirstOrDefaultAsync(d => d.Id == request.DispositivoId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("DispositivoTablet", request.DispositivoId);

        dispositivo.Revocar();
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
