import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Servidor de dev acessível de fora do container (--host) na porta 5173.
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5173,
  },
})
