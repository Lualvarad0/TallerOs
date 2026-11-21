<template>
  <div class="flex h-full flex-col">
    <div class="flex h-16 items-center gap-3 border-b px-6" :style="{ borderColor: 'var(--bd)' }">
      <div class="font-titulo text-2xl font-bold tracking-tight">Tablero de OT</div>
      <div class="font-mono text-[13px]" :style="{ color: 'var(--tx3)' }">{{ ordenes.length }} activas</div>
      <div class="ml-2 flex gap-1 rounded-lg border p-0.5" :style="{ background: 'var(--sf2)', borderColor: 'var(--bd)' }">
        <button
          class="flex h-7 items-center gap-1.5 rounded-md px-3 text-[13px] font-semibold"
          :style="vista === 'tablero' ? { background: 'var(--sf)' } : {}"
          @click="vista = 'tablero'"
        >
          <PhKanban />Tablero
        </button>
        <button
          class="flex h-7 items-center gap-1.5 rounded-md px-3 text-[13px] font-semibold"
          :style="vista === 'tabla' ? { background: 'var(--sf)' } : {}"
          @click="vista = 'tabla'"
        >
          <PhRows />Tabla
        </button>
      </div>
      <div class="flex-1" />
      <CampoSelect
        v-model="filtroSucursalId"
        placeholder="Todas las sucursales"
        class="w-44"
        :opciones="sucursales.map((s) => ({ valor: s.id, etiqueta: s.nombre }))"
        @update:model-value="cargar"
      />
      <CampoSelect
        v-model="filtroPrioridad"
        placeholder="Toda prioridad"
        class="w-36"
        :opciones="[
          { valor: 'Normal', etiqueta: 'Normal' },
          { valor: 'Alta', etiqueta: 'Alta' },
          { valor: 'Urgente', etiqueta: 'Urgente' },
        ]"
        @update:model-value="cargar"
      />
      <Boton @click="navigateTo('/recepcion')"><PhPlus />Nueva OT</Boton>
    </div>

    <div v-if="carga.cargando.value && !ordenes.length" class="p-7 text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>

    <div v-else-if="vista === 'tablero'" class="flex min-h-0 flex-1 gap-3 overflow-x-auto p-5">
      <div
        v-for="col in columnas"
        :key="col.estadoId"
        class="flex w-[270px] flex-shrink-0 flex-col gap-2 rounded-lg p-2"
        :style="{ background: colEnArrastre === col.estadoId ? 'var(--ac-s)' : 'var(--sf2)' }"
        @dragover.prevent="colEnArrastre = col.estadoId"
        @dragleave="colEnArrastre = null"
        @drop="soltarEn(col.estadoId)"
      >
        <div class="flex items-center gap-2 px-1 py-1">
          <span class="h-2 w-2 rounded-sm" :style="{ background: tonoEstado(col.colorToken).texto }" />
          <span class="text-[13.5px] font-bold">{{ col.estadoNombre }}</span>
          <span class="font-mono text-[12px]" :style="{ color: 'var(--tx3)' }">{{ col.cards.length }}</span>
        </div>

        <NuxtLink
          v-for="o in col.cards"
          :key="o.otId"
          :to="`/ordenes-trabajo/${o.otId}`"
          draggable="true"
          class="flex flex-col gap-2 rounded-lg border p-3"
          :style="{ background: 'var(--sf)', borderColor: 'var(--bd)', borderLeft: `3px solid ${tonoEstado(col.colorToken).texto}` }"
          @dragstart="otArrastrada = o.otId"
        >
          <div class="flex items-center gap-2">
            <span class="rounded px-1.5 py-0.5 font-mono text-[12.5px] font-bold" :style="{ border: '1.5px solid var(--tx)' }">{{ o.placa ?? 's/placa' }}</span>
            <span class="font-mono text-[11.5px]" :style="{ color: 'var(--tx3)' }">OT-{{ o.numero }}</span>
            <div class="flex-1" />
            <component :is="prioridadIcono(o.prioridad)" weight="bold" :style="{ color: prioridadColor(o.prioridad) }" />
          </div>
          <div class="text-[13.5px] font-semibold">{{ o.clienteNombre }}</div>
          <div class="flex items-center gap-1.5 font-mono text-[12px]" :style="{ color: 'var(--tx2)' }">
            <PhClock />{{ transcurrido(o.ingresoEn) }}
          </div>
        </NuxtLink>
      </div>
    </div>

    <div v-else class="flex-1 overflow-auto px-6">
      <div class="grid grid-cols-[110px_90px_1.2fr_170px_110px] text-[13.5px]">
        <div class="contents text-[12px] font-bold uppercase" :style="{ color: 'var(--tx3)' }">
          <div class="border-b px-2 py-3" :style="{ borderColor: 'var(--bd)' }">Placa</div>
          <div class="border-b px-2 py-3" :style="{ borderColor: 'var(--bd)' }">OT</div>
          <div class="border-b px-2 py-3" :style="{ borderColor: 'var(--bd)' }">Cliente</div>
          <div class="border-b px-2 py-3" :style="{ borderColor: 'var(--bd)' }">Estado</div>
          <div class="border-b px-2 py-3" :style="{ borderColor: 'var(--bd)' }">Ingreso</div>
        </div>
        <template v-for="o in ordenes" :key="o.otId">
          <div class="contents">
            <div class="border-b px-2 py-2.5" :style="{ borderColor: 'var(--bd)' }">
              <span class="rounded px-1.5 py-0.5 font-mono text-[12.5px] font-bold" :style="{ border: '1.5px solid var(--tx)' }">{{ o.placa ?? '—' }}</span>
            </div>
            <div class="border-b px-2 py-2.5 font-mono text-[12.5px]" :style="{ borderColor: 'var(--bd)', color: 'var(--tx2)' }">OT-{{ o.numero }}</div>
            <NuxtLink :to="`/ordenes-trabajo/${o.otId}`" class="border-b px-2 py-2.5" :style="{ borderColor: 'var(--bd)' }">{{ o.clienteNombre }}</NuxtLink>
            <div class="border-b px-2 py-2" :style="{ borderColor: 'var(--bd)' }">
              <span
                class="inline-flex h-[22px] items-center rounded px-2 text-[12px] font-bold"
                :style="{ background: tonoEstado(o.colorToken).fondo, color: tonoEstado(o.colorToken).texto }"
                >{{ o.estadoNombre }}</span
              >
            </div>
            <div class="border-b px-2 py-2.5 font-mono text-[12.5px]" :style="{ borderColor: 'var(--bd)' }">{{ transcurrido(o.ingresoEn) }}</div>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhCaretDoubleUp, PhCaretUp, PhClock, PhEquals, PhKanban, PhPlus, PhRows } from '@phosphor-icons/vue';
import type { EstadoOtDto, PrioridadOt, SucursalDto, TableroOtDto } from '@talleros/api-client';

const api = useApi();
const carga = useCarga<TableroOtDto[]>();
const ordenes = ref<TableroOtDto[]>([]);
const sucursales = ref<SucursalDto[]>([]);
const vista = ref<'tablero' | 'tabla'>('tablero');
const filtroSucursalId = ref('');
const filtroPrioridad = ref<PrioridadOt | ''>('');
const otArrastrada = ref<string | null>(null);
const colEnArrastre = ref<string | null>(null);

// Columnas = el catálogo de estados de la empresa (no solo los estados que ya tienen alguna OT):
// una empresa nueva, sin OT todavía, debe ver sus 8 columnas vacías, no un tablero en blanco. Los
// estados son configurables por empresa (ver EstadoOt.cs), por eso no hay un mapa fijo aquí.
const estados = ref<EstadoOtDto[]>([]);
const columnas = computed(() =>
  estados.value.map((e) => ({
    estadoId: e.id,
    estadoNombre: e.nombre,
    colorToken: e.colorToken,
    cards: ordenes.value.filter((o) => o.estadoId === e.id),
  })),
);

// "sf3" (Recibido) no tiene variante "-s": es ya un tono neutro de superficie, a diferencia de
// blue/vio/grn que sí vienen en par color+fondo claro (ver docs/diseno/README.md § Design Tokens).
function tonoEstado(colorToken: string) {
  if (colorToken === 'sf3') return { texto: 'var(--tx2)', fondo: 'var(--sf3)' };
  return { texto: `var(--${colorToken})`, fondo: `var(--${colorToken}-s)` };
}

const iconosPrioridad = { Urgente: PhCaretDoubleUp, Alta: PhCaretUp, Normal: PhEquals };
function prioridadIcono(p: PrioridadOt) {
  return iconosPrioridad[p];
}
function prioridadColor(p: PrioridadOt) {
  return p === 'Urgente' ? 'var(--red)' : p === 'Alta' ? 'var(--ac-tx)' : 'var(--tx3)';
}

function transcurrido(ingresoEn: string) {
  const ms = Date.now() - new Date(ingresoEn).getTime();
  const minutos = Math.floor(ms / 60000);
  if (minutos < 60) return `${minutos}m`;
  const horas = Math.floor(minutos / 60);
  if (horas < 24) return `${horas}h ${minutos % 60}m`;
  return `${Math.floor(horas / 24)}d ${horas % 24}h`;
}

async function cargar() {
  const lista = await carga.ejecutar(() =>
    api.ordenesTrabajo.listarTablero({
      sucursalId: filtroSucursalId.value || undefined,
      prioridad: filtroPrioridad.value || undefined,
    }),
  );
  ordenes.value = lista ?? [];
}

async function soltarEn(nuevoEstadoId: string) {
  colEnArrastre.value = null;
  const otId = otArrastrada.value;
  otArrastrada.value = null;
  if (!otId) return;
  const orden = ordenes.value.find((o) => o.otId === otId);
  if (!orden || orden.estadoId === nuevoEstadoId) return;

  const anterior = { estadoId: orden.estadoId, estadoNombre: orden.estadoNombre, colorToken: orden.colorToken };
  const nuevoEstado = estados.value.find((e) => e.id === nuevoEstadoId);
  orden.estadoId = nuevoEstadoId;
  if (nuevoEstado) {
    orden.estadoNombre = nuevoEstado.nombre;
    orden.colorToken = nuevoEstado.colorToken;
  }
  try {
    await api.ordenesTrabajo.cambiarEstado(otId, nuevoEstadoId);
  } catch {
    // Nada cambió en el servidor: revertimos la tarjeta localmente en vez de recargar todo el
    // tablero (cargar() deja ordenes.value en [] si el refetch también falla, ver code review).
    orden.estadoId = anterior.estadoId;
    orden.estadoNombre = anterior.estadoNombre;
    orden.colorToken = anterior.colorToken;
  }
}

onMounted(async () => {
  sucursales.value = await api.sucursales.listar();
  estados.value = await api.estadosOt.listar();
  await cargar();
});
</script>
