namespace TallerOS.Domain.Catalogos;

/// <summary>Catálogo global y fijo (Auto, Moto, etc.). Id smallint porque son pocos valores y otras
/// tablas (marca, y más adelante empresa_tipo_vehiculo, checklist_plantilla) lo referencian por id.</summary>
public class TipoVehiculo
{
    public short Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    private TipoVehiculo() { }

    public TipoVehiculo(short id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }
}
