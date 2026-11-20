import { describe, expect, it } from 'vitest';
import { esRucDePersonaNatural, esRucDeSociedad, esRucValido } from './ruc';

describe('esRucValido', () => {
  it('pasa con persona natural válido', () => {
    expect(esRucValido('1710034065001')).toBe(true);
  });

  it('no pasa con establecimiento 000', () => {
    expect(esRucValido('1710034065000')).toBe(false);
  });

  it('pasa con sociedad privada válida', () => {
    expect(esRucValido('1791234561001')).toBe(true);
  });

  it('no pasa con sociedad privada con verificador incorrecto', () => {
    expect(esRucValido('1791234562001')).toBe(false);
  });

  it('pasa con entidad pública válida', () => {
    expect(esRucValido('1761234510001')).toBe(true);
  });

  it('no pasa con entidad pública con verificador incorrecto', () => {
    expect(esRucValido('1761234520001')).toBe(false);
  });

  it('no pasa con entidad pública que no termina en 0001', () => {
    expect(esRucValido('1761234510002')).toBe(false);
  });

  it('no pasa con tercer dígito sin estructura conocida', () => {
    expect(esRucValido('1781234561001')).toBe(false);
  });

  it.each(['', '171003406500', '171003406500A'])('no pasa con formato inválido (%s)', (ruc) => {
    expect(esRucValido(ruc)).toBe(false);
  });
});

describe('esRucDePersonaNatural', () => {
  it('pasa con tercer dígito 0-5', () => {
    expect(esRucDePersonaNatural('1710034065001')).toBe(true);
  });

  it('no pasa con tercer dígito de sociedad (9)', () => {
    expect(esRucDePersonaNatural('1791234561001')).toBe(false);
  });
});

describe('esRucDeSociedad', () => {
  it('pasa con sociedad privada (9)', () => {
    expect(esRucDeSociedad('1791234561001')).toBe(true);
  });

  it('pasa con entidad pública (6)', () => {
    expect(esRucDeSociedad('1761234510001')).toBe(true);
  });

  it('no pasa con tercer dígito de persona natural', () => {
    expect(esRucDeSociedad('1710034065001')).toBe(false);
  });
});
