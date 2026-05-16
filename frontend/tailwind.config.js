/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {
      colors: {
        // Primary palette — light sage / olive. The "big block" green of the
        // reference image. Used by Material primary AND via `bg-brand-*`.
        brand: {
          50:  '#f5f8ec',
          100: '#e8efd1',
          200: '#d4e2a8',
          300: '#bcd483',
          400: '#a5c46a',
          500: '#88a23f',  // The deeper olive of the reference.
          600: '#6f8632',
          700: '#58692a',
          800: '#444f23',
          900: '#34401b'
        },
        // Warm accents — taken from the bottom row of the reference image.
        // Used on home feature cards and any spot that needs extra warmth.
        sage: {
          50:  '#eef5e0',
          100: '#dceac3',
          500: '#a1c178',
          700: '#6e8e4a'
        },
        peach: {
          50:  '#fef0e8',
          100: '#fbdcc8',
          500: '#f5b59b',
          700: '#c98365'
        },
        honey: {
          50:  '#fef6db',
          100: '#fbe79f',
          500: '#f5c731',
          700: '#9a7710'
        },
        coral: {
          50:  '#fbeaee',
          100: '#f3c4cf',
          500: '#df6b80',
          700: '#a83a52'
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif']
      },
      boxShadow: {
        soft: '0 1px 2px 0 rgb(0 0 0 / 0.03), 0 1px 1px 0 rgb(0 0 0 / 0.02)',
        card: '0 1px 3px 0 rgb(0 0 0 / 0.04), 0 1px 2px -1px rgb(0 0 0 / 0.04)'
      }
    }
  },
  plugins: []
};
