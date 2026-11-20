import { describe, expect, it } from 'vitest';
import { placaTieneFormatoReconocido } from './placa';

describe('placaTieneFormatoReconocido', () => {
  it.each(['ABC-1234', 'AB-1234', 'abc-1234'])('pasa con formato reconocido (%s)', (placa) => {
    expect(placaTieneFormatoReconocido(placa)).toBe(true);
  });

  it.each(['', 'ABC1234', 'ABCD-1234', 'AB-12'])(
    'no pasa con formato no reconocido (%s)',
    (placa) => {
      expect(placaTieneFormatoReconocido(placa)).toBe(false);
    },
  );
});
