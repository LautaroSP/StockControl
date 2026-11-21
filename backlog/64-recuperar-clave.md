# Recuperar contraseña

- **Roles:** dueño, socio (empleado: se la resetea el dueño)
- **Tipo:** auth
- **Depende de:** 01 Roles y permisos, 42 Sesión: cortar el token

## Qué

En el login, «¿Olvidaste tu contraseña?» lleva a `/recuperar`. La persona pone el mail o el usuario. Si es dueño o socio, activo y con el mail confirmado, le llega un link de un solo uso que vence en 10 minutos. Elige la contraseña nueva (mínimo 8) y las sesiones abiertas se cortan. Si estaba frenada por intentos fallidos, se destraba.

La respuesta es siempre la misma, exista o no la cuenta. Rate limit de 5 pedidos cada 15 minutos por IP, y no se manda otro link a la misma cuenta antes de un minuto. En la base queda el hash del token. Usa el mismo Resend que la confirmación del registro (`Correo:ApiKey`, `Correo:Desde`, `App:UrlWeb`); sin eso, la pantalla dice que escriban desde Contacto.

El empleado no tiene mail propio: se la resetea el dueño desde Usuarios, como hoy. El admin sigue con su clave secreta.

## Criterios de aceptación

- [x] Link en el login y pantalla para pedir el mail.
- [x] Mensaje igual exista o no la cuenta.
- [x] Link de un uso, 10 minutos, token con hash.
- [x] Cambiar la contraseña corta las sesiones (`VersionSesion`).
- [x] Rate limit.
- [x] Migración `RecuperarClave`.
- [x] Tests xUnit (`ServicioRecuperarClaveTests`).
- [ ] Probado con Resend y el navegador.
