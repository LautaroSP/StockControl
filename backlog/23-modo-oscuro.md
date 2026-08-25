# Modo oscuro

- **Roles:** dueño · empleado · admin · visitante (sitio público)
- **Tipo:** UI — todas las pantallas Angular

## Qué

La app se ve en **claro** (el papel/verde del prototipo) y en **oscuro**. Un control a mano; se recuerda en el navegador.

No es un tema Material aparte: mismas variables CSS (`--ink`, `--paper`, `--forest`, …) con un segundo set.

## Criterios de aceptación

- [ ] Toggle visible (caja, catálogo, admin y páginas públicas).
- [ ] Preferencia en `localStorage` (no en el servidor).
- [ ] Si no hay preferencia, respetar `prefers-color-scheme` del sistema.
- [ ] Contraste legible: texto, stock bajo, botones COBRAR, sidebar.
- [ ] Al armar el front, el CSS nace con variables; no hardcodear colores que el oscuro no pueda pisar.
