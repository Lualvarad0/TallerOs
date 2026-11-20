<template>
  <div class="flex flex-col gap-4">
    <div class="flex items-end gap-3">
      <div class="flex-1">
        <div class="font-titulo text-2xl font-bold tracking-tight">Roles y permisos</div>
        <div class="mt-1 text-[13.5px]" :style="{ color: 'var(--tx2)' }">
          Los roles son acumulables: una persona puede tener varios a la vez.
        </div>
      </div>
      <Boton v-if="!mostrarNuevoRol" variante="secundario" @click="mostrarNuevoRol = true"><PhPlus /> Rol personalizado</Boton>
    </div>

    <Tarjeta v-if="mostrarNuevoRol" class="gap-3">
      <CampoTexto v-model="nombreNuevoRol" label="Nombre del rol" />
      <div v-if="carga.error.value" class="text-[13px] font-medium" :style="{ color: 'var(--red)' }">{{ carga.error.value }}</div>
      <div class="flex gap-2">
        <Boton :disabled="carga.cargando.value" @click="crearRol">Guardar</Boton>
        <Boton variante="fantasma" @click="mostrarNuevoRol = false">Cancelar</Boton>
      </div>
    </Tarjeta>

    <div v-if="carga.cargando.value && !roles.length" class="text-sm" :style="{ color: 'var(--tx3)' }">Cargando…</div>

    <div v-if="roles.length" class="overflow-x-auto rounded-xl border" :style="{ borderColor: 'var(--bd)', background: 'var(--sf)' }">
      <div class="grid" :style="{ gridTemplateColumns: `minmax(0,1.6fr) repeat(${roles.length}, minmax(0,1fr))` }">
        <div
          class="border-b p-3 text-[12px] font-bold uppercase tracking-wider"
          :style="{ color: 'var(--tx3)', background: 'var(--sf2)', borderColor: 'var(--bd)' }"
        >
          Permiso
        </div>
        <div
          v-for="rol in roles"
          :key="rol.id"
          class="border-b p-3 text-center text-[12px] font-bold leading-tight"
          :style="{ background: 'var(--sf2)', borderColor: 'var(--bd)' }"
        >
          {{ rol.nombre }}
        </div>

        <template v-for="permiso in permisos" :key="permiso.codigo">
          <div class="border-b px-4 py-2.5 text-[13.5px]" :style="{ borderColor: 'var(--bd)' }">{{ permiso.nombre }}</div>
          <div
            v-for="rol in roles"
            :key="rol.id + permiso.codigo"
            class="grid place-items-center border-b p-1.5"
            :style="{ borderColor: 'var(--bd)' }"
          >
            <button
              type="button"
              class="grid h-[26px] w-[26px] place-items-center rounded-md border-[1.5px]"
              :disabled="guardando === rol.id"
              :style="tieneElPermiso(rol, permiso.codigo)
                ? { background: 'var(--ac)', borderColor: 'var(--ac)', color: 'var(--ac-ink)' }
                : { background: 'transparent', borderColor: 'var(--bd2)' }"
              @click="alternarPermiso(rol, permiso.codigo)"
            >
              <PhCheck v-if="tieneElPermiso(rol, permiso.codigo)" weight="bold" />
            </button>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhPlus, PhCheck } from '@phosphor-icons/vue';
import type { PermisoDto, RolDto } from '@talleros/api-client';

const api = useApi();
const carga = useCarga<unknown>();
const roles = ref<RolDto[]>([]);
const permisos = ref<PermisoDto[]>([]);
const guardando = ref<string | null>(null);

const mostrarNuevoRol = ref(false);
const nombreNuevoRol = ref('');

function tieneElPermiso(rol: RolDto, codigo: string) {
  return rol.permisos.includes(codigo);
}

async function alternarPermiso(rol: RolDto, codigo: string) {
  const nuevosPermisos = tieneElPermiso(rol, codigo)
    ? rol.permisos.filter((c) => c !== codigo)
    : [...rol.permisos, codigo];

  guardando.value = rol.id;
  try {
    await api.roles.asignarPermisos(rol.id, nuevosPermisos);
    rol.permisos = nuevosPermisos;
  } finally {
    guardando.value = null;
  }
}

async function crearRol() {
  if (!nombreNuevoRol.value.trim()) return;
  await carga.ejecutar(async () => {
    await api.roles.crear(nombreNuevoRol.value);
    nombreNuevoRol.value = '';
    mostrarNuevoRol.value = false;
    return null;
  });
  await cargarTodo();
}

async function cargarTodo() {
  await carga.ejecutar(async () => {
    [roles.value, permisos.value] = await Promise.all([api.roles.listar(), api.roles.listarPermisos()]);
    return null;
  });
}

onMounted(cargarTodo);
</script>
