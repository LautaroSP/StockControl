# Auditoría de seguridad

- **Roles:** —
- **Tipo:** seguridad (API, Angular, base y deploy)

## Qué

Última tarjeta antes de producción. Revisar todo el sistema contra las reglas de seguridad de `AGENTS.md` y dejar un informe con lo encontrado, la gravedad y qué se hizo. Lo crítico y lo alto se corrigen en esta tarjeta. Lo medio o bajo puede quedar como tarjeta nueva, anotada en History.

Incluye revisar que las tarjetas 42 (sesión) y 43 (SQL injection) quedaron cerradas.

## Criterios de aceptación

### Informe

- [x] `backlog/44-auditoria-informe.md` con cada hallazgo: dónde, gravedad (crítica, alta, media, baja), cómo se reproduce y estado (corregido o tarjeta nueva).

### Autenticación y sesión

- [x] Login con límite propio por IP y por usuario (hoy comparte el límite general de 120 por minuto). Freno tras intentos fallidos seguidos.
- [x] Claves solo con `PasswordHasher` de Identity. Largo mínimo de clave al crear y resetear.
- [x] Revisado lo de la 42: token con versión, emisor y audiencia.
- [x] Decisión escrita sobre dónde vive el token (`localStorage` o cookie `httpOnly`) y por qué.

### Autorización y separación por local

- [x] Cada endpoint tiene `RequireAuthorization` o está en una lista de públicos justificada (login, registro, contacto, webhooks, salud).
- [x] Cada endpoint de dueño revisa el permiso en el API, no solo en Angular. Prueba: un empleado no puede llamar ninguno.
- [x] Un usuario no lee ni modifica datos de otro local cambiando un id en la URL. Revisados uno por uno los usos de `IgnoreQueryFilters`.
- [x] RLS en Postgres: `AGENTS.md` pide políticas por `IdLocal` además del filtro de EF y hoy no hay. Se implementan o queda la decisión escrita.

### Entradas y salidas

- [x] Las DataAnnotations de los contratos no se ejecutan solas en minimal API (.NET 8). Agregar un filtro de validación para todos los endpoints con cuerpo.
- [x] Todos los listados tienen tope de página.
- [x] Errores sin traza ni detalles internos en producción.
- [x] Angular: sin `innerHTML` ni `bypassSecurityTrust*` con datos del usuario.

### Cabeceras, CORS y transporte

- [x] Además de las actuales (`X-Content-Type-Options`, `Referrer-Policy`, HSTS): `Content-Security-Policy` y `frame-ancestors` (o `X-Frame-Options`).
- [x] CORS de producción configurable por entorno. Hoy solo permite `localhost:4200`, fijo en el código.
- [x] HTTPS forzado en producción. Swagger solo en desarrollo.
- [x] Postgres no expuesto a internet y con SSL si está en otra máquina.

### Secretos y terceros

- [x] Escaneo del historial de git en busca de secretos (por ejemplo, gitleaks). Si aparece alguno, se rota.
- [x] Tokens de Mercado Pago cifrados con Data Protection, y las claves de Data Protection guardadas fuera de git y persistidas en el servidor.
- [x] Webhooks de Mercado Pago con firma verificada y sin efecto si llegan repetidos.
- [x] Formulario de contacto y registro: cebo, límite por IP y captcha funcionando.
- [x] Los logs no guardan claves, tokens ni datos de tarjetas.

### Dependencias y respaldo

- [x] `dotnet list package --vulnerable` sin altas. `npm audit fix` sin `--force`. Las altas de Angular 18 no tienen parche en esa línea y quedan en la 52. Los críticos que quedan son de la herramienta de build y no van en la imagen.
- [x] Backup de la base programado, con una restauración probada.

## Fuera

- Prueba de penetración externa contratada.
- Certificaciones (PCI, ISO). El sistema no guarda datos de tarjetas: los cobra Mercado Pago.
