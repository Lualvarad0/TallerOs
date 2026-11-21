<template>
  <div class="flex h-full flex-col gap-5 p-7">
    <div class="font-titulo text-2xl font-bold tracking-tight">Nueva recepción</div>

    <!-- Paso 1: cliente y vehículo. Sin endpoint de "buscar por placa" (no existe en el backend
    todavía, ver packages/api-client/recursos/vehiculos.ts): se busca el cliente, igual que en
    /clientes, y se elige uno de sus vehículos ya vinculados. -->
    <Tarjeta v-if="!vehiculoId" class="gap-3.5">
      <div class="flex items-center gap-2 text-[16px] font-bold">
        <span
          class="grid h-6 w-6 place-items-center rounded-full text-[12px]"
          :style="{ background: 'var(--tx)', color: 'var(--bg)' }"
          >1</span
        >
        Vehículo y cliente
      </div>
      <CampoTexto v-model="busquedaCliente" placeholder="Buscar cliente por nombre o identificación…" />

      <div v-if="cargaClientes.cargando.value" class="text-[13px]" :style="{ color: 'var(--tx3)' }">Buscando…</div>
      <div v-else-if="busquedaCliente && !clientesEncontrados.length" class="text-[13px]" :style="{ color: 'var(--tx3)' }">
        Sin resultados.
      </div>
      <div v-if="clientesEncontrados.length && !clienteElegido" class="flex flex-col gap-1.5">
        <button
          v-for="c in clientesEncontrados"
          :key="c.id"
          type="button"
          class="flex items-center gap-2 rounded-lg border px-3 py-2 text-left text-[14px]"
          :style="{ borderColor: 'var(--bd)', background: 'var(--sf)' }"
          @click="elegirCliente(c)"
        >
          <b class="flex-1 font-semibold">{{ c.nombre }}</b>
          <span class="font-mono text-[12.5px]" :style="{ color: 'var(--tx3)' }">{{ c.identificacion }}</span>
        </button>
      </div>

      <div v-if="clienteElegido" class="flex flex-col gap-3">
        <div
          class="flex items-center gap-3 rounded-lg border px-3.5 py-2.5"
          :style="{ background: 'var(--grn-s)', borderColor: 'var(--grn)' }"
        >
          <PhCheckCircle weight="fill" :style="{ color: 'var(--grn)', fontSize: '20px' }" />
          <div class="flex-1">
            <div class="text-[15px] font-bold">{{ clienteElegido.nombre }}</div>
            <div class="text-[13px]" :style="{ color: 'var(--tx2)' }">
              {{ clienteElegido.identificacion }}<span v-if="clienteElegido.telefono"> · {{ clienteElegido.telefono }}</span>
            </div>
          </div>
          <button type="button" class="text-[13px] font-semibold" :style="{ color: 'var(--grn)' }" @click="limpiarCliente">
            Cambiar
          </button>
        </div>

        <div v-if="cargaClienteDetalle.cargando.value" class="text-[13px]" :style="{ color: 'var(--tx3)' }">Cargando vehículos…</div>
        <template v-else>
          <div class="grid grid-cols-2 gap-2.5">
            <button
              v-for="v in clienteElegido.vehiculos"
              :key="v.vehiculoId"
              type="button"
              class="flex flex-col gap-1 rounded-lg border p-3 text-left"
              :style="{ borderColor: 'var(--bd)', background: 'var(--sf)' }"
              @click="elegirVehiculo(v)"
            >
              <span v-if="v.placa" class="w-fit rounded px-1.5 py-0.5 font-mono text-[13px] font-bold" :style="{ border: '2px solid var(--tx)' }">{{ v.placa }}</span>
              <span class="text-[15px] font-bold">{{ v.marca }} {{ v.modelo }} {{ v.anio }}</span>
            </button>
          </div>

          <Boton v-if="!mostrarNuevoVehiculo" variante="secundario" @click="mostrarNuevoVehiculo = true"><PhPlus />Vehículo nuevo para este cliente</Boton>
          <FormularioVehiculo
            v-else
            :cliente-id="clienteElegido.id"
            @guardado="onVehiculoRegistrado"
            @cancelar="mostrarNuevoVehiculo = false"
          />
        </template>
      </div>
    </Tarjeta>

    <!-- Paso 2 en adelante: solo una vez hay vehículo elegido. -->
    <template v-else>
      <div
        class="flex items-center gap-3 rounded-lg border px-3.5 py-2.5"
        :style="{ background: 'var(--grn-s)', borderColor: 'var(--grn)' }"
      >
        <PhCheckCircle weight="fill" :style="{ color: 'var(--grn)', fontSize: '20px' }" />
        <div class="flex-1 text-[15px] font-bold">
          {{ vehiculoElegidoLabel }} <span class="font-normal" :style="{ color: 'var(--tx2)' }">· {{ clienteElegido?.nombre }}</span>
        </div>
        <button type="button" class="text-[13px] font-semibold" :style="{ color: 'var(--grn)' }" @click="vehiculoId = null">Cambiar</button>
      </div>

      <Tarjeta class="gap-3.5">
        <div class="flex items-center gap-2 text-[16px] font-bold">
          <span class="grid h-6 w-6 place-items-center rounded-full text-[12px]" :style="{ background: 'var(--tx)', color: 'var(--bg)' }">2</span>
          Datos de ingreso
        </div>
        <div class="grid grid-cols-4 gap-3">
          <CampoSelect
            v-model="form.sucursalId"
            label="Sucursal"
            placeholder="Elegir…"
            :opciones="sucursales.map((s) => ({ valor: s.id, etiqueta: s.nombre }))"
            @update:model-value="onCambioSucursal"
          />
          <CampoSelect
            v-model="form.prioridad"
            label="Prioridad"
            :opciones="[
              { valor: 'Normal', etiqueta: 'Normal' },
              { valor: 'Alta', etiqueta: 'Alta' },
              { valor: 'Urgente', etiqueta: 'Urgente' },
            ]"
          />
          <CampoTexto v-model="form.kmIngreso" label="Kilometraje actual" type="number" placeholder="66480" />
          <CampoSelect
            v-model="form.asesorPersonalId"
            label="Asesor (opcional)"
            placeholder="Sin asesor"
            :opciones="personalDeLaSucursal.map((p) => ({ valor: p.id, etiqueta: p.nombre }))"
          />
        </div>

        <div class="flex flex-col gap-1.5">
          <label class="text-[13px] font-semibold" :style="{ color: 'var(--tx2)' }">Nivel de combustible · {{ etiquetaCombustible }}</label>
          <div class="grid grid-cols-8 gap-1">
            <button
              v-for="n in 8"
              :key="n"
              type="button"
              class="h-8 rounded-md"
              :style="{ border: '1px solid var(--bd2)', background: n <= form.combustible ? 'var(--ac)' : 'var(--sf)' }"
              @click="form.combustible = form.combustible === n ? n - 1 : n"
            />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <div class="flex flex-col gap-1.5">
            <label class="text-[13px] font-semibold" :style="{ color: 'var(--tx2)' }">Objetos dentro del vehículo (opcional)</label>
            <textarea
              v-model="form.objetos"
              rows="2"
              class="rounded-lg px-3.5 py-2 text-[15px]"
              :style="{ border: '1px solid var(--bd2)', background: 'var(--sf)', color: 'var(--tx)' }"
            />
          </div>
          <div class="flex flex-col gap-1.5">
            <label class="text-[13px] font-semibold" :style="{ color: 'var(--tx2)' }">Motivo de ingreso / observaciones</label>
            <textarea
              v-model="form.observaciones"
              rows="2"
              class="rounded-lg px-3.5 py-2 text-[15px]"
              :style="{ border: '1px solid var(--bd2)', background: 'var(--sf)', color: 'var(--tx)' }"
            />
          </div>
        </div>
      </Tarjeta>

      <Tarjeta class="gap-3">
        <div class="flex items-center gap-2 text-[16px] font-bold">
          <span class="grid h-6 w-6 place-items-center rounded-full text-[12px]" :style="{ background: 'var(--tx)', color: 'var(--bg)' }">3</span>
          Golpes y daños previos
          <span class="font-mono text-[13px]" :style="{ color: 'var(--tx3)' }">{{ form.daniosPrevios.length }} marcas</span>
        </div>
        <div v-for="(d, i) in form.daniosPrevios" :key="i" class="flex items-end gap-2">
          <CampoTexto v-model="d.zona" label="Zona" placeholder="Puerta delantera izquierda" class="flex-1" />
          <CampoSelect
            v-model="d.tipo"
            label="Tipo"
            class="w-40"
            :opciones="[
              { valor: 'Golpe', etiqueta: 'Golpe' },
              { valor: 'Rayon', etiqueta: 'Rayón' },
              { valor: 'Abolladura', etiqueta: 'Abolladura' },
              { valor: 'Vidrio', etiqueta: 'Vidrio' },
            ]"
          />
          <CampoTexto v-model="d.descripcion" label="Descripción (opcional)" class="flex-1" />
          <Boton variante="fantasma" icono @click="form.daniosPrevios.splice(i, 1)"><PhTrash /></Boton>
        </div>
        <Boton variante="secundario" @click="form.daniosPrevios.push({ zona: '', tipo: 'Golpe', descripcion: '', fotoUrl: null })">
          <PhPlus />Agregar marca
        </Boton>
      </Tarjeta>

      <div v-if="guardando.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">{{ guardando.error.value }}</div>
      <div class="flex gap-2">
        <Boton :disabled="guardando.cargando.value || !puedeGuardar" @click="crearOt">Crear OT<PhArrowRight /></Boton>
        <Boton variante="fantasma" @click="navigateTo('/tablero')">Cancelar</Boton>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { PhArrowRight, PhCheckCircle, PhPlus, PhTrash } from '@phosphor-icons/vue';
import type { AreaDto, ClienteDetalleDto, ClienteDto, PersonalDto, PrioridadOt, SucursalDto, TipoDanio, VehiculoResumenDto } from '@talleros/api-client';

const api = useApi();

// Paso 1
const busquedaCliente = ref('');
const cargaClientes = useCarga<ClienteDto[]>();
const clientesEncontrados = ref<ClienteDto[]>([]);
const clienteElegido = ref<ClienteDetalleDto | null>(null);
const cargaClienteDetalle = useCarga<ClienteDetalleDto>();
const vehiculoId = ref<string | null>(null);
const vehiculoElegidoLabel = ref('');
const mostrarNuevoVehiculo = ref(false);
let temporizadorBusqueda: ReturnType<typeof setTimeout> | undefined;

watch(busquedaCliente, () => {
  clearTimeout(temporizadorBusqueda);
  if (!busquedaCliente.value) {
    clientesEncontrados.value = [];
    return;
  }
  temporizadorBusqueda = setTimeout(async () => {
    clientesEncontrados.value = (await cargaClientes.ejecutar(() => api.clientes.listar(busquedaCliente.value))) ?? [];
  }, 300);
});

async function elegirCliente(c: ClienteDto) {
  clientesEncontrados.value = [];
  busquedaCliente.value = '';
  clienteElegido.value = await cargaClienteDetalle.ejecutar(() => api.clientes.obtener(c.id));
}

function limpiarCliente() {
  clienteElegido.value = null;
  vehiculoId.value = null;
}

function elegirVehiculo(v: VehiculoResumenDto) {
  vehiculoId.value = v.vehiculoId;
  vehiculoElegidoLabel.value = `${v.marca} ${v.modelo} ${v.anio}${v.placa ? ' · ' + v.placa : ''}`;
}

async function onVehiculoRegistrado() {
  mostrarNuevoVehiculo.value = false;
  if (clienteElegido.value) clienteElegido.value = await cargaClienteDetalle.ejecutar(() => api.clientes.obtener(clienteElegido.value!.id));
}

// Paso 2: sucursal, prioridad, km, combustible, asesor
const sucursales = ref<SucursalDto[]>([]);
const areasDeLaSucursal = ref<AreaDto[]>([]);
const personalDeLaSucursal = ref<PersonalDto[]>([]);

const form = reactive({
  sucursalId: '',
  prioridad: 'Normal' as PrioridadOt,
  kmIngreso: '',
  asesorPersonalId: '',
  combustible: 4,
  objetos: '',
  observaciones: '',
  daniosPrevios: [] as { zona: string; tipo: TipoDanio; descripcion: string; fotoUrl: null }[],
});

const etiquetaCombustible = computed(() => ['Vacío', '1/8', '1/4', '3/8', '1/2', '5/8', '3/4', '7/8', 'Lleno'][form.combustible]);

let idCambioSucursal = 0;
async function onCambioSucursal(sucursalId: string) {
  const idPeticion = ++idCambioSucursal;
  form.asesorPersonalId = '';
  personalDeLaSucursal.value = [];
  if (!sucursalId) return;
  const areas = await api.areas.listar(sucursalId);
  const porArea = await Promise.all(areas.map((a) => api.personal.listar(a.id)));
  if (idPeticion !== idCambioSucursal) return; // el usuario ya cambió de sucursal otra vez
  areasDeLaSucursal.value = areas;
  personalDeLaSucursal.value = porArea.flat().filter((p) => p.tieneAcceso);
}

const puedeGuardar = computed(() => !!form.sucursalId && !!form.kmIngreso && !!vehiculoId.value && !!clienteElegido.value);

const guardando = useCarga<{ otId: string }>();

async function crearOt() {
  if (!clienteElegido.value || !vehiculoId.value) return;
  const resultado = await guardando.ejecutar(() =>
    api.recepcion.registrar({
      sucursalId: form.sucursalId,
      vehiculoId: vehiculoId.value!,
      clienteId: clienteElegido.value!.id,
      prioridad: form.prioridad,
      kmIngreso: Number(form.kmIngreso),
      asesorPersonalId: form.asesorPersonalId || null,
      combustible: form.combustible,
      objetos: form.objetos.trim() || null,
      observaciones: form.observaciones.trim() || null,
      daniosPrevios: form.daniosPrevios
        .filter((d) => d.zona.trim())
        .map((d) => ({ zona: d.zona.trim(), tipo: d.tipo, descripcion: d.descripcion.trim() || null, fotoUrl: null })),
    }),
  );
  if (resultado) await navigateTo(`/ordenes-trabajo/${resultado.otId}`);
}

onMounted(async () => {
  sucursales.value = await api.sucursales.listar();
});
</script>
