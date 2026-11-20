import type { $Fetch } from 'ofetch';
import type { UsuarioRolDto } from '../tipos/seguridad';

export function crearRecursoUsuarios(fetch: $Fetch) {
  return {
    listar: () => fetch<UsuarioRolDto[]>('/api/usuarios'),

    invitar: (email: string, password: string | null, rolId: string, sucursalId: string | null) =>
      fetch<{ id: string }>('/api/usuarios', { method: 'POST', body: { email, password, rolId, sucursalId } }),

    quitarRol: (usuarioRolId: string) => fetch<void>(`/api/usuarios/roles/${usuarioRolId}`, { method: 'DELETE' }),
  };
}
