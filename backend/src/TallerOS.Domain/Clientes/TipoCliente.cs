using System.Text.Json.Serialization;

namespace TallerOS.Domain.Clientes;

// String en JSON (y en la base, ver ClienteConfiguration.HasConversion<string>): el frontend nunca
// recibe un número mágico para esto.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoCliente
{
    PersonaNatural,
    Empresa,
}
