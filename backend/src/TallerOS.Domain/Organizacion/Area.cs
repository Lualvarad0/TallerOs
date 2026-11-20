namespace TallerOS.Domain.Organizacion;

public class Area
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid SucursalId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public DateTimeOffset? EliminadoEn { get; private set; }

    private Area() { }

    public Area(Guid empresaId, Guid sucursalId, string nombre)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        SucursalId = sucursalId;
        Nombre = nombre;
    }

    public void Renombrar(string nombre) => Nombre = nombre;

    public void Desactivar() => EliminadoEn = DateTimeOffset.UtcNow;
}
