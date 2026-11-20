using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Sucursales.Commands.ActualizarSucursal;

public sealed record ActualizarSucursalCommand(Guid SucursalId, string Nombre, string? Direccion) : IRequest<Unit>;
