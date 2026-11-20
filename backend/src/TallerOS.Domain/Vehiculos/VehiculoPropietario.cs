namespace TallerOS.Domain.Vehiculos;

/// <summary>Historial de propiedad del vehículo en Mi Garage (catálogo global): quién lo posee según
/// esa app, no según el taller (eso es ClienteVehiculo). Reservada para cuando se construya la
/// transferencia de propiedad; nada en este módulo crea ni lee filas aquí todavía, y su acceso
/// (ver AppDbContext) no se abre a ninguna empresa mientras tanto.</summary>
public class VehiculoPropietario
{
    public Guid Id { get; private set; }
    public Guid VehiculoId { get; private set; }
    public Guid CuentaClienteId { get; private set; }
    public DateOnly Desde { get; private set; }
    public DateOnly? Hasta { get; private set; }

    private VehiculoPropietario() { }

    public VehiculoPropietario(Guid vehiculoId, Guid cuentaClienteId, DateOnly desde)
    {
        Id = Guid.CreateVersion7();
        VehiculoId = vehiculoId;
        CuentaClienteId = cuentaClienteId;
        Desde = desde;
    }
}
