import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: 'wwwroot',
    emptyOutDir: true,
    rollupOptions: {
      input: {
        main: './src/main.tsx',
      },
    },
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5000',
      '/hubs': 'http://localhost:5000',
    },
  },
})