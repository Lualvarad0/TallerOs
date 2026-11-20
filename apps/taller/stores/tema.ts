import { defineStore } from 'pinia';

const CLAVE_TEMA = 'talleros-theme';
type Tema = 'claro' | 'oscuro';

function leerTemaGuardado(): Tema | null {
  try {
    const guardado = localStorage.getItem(CLAVE_TEMA);
    return guardado === 'claro' || guardado === 'oscuro' ? guardado : null;
  } catch {
    return null;
  }
}

export const useTemaStore = defineStore('tema', () => {
  const tema = ref<Tema>(leerTemaGuardado() ?? 'claro');

  function aplicar() {
    document.documentElement.setAttribute('data-theme', tema.value === 'oscuro' ? 'dark' : 'light');
  }

  function alternar() {
    tema.value = tema.value === 'claro' ? 'oscuro' : 'claro';
    try {
      localStorage.setItem(CLAVE_TEMA, tema.value);
    } catch {
      /* el tema sigue aplicado en memoria */
    }
    aplicar();
  }

  return { tema, aplicar, alternar };
});
