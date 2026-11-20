import type { $Fetch } from 'ofetch';
import type { IniciarSesionResultado, SeleccionarEmpresaResultado, VerificarPinResultado } from '../tipos/auth';

export function crearRecursoAuth(fetch: $Fetch) {
  return {
    iniciarSesion: (email: string, password: string) =>
      fetch<IniciarSesionResultado>('/api/auth/iniciar-sesion', { method: 'POST', body: { email, password } }),

    // Requiere el token pre-auth (ver useSesionStore): el usuarioId lo saca el backend del token,
    // no se manda aquí.
    seleccionarEmpresa: (empresaId: string) =>
      fetch<SeleccionarEmpresaResultado>('/api/auth/seleccionar-empresa', { method: 'POST', body: { empresaId } }),

    verificarPin: (tokenDispositivo: string, pin: string) =>
      fetch<VerificarPinResultado>('/api/auth/verificar-pin', { method: 'POST', body: { tokenDispositivo, pin } }),
  };
}
