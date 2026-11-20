namespace TallerOS.Domain.Validators;

/// <summary>Cédula ecuatoriana (10 dígitos): provincia (01-24), tercer dígito 0-5 (persona natural;
/// 6 y 9 son de RUC de entidad pública/sociedad, no de cédula) y dígito verificador módulo 10.</summary>
public static class CedulaValidator
{
    public static bool EsValida(string? cedula)
    {
        if (cedula is null || cedula.Length != 10 || !cedula.All(char.IsAsciiDigit))
        {
            return false;
        }

        var digitos = cedula.Select(c => c - '0').ToArray();

        var provincia = digitos[0] * 10 + digitos[1];
        if (provincia < 1 || provincia > 24)
        {
            return false;
        }

        if (digitos[2] > 5)
        {
            return false;
        }

        return digitos[9] == DigitoVerificadorModulo10(digitos);
    }

    private static int DigitoVerificadorModulo10(int[] digitos)
    {
        var suma = 0;
        for (var i = 0; i < 9; i++)
        {
            var valor = digitos[i] * (i % 2 == 0 ? 2 : 1);
            suma += valor > 9 ? valor - 9 : valor;
        }

        var residuo = suma % 10;
        return residuo == 0 ? 0 : 10 - residuo;
    }
}
