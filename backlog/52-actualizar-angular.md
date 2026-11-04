# Actualizar Angular

- **Roles:** —
- **Tipo:** seguridad (dependencias del front)
- **Depende de:** 44 Auditoría de seguridad
- **Bloquea:** dejar npm audit de producción sin altas

## Qué

`npm audit` de lo que va al navegador marca altas en `@angular/*` 18.2. El arreglo publicado es 22.2.4. No hay parche dentro de la 18 ni de la 19. `npm audit fix --force` cambia de mayor y no se corrió en la 44.

A esta app le alcanzan las de XSS en plantillas, atributos SVG y host bindings. No hay SSR, ni i18n, ni `HttpTransferCache`. Esas últimas no se usan, pero viajan en el mismo paquete.

Los críticos de la herramienta de build (`piscina`, `tar`) no van en la imagen de nginx. No son esta tarjeta.

## Criterios de aceptación

- [ ] `@angular/*` en una versión que `npm audit --omit=dev` no marque como alta.
- [ ] Login, caja, tienda y contacto siguen andando después del salto.
- [ ] No se corre `npm audit fix --force` a ciegas: el salto es un cambio de mayor, revisado.

## Fuera

- Reescribir pantallas.
- Los paquetes que solo usa `ng serve` o el build.
