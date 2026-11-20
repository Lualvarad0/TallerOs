namespace TallerOS.Domain.Catalogos;

/// <summary>Catálogo global (sin empresa_id). El año del vehículo no vive aquí: pertenece al
/// vehículo concreto del cliente (Módulo 3), no al catálogo de modelos.</summary>
public class Modelo
{
    public Guid Id { get; private set; }
    public Guid MarcaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    private Modelo() { }

    public Modelo(Guid marcaId, string nombre)
    {
        Id = Guid.CreateVersion7();
        MarcaId = marcaId;
        Nombre = nombre;
    }
}
