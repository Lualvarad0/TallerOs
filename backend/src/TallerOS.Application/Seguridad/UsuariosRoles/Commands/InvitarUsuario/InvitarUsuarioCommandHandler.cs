using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Application.Seguridad.UsuariosRoles.Commands.InvitarUsuario;

public sealed class InvitarUsuarioCommandHandler(IAppDbContext db, ITenantContext tenant, IHashService hash)
    : IRequestHandler<InvitarUsuarioCommand, Guid>
{
    public async Task<Guid> Handle(InvitarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var empresaId = tenant.EmpresaId ?? throw new SinContextoDeEmpresaException();

        var existeRol = await db.Roles.AnyAsync(r => r.Id == request.RolId && r.EliminadoEn == null, cancellationToken);
        if (!existeRol)
        {
            throw new EntidadNoEncontradaException("Rol", request.RolId);
        }

        if (request.SucursalId is { } sucursalId
            && !await db.Sucursales.AnyAsync(s => s.Id == sucursalId && s.EliminadoEn == null, cancellationToken))
        {
            throw new EntidadNoEncontradaException("Sucursal", sucursalId);
        }

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (usuario is null)
        {
            if (request.Password is null)
            {
                throw new ConflictoDeNegocioException(
                    $"No existe una cuenta con el correo {request.Email}: hace falta una contraseña para crearla.");
            }

            usuario = new Usuario(request.Email, hash.Hash(request.Password));
            db.Usuarios.Add(usuario);
        }

        db.UsuarioRoles.Add(new UsuarioRol(usuario.Id, empresaId, request.RolId, request.SucursalId));
        await db.SaveChangesAsync(cancellationToken);

        return usuario.Id;
    }
}
