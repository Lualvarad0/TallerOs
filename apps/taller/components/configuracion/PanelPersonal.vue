<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-end gap-3">
      <div class="flex-1">
        <div class="font-titulo text-2xl font-bold tracking-tight">Personal</div>
        <div class="mt-1 text-[13.5px]" :style="{ color: 'var(--tx2)' }">
          {{ filas.length }} personas · {{ conAcceso }} con acceso · {{ filas.length - conAcceso }} sin acceso
        </div>
      </div>
      <Boton v-if="!mostrarForm" @click="mostrarForm = true"><PhPlus /> Agregar persona</Boton>
    </div>

    <Tarjeta v-if="mostrarForm" class="gap-3">
      <div class="grid grid-cols-2 gap-3">
        <div class="flex flex-col gap-1.5">
          <label class="text-[13px] font-semibold" :style="{ color: 'var(--tx2)' }">Área</label>
          <select v-model="form.areaId" class="h-11 rounded-lg border px-3 text-[15px]" :style="{ borderColor: 'var(--bd2)', background: 'var(--sf)', color: 'var(--tx)' }">
            <option disabled value="">Elige un área</option>
            <option v-for="a in todasLasAreas" :key="a.id" :value="a.id">{{ a.sucursalNombre }} · {{ a.nombre }}</option>
          </select>
        </div>
        <CampoTexto v-model="form.nombre" label="Nombre" />
        <CampoTexto v-model="form.tarifaHora" label="Tarifa por hora (opcional)" type="number" />
        <label class="flex items-center gap-2 pt-6 text-[14px]">
          <input v-model="form.tieneAcceso" type="checkbox" />
          Tiene acceso a la app
        </label>
      </div>
      <div v-if="carga.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">{{ carga.error.value }}</div>
      <div class="flex gap-2">
        <Boton :disabled="carga.cargando.value" @click="crearPersonal">Guardar</Boton>
        <Boton variante="fantasma" @click="mostrarForm = false">Cancelar</Boton>
      </div>
    </Tarjeta>

    <div v-if="carga.cargando.value && !filas.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>

    <div class="overflow-hidden rounded-xl border" :style="{ borderColor: 'var(--bd)', background: 'var(--sf)' }">
      <div
        v-for="p in filas"
        :key="p.id"
        class="grid grid-cols-[1.4fr_1fr_110px] items-center gap-3 border-b px-4 py-3 last:border-b-0"
        :style="{ borderColor: 'var(--bd)' }"
      >
        <div>
          <div class="text-[15.5px] font-semibold">{{ p.nombre }}</div>
          <div class="text-[12.5px]" :style="{ color: 'var(--tx3)' }">{{ p.areaNombre }} · {{ p.sucursalNombre }}</div>
        </div>
        <span class="text-[13px]" :style="{ color: 'var(--tx2)' }">{{ p.tarifaHora ? `$${p.tarifaHora}/h` : '—' }}</span>
        <Badge :tono="p.tieneAcceso ? 'verde' : 'neutro'">
          <PhCheckCircle v-if="p.tieneAcceso" />
          <PhProhibit v-else />
          {{ p.tieneAcceso ? 'Con acceso' : 'Sin acceso' }}
        </Badge>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhPlus, PhCheckCircle, PhProhibit } from '@phosphor-icons/vue';
import type { AreaDto, PersonalDto, SucursalDto } from '@talleros/api-client';

interface AreaConSucursal extends AreaDto {
  sucursalNombre: string;
}
interface FilaPersonal extends PersonalDto {
  areaNombre: string;
  sucursalNombre: string;
}

const api = useApi();
const carga = useCarga<unknown>();
const filas = ref<FilaPersonal[]>([]);
const todasLasAreas = ref<AreaConSucursal[]>([]);

const conAcceso = computed(() => filas.value.filter((p) => p.tieneAcceso).length);

const mostrarForm = ref(false);
const form = reactive({ areaId: '', nombre: '', tarifaHora: '', tieneAcceso: true });

async function cargarTodo() {
  await carga.ejecutar(async () => {
    const sucursales: SucursalDto[] = await api.sucursales.listar();
    const areasPorSucursal = await Promise.all(
      sucursales.map(async (s) => ({ sucursal: s, areas: await api.areas.listar(s.id) })),
    );

    todasLasAreas.value = areasPorSucursal.flatMap(({ sucursal, areas }) =>
      areas.map((a) => ({ ...a, sucursalNombre: sucursal.nombre })),
    );

    const personalPorArea = await Promise.all(
      todasLasAreas.value.map(async (a) => ({ area: a, personal: await api.personal.listar(a.id) })),
    );

    filas.value = personalPorArea.flatMap(({ area, personal }) =>
      personal.map((p) => ({ ...p, areaNombre: area.nombre, sucursalNombre: area.sucursalNombre })),
    );
    return null;
  });
}

async function crearPersonal() {
  if (!form.areaId || !form.nombre.trim()) return;
  await carga.ejecutar(async () => {
    await api.personal.crear({
      areaId: form.areaId,
      nombre: form.nombre,
      tarifaHora: form.tarifaHora ? Number(form.tarifaHora) : null,
      tieneAcceso: form.tieneAcceso,
      usuarioId: null,
      pin: null,
    });
    form.areaId = '';
    form.nombre = '';
    form.tarifaHora = '';
    form.tieneAcceso = true;
    mostrarForm.value = false;
    return null;
  });
  await cargarTodo();
}

onMounted(cargarTodo);
</script>
