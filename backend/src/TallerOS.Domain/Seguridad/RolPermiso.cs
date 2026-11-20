namespace TallerOS.Domain.Seguridad;

/// <summary>Clave compuesta (RolId, PermisoCodigo): un rol tiene un permiso, una sola vez.</summary>
public class RolPermiso
{
    public Guid RolId { get; private set; }
    public string PermisoCodigo { get; private set; } = string.Empty;

    private RolPermiso() { }

    public RolPermiso(Guid rolId, string permisoCodigo)
    {
        RolId = rolId;
        PermisoCodigo = permisoCodigo;
    }
}
