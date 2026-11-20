import { FetchError } from 'ofetch';
import type { ProblemaApi } from '@talleros/api-client';

/** Traduce un error de `fetch` (ProblemDetails de ManejoDeExcepcionesMiddleware) a un mensaje para
 * mostrar en el formulario. Equivale a mensajeDeError() de la referencia Angular. */
export function mensajeDeError(error: unknown): string {
  if (error instanceof FetchError) {
    if (!error.response) return 'No se pudo conectar con el servidor. Revisa tu conexión.';
    const cuerpo = error.data as ProblemaApi | undefined;
    if (cuerpo?.errores?.length) return cuerpo.errores.map((e) => e.mensaje).join(' ');
    if (cuerpo?.detail) return cuerpo.detail;
    if (error.response.status >= 500) return 'Ocurrió un error en el servidor. Inténtalo de nuevo.';
    return cuerpo?.title ?? 'No se pudo completar la operación.';
  }
  return 'Ocurrió un error inesperado.';
}
