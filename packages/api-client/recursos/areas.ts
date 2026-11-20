import type { $Fetch } from 'ofetch';
import type { AreaDto } from '../tipos/organizacion';

export function crearRecursoAreas(fetch: $Fetch) {
  return {
    listar: (sucursalId: string) => fetch<AreaDto[]>('/api/areas', { query: { sucursalId } }),

    crear: (sucursalId: string, nombre: string) =>
      fetch<{ id: string }>('/api/areas', { method: 'POST', body: { sucursalId, nombre } }),

    actualizar: (areaId: string, nombre: string) =>
      fetch<void>(`/api/areas/${areaId}`, { method: 'PUT', body: { nombre } }),

    desactivar: (areaId: string) => fetch<void>(`/api/areas/${areaId}`, { method: 'DELETE' }),
  };
}
