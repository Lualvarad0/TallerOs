using TallerOS.Application.Comun.Cqrs;
using TallerOS.Domain.Clientes;

namespace TallerOS.Application.Clientes.Commands.CrearCliente;

public sealed record CrearClienteCommand(
    string Nombre,
    TipoCliente Tipo,
    string Identificacion,
    string? Telefono,
    string? Email,
    CanalPreferido CanalPreferido) : IRequest<Guid>;
