using System.Text.Json.Serialization;

namespace TallerOS.Domain.Clientes;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CanalPreferido
{
    Correo,
    WhatsApp,
}
