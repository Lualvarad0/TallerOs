using TallerOS.Application.Comun.Cqrs;

namespace TallerOS.Application.OrdenesTrabajo.Queries.ListarEstadosOt;

/// <summary>El flujo de estados de OT es configurable por empresa (ver EstadoOt.cs), así que el
/// Kanban y el selector de "cambiar estado" del frontend necesitan este catálogo propio en vez de un
/// enum fijo. Hoy nace con los 8 estados estándar (EstadosOtEstandar) y no hay pantalla para editarlo.</summary>
public sealed record ListarEstadosOtQuery : IRequest<IReadOnlyList<EstadoOtDto>>;
