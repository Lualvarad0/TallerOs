import type { RolClienteVehiculo } from './clientes';

export interface ClienteVinculadoDto {
  clienteId: string;
  nombre: string;
  rol: RolClienteVehiculo;
}

/** Placa es la activa, no el historial completo: ver VehiculoDetalleDto (backend). */
export interface VehiculoDetalleDto {
  id: string;
  vin: string | null;
  marcaId: string;
  marca: string;
  modeloId: string;
  modelo: string;
  anio: number;
  motor: string | null;
  placa: string | null;
  clientes: ClienteVinculadoDto[];
}

/** advertenciaVin no es nulo cuando se dio un VIN con dígito verificador inválido: el registro se
 * completó igual (ver docs/especificacion.md, cambio de contrato del 2026-11-02). */
export interface RegistrarVehiculoResultado {
  vehiculoId: string;
  clienteVehiculoId: string;
  esVehiculoNuevo: boolean;
  advertenciaVin: string | null;
}
