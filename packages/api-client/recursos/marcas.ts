import type { $Fetch } from 'ofetch';
import type { MarcaDto, ModeloDto } from '../tipos/catalogos';

export function crearRecursoMarcas(fetch: $Fetch) {
  return {
    listar: (tipoVehiculoId?: number) => fetch<MarcaDto[]>('/api/marcas', { query: { tipoVehiculoId } }),

    listarModelos: (marcaId: string) => fetch<ModeloDto[]>(`/api/marcas/${marcaId}/modelos`),

    // Solo Admin/Supervisor (permiso catalogo_vehiculo.gestionar): la Api responde 403 si no.
    crear: (tipoVehiculoId: number, nombre: string) =>
      fetch<{ id: string }>('/api/marcas', { method: 'POST', body: { tipoVehiculoId, nombre } }),
  };
}
