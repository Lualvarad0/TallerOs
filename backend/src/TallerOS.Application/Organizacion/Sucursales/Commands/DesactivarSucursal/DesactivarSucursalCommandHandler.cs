using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Sucursales.Commands.DesactivarSucursal;

public sealed class DesactivarSucursalCommandHandler(IAppDbContext db) : IRequestHandler<DesactivarSucursalCommand, Unit>
{
    public async Task<Unit> Handle(DesactivarSucursalCommand request, CancellationToken cancellationToken)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == request.SucursalId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Sucursal", request.SucursalId);

        sucursal.Desactivar();
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
