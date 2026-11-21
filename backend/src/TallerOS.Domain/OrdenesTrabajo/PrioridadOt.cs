using System.Text.Json.Serialization;

namespace TallerOS.Domain.OrdenesTrabajo;

// String en JSON, igual que TipoCliente/CanalPreferido (ver docs/convenciones.md): sin esto, System.Text.Json
// serializa el enum como el número subyacente (0/1/2) aunque en la base se guarde como texto (ver
// OrdenTrabajoConfiguration.HasConversion<string>), y el frontend tendría que adivinar el orden.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PrioridadOt
{
    Normal,
    Alta,
    Urgente,
}
