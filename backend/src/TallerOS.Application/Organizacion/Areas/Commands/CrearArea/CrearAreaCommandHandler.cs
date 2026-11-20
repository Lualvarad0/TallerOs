using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Application.Organizacion.Areas.Commands.CrearArea;

public sealed class CrearAreaCommandHandler(IAppDbContext db, ITenantContext tenant) : IRequestHandler<CrearAreaCommand, Guid>
{
    public async Task<Guid> Handle(CrearAreaCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        // El filtro global ya acota Sucursales a la empresa de la sesión: si esto no aparece, o no
        // existe o es de otra empresa (da igual cuál, el mensaje es el mismo).
        var existeSucursal = await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId && s.EliminadoEn == null, cancellationToken);
        if (!existeSucursal)
        {
            throw new EntidadNoEncontradaException("Sucursal", request.SucursalId);
        }

        var nombreEnUso = await db.Areas.AnyAsync(
            a => a.SucursalId == request.SucursalId && a.Nombre == request.Nombre && a.EliminadoEn == null, cancellationToken);
        if (nombreEnUso)
        {
            throw new ConflictoDeNegocioException($"Ya existe un área llamada '{request.Nombre}' en esa sucursal.");
        }

        var area = new Area(empresaId, request.SucursalId, request.Nombre);
        db.Areas.Add(area);
        await db.SaveChangesAsync(cancellationToken);

        return area.Id;
    }
}
