using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.SeleccionarEmpresa;

public sealed class SeleccionarEmpresaCommandHandler(IAppDbContext db, ITokenService tokens)
    : IRequestHandler<SeleccionarEmpresaCommand, SeleccionarEmpresaResultado>
{
    public async Task<SeleccionarEmpresaResultado> Handle(SeleccionarEmpresaCommand request, CancellationToken cancellationToken)
    {
        await db.ElevarPrivilegiosSinEmpresaAsync(cancellationToken);

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.Activo, cancellationToken)
            ?? throw new CredencialesInvalidasException();

        var tieneAcceso = await db.UsuarioRoles.IgnoreQueryFilters()
            .AnyAsync(ur => ur.UsuarioId == request.UsuarioId && ur.EmpresaId == request.EmpresaId, cancellationToken);
        if (!tieneAcceso)
        {
            throw new EntidadNoEncontradaException("Empresa", request.EmpresaId);
        }

        var permisos = await PermisosEfectivos.ObtenerAsync(db, usuario.Id, request.EmpresaId, cancellationToken);
        var tokenSesion = tokens.GenerarTokenSesion(usuario.Id, request.EmpresaId, usuario.EsSuperAdmin, permisos);
        return new SeleccionarEmpresaResultado(tokenSesion);
    }
}
