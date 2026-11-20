using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Clientes.Commands.DesactivarCliente;

public sealed record DesactivarClienteCommand(Guid ClienteId) : IRequest<Unit>;
