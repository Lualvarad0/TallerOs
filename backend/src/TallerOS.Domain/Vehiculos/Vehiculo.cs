namespace TallerOS.Domain.Vehiculos;

/// <summary>Catálogo global (sin empresa_id, ver docs/convenciones.md § Seguridad): el mismo vehículo puede
/// pasar por varios talleres a lo largo de su vida, así que sus datos técnicos no pertenecen a
/// ninguna empresa. Quién puede verlo lo decide ClienteVehiculo (ver AppDbContext), no esta tabla.
/// Vin es nulo cuando no se pudo leerlo (vehículo viejo, VIN ilegible); la placa vive aparte, en
/// VehiculoPlaca, porque cambia con el tiempo y el VIN no.</summary>
public class Vehiculo
{
    public Guid Id { get; private set; }
    public string? Vin { get; private set; }
    public Guid MarcaId { get; private set; }
    public Guid ModeloId { get; private set; }
    public short Anio { get; private set; }
    public string? Motor { get; private set; }

    private Vehiculo() { }

    public Vehiculo(string? vin, Guid marcaId, Guid modeloId, short anio, string? motor)
    {
        Id = Guid.CreateVersion7();
        Vin = vin;
        MarcaId = marcaId;
        ModeloId = modeloId;
        Anio = anio;
        Motor = motor;
    }

    /// <summary>Solo completa el VIN cuando antes era nulo (ej. se capturó manualmente después). No
    /// existe un método para reemplazar uno que ya tenía valor: eso es un conflicto, no una
    /// corrección, y este módulo no expone ninguna forma de resolverlo.</summary>
    public void CompletarVin(string vin)
    {
        if (Vin is not null)
        {
            throw new InvalidOperationException("El vehículo ya tiene un VIN registrado.");
        }

        Vin = vin;
    }
}
