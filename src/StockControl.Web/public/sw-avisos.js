// Avisos de la tienda con la pestaña cerrada (Web Push). Solo muestra el aviso y abre el pedido.

function rutaSegura(url) {
  return typeof url === 'string' && url.startsWith('/') && !url.startsWith('//') ? url : '/mi-tienda/pedidos';
}

self.addEventListener('push', (evento) => {
  let datos = {};
  try {
    datos = evento.data ? evento.data.json() : {};
  } catch {
    datos = {};
  }
  const url = rutaSegura(datos.url);
  evento.waitUntil(
    self.registration.showNotification(datos.titulo || 'StockControl', {
      body: datos.texto || 'Tenés un aviso nuevo de la tienda.',
      tag: url,
      data: { url }
    })
  );
});

self.addEventListener('notificationclick', (evento) => {
  evento.notification.close();
  const url = rutaSegura(evento.notification.data && evento.notification.data.url);
  evento.waitUntil(
    self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((ventanas) => {
      const abierta = ventanas.find((v) => new URL(v.url).origin === self.location.origin);
      if (!abierta) return self.clients.openWindow(url);
      abierta.postMessage({ url });
      return abierta.focus();
    })
  );
});
