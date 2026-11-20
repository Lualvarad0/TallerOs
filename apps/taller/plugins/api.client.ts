import { ofetch } from 'ofetch';
import { crearClienteApi } from '@talleros/api-client';

/**
 * Una sola instancia de fetch con el token y el manejo de 401 (equivale al authInterceptor de la
 * referencia Angular): nadie más agrega el header Authorization a mano.
 */
export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig();
  const sesion = useSesionStore();

  const fetch = ofetch.create({
    baseURL: config.public.apiBase,
    onRequest({ options }) {
      const token = sesion.tokenParaPeticiones;
      if (!token) return;
      const headers = new Headers(options.headers as HeadersInit);
      headers.set('Authorization', `Bearer ${token}`);
      options.headers = headers;
    },
    onResponseError({ response }) {
      if (response.status === 401) {
        sesion.cerrarSesion();
        navigateTo('/login');
      }
    },
  });

  return {
    provide: {
      api: crearClienteApi(fetch),
    },
  };
});
