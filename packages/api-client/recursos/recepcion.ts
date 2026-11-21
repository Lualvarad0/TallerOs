import type { $Fetch } from 'ofetch';
import type { PrioridadOt, TipoDanio } from '../tipos/ordenesTrabajo';

export interface DanioPrevioInput {
  zona: string;
  tipo: TipoDanio;
  descripcion: string | null;
  fotoUrl: string | null;
}

export interface RegistrarRecepcionDatos {
  sucursalId: string;
  vehiculoId: string;
  clienteId: string;
  prioridad: PrioridadOt;
  kmIngreso: number;
  asesorPersonalId: string | null;
  combustible: number;
  objetos: string | null;
  observaciones: string | null;
  daniosPrevios: DanioPrevioInput[];
}

export interface RegistrarRecepcionResultado {
  otId: string;
  numero: number;
  estadoId: string;
}

export function crearRecursoRecepcion(fetch: $Fetch) {
  return {
    registrar: (datos: RegistrarRecepcionDatos) =>
      fetch<RegistrarRecepcionResultado>('/api/recepcion', { method: 'POST', body: datos }),
  };
}
