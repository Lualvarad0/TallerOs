using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Catalogos.Modelos.Commands.CrearModelo;

public sealed record CrearModeloCommand(Guid MarcaId, string Nombre) : IRequest<Guid>;
