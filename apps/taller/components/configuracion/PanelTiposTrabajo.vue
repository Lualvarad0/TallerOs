<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-end gap-3">
      <div class="flex-1">
        <div class="font-titulo text-2xl font-bold tracking-tight">Tipos de trabajo</div>
        <div class="mt-1 text-[13.5px]" :style="{ color: 'var(--tx2)' }">{{ tipos.length }} tipos registrados</div>
      </div>
      <Boton v-if="!mostrarForm" @click="mostrarForm = true"><PhPlus /> Agregar tipo</Boton>
    </div>

    <Tarjeta v-if="mostrarForm" class="gap-3">
      <div class="grid grid-cols-4 gap-3">
        <CampoTexto v-model="form.nombre" label="Nombre" placeholder="Cambio de aceite" class="col-span-2" />
        <CampoTexto v-model="form.tiempoEstandarMin" label="Tiempo estándar (min)" type="number" />
        <CampoTexto v-model="form.intervaloKm" label="Intervalo (km, opcional)" type="number" />
        <CampoTexto v-model="form.intervaloMeses" label="Intervalo (meses, opcional)" type="number" />
      </div>
      <div v-if="carga.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">{{ carga.error.value }}</div>
      <div class="flex gap-2">
        <Boton :disabled="carga.cargando.value || !puedeGuardar" @click="crear">Guardar</Boton>
        <Boton variante="fantasma" @click="mostrarForm = false">Cancelar</Boton>
      </div>
    </Tarjeta>

    <div v-if="cargaLista.cargando.value && !tipos.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>
    <div v-else-if="!tipos.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Sin tipos de trabajo todavía.</div>

    <div class="overflow-hidden rounded-xl border" :style="{ borderColor: 'var(--bd)', background: 'var(--sf)' }">
      <div
        v-for="t in tipos"
        :key="t.id"
        class="grid grid-cols-[1.4fr_140px_1fr] items-center gap-3 border-b px-4 py-3 last:border-b-0"
        :style="{ borderColor: 'var(--bd)' }"
      >
        <span class="text-[15.5px] font-semibold">{{ t.nombre }}</span>
        <span class="font-mono text-[13px]" :style="{ color: 'var(--tx2)' }">{{ t.tiempoEstandarMin }} min</span>
        <span class="text-[13px]" :style="{ color: 'var(--tx3)' }">
          <template v-if="t.intervaloKm != null || t.intervaloMeses != null">
            Cada {{ [t.intervaloKm != null ? `${t.intervaloKm.toLocaleString()} km` : null, t.intervaloMeses != null ? `${t.intervaloMeses} meses` : null].filter(Boolean).join(' / ') }}
          </template>
          <template v-else>Sin periodicidad</template>
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhPlus } from '@phosphor-icons/vue';
import type { TipoTrabajoDto } from '@talleros/api-client';

const api = useApi();
const cargaLista = useCarga<TipoTrabajoDto[]>();
const carga = useCarga<unknown>();
const tipos = ref<TipoTrabajoDto[]>([]);
const mostrarForm = ref(false);
const form = reactive({ nombre: '', tiempoEstandarMin: '', intervaloKm: '', intervaloMeses: '' });

const puedeGuardar = computed(() => !!form.nombre.trim() && Number(form.tiempoEstandarMin) > 0);

async function cargar() {
  tipos.value = (await cargaLista.ejecutar(() => api.tiposTrabajo.listar())) ?? [];
}

async function crear() {
  const resultado = await carga.ejecutar(() =>
    api.tiposTrabajo.crear({
      nombre: form.nombre.trim(),
      tiempoEstandarMin: Number(form.tiempoEstandarMin),
      intervaloKm: form.intervaloKm ? Number(form.intervaloKm) : null,
      intervaloMeses: form.intervaloMeses ? Number(form.intervaloMeses) : null,
    }),
  );
  if (resultado) {
    form.nombre = '';
    form.tiempoEstandarMin = '';
    form.intervaloKm = '';
    form.intervaloMeses = '';
    mostrarForm.value = false;
    await cargar();
  }
}

onMounted(cargar);
</script>
