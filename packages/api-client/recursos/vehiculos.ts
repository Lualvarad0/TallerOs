import type { $Fetch } from 'ofetch';
import type { RolClienteVehiculo } from '../tipos/clientes';
import type { RegistrarVehiculoResultado, VehiculoDetalleDto } from '../tipos/vehiculos';

export interface RegistrarVehiculoDatos {
  clienteId: string;
  vin: string | null;
  placa: string | null;
  marcaId: string;
  modeloId: string;
  anio: number;
  motor: string | null;
  rol: RolClienteVehiculo;
}

export function crearRecursoVehiculos(fetch: $Fetch) {
  return {
    obtener: (vehiculoId: string) => fetch<VehiculoDetalleDto>(`/api/vehiculos/${vehiculoId}`),

    registrar: (datos: RegistrarVehiculoDatos) =>
      fetch<RegistrarVehiculoResultado>('/api/vehiculos', { method: 'POST', body: datos }),
  };
}
