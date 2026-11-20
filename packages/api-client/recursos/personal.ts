import type { $Fetch } from 'ofetch';
import type { PersonalDto } from '../tipos/organizacion';

export interface CrearPersonalDatos {
  areaId: string;
  nombre: string;
  tarifaHora: number | null;
  tieneAcceso: boolean;
  usuarioId: string | null;
  pin: string | null;
}

export function crearRecursoPersonal(fetch: $Fetch) {
  return {
    listar: (areaId: string) => fetch<PersonalDto[]>('/api/personal', { query: { areaId } }),

    crear: (datos: CrearPersonalDatos) => fetch<{ id: string }>('/api/personal', { method: 'POST', body: datos }),

    actualizar: (personalId: string, areaId: string, nombre: string, tarifaHora: number | null) =>
      fetch<void>(`/api/personal/${personalId}`, { method: 'PUT', body: { areaId, nombre, tarifaHora } }),

    desactivar: (personalId: string) => fetch<void>(`/api/personal/${personalId}`, { method: 'DELETE' }),
  };
}
