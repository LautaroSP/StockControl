# SQL injection

- **Roles:** —
- **Tipo:** seguridad API y base de datos

## Qué

Revisar y dejar cerrado que ningún dato del usuario llegue a la base como texto SQL.

Hoy las consultas van por EF con LINQ, que manda los valores como parámetros. El SQL escrito a mano es poco:

- `ImportadorSqlite.AjustarSeriales`: `ExecuteSqlRawAsync` con texto fijo, sin datos del usuario.
- `CobrosMercadoPagoApp.Confirmar`: `SqlQuery<long>($"… pg_advisory_xact_lock({idCobro}) …")`. Interpolado con `FormattableString`, que EF convierte en parámetro.
- Búsquedas con `EF.Functions.ILike` en productos, clientes y cuentas: el texto va como parámetro, pero los comodines `%` y `_` que escribe el usuario no se escapan. No es inyección, pero alguien puede forzar búsquedas que recorran toda la tabla.

## Criterios de aceptación

- [ ] Inventario de todo el SQL crudo (`FromSqlRaw`, `ExecuteSqlRaw`, `SqlQueryRaw`, `NpgsqlCommand`). Ninguno concatena ni interpola datos de afuera. Con datos variables se usan `FromSql`, `SqlQuery` o `ExecuteSql` (interpolación parametrizada).
- [ ] Regla en `.cursor/rules`: prohibido `*Raw` con concatenación o `string.Format`; el `Raw` solo con texto fijo.
- [ ] Las búsquedas `ILike` escapan `%`, `_` y `\` con una función del dominio, con test xUnit.
- [ ] Los textos de búsqueda tienen largo máximo (por ejemplo 80) y se cortan antes de consultar.
- [ ] El usuario de Postgres que usa la app no es superusuario ni dueño del esquema. Las migraciones corren con otro usuario o en el deploy. Queda escrito en el manual de deploy.
- [ ] En producción, un error de la base no devuelve el mensaje de Npgsql ni la traza al cliente: responde 500 genérico y lo deja en el log.

## Fuera

- WAF o filtros de texto en el proxy.
