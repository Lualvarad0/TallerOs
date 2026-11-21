using Microsoft.EntityFrameworkCore;
using TallerOS.Application.Comun;
using TallerOS.Application.Comun.Cqrs;
using TallerOS.Application.Comun.Excepciones;
using TallerOS.Application.OrdenesTrabajo;
using TallerOS.Domain.Organizacion;
using TallerOS.Domain.Plataforma;
using TallerOS.Domain.Seguridad;

namespace TallerOS.Application.Organizacion.Empresas.Commands.CrearEmpresa;

public sealed class CrearEmpresaCommandHandler(IAppDbContext db, IHashService hash)
    : IRequestHandler<CrearEmpresaCommand, CrearEmpresaResultado>
{
    public async Task<CrearEmpresaResultado> Handle(CrearEmpresaCommand request, CancellationToken cancellationToken)
    {
        // Quien llama a este endpoint todavía no tiene empresa (la está creando): sin esto, RLS
        // le ocultaría incluso las empresas de otros para el chequeo de RUC duplicado de abajo.
        await db.ElevarPrivilegiosSinEmpresaAsync(cancellationToken);

        // Esa elevación es del lado de PostgreSQL (RLS). El filtro global de EF Core (primera
        // barrera, en AppDbContext) es independiente y por defecto no deja ver ninguna empresa
        // sin un ITenantContext real; IgnoreQueryFilters() lo salta solo para esta lectura.
        if (await db.Empresas.IgnoreQueryFilters().AnyAsync(e => e.Ruc == request.Ruc && e.EliminadoEn == null, cancellationToken))
        {
            throw new ConflictoDeNegocioException($"Ya existe una empresa registrada con el RUC {request.Ruc}.");
        }

        if (await db.Usuarios.AnyAsync(u => u.Email == request.EmailDueño, cancellationToken))
        {
            throw new ConflictoDeNegocioException($"Ya existe una cuenta con el correo {request.EmailDueño}.");
        }

        if (!await db.Planes.AnyAsync(p => p.Id == request.PlanId && p.Activo, cancellationToken))
        {
            throw new EntidadNoEncontradaException("Plan", request.PlanId);
        }

        var empresa = new Empresa(request.Nombre, request.Ruc);
        db.Empresas.Add(empresa);

        var suscripcion = new Suscripcion(empresa.Id, request.PlanId, DateTimeOffset.UtcNow);
        db.Suscripciones.Add(suscripcion);

        var rolDueño = new Rol(empresa.Id, "Dueño/Gerente");
        db.Roles.Add(rolDueño);

        // El rol fundador recibe todos los permisos que existan hoy en el catálogo; no se reevalúa
        // retroactivamente cuando el catálogo crezca en otro módulo.
        var codigosPermiso = await db.Permisos.Select(p => p.Codigo).ToListAsync(cancellationToken);
        db.RolPermisos.AddRange(codigosPermiso.Select(codigo => new RolPermiso(rolDueño.Id, codigo)));

        var usuario = new Usuario(request.EmailDueño, hash.Hash(request.PasswordDueño));
        db.Usuarios.Add(usuario);

        // sucursalId nulo: el dueño administra todas las sucursales desde el día uno.
        db.UsuarioRoles.Add(new UsuarioRol(usuario.Id, empresa.Id, rolDueño.Id, sucursalId: null));

        // Módulo 4: los 8 estados estándar del flujo de OT y el contador de numeración nacen con la
        // empresa, no cuando se crea la primera OT (ver EstadosOtEstandar).
        await EstadosOtEstandar.SembrarAsync(db, empresa.Id, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return new CrearEmpresaResultado(empresa.Id, usuario.Id);
    }
}
