/** Envoltorio de estado de carga para una llamada async, traducido del `Carga<T>`/`cargar()` de la
 * referencia Angular (RxJS) a reactividad de Vue. Cada llamada a `ejecutar` descarta el resultado
 * de una llamada anterior que aún esté en curso, para no pisar datos nuevos con una respuesta vieja. */
export interface Carga<T> {
  cargando: Ref<boolean>;
  datos: Ref<T | null>;
  error: Ref<string | null>;
  ejecutar: (fn: () => Promise<T>) => Promise<T | null>;
  limpiar: () => void;
}

export function useCarga<T>(): Carga<T> {
  const cargando = ref(false);
  const datos = ref<T | null>(null) as Ref<T | null>;
  const error = ref<string | null>(null);
  let idPeticionActual = 0;

  async function ejecutar(fn: () => Promise<T>): Promise<T | null> {
    const idPeticion = ++idPeticionActual;
    cargando.value = true;
    error.value = null;
    try {
      const resultado = await fn();
      if (idPeticion !== idPeticionActual) return null;
      datos.value = resultado;
      return resultado;
    } catch (err) {
      if (idPeticion !== idPeticionActual) return null;
      error.value = mensajeDeError(err);
      return null;
    } finally {
      if (idPeticion === idPeticionActual) cargando.value = false;
    }
  }

  function limpiar() {
    idPeticionActual++;
    cargando.value = false;
    datos.value = null;
    error.value = null;
  }

  return { cargando, datos, error, ejecutar, limpiar };
}
