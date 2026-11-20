using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Organizacion.Sucursales.Commands.CrearSucursal;

public sealed record CrearSucursalCommand(string Nombre, string? Direccion) : IRequest<Guid>;
