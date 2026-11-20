<template>
  <div class="flex h-screen w-screen overflow-hidden" :style="{ background: 'var(--bg)' }">
    <TheSidebar />
    <main class="flex-1 overflow-y-auto">
      <slot />
    </main>
  </div>
</template>
