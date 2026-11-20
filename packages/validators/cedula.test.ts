import { describe, expect, it } from 'vitest';
import { esCedulaValida } from './cedula';

describe('esCedulaValida', () => {
  it('pasa con una cédula válida', () => {
    expect(esCedulaValida('1710034065')).toBe(true);
  });

  it('no pasa con el dígito verificador incorrecto', () => {
    expect(esCedulaValida('1710034060')).toBe(false);
  });

  it('no pasa con la provincia fuera de rango', () => {
    expect(esCedulaValida('9910034068')).toBe(false);
  });

  it('no pasa con el tercer dígito de RUC, no de cédula', () => {
    // Checksum correcto (verificador 4), pero el tercer dígito (6) es de entidad pública.
    expect(esCedulaValida('1760034064')).toBe(false);
  });

  it.each(['', '12345', '17100340A5'])('no pasa con formato inválido (%s)', (cedula) => {
    expect(esCedulaValida(cedula)).toBe(false);
  });
});
