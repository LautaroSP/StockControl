# Sesión: cortar el token al cambiar rol o clave

- **Roles:** dueño, socio, admin (cambian rol o clave). Todos los usuarios (su token).
- **Tipo:** seguridad API

## Qué

Hoy el JWT vale 12 horas. Desactivar un usuario corta el acceso en el próximo pedido, porque `MiddlewareSesion` lo revisa en la base. Pero si a un socio lo pasás a empleado, o le cambiás la clave, el token que ya tenía sigue valiendo con el rol viejo hasta que vence.

Cada usuario guarda una **versión de sesión**. El token la lleva. Cuando cambia algo que define qué puede hacer, la versión sube y los tokens anteriores dejan de servir.

## Criterios de aceptación

- [ ] El usuario guarda una versión de sesión y el JWT la incluye.
- [ ] `MiddlewareSesion` compara la versión del token con la de la base. Si no coincide, responde 401 y el front vuelve al login.
- [ ] Sube la versión al cambiar el rol, al cambiar o resetear la clave, al desactivar al usuario y al sacarlo de un local.
- [ ] Dueño y admin tienen «Cerrar sesiones» en la ficha del usuario: sube la versión sin cambiar nada más.
- [ ] «Salir» en el front sigue cerrando solo esa pestaña o navegador.
- [ ] El token valida emisor y audiencia (`ValidateIssuer` y `ValidateAudience` en `true`), con valores en configuración.
- [ ] El API no arranca si `Jwt:Clave` tiene menos de 32 caracteres.
- [ ] Test xUnit del dominio: un token con versión vieja se rechaza; cambiar el rol o la clave sube la versión.

## Fuera

- Refresh tokens.
- Mover el token de `localStorage` a una cookie `httpOnly`. Lo evalúa la auditoría (44).
