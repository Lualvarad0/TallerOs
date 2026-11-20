<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-end gap-3">
      <div class="flex-1">
        <div class="font-titulo text-2xl font-bold tracking-tight">Dispositivos tablet</div>
        <div class="mt-1 text-[13.5px]" :style="{ color: 'var(--tx2)' }">Tablets registradas para el modo taller (PIN).</div>
      </div>
      <Boton v-if="!mostrarForm" @click="mostrarForm = true"><PhPlus /> Registrar dispositivo</Boton>
    </div>

    <Tarjeta v-if="mostrarForm" class="gap-3">
      <div class="grid grid-cols-2 gap-3">
        <div class="flex flex-col gap-1.5">
          <label class="text-[13px] font-semibold" :style="{ color: 'var(--tx2)' }">Sucursal</label>
          <select v-model="form.sucursalId" class="h-11 rounded-lg border px-3 text-[15px]" :style="{ borderColor: 'var(--bd2)', background: 'var(--sf)', color: 'var(--tx)' }">
            <option disabled value="">Elige una sucursal</option>
            <option v-for="s in sucursales" :key="s.id" :value="s.id">{{ s.nombre }}</option>
          </select>
        </div>
        <CampoTexto v-model="form.nombre" label="Nombre del dispositivo" placeholder="Tablet recepción" />
      </div>
      <div v-if="carga.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">{{ carga.error.value }}</div>
      <div class="flex gap-2">
        <Boton :disabled="carga.cargando.value" @click="registrar">Registrar</Boton>
        <Boton variante="fantasma" @click="mostrarForm = false">Cancelar</Boton>
      </div>
    </Tarjeta>

    <Tarjeta v-if="tokenNuevo" class="gap-2" :style="{ borderColor: 'var(--ac)' }">
      <div class="text-[13.5px] font-semibold">Token del dispositivo (solo se muestra una vez):</div>
      <div
        class="rounded-md border px-3 py-2 font-mono text-[13.5px]"
        :style="{ borderColor: 'var(--bd)', background: 'var(--sf2)' }"
      >
        {{ tokenNuevo }}
      </div>
    </Tarjeta>

    <div v-if="carga.cargando.value && !dispositivos.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>

    <div class="overflow-hidden rounded-xl border" :style="{ borderColor: 'var(--bd)', background: 'var(--sf)' }">
      <div
        v-for="d in dispositivos"
        :key="d.id"
        class="flex items-center gap-3 border-b px-4 py-3 last:border-b-0"
        :style="{ borderColor: 'var(--bd)' }"
      >
        <PhDeviceTablet class="text-lg" :style="{ color: 'var(--tx3)' }" />
        <div class="min-w-0 flex-1">
          <div class="text-[15px] font-semibold">{{ d.nombre }}</div>
          <div class="text-[12.5px]" :style="{ color: 'var(--tx3)' }">{{ nombreSucursal(d.sucursalId) }}</div>
        </div>
        <Badge :tono="d.activo ? 'verde' : 'neutro'">{{ d.activo ? 'Activo' : 'Revocado' }}</Badge>
        <Boton v-if="d.activo" variante="fantasma" :disabled="revocando === d.id" @click="revocar(d.id)">Revocar</Boton>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhPlus, PhDeviceTablet } from '@phosphor-icons/vue';
import type { DispositivoTabletDto, SucursalDto } from '@talleros/api-client';

const api = useApi();
const carga = useCarga<unknown>();
const dispositivos = ref<DispositivoTabletDto[]>([]);
const sucursales = ref<SucursalDto[]>([]);
const tokenNuevo = ref<string | null>(null);
const revocando = ref<string | null>(null);

const mostrarForm = ref(false);
const form = reactive({ sucursalId: '', nombre: '' });

function nombreSucursal(sucursalId: string) {
  return sucursales.value.find((s) => s.id === sucursalId)?.nombre ?? '—';
}

async function cargarTodo() {
  await carga.ejecutar(async () => {
    [dispositivos.value, sucursales.value] = await Promise.all([api.dispositivosTablet.listar(), api.sucursales.listar()]);
    return null;
  });
}

async function registrar() {
  if (!form.sucursalId || !form.nombre.trim()) return;
  await carga.ejecutar(async () => {
    const resultado = await api.dispositivosTablet.registrar(form.sucursalId, form.nombre);
    tokenNuevo.value = resultado.token;
    form.sucursalId = '';
    form.nombre = '';
    mostrarForm.value = false;
    return null;
  });
  await cargarTodo();
}

async function revocar(dispositivoId: string) {
  revocando.value = dispositivoId;
  try {
    await api.dispositivosTablet.revocar(dispositivoId);
    await cargarTodo();
  } finally {
    revocando.value = null;
  }
}

onMounted(cargarTodo);
</script>
