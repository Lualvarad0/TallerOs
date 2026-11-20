using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Catalogos.Marcas.Commands.CrearMarca;

public sealed record CrearMarcaCommand(short TipoVehiculoId, string Nombre) : IRequest<Guid>;
