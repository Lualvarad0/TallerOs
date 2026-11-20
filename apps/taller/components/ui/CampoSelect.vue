<template>
  <div class="flex flex-col gap-1.5">
    <label v-if="label" class="text-[13px] font-semibold" :style="{ color: 'var(--tx2)' }">{{ label }}</label>
    <select
      :value="modelValue"
      class="h-11 w-full min-w-0 rounded-lg px-3.5 text-[16px]"
      :style="{ border: '1px solid var(--bd2)', background: 'var(--sf)', color: 'var(--tx)' }"
      @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
    >
      <option v-if="placeholder" value="" disabled>{{ placeholder }}</option>
      <option v-for="opcion in opciones" :key="opcion.valor" :value="opcion.valor">{{ opcion.etiqueta }}</option>
    </select>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  modelValue: string;
  opciones: { valor: string; etiqueta: string }[];
  label?: string;
  placeholder?: string;
}>();

const emit = defineEmits<{ 'update:modelValue': [string] }>();
</script>
