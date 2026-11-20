using Microsoft.EntityFrameworkCore;

namespace TallerOS.Application.Comun;

/// <summary>Códigos de permiso que un usuario tiene en una empresa, vía los roles que le asignaron
/// (usuario_rol → rol_permiso). Usado al emitir el token de sesión (IniciarSesionCommandHandler,
/// SeleccionarEmpresaCommandHandler): ambos corren sin ITenantContext real todavía, así que ignora
/// los filtros globales igual que las demás consultas de esos handlers.</summary>
public static class PermisosEfectivos
{
    public static async Task<IReadOnlyCollection<string>> ObtenerAsync(
        IAppDbContext db, Guid usuarioId, Guid empresaId, CancellationToken cancellationToken)
    {
        return await db.UsuarioRoles.IgnoreQueryFilters()
            .Where(ur => ur.UsuarioId == usuarioId && ur.EmpresaId == empresaId)
            .Join(db.RolPermisos.IgnoreQueryFilters(), ur => ur.RolId, rp => rp.RolId, (_, rp) => rp.PermisoCodigo)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
