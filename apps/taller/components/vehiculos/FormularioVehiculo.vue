<template>
  <Tarjeta class="gap-3">
    <div class="flex gap-3">
      <CampoSelect
        v-model="form.tipoVehiculoId"
        label="Tipo"
        placeholder="Elegir…"
        :opciones="tiposVehiculo.map((t) => ({ valor: String(t.id), etiqueta: t.nombre }))"
        class="min-w-0 flex-1"
        @update:model-value="onCambioTipo"
      />
      <CampoSelect
        v-model="form.marcaId"
        label="Marca"
        placeholder="Elegir…"
        :opciones="marcas.map((m) => ({ valor: m.id, etiqueta: m.nombre }))"
        class="min-w-0 flex-1"
        @update:model-value="onCambioMarca"
      />
      <CampoSelect
        v-model="form.modeloId"
        label="Modelo"
        placeholder="Elegir…"
        :opciones="modelos.map((m) => ({ valor: m.id, etiqueta: m.nombre }))"
        class="min-w-0 flex-1"
      />
      <CampoTexto v-model="form.anio" label="Año" type="number" placeholder="2020" class="w-24 flex-shrink-0" />
    </div>
    <div class="flex gap-3">
      <CampoTexto
        v-model="form.vin"
        label="VIN (opcional si hay placa)"
        placeholder="17 caracteres"
        class="flex-1"
        :advertencia="advertenciaVin"
      />
      <CampoTexto
        v-model="form.placa"
        label="Placa (opcional si hay VIN)"
        placeholder="ABC-1234"
        class="flex-1"
        :advertencia="advertenciaPlaca"
      />
      <CampoTexto v-model="form.motor" label="Motor (opcional)" class="flex-1" />
      <CampoSelect
        v-model="form.rol"
        label="Rol de este cliente"
        class="w-52 flex-shrink-0"
        :opciones="[
          { valor: 'Propietario', etiqueta: 'Propietario' },
          { valor: 'ConductorHabitual', etiqueta: 'Conductor habitual' },
          { valor: 'ResponsableFlota', etiqueta: 'Responsable de flota' },
        ]"
      />
    </div>
    <div v-if="carga.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">{{ carga.error.value }}</div>
    <div class="flex gap-2">
      <Boton :disabled="carga.cargando.value || !puedeGuardar" @click="guardar">Guardar</Boton>
      <Boton variante="fantasma" @click="emit('cancelar')">Cancelar</Boton>
    </div>
  </Tarjeta>
</template>

<script setup lang="ts">
import { esVinValido, placaTieneFormatoReconocido } from '@talleros/validators';
import type { MarcaDto, ModeloDto, RolClienteVehiculo, TipoVehiculoDto } from '@talleros/api-client';

const props = defineProps<{ clienteId: string }>();
const emit = defineEmits<{ guardado: []; cancelar: [] }>();

const api = useApi();
const carga = useCarga<unknown>();

const tiposVehiculo = ref<TipoVehiculoDto[]>([]);
const marcas = ref<MarcaDto[]>([]);
const modelos = ref<ModeloDto[]>([]);

const form = reactive({
  tipoVehiculoId: '',
  marcaId: '',
  modeloId: '',
  anio: '',
  vin: '',
  placa: '',
  motor: '',
  rol: 'Propietario' as RolClienteVehiculo,
});

async function onCambioTipo(tipoVehiculoId: string) {
  form.marcaId = '';
  form.modeloId = '';
  modelos.value = [];
  marcas.value = tipoVehiculoId ? await api.marcas.listar(Number(tipoVehiculoId)) : [];
}

async function onCambioMarca(marcaId: string) {
  form.modeloId = '';
  modelos.value = marcaId ? await api.marcas.listarModelos(marcaId) : [];
}

// Advertencias, no bloqueos: el VIN de verdad lo valida el backend al guardar (dígito verificador
// real, ver RegistrarVehiculoCommandValidator) y la placa ni eso — su formato cambia con el tiempo
// y hay excepciones oficiales (ver placaTieneFormatoReconocido). Por eso ninguna de las dos entra
// en puedeGuardar: avisan antes de que el usuario guarde, pero nunca le impiden intentarlo.
const advertenciaVin = computed(() => {
  const vin = form.vin.trim();
  return vin && !esVinValido(vin) ? 'Este VIN no parece válido (dígito verificador); se puede guardar igual.' : null;
});

const advertenciaPlaca = computed(() => {
  const placa = form.placa.trim();
  return placa && !placaTieneFormatoReconocido(placa)
    ? 'Este formato de placa no es el habitual; se puede guardar igual.'
    : null;
});

const puedeGuardar = computed(
  () => !!form.marcaId && !!form.modeloId && !!form.anio && (!!form.vin.trim() || !!form.placa.trim()),
);

async function guardar() {
  const resultado = await carga.ejecutar(() =>
    api.vehiculos.registrar({
      clienteId: props.clienteId,
      vin: form.vin.trim() || null,
      placa: form.placa.trim() || null,
      marcaId: form.marcaId,
      modeloId: form.modeloId,
      anio: Number(form.anio),
      motor: form.motor.trim() || null,
      rol: form.rol,
    }),
  );
  if (resultado) emit('guardado');
}

onMounted(async () => {
  tiposVehiculo.value = await api.tiposVehiculo.listar();
});
</script>
