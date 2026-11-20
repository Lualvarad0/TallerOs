using TallerOS.Domain.Vehiculos;

namespace TallerOS.Application.Clientes;

/// <summary>Lo que un cliente necesita mostrar de cada uno de sus vehículos en su ficha: datos
/// propios del vehículo (ver Vehiculo, catálogo global) más el rol con el que ESTE cliente está
/// vinculado a él (ver ClienteVehiculo). Placa es la activa (Hasta == null); el historial completo
/// de placas vive en la ficha del vehículo, no en la del cliente.</summary>
public sealed record VehiculoResumenDto(
    Guid VehiculoId,
    string? Vin,
    string? Placa,
    Guid MarcaId,
    string Marca,
    Guid ModeloId,
    string Modelo,
    short Anio,
    RolClienteVehiculo Rol);
