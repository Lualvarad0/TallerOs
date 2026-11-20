using TallerOS.Domain.Clientes;

namespace TallerOS.Application.Clientes;

public sealed record ClienteDto(
    Guid Id,
    string Nombre,
    TipoCliente Tipo,
    string Identificacion,
    string? Telefono,
    string? Email,
    CanalPreferido CanalPreferido);
