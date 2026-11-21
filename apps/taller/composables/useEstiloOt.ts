import { PhCaretDoubleUp, PhCaretUp, PhEquals } from '@phosphor-icons/vue';
import type { PrioridadOt } from '@talleros/api-client';

/** Íconos/colores de PrioridadOt y el token de color de un EstadoOt, compartidos entre el
 * Tablero Kanban y el Detalle de OT (ambos muestran las mismas prioridades y estados). */
const iconosPrioridad = { Urgente: PhCaretDoubleUp, Alta: PhCaretUp, Normal: PhEquals };

export function prioridadIcono(p: PrioridadOt) {
  return iconosPrioridad[p];
}

export function prioridadColor(p: PrioridadOt) {
  return p === 'Urgente' ? 'var(--red)' : p === 'Alta' ? 'var(--ac-tx)' : 'var(--tx3)';
}

// "sf3" (Recibido) no tiene variante "-s": es ya un tono neutro de superficie, a diferencia de
// blue/vio/grn que sí vienen en par color+fondo claro (ver docs/diseno/README.md § Design Tokens).
export function tonoEstado(colorToken: string | undefined) {
  if (!colorToken || colorToken === 'sf3') return { texto: 'var(--tx2)', fondo: 'var(--sf3)' };
  return { texto: `var(--${colorToken})`, fondo: `var(--${colorToken}-s)` };
}
