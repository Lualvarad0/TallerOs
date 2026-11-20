<template>
  <div class="flex h-full flex-col gap-4 p-7">
    <div class="flex items-end gap-3">
      <div class="flex-1">
        <div class="font-titulo text-2xl font-bold tracking-tight">Clientes</div>
      </div>
      <CampoTexto v-model="busqueda" placeholder="Buscar por nombre o identificación…" class="w-72" />
      <Boton v-if="!mostrarNuevo" variante="secundario" @click="mostrarNuevo = true"><PhPlus /> Cliente</Boton>
    </div>

    <FormularioCliente v-if="mostrarNuevo" @guardado="onCreado" @cancelar="mostrarNuevo = false" />

    <div v-if="carga.cargando.value && !clientes.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>
    <div v-else-if="!clientes.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Sin clientes todavía.</div>

    <div class="grid grid-cols-3 gap-4">
      <NuxtLink
        v-for="cliente in clientes"
        :key="cliente.id"
        :to="`/clientes/${cliente.id}`"
        class="flex flex-col gap-2 rounded-xl border p-4"
        :style="{ background: 'var(--sf)', borderColor: 'var(--bd)' }"
      >
        <div class="flex items-center gap-2">
          <b class="flex-1 text-[16px]">{{ cliente.nombre }}</b>
          <Badge :tono="cliente.tipo === 'Empresa' ? 'violeta' : 'neutro'">
            {{ cliente.tipo === 'Empresa' ? 'Empresa' : 'Persona' }}
          </Badge>
        </div>
        <div class="font-mono text-[12.5px]" :style="{ color: 'var(--tx3)' }">{{ cliente.identificacion }}</div>
        <div class="flex gap-3 text-[13px]" :style="{ color: 'var(--tx2)' }">
          <span v-if="cliente.telefono">{{ cliente.telefono }}</span>
          <span v-if="cliente.email">{{ cliente.email }}</span>
        </div>
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhPlus } from '@phosphor-icons/vue';
import type { ClienteDto } from '@talleros/api-client';

const api = useApi();
const carga = useCarga<ClienteDto[]>();
const clientes = ref<ClienteDto[]>([]);
const busqueda = ref('');
const mostrarNuevo = ref(false);
let temporizadorBusqueda: ReturnType<typeof setTimeout> | undefined;

async function cargar() {
  const lista = await carga.ejecutar(() => api.clientes.listar(busqueda.value || undefined));
  clientes.value = lista ?? [];
}

// Debounce + cancelación de la petición anterior (useCarga ya descarta una respuesta vieja si
// llega después de que el usuario volvió a escribir).
watch(busqueda, () => {
  clearTimeout(temporizadorBusqueda);
  temporizadorBusqueda = setTimeout(cargar, 300);
});

function onCreado() {
  mostrarNuevo.value = false;
  cargar();
}

onMounted(cargar);
</script>
