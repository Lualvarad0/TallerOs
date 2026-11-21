export type PrioridadOt = 'Normal' | 'Alta' | 'Urgente';
export type TipoDanio = 'Golpe' | 'Rayon' | 'Abolladura' | 'Vidrio';
export type EstadoSubtarea = 'Pendiente' | 'EnCurso' | 'Hecha';

/** Token del design system para pintar la columna/borde de este estado (ver docs/diseno/README.md
 * § Mapeo de estados de OT → color): "sf3", "blue", "vio" o "grn", nunca el nombre completo de la
 * variable CSS. */
export interface TableroOtDto {
  otId: string;
  numero: number;
  placa: string | null;
  clienteNombre: string;
  prioridad: PrioridadOt;
  estadoId: string;
  estadoNombre: string;
  colorToken: string;
  ingresoEn: string;
}

export interface DanioPrevioDto {
  id: string;
  zona: string;
  tipo: TipoDanio;
  descripcion: string | null;
  fotoUrl: string | null;
}

export interface RecepcionDto {
  combustible: number;
  objetos: string | null;
  observaciones: string | null;
  firmaUrl: string | null;
  daniosPrevios: DanioPrevioDto[];
}

export interface SubtareaDto {
  id: string;
  tipoTrabajoId: string;
  tipoTrabajoNombre: string;
  responsablePersonalId: string | null;
  estado: EstadoSubtarea;
  tiempoEstimadoMin: number;
  orden: number;
}

export interface HistorialEstadoOtDto {
  deEstadoId: string | null;
  aEstadoId: string;
  aEstadoNombre: string;
  usuarioId: string;
  fecha: string;
}

export interface OrdenTrabajoDetalleDto {
  id: string;
  numero: number;
  sucursalId: string;
  vehiculoId: string;
  clienteId: string;
  clienteNombre: string;
  estadoId: string;
  estadoNombre: string;
  prioridad: PrioridadOt;
  kmIngreso: number;
  asesorPersonalId: string | null;
  ingresoEn: string;
  entregadoEn: string | null;
  recepcion: RecepcionDto | null;
  subtareas: SubtareaDto[];
  historial: HistorialEstadoOtDto[];
}

export interface EstadoOtDto {
  id: string;
  nombre: string;
  orden: number;
  esFinal: boolean;
  colorToken: string;
}

export interface TipoTrabajoDto {
  id: string;
  nombre: string;
  tiempoEstandarMin: number;
  intervaloKm: number | null;
  intervaloMeses: number | null;
}
