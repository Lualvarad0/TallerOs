import type { $Fetch } from 'ofetch';
import type { EstadoOtDto } from '../tipos/ordenesTrabajo';

export function crearRecursoEstadosOt(fetch: $Fetch) {
  return {
    listar: () => fetch<EstadoOtDto[]>('/api/estados-ot'),
  };
}
