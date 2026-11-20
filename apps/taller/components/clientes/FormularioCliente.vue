<template>
  <Tarjeta class="gap-3">
    <div class="flex gap-3">
      <CampoSelect
        v-if="!cliente"
        v-model="form.tipo"
        label="Tipo"
        class="w-44 flex-shrink-0"
        :opciones="[
          { valor: 'PersonaNatural', etiqueta: 'Persona natural' },
          { valor: 'Empresa', etiqueta: 'Empresa' },
        ]"
      />
      <CampoTexto v-model="form.nombre" label="Nombre" placeholder="Carlos Méndez" class="flex-1" />
    </div>
    <div class="flex gap-3">
      <CampoTexto
        v-if="!cliente"
        v-model="form.identificacion"
        label="Identificación"
        :placeholder="form.tipo === 'PersonaNatural' ? 'Cédula o RUC (10 o 13 dígitos)' : 'RUC (13 dígitos)'"
        :error="errorIdentificacion"
        class="flex-1"
      />
      <CampoTexto v-model="form.telefono" label="Teléfono (opcional)" placeholder="0991234567" class="flex-1" />
      <CampoTexto v-model="form.email" label="Correo (opcional)" placeholder="cliente@correo.com" class="flex-1" />
    </div>
    <div class="flex flex-col gap-1.5">
      <div class="text-[13px] font-semibold" :style="{ color: 'var(--tx2)' }">Canal preferido de aviso</div>
      <div class="grid w-72 grid-cols-2 gap-1.5 rounded-lg border p-1" :style="{ borderColor: 'var(--bd)' }">
        <button
          v-for="canal in (['WhatsApp', 'Correo'] as const)"
          :key="canal"
          type="button"
          class="h-9 rounded-md text-[13.5px] font-bold"
          :style="form.canalPreferido === canal ? { background: 'var(--sf3)', color: 'var(--tx)' } : { color: 'var(--tx3)' }"
          @click="form.canalPreferido = canal"
        >
          {{ canal }}
        </button>
      </div>
    </div>
    <div v-if="carga.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">{{ carga.error.value }}</div>
    <div class="flex gap-2">
      <Boton :disabled="carga.cargando.value || !puedeGuardar" @click="guardar">Guardar</Boton>
      <Boton variante="fantasma" @click="emit('cancelar')">Cancelar</Boton>
    </div>
  </Tarjeta>
</template>

<script setup lang="ts">
import { esCedulaValida, esRucDePersonaNatural, esRucDeSociedad, esRucValido } from '@talleros/validators';
import type { ClienteDto, TipoCliente } from '@talleros/api-client';

// Sin cliente (modo crear): tipo + identificación, igual que CrearClienteCommand. Con cliente
// (modo editar): esos dos no se tocan, ver ActualizarClienteCommand (backend) — cambiar de persona
// natural a empresa, o la cédula/RUC, no es "editar", es otra persona.
const props = defineProps<{ cliente?: ClienteDto }>();
const emit = defineEmits<{ guardado: []; cancelar: [] }>();

const api = useApi();
const carga = useCarga<unknown>();

const form = reactive({
  tipo: (props.cliente?.tipo ?? 'PersonaNatural') as TipoCliente,
  nombre: props.cliente?.nombre ?? '',
  identificacion: '',
  telefono: props.cliente?.telefono ?? '',
  email: props.cliente?.email ?? '',
  canalPreferido: props.cliente?.canalPreferido ?? 'WhatsApp',
});

// Misma regla que CrearClienteCommandValidator (backend): persona natural con cédula o con RUC de
// persona natural (sin obligarla a registrarse como empresa); empresa solo con RUC de sociedad.
const identificacionValida = computed(() => {
  if (props.cliente) return true;
  const id = form.identificacion.trim();
  if (form.tipo === 'PersonaNatural') return esCedulaValida(id) || (esRucDePersonaNatural(id) && esRucValido(id));
  return esRucDeSociedad(id) && esRucValido(id);
});

const errorIdentificacion = computed(() => {
  if (!form.identificacion || identificacionValida.value) return null;
  return form.tipo === 'PersonaNatural'
    ? 'Debe ser una cédula válida o, si se factura con RUC, un RUC de persona natural válido.'
    : 'El RUC no es válido.';
});

const puedeGuardar = computed(() => !!form.nombre.trim() && (!!props.cliente || identificacionValida.value));

async function guardar() {
  const resultado = await carga.ejecutar(async () => {
    const datosComunes = {
      nombre: form.nombre,
      telefono: form.telefono || null,
      email: form.email || null,
      canalPreferido: form.canalPreferido,
    };
    if (props.cliente) {
      await api.clientes.actualizar(props.cliente.id, datosComunes);
    } else {
      await api.clientes.crear({ ...datosComunes, tipo: form.tipo, identificacion: form.identificacion.trim() });
    }
    return true;
  });
  if (resultado) emit('guardado');
}
</script>
