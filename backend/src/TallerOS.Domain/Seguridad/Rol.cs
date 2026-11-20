namespace TallerOS.Domain.Seguridad;

public class Rol
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public DateTimeOffset? EliminadoEn { get; private set; }

    private Rol() { }

    public Rol(Guid empresaId, string nombre)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        Nombre = nombre;
    }

    public void Renombrar(string nombre) => Nombre = nombre;

    public void Desactivar() => EliminadoEn = DateTimeOffset.UtcNow;
}
