export type TipoCliente = 'PersonaNatural' | 'Empresa';
export type CanalPreferido = 'Correo' | 'WhatsApp';
export type RolClienteVehiculo = 'Propietario' | 'ConductorHabitual' | 'ResponsableFlota';

export interface ClienteDto {
  id: string;
  nombre: string;
  tipo: TipoCliente;
  identificacion: string;
  telefono: string | null;
  email: string | null;
  canalPreferido: CanalPreferido;
}

export interface VehiculoResumenDto {
  vehiculoId: string;
  vin: string | null;
  placa: string | null;
  marcaId: string;
  marca: string;
  modeloId: string;
  modelo: string;
  anio: number;
  rol: RolClienteVehiculo;
}

export interface ClienteDetalleDto extends ClienteDto {
  vehiculos: VehiculoResumenDto[];
}
