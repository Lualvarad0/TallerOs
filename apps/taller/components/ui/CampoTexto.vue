<template>
  <div class="flex flex-col gap-1.5">
    <div v-if="label" class="flex items-center">
      <label class="flex-1 text-[13px] font-semibold" :style="{ color: 'var(--tx2)' }">{{ label }}</label>
      <slot name="accion-label" />
    </div>
    <div
      class="flex h-11 items-center gap-2 rounded-lg px-3.5 text-[16px]"
      :style="{ border: `${error ? 2 : 1}px solid ${error ? 'var(--red)' : 'var(--bd2)'}` }"
    >
      <input
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :autocomplete="autocomplete"
        class="min-w-0 flex-1 bg-transparent outline-none"
        :style="{ color: 'var(--tx)' }"
        @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
      />
      <slot name="icono" />
    </div>
    <div v-if="error" class="text-[12.5px] font-medium" :style="{ color: 'var(--red)' }">{{ error }}</div>
    <div v-else-if="advertencia" class="text-[12.5px] font-medium" :style="{ color: 'var(--ac-tx)' }">{{ advertencia }}</div>
  </div>
</template>

<script setup lang="ts">
// advertencia (a diferencia de error) no pone el campo en rojo: es para avisos que no bloquean
// guardar, como el formato de VIN/placa en FormularioVehiculo (ver ese componente para la razón).
withDefaults(
  defineProps<{
    modelValue: string;
    label?: string;
    type?: string;
    placeholder?: string;
    autocomplete?: string;
    error?: string | null;
    advertencia?: string | null;
  }>(),
  { type: 'text' },
);

const emit = defineEmits<{ 'update:modelValue': [string] }>();
</script>
