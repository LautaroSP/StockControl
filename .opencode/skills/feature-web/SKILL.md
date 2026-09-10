---
name: feature-web
description: Implementa una tarjeta del backlog web con cambio mínimo, nombres en español y tests xUnit del dominio. Usar al codear API/Angular, una feature de StockControl web, o una tarjeta de backlog/.
---

# Feature web

Al implementar una tarjeta de `backlog/` o código en `src/`:

1. Leer la tarjeta y AGENTS.md (español, secretos, tests, seguridad).
2. Cambio **mínimo**. No tocar `StockControl/` (WinForms) ni inflar el prototipo.
3. Nombres en español (`Producto`, `/ventas`, `caja`).
4. Test xUnit del comportamiento en el **mismo** cambio.
5. Si toca auth, `.env`, contacto o Mercado Pago: seguir también `seguridad-web`.
6. Si toca Angular/CSS: variables de color y modo oscuro (tarjeta 23 / AGENTS.md front público).

No dar la feature por cerrada sin test del dominio.
