import { describe, expect, it } from 'vitest';
import { esVinValido } from './vin';

describe('esVinValido', () => {
  it('pasa con un VIN válido', () => {
    expect(esVinValido('1HGCM82633A004352')).toBe(true);
  });

  it('no pasa con el dígito verificador incorrecto', () => {
    expect(esVinValido('1HGCM82633A004353')).toBe(false);
  });

  it('no pasa con un carácter no permitido (I)', () => {
    expect(esVinValido('1HGCM82633AI04352')).toBe(false);
  });

  it.each(['', '1HGCM82633A00435', '1HGCM82633A0043522'])(
    'no pasa con longitud inválida (%s)',
    (vin) => {
      expect(esVinValido(vin)).toBe(false);
    },
  );
});
