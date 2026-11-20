import { esCedulaValida } from './cedula';

// RUC ecuatoriano (13 dígitos), con sus tres estructuras según el tercer dígito: persona natural
// (0-5) = cédula + código de establecimiento; sociedad privada (9) y entidad pública (6) = dígito
// verificador propio, módulo 11.
export function esRucValido(ruc: string): boolean {
  if (!/^\d{13}$/.test(ruc)) return false;

  const digitos = ruc.split('').map(Number);

  const provincia = digitos[0] * 10 + digitos[1];
  if (provincia < 1 || provincia > 24) return false;

  if (digitos[2] <= 5) return esPersonaNatural(ruc, digitos);
  if (digitos[2] === 9) return esSociedadPrivada(digitos);
  if (digitos[2] === 6) return esEntidadPublica(digitos);
  return false;
}

function esPersonaNatural(ruc: string, digitos: number[]): boolean {
  if (!esCedulaValida(ruc.slice(0, 10))) return false;

  const establecimiento = digitos[10] * 100 + digitos[11] * 10 + digitos[12];
  return establecimiento >= 1;
}

function esSociedadPrivada(digitos: number[]): boolean {
  const coeficientes = [4, 3, 2, 7, 6, 5, 4, 3, 2];
  if (digitos[9] !== digitoVerificadorModulo11(digitos, coeficientes)) return false;

  const establecimiento = digitos[10] * 100 + digitos[11] * 10 + digitos[12];
  return establecimiento >= 1;
}

function esEntidadPublica(digitos: number[]): boolean {
  const coeficientes = [3, 2, 7, 6, 5, 4, 3, 2];
  if (digitos[8] !== digitoVerificadorModulo11(digitos, coeficientes)) return false;

  // Las entidades públicas siempre terminan en "0001" (no tienen más de un establecimiento).
  return digitos[9] === 0 && digitos[10] === 0 && digitos[11] === 0 && digitos[12] === 1;
}

function digitoVerificadorModulo11(digitos: number[], coeficientes: number[]): number {
  const suma = coeficientes.reduce((acc, coef, i) => acc + coef * digitos[i], 0);
  const residuo = suma % 11;
  return residuo === 0 ? 0 : 11 - residuo;
}

// esRucValido no distingue la estructura por fuera (solo dice si el checksum entero es válido); el
// tercer dígito es lo único que dice qué estructura es, igual que en el RucValidator de C#. Un
// profesional que factura con su propio RUC (persona natural) no debería poder registrarse como
// Tipo.Empresa con ese mismo RUC, y viceversa — ver CrearClienteCommandValidator (backend).
export function esRucDePersonaNatural(ruc: string): boolean {
  return /^\d{13}$/.test(ruc) && Number(ruc[2]) <= 5;
}

export function esRucDeSociedad(ruc: string): boolean {
  return /^\d{13}$/.test(ruc) && [6, 9].includes(Number(ruc[2]));
}
