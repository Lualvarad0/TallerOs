/**
 * Preset de Tailwind con los tokens de docs/diseno/README.md. Colores mapeados a las variables
 * CSS de tokens.css (no valores fijos) para que el claro/oscuro funcione solo cambiando
 * data-theme, sin duplicar la paleta.
 */
export default {
  theme: {
    extend: {
      colors: {
        bg: 'var(--bg)',
        sf: 'var(--sf)',
        sf2: 'var(--sf2)',
        sf3: 'var(--sf3)',
        bd: 'var(--bd)',
        bd2: 'var(--bd2)',
        tx: 'var(--tx)',
        tx2: 'var(--tx2)',
        tx3: 'var(--tx3)',
        ac: { DEFAULT: 'var(--ac)', ink: 'var(--ac-ink)', tx: 'var(--ac-tx)', s: 'var(--ac-s)' },
        red: { DEFAULT: 'var(--red)', s: 'var(--red-s)' },
        blue: { DEFAULT: 'var(--blue)', s: 'var(--blue-s)' },
        vio: { DEFAULT: 'var(--vio)', s: 'var(--vio-s)' },
        grn: { DEFAULT: 'var(--grn)', s: 'var(--grn-s)' },
      },
      fontFamily: {
        titulo: ['"Barlow Semi Condensed"', 'sans-serif'],
        sans: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'monospace'],
      },
      borderRadius: {
        sm: '4px',
        md: '8px',
        lg: '12px',
        xl: '20px',
      },
      boxShadow: {
        tarjeta: 'var(--sh)',
      },
      spacing: {
        // Base 4px (ver design system); los botones táctiles de taller usan 18/30 (72/120px).
        18: '4.5rem',
        30: '7.5rem',
      },
    },
  },
};
