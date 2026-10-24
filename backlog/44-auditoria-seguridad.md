# Auditoría de seguridad

- **Roles:** —
- **Tipo:** seguridad (API, Angular, base y deploy)

## Qué

Última tarjeta antes de producción. Revisar todo el sistema contra las reglas de seguridad de `AGENTS.md` y dejar un informe con lo encontrado, la gravedad y qué se hizo. Lo crítico y lo alto se corrigen en esta tarjeta. Lo medio o bajo puede quedar como tarjeta nueva, anotada en History.

Incluye revisar que las tarjetas 42 (sesión) y 43 (SQL injection) quedaron cerradas.

## Criterios de aceptación

### Informe

- [ ] `backlog/44-auditoria-informe.md` con cada hallazgo: dónde, gravedad (crítica, alta, media, baja), cómo se reproduce y estado (corregido o tarjeta nueva).

### Autenticación y sesión

- [ ] Login con límite propio por IP y por usuario (hoy comparte el límite general de 120 por minuto). Freno tras intentos fallidos seguidos.
- [ ] Claves solo con `PasswordHasher` de Identity. Largo mínimo de clave al crear y resetear.
- [ ] Revisado lo de la 42: token con versión, emisor y audiencia.
- [ ] Decisión escrita sobre dónde vive el token (`localStorage` o cookie `httpOnly`) y por qué.

### Autorización y separación por local

- [ ] Cada endpoint tiene `RequireAuthorization` o está en una lista de públicos justificada (login, registro, contacto, webhooks, salud).
- [ ] Cada endpoint de dueño revisa el permiso en el API, no solo en Angular. Prueba: un empleado no puede llamar ninguno.
- [ ] Un usuario no lee ni modifica datos de otro local cambiando un id en la URL. Revisados uno por uno los usos de `IgnoreQueryFilters`.
- [ ] RLS en Postgres: `AGENTS.md` pide políticas por `IdLocal` además del filtro de EF y hoy no hay. Se implementan o queda la decisión escrita.

### Entradas y salidas

- [ ] Las DataAnnotations de los contratos no se ejecutan solas en minimal API (.NET 8). Agregar un filtro de validación para todos los endpoints con cuerpo.
- [ ] Todos los listados tienen tope de página.
- [ ] Errores sin traza ni detalles internos en producción.
- [ ] Angular: sin `innerHTML` ni `bypassSecurityTrust*` con datos del usuario.

### Cabeceras, CORS y transporte

- [ ] Además de las actuales (`X-Content-Type-Options`, `Referrer-Policy`, HSTS): `Content-Security-Policy` y `frame-ancestors` (o `X-Frame-Options`).
- [ ] CORS de producción configurable por entorno. Hoy solo permite `localhost:4200`, fijo en el código.
- [ ] HTTPS forzado en producción. Swagger solo en desarrollo.
- [ ] Postgres no expuesto a internet y con SSL si está en otra máquina.

### Secretos y terceros

- [ ] Escaneo del historial de git en busca de secretos (por ejemplo, gitleaks). Si aparece alguno, se rota.
- [ ] Tokens de Mercado Pago cifrados con Data Protection, y las claves de Data Protection guardadas fuera de git y persistidas en el servidor.
- [ ] Webhooks de Mercado Pago con firma verificada y sin efecto si llegan repetidos.
- [ ] Formulario de contacto y registro: cebo, límite por IP y captcha funcionando.
- [ ] Los logs no guardan claves, tokens ni datos de tarjetas.

### Dependencias y respaldo

- [ ] `dotnet list package --vulnerable` y `npm audit` sin vulnerabilidades altas o críticas.
- [ ] Backup de la base programado, con una restauración probada.

## Fuera

- Prueba de penetración externa contratada.
- Certificaciones (PCI, ISO). El sistema no guarda datos de tarjetas: los cobra Mercado Pago.
