const RUTAS_PUBLICAS = ['/login'];

export default defineNuxtRouteMiddleware((to) => {
  if (RUTAS_PUBLICAS.includes(to.path)) return;

  const sesion = useSesionStore();
  if (!sesion.estaAutenticado) return navigateTo('/login');
});
