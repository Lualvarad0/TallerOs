<template>
  <div class="flex h-full min-h-screen items-center justify-center p-6">
    <div
      class="flex w-full max-w-[420px] flex-col gap-7 rounded-xl border p-10"
      :style="{ background: 'var(--sf)', borderColor: 'var(--bd)' }"
    >
      <div class="flex items-center gap-2.5">
        <div
          class="grid h-9 w-9 place-items-center rounded-lg text-xl"
          :style="{ background: 'var(--ac)', color: 'var(--ac-ink)' }"
        >
          <PhWrench weight="bold" />
        </div>
        <div class="font-titulo text-xl font-bold tracking-tight">TallerOS</div>
      </div>

      <!-- Paso 1: credenciales -->
      <form v-if="paso === 'credenciales'" class="flex flex-col gap-5" @submit.prevent="enviarCredenciales">
        <div>
          <div class="font-titulo text-[28px] font-bold tracking-tight">Ingresar</div>
          <div class="mt-1.5 text-[15px]" :style="{ color: 'var(--tx2)' }">Con tu correo o número de celular.</div>
        </div>

        <CampoTexto v-model="email" label="Correo o celular" type="text" autocomplete="username" />
        <CampoTexto v-model="password" label="Contraseña" type="password" autocomplete="current-password" />

        <div v-if="credenciales.error.value" class="text-[13.5px] font-medium" :style="{ color: 'var(--red)' }">
          {{ credenciales.error.value }}
        </div>

        <Boton type="submit" class="h-[46px] w-full justify-center text-[16px]" :disabled="credenciales.cargando.value">
          {{ credenciales.cargando.value ? 'Entrando…' : 'Continuar' }}
        </Boton>

        <div class="flex items-center gap-3 text-[13px]" :style="{ color: 'var(--tx3)' }">
          <div class="h-px flex-1" :style="{ background: 'var(--bd)' }" />
          o
          <div class="h-px flex-1" :style="{ background: 'var(--bd)' }" />
        </div>

        <NuxtLink
          to="/proximamente"
          class="flex h-[46px] items-center justify-center gap-2 rounded-lg border text-[15px] font-bold"
          :style="{ borderColor: 'var(--bd2)' }"
        >
          <PhDeviceTablet class="text-lg" />
          Modo tablet de taller (PIN)
        </NuxtLink>
      </form>

      <!-- Paso 2: elegir empresa -->
      <div v-else class="flex flex-col gap-5">
        <div class="text-[13px] font-bold uppercase tracking-wider" :style="{ color: 'var(--tx3)' }">
          Paso 2 · Elige dónde trabajar
        </div>
        <div class="font-titulo text-[26px] font-bold tracking-tight">Tienes acceso a {{ sesion.empresasDisponibles.length }} empresas</div>

        <button
          v-for="empresa in sesion.empresasDisponibles"
          :key="empresa.empresaId"
          type="button"
          class="flex items-center gap-3.5 rounded-xl border p-4 text-left disabled:opacity-60"
          :style="{ borderColor: 'var(--bd)' }"
          :disabled="seleccion.cargando.value"
          @click="elegirEmpresa(empresa.empresaId)"
        >
          <div
            class="grid h-11 w-11 flex-shrink-0 place-items-center rounded-lg font-extrabold"
            :style="{ background: 'var(--ac)', color: 'var(--ac-ink)' }"
          >
            {{ iniciales(empresa.nombreEmpresa) }}
          </div>
          <div class="min-w-0 flex-1 text-[16px] font-bold">{{ empresa.nombreEmpresa }}</div>
          <PhCaretRight :style="{ color: 'var(--tx3)' }" />
        </button>

        <div v-if="seleccion.error.value" class="text-[13.5px] font-medium" :style="{ color: 'var(--red)' }">
          {{ seleccion.error.value }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PhWrench, PhDeviceTablet, PhCaretRight } from '@phosphor-icons/vue';

definePageMeta({ layout: 'vacio', middleware: 'invitado' });

const email = ref('');
const password = ref('');
const paso = ref<'credenciales' | 'empresas'>('credenciales');

const sesion = useSesionStore();
const api = useApi();
const credenciales = useCarga<void>();
const seleccion = useCarga<void>();

async function enviarCredenciales() {
  await credenciales.ejecutar(async () => {
    const resultado = await api.auth.iniciarSesion(email.value, password.value);
    sesion.procesarLogin(resultado, email.value);
    if (!resultado.tokenSesion) paso.value = 'empresas';
  });
  if (sesion.estaAutenticado) await navigateTo('/');
}

async function elegirEmpresa(empresaId: string) {
  await seleccion.ejecutar(async () => {
    const resultado = await api.auth.seleccionarEmpresa(empresaId);
    sesion.guardarTokenSesion(resultado.tokenSesion);
  });
  if (sesion.estaAutenticado) await navigateTo('/');
}

function iniciales(nombre: string) {
  return nombre
    .split(/\s+/)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase())
    .join('');
}
</script>
