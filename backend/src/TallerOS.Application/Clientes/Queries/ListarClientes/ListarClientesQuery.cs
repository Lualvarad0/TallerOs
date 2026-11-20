using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Clientes.Queries.ListarClientes;

public sealed record ListarClientesQuery(string? Busqueda) : IRequest<IReadOnlyList<ClienteDto>>;
