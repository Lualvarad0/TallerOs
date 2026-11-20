using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Domain.Organizacion;

namespace TallerOS.Application.Seguridad.Autenticacion.Commands.IniciarSesion;

public sealed class IniciarSesionCommandHandler(IAppDbContext db, IHashService hash, ITokenService tokens)
    : IRequestHandler<IniciarSesionCommand, IniciarSesionResultado>
{
    public async Task<IniciarSesionResultado> Handle(IniciarSesionCommand request, CancellationToken cancellationToken)
    {
        // Como en CrearEmpresaCommand: todavía no hay empresa de sesión, así que hace falta la
        // elevación de RLS (Postgres) para poder leer Usuario/UsuarioRol/Empresa de quien sea.
        await db.ElevarPrivilegiosSinEmpresaAsync(cancellationToken);

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email && u.Activo, cancellationToken);
        if (usuario is null || !hash.Verificar(request.Password, usuario.PasswordHash))
        {
            throw new CredencialesInvalidasException();
        }

        // Super Admin es acceso de plataforma, no de una empresa: no elige ninguna.
        if (usuario.EsSuperAdmin)
        {
            var tokenSesion = tokens.GenerarTokenSesion(usuario.Id, empresaId: null, esSuperAdmin: true, permisos: []);
            return new IniciarSesionResultado(tokenSesion, TokenPreAutenticacion: null, Empresas: []);
        }

        // Filtro global de EF (independiente de la elevación de RLS de arriba): sin un ITenantContext
        // real, por defecto no deja ver ninguna empresa ni ningún UsuarioRol.
        var empresas = await db.UsuarioRoles.IgnoreQueryFilters()
            .Where(ur => ur.UsuarioId == usuario.Id)
            .Join(
                db.Empresas.IgnoreQueryFilters().Where(e => e.EliminadoEn == null && e.Estado == EstadoEmpresa.Activa),
                ur => ur.EmpresaId, e => e.Id, (_, e) => new EmpresaAccesibleDto(e.Id, e.Nombre))
            .Distinct()
            .ToListAsync(cancellationToken);

        if (empresas.Count == 0)
        {
            // Mismo mensaje genérico que una contraseña incorrecta: no revela que el usuario existe
            // pero quedó sin ninguna empresa activa.
            throw new CredencialesInvalidasException();
        }

        if (empresas.Count == 1)
        {
            var permisos = await PermisosEfectivos.ObtenerAsync(db, usuario.Id, empresas[0].EmpresaId, cancellationToken);
            var tokenSesion = tokens.GenerarTokenSesion(usuario.Id, empresas[0].EmpresaId, esSuperAdmin: false, permisos);
            return new IniciarSesionResultado(tokenSesion, TokenPreAutenticacion: null, Empresas: []);
        }

        var tokenPreAuth = tokens.GenerarTokenPreAutenticacion(usuario.Id);
        return new IniciarSesionResultado(TokenSesion: null, tokenPreAuth, empresas);
    }
}
