using System.Text.RegularExpressions;

namespace TallerOS.Domain.Validators;

/// <summary>Formato de placa ecuatoriana (ANT): "ABC-1234" para autos/camionetas/camiones, "AB-1234"
/// para motos. Es solo una advertencia, no un bloqueo: la placa puede cambiar de formato con el
/// tiempo (la de la ANT no es la identidad del vehículo, el VIN sí) y hay excepciones oficiales
/// (diplomáticas, de prueba, históricas) que no siguen este patrón.</summary>
public static partial class PlacaValidator
{
    [GeneratedRegex(@"^[A-Z]{2,3}-\d{3,4}$")]
    private static partial Regex Formato();

    public static bool TieneFormatoReconocido(string? placa) =>
        placa is not null && Formato().IsMatch(placa.ToUpperInvariant());
}
