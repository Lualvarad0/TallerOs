import type { $Fetch } from 'ofetch';
import type { CanalPreferido, ClienteDetalleDto, ClienteDto, TipoCliente } from '../tipos/clientes';

export interface CrearClienteDatos {
  nombre: string;
  tipo: TipoCliente;
  identificacion: string;
  telefono: string | null;
  email: string | null;
  canalPreferido: CanalPreferido;
}

// Tipo e Identificacion no se editan aquí: ver ActualizarClienteCommand (backend).
export interface ActualizarClienteDatos {
  nombre: string;
  telefono: string | null;
  email: string | null;
  canalPreferido: CanalPreferido;
}

export function crearRecursoClientes(fetch: $Fetch) {
  return {
    listar: (busqueda?: string) => fetch<ClienteDto[]>('/api/clientes', { query: { busqueda } }),

    obtener: (clienteId: string) => fetch<ClienteDetalleDto>(`/api/clientes/${clienteId}`),

    crear: (datos: CrearClienteDatos) => fetch<{ id: string }>('/api/clientes', { method: 'POST', body: datos }),

    actualizar: (clienteId: string, datos: ActualizarClienteDatos) =>
      fetch<void>(`/api/clientes/${clienteId}`, { method: 'PUT', body: datos }),

    desactivar: (clienteId: string) => fetch<void>(`/api/clientes/${clienteId}`, { method: 'DELETE' }),
  };
}
