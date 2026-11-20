<template>
  <div class="flex h-full flex-col items-center justify-center gap-3 text-center">
    <PhHourglassMedium class="text-4xl" :style="{ color: 'var(--tx3)' }" />
    <div class="text-lg font-semibold" :style="{ color: 'var(--tx)' }">Próximamente</div>
    <div class="text-sm" :style="{ color: 'var(--tx2)' }">Este módulo todavía no está disponible.</div>
  </div>
</template>

<script setup lang="ts">
import { PhHourglassMedium } from '@phosphor-icons/vue';
</script>
