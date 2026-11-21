using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.TiposTrabajo.Commands.CrearTipoTrabajo;

public sealed record CrearTipoTrabajoCommand(string Nombre, int TiempoEstandarMin, int? IntervaloKm, int? IntervaloMeses) : IRequest<Guid>;
