# Empleados y socios

- **Roles:** dueño, socio
- **Tipo:** cuenta

## Qué

El dueño carga todos los empleados que quiera: usuario + contraseña, y a qué local(es) entran.

El dueño también puede asignar el rol **socio**. El socio tiene los mismos permisos operativos que el dueño, pero no crea la cuenta ni administra el abono del servicio.

No hay límite de empleados. No cambia el abono.

## Criterios de aceptación

- [x] Dueño y socio: listado de usuarios (nombre, usuario, rol, locales, activo sí/no).
- [x] Alta: usuario, contraseña, rol y locales asignados (uno o varios).
- [x] El dueño puede asignar el rol socio; el socio no puede crear ni promover socios.
- [x] Dueño y socio pueden desactivar un empleado sin borrar el histórico de ventas de esa persona.
- [x] Dueño y socio pueden resetear contraseña según sus permisos.
- [x] El empleado solo ve los locales que le asignaron.
- [x] Un empleado no crea otros usuarios.
- [x] El dueño y el socio pueden operar caja.
