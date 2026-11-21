using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.OrdenesTrabajo;

namespace TallerOS.Application.OrdenesTrabajo.Subtareas.Commands.CrearSubtarea;

public sealed class CrearSubtareaCommandHandler(IAppDbContext db, ITenantContext tenant) : IRequestHandler<CrearSubtareaCommand, Guid>
{
    public async Task<Guid> Handle(CrearSubtareaCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        var otExiste = await db.OrdenesTrabajo.AnyAsync(o => o.Id == request.OtId, cancellationToken);
        if (!otExiste)
        {
            throw new EntidadNoEncontradaException("Orden de trabajo", request.OtId);
        }

        var tipoTrabajoExiste = await db.TiposTrabajo.AnyAsync(t => t.Id == request.TipoTrabajoId && t.EliminadoEn == null, cancellationToken);
        if (!tipoTrabajoExiste)
        {
            throw new EntidadNoEncontradaException("Tipo de trabajo", request.TipoTrabajoId);
        }

        var subtarea = new Subtarea(empresaId, request.OtId, request.TipoTrabajoId, request.ResponsablePersonalId, request.TiempoEstimadoMin, request.Orden);
        db.Subtareas.Add(subtarea);
        await db.SaveChangesAsync(cancellationToken);

        return subtarea.Id;
    }
}
