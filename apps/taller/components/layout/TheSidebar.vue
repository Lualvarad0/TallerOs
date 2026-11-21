<template>
  <aside
    style="width: 232px"
    class="h-full flex-shrink-0 flex flex-col gap-0.5 border-r py-3.5 px-2.5 font-sans"
    :style="{ background: 'var(--sf)', borderColor: 'var(--bd)', color: 'var(--tx)' }"
  >
    <NuxtLink to="/" class="flex items-center gap-2.5 px-2 pb-3.5 pt-1">
      <div
        class="grid h-[30px] w-[30px] place-items-center rounded-md text-[17px]"
        :style="{ background: 'var(--ac)', color: 'var(--ac-ink)' }"
      >
        <PhWrench weight="bold" />
      </div>
      <div class="font-titulo text-lg font-extrabold tracking-tight">TallerOS</div>
    </NuxtLink>

    <div
      class="mb-3 flex items-center gap-2 rounded-lg border px-2.5 py-2"
      :style="{ borderColor: 'var(--bd)', background: 'var(--sf2)' }"
    >
      <div class="min-w-0 flex-1">
        <div class="truncate text-[13.5px] font-semibold">{{ nombreEmpresa ?? 'Cargando…' }}</div>
      </div>
    </div>

    <nav class="flex flex-1 flex-col gap-0.5 overflow-y-auto">
      <template v-for="(item, i) in items" :key="i">
        <div
          v-if="item.separador"
          class="px-2.5 pb-1 pt-3.5 text-[11px] font-semibold uppercase tracking-wider"
          :style="{ color: 'var(--tx3)' }"
        >
          {{ item.label }}
        </div>
        <NuxtLink
          v-else
          :to="item.href"
          class="flex h-9 items-center gap-2.5 rounded-md px-2.5 text-[15px] transition-colors"
          :style="esActivo(item.href)
            ? { background: 'var(--sf3)', color: 'var(--tx)', fontWeight: 600, boxShadow: 'inset 3px 0 0 var(--ac)' }
            : { color: 'var(--tx2)', fontWeight: 500 }"
        >
          <component :is="item.icono" :weight="esActivo(item.href) ? 'bold' : 'regular'" class="text-lg" />
          <span class="flex-1">{{ item.label }}</span>
        </NuxtLink>
      </template>
    </nav>

    <div class="flex items-center gap-2.5 border-t pt-2.5" :style="{ borderColor: 'var(--bd)' }">
      <div
        class="grid h-8 w-8 place-items-center rounded-full text-xs font-bold"
        :style="{ background: 'var(--vio-s)', color: 'var(--vio)' }"
      >
        {{ iniciales }}
      </div>
      <div class="min-w-0 flex-1">
        <div class="truncate text-[13.5px] font-semibold">{{ sesion.email }}</div>
      </div>
      <NuxtLink to="/configuracion" aria-label="Configuración" :style="{ color: 'var(--tx3)' }">
        <PhGearSix class="text-lg" />
      </NuxtLink>
    </div>
  </aside>
</template>

<script setup lang="ts">
import {
  PhWrench,
  PhGearSix,
  PhSquaresFour,
  PhKanban,
  PhClipboardText,
  PhUsersThree,
  PhDeviceTablet,
  PhCarProfile,
  PhAddressBook,
  PhReceipt,
  PhWhatsappLogo,
  PhChartBar,
  PhSlidersHorizontal,
} from '@phosphor-icons/vue';

const sesion = useSesionStore();
const route = useRoute();

const nombreEmpresa = ref<string | null>(null);
const iniciales = computed(() => (sesion.email ? sesion.email.slice(0, 2).toUpperCase() : '--'));

onMounted(async () => {
  const empresaId = sesion.claims?.empresaId;
  if (!empresaId) return;
  try {
    const empresa = await useApi().empresas.obtener(empresaId);
    nombreEmpresa.value = empresa.nombre;
  } catch {
    nombreEmpresa.value = null;
  }
});

interface ItemNav {
  separador?: boolean;
  label: string;
  href?: string;
  icono?: unknown;
}

// "Configuración" (Módulo 1), "Clientes" (Módulo 3) y "Tablero OT"/"Recepción" (Módulo 4) tienen
// pantalla construida; el resto de módulos aún no existen y llevan a /proximamente, siguiendo el
// mismo orden y agrupación que Sidebar.dc.html. "Vehículos" no tiene lista propia (no hay query
// para listar todos los vehículos ni diseño para esa pantalla): se llega a la ficha de un vehículo
// desde la ficha de su cliente. "Registro delegado" y "Modo taller (PIN)" dependen de jornada y
// marcación (Módulo 5), que todavía no existen.
const items: ItemNav[] = [
  { label: 'Dashboard', href: '/proximamente', icono: PhSquaresFour },
  { label: 'Tablero OT', href: '/tablero', icono: PhKanban },
  { label: 'Recepción', href: '/recepcion', icono: PhClipboardText },
  { label: 'Registro delegado', href: '/proximamente', icono: PhUsersThree },
  { label: 'Modo taller (PIN)', href: '/proximamente', icono: PhDeviceTablet },
  { separador: true, label: 'Clientes y vehículos' },
  { label: 'Vehículos', href: '/proximamente', icono: PhCarProfile },
  { label: 'Clientes', href: '/clientes', icono: PhAddressBook },
  { label: 'Cotizaciones', href: '/proximamente', icono: PhReceipt },
  { label: 'Notificaciones', href: '/proximamente', icono: PhWhatsappLogo },
  { separador: true, label: 'Gestión' },
  { label: 'Reportes', href: '/proximamente', icono: PhChartBar },
  { label: 'Configuración', href: '/configuracion', icono: PhSlidersHorizontal },
];

function esActivo(href?: string) {
  return !!href && href !== '/proximamente' && route.path.startsWith(href);
}
</script>
