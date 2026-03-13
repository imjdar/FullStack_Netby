
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
# Usamos las variables de entorno de Docker (URLs vacías = mismo origen)
COPY .env.docker .env
RUN npm run build

FROM nginx:stable-alpine
# Copiamos nuestra configuración personalizada de Nginx (reverse proxy + SPA)
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]