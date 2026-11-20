import { describe, expect, it } from 'vitest';
import { esTelefonoValido } from './telefono';

describe('esTelefonoValido', () => {
  it.each(['0991234567', '+593991234567', '022345678', '+59322345678'])(
    'pasa con un número válido (%s)',
    (telefono) => {
      expect(esTelefonoValido(telefono)).toBe(true);
    },
  );

  it.each(['', '123', '099123456', '+15551234567'])(
    'no pasa con un número inválido (%s)',
    (telefono) => {
      expect(esTelefonoValido(telefono)).toBe(false);
    },
  );
});
