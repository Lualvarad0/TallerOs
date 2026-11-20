# Handoff: TallerOS — Sistema de diseño + pantallas (v2 en progreso)

## Overview
TallerOS es un SaaS multi-empresa para talleres mecánicos (1 persona → cadenas con sucursales). Se implementará en **Nuxt 3 + Tailwind CSS** y se empaquetará como web, escritorio (Tauri) y móvil (Capacitor). Interfaz en español. Este paquete contiene el sistema de diseño y las pantallas clave en escritorio (1440px) y móvil (390px), más la app del cliente «Mi Garage».

## About the Design Files
Los archivos de este paquete son **referencias de diseño hechas en HTML**: prototipos que muestran el aspecto y el comportamiento esperados, **no código de producción para copiar**. La tarea es **recrear estos diseños en Nuxt 3 + Tailwind** con componentes Vue (Composition API), tokens en `tailwind.config` (o variables CSS) y el patrón de stores (Pinia) que use el proyecto.

Cada `*.dc.html` es un «Design Component»: un archivo que se abre directamente en el navegador (necesita `support.js` en la misma carpeta). Dentro de cada archivo:
- El **template** (HTML con estilos inline y huecos `{{ }}`) está entre `<x-dc>` y `</x-dc>`.
- La **lógica** (datos de ejemplo + estado + handlers) está en `<script type="text/x-dc" data-dc-script>` como `class Component extends DCLogic` con `renderVals()` (equivale a un `computed` que expone datos al template).
- `<sc-for list="{{ x }}" as="item">` = `v-for`; `<sc-if value="{{ x }}">` = `v-if`; `<dc-import name="Sidebar">` = componente hijo.
- Cada archivo muestra un **lienzo**: escritorio 1440 + teléfono 390 lado a lado (los marcos/bisel y la barra gris superior son solo de presentación).

Abrir `TallerOS.dc.html` (índice) en un navegador para navegar todo.

## Fidelity
**Alta fidelidad (hifi).** Colores, tipografía, espaciados, radios y estados son finales. Recrear con precisión usando Tailwind. Los datos son de ejemplo.

## Design Tokens
Definidos como variables CSS en el `<style>` del `<helmet>` de cada archivo; el tema se aplica con `data-theme="dark|light"` en el contenedor raíz. Recomendado: mapearlos a `tailwind.config` con `colors: { bg: 'var(--bg)', ... }` y modo oscuro por atributo.

| Token | Claro | Oscuro | Uso |
|---|---|---|---|
| --bg | #EEEEEB | #0C0E10 | Fondo de app |
| --sf | #FFFFFF | #15181B | Superficie (tarjetas, sidebar) |
| --sf2 | #F6F6F3 | #1B1F23 | Superficie 2 (inputs, cabeceras de tabla) |
| --sf3 | #E9E9E5 | #252A2F | Superficie 3 (tracks, chips neutros, placeholders) |
| --bd | #DEDED8 | #2A3036 | Borde |
| --bd2 | #C6C6BF | #3B434A | Borde fuerte (botones secundarios, inputs) |
| --tx | #15171A | #EEF0F2 | Texto |
| --tx2 | #474E56 | #B4BCC4 | Texto secundario |
| --tx3 | #737B84 | #8A939C | Texto terciario / meta |
| --ac | #FFC21A | #FFC21A | Acento amarillo industrial: acción primaria, foco, selección. Nunca como estado |
| --ac-ink | #1A1400 | #1A1400 | Texto sobre acento |
| --ac-tx | #8A5F00 | #FFC940 | Texto en tono acento |
| --ac-s | #FFF3CC | #2E2710 | Fondo tinte acento |
| --red / --red-s | #C8282E / #FDE6E6 | #FF6166 / #3A1618 | **Atrasado**, error, urgente |
| --blue / --blue-s | #1A62D6 / #E2ECFD | #5AA2FF / #14283F | **En curso** (diagnóstico, reparación, QC) |
| --vio / --vio-s | #6E41D0 / #EEE7FC | #AE92FF / #261D42 | **En espera** (aprobación, repuestos, pausa) |
| --grn / --grn-s | #137A3A / #DFF3E6 | #43D88F / #0F2E20 | **Terminado**, válido, aprobado |
| --sh | 0 1px 2px rgba(20,20,20,.06), 0 6px 20px rgba(20,20,20,.07) | 0 1px 2px rgba(0,0,0,.4), 0 8px 24px rgba(0,0,0,.35) | Sombra de tarjeta |

**Mapeo de estados de OT → color**: Recibido (sf3/tx2) · Diagnóstico, En reparación, Control de calidad (blue) · Esperando aprobación, Esperando repuestos (vio) · Listo para entregar (grn) · Entregado (sf3 fondo, grn texto). Atraso = borde izquierdo rojo + badge rojo sólido `+2h`.

**Tipografía** (Google Fonts, OFL):
- Títulos: **Barlow Semi Condensed** 600–700 (48 / 32 / 26 / 22 / 20 px).
- Interfaz: **Inter** 400–600, base 16px escritorio (meta 12–13px).
- Datos: **JetBrains Mono** 500–700 con `font-variant-numeric: tabular-nums` para placa, VIN, nº OT, cronómetros, montos.
- Vista del mecánico: 18px mínimo; cronómetro activo ≥48px (68px en tarea en curso).
- Placa: mono 700, 12.5–22px, `border: 1.5–2.5px solid var(--tx)`, radio 4–6px, letter-spacing .06em.

**Espaciado**: base 4px (4, 8, 12, 16, 24, 32, 48, 64). Escritorio denso; botones táctiles de taller 64–120px de alto.
**Radios**: 4 (placa, badges pequeños), 5–8 (badges, botones, inputs), 10–12 (tarjetas), 14–22 (botones táctiles del mecánico), 44 (marco de teléfono, solo presentación).
**Iconos**: Phosphor Icons (`@phosphor-icons/web` 2.1.1; en Nuxt usar `@phosphor-icons/vue`). Pesos regular, bold y fill.
**Foco**: `outline: 2px solid var(--ac); outline-offset: 2px`.

## Componentes del sistema (ver Design-System.dc.html)
- **Botones**: primario (bg --ac, texto --ac-ink, 36px, radio 8, 700), secundario (bg --sf, borde --bd2), ghost, icono 36×36. Botones de taller 72–120px (Iniciar = grn, Pausar = ac, Finalizar = tx sobre bg).
- **Badge de estado OT**: alto 22–24px, padding 0 8–9px, radio 5, 12–12.5px/700, icono Phosphor bold + texto, fondo `*-s` y texto color de estado.
- **Prioridad**: icono caret-double-up (Urgente, red), caret-up (Alta, ac-tx), equals (Normal, tx3).
- **Avatar de mecánico**: círculo 24–40px con iniciales 700; punto de estado (grn trabajando / ac pausa). **Sin acceso** = fondo transparente + `1.5px dashed var(--tx3)`. Grupo con solape −6/−8px.
- **Cronómetro**: punto pulsante grn + mono 28–68px + barra de progreso (blue; red si excede).
- **Chip origen de tiempo**: En vivo (grn-s/grn con punto pulsante), Manual (borde punteado tx3), Al cierre (blue-s/blue, icono flag-checkered). Registro muestra «Ejecutado por» y «Registrado por».
- **Motivos de desviación**: Pieza oxidada, Hallazgo adicional, Faltó herramienta, Faltó repuesto, Esperó aprobación, Retrabajo.
- **Semáforo de severidad**: rojo Urgente / amarillo (--ac) Pronto / verde Vigilar (punto 12–16px con halo 3–4px del tinte).
- **Estado de hallazgo**: Propuesto (sf3), Aprobado · en OT (grn), Rechazado (red), Pospuesto (vio).
- **Chip origen de repuesto**: Bodega (blue, warehouse), Conseguido (vio, storefront), Cliente (borde bd2, user).
- **Validación de campo**: error (borde 2px red + x-circle + mensaje red), advertencia (borde ac + warning + mensaje ac-tx), válido (borde grn + check-circle).
- **Siluetas por carrocería**: `Silueta.dc.html` (sedan, suv, pickup, hatchback, moto), SVG 120×56, relleno --tx3 + ruedas --tx2. Se usan cuando no hay foto real.
- **Tarjeta de OT**: miniatura 58×42 (silueta o foto) + mini-badge auto/moto, placa + nº OT + prioridad, modelo (Barlow 17px), cliente · trabajo, chips «N hallazgos por aprobar» (ac) y «Falta repuesto» (vio), avatares, tiempo transcurrido, badge de atraso. Borde izquierdo 3px del color de estado (rojo si atrasada).
- **Timeline de visitas**, **Sello Verificado por taller / Registro propio**, **Botón grande de marcación** (Entrada grn, Almuerzo ac, Salida tx).

## Screens / Views
| # | Archivo | Estado |
|---|---|---|
| 00 | Design-System.dc.html | Tokens + componentes (v2) |
| 01 | Login.dc.html | Login + selector empresa/sucursal (acordeón interactivo) |
| 02 | Dashboard.dc.html | KPIs, OT por estado (barra apilada), alertas en vivo, carga por mecánico, listos para entregar, detenidas |
| 03 | Tablero-Kanban.dc.html | **1a** columnas por estado con drag & drop + vista tabla; **1b** carriles por mecánico × 4 fases. Tarjetas v2 |
| 04 | Detalle-OT.dc.html | **2a (v2 completo)**: resumen ingresó/sale con, subtareas con cronómetro, hallazgos con severidad/foto marcada/momento/estado, registro de tiempos con origen y desvío, repuestos con buscador de compatibles + origen/costo/precio/margen, rentabilidad, checklist de prueba de ruta, fotos/nota de voz, comentarios, actividad. **2b**: alternativa «cabina» (aún sin secciones v2) |
| 05 | Vista-Mecanico.dc.html | A Mis tareas · B tarea en curso (Pausar con motivo) · C reportar hallazgo · E **Cerrar trabajo** (modo Al cierre) · F **Marcación de jornada** · D tablet compartida con PIN (prueba: 1234) |
| 06 | Registro-Delegado.dc.html | Lista por mecánico, checks, hora inicio/fin manual, foto, nota de voz |
| 07 | Recepcion.dc.html | v1 (placa, km, combustible, diagrama de daños clicable, motivo, fotos, objetos, firma). **Pendiente v2** (ver abajo) |
| 08 | Ficha-Vehiculo.dc.html | Datos técnicos, km histórico, total invertido MO vs repuestos, timeline de visitas, próximos mantenimientos, pendientes |
| 09 | Ficha-Cliente.dc.html | Contacto, canal preferido, vehículos, pendientes, historial |
| 10 | Cotizacion.dc.html | Editor ítem por ítem + vista móvil del cliente para aprobar/rechazar (totales en vivo) |
| 11 | Notificaciones.dc.html | Conexión WhatsApp (QR / API oficial), reglas con toggle, plantilla con variables, bitácora |
| 12 | Reportes.dc.html | Productividad real vs estimado, ingresos, tiempo por tipo de trabajo, OT por estado, retrabajos |
| 13 | Configuracion.dc.html | Sucursales/áreas, personal con y sin acceso, matriz editable de permisos (8 roles), tipos de trabajo, alertas e intervalos |
| 14 | Super-Admin.dc.html | Empresas, plan, uso, estado de suscripción (filtros) |
| 15–20 | Mi-Garage.dc.html | App cliente (marca propia: Figtree, arena #F6F2EA, verde #0E7C6B, ámbar #F2B544): vehículos, detalle con inversión por año/visita, historial con sellos, registro propio, OT en curso, transferir historial |
| — | Sidebar.dc.html, TabBar.dc.html, Silueta.dc.html | Componentes compartidos |
| — | Recepcion v1.dc.html | Respaldo de la versión anterior |

### Layout común
- **Escritorio 1440**: Sidebar 232px (logo, selector empresa/sucursal, navegación con contadores, usuario) + área principal. Barra superior 52–64px (título Barlow 22px, buscador 280px con atajo «/», botón primario).
- **Móvil 390**: barra de estado 44px, cabecera con título 24px, contenido con scroll, TabBar 84px (Inicio, Tablero, + Recepción, Vehículos, Más).

## Pendiente del brief v2 (no diseñado aún — implementar siguiendo el sistema)
- **Recepción v2**: búsqueda por placa/cédula/nombre; lista de vehículos del cliente o alta nueva; VIN que autocompleta marca/modelo/año (o catálogo manual); validación en vivo (cédula/RUC inválido = error, placa rara = advertencia, teléfono normalizado a WhatsApp); diagrama según auto/moto; checklist por tipo; fotos con herramienta de flechas/círculos.
- **Ficha vehículo v2**: foto real de la última recepción como imagen principal (silueta si no hay), historial de placa, motos.
- **Ficha cliente v2**: empresa con flota, contactos autorizados, rol por vehículo (propietario/conductor/flota).
- **Cotización v2**: hallazgos con foto marcada + semáforo + precio; constancia «fui informado» al rechazar uno rojo.
- **Reportes v2**: motivos de desviación, rentabilidad por OT, trabajos adicionales y tasa de aprobación por asesor, hallazgos frecuentes por marca/modelo.
- **Configuración v2**: tipos de vehículo, modo de registro de tiempo por empresa/área/persona (En vivo / Al cierre / Delegado), márgenes y listas de precio, turnos y reglas laborales, checklists por tipo.
- **Inventario I1–I3**: stock por sucursal/bodega, tipos de producto, mínimo, costo/precio (costo con permiso), compatibilidad; alertas de stock y sugerencia de compra; órdenes de compra, recepción, kardex.
- **Asistencia I6**: quién está, atrasos, jornadas sin cerrar, horas extra por aprobar (suplementaria/extraordinaria/nocturna), reporte mensual para nómina.
- **Mi Garage v2**: fotos en tarjetas y motos.

## Interactions & Behavior
- Tema claro/oscuro persistido en `localStorage['talleros-theme']`.
- Kanban: drag & drop HTML5 entre columnas cambia el estado; columna destino resaltada (fondo --ac-s, borde 2px dashed --ac). Toggle Tablero/Tabla. Móvil: pestañas por estado.
- Subtareas: Iniciar/Pausar/Reanudar; un solo cronómetro activo por mecánico; cronómetro rojo al exceder estimado.
- Pausa requiere motivo (Sin repuesto, Esperando cliente, Almuerzo, Otro) en hoja inferior con botones de 76px.
- Cerrar trabajo: total ± 15 min, cada subtarea ± 5 min; si una subtarea excede su estimado, es obligatorio elegir motivo (botón final deshabilitado hasta completar).
- Marcación: Entrada → (Almuerzo → Volver) → Salida, con lista de marcas del día.
- PIN de 4 dígitos con teclado de 96px; error inline.
- Hallazgo propuesto → «Enviar a cotización» (Aprobado · en OT) o «Posponer».
- Checklist prueba de ruta: tri-estado (pendiente / OK / generó hallazgo).
- Cotización cliente: aprobar/rechazar por ítem recalcula total aprobado (impuesto 12% de ejemplo).
- Recepción: clic en diagrama agrega marca numerada con tipo seleccionado (Golpe, Rayón, Abolladura, Vidrio); nivel de combustible en 8 segmentos.

## State Management (sugerido, Pinia)
`session` (empresa, sucursal, roles acumulables, permisos), `workOrders` (OT, estado, prioridad, subtareas, timeLogs {origen, ejecutadoPor, registradoPor, desvío, motivo}, hallazgos {severidad, momento, estado, fotos con anotaciones}, repuestos {origen, costo, precio, margen}), `timers` (cronómetro activo por mecánico), `attendance` (marcas), `vehicles`, `customers`, `inventory`, `notifications`. Tiempo real (alertas, «Actualizado en vivo») vía websockets/SSE.

## Assets
- Iconos Phosphor (CDN). Fuentes Google: Barlow Semi Condensed, Inter, JetBrains Mono, Figtree (Mi Garage).
- No hay fotos reales: los recuadros con icono de imagen son marcadores de posición; las siluetas SVG están en `Silueta.dc.html`.

## Files
- `Configuracion.dc.html`
- `Cotizacion.dc.html`
- `Dashboard.dc.html`
- `Design-System.dc.html`
- `Detalle-OT.dc.html`
- `Ficha-Cliente.dc.html`
- `Ficha-Vehiculo.dc.html`
- `Login.dc.html`
- `Mi-Garage.dc.html`
- `Notificaciones.dc.html`
- `Recepcion v1.dc.html`
- `Recepcion.dc.html`
- `Registro-Delegado.dc.html`
- `Reportes.dc.html`
- `Sidebar.dc.html`
- `Silueta.dc.html`
- `Super-Admin.dc.html`
- `TabBar.dc.html`
- `Tablero-Kanban.dc.html`
- `TallerOS.dc.html`
- `Vista-Mecanico.dc.html`
- `support.js` — runtime necesario para abrir los .dc.html en el navegador (no es parte del diseño).
