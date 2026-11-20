using System.Text.Json.Serialization;

namespace TallerOS.Domain.Vehiculos;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RolClienteVehiculo
{
    Propietario,
    ConductorHabitual,
    ResponsableFlota,
}
