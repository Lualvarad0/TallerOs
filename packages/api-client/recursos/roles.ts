import type { $Fetch } from 'ofetch';
import type { PermisoDto, RolDto } from '../tipos/seguridad';

export function crearRecursoRoles(fetch: $Fetch) {
  return {
    listar: () => fetch<RolDto[]>('/api/roles'),

    listarPermisos: () => fetch<PermisoDto[]>('/api/roles/permisos'),

    crear: (nombre: string) => fetch<{ id: string }>('/api/roles', { method: 'POST', body: { nombre } }),

    // Reemplaza el conjunto completo de permisos del rol (no es un "agregar uno").
    asignarPermisos: (rolId: string, codigos: string[]) =>
      fetch<void>(`/api/roles/${rolId}/permisos`, { method: 'PUT', body: { codigos } }),
  };
}
