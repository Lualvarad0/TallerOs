namespace TallerOS.Domain.Seguridad;

/// <summary>Une un usuario a un rol dentro de una empresa, opcionalmente acotado a una sucursal.
/// SucursalId nulo = el rol aplica a todas las sucursales de esa empresa.</summary>
public class UsuarioRol
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid? SucursalId { get; private set; }
    public Guid RolId { get; private set; }

    private UsuarioRol() { }

    public UsuarioRol(Guid usuarioId, Guid empresaId, Guid rolId, Guid? sucursalId)
    {
        Id = Guid.CreateVersion7();
        UsuarioId = usuarioId;
        EmpresaId = empresaId;
        RolId = rolId;
        SucursalId = sucursalId;
    }
}
