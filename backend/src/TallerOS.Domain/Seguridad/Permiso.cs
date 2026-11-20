namespace TallerOS.Domain.Seguridad;

/// <summary>Catálogo global. La clave es el código (ej. "ot.asignar"), no un id numérico:
/// el código es lo que el código del backend y del frontend referencian directamente.</summary>
public class Permiso
{
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }

    private Permiso() { }

    public Permiso(string codigo, string nombre, string? descripcion = null)
    {
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
    }
}
