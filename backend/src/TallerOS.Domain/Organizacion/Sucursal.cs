namespace TallerOS.Domain.Organizacion;

public class Sucursal
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Direccion { get; private set; }
    public DateTimeOffset? EliminadoEn { get; private set; }

    private Sucursal() { }

    public Sucursal(Guid empresaId, string nombre, string? direccion)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        Nombre = nombre;
        Direccion = direccion;
    }

    public void ActualizarDatos(string nombre, string? direccion)
    {
        Nombre = nombre;
        Direccion = direccion;
    }

    public void Desactivar() => EliminadoEn = DateTimeOffset.UtcNow;
}
