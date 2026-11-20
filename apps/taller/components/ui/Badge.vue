<template>
  <span
    class="inline-flex h-6 items-center gap-1 whitespace-nowrap rounded px-2.5 text-[12.5px] font-bold"
    :style="estilo"
  >
    <slot name="icono" />
    <slot />
  </span>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{ tono?: 'neutro' | 'rojo' | 'azul' | 'violeta' | 'verde' | 'acento' }>(), {
  tono: 'neutro',
});

const estilo = computed(() => {
  const mapa: Record<string, { background: string; color: string }> = {
    neutro: { background: 'var(--sf3)', color: 'var(--tx2)' },
    rojo: { background: 'var(--red-s)', color: 'var(--red)' },
    azul: { background: 'var(--blue-s)', color: 'var(--blue)' },
    violeta: { background: 'var(--vio-s)', color: 'var(--vio)' },
    verde: { background: 'var(--grn-s)', color: 'var(--grn)' },
    acento: { background: 'var(--ac-s)', color: 'var(--ac-tx)' },
  };
  return mapa[props.tono];
});
</script>
