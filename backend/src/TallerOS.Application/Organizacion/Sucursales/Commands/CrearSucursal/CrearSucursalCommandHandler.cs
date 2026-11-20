using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Application.Organizacion.Sucursales.Commands.CrearSucursal;

public sealed class CrearSucursalCommandHandler(IAppDbContext db, ITenantContext tenant) : IRequestHandler<CrearSucursalCommand, Guid>
{
    public async Task<Guid> Handle(CrearSucursalCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        var nombreEnUso = await db.Sucursales.AnyAsync(
            s => s.EmpresaId == empresaId && s.Nombre == request.Nombre && s.EliminadoEn == null, cancellationToken);
        if (nombreEnUso)
        {
            throw new ConflictoDeNegocioException($"Ya existe una sucursal llamada '{request.Nombre}'.");
        }

        var sucursal = new Sucursal(empresaId, request.Nombre, request.Direccion);
        db.Sucursales.Add(sucursal);
        await db.SaveChangesAsync(cancellationToken);

        return sucursal.Id;
    }
}
