/** Para /login: si ya hay sesión activa, no tiene sentido volver a mostrar el formulario. */
export default defineNuxtRouteMiddleware(() => {
  const sesion = useSesionStore();
  if (sesion.estaAutenticado) return navigateTo('/');
});
