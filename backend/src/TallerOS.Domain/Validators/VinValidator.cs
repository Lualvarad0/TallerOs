namespace TallerOS.Domain.Validators;

/// <summary>VIN (17 caracteres, ISO 3779): dígito verificador en la posición 9 (índice 8), el mismo
/// algoritmo que usa NHTSA. I, O y Q no se usan nunca (se confunden con 1, 0 y 9), así que no están
/// en la tabla de transliteración.</summary>
public static class VinValidator
{
    private static readonly Dictionary<char, int> Transliteracion = new()
    {
        ['A'] = 1, ['B'] = 2, ['C'] = 3, ['D'] = 4, ['E'] = 5, ['F'] = 6, ['G'] = 7, ['H'] = 8,
        ['J'] = 1, ['K'] = 2, ['L'] = 3, ['M'] = 4, ['N'] = 5, ['P'] = 7, ['R'] = 9,
        ['S'] = 2, ['T'] = 3, ['U'] = 4, ['V'] = 5, ['W'] = 6, ['X'] = 7, ['Y'] = 8, ['Z'] = 9,
    };

    private static readonly int[] Pesos = [8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2];

    public static bool EsValido(string? vin)
    {
        if (vin is null || vin.Length != 17)
        {
            return false;
        }

        vin = vin.ToUpperInvariant();

        var suma = 0;
        for (var i = 0; i < 17; i++)
        {
            if (!TryValorTransliterado(vin[i], out var valor))
            {
                return false;
            }

            suma += valor * Pesos[i];
        }

        var residuo = suma % 11;
        var caracterEsperado = residuo == 10 ? 'X' : (char)('0' + residuo);
        return vin[8] == caracterEsperado;
    }

    private static bool TryValorTransliterado(char c, out int valor)
    {
        if (c is >= '0' and <= '9')
        {
            valor = c - '0';
            return true;
        }

        return Transliteracion.TryGetValue(c, out valor);
    }
}
