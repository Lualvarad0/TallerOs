// VIN (17 caracteres, ISO 3779): dígito verificador en la posición 9 (índice 8), el mismo
// algoritmo que usa NHTSA. I, O y Q no se usan nunca (se confunden con 1, 0 y 9), así que no están
// en la tabla de transliteración.
const TRANSLITERACION: Record<string, number> = {
  A: 1, B: 2, C: 3, D: 4, E: 5, F: 6, G: 7, H: 8,
  J: 1, K: 2, L: 3, M: 4, N: 5, P: 7, R: 9,
  S: 2, T: 3, U: 4, V: 5, W: 6, X: 7, Y: 8, Z: 9,
};

const PESOS = [8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2];

export function esVinValido(vin: string): boolean {
  if (vin.length !== 17) return false;

  vin = vin.toUpperCase();

  let suma = 0;
  for (let i = 0; i < 17; i++) {
    const valor = valorTransliterado(vin[i]);
    if (valor === null) return false;
    suma += valor * PESOS[i];
  }

  const residuo = suma % 11;
  const caracterEsperado = residuo === 10 ? 'X' : String(residuo);
  return vin[8] === caracterEsperado;
}

function valorTransliterado(c: string): number | null {
  if (c >= '0' && c <= '9') return Number(c);
  return TRANSLITERACION[c] ?? null;
}
