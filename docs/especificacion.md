# TallerOS — Diseño y especificación técnica

Oct 30, 2026 · @David Alvarado

## Resumen y decisiones tomadas

TallerOS (nombre provisional) es un SaaS multi-empresa para talleres mecánicos de cualquier tamaño: órdenes de trabajo tipo Jira, tiempo real trabajado por mecánico, ficha técnica del vehículo, seguimiento del cliente y avisos de mantenimiento por WhatsApp y correo.

| Tema | Decisión |
| --- | --- |
| Modelo de negocio | SaaS por suscripción, planes Básico / Pro / Empresa |
| Multi-empresa | Un solo sistema, empresas aisladas entre sí (ningún dato se cruza) |
| Jerarquía | Empresa → Sucursal → Área → Personal; roles configurables y acumulables |
| Personal sin acceso | El mecánico puede no tener cuenta; otro registra por él ("Ejecutado por" / "Registrado por") y no cuenta como usuario pagado |
| Plataformas | Web, escritorio (Tauri) y móvil (Capacitor) desde un solo código |
| Stack | Nuxt 3 + Tailwind + TypeScript; backend .NET Clean Architecture; base relacional |
| App del cliente | Mismo backend, frontend aparte en el mismo monorepo. Registro propio del dueño + registros verificados por talleres |
| Avisos | Correo (SMTP barato) + WhatsApp: QR del taller en plan Básico, API oficial de Meta en Pro/Empresa |
| Internet | MVP en línea, con la base preparada para trabajar sin conexión (offline-first) |

## Prompt para Design

Cópialo completo en Design. Pide primero las 6 pantallas prioritarias y luego el resto.

```markdown
Diseña el sistema completo de "TallerOS" (nombre provisional): un SaaS para talleres mecánicos que se vende por suscripción a talleres de cualquier tamaño, desde 1 persona hasta cadenas con varias sucursales. Se implementará en Nuxt 3 + Tailwind CSS y se empaquetará como web, app de escritorio (Tauri) y app móvil (Capacitor). Diseña un solo sistema de componentes responsive: escritorio (1440px) y móvil (390px) para cada pantalla clave. Interfaz en español.

## Contexto
- Multi-empresa: muchas empresas (talleres) en el mismo sistema, totalmente aisladas. Cada empresa tiene sucursales, áreas (mecánica, eléctrico, latonería, pintura) y personal.
- Roles configurables y acumulables: Super Admin de plataforma, Dueño/Gerente, Jefe de taller/sucursal, Jefe de área, Asesor de servicio (recepción), Mecánico, Bodeguero, Caja.
- Personal sin acceso: mecánicos que no usan la app. Otra persona registra por ellos. Cada registro muestra "Ejecutado por" y "Registrado por"; los tiempos ingresados a mano llevan un chip "Manual", distinto del chip "En vivo" del cronómetro.
- Modo tablet compartida: cada mecánico entra con un PIN de 4 dígitos.

## Flujo central (estilo Jira)
La unidad de trabajo es la Orden de Trabajo (OT): Recibido → Diagnóstico → Esperando aprobación del cliente → Esperando repuestos → En reparación → Control de calidad → Listo para entregar → Entregado.
Cada OT tiene subtareas asignadas a mecánicos. Desde que se asigna un mecánico hasta que termina se mide su tiempo trabajado en ese vehículo (iniciar / pausar con motivo / reanudar / finalizar), comparando tiempo estimado y real.

## Pantallas del taller
1. Login + selector de empresa/sucursal.
2. Dashboard del jefe: OT por estado, OT atrasadas o detenidas, alertas en tiempo real (tarea pasó su tiempo estimado, OT detenida más de X horas, hallazgo nuevo), carga por mecánico, vehículos listos para entregar.
3. Tablero Kanban de OT: columnas por estado; tarjeta con miniatura de la foto real del vehículo (o silueta por carrocería), ícono de tipo (auto / moto), placa, marca/modelo, cliente, avatares de mecánicos, tiempo transcurrido, prioridad, indicador de atraso, contador de hallazgos por aprobar y aviso de repuesto faltante. Filtros por sucursal, área, mecánico, prioridad. Arrastrar y soltar. Vista alternativa en tabla.
4. Detalle de OT: vehículo y cliente, subtareas con responsable y estado, cronómetro por subtarea, registro de tiempos con chip de origen (En vivo / Manual / Al cierre) y motivo de desviación, repuestos con costo, mano de obra, fotos, notas y notas de voz, comentarios internos, historial de actividad (quién cambió qué y cuándo), hallazgos del mecánico con severidad (rojo urgente, amarillo pronto, verde vigilar), fotos marcadas con flechas, momento (durante el trabajo, prueba de ruta, cierre) y estado (propuesto, aprobado y agregado a la OT, rechazado, pospuesto); checklist de prueba de ruta; resumen "ingresó por X, salió con X + Y".
5. Vista del mecánico (móvil, prioridad alta): "Mis tareas de hoy", botones grandes Iniciar / Pausar / Finalizar, motivo de pausa (sin repuesto, esperando cliente, almuerzo, otro), foto, reportar hallazgo. Si su modo es "Al cierre": pantalla "Cerrar trabajo" donde ingresa el tiempo total del vehículo, el sistema lo reparte entre subtareas según lo estimado, ajusta con +/− y elige el motivo cuando una subtarea se pasó del estimado (pieza oxidada, hallazgo adicional, faltó herramienta, faltó repuesto, esperó aprobación, retrabajo). Usable con manos sucias o guantes: botones grandes, alto contraste, poco texto.
6. Registro delegado: el asesor o jefe registra rápido lo que hicieron varios mecánicos (lista por mecánico, checks de tareas, hora inicio/fin manual, foto, nota de voz).
7. Recepción / check-in: búsqueda por placa, cédula o nombre; si el cliente tiene varios vehículos, lista para elegir o agregar uno nuevo; alta de vehículo con VIN que autocompleta marca/modelo/año (o selección manual del catálogo si no decodifica); validación en vivo con estados claros (cédula/RUC inválido = error, placa con formato raro = advertencia, teléfono normalizado para WhatsApp); kilometraje, nivel de combustible, diagrama según el tipo (auto o moto) para marcar golpes previos, checklist por tipo de vehículo, fotos con herramienta para marcar golpes con flechas y círculos sobre la imagen, objetos dentro, motivo de ingreso, firma del cliente en pantalla.
8. Ficha técnica del vehículo (autos y motos; foto real de la última recepción como imagen principal, silueta por tipo de carrocería si aún no hay foto): placa con historial, VIN, marca, modelo, año, motor; kilometraje histórico; línea de tiempo de visitas con fecha, trabajos, partes cambiadas, quién lo atendió en cada visita y quién lo atiende ahora; pendientes recomendados; próximos mantenimientos por km/fecha; total invertido (mano de obra vs repuestos).
9. Ficha del cliente (persona o empresa con flota): contactos autorizados, todos sus vehículos con rol (propietario, conductor, flota), historial de visitas, pendientes, canal preferido de aviso (WhatsApp / correo).
10. Cotización y aprobación: presupuesto ítem por ítem; diseña también la vista móvil que recibe el cliente por link para aprobar o rechazar cada ítem, incluidos los hallazgos con foto marcada, semáforo de severidad y precio, y la constancia "fui informado" cuando rechaza uno rojo.
11. Notificaciones al cliente: plantillas y reglas (próximo mantenimiento, vehículo listo, cotización pendiente), canal, estado de conexión de WhatsApp (QR del taller o API oficial) y bitácora de envíos.
12. Reportes: productividad por mecánico (horas trabajadas vs estimadas), tiempo promedio por tipo de trabajo, OT por estado, retrabajos, ingresos, motivos de desviación de tiempo, rentabilidad por OT (mano de obra vs repuestos), trabajos adicionales aprobados y tasa de aprobación por asesor, hallazgos frecuentes por marca/modelo.
13. Configuración de empresa: sucursales, áreas, personal (con y sin acceso), matriz editable de roles y permisos, tipos de vehículo que atiende, tipos de trabajo con tiempo estándar, modo de registro de tiempo por empresa/área/persona (En vivo / Al cierre / Delegado), márgenes por categoría y listas de precio, turnos y reglas laborales, reglas de alerta, intervalos de mantenimiento y checklists por tipo de vehículo.
14. Super Admin de plataforma: empresas clientes, plan (Básico / Pro / Empresa), uso, estado de suscripción.

## Inventario y jornada
I1. Inventario por sucursal/bodega: productos por tipo (repuesto, accesorio, insumo de pintura, insumo electromecánico, consumible), stock, mínimo, costo y precio (costo visible solo con permiso), compatibilidad por vehículo.
I2. Alertas de stock: bajo mínimo, sin stock para vehículos en taller o con cita, OT esperando repuestos, productos sin movimiento; sugerencia de compra.
I3. Compras y proveedores: orden de compra, recepción, kardex de movimientos.
I4. Repuestos en la OT: buscador que sugiere solo compatibles, origen (Bodega / Conseguido / Traído por el cliente), costo, precio y margen por línea; resumen de rentabilidad de la OT (mano de obra vs repuestos).
I5. Marcación de jornada (móvil y tablet con PIN): botón grande Entrada / Almuerzo / Salida, estado del día.
I6. Panel de asistencia del jefe: quién está, atrasos, jornadas sin cerrar por regularizar, horas extra por aprobar con su clasificación (suplementaria, extraordinaria, nocturna), reporte mensual para nómina.

## App del cliente "Mi Garage" (móvil, marca propia)
15. Mis vehículos (autos y motos): tarjetas con foto, próximo mantenimiento y última visita.
16. Detalle del vehículo: total invertido (mano de obra vs repuestos, por año y por visita), historial de partes cambiadas, próximos cambios por fecha/km.
17. Historial de visitas: cada visita con sello "Verificado por taller" o "Registro propio".
18. Agregar registro propio: el dueño anota un trabajo hecho con su mecánico de confianza (fecha, km, trabajo, costo de pieza, costo de mano de obra, foto de factura).
19. Estado de OT en curso: en qué etapa va su carro y cotización por aprobar.
20. Transferir historial al vender el vehículo.

## Estilo visual
- Herramienta de productividad tipo Jira / Linear, adaptada al taller: legible a distancia, alto contraste, iconografía clara.
- Tipografía (seria, técnica, nada redondeada ni decorativa; todas gratuitas con licencia OFL de Google Fonts):
  - Títulos y encabezados: Barlow Semi Condensed, peso 600–700. Carácter industrial y automotriz, ocupa poco ancho en tarjetas y columnas.
  - Texto de interfaz, formularios y tablas: Inter, pesos 400–600. Neutra y muy legible en pantallas pequeñas.
  - Datos técnicos: JetBrains Mono para placa, VIN, número de OT, cronómetros y montos, con cifras tabulares para que los números no "salten" al cambiar.
  - Escala: base 16px en escritorio; 18px mínimo en la vista del mecánico; cronómetro activo en 48px o más.
- Modo claro y oscuro. Paleta sobria con acento fuerte (naranja o amarillo industrial). Colores de estado consistentes: atrasado, en curso, en espera, terminado.
- Entrégalo como design system: tokens de color, tipografía, espaciado y componentes (tarjeta de OT, badge de estado, avatar de mecánico, cronómetro, chip En vivo / Manual / Al cierre, semáforo de severidad de hallazgos, chip de origen de repuesto (Bodega / Conseguido / Cliente), siluetas por carrocería (sedán, SUV, pickup, hatchback, moto), estados de validación (error, advertencia, válido), timeline de visitas, sello Verificado / Registro propio, botón grande de marcación).

Prioridad: Tablero Kanban, Detalle de OT con hallazgos, Vista del mecánico (móvil) con "Cerrar trabajo" y marcación de jornada, Recepción, Ficha del vehículo, Dashboard del jefe, Repuestos en la OT. Después el resto, y al final la app Mi Garage.
```

## Stack y arquitectura

Un monorepo con dos frontends Nuxt (taller y cliente) sobre un único backend .NET con Clean Architecture y una base PostgreSQL compartida por todas las empresas.

| Capa | Tecnología | Nota |
| --- | --- | --- |
| Frontend taller | Nuxt 3 + TypeScript + Tailwind + Pinia | Modo SPA (`ssr: false`) para poder empaquetarlo |
| Escritorio | Tauri 2 | Envuelve el mismo build de Nuxt; instalador .exe / .dmg |
| Móvil | Capacitor | Android primero; iOS después |
| App cliente "Mi Garage" | Nuxt 3 + Capacitor | Frontend aparte, mismo backend |
| Backend | .NET 10 LTS, ASP.NET Core Web API, Clean Architecture | Domain / Application / Infrastructure / Api |
| Casos de uso | CQRS + FluentValidation | Revisar licencia de MediatR (pasó a comercial); alternativa libre si hace falta |
| ORM | Entity Framework Core | Filtros globales por empresa (ver modelo de datos) |
| Base de datos | PostgreSQL | Sin costo de licencia por servidor; SQL Server es válido si prefieres |
| Tiempo real | SignalR | Kanban y alertas al jefe al instante |
| Tareas programadas | Hangfire | Recordatorios, escalamiento de alertas |
| Archivos | Almacenamiento compatible S3: Garage en local, Cloudflare R2 en producción | Fotos, firmas, notas de voz. *Cambio del 31-oct-2026: se descartó MinIO porque su imagen de Docker pasó a requerir login/suscripción; Garage es S3-compatible, misma licencia (AGPLv3) y se descarga sin login.* |
| Correo | MailKit + SMTP (Brevo o Amazon SES) |  |
| WhatsApp | Evolution API en Docker | Baileys (QR) en Básico, API oficial de Meta en Pro/Empresa; revisar su licencia |
| Autenticación | ASP.NET Identity + JWT con refresh token | PIN de 4 dígitos para tablet compartida |
| Despliegue | Docker + docker-compose en un VPS | Escalar después |

Estructura del repositorio:

```text
talleros/
  apps/
    taller/            Nuxt: web + Tauri (src-tauri)
    mi-garage/         Nuxt: app del cliente
  packages/
    ui/                design system: tokens y componentes compartidos
    api-client/        cliente TypeScript generado desde el OpenAPI del backend
  backend/
    src/TallerOS.Domain
    src/TallerOS.Application
    src/TallerOS.Infrastructure
    src/TallerOS.Api
    tests/
  infra/
    docker-compose.yml postgres, evolution-api, garage, api
```

> Nota (2026-10-31): los wrappers nativos de Capacitor (`android/`, `ios/`) no quedan dentro de `apps/taller` y `apps/mi-garage` como en el repositorio original de este documento. A pedido de David viven aparte, en `C:\Projects\Mobile\TallerOS\{taller,mi-garage}`, apuntando (`webDir`) al build de cada app Nuxt. El código fuente sigue siendo único por app ("un solo código" para web/Tauri/Capacitor se mantiene); solo se separó físicamente el proyecto nativo generado. Detalle en `C:\Projects\Mobile\TallerOS\README.md`.

> Nota (2026-11-02) — contrato de la Api, enums como string: por defecto, ASP.NET Core serializa los
> enums de C# como el número del valor (`0`, `1`, `2`...) en el JSON. Cómo se guarda un enum en
> Postgres (`HasConversion<string>()`, ver cada `*Configuration.cs`) es una decisión de persistencia
> aparte y no cambia ese default — hay que decirle explícitamente a `System.Text.Json` que serialice
> como texto. A partir del Módulo 3, `TipoCliente`, `CanalPreferido` y `RolClienteVehiculo` llevan
> `[JsonConverter(typeof(JsonStringEnumConverter))]` en su `enum` del Domain, así que viajan como
> `"PersonaNatural"`/`"WhatsApp"`/`"Propietario"`, no como `0`/`1`/`2`; el frontend (`api-client`) los
> tipa como union de strings asumiendo esto. Los enums de módulos anteriores (p. ej. `EmpresaDto.Estado`)
> NO tienen este converter todavía y en realidad viajan como número aunque su tipo en TypeScript diga
> string — inconsistencia pre-existente, fuera de alcance de este módulo. Al agregar un enum nuevo a un
> DTO: decidir a propósito si lleva este converter, no asumir que "ya se guarda como string" basta.

> Nota (2026-11-02) — limpieza de datos de prueba del catálogo: varias pruebas de integración del
> backend (`RegistrarVehiculoCommandTests`, `EndpointsClientesYVehiculosTests`,
> `ManejoDeExcepcionesMiddlewareTests`) necesitan un COMMIT real contra Postgres para probar lo que
> prueban (carreras, búsquedas globales entre conexiones distintas) y por eso, a propósito, cada una
> deja un `vehiculo` + su `marca`/`modelo` de prueba huérfanos — RLS no deja que ni la Api ni el rol
> de prueba los borren (ver el comentario de cada clase). Una corrida completa de `dotnet test` deja
> 6 filas nuevas así. Sin limpieza periódica esto se acumula; llegaron a juntarse 36 filas "Marca de
> prueba (guid)" visibles en el selector de Marca del formulario de vehículo.
> `backend/scripts/limpiar-datos-de-prueba.sql` borra exactamente esas filas (por nombre, nunca
> marcas reales) usando el superusuario de Postgres, que sí bypassea RLS; no hay forma de que esto
> corra solo al final de la suite por la misma razón de RLS, así que es un script para correr a mano
> de tanto en tanto (por ejemplo, después de una sesión larga corriendo pruebas de backend) en el
> Postgres de desarrollo.

> Nota (2026-11-02) — contrato de la Api, VIN con dígito verificador inválido ya no bloquea: hasta
> ahora `RegistrarVehiculoCommandValidator` rechazaba con 400 un VIN cuyo dígito verificador (ISO
> 3779) no cerraba. Eso cambió: ya no se valida el formato del VIN en el validador, solo se exige
> VIN o placa. `RegistrarVehiculoCommandHandler` calcula la misma comprobación (`VinValidator.EsValido`)
> después de guardar y la expone como `RegistrarVehiculoResultado.AdvertenciaVin` (string, nulo si
> no aplica) — el registro se completa y el VIN queda guardado tal cual se envió. Mismo trato
> no-bloqueante que ya tenía la placa (ver tabla de Validadores más abajo, que se actualizó para
> reflejarlo). Los bloqueos de este comando siguen siendo solo de identidad: VIN y placa que señalan
> a vehículos distintos, o un VIN que ya pertenece a otro vehículo con una placa distinta (ambos
> `ConflictoDeNegocioException` → 409) — ver `RegistrarVehiculoCommandHandler`, que no cambió.

## Modelo de datos multi-empresa

Una sola base y un solo esquema: cada tabla del taller lleva `empresa_id`. Solo usuarios, catálogo de permisos, vehículos y cuentas de cliente de Mi Garage son globales.

Reglas de aislamiento:

1. Toda tabla de empresa tiene `empresa_id NOT NULL` con índice, y las llaves foráneas entre tablas de empresa son compuestas (`empresa_id`, `id`). Así es imposible enlazar una OT de un taller con un cliente de otro.
2. EF Core aplica un filtro global por `empresa_id` tomado del token. Nadie escribe ese filtro a mano en cada consulta.
3. Segunda barrera en PostgreSQL: Row-Level Security con la empresa de la sesión. Si un día falla el filtro de EF, la base igual bloquea.
4. Ids UUID v7, fechas `timestamptz` en UTC, dinero `numeric(12,2)`, borrado lógico (`eliminado_en`).
5. La app del cliente solo lee OT de un vehículo cuyo propietario actual es esa cuenta, y solo desde la fecha en que lo es.

| Módulo | Tabla | Campos clave |
| --- | --- | --- |
| Plataforma | `empresa` | nombre, ruc, plan_id, estado, creado_en |
| Plataforma | `plan`, `suscripcion` | límites de usuarios y sucursales, canal WhatsApp permitido, vigencia |
| Organización | `sucursal` | empresa_id, nombre, dirección |
| Organización | `area` | empresa_id, sucursal_id, nombre |
| Organización | `personal` | empresa_id, area_id, nombre, tarifa_hora, tiene_acceso, usuario_id (nulo si no usa la app), pin_hash |
| Seguridad | `usuario` (global) | email, password_hash, activo |
| Seguridad | `rol` | empresa_id, nombre |
| Seguridad | `permiso` (global) | código (ej. `ot.asignar`, `tiempo.registrar_manual`) |
| Seguridad | `rol_permiso`, `usuario_rol` | usuario_id, empresa_id, sucursal_id (nulo = todas), rol_id |
| Clientes | `cliente` | empresa_id, cuenta_cliente_id (nulo), nombre, cédula/RUC, teléfono, email, canal_preferido |
| Clientes | `cuenta_cliente` (global) | usuario de Mi Garage, teléfono verificado |
| Vehículos | `vehiculo` (global) | vin (único), placa, marca, modelo, año, motor |
| Vehículos | `vehiculo_propietario` | vehiculo_id, cuenta_cliente_id, desde, hasta (transferencia al vender) |
| Vehículos | `cliente_vehiculo` | empresa_id, cliente_id, vehiculo_id |
| Trabajo | `orden_trabajo` | empresa_id, sucursal_id, número, vehiculo_id, cliente_id, estado_id, prioridad, km_ingreso, asesor_id, ingreso_en, entregado_en |
| Trabajo | `estado_ot` | empresa_id, nombre, orden, es_final (flujo configurable) |
| Trabajo | `historial_estado_ot` | ot_id, de, a, usuario_id, fecha |
| Trabajo | `recepcion` | ot_id, combustible, firma_url, objetos, observaciones |
| Trabajo | `danio_previo` | recepcion_id, zona del diagrama, descripción, foto |
| Trabajo | `tipo_trabajo` | empresa_id, nombre, tiempo_estandar_min, intervalo_km, intervalo_meses |
| Trabajo | `subtarea` | ot_id, tipo_trabajo_id, responsable_personal_id, estado, tiempo_estimado_min |
| Tiempo | `registro_tiempo` | subtarea_id, ejecutado_por (personal), registrado_por (usuario), inicio, fin, origen (EnVivo / Manual / AlCierre), nivel (Subtarea / OT), motivo_pausa, motivo_desviacion, comentario |
| Costos | `item_ot` | ot_id, tipo (ManoObra / Repuesto), descripción, cantidad, precio_unitario |
| Costos | `cotizacion`, `cotizacion_item` | estado por ítem (Pendiente / Aprobado / Rechazado), token del link, expira_en |
| Seguimiento | `recomendacion` | empresa_id, vehiculo_id, ot_origen_id, descripción, km_sugerido, fecha_sugerida, estado |
| Seguimiento | `registro_propio` | vehiculo_id, cuenta_cliente_id, fecha, km, descripción, costo_repuesto, costo_mano_obra, adjunto |
| Colaboración | `comentario`, `adjunto` | entidad, entidad_id, autor, url, tipo (foto, voz, firma) |
| Alertas | `regla_alerta`, `alerta` | tipo, umbral, rol destinatario, leída |
| Avisos | `canal_empresa` | empresa_id, tipo (Correo / WhatsAppQR / WhatsAppOficial), configuración cifrada |
| Avisos | `plantilla_notificacion`, `regla_recordatorio` | evento, texto con variables, anticipación en días o km |
| Avisos | `notificacion` | empresa_id, destinatario, canal, estado, intentos, enviado_en (bandeja de salida) |
| Auditoría | `auditoria` | empresa_id, usuario_id, entidad, entidad_id, acción, antes (jsonb), después (jsonb), fecha |

## Catálogos, validadores y motos

No inventamos catálogos ni algoritmos que ya existen: sembramos marcas y modelos desde datasets abiertos, validamos con librerías probadas y tratamos la moto como un tipo de vehículo más, con su propio checklist e intervalos.

**Marcas y modelos**

| Fuente | Qué aporta | Uso en TallerOS |
| --- | --- | --- |
| [NHTSA vPIC API](https://vpic.nhtsa.dot.gov/api/Home) | Decodifica VIN, marcas por tipo de vehículo (car, motorcycle, truck), modelos por marca y año; base descargable | Autocompletar marca/modelo/año al escribir el VIN. Límite de uso: descargar la base y consultarla en local |
| [nhtsa-api-wrapper](https://github.com/shaggytech/nhtsa-api-wrapper) (npm) | Cliente JS de vPIC + `isValidVin` sin conexión | Validar el dígito verificador del VIN en el frontend |
| [vehicle-make-model-data](https://github.com/arthurkao/vehicle-make-model-data) | Autos, motos, camiones y UTV; ~19.700 modelos en JSON, CSV y MySQL | Semilla inicial de `marca` y `modelo` |
| [vehicle-makes-models](https://github.com/gor3a/vehicle-makes-models/releases) | 164 marcas, 2.621 modelos, generaciones y motores; SQLite, JSON, CSV | Detalle de motor para la ficha técnica |

Límite importante: vPIC está pensado para Estados Unidos y Canadá. Marcas comunes en Ecuador (chinas, motos ensambladas localmente) pueden no decodificar. Por eso el VIN ayuda a autocompletar, pero nunca bloquea; si no hay coincidencia, se elige o se crea la marca y el modelo a mano. Revisar la licencia de cada dataset antes de incluirlo.

**Validadores**

| Dato | Regla | Librería o base |
| --- | --- | --- |
| Cédula y RUC (persona natural, sociedad privada, sociedad pública) | Dígito verificador módulo 10 / 11, código de provincia | Portar a C# y TS la lógica de [ec-validador-cedula-ruc](https://github.com/tavo1987/ec-validador-cedula-ruc), con sus casos de prueba; incluir la corrección del tercer dígito menor a 6 de [validador-cedula-ruc-ec](https://packagist.org/packages/alban/validador-cedula-ruc-ec) |
| Teléfono (celular 09, fijo, internacional para WhatsApp) | Formato E.164 | libphonenumber-csharp (backend) y libphonenumber-js (frontend) |
| VIN / chasis | 17 caracteres, sin I, O, Q, dígito verificador | `isValidVin` en frontend; misma regla en C#. Advertencia, no bloqueo (desde el 2026-11-02): un VIN mal transcrito se guarda igual, con aviso |
| Placa | Autos: 3 letras + 3 o 4 números ([formato ANT](https://elyex.com/placas-de-autos-en-ecuador/)); motos: formato propio en placa de 16 × 19 cm | Advertencia, no bloqueo: hay placas provisionales y cambio de placas en curso |
| Correo y formularios | Reglas por campo | FluentValidation (backend), Zod + VeeValidate (frontend) |

Los validadores de Ecuador van en un paquete propio (`packages/validators` y `TallerOS.Domain/Validators`) con las mismas pruebas en ambos lados. Así, el día que se agregue otro país, solo se suma un validador.

**Clientes con varios vehículos**

- Un `cliente` tiene N vehículos a través de `cliente_vehiculo`, con rol: Propietario, Conductor habitual o Responsable de flota.
- `cliente.tipo`: Persona natural o Empresa. Una empresa (flota de reparto, cooperativa de taxis) agrupa decenas de vehículos y varios contactos autorizados para aprobar cotizaciones.
- La placa puede cambiar: el vehículo se identifica por VIN o chasis, y la placa se guarda con historial (`vehiculo_placa`: placa, desde, hasta).
- En recepción, buscar por placa, cédula o nombre muestra todos los vehículos del cliente para elegir.

**Motos**

- `tipo_vehiculo` global: Auto, Camioneta/SUV, Moto, Camión, Otro. Cada empresa elige qué tipos atiende.
- Recepción con diagrama y checklist según el tipo: en moto, cadena, kit de arrastre, frenos, luces, casco u objetos dejados.
- `tipo_trabajo` y `regla_recordatorio` filtrados por tipo de vehículo: cambio de aceite cada X km es distinto en moto y en auto.
- Motos sin odómetro funcional: el mantenimiento se calcula por fecha.

Tablas nuevas o ajustadas: `tipo_vehiculo`, `marca` (con tipo), `modelo`, `vehiculo_placa`, `checklist_plantilla` por tipo, y `cliente.tipo` + `cliente_vehiculo.rol`.

## Inventario, costos y margen

Cada repuesto e insumo guarda su costo y su precio al cliente por separado. Así el dueño ve, por OT, cuánto ganó en mano de obra y cuánto en repuestos, y decide dónde poner su margen.

**Qué se maneja**

| Tipo de producto | Ejemplos | Cómo se descuenta |
| --- | --- | --- |
| Repuesto | Pastillas, filtros, bujías, kit de arrastre | Por unidad, cargado a la OT y cobrado al cliente |
| Accesorio | Alarmas, radios, luces LED, cubre asientos | Por unidad; también venta directa sin OT |
| Insumo de pintura | Pintura, thinner, masilla, lija, cinta | Por fracción (0,25 L, 2 pliegos) cargada a la OT; puede cobrarse o solo costearse |
| Insumo electromecánico | Cable, terminales, fusibles, relés | Por metro o unidad, igual que pintura |
| Consumible del taller | Guaipe, desengrasante, guantes | Consumo interno, entra al costo del taller, no a la OT |

**Origen del repuesto en cada OT**

1. **De bodega**: sale del stock al costo promedio ponderado.
2. **Conseguido para esta OT**: el taller lo compra afuera; se registra proveedor, costo real y precio al cliente. No pasa por stock.
3. **Traído por el cliente**: costo y precio cero; el taller cobra solo mano de obra y la OT marca que esa pieza no tiene garantía del taller.

**Precio y margen**

- Precio al cliente = costo × (1 + margen). El margen se define por categoría o por producto, con opción de precio fijo.
- Listas de precio: Público, Flota o convenio. Un cliente empresa puede tener su propia lista.
- La mano de obra se cobra por tarifa del tipo de trabajo (hora o precio fijo), y su costo es horas reales × tarifa del mecánico.
- Costos y márgenes solo los ven roles con permiso `costos.ver`. Mecánico y cliente ven nombre y precio, nunca el costo.
- Reporte de rentabilidad por OT, por mecánico y por tipo de trabajo: ingreso y costo de mano de obra, ingreso y costo de repuestos, margen.

**Compatibilidad y alertas de stock**

- Cada producto tiene compatibilidad por tipo de vehículo, marca, modelo, rango de años y motor (`producto_compatibilidad`). Al cargar repuestos en una OT, el sistema sugiere solo los compatibles con ese vehículo.
- Stock por sucursal o bodega, con mínimo y punto de reorden.
- Alertas al bodeguero y al jefe:
  - Producto bajo el mínimo.
  - Sin stock para un vehículo que está en el taller o tiene cita ("no hay pastillas para la Hilux 2018 que entra mañana").
  - OT en "Esperando repuestos" por más de X días.
  - Producto sin movimiento en 90 días (dinero detenido).
- Sugerencia de compra según consumo promedio de las últimas semanas.
- Cada área ve su propio inventario: pintura sus insumos, electromecánica sus partes.

**Tablas**: `producto` (tipo, código interno, código OEM, marca, unidad, categoría), `producto_compatibilidad`, `bodega`, `stock` (producto, bodega, cantidad, mínimo, costo_promedio), `movimiento_inventario` (kardex: compra, salida a OT, ajuste, consumo interno, traspaso, devolución), `proveedor`, `compra` + `compra_item`, `solicitud_repuesto` (desde la OT al bodeguero), `lista_precio`, `regla_margen`. `item_ot` suma: producto_id, origen (Bodega / Conseguido / Cliente), costo_unitario, precio_unitario.

## Jornada laboral y horas extra

La jornada (entrada y salida del trabajador) se registra aparte del tiempo por tarea. Así se mide la productividad real: horas en tareas frente a horas en el taller.

**Marcación**

- Desde el celular, con ubicación opcional dentro del radio de la sucursal y foto opcional.
- Desde la tablet compartida con PIN.
- Registrada por el jefe para el personal sin acceso, con "Registrado por" igual que en los tiempos.
- Hora del servidor, no del dispositivo. Marcas: Entrada, Inicio almuerzo, Fin almuerzo, Salida.
- Cada persona tiene un turno con horario y tolerancia de atraso.
- No se puede iniciar una tarea sin jornada abierta. Al cerrar la jornada se pausan las tareas abiertas.

**Olvido de cierre**

1. A la hora de salida del turno + 15 minutos (configurable), aviso push al trabajador.
2. Si sigue abierta + 1 hora, aviso al jefe.
3. A medianoche la jornada pasa a "Pendiente de regularizar". El sistema no inventa la hora de salida.
4. El jefe la corrige con motivo obligatorio y queda en auditoría. Ninguna hora extra de esa jornada se aprueba sin regularizar.

**Horas extra (reglas configurables por país; valores por defecto de Ecuador)**

- Jornada ordinaria: 8 horas diarias y 40 semanales ([Art. 47](https://derechoecuador.com/?p=17756)).
- Suplementarias: después de la jornada y hasta las 24:00, recargo del 50%; máximo 4 por día y 12 por semana ([fuente](https://www.teleamazonas.com/actualidad/noticias/ecuador/calculo-horas-suplementarias-extraordinarias-ecuador-125035/)).
- Extraordinarias: entre 00:00 y 06:00, y en feriados o días de descanso, recargo del 100% (misma fuente).
- Recargo nocturno: 25% ([Art. 49](https://derechoecuador.com/jornada-maxima-de-trabajo/)).
- Flujo: el sistema detecta el excedente, lo clasifica y el jefe lo aprueba o rechaza. Alerta si alguien se acerca al límite semanal.
- Salida: reporte mensual por trabajador (ordinarias, suplementarias, extraordinarias, atrasos, faltas) exportable para nómina. TallerOS no calcula la nómina completa.
- Las reglas laborales se guardan como configuración, no en código, porque la ley cambia. Validar los valores con un abogado laboral antes de vender.

**Tablas**: `turno`, `asignacion_turno`, `marcacion` (tipo, hora, origen App / Tablet / Delegada, ubicación), `jornada` (fecha, estado Abierta / Cerrada / Pendiente de regularizar, horas ordinarias, suplementarias, extraordinarias, nocturnas), `aprobacion_horas_extra`, `feriado`, `regla_laboral` (por país).

## Hallazgos y trabajos adicionales

El vehículo entra por un motivo y puede salir con más trabajos: cada hallazgo del mecánico queda registrado con foto y severidad, y se convierte en trabajo aprobado dentro de la OT o en recomendación para la próxima visita.

**Cuándo se reporta**

- Durante el trabajo: "fui por frenos y la barra de dirección tiene juego".
- En la prueba de ruta del control de calidad, con un checklist por tipo de vehículo (ruidos, dirección, frenado, luces, fugas).
- Al cerrar la OT: observaciones generales del vehículo a considerar después del arreglo.

**Qué guarda cada hallazgo**

| Campo | Detalle |
| --- | --- |
| Sistema | Frenos, suspensión, dirección, motor, eléctrico, carrocería, llantas, otro |
| Severidad | Rojo: urgente, afecta la seguridad · Amarillo: pronto, en los próximos km o meses · Verde: vigilar |
| Descripción y evidencia | Texto o nota de voz, fotos marcadas con flechas o círculos |
| Sugerencia | Trabajo recomendado, repuestos y costo estimado, km o fecha sugeridos |
| Reportado por | Mecánico (o registrado por el jefe) y momento: durante el trabajo, prueba de ruta, cierre |

**Qué pasa después**

1. El asesor envía al cliente un link por WhatsApp con foto, severidad y precio.
2. Si aprueba: se agrega a la misma OT como trabajo adicional, con sus subtareas, repuestos y tiempo estimado.
3. Si rechaza o lo deja para después: pasa a `recomendacion` del vehículo y genera recordatorio. Si es rojo, el cliente firma o acepta en el link que fue informado.
4. La ficha del vehículo muestra la diferencia: "Ingresó por cambio de frenos; salió con frenos, aceite y barra de dirección".

Reportes que salen de aquí: ingresos por trabajos adicionales, tasa de aprobación por asesor, hallazgos más frecuentes por marca y modelo.

**Tabla**: `hallazgo` (empresa_id, ot_id, vehiculo_id, sistema, severidad, descripción, momento, reportado_por, registrado_por, estado Propuesto / Aprobado / Rechazado / Pospuesto, ot_adicional_subtarea_id, recomendacion_id). El momento "prueba de ruta" usa `checklist_plantilla` por tipo de vehículo.

## Fotos del vehículo

La foto principal de cada vehículo es la que se toma en recepción, no una imagen de catálogo. Los bancos de imágenes de autos en GitHub casi siempre prohíben el uso comercial, así que no sirven para un producto que se vende.

| Necesidad | Solución | Licencia o costo |
| --- | --- | --- |
| Foto real del vehículo | La última foto frontal de recepción queda como avatar en la ficha, el Kanban y Mi Garage | Propia del taller |
| Tomar fotos en móvil y escritorio | Plugin Camera de Capacitor; en web, input de archivo con cámara | Libre |
| Comprimir antes de subir | [browser-image-compression](https://github.com/Donaldcwl/browser-image-compression): reduce peso en el navegador con web worker | MIT |
| Marcar golpes y hallazgos sobre la foto | [image-annotator](https://github.com/hanieldaniel/image-annotator) (flechas, círculos, texto; exporta PNG) o [marker.js 3](https://github.com/ailon/markerjs3) | image-annotator: MIT · marker.js: revisar licencia, la versión 2 era linkware |
| Imagen de catálogo cuando aún no hay foto | Silueta propia por tipo de carrocería (sedán, SUV, pickup, hatchback, moto) en SVG | Diseñada en el design system |
| Imagen del modelo exacto (opcional, plan Pro) | [IMAGIN.studio](https://docs.imagin.studio/guides/getting-images/selecting-a-car): imagen por marca, modelo, año y color; o [CarsXE Vehicle Images](https://carsxe.com/vehicle-images), que incluye motos | De pago |

No usar: datasets como Stanford Cars o el de vehículos de la India (licencias no comerciales) ni logos de marcas descargados de repositorios, porque son marcas registradas y varios de esos repositorios prohíben el uso comercial.

Almacenamiento: original comprimido + miniatura en el almacenamiento S3, tabla `adjunto` con tipo (Recepción, Hallazgo, Trabajo terminado, Factura) y la marca de agua opcional del taller con fecha y placa.

## Reglas de negocio clave

**Tiempo trabajado**

- El tiempo de un mecánico en un vehículo es la suma de sus `registro_tiempo` cerrados en las subtareas de esa OT.
- Un mecánico no puede tener dos cronómetros abiertos a la vez. Al iniciar otro, el anterior se pausa con motivo "Cambio de tarea".
- La hora la pone el servidor, no el dispositivo, para evitar manipulación.
- Un registro manual guarda quién lo digitó. Editar un registro cerrado exige permiso `tiempo.editar` y queda en auditoría.
- Reportes separan horas En vivo y Manual, para que el dueño sepa qué tan confiable es cada dato.

**Modos de registro de tiempo (configurable por empresa, área o persona)**

| Modo | Cómo marca el mecánico | Para quién |
| --- | --- | --- |
| En vivo | Iniciar / pausar / finalizar cada subtarea | Talleres que quieren precisión por tarea |
| Al cierre | Al terminar la OT (o el día) indica el tiempo total y lo reparte entre las subtareas | Mecánicos que no se detienen entre tareas |
| Delegado | El jefe o asesor registra por él | Personal sin acceso a la app |

Pantalla "Cerrar trabajo" del modo al cierre:

1. El mecánico pone el tiempo total que le tomó el vehículo (ej. 3 h 40 min). El sistema propone la hora de inicio desde su primera asignación y la de fin en el momento actual.
2. El sistema reparte ese total entre las subtareas en proporción a su tiempo estimado; el mecánico ajusta con +/− si alguna tomó más.
3. Si una subtarea supera su estimado en más de 25% (configurable), debe indicar el motivo: pieza oxidada o trabada, hallazgo adicional, faltó herramienta, faltó repuesto, esperó aprobación del cliente, retrabajo, otro. Nota de voz o texto opcional.
4. Queda marcado como "Al cierre": en reportes se distingue de "En vivo" y de "Manual", y el total no puede superar el tiempo de su jornada abierta ese día.

Con los motivos acumulados, el dueño ve por qué se van las horas (ej. 30% de los retrasos en frenos son por piezas oxidadas) y ajusta el tiempo estándar de cada tipo de trabajo.

**Alertas al jefe**

- Hangfire revisa cada minuto: subtarea que superó su tiempo estimado, OT sin movimiento más de X horas, hallazgo nuevo.
- La alerta llega por SignalR al instante y, si no se lee en N minutos, por push o WhatsApp al jefe del área.

**Avisos al cliente**

- Patrón bandeja de salida: el sistema escribe en `notificacion` dentro de la misma transacción y un job la envía con reintentos. Nunca se pierde un aviso si WhatsApp está caído.
- Próximo mantenimiento = el primero que llegue entre `intervalo_km` y `intervalo_meses` del tipo de trabajo. El km se estima con el promedio diario entre visitas.
- El cliente puede darse de baja de los avisos; se respeta por canal.

**Offline (preparar desde ya, activar después)**

- Escritorio y móvil guardan en local (SQLite o IndexedDB) las OT del día y una cola de operaciones.
- Al reconectar se envía la cola. Los registros de tiempo usan la hora del dispositivo marcada como "sin conexión" y el servidor la valida.
- Para eso, todo id se genera en el cliente (UUID v7) y todas las escrituras son idempotentes.

**Seguridad**

- JWT corto + refresh token rotativo; el token lleva `usuario_id`, `empresa_id` activa y permisos.
- Cambiar de empresa emite un token nuevo, nunca se pasa `empresa_id` por parámetro.
- Links de cotización con token aleatorio de un solo propósito y expiración.
- Credenciales de WhatsApp y SMTP cifradas en la base.
- Pruebas automáticas que intentan leer datos de otra empresa en cada endpoint.

## Fases y pendientes

El MVP es lo mínimo para que un taller piloto deje el cuaderno: recepción, OT tipo Jira, tiempos y alertas al jefe.

1. **Fase 0 — Base**: monorepo, design system, autenticación, multi-empresa con aislamiento probado, roles y permisos, personal con y sin acceso.
2. **Fase 1 — MVP (web + Android)**: recepción, tablero Kanban, detalle de OT, vista del mecánico con cronómetro, registro delegado, ficha de vehículo y cliente, alertas al jefe, jornada laboral (marcación, olvido de cierre, horas extra), reportes básicos.
3. **Fase 2 — Seguimiento y venta**: cotización con aprobación por link, inventario con costo, margen, compatibilidad y alertas de stock, recomendaciones pendientes, avisos por correo y WhatsApp, portal web del cliente.
4. **Fase 3 — Escritorio y offline**: empaquetado Tauri, modo sin conexión, tablet compartida con PIN.
5. **Fase 4 — Mi Garage**: app del cliente con registro propio, inversión total y transferencia de historial.
6. **Fase 5 — Ampliación**: compras avanzadas y sugerencia automática de pedidos, facturación electrónica SRI (reutilizando tu SaaS de facturación), agenda de citas, panel Super Admin con cobro de planes.

Por decidir:

- [ ] Mercado inicial: solo Ecuador o Latinoamérica
- [ ] Taller piloto para validar la Fase 1
- [ ] PostgreSQL o SQL Server
- [ ] Nombre definitivo del producto
- [ ] Precio de cada plan y cómo se cobra WhatsApp oficial
- [ ] Revisar licencias de Evolution API y MediatR antes de integrarlos

Mantenimiento pendiente (no resuelto, solo mitigado):

- [ ] `backend/scripts/limpiar-datos-de-prueba.sql` borra los huérfanos que dejan las pruebas de
  integración de `RegistrarVehiculo` contra Postgres real (ver la nota del 2026-11-02 arriba y el
  comentario de `RegistrarVehiculoCommandTests`), pero no evita que se sigan generando — cada
  corrida de esas pruebas vuelve a dejar filas nuevas. No hay forma de automatizarlo del todo
  mientras la limpieza necesite bypassear RLS (requiere el superusuario de Postgres, no un rol de
  prueba ni de la Api). Mientras tanto: correr el script de tanto en tanto, sobre todo antes de
  revisar visualmente el catálogo de Marca.
