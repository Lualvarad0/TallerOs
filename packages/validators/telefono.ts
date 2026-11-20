import { parsePhoneNumberFromString } from 'libphonenumber-js';

// Teléfono ecuatoriano (celular o convencional), vía libphonenumber-js: acepta tanto formato local
// ("0991234567") como E.164 ("+593991234567").
export function esTelefonoValido(telefono: string): boolean {
  const numero = parsePhoneNumberFromString(telefono, 'EC');
  return numero?.isValid() ?? false;
}
