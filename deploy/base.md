# Base de datos

Dos usuarios de Postgres. Las claves van en el entorno, nunca en git ni en este archivo.

| Usuario | Para qué |
|---|---|
| `stockcontrol_migracion` | Dueño del esquema. Migraciones e importación. |
| `stockcontrol_app` | El API en producción. No es superusuario ni dueño. Sin `CREATEDB`, `CREATEROLE` ni `BYPASSRLS`. Solo lee y escribe datos. |

El script de ejemplo es [roles.sql](roles.sql). Se corre una vez con `psql`, conectado a la base, como superusuario. Después, las claves:

```
\password stockcontrol_migracion
\password stockcontrol_app
```

## Producción

El API no migra al arrancar. La cadena `ConnectionStrings__Default` usa `stockcontrol_app`. Si Postgres no está en la misma máquina, `SSL Mode=Require`.

Las migraciones y el comando `import` usan `stockcontrol_migracion` (el import ajusta secuencias con `setval`, y eso pide ser dueño):

```
ConnectionStrings__Default=Host=...;Database=...;Username=stockcontrol_migracion;Password=...;SSL Mode=Require
dotnet ef database update --project src/StockControl.Api
```

`import` también migra y luego llama a `setval`. Misma cadena, no la del API.

Las tablas quedan de `stockcontrol_migracion`. El usuario de la app no es dueño y no tiene `BYPASSRLS`: las políticas de la migración `AuditoriaSeguridad` le aplican. El dueño de las tablas y un superusuario las saltean (no hay `FORCE ROW LEVEL SECURITY`), así que el compose de desarrollo y las migraciones siguen igual.

Cada conexión del API escribe `app.id_local`, `app.id_usuario` y, si hace falta, `app.alcance=todo` o `app.locales`. Sin eso, una tabla con `IdLocal` no devuelve filas. `todo` lo usan el admin de plataforma, el webhook ya firmado, el registro, contratar un local y los trabajos de la tienda.

Para verlo con el usuario de la app:

```
SET ROLE stockcontrol_app;
SELECT set_config('app.id_local', '1', true);
SELECT count(*) FROM "Productos";
RESET ROLE;
```

## Desarrollo

Un solo usuario, dueño del esquema, alcanza. Con el entorno Development el API migra al arrancar. No hace falta el par de roles.
