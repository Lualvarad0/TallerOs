<template>
  <div v-if="carga.cargando.value && !ot" class="p-7 text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>
  <div v-else-if="!ot" class="p-7 text-sm" :style="{ color: 'var(--tx3)' }">No se encontró la orden de trabajo.</div>

  <div v-else class="grid grid-cols-[minmax(0,1fr)_320px] gap-6 p-7">
    <div class="flex flex-col gap-6">
      <div class="flex items-start gap-4">
        <div class="flex-1">
          <div class="flex items-center gap-2.5">
            <span class="rounded px-2 py-0.5 font-mono text-[16px] font-bold" :style="{ border: '2px solid var(--tx)' }">{{ vehiculo?.placa ?? 's/placa' }}</span>
            <span class="font-mono text-[13px]" :style="{ color: 'var(--tx3)' }">OT-{{ ot.numero }}</span>
            <span class="flex items-center gap-1 text-[13px] font-bold" :style="{ color: prioridadColor(ot.prioridad) }">
              <component :is="prioridadIcono(ot.prioridad)" weight="bold" />{{ ot.prioridad }}
            </span>
          </div>
          <div class="font-titulo mt-1 text-[30px] font-bold tracking-tight leading-tight">
            {{ vehiculo ? `${vehiculo.marca} ${vehiculo.modelo} ${vehiculo.anio}` : 'Cargando vehículo…' }}
          </div>
          <div class="mt-1 text-[14.5px]" :style="{ color: 'var(--tx2)' }">
            Ingresó {{ formatearFecha(ot.ingresoEn) }}<span v-if="ot.entregadoEn"> · Entregado {{ formatearFecha(ot.entregadoEn) }}</span>
          </div>
        </div>
        <div class="relative">
          <button
            class="flex h-9 items-center gap-2 rounded-lg px-3.5 text-[14px] font-bold"
            :style="{ background: tonoEstado(estadoActual?.colorToken).fondo, color: tonoEstado(estadoActual?.colorToken).texto }"
            @click="mostrarCambioEstado = !mostrarCambioEstado"
          >
            {{ ot.estadoNombre }}<PhCaretDown />
          </button>
          <div
            v-if="mostrarCambioEstado"
            class="absolute right-0 top-10 z-10 flex w-52 flex-col gap-0.5 rounded-lg border p-1.5"
            :style="{ background: 'var(--sf)', borderColor: 'var(--bd2)', boxShadow: 'var(--sh)' }"
          >
            <button
              v-for="e in estados"
              :key="e.id"
              class="rounded-md px-2.5 py-1.5 text-left text-[13.5px]"
              :style="e.id === ot.estadoId ? { background: 'var(--sf3)', fontWeight: 700 } : {}"
              @click="cambiarEstado(e.id)"
            >
              {{ e.nombre }}
            </button>
          </div>
        </div>
      </div>

      <Tarjeta v-if="ot.recepcion" class="gap-3">
        <div class="font-titulo text-lg font-bold">Recepción</div>
        <div class="grid grid-cols-3 gap-3 text-[14px]">
          <div><span :style="{ color: 'var(--tx3)' }">Km de ingreso</span><div class="font-mono font-bold">{{ ot.kmIngreso.toLocaleString() }}</div></div>
          <div><span :style="{ color: 'var(--tx3)' }">Combustible</span><div class="font-bold">{{ etiquetaCombustible(ot.recepcion.combustible) }}</div></div>
          <div><span :style="{ color: 'var(--tx3)' }">Daños previos</span><div class="font-bold">{{ ot.recepcion.daniosPrevios.length }}</div></div>
        </div>
        <div v-if="ot.recepcion.observaciones" class="text-[14px]"><span :style="{ color: 'var(--tx3)' }">Motivo:</span> {{ ot.recepcion.observaciones }}</div>
        <div v-if="ot.recepcion.objetos" class="text-[14px]"><span :style="{ color: 'var(--tx3)' }">Objetos dentro:</span> {{ ot.recepcion.objetos }}</div>
        <div v-if="ot.recepcion.daniosPrevios.length" class="flex flex-col gap-1.5">
          <div v-for="d in ot.recepcion.daniosPrevios" :key="d.id" class="flex items-center gap-2 text-[13.5px]">
            <b>{{ d.tipo }}</b><span :style="{ color: 'var(--tx3)' }">{{ d.zona }}</span><span v-if="d.descripcion" :style="{ color: 'var(--tx2)' }">· {{ d.descripcion }}</span>
          </div>
        </div>
      </Tarjeta>

      <div class="flex flex-col gap-2.5">
        <div class="flex items-center gap-2">
          <div class="font-titulo text-lg font-bold">Subtareas</div>
          <span class="font-mono text-[13px]" :style="{ color: 'var(--tx3)' }">{{ hechas }}/{{ ot.subtareas.length }}</span>
          <div class="flex-1" />
          <Boton v-if="!mostrarNuevaSubtarea" variante="secundario" @click="mostrarNuevaSubtarea = true"><PhPlus />Subtarea</Boton>
        </div>

        <Tarjeta v-if="mostrarNuevaSubtarea" class="gap-2.5">
          <div class="flex items-end gap-2">
            <CampoSelect
              v-model="nuevaSubtarea.tipoTrabajoId"
              label="Tipo de trabajo"
              placeholder="Elegir…"
              class="flex-1"
              :opciones="tiposTrabajo.map((t) => ({ valor: t.id, etiqueta: t.nombre }))"
              @update:model-value="onElegirTipoTrabajo"
            />
            <CampoTexto v-model="nuevaSubtarea.tiempoEstimadoMin" label="Tiempo est. (min)" type="number" class="w-40" />
            <CampoSelect
              v-model="nuevaSubtarea.responsablePersonalId"
              label="Responsable (opcional)"
              placeholder="Sin asignar"
              class="w-52"
              :opciones="personal.map((p) => ({ valor: p.id, etiqueta: p.nombre }))"
            />
          </div>
          <div class="flex gap-2">
            <Boton :disabled="creandoSubtarea.cargando.value || !nuevaSubtarea.tipoTrabajoId" @click="crearSubtarea">Guardar</Boton>
            <Boton variante="fantasma" @click="mostrarNuevaSubtarea = false">Cancelar</Boton>
          </div>
        </Tarjeta>

        <div v-if="!ot.subtareas.length" class="text-[13.5px]" :style="{ color: 'var(--tx3)' }">Sin subtareas todavía.</div>
        <div v-else class="overflow-hidden rounded-lg border" :style="{ borderColor: 'var(--bd)', background: 'var(--sf)' }">
          <div v-for="s in ot.subtareas" :key="s.id" class="flex items-center gap-3 border-b px-3.5 py-2.5 last:border-b-0" :style="{ borderColor: 'var(--bd)' }">
            <div class="flex-1 text-[14.5px] font-semibold">{{ s.tipoTrabajoNombre }}</div>
            <div class="font-mono text-[12.5px]" :style="{ color: 'var(--tx3)' }">{{ s.tiempoEstimadoMin }} min</div>
            <CampoSelect
              :model-value="s.responsablePersonalId ?? ''"
              placeholder="Sin asignar"
              class="w-44"
              :opciones="personal.map((p) => ({ valor: p.id, etiqueta: p.nombre }))"
              @update:model-value="(v) => actualizarSubtarea(s, v || null, s.estado)"
            />
            <div class="flex gap-1">
              <button
                v-for="opcion in (['Pendiente', 'EnCurso', 'Hecha'] as const)"
                :key="opcion"
                class="rounded-md px-2 py-1 text-[12px] font-bold"
                :style="s.estado === opcion ? { background: 'var(--ac)', color: 'var(--ac-ink)' } : { background: 'var(--sf3)', color: 'var(--tx2)' }"
                @click="actualizarSubtarea(s, s.responsablePersonalId, opcion)"
              >
                {{ opcion === 'EnCurso' ? 'En curso' : opcion }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="flex flex-col gap-2">
        <div class="font-titulo text-lg font-bold">Historial</div>
        <div v-for="(h, i) in ot.historial" :key="i" class="flex items-center gap-3 text-[13.5px]">
          <span class="font-mono" :style="{ color: 'var(--tx3)' }">{{ formatearFecha(h.fecha) }}</span>
          <span>→ <b>{{ h.aEstadoNombre }}</b></span>
        </div>
      </div>
    </div>

    <div class="flex flex-col gap-4">
      <NuxtLink :to="`/vehiculos/${ot.vehiculoId}`" class="flex flex-col gap-1.5 rounded-lg border p-3.5" :style="{ borderColor: 'var(--bd)' }">
        <div class="text-[11px] font-bold uppercase tracking-wider" :style="{ color: 'var(--tx3)' }">Vehículo</div>
        <div class="text-[15px] font-bold">{{ vehiculo ? `${vehiculo.marca} ${vehiculo.modelo} ${vehiculo.anio}` : '—' }}</div>
        <div v-if="vehiculo?.vin" class="font-mono text-[12px]" :style="{ color: 'var(--tx3)' }">{{ vehiculo.vin }}</div>
      </NuxtLink>
      <NuxtLink :to="`/clientes/${ot.clienteId}`" class="flex flex-col gap-1.5 rounded-lg border p-3.5" :style="{ borderColor: 'var(--bd)' }">
        <div class="text-[11px] font-bold uppercase tracking-wider" :style="{ color: 'var(--tx3)' }">Cliente</div>
        <div class="text-[15px] font-bold">{{ ot.clienteNombre }}</div>
      </NuxtLink>

      <SeccionProximamente titulo="Hallazgos y repuestos">
        Hallazgos (Módulo 6) e inventario/repuestos (Módulo 7) todavía no existen.
      </SeccionProximamente>
      <SeccionProximamente titulo="Registro de tiempos">
        Cronómetro y jornada del mecánico son del Módulo 5.
      </SeccionProximamente>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhCaretDown, PhPlus } from '@phosphor-icons/vue';
import type { EstadoOtDto, EstadoSubtarea, OrdenTrabajoDetalleDto, PersonalDto, SubtareaDto, TipoTrabajoDto, VehiculoDetalleDto } from '@talleros/api-client';

const route = useRoute();
const api = useApi();
const carga = useCarga<OrdenTrabajoDetalleDto>();
const personalDeLaSucursal = usePersonalDeSucursal();
const ot = ref<OrdenTrabajoDetalleDto | null>(null);
const vehiculo = ref<VehiculoDetalleDto | null>(null);
const estados = ref<EstadoOtDto[]>([]);
const tiposTrabajo = ref<TipoTrabajoDto[]>([]);
const personal = ref<PersonalDto[]>([]);
const mostrarCambioEstado = ref(false);
const mostrarNuevaSubtarea = ref(false);

const estadoActual = computed(() => estados.value.find((e) => e.id === ot.value?.estadoId));
const hechas = computed(() => ot.value?.subtareas.filter((s) => s.estado === 'Hecha').length ?? 0);

function formatearFecha(iso: string) {
  return new Date(iso).toLocaleString('es', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
}

function etiquetaCombustible(octavos: number) {
  return ['Vacío', '1/8', '1/4', '3/8', '1/2', '5/8', '3/4', '7/8', 'Lleno'][octavos] ?? `${octavos}/8`;
}

async function cargar() {
  const id = route.params.id as string;
  ot.value = await carga.ejecutar(() => api.ordenesTrabajo.obtener(id));
  if (ot.value) {
    vehiculo.value = await api.vehiculos.obtener(ot.value.vehiculoId);
    personal.value = await personalDeLaSucursal.listar(ot.value.sucursalId);
  }
}

async function cambiarEstado(nuevoEstadoId: string) {
  mostrarCambioEstado.value = false;
  if (!ot.value || nuevoEstadoId === ot.value.estadoId) return;
  await api.ordenesTrabajo.cambiarEstado(ot.value.id, nuevoEstadoId);
  await cargar();
}

const nuevaSubtarea = reactive({ tipoTrabajoId: '', tiempoEstimadoMin: '', responsablePersonalId: '' });
const creandoSubtarea = useCarga<unknown>();

function onElegirTipoTrabajo(tipoTrabajoId: string) {
  const tipo = tiposTrabajo.value.find((t) => t.id === tipoTrabajoId);
  if (tipo) nuevaSubtarea.tiempoEstimadoMin = String(tipo.tiempoEstandarMin);
}

async function crearSubtarea() {
  if (!ot.value || !nuevaSubtarea.tipoTrabajoId) return;
  await creandoSubtarea.ejecutar(() =>
    api.ordenesTrabajo.crearSubtarea(ot.value!.id, {
      tipoTrabajoId: nuevaSubtarea.tipoTrabajoId,
      responsablePersonalId: nuevaSubtarea.responsablePersonalId || null,
      tiempoEstimadoMin: Number(nuevaSubtarea.tiempoEstimadoMin) || 0,
      orden: ot.value!.subtareas.length + 1,
    }),
  );
  nuevaSubtarea.tipoTrabajoId = '';
  nuevaSubtarea.tiempoEstimadoMin = '';
  nuevaSubtarea.responsablePersonalId = '';
  mostrarNuevaSubtarea.value = false;
  await cargar();
}

async function actualizarSubtarea(s: SubtareaDto, responsablePersonalId: string | null, estado: EstadoSubtarea) {
  await api.subtareas.actualizar(s.id, { responsablePersonalId, estado });
  await cargar();
}

onMounted(async () => {
  [estados.value, tiposTrabajo.value] = await Promise.all([api.estadosOt.listar(), api.tiposTrabajo.listar()]);
  await cargar();
});
</script>
