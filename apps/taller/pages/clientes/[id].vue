<template>
  <div v-if="carga.cargando.value && !cliente" class="p-7 text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>
  <div v-else-if="!cliente" class="p-7 text-sm" :style="{ color: 'var(--tx3)' }">No se encontró el cliente.</div>

  <div v-else class="grid grid-cols-[340px_minmax(0,1fr)] gap-6 p-7">
    <div class="flex flex-col gap-4">
      <FormularioCliente v-if="editando" :cliente="cliente" @guardado="onEditado" @cancelar="editando = false" />

      <Tarjeta v-else class="gap-3.5">
        <div
          class="font-titulo grid h-16 w-16 place-items-center rounded-full text-[22px] font-bold"
          :style="{ background: 'var(--sf3)' }"
        >
          {{ iniciales }}
        </div>
        <div>
          <div class="font-titulo text-[26px] font-bold tracking-tight">{{ cliente.nombre }}</div>
          <div class="text-[13.5px]" :style="{ color: 'var(--tx3)' }">
            {{ cliente.tipo === 'Empresa' ? 'Empresa' : 'Persona natural' }}
          </div>
        </div>
        <div class="grid grid-cols-[20px_1fr] items-center gap-2.5 text-[15px]">
          <PhIdentificationCard :style="{ color: 'var(--tx3)' }" />
          <span class="font-mono">{{ cliente.identificacion }}</span>
          <template v-if="cliente.telefono">
            <PhPhone :style="{ color: 'var(--tx3)' }" />
            <span class="font-mono">{{ cliente.telefono }}</span>
          </template>
          <template v-if="cliente.email">
            <PhEnvelopeSimple :style="{ color: 'var(--tx3)' }" />
            <span>{{ cliente.email }}</span>
          </template>
        </div>
        <Badge :tono="cliente.canalPreferido === 'WhatsApp' ? 'verde' : 'azul'">
          Prefiere avisos por {{ cliente.canalPreferido }}
        </Badge>
        <div v-if="desactivando.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">
          {{ desactivando.error.value }}
        </div>
        <div class="flex gap-2">
          <Boton variante="secundario" @click="editando = true">Editar</Boton>
          <Boton variante="fantasma" :disabled="desactivando.cargando.value" @click="desactivar">Desactivar</Boton>
        </div>
      </Tarjeta>

      <SeccionProximamente titulo="Total facturado y aprobación">
        Depende de las órdenes de trabajo y cotizaciones del cliente (Módulos 4 y 6), que todavía no existen.
      </SeccionProximamente>
      <SeccionProximamente titulo="Mi Garage">
        La cuenta del cliente en la app móvil es un dato distinto de este módulo (cuenta_cliente).
      </SeccionProximamente>
    </div>

    <div class="flex flex-col gap-5">
      <div class="flex flex-col gap-2.5">
        <div class="flex items-center gap-2">
          <div class="font-titulo text-xl font-bold">Vehículos <span class="font-mono text-[14px] font-medium" :style="{ color: 'var(--tx3)' }">{{ cliente.vehiculos.length }}</span></div>
          <div class="flex-1" />
          <Boton v-if="!mostrandoNuevoVehiculo" variante="secundario" @click="mostrandoNuevoVehiculo = true"><PhPlus /> Vehículo</Boton>
        </div>

        <FormularioVehiculo
          v-if="mostrandoNuevoVehiculo"
          :cliente-id="cliente.id"
          @guardado="onVehiculoRegistrado"
          @cancelar="mostrandoNuevoVehiculo = false"
        />

        <div v-if="!cliente.vehiculos.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Sin vehículos vinculados todavía.</div>
        <div class="grid grid-cols-2 gap-3">
          <NuxtLink
            v-for="vehiculo in cliente.vehiculos"
            :key="vehiculo.vehiculoId"
            :to="`/vehiculos/${vehiculo.vehiculoId}`"
            class="flex flex-col gap-2 rounded-xl border p-4"
            :style="{ background: 'var(--sf)', borderColor: 'var(--bd)' }"
          >
            <div class="flex items-center gap-2">
              <span
                v-if="vehiculo.placa"
                class="rounded px-1.5 py-0.5 font-mono text-[14px] font-bold"
                :style="{ border: '2px solid var(--tx)' }"
              >
                {{ vehiculo.placa }}
              </span>
              <div class="flex-1" />
              <Badge tono="neutro">{{ etiquetaRol(vehiculo.rol) }}</Badge>
            </div>
            <div class="text-[17px] font-bold">{{ vehiculo.marca }} {{ vehiculo.modelo }} {{ vehiculo.anio }}</div>
          </NuxtLink>
        </div>
      </div>

      <SeccionProximamente titulo="Pendientes">
        Hallazgos y cotizaciones sin responder vienen del Módulo 6.
      </SeccionProximamente>
      <SeccionProximamente titulo="Historial de visitas">
        Viene de las órdenes de trabajo del cliente (Módulo 4), que todavía no existen.
      </SeccionProximamente>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhEnvelopeSimple, PhIdentificationCard, PhPhone, PhPlus } from '@phosphor-icons/vue';
import type { ClienteDetalleDto, RolClienteVehiculo } from '@talleros/api-client';

const route = useRoute();
const api = useApi();
const carga = useCarga<ClienteDetalleDto>();
const desactivando = useCarga<unknown>();
const cliente = ref<ClienteDetalleDto | null>(null);
const editando = ref(false);
const mostrandoNuevoVehiculo = ref(false);

const iniciales = computed(() =>
  (cliente.value?.nombre ?? '')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((parte) => parte[0]!.toUpperCase())
    .join(''),
);

const etiquetasRol: Record<RolClienteVehiculo, string> = {
  Propietario: 'Propietario',
  ConductorHabitual: 'Conductor habitual',
  ResponsableFlota: 'Responsable de flota',
};
function etiquetaRol(rol: RolClienteVehiculo) {
  return etiquetasRol[rol];
}

async function cargar() {
  cliente.value = await carga.ejecutar(() => api.clientes.obtener(route.params.id as string));
}

function onEditado() {
  editando.value = false;
  cargar();
}

function onVehiculoRegistrado() {
  mostrandoNuevoVehiculo.value = false;
  cargar();
}

async function desactivar() {
  if (!cliente.value) return;
  if (!confirm(`¿Desactivar a ${cliente.value.nombre}?`)) return;
  const resultado = await desactivando.ejecutar(() => api.clientes.desactivar(cliente.value!.id));
  if (resultado !== null) await navigateTo('/clientes');
}

onMounted(cargar);
</script>
