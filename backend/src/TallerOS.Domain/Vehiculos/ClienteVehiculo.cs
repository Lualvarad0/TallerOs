using TallerOS.Domain.Clientes;

namespace TallerOS.Domain.Vehiculos;

/// <summary>El único vínculo entre un cliente de una empresa y un vehículo (global). Crearlo es lo
/// que concede a esa empresa visibilidad sobre los datos del vehículo (ver AppDbContext); por eso
/// este módulo no expone un endpoint que reciba un VehiculoId suelto, solo RegistrarVehiculo, que
/// resuelve el vehículo él mismo.</summary>
public class ClienteVehiculo
{
    public Guid Id { get; private set; }
    public Guid EmpresaId { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VehiculoId { get; private set; }
    public RolClienteVehiculo Rol { get; private set; }

    private ClienteVehiculo() { }

    public ClienteVehiculo(Guid empresaId, Guid clienteId, Guid vehiculoId, RolClienteVehiculo rol)
    {
        Id = Guid.CreateVersion7();
        EmpresaId = empresaId;
        ClienteId = clienteId;
        VehiculoId = vehiculoId;
        Rol = rol;
    }
}
