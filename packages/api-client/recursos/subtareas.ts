import type { $Fetch } from 'ofetch';
import type { EstadoSubtarea } from '../tipos/ordenesTrabajo';

export interface ActualizarSubtareaDatos {
  responsablePersonalId: string | null;
  estado: EstadoSubtarea;
}

export function crearRecursoSubtareas(fetch: $Fetch) {
  return {
    actualizar: (subtareaId: string, datos: ActualizarSubtareaDatos) =>
      fetch<void>(`/api/subtareas/${subtareaId}`, { method: 'PUT', body: datos }),
  };
}
