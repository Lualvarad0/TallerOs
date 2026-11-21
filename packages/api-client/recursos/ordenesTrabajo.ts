import type { $Fetch } from 'ofetch';
import type { OrdenTrabajoDetalleDto, PrioridadOt, TableroOtDto } from '../tipos/ordenesTrabajo';

export interface FiltrosTablero {
  sucursalId?: string;
  asesorPersonalId?: string;
  prioridad?: PrioridadOt;
  estadoId?: string;
}

export interface CrearSubtareaDatos {
  tipoTrabajoId: string;
  responsablePersonalId: string | null;
  tiempoEstimadoMin: number;
  orden: number;
}

export function crearRecursoOrdenesTrabajo(fetch: $Fetch) {
  return {
    listarTablero: (filtros?: FiltrosTablero) => fetch<TableroOtDto[]>('/api/ordenes-trabajo', { query: filtros }),

    obtener: (otId: string) => fetch<OrdenTrabajoDetalleDto>(`/api/ordenes-trabajo/${otId}`),

    cambiarEstado: (otId: string, nuevoEstadoId: string) =>
      fetch<void>(`/api/ordenes-trabajo/${otId}/estado`, { method: 'PUT', body: { nuevoEstadoId } }),

    crearSubtarea: (otId: string, datos: CrearSubtareaDatos) =>
      fetch<{ id: string }>(`/api/ordenes-trabajo/${otId}/subtareas`, { method: 'POST', body: datos }),
  };
}
