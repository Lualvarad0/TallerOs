<template>
  <button
    :type="type"
    class="inline-flex items-center justify-center gap-1.5 whitespace-nowrap rounded-md text-[15px] font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-50"
    :class="claseVariante"
    :style="estiloVariante"
  >
    <slot name="icono" />
    <slot />
  </button>
</template>

<script setup lang="ts">
const props = withDefaults(
  defineProps<{ variante?: 'primario' | 'secundario' | 'fantasma'; icono?: boolean; type?: 'button' | 'submit' }>(),
  { variante: 'primario', icono: false, type: 'button' },
);

const claseVariante = computed(() => {
  if (props.icono) return 'h-9 w-9';
  return props.variante === 'fantasma' ? 'h-9 px-3' : 'h-9 px-3.5';
});

const estiloVariante = computed(() => {
  switch (props.variante) {
    case 'secundario':
      return { background: 'var(--sf)', color: 'var(--tx)', border: '1px solid var(--bd2)' };
    case 'fantasma':
      return { background: 'transparent', color: 'var(--tx2)' };
    default:
      return { background: 'var(--ac)', color: 'var(--ac-ink)', fontWeight: 700 };
  }
});
</script>
