FROM node:22-alpine AS build
ARG BUILD_ID=
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npx ng build --configuration=production \
  && ID="${BUILD_ID:-$(date -u +%Y%m%d%H%M%S)-$(head -c 6 /dev/urandom | od -An -tx1 | tr -d ' \n')}" \
  && printf '{"id":"%s"}\n' "$ID" > /app/dist/web/browser/version.json

FROM nginx:1.30.7-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
EXPOSE 80
