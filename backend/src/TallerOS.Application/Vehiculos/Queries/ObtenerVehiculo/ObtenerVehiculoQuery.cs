using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.Vehiculos.Queries.ObtenerVehiculo;

public sealed record ObtenerVehiculoQuery(Guid VehiculoId) : IRequest<VehiculoDetalleDto>;
