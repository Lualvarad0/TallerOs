using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.Commands.CambiarEstadoOt;

public sealed class CambiarEstadoOtCommandHandler(IAppDbContext db, ITenantContext tenant) : IRequestHandler<CambiarEstadoOtCommand, Unit>
{
    public async Task<Unit> Handle(CambiarEstadoOtCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        var ot = await db.OrdenesTrabajo.FirstOrDefaultAsync(o => o.Id == request.OtId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Orden de trabajo", request.OtId);

        // El filtro global de EF ya acota estado_ot a esta empresa, así que si NuevoEstadoId es de
        // otra empresa esto no lo encuentra: 404, no un estado prestado de otra empresa.
        var nuevoEstado = await db.EstadosOt.FirstOrDefaultAsync(e => e.Id == request.NuevoEstadoId, cancellationToken)
            ?? throw new EntidadNoEncontradaException("Estado de OT", request.NuevoEstadoId);

        var estadoAnteriorId = ot.EstadoId;
        ot.CambiarEstado(nuevoEstado.Id, nuevoEstado.EsFinal);

        db.HistorialEstadoOt.Add(new HistorialEstadoOt(empresaId, ot.Id, estadoAnteriorId, nuevoEstado.Id, request.UsuarioId));

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Valor;
    }
}
