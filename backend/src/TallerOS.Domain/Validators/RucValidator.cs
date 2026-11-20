namespace TallerOS.Domain.Validators;

/// <summary>RUC ecuatoriano (13 dígitos), con sus tres estructuras según el tercer dígito:
/// persona natural (0-5) = cédula + código de establecimiento; sociedad privada (9) y entidad
/// pública (6) = dígito verificador propio, módulo 11.</summary>
public static class RucValidator
{
    public static bool EsValido(string? ruc)
    {
        if (ruc is null || ruc.Length != 13 || !ruc.All(char.IsAsciiDigit))
        {
            return false;
        }

        var digitos = ruc.Select(c => c - '0').ToArray();

        var provincia = digitos[0] * 10 + digitos[1];
        if (provincia < 1 || provincia > 24)
        {
            return false;
        }

        return digitos[2] switch
        {
            <= 5 => EsPersonaNatural(ruc, digitos),
            9 => EsSociedadPrivada(digitos),
            6 => EsEntidadPublica(digitos),
            _ => false,
        };
    }

    private static bool EsPersonaNatural(string ruc, int[] digitos)
    {
        if (!CedulaValidator.EsValida(ruc[..10]))
        {
            return false;
        }

        var establecimiento = digitos[10] * 100 + digitos[11] * 10 + digitos[12];
        return establecimiento >= 1;
    }

    private static bool EsSociedadPrivada(int[] digitos)
    {
        int[] coeficientes = [4, 3, 2, 7, 6, 5, 4, 3, 2];
        if (digitos[9] != DigitoVerificadorModulo11(digitos, coeficientes))
        {
            return false;
        }

        var establecimiento = digitos[10] * 100 + digitos[11] * 10 + digitos[12];
        return establecimiento >= 1;
    }

    private static bool EsEntidadPublica(int[] digitos)
    {
        int[] coeficientes = [3, 2, 7, 6, 5, 4, 3, 2];
        if (digitos[8] != DigitoVerificadorModulo11(digitos, coeficientes))
        {
            return false;
        }

        // Las entidades públicas siempre terminan en "0001" (no tienen más de un establecimiento).
        return digitos[9] == 0 && digitos[10] == 0 && digitos[11] == 0 && digitos[12] == 1;
    }

    private static int DigitoVerificadorModulo11(int[] digitos, int[] coeficientes)
    {
        var suma = coeficientes.Select((coef, i) => coef * digitos[i]).Sum();
        var residuo = suma % 11;
        return residuo == 0 ? 0 : 11 - residuo;
    }
}
