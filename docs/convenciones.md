# TallerOS — Convenciones y guía de arquitectura

SaaS multi-empresa para talleres mecánicos (autos y motos): órdenes de trabajo tipo Jira, tiempo por mecánico, jornada laboral, inventario con margen, hallazgos y avisos a clientes. Web, escritorio (Tauri) y móvil (Capacitor) desde un solo código.

## Ubicación

- Este proyecto vive en `C:\Projects\SAS\TallerOS`. No crear ni modificar nada fuera de esa carpeta, salvo la excepción de abajo.
- Excepción: los wrappers nativos de Capacitor (carpetas `android/`, `ios/` y `capacitor.config.ts`) viven en `C:\Projects\Mobile\TallerOS` (ver `C:\Projects\Mobile\TallerOS\README.md`), separados del monorepo principal. El código Nuxt sigue siendo único en `apps/taller` y `apps/mi-garage`; el wrapper nativo solo referencia (`webDir`) el build de esas apps. No crear este wrapper hasta llegar al paso de empaquetado móvil de cada módulo de frontend.
- Referencia de estructura del frontend: `C:\Projects\SAS\Gestionador de facturas` (solo el front). Es de **solo lectura**: se analiza para copiar su organización de carpetas, nombres y patrones; nunca se edita.

## Documentación (leer antes de cualquier tarea)

- `docs/especificacion.md` — especificación completa: stack, modelo de datos, reglas de negocio, fases. Es la fuente de verdad.
- `docs/diseno/` — pantallas exportadas del diseño. Implementar lo que se ve ahí, no inventar layouts.
- `docs/diseno/tokens.md` — colores, tipografía y espaciado del design system.

Si algo del pedido contradice la especificación, detente y pregunta antes de escribir código.

## Forma de trabajar: un módulo a la vez

Para no gastar tokens ni contexto, se construye **un módulo por sesión**, en este orden:

1. Organización y seguridad (empresa, sucursal, área, usuario, personal, roles, permisos, autenticación)
2. Catálogos de vehículos (tipo de vehículo, marca, modelo) y validadores
3. Clientes y vehículos (cliente, vehículo, placas, relación cliente-vehículo)
4. Órdenes de trabajo y Kanban (OT, estados, subtareas, recepción)
5. Tiempos y jornada (registro de tiempo, modos, marcación, horas extra)
6. Hallazgos y cotizaciones
7. Inventario, compras y margen
8. Avisos al cliente (correo, WhatsApp, recordatorios)

Cada módulo sigue siempre estos pasos, deteniéndose al final de cada uno para que yo revise:

1. **Plan**: entidades, campos con su tipo, índices, endpoints y pantallas. Esperar aprobación.
2. **Base de datos**: configuración EF Core, migración, índices, RLS y datos semilla del módulo.
3. **Backend**: dominio, casos de uso, validaciones, endpoints y pruebas (incluida la de aislamiento entre empresas).
4. **Frontend**: servicios, store, composables, componentes y páginas del módulo.

No leer ni tocar módulos que no son parte de la tarea actual. No adelantar trabajo de otro módulo.

## Stack

- Frontend: Nuxt 3 en modo SPA (`ssr: false`), TypeScript estricto, Tailwind CSS, Pinia, Zod + VeeValidate.
- Backend: .NET 10, ASP.NET Core Web API, Clean Architecture (Domain / Application / Infrastructure / Api), CQRS, FluentValidation, EF Core.
- Base de datos: PostgreSQL en Docker.
- Caché y tiempo real: Redis. SignalR para Kanban y alertas. Hangfire para tareas programadas. Archivos en almacenamiento compatible S3 (Garage en local; MinIO quedó descartado porque su imagen oficial de Docker pasó a requerir login/suscripción en 2024).

## Estructura del proyecto

```text
TallerOS/
  CLAUDE.md
  docs/
  apps/
    taller/              Nuxt (misma organización que el front de "Gestionador de facturas")
    mi-garage/           Nuxt, app del cliente (más adelante)
  packages/
    ui/                  design system: tokens y componentes base
    validators/          cédula, RUC, teléfono, VIN, placa (TypeScript)
    api-client/          cliente tipado generado desde OpenAPI
  backend/
    src/
      TallerOS.Domain/          entidades, value objects, eventos de dominio, reglas, validadores (C#)
      TallerOS.Application/     casos de uso por módulo (Commands / Queries / DTOs / Validators / Behaviors)
      TallerOS.Infrastructure/  EF Core, configuraciones, migraciones, Redis, S3, correo, WhatsApp, Hangfire
      TallerOS.Api/             endpoints por módulo, middlewares, autenticación, rate limiting
    tests/
      TallerOS.Domain.Tests/
      TallerOS.Application.Tests/
      TallerOS.Api.Tests/       pruebas de integración y de aislamiento entre empresas
  infra/
    docker-compose.yml
    postgres/init/       scripts de roles, extensiones y RLS
    seed/                datos semilla (catálogos, permisos, empresa demo)
```

Wrapper nativo móvil (fuera de este repo, ver excepción en "Ubicación"):

```text
C:\Projects\Mobile\TallerOS/
  taller/        capacitor.config.ts + android/ + ios/ (apunta al build de apps/taller)
  mi-garage/     capacitor.config.ts + android/ + ios/ (apunta al build de apps/mi-garage)
```

- Backend organizado **por módulo dentro de cada capa** (`Application/OrdenesTrabajo/Commands/CrearOrdenTrabajo/...`), no por tipo de archivo suelto.
- Frontend: antes del primer módulo de front, analizar `C:\Projects\SAS\Gestionador de facturas` y escribir en `docs/estructura-front.md` qué carpetas, convenciones y patrones se van a replicar. Luego seguir ese documento.

## Base de datos (Docker + PostgreSQL)

### Docker
- `infra/docker-compose.yml` levanta: PostgreSQL, Redis, MinIO y la API. Volúmenes con nombre para que los datos persistan.
- Imagen de PostgreSQL con scripts en `postgres/init/` que crean: extensiones (`citext`, `pg_trgm`), el rol dueño de las migraciones y un **rol de aplicación sin privilegios de superusuario ni `BYPASSRLS`**.
- Semilla reproducible: tipos de vehículo, marcas y modelos, catálogo de permisos, roles base y una empresa demo con datos de prueba. Comando único para recrear todo desde cero.
- Contraseñas y cadenas de conexión solo en `.env` (con `.env.example` en el repo); nunca en el código ni en git.

### Tipos de dato (el más pequeño que sea correcto)
- Ids: `uuid` (v7, ordenables). Catálogos fijos pequeños: `smallint`.
- Texto: `varchar(n)` con límite real (placa 10, VIN 17, cédula 13, teléfono E.164 16). Correo: `citext`. Texto libre largo: `text`.
- Dinero: `numeric(12,2)`. Cantidades fraccionarias (litros, metros): `numeric(10,3)`.
- Fechas: `timestamptz` en UTC; solo fecha: `date`. Duraciones: minutos en `integer`.
- Estados y tipos: `smallint` con tabla de catálogo o enum mapeado, más un `CHECK` que limite los valores.
- `NOT NULL` por defecto; nulo solo cuando el dato realmente puede faltar.
- Restricciones en la base, no solo en el código: `CHECK` (cantidades ≥ 0, fin > inicio), `UNIQUE` por empresa (`empresa_id`, `numero_ot`), llaves foráneas compuestas.
- JSON (`jsonb`) solo para auditoría y configuraciones flexibles, nunca para datos que se filtran.

### Índices
- En tablas de empresa, `empresa_id` va **primero** en los índices compuestos.
- Índice en toda llave foránea.
- Índices parciales para lo activo: `WHERE eliminado_en IS NULL`, `WHERE estado = abierta`.
- Índices de cobertura (`INCLUDE`) para las consultas más frecuentes: Kanban por empresa/sucursal/estado, búsqueda por placa, agenda del mecánico.
- `pg_trgm` para búsqueda parcial por placa, nombre de cliente y nombre de producto.
- Cada índice va justificado en el plan del módulo; no crear índices "por si acaso".

## Optimización de consultas

### Backend
- Lecturas con `AsNoTracking()` y proyección a DTO con `Select`; nunca devolver entidades ni traer columnas que no se usan.
- Sin consultas N+1: `Include` o proyección explícita. Consultas grandes con `AsSplitQuery()` cuando corresponda.
- Paginación por cursor (keyset) en listas grandes; paginación por página solo en tablas administrativas pequeñas.
- Todo endpoint de listado tiene filtro, orden y tamaño máximo de página.
- Consultas críticas (Kanban, búsqueda por placa, reportes) se revisan con `EXPLAIN ANALYZE` y el resultado se anota en el plan del módulo.
- Reportes pesados: vistas materializadas o consultas SQL específicas, no LINQ encadenado.
- Operaciones masivas con `ExecuteUpdate` / `ExecuteDelete`.

### Redis
- Caché de lectura frecuente y poco cambiante: catálogos (marcas, modelos, tipos), permisos efectivos del usuario, configuración de la empresa.
- Toda clave lleva prefijo de empresa: `t:{empresaId}:...`. Nunca compartir caché entre empresas.
- Invalidación explícita al modificar el dato (evento de dominio → borrar clave). TTL siempre definido.
- También para: backplane de SignalR, límite de peticiones (rate limiting), bloqueo distribuido para evitar dos cronómetros abiertos del mismo mecánico.
- Si Redis no responde, la aplicación sigue funcionando contra la base de datos.

### Frontend
- Rutas y módulos cargados de forma diferida (lazy).
- Listas largas (Kanban, inventario, historial) con virtualización.
- Búsquedas con debounce y cancelación de la petición anterior.
- Caché de datos en el store con revalidación; no repetir la misma petición en cada montaje.
- Imágenes comprimidas antes de subir, miniaturas en listas, carga diferida.
- Componentes pequeños; lógica en composables, no en la plantilla.

## Patrones de diseño

Usar el patrón cuando resuelve un problema real del módulo, y explicar en el plan por qué se usa:

- **Clean Architecture + CQRS**: comandos y consultas separados por caso de uso.
- **Pipeline behaviors** (decorador): validación, registro, transacción y caché alrededor de cada caso de uso.
- **Result pattern**: los casos de uso devuelven éxito o error de negocio; las excepciones solo para lo inesperado.
- **Specification**: filtros reutilizables de consultas (OT activas de una sucursal, productos compatibles con un vehículo).
- **Unit of Work**: el `DbContext` es la unidad de trabajo; no crear repositorios genéricos que solo envuelvan EF.
- **State**: transiciones de estado de la OT y de la jornada (qué cambio es válido desde cada estado).
- **Strategy**: modos de registro de tiempo (En vivo / Al cierre / Delegado), reglas de horas extra por país, canal de aviso (correo / WhatsApp QR / WhatsApp oficial), cálculo de precio y margen.
- **Domain events + Outbox**: acciones que disparan efectos (aviso al cliente, alerta al jefe, invalidar caché) se guardan en la misma transacción y se procesan después.
- **Factory**: creación de OT desde recepción, de checklist según tipo de vehículo.
- **Value objects**: Placa, Vin, Cedula, Ruc, Telefono, Dinero, Duracion; se validan al crearse y nunca existen inválidos.
- Frontend: composables por caso de uso, servicios de API separados de los componentes, stores por módulo.

## Seguridad

### Multi-empresa (regla número uno)
- Toda tabla de empresa tiene `empresa_id NOT NULL` con índice; llaves foráneas compuestas (`empresa_id`, `id`).
- El filtro por empresa lo aplica EF Core con un filtro global tomado del token. Nunca escribir `Where(x => x.EmpresaId == ...)` a mano.
- Segunda barrera: Row-Level Security en PostgreSQL con `FORCE ROW LEVEL SECURITY`, y la aplicación conectada con el rol sin privilegios (el dueño de la tabla se salta RLS si no se fuerza).
- `empresa_id` nunca llega por parámetro de la API; viene solo del token.
- Cada endpoint nuevo tiene una prueba de aislamiento: un usuario de la empresa A no puede leer ni escribir datos de la empresa B.
- Tablas globales (sin `empresa_id`): `usuario`, `permiso`, `vehiculo`, `cuenta_cliente`, `tipo_vehiculo`, `marca`, `modelo`.

### Autenticación y sesiones
- ASP.NET Identity; contraseñas con hash fuerte, política mínima de longitud y bloqueo tras intentos fallidos.
- JWT de vida corta en memoria del frontend; refresh token rotativo en cookie `HttpOnly`, `Secure`, `SameSite=Strict`, con detección de reutilización.
- PIN de tablet: solo válido desde dispositivos registrados de la sucursal, con límite de intentos.
- Autorización por permiso (`ot.asignar`, `costos.ver`), no por nombre de rol.

### API
- HTTPS obligatorio, HSTS, cabeceras de seguridad (CSP, X-Content-Type-Options, Referrer-Policy, frame-ancestors).
- CORS con lista explícita de orígenes.
- Rate limiting por IP y por usuario (más estricto en login, PIN y links públicos de cotización).
- Validación de toda entrada en el servidor, aunque el frontend ya valide.
- Solo consultas parametrizadas; SQL crudo únicamente con parámetros.
- Errores sin detalles internos al cliente (Problem Details); el detalle va al log.
- Links públicos (cotización, aprobación de hallazgos): token aleatorio largo, un solo propósito, con expiración y sin exponer ids internos.

### Archivos
- Validar tipo real (no solo la extensión) y tamaño máximo; renombrar al guardar.
- Bucket privado; acceso solo con URLs firmadas de corta duración.

### Datos sensibles y operación
- Secretos solo en variables de entorno o gestor de secretos; `.env` en `.gitignore`.
- Credenciales de WhatsApp y SMTP cifradas en la base.
- Costos y márgenes solo para roles con `costos.ver`.
- Auditoría de cambios en tiempos, jornadas, costos, permisos y accesos.
- Logs sin contraseñas, tokens ni datos personales completos.
- Revisar dependencias vulnerables (`dotnet list package --vulnerable`, `npm audit`) al cerrar cada módulo.

## Reglas de datos y negocio

- Ids UUID v7 generados en el cliente; escrituras idempotentes (preparado para offline).
- Las horas de tiempo y jornada las pone el servidor, no el dispositivo.
- Todo registro de tiempo o jornada guarda "Ejecutado por" (personal) y "Registrado por" (usuario).
- Borrado lógico con `eliminado_en`.

## Validaciones

- Cédula, RUC, teléfono, VIN y placa viven en `packages/validators` (TS) y `TallerOS.Domain/ValueObjects` (C#), con los mismos casos de prueba en ambos.
- Placa inválida = advertencia, no bloqueo. VIN que no decodifica = selección manual, no bloqueo.

## Interfaz

- Textos de la interfaz en español.
- Solo tokens del design system: nada de colores, tamaños o espaciados inventados.
- Tipografías: Barlow Semi Condensed (títulos), Inter (interfaz), JetBrains Mono con cifras tabulares (placa, VIN, número de OT, cronómetros, montos).
- Vista del mecánico: texto mínimo 18px, botones grandes, alto contraste.
- Todo componente funciona en escritorio (1440px) y móvil (390px), modo claro y oscuro.

## Convenciones

- Base de datos: tablas y columnas en español y snake_case (`orden_trabajo`, `empresa_id`).
- C#: dominio en español (`OrdenTrabajo`, `RegistroTiempo`); infraestructura técnica en inglés (`DependencyInjection`, `Behaviors`).
- TypeScript: camelCase; componentes Vue en PascalCase; seguir la convención del proyecto de referencia cuando exista.
- Commits pequeños con mensaje en español que diga qué y por qué.
- Si una librería tiene licencia dudosa para uso comercial (ej. Evolution API, MediatR, marker.js), avisar antes de agregarla.
