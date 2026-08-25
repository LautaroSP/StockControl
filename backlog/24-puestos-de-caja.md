# Puestos de caja

- **Roles:** dueño (cantidad) · empleado y dueño (elegir, vender, cerrar)
- **Tipo:** mostrador + config
- **Pisa:** el `NroCaja` de la [09](09-cierre-de-caja.md) deja de ser “cierre correlativo del local” y pasa a ser el **puesto** (Caja 1…N).

## Qué

El local tiene N cajas (puestos). Para vender hay que elegir una. El cierre es **solo de esa caja**. Dos personas pueden estar en la misma: se avisa, no se bloquea.

## Config (dueño)

- Cantidad de cajas del local (mínimo 1). Default 1 = un solo mostrador, como hoy.
- Bajar N no borra historial; no se puede elegir un nro mayor a N para vender.
- El dueño también elige caja para operar (no está obligado a tener empleados).

## Sesión

Después del local, **antes de cobrar**: elegir Caja 1…N.

Si otro usuario ya tiene esa caja en una sesión activa: aviso (“X está usando la Caja 2”) y se puede seguir.

La venta guarda **quién** (`IdUsuario`, ya está en `InformeVenta`) y **qué caja** (nro de puesto). Sin caja elegida no se cobra.

## Cierre

Se cierra la caja **elegida** en la sesión (hoy o día anterior). No se llevan las ventas de las otras.

Al cerrar, elegir desglose:

- por **medio de pago** (como la 09), o
- por **usuario** (quién cobró en ese puesto).

Siempre hay fila Total. Confirmación. Sin ventas abiertas de **esa** caja en el período: avisa, no cierra vacío.

## Criterios de aceptación

- [ ] Dueño setea cuántas cajas tiene el local (config).
- [ ] Empleado y dueño eligen caja para vender; sin elección no hay cobro.
- [ ] Dos en la misma caja: aviso con el nombre del otro; se permite.
- [ ] Cada venta registra usuario y nro de puesto.
- [ ] Cerrar solo sella las ventas abiertas de ese puesto / período.
- [ ] Desglose al cerrar: medio **o** usuario, más Total.
- [ ] Listado de cierres: nro de puesto, fecha, quién cerró, total. Click ve el desglose elegido.

## Fuera

Varios carritos en el mismo puesto (08). Empleados (05). Ticket (18).
