# Estructura del frontend

Análisis de `C:\Projects\SAS\Gestionador de facturas\Web` (Angular 22 standalone + Tailwind) antes
de escribir el primer módulo de frontend de TallerOS, por lo que pide `docs/convenciones.md`. Es de **solo
lectura**: lo de abajo es lo que se traduce a Nuxt 3, no una copia literal (es Angular, nosotros
usamos Nuxt/Vue + Pinia + Zod/VeeValidate, que ese proyecto no usa).

## Qué tiene el proyecto de referencia

```text
Web/src/app/
  core/<dominio>/{models,services,guards,interceptors,constants,utils,validators}/
  features/<dominio>/<pagina>.ts + <pagina>.html (+ .spec.ts)
  layout/        shell, header, sidebar (componentes de la app ya autenticada)
  shared/{components,pipes}/   UI reutilizable sin lógica de dominio
  app.routes.ts, app.config.ts, app.ts
```

Patrones que importan (no la sintaxis de Angular en sí):

- **Separación por dominio, no por tipo de archivo**: todo lo de "auth" vive junto (modelo, servicio,
  guard), no en carpetas globales `services/`, `models/` sueltas.
- **`core` = estado y acceso a la API; `features` = pantallas**. Un componente de `features` llama
  a un servicio de `core`, nunca hace `fetch` directo.
- **Un servicio por dominio con estado reactivo** (`AuthService` con signals) en vez de un store
  global gigante: cada dominio administra su propio estado.
- **`index.ts` de barril** por carpeta de `core/<dominio>`, para importar `@core/auth` en vez de
  rutas largas.
- **Guards de ruta** (`authGuard`, `invitadoGuard`, `idValidoGuard`) separados del componente de
  página; la página no decide si puede entrar, la ruta sí.
- **Un interceptor HTTP** (`authInterceptor`) agrega el token a cada petición; un solo lugar, no en
  cada llamada a la API.
- **`Carga<T>` + `cargar()`**: wrapper genérico `{ cargando, datos, error }` para cualquier fetch,
  con una clave reactiva (ej. el mes elegido) que cancela la petición anterior si cambia antes de
  responder, y que limpia los datos si cambia el usuario/cuenta activa (para no mostrar un parpadeo
  de datos de otra cuenta). Esto es exactamente el problema de aislamiento que tenemos nosotros
  entre empresas: hay que resolverlo igual en el cliente, no solo en el servidor.
- **`mensajeDeError()`**: traduce la respuesta de error de la API (ProblemDetails) a un string legible
  para mostrar en el formulario, en un solo lugar.
- **Nombres de archivo y componentes en español**, igual que el dominio del backend
  (`acceso.ts`, `formulario-acceso.ts`, `recuperar-clave.ts`).
- **Sin plantilla inline para páginas**: `.ts` + `.html` separados cuando el componente tiene
  markup real; algo puramente lógico puede no tener su propio archivo visual.

## Traducción a Nuxt 3 (lo que usa TallerOS)

| Referencia (Angular) | TallerOS (Nuxt 3) |
|---|---|
| `core/<dominio>/services/*.service.ts` (estado con signals) | `stores/<dominio>.ts` (Pinia) |
| `core/<dominio>/models/*.model.ts` | `types/<dominio>.ts` (interfaces simples, sin lógica) |
| `core/<dominio>/guards/*.guards.ts` | `middleware/<nombre>.ts` (`definePageMeta({ middleware })`) |
| `core/auth/interceptors/auth.interceptor.ts` | `plugins/api.ts` (instancia de `ofetch` con token + 401) |
| `core/http/utils/carga.ts` + `carga.model.ts` | `composables/useCarga.ts` (mismo contrato `{cargando,datos,error}`, con `watch` en vez de RxJS) |
| `core/http/utils/errores.ts` | `composables/useMensajeError.ts` (adaptado a nuestro `ProblemDetails`: `título`, `detail`, `errores: [{campo, mensaje}]`, no el `errors: Record<string,string[]>` de ASP.NET Identity por defecto) |
| `features/<dominio>/<pagina>.ts/.html` | `pages/<ruta>.vue` (routing por archivo de Nuxt; no hace falta `app.routes.ts` a mano) |
| `layout/shell.ts` + `sidebar.ts` | `layouts/default.vue` + `components/layout/TheSidebar.vue` |
| `shared/components/*` | `components/ui/*` (empiezan dentro de `apps/taller`; si `mi-garage` llega a necesitar el mismo botón/tarjeta, recién ahí se mueven a `packages/ui` — no antes) |
| `core/<dominio>/index.ts` (barril) | No hace falta: Nuxt auto-importa `stores/`, `composables/` y `components/` por convención |

Diferencias deliberadas (no son un olvido):

- Formularios: Zod (esquema) + VeeValidate (binding), no `ReactiveFormsModule`.
- Estado: Pinia, no servicios inyectables con signals.
- HTTP: `ofetch`/`$fetch` de Nuxt, no `HttpClient`.
- `packages/ui` guarda por ahora solo los **tokens** (`tokens.css`, preset de Tailwind). Los
  componentes base (`Boton`, `Tarjeta`, `CampoTexto`, `Badge`) se quedan en
  `apps/taller/components/ui/` hasta que una segunda app los necesite — evita el costo de empaquetar
  un paquete de componentes Vue que hoy tiene un solo consumidor.
- `packages/api-client` sí es un paquete real desde ahora (tipos + funciones `ofetch`, sin
  dependencia de Vue): lo consumen `apps/taller` y, más adelante, `apps/mi-garage` por igual.
  docs/convenciones.md lo describe como "generado desde OpenAPI"; todavía no hay pipeline de generación desde
  Swagger/OpenAPI montado, así que por ahora se escribe a mano siguiendo los DTOs reales del backend
  (`backend/src/TallerOS.Application/**/*.cs`). Generarlo automáticamente es una mejora de
  infraestructura para más adelante, no bloquea este módulo.

## Estructura de `apps/taller` para el Módulo 1

```text
apps/taller/
  app.vue
  nuxt.config.ts
  assets/css/tailwind.css      @import de packages/ui/tokens.css + directivas Tailwind
  middleware/
    autenticado.global.ts      exige token de sesión; redirige a /login (equivale a authGuard)
    invitado.ts                /login: si ya hay sesión, redirige adentro (equivale a invitadoGuard)
  plugins/
    api.client.ts              instancia de $fetch con baseURL, Authorization y manejo de 401
  stores/
    sesion.ts                  login, selección de empresa, logout, token/empresa actuales
    sucursales.ts
    areas.ts
    personal.ts
    roles.ts
    dispositivosTablet.ts
  types/
    auth.ts  organizacion.ts  seguridad.ts
  composables/
    useCarga.ts
    useMensajeError.ts
  layouts/
    default.vue                shell con TheSidebar, para toda ruta autenticada
    vacio.vue                  sin shell, para /login
  components/
    layout/TheSidebar.vue
    ui/{Boton,Tarjeta,CampoTexto,Badge}.vue
    configuracion/{PanelSucursales,PanelPersonal,PanelRoles,PanelDispositivos}.vue
  pages/
    login.vue
    configuracion/index.vue
```

Páginas/secciones de `docs/diseno/Configuracion.dc.html` que **no** se implementan en este módulo
porque dependen de datos de módulos futuros (no inventar la pantalla sin los datos reales):
"Tipos de trabajo" (catálogo de Módulo 4) y "Alertas y mantenimiento" (Módulo 8). Quedan como
pestañas deshabilitadas con la etiqueta "Próximamente", mismo patrón que la referencia
(`features/proximamente`). `Super-Admin.dc.html` tampoco se construye todavía: necesita listar
todas las empresas (no existe esa query en el backend) y KPIs de uso/facturación que no existen
hasta los módulos de OT y cobros.
