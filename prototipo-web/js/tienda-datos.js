// Maqueta: los pedidos viven en localStorage y las dos pestañas se avisan con el evento "storage".
// En el producto real eso lo hace el servidor (SignalR o SSE). Acá solo se simula.
(function () {
  const CLAVE_PEDIDOS = "sc_tienda_pedidos";
  const CLAVE_EVENTO = "sc_tienda_evento";

  const local = {
    nombre: "Lo de Pepe",
    sucursal: "Almagro",
    alias: "lo-de-pepe-almagro",
    direccion: "Av. Rivadavia 3850, Almagro, CABA",
    horario: "Lun a Sáb 8 a 21 · Dom 9 a 13",
    whatsapp: "5491155550101",
    envio: "Retiro en el local · Envío hasta 5 km",
    lat: -34.6105,
    lng: -58.4216,
    aliasTransferencia: "lo.de.pepe.mp"
  };

  const productos = [
    { id: 1, codigo: "7790070012345", nombre: "Leche entera 1 L", grupo: "Lácteos", precio: 1450, stock: "ok" },
    { id: 2, codigo: "7790070099887", nombre: "Yogur bebible frutilla 1 L", grupo: "Lácteos", precio: 2100, stock: "poco" },
    { id: 3, codigo: "7791234500011", nombre: "Queso cremoso x kg", grupo: "Lácteos", precio: 8900, stock: "ok" },
    { id: 4, codigo: "7790895000997", nombre: "Gaseosa cola 2,25 L", grupo: "Bebidas", precio: 3200, stock: "ok" },
    { id: 5, codigo: "7790895001222", nombre: "Agua sin gas 1,5 L", grupo: "Bebidas", precio: 1100, stock: "ok" },
    { id: 6, codigo: "7792799000018", nombre: "Cerveza rubia lata 473 ml", grupo: "Bebidas", precio: 1800, stock: "sin" },
    { id: 7, codigo: "7790040133456", nombre: "Galletitas de agua", grupo: "Almacén", precio: 950, stock: "ok" },
    { id: 8, codigo: "7790580120034", nombre: "Fideos spaghetti 500 g", grupo: "Almacén", precio: 1250, stock: "ok" },
    { id: 9, codigo: "7790060023456", nombre: "Arroz largo fino 1 kg", grupo: "Almacén", precio: 1600, stock: "poco" },
    { id: 10, codigo: "7790310001234", nombre: "Yerba mate 1 kg", grupo: "Almacén", precio: 4800, stock: "ok" },
    { id: 11, codigo: "7790387000123", nombre: "Aceite de girasol 1,5 L", grupo: "Almacén", precio: 3900, stock: "ok" },
    { id: 12, codigo: "7791290012345", nombre: "Detergente 750 ml", grupo: "Limpieza", precio: 2300, stock: "ok" },
    { id: 13, codigo: "7790990000222", nombre: "Lavandina 2 L", grupo: "Limpieza", precio: 1500, stock: "poco" },
    { id: 14, codigo: "7793100120001", nombre: "Papel higiénico x4", grupo: "Limpieza", precio: 2700, stock: "ok" }
  ];

  const estados = {
    nuevo: "Nuevo",
    preparando: "Preparando",
    listo: "Listo para retirar",
    entregado: "Entregado",
    cancelado: "Cancelado"
  };
  const recorrido = ["nuevo", "preparando", "listo", "entregado"];

  function pedidos() {
    try {
      return JSON.parse(localStorage.getItem(CLAVE_PEDIDOS) || "[]");
    } catch {
      return [];
    }
  }

  function guardar(lista) {
    localStorage.setItem(CLAVE_PEDIDOS, JSON.stringify(lista));
  }

  let ultimoVisto = JSON.parse(localStorage.getItem(CLAVE_EVENTO) || "{}").ts || 0;

  function emitir(evento) {
    ultimoVisto = Date.now();
    localStorage.setItem(CLAVE_EVENTO, JSON.stringify({ ...evento, ts: ultimoVisto }));
  }

  function escuchar(fn) {
    const revisar = () => {
      const evento = JSON.parse(localStorage.getItem(CLAVE_EVENTO) || "{}");
      if (!evento.ts || evento.ts <= ultimoVisto) return;
      ultimoVisto = evento.ts;
      fn(evento);
    };
    window.addEventListener("storage", (e) => {
      if (e.key === CLAVE_EVENTO) revisar();
    });
    setInterval(revisar, 1500);
  }

  function codigoSeguimiento() {
    const letras = "abcdefghjkmnpqrstuvwxyz23456789";
    let c = "";
    for (let i = 0; i < 16; i++) c += letras[Math.floor(Math.random() * letras.length)];
    return c;
  }

  function crearPedido(datos) {
    const lista = pedidos();
    const ahora = new Date().toISOString();
    const pedido = {
      id: 1040 + lista.length + 1,
      codigo: codigoSeguimiento(),
      fecha: ahora,
      estado: "nuevo",
      historial: [{ estado: "nuevo", fecha: ahora }],
      ...datos
    };
    lista.push(pedido);
    guardar(lista);
    emitir({ tipo: "pedido-nuevo", id: pedido.id });
    return pedido;
  }

  function cambiarEstado(id, estado) {
    const lista = pedidos();
    const p = lista.find((x) => x.id === id);
    if (!p) return null;
    p.estado = estado;
    p.historial.push({ estado, fecha: new Date().toISOString() });
    guardar(lista);
    emitir({ tipo: "estado", id, estado });
    return p;
  }

  function porCodigo(codigo) {
    return pedidos().find((p) => p.codigo === codigo) || null;
  }

  function dinero(n) {
    return "$ " + Number(n).toLocaleString("es-AR", { minimumFractionDigits: 0, maximumFractionDigits: 2 });
  }

  function hora(iso) {
    return new Date(iso).toLocaleTimeString("es-AR", { hour: "2-digit", minute: "2-digit" });
  }

  function iniciales(nombre) {
    return nombre
      .split(" ")
      .slice(0, 2)
      .map((p) => p[0])
      .join("")
      .toUpperCase();
  }

  function aplicarTema() {
    const guardado = localStorage.getItem("sc_tema");
    const oscuro = guardado ? guardado === "oscuro" : matchMedia("(prefers-color-scheme: dark)").matches;
    document.documentElement.setAttribute("data-tema", oscuro ? "oscuro" : "claro");
  }

  function alternarTema() {
    const oscuro = document.documentElement.getAttribute("data-tema") === "oscuro";
    localStorage.setItem("sc_tema", oscuro ? "claro" : "oscuro");
    aplicarTema();
  }

  aplicarTema();

  window.Tienda = {
    local,
    productos,
    estados,
    recorrido,
    pedidos,
    crearPedido,
    cambiarEstado,
    porCodigo,
    escuchar,
    dinero,
    hora,
    iniciales,
    alternarTema
  };
})();
