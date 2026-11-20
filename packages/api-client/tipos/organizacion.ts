export type EstadoEmpresa = 'Activa' | 'Suspendida' | 'Cancelada';

export interface EmpresaDto {
  id: string;
  nombre: string;
  ruc: string;
  estado: EstadoEmpresa;
  creadoEn: string;
}

export interface SucursalDto {
  id: string;
  nombre: string;
  direccion: string | null;
}

export interface AreaDto {
  id: string;
  sucursalId: string;
  nombre: string;
}

export interface PersonalDto {
  id: string;
  areaId: string;
  nombre: string;
  tarifaHora: number | null;
  tieneAcceso: boolean;
}
