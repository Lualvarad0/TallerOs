using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using PersonalEntidad = TallerOS.Domain.Organizacion.Personal;

namespace TallerOS.Application.Organizacion.Personal.Commands.CrearPersonal;

public sealed class CrearPersonalCommandHandler(IAppDbContext db, ITenantContext tenant, IHashService hash)
    : IRequestHandler<CrearPersonalCommand, Guid>
{
    public async Task<Guid> Handle(CrearPersonalCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        var existeArea = await db.Areas.AnyAsync(a => a.Id == request.AreaId && a.EliminadoEn == null, cancellationToken);
        if (!existeArea)
        {
            throw new EntidadNoEncontradaException("Area", request.AreaId);
        }

        if (request.UsuarioId is { } usuarioId && !await db.Usuarios.AnyAsync(u => u.Id == usuarioId, cancellationToken))
        {
            throw new EntidadNoEncontradaException("Usuario", usuarioId);
        }

        var pinHash = request.Pin is null ? null : hash.Hash(request.Pin);

        var personal = new PersonalEntidad(empresaId, request.AreaId, request.Nombre, request.TarifaHora, request.TieneAcceso, request.UsuarioId, pinHash);
        db.Personal.Add(personal);
        await db.SaveChangesAsync(cancellationToken);

        return personal.Id;
    }
}
