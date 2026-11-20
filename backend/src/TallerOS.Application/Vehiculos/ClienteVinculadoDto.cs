using TallerOS.Domain.Vehiculos;

namespace TallerOS.Application.Vehiculos;

/// <summary>El filtro global de ClienteVehiculo (ver AppDbContext) ya acota esta lista a los
/// vínculos de la empresa de la sesión: nunca aparece aquí un cliente de otra empresa, aunque esa
/// otra empresa también haya atendido este mismo vehículo.</summary>
public sealed record ClienteVinculadoDto(Guid ClienteId, string Nombre, RolClienteVehiculo Rol);
