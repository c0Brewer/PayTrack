ARG NODE_BUILD_IMAGE=cgr.dev/chainguard/node:latest-dev
ARG NODE_RUNTIME_IMAGE=cgr.dev/chainguard/node:latest

FROM ${NODE_BUILD_IMAGE} AS build
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

FROM ${NODE_RUNTIME_IMAGE} AS runtime
WORKDIR /app

COPY --from=build /app/dist/pay-track/browser ./dist
COPY server.mjs ./server.mjs

EXPOSE 8080

ENTRYPOINT ["node", "/app/server.mjs"]
