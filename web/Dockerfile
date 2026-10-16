# Dockerfile da tela (Vite + React + TS) em modo de desenvolvimento.
# O contexto de build é a pasta web/.
FROM node:22-alpine

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

EXPOSE 5173

# --host para aceitar conexões de fora do container.
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "5173"]
