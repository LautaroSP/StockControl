# Stack y migración

Decisión de tecnología para el web. El escritorio (WinForms + SQLite) sigue; el web es otro producto con **los mismos datos de negocio** para poder importar un `stock.db`.

## Qué usamos

| Capa | Elección | Por qué |
|---|---|---|
| API | **ASP.NET Core 8** (Web API) | Mismo runtime que el escritorio (`net8.0-windows`). C# compartible a nivel de reglas (descuento, sector, grupos). |
| Front | **Angular** (standalone, TypeScript) | Pantallas con estado (caja, varios carritos, filtros). Encaja con el prototipo. |
| Base | **PostgreSQL 16+** | Varios locales, varios cajeros a la vez, backups, índices. Escala más que SQLite. |
| Acceso a datos | **EF Core 8 + Npgsql** | Migraciones versionadas. El escritorio usa Dapper; el web no hace falta copiar eso. |
| Auth | **ASP.NET Identity + JWT** | Usuario/contraseña. Roles: `admin`, `dueno`, `empleado`. Claim `localId` actual. |
| Host (después) | Linux + Nginx + Postgres (Hetzner o similar) | Un API, un Angular estático, un Postgres. |

Estilo: reutilizar `prototipo-web/css/estilos.css` (verde / papel), no Material de fábrica. Angular arma las pantallas; el look ya está.

No en v1: débito automático MP, QR de mostrador integrado, app nativa, facturación AFIP.

---

## Cómo queda el repo

```
StockControl/          ← WinForms (no se toca para el web)
prototipo-web/         ← maqueta, se mira, no se “compila”
backlog/               ← tarjetas
src/
  StockControl.Api/    ← ASP.NET
  StockControl.Web/    ← Angular
```

El escritorio **no** apunta a Postgres. La migración es un import: un `.db` → un local nuevo en la nube.

---

## Postgres: mismas tablas, más columnas

Regla: **mismos nombres de tabla y de campo de negocio**. Se agrega `IdLocal` (y usuarios, abono, etc.). Así el import es columna a columna.

Tipos: SQLite `REAL` → `numeric(14,2)` (plata) o `numeric(14,3)` (cantidad). `INTEGER` 0/1 → `boolean`. Fechas → `timestamptz`.

### Lo que viene del escritorio (núcleo)

**Productos** (hoy)

- `Id`, `Codigo`, `Nombre`, `Cantidad`, `Costo`, `Precio`
- `ProductoSector`, `IdGrupoProducto`, `GananciaIndividual`, `ValorGanancia`, `FechaModificacion`

Web: `+ IdLocal`. Único: `(IdLocal, Codigo)`.

**GrupoProductos**

- `IdGrupoProducto`, `NombreGrupo`, `PrecioGrupo`, `Costo`, `Ganancia`, `GananciaIndividual`

Web: `+ IdLocal`.

**MetodosPago**

- `Id`, `Descripcion`

Web: `+ IdLocal`, `Activo` (no borrar si hay ventas).

**InformeVenta**

- `IdInformeVenta`, `Fecha`, `Total`, `MetodoPago`
- `MultipleMetodoDePago`, `DetalleAdjunto`, `Descuento`, `Subtotal`, `PrecioCosto`

Web: `+ IdLocal`, `IdUsuario` (quién cobró).

**InformeVentaDetalle**

- `IdInformeVentaDetalle`, `IdInformeVenta`, `Codigo`, `Nombre`, `Cantidad`, `Costo`, `Precio`, `SubTotal`

En SQLite `Costo` del detalle es `TEXT`; en Postgres `numeric` (vacío → null). Web: opcional `IdProducto`.

**Cajas**

- `IdCaja`, `Fecha`, `Total`, `MetodoPago`, `CantidadVentas`

Web: `+ IdLocal`, `NroCaja` (correlativo por local), `IdUsuarioCierre`.

**Configuracion**

Hoy es global: `Clave` / `Valor` (`NombreLocal`, `FactorGanancia`, `IVA`).

Web: `IdLocal` + `Clave` + `Valor` (una fila por sucursal). Claves nuevas: stock rígido, umbral, ticket POS/A4, empleado puede precios, etc.

### Lo nuevo (multitenant / cobro)

No existe en el `.db` del escritorio; se crea al importar o al dar de alta:

| Tabla | Para qué |
|---|---|
| `Cuentas` | Dueño (mail, etc.) |
| `Locales` | Sucursal, estado abono (`trial` / `al_dia` / `vencido`), vence |
| `Usuarios` | Login, rol, hash de clave |
| `UsuarioLocales` | Ana en Almagro; Pedro en dos sucursales |
| `Abonos` | Pagos de $ 9.999 (QR / marca admin) |

Un `.db` importado = **1 cuenta + 1 local + 1 dueño** + todas las filas de producto/venta con ese `IdLocal`.

---

## Import limpio (un cliente que ya usa el .exe)

1. Admin: alta de dueño + local (o el dueño paga y vos importás).
2. Subís `stock.db`.
3. Script: lee SQLite (mismas tablas) → inserta en Postgres con el `IdLocal` nuevo.
4. IDs: se pueden **conservar** dentro del local (`Id` de producto 17 sigue 17) porque el único global es `(IdLocal, Id)` o se regeneran y se reescribe `IdGrupoProducto` / `IdInformeVenta`. Conservar es más simple para el dueño.
5. Configuración key-value → filas de ese local.
6. Crear usuario dueño; empleados se cargan después (el .exe no tiene usuarios).

Si el código de producto se duplica en el mismo `.db`, el import avisa y no pisa.

---

## Angular: un módulo por zona del prototipo

| Ruta | Pantalla prototipo | API |
|---|---|---|
| `/login` | `login.html` | `POST /auth/login` |
| `/locales` | `locales.html` | `GET /locales` |
| `/caja` | `caja.html` | productos, `POST /ventas` |
| `/productos` | `productos.html` | CRUD productos, stock |
| `/grupos` | `grupos.html` | grupos |
| `/informes` | `informes.html` | ventas, `POST /cajas/cerrar` |
| `/estadisticas` | `estadisticas.html` | agregados |
| `/empleados` | `empleados.html` | usuarios del dueño |
| `/configuracion` | `configuracion.html` | config del local |
| `/plan` | `plan.html` | abono (después) |
| `/admin/*` | `admin.html` | solo rol admin |

Guards: `dueno`, `empleado`, `admin`. El empleado no entra a `/estadisticas` (igual que el prototipo).

Caja: estado en el cliente (carritos en memoria). Al cobrar, un POST atómico: venta + detalle + stock.

---

## API (mínimo para arrancar)

Auth y tenancy: todo lo de negocio lleva el local del JWT. No se cruza un `IdLocal` a mano.

```
POST /auth/login
GET  /locales
POST /locales/{id}/entrar          ← elige sucursal (dueño)

GET  /productos
POST /productos
PUT  /productos/{id}
POST /productos/{id}/stock

POST /ventas                       ← cobrar
GET  /ventas
GET  /ventas/{id}

POST /cajas/cerrar
GET  /cajas
```

El resto (grupos, config, empleados, admin) se suma después del primer corte.

---

## Primer corte para “ya empezar”

No todo el backlog. Para que Jorge cobre en Chrome:

1. Postgres + tablas núcleo + `Locales` / `Usuarios`.
2. API login + productos + cobrar.
3. Angular: login, selector de local, caja, productos finos.
4. Herramienta de import SQLite (puede ser un `dotnet run -- import stock.db --local 1`).

Escritorio se sigue usando hasta que el import + caja estén sólidos.
