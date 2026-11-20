using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Organizacion.Sucursales.Commands.ActualizarSucursal;

public sealed class ActualizarSucursalCommandHandler(IAppDbContext db) : IRequestHandler<ActualizarSucursalCommand, Unit>
{
    public async Task<Unit> Handle(ActualizarSucursalCommand request, CancellationToken cancellationToken)
    {
        var sucursal = await db.Sucursales.FirstOrDefaultAsync(s => s.Id == request.SucursalId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Sucursal", request.SucursalId);

        sucursal.ActualizarDatos(request.Nombre, request.Direccion);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
