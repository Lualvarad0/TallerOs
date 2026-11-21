using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.TiposTrabajo.Commands.CrearTipoTrabajo;

public sealed class CrearTipoTrabajoCommandHandler(IAppDbContext db, ITenantContext tenant) : IRequestHandler<CrearTipoTrabajoCommand, Guid>
{
    public async Task<Guid> Handle(CrearTipoTrabajoCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        var nombreEnUso = await db.TiposTrabajo.AnyAsync(
            t => t.EmpresaId == empresaId && t.Nombre == request.Nombre && t.EliminadoEn == null, cancellationToken);
        if (nombreEnUso)
        {
            throw new ConflictoDeNegocioException($"Ya existe un tipo de trabajo con el nombre '{request.Nombre}'.");
        }

        var tipoTrabajo = new TipoTrabajo(empresaId, request.Nombre, request.TiempoEstandarMin, request.IntervaloKm, request.IntervaloMeses);
        db.TiposTrabajo.Add(tipoTrabajo);
        await db.SaveChangesAsync(cancellationToken);

        return tipoTrabajo.Id;
    }
}
