# Iconos

- **Roles:** dueño, empleado (ven la barra); dueño (Configuración y Mercado Pago)
- **Tipo:** UI

## Qué

Íconos en la barra lateral y en cada bloque de Configuración del local. Mercado Pago usa su marca. Si el local ya vinculó la cuenta, se ve en verde con un check y el botón pasa a decir «Conectar otra cuenta».

Los colores de la interfaz siguen por variables CSS, para que el modo oscuro no se rompa. La marca de Mercado Pago puede quedar en sus colores.

Sin caso de uso nuevo: no lleva test de dominio.

## Criterios de aceptación

- [x] Cada entrada de la barra tiene un ícono junto al texto: Caja, Productos, Estadísticas, Grupos, Medios de pago, Proveedores, Usuarios, Configuración, Abono, Informes, Cajas, Cambiar caja y Locales.
- [x] En Configuración, cada título lleva ícono: Datos del local, Precios, Stock, Empleados, Tickets, Cajas, Mercado Pago y Carteles.
- [x] El bloque Mercado Pago muestra la marca de Mercado Pago.
- [x] Sin cuenta vinculada, el botón dice «Conectar Mercado Pago».
- [x] Con cuenta vinculada, se lee «Cuenta conectada» en verde (u otro color que resalte) con un check, y el botón dice «Conectar otra cuenta».
- [x] El empleado no ve el botón de conectar. Si llega a ver el estado, también lee «Cuenta conectada» con el check.
