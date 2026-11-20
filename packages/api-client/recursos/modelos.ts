import type { $Fetch } from 'ofetch';

export function crearRecursoModelos(fetch: $Fetch) {
  return {
    // Mismo permiso que marcas.crear: ver el comentario ahí.
    crear: (marcaId: string, nombre: string) =>
      fetch<{ id: string }>('/api/modelos', { method: 'POST', body: { marcaId, nombre } }),
  };
}
