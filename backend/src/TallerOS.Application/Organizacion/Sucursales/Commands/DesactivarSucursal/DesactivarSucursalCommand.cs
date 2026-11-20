using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Sucursales.Commands.DesactivarSucursal;

public sealed record DesactivarSucursalCommand(Guid SucursalId) : IRequest<Unit>;
