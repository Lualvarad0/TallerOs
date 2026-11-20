<template>
  <div
    class="flex flex-col gap-3.5 rounded-xl border p-5"
    :style="{ background: 'var(--sf)', borderColor: 'var(--bd)' }"
  >
    <slot />
  </div>
</template>
