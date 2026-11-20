<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-end gap-3">
      <div class="flex-1">
        <div class="font-titulo text-2xl font-bold tracking-tight">Sucursales y áreas</div>
      </div>
      <Boton v-if="!mostrarNuevaSucursal" variante="secundario" @click="mostrarNuevaSucursal = true">
        <PhPlus /> Sucursal
      </Boton>
    </div>

    <Tarjeta v-if="mostrarNuevaSucursal" class="gap-3">
      <div class="flex gap-3">
        <CampoTexto v-model="nuevaSucursal.nombre" label="Nombre" placeholder="Sucursal Norte" class="flex-1" />
        <CampoTexto v-model="nuevaSucursal.direccion" label="Dirección (opcional)" class="flex-1" />
      </div>
      <div v-if="carga.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">{{ carga.error.value }}</div>
      <div class="flex gap-2">
        <Boton :disabled="carga.cargando.value" @click="crearSucursal">Guardar</Boton>
        <Boton variante="fantasma" @click="mostrarNuevaSucursal = false">Cancelar</Boton>
      </div>
    </Tarjeta>

    <div v-if="carga.cargando.value && !sucursales.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>

    <div class="grid grid-cols-2 gap-4">
      <Tarjeta v-for="sucursal in sucursales" :key="sucursal.id">
        <div class="flex items-center gap-2">
          <PhMapPin class="text-lg" />
          <b class="flex-1 text-base">{{ sucursal.nombre }}</b>
          <span class="text-[12.5px]" :style="{ color: 'var(--tx3)' }">{{ sucursal.direccion }}</span>
        </div>
        <div class="flex flex-wrap gap-1.5">
          <span
            v-for="area in areasPorSucursal[sucursal.id] ?? []"
            :key="area.id"
            class="flex h-[30px] items-center gap-1.5 rounded-md border px-2.5 text-[13px] font-semibold"
            :style="{ borderColor: 'var(--bd2)' }"
          >
            {{ area.nombre }}
          </span>
          <button
            type="button"
            class="flex h-[30px] items-center gap-1 rounded-md border border-dashed px-2.5 text-[13px]"
            :style="{ borderColor: 'var(--bd2)', color: 'var(--tx3)' }"
            @click="nuevaArea.sucursalId = sucursal.id"
          >
            <PhPlus /> Área
          </button>
        </div>
        <div v-if="nuevaArea.sucursalId === sucursal.id" class="flex gap-2">
          <CampoTexto v-model="nuevaArea.nombre" placeholder="Nombre del área" class="flex-1" />
          <Boton variante="secundario" @click="crearArea(sucursal.id)">Agregar</Boton>
        </div>
      </Tarjeta>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhPlus, PhMapPin } from '@phosphor-icons/vue';
import type { AreaDto, SucursalDto } from '@talleros/api-client';

const api = useApi();
const carga = useCarga<SucursalDto[]>();
const sucursales = ref<SucursalDto[]>([]);
const areasPorSucursal = ref<Record<string, AreaDto[]>>({});

const mostrarNuevaSucursal = ref(false);
const nuevaSucursal = reactive({ nombre: '', direccion: '' });
const nuevaArea = reactive({ sucursalId: null as string | null, nombre: '' });

async function cargarTodo() {
  const lista = await carga.ejecutar(() => api.sucursales.listar());
  sucursales.value = lista ?? [];
  const entradas = await Promise.all(
    sucursales.value.map(async (s) => [s.id, await api.areas.listar(s.id)] as const),
  );
  areasPorSucursal.value = Object.fromEntries(entradas);
}

async function crearSucursal() {
  await carga.ejecutar(async () => {
    await api.sucursales.crear(nuevaSucursal.nombre, nuevaSucursal.direccion || null);
    nuevaSucursal.nombre = '';
    nuevaSucursal.direccion = '';
    mostrarNuevaSucursal.value = false;
    return [];
  });
  await cargarTodo();
}

async function crearArea(sucursalId: string) {
  if (!nuevaArea.nombre.trim()) return;
  await api.areas.crear(sucursalId, nuevaArea.nombre);
  nuevaArea.sucursalId = null;
  nuevaArea.nombre = '';
  await cargarTodo();
}

onMounted(cargarTodo);
</script>
