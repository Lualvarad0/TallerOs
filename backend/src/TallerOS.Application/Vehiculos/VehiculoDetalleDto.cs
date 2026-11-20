namespace TallerOS.Application.Vehiculos;

/// <summary>Placa es la activa, no el historial completo (desde/hasta de cada placa pasada): lo
/// acordado para este comando es compartir solo "datos básicos" del vehículo (ver
/// RegistrarVehiculoCommand: "VIN, marca, modelo, año, motor, placa"), no una línea de tiempo de
/// cambios de placa, que además podría insinuar cuándo el vehículo cambió de dueño o de taller. El
/// historial completo, si algún día se expone, es una decisión de producto aparte, no un
/// subproducto de este query.</summary>
public sealed record VehiculoDetalleDto(
    Guid Id,
    string? Vin,
    Guid MarcaId,
    string Marca,
    Guid ModeloId,
    string Modelo,
    short Anio,
    string? Motor,
    string? Placa,
    IReadOnlyList<ClienteVinculadoDto> Clientes);
