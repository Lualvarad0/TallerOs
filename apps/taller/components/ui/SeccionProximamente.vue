<template>
  <div class="flex flex-col gap-1.5 rounded-xl border border-dashed p-4" :style="{ borderColor: 'var(--bd2)' }">
    <div class="flex items-center gap-2 text-[13px] font-bold" :style="{ color: 'var(--tx3)' }">
      <PhHourglass />
      {{ titulo }}
      <Badge tono="neutro">Próximamente</Badge>
    </div>
    <div class="text-[12.5px]" :style="{ color: 'var(--tx3)' }"><slot /></div>
  </div>
</template>

<script setup lang="ts">
import { PhHourglass } from '@phosphor-icons/vue';

defineProps<{ titulo: string }>();
</script>
