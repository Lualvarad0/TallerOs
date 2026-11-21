using System.Text.Json.Serialization;

namespace TallerOS.Domain.OrdenesTrabajo;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EstadoSubtarea
{
    Pendiente,
    EnCurso,
    Hecha,
}
