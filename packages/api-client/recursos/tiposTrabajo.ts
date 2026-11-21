import type { $Fetch } from 'ofetch';
import type { TipoTrabajoDto } from '../tipos/ordenesTrabajo';

export interface CrearTipoTrabajoDatos {
  nombre: string;
  tiempoEstandarMin: number;
  intervaloKm: number | null;
  intervaloMeses: number | null;
}

export function crearRecursoTiposTrabajo(fetch: $Fetch) {
  return {
    listar: () => fetch<TipoTrabajoDto[]>('/api/tipos-trabajo'),

    crear: (datos: CrearTipoTrabajoDatos) => fetch<{ id: string }>('/api/tipos-trabajo', { method: 'POST', body: datos }),
  };
}
