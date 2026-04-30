ARG NODE_IMAGE=node:20-alpine
ARG NGINX_IMAGE=nginx:1.27-alpine

FROM ${NODE_IMAGE} AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
ARG GOOGLE_CLIENT_ID
ARG API_BASE_URL=
ENV GOOGLE_CLIENT_ID=${GOOGLE_CLIENT_ID}
ENV API_BASE_URL=${API_BASE_URL}
ENV NODE_ENV=production
RUN npm run build -- --configuration production

FROM ${NGINX_IMAGE} AS runtime
WORKDIR /usr/share/nginx/html

COPY nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/pay-track/browser ./

EXPOSE 8080
