export default defineNuxtConfig({
  compatibilityDate: '2026-10-31',
  devtools: { enabled: true },

  // docs/convenciones.md: Nuxt 3 en modo SPA.
  ssr: false,

  modules: ['@pinia/nuxt', '@nuxtjs/tailwindcss'],

  // Sin prefijo de carpeta: components/ui/Boton.vue se usa como <Boton>, no <UiBoton>.
  components: [{ path: '~/components', pathPrefix: false }],

  css: ['~/assets/css/tailwind.css'],

  app: {
    head: {
      link: [
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Barlow+Semi+Condensed:wght@600;700&family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@500;700&display=swap',
        },
      ],
    },
  },

  runtimeConfig: {
    public: {
      // Perfil "http" de TallerOS.Api (ver backend/src/TallerOS.Api/Properties/launchSettings.json).
      apiBase: 'http://localhost:5151',
    },
  },

  typescript: {
    strict: true,
  },
});
