import type { $Fetch } from 'ofetch';
import type { EmpresaDto } from '../tipos/organizacion';

export interface CrearEmpresaDatos {
  nombre: string;
  ruc: string;
  planId: string;
  emailDueño: string;
  passwordDueño: string;
}

export interface CrearEmpresaResultado {
  empresaId: string;
  usuarioId: string;
}

export function crearRecursoEmpresas(fetch: $Fetch) {
  return {
    // Alta pública (sin sesión): registro de un taller nuevo.
    crear: (datos: CrearEmpresaDatos) => fetch<CrearEmpresaResultado>('/api/empresas', { method: 'POST', body: datos }),

    obtener: (empresaId: string) => fetch<EmpresaDto>(`/api/empresas/${empresaId}`),
  };
}
