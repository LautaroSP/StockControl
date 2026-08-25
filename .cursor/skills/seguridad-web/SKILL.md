---
name: seguridad-web
description: Checklist de seguridad al tocar auth, deploy, .env, formulario de contacto o Mercado Pago en StockControl web. Usar en login, Identity, RLS, HTTPS, secretos, tokens MP o el form de contacto.
---

# Seguridad web

Antes de cerrar un cambio de auth, deploy, secretos o contacto:

- [ ] Ningún secreto en git (connection string, API keys, `.pfx`, `.env`)
- [ ] Contraseñas con Identity (hash)
- [ ] Inputs validados
- [ ] Rate limit y tope de página en listados
- [ ] Cabeceras de seguridad; HTTPS + HSTS en producción (Let’s Encrypt en Caddy/Nginx)
- [ ] Filtro `IdLocal` (EF) y RLS en Postgres
- [ ] Tokens MP del local cifrados (Data Protection); no el catálogo
- [ ] Postgres remoto solo con SSL; BD no abierta a internet
- [ ] Contacto: honeypot + rate limit (CAPTCHA si hace falta)

Local: HTTP en localhost está bien. No commitear certificados.
