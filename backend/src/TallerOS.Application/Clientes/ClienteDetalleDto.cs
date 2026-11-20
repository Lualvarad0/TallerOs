using TallerOS.Domain.Clientes;

namespace TallerOS.Application.Clientes;

public sealed record ClienteDetalleDto(
    Guid Id,
    string Nombre,
    TipoCliente Tipo,
    string Identificacion,
    string? Telefono,
    string? Email,
    CanalPreferido CanalPreferido,
    IReadOnlyList<VehiculoResumenDto> Vehiculos);
