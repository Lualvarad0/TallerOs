using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Clientes.Queries.ObtenerCliente;

public sealed record ObtenerClienteQuery(Guid ClienteId) : IRequest<ClienteDetalleDto>;
