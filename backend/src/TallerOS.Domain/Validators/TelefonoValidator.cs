using PhoneNumbers;

namespace TallerOS.Domain.Validators;

/// <summary>Teléfono ecuatoriano (celular o convencional), vía libphonenumber: acepta tanto formato
/// local ("0991234567") como E.164 ("+593991234567").</summary>
public static class TelefonoValidator
{
    private const string Region = "EC";

    public static bool EsValido(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            return false;
        }

        try
        {
            var util = PhoneNumberUtil.GetInstance();
            var numero = util.Parse(telefono, Region);
            return util.IsValidNumberForRegion(numero, Region);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
