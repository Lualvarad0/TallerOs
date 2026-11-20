import type { $Fetch } from 'ofetch';
import type { SucursalDto } from '../tipos/organizacion';

export function crearRecursoSucursales(fetch: $Fetch) {
  return {
    listar: () => fetch<SucursalDto[]>('/api/sucursales'),

    crear: (nombre: string, direccion: string | null) =>
      fetch<{ id: string }>('/api/sucursales', { method: 'POST', body: { nombre, direccion } }),

    actualizar: (sucursalId: string, nombre: string, direccion: string | null) =>
      fetch<void>(`/api/sucursales/${sucursalId}`, { method: 'PUT', body: { nombre, direccion } }),

    desactivar: (sucursalId: string) => fetch<void>(`/api/sucursales/${sucursalId}`, { method: 'DELETE' }),
  };
}
