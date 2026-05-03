FROM node:18-alpine AS build
WORKDIR /app

COPY HomeManager.Frontend/package*.json ./
RUN npm ci

COPY HomeManager.Frontend/ ./
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY HomeManager.Frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/home-manager.frontend /usr/share/nginx/html

EXPOSE 80
