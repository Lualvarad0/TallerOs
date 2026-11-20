<template>
  <div v-if="carga.cargando.value && !vehiculo" class="p-7 text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>
  <div v-else-if="!vehiculo" class="p-7 text-sm" :style="{ color: 'var(--tx3)' }">No se encontró el vehículo.</div>

  <div v-else class="flex flex-col gap-6 p-7">
    <div class="flex items-start gap-6">
      <div
        class="grid h-28 w-42 flex-shrink-0 place-items-center rounded-xl"
        :style="{ background: 'var(--sf3)', color: 'var(--tx3)' }"
      >
        <PhCarProfile class="text-4xl" />
      </div>
      <div class="flex flex-1 flex-col gap-1.5">
        <div class="flex items-center gap-3">
          <span v-if="vehiculo.placa" class="rounded-md px-2.5 py-0.5 font-mono text-[20px] font-bold" :style="{ border: '2.5px solid var(--tx)' }">
            {{ vehiculo.placa }}
          </span>
          <Badge v-else tono="neutro">Sin placa activa</Badge>
        </div>
        <div class="font-titulo text-[34px] font-bold leading-tight tracking-tight">
          {{ vehiculo.marca }} {{ vehiculo.modelo }} · {{ vehiculo.anio }}
        </div>
        <div class="mt-1 flex gap-7 text-[13.5px]">
          <div v-if="vehiculo.vin"><div class="text-[12px]" :style="{ color: 'var(--tx3)' }">VIN</div><div class="font-mono font-semibold">{{ vehiculo.vin }}</div></div>
          <div v-if="vehiculo.motor"><div class="text-[12px]" :style="{ color: 'var(--tx3)' }">Motor</div><div class="font-semibold">{{ vehiculo.motor }}</div></div>
        </div>
      </div>
    </div>

    <div class="flex flex-col gap-2.5">
      <div class="font-titulo text-xl font-bold">Clientes vinculados</div>
      <div v-if="!vehiculo.clientes.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Sin clientes vinculados en esta empresa.</div>
      <div class="grid grid-cols-3 gap-3">
        <NuxtLink
          v-for="vinculo in vehiculo.clientes"
          :key="vinculo.clienteId"
          :to="`/clientes/${vinculo.clienteId}`"
          class="flex items-center gap-2.5 rounded-xl border p-3.5"
          :style="{ background: 'var(--sf)', borderColor: 'var(--bd)' }"
        >
          <div class="min-w-0 flex-1">
            <div class="font-semibold">{{ vinculo.nombre }}</div>
            <div class="text-[12px]" :style="{ color: 'var(--tx3)' }">{{ etiquetaRol(vinculo.rol) }}</div>
          </div>
        </NuxtLink>
      </div>
    </div>

    <div class="grid grid-cols-3 gap-3.5">
      <SeccionProximamente titulo="Kilometraje">Viene del kilometraje registrado en cada OT (Módulo 4).</SeccionProximamente>
      <SeccionProximamente titulo="Visitas">Viene del historial de órdenes de trabajo (Módulo 4).</SeccionProximamente>
      <SeccionProximamente titulo="Total invertido">Viene de los costos de cada OT (Módulos 4 y 7).</SeccionProximamente>
    </div>

    <div class="grid grid-cols-[minmax(0,1fr)_320px] gap-6">
      <SeccionProximamente titulo="Línea de tiempo de visitas">Viene del historial de órdenes de trabajo (Módulo 4).</SeccionProximamente>
      <div class="flex flex-col gap-3.5">
        <SeccionProximamente titulo="Próximos mantenimientos">Reglas por km/fecha del Módulo 8.</SeccionProximamente>
        <SeccionProximamente titulo="Pendientes recomendados">Hallazgos sin cotizar del Módulo 6.</SeccionProximamente>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhCarProfile } from '@phosphor-icons/vue';
import type { RolClienteVehiculo, VehiculoDetalleDto } from '@talleros/api-client';

const route = useRoute();
const api = useApi();
const carga = useCarga<VehiculoDetalleDto>();
const vehiculo = ref<VehiculoDetalleDto | null>(null);

const etiquetasRol: Record<RolClienteVehiculo, string> = {
  Propietario: 'Propietario',
  ConductorHabitual: 'Conductor habitual',
  ResponsableFlota: 'Responsable de flota',
};
function etiquetaRol(rol: RolClienteVehiculo) {
  return etiquetasRol[rol];
}

onMounted(async () => {
  vehiculo.value = await carga.ejecutar(() => api.vehiculos.obtener(route.params.id as string));
});
</script>
