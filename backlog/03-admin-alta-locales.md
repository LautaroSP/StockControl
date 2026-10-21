# Admin: alta de locales

- **Roles:** admin
- **Tipo:** tu panel

## Qué

Vos das de alta un local y lo asociás a un dueño. El dueño no crea sucursales solo.

## Flujo

1. Alta (o búsqueda) del dueño: nombre, mail, usuario.
2. Alta del local: nombre, dirección/barrio, dueño, fecha de cobro.
3. El dueño entra y ya ve ese local.

## Criterios de aceptación

- [x] Panel admin: lista de dueños registrados. Un clic expande sus locales.
- [x] Alta de un local para un dueño. El primero queda en prueba de 7 días. Si ya tiene un local, el siguiente nace pendiente de pago: el trial es uno por dueño.
- [x] Al crear otro local se pueden copiar los productos de uno que ya tenga. Pregunta si el stock queda igual o en 0.
- [x] Dar rol admin solo con la clave secreta (`Admin:Clave` en entorno, no en git).
- [x] Bloquear a un usuario: no puede entrar ni seguir usando la API.
- [x] Eliminar un usuario. No se puede eliminar ni bloquear al propio, ni dejar la plataforma sin admin.
- [x] Botón para ver los empleados y socios de ese dueño.
- [x] Panel admin: listado de locales (cliente, estado, vence, cantidad de empleados).
- [x] Alta de local obliga a un dueño existente o crea el dueño en el mismo paso.
- [x] Editar vencimiento / marcar pago recibido (transferencia, efectivo, “hablé con ventas”).
- [x] Ver y editar el teléfono de “Contactá a ventas” que ve el dueño (tarjeta 22).
- [x] Poder dejar un local en trial 7 días o al día un mes (mismo día del mes siguiente, misma hora; `ServicioAbono.VencePorPago`) sin pasar por el QR.
- [x] No se mezcla el catálogo ni las ventas entre locales.
- [x] Admin puede desactivar un local (deuda, baja) sin borrar el histórico.
- [x] Admin ve el mes ≈ locales pagos × $ 14.999 (los trial no cuentan como cobrado). Viene de la tarjeta 02.
