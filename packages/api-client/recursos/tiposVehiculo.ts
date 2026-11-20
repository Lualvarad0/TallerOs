import type { $Fetch } from 'ofetch';
import type { TipoVehiculoDto } from '../tipos/catalogos';

export function crearRecursoTiposVehiculo(fetch: $Fetch) {
  return {
    listar: () => fetch<TipoVehiculoDto[]>('/api/tipos-vehiculo'),
  };
}
