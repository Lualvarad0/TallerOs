using System.Text.Json.Serialization;

namespace TallerOS.Domain.OrdenesTrabajo;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoDanio
{
    Golpe,
    Rayon,
    Abolladura,
    Vidrio,
}
