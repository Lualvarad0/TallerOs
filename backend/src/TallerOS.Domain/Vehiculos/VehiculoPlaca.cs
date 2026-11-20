namespace TallerOS.Domain.Vehiculos;

/// <summary>Historial de placas de un vehículo (catálogo global, igual que Vehiculo): la placa
/// cambia con el tiempo, el VIN no, así que la identidad del vehículo nunca depende de esta tabla.
/// Hasta nulo = placa activa. Este módulo solo crea la primera fila al registrar un vehículo nuevo;
/// cambiar de placa (cerrar una fila y abrir otra) queda fuera de alcance por ahora.</summary>
public class VehiculoPlaca
{
    public Guid Id { get; private set; }
    public Guid VehiculoId { get; private set; }
    public string Placa { get; private set; } = string.Empty;
    public DateOnly Desde { get; private set; }
    public DateOnly? Hasta { get; private set; }

    private VehiculoPlaca() { }

    public VehiculoPlaca(Guid vehiculoId, string placa, DateOnly desde)
    {
        Id = Guid.CreateVersion7();
        VehiculoId = vehiculoId;
        Placa = placa;
        Desde = desde;
    }
}
