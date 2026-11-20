// Cédula ecuatoriana (10 dígitos): provincia (01-24), tercer dígito 0-5 (persona natural; 6 y 9
// son de RUC de entidad pública/sociedad, no de cédula) y dígito verificador módulo 10.
export function esCedulaValida(cedula: string): boolean {
  if (!/^\d{10}$/.test(cedula)) return false;

  const digitos = cedula.split('').map(Number);

  const provincia = digitos[0] * 10 + digitos[1];
  if (provincia < 1 || provincia > 24) return false;

  if (digitos[2] > 5) return false;

  return digitos[9] === digitoVerificadorModulo10(digitos);
}

function digitoVerificadorModulo10(digitos: number[]): number {
  let suma = 0;
  for (let i = 0; i < 9; i++) {
    const valor = digitos[i] * (i % 2 === 0 ? 2 : 1);
    suma += valor > 9 ? valor - 9 : valor;
  }

  const residuo = suma % 10;
  return residuo === 0 ? 0 : 10 - residuo;
}
