/** Restaura la sesión (sessionStorage) antes de que corra el middleware de rutas: en app.vue
 * sería demasiado tarde, porque el router ya resuelve la primera navegación mientras ese
 * componente se monta, y autenticado.global.ts vería estaAutenticado=false todavía. */
export default defineNuxtPlugin(() => {
  useSesionStore().restaurar();
  useTemaStore().aplicar();
});
