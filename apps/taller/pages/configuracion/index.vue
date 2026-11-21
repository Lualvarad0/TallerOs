<template>
  <div class="flex h-full">
    <div class="flex w-60 flex-shrink-0 flex-col gap-0.5 border-r p-3" :style="{ borderColor: 'var(--bd)', background: 'var(--sf2)' }">
      <div class="font-titulo px-2.5 pb-3.5 pt-1 text-xl font-bold">Configuración</div>
      <button
        v-for="seccion in secciones"
        :key="seccion.id"
        type="button"
        class="flex h-[38px] items-center gap-2.5 rounded-md px-2.5 text-left text-[15px] disabled:cursor-not-allowed disabled:opacity-50"
        :style="activa === seccion.id ? { background: 'var(--sf)', color: 'var(--tx)', fontWeight: 700 } : { color: 'var(--tx2)', fontWeight: 500 }"
        :disabled="seccion.proximamente"
        @click="activa = seccion.id"
      >
        <component :is="seccion.icono" class="text-lg" />
        <span class="flex-1">{{ seccion.label }}</span>
        <span v-if="seccion.proximamente" class="text-[11px]" :style="{ color: 'var(--tx3)' }">Próx.</span>
      </button>
    </div>

    <div class="flex-1 overflow-y-auto p-7">
      <PanelSucursales v-if="activa === 'sucursales'" />
      <PanelPersonal v-else-if="activa === 'personal'" />
      <PanelRoles v-else-if="activa === 'roles'" />
      <PanelDispositivos v-else-if="activa === 'dispositivos'" />
      <PanelTiposTrabajo v-else-if="activa === 'tipos-trabajo'" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhBuildings, PhUsers, PhShieldCheck, PhWrench, PhBellRinging, PhDeviceTablet } from '@phosphor-icons/vue';

type IdSeccion = 'sucursales' | 'personal' | 'roles' | 'dispositivos' | 'tipos-trabajo' | 'alertas';
interface Seccion {
  id: IdSeccion;
  label: string;
  icono: unknown;
  proximamente?: boolean;
}

const activa = ref<IdSeccion>('sucursales');

// "Tipos de trabajo" ya tiene pantalla (Módulo 4). "Alertas" depende del Módulo 8, que no existe aún.
const secciones: Seccion[] = [
  { id: 'sucursales', label: 'Sucursales y áreas', icono: PhBuildings },
  { id: 'personal', label: 'Personal', icono: PhUsers },
  { id: 'roles', label: 'Roles y permisos', icono: PhShieldCheck },
  { id: 'dispositivos', label: 'Dispositivos tablet', icono: PhDeviceTablet },
  { id: 'tipos-trabajo', label: 'Tipos de trabajo', icono: PhWrench },
  { id: 'alertas', label: 'Alertas y mantenimiento', icono: PhBellRinging, proximamente: true },
];
</script>
