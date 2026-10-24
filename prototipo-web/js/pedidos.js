const T = window.Tienda;
const $ = (id) => document.getElementById(id);
const CLAVE_AVISOS = "sc_tienda_avisos";

const columnas = [
  { estado: "nuevo", titulo: "Nuevos" },
  { estado: "preparando", titulo: "Preparando" },
  { estado: "listo", titulo: "Listos para retirar" },
  { estado: "entregado", titulo: "Entregados hoy" }
];

const siguiente = {
  nuevo: { estado: "preparando", texto: "Preparar" },
  preparando: { estado: "listo", texto: "Listo" },
  listo: { estado: "entregado", texto: "Entregado" }
};

let avisos = JSON.parse(localStorage.getItem(CLAVE_AVISOS) || "[]");
let recienLlegado = null;

function guardarAvisos() {
  localStorage.setItem(CLAVE_AVISOS, JSON.stringify(avisos));
}

function pintarTablero() {
  const pedidos = T.pedidos();
  const cancelados = pedidos.filter((p) => p.estado === "cancelado").length;
  $("tablero").innerHTML =
    columnas
      .map((c) => {
        const lista = pedidos.filter((p) => p.estado === c.estado).reverse();
        return `<section class="columna">
          <h2>${c.titulo} <span class="meta">${lista.length}</span></h2>
          ${lista.map(tarjeta).join("") || '<p class="meta vacio">Sin pedidos</p>'}
        </section>`;
      })
      .join("") + (cancelados ? `<p class="meta">${cancelados} cancelados</p>` : "");
}

function tarjeta(p) {
  const paso = siguiente[p.estado];
  const telefono = p.telefono.replace(/\D/g, "");
  const mensaje = encodeURIComponent(`Hola ${p.cliente}, te escribimos de ${T.local.nombre} por tu pedido #${p.id}.`);
  return `<article class="panel pedido-card ${p.id === recienLlegado ? "recien" : ""}">
    <div class="row">
      <strong class="grow">#${p.id} · ${p.cliente}</strong>
      <span class="meta">${T.hora(p.fecha)}</span>
    </div>
    <ul class="pedido-items">${p.items.map((i) => `<li>${i.cantidad} × ${i.nombre}</li>`).join("")}</ul>
    <div class="row">
      <span class="badge ${p.pago.includes("confirmar") ? "wait" : ""}">${p.pago}</span>
      <span class="meta grow">${p.entrega}</span>
      <strong>${T.dinero(p.total)}</strong>
    </div>
    <div class="row pedido-acciones">
      ${paso ? `<button class="btn btn-primary" type="button" data-id="${p.id}" data-estado="${paso.estado}">${paso.texto}</button>` : ""}
      ${p.estado !== "entregado" ? `<button class="btn btn-danger" type="button" data-id="${p.id}" data-estado="cancelado">Cancelar</button>` : ""}
      <a class="btn" href="https://wa.me/549${telefono}?text=${mensaje}" target="_blank" rel="noopener">WhatsApp</a>
    </div>
  </article>`;
}

function pintarAvisos() {
  const noLeidas = avisos.filter((a) => !a.leida).length;
  $("contador").hidden = noLeidas === 0;
  $("contador").textContent = noLeidas > 9 ? "9+" : noLeidas;
  $("avisos").innerHTML = avisos.length
    ? avisos
        .slice()
        .reverse()
        .map(
          (a) => `<button type="button" class="aviso ${a.leida ? "" : "no-leida"}" data-aviso="${a.id}">
            <span>${a.texto}</span><span class="meta">${T.hora(a.fecha)}</span>
          </button>`
        )
        .join("")
    : '<p class="meta pad">No hay avisos.</p>';
}

function sonar() {
  if (!$("chkSonido").checked) return;
  const ctx = new (window.AudioContext || window.webkitAudioContext)();
  [880, 1320].forEach((f, i) => {
    const osc = ctx.createOscillator();
    const vol = ctx.createGain();
    osc.frequency.value = f;
    vol.gain.setValueAtTime(0.15, ctx.currentTime + i * 0.18);
    vol.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + i * 0.18 + 0.16);
    osc.connect(vol).connect(ctx.destination);
    osc.start(ctx.currentTime + i * 0.18);
    osc.stop(ctx.currentTime + i * 0.18 + 0.17);
  });
}

function notificarNavegador(p) {
  if (!("Notification" in window) || Notification.permission !== "granted") return;
  const n = new Notification(`Pedido nuevo #${p.id}`, {
    body: `${p.cliente} · ${p.items.length} productos · ${T.dinero(p.total)}`,
    tag: `pedido-${p.id}`
  });
  n.onclick = () => {
    window.focus();
    n.close();
  };
}

function toast(msg) {
  $("toast").textContent = msg;
  $("toast").classList.add("show");
  setTimeout(() => $("toast").classList.remove("show"), 3500);
}

function llegoPedido(id) {
  const p = T.pedidos().find((x) => x.id === id);
  if (!p) return;
  avisos.push({ id: Date.now(), pedido: p.id, texto: `Pedido nuevo #${p.id} de ${p.cliente} · ${T.dinero(p.total)}`, fecha: new Date().toISOString(), leida: false });
  guardarAvisos();
  recienLlegado = p.id;
  pintarTablero();
  pintarAvisos();
  toast(`Entró el pedido #${p.id} de ${p.cliente}`);
  sonar();
  notificarNavegador(p);
  setTimeout(() => {
    recienLlegado = null;
    pintarTablero();
  }, 4000);
}

function simularPedido() {
  const nombres = ["María López", "Juan Pérez", "Sofía Gómez", "Carlos Díaz", "Lucía Fernández"];
  const disponibles = T.productos.filter((p) => p.stock !== "sin");
  const items = disponibles
    .sort(() => Math.random() - 0.5)
    .slice(0, 2 + Math.floor(Math.random() * 3))
    .map((p) => ({ nombre: p.nombre, cantidad: 1 + Math.floor(Math.random() * 3), precio: p.precio }));
  const pagos = ["Pago al retirar", "Mercado Pago (pagado)", "Transferencia a confirmar"];
  const p = T.crearPedido({
    cliente: nombres[Math.floor(Math.random() * nombres.length)],
    telefono: "11 5555-" + String(1000 + Math.floor(Math.random() * 8999)),
    mail: "",
    entrega: "Retiro en el local",
    pago: pagos[Math.floor(Math.random() * pagos.length)],
    items,
    envio: 0,
    total: items.reduce((s, i) => s + i.precio * i.cantidad, 0)
  });
  llegoPedido(p.id);
}

async function activarAvisos() {
  if (!("Notification" in window)) {
    toast("Este navegador no tiene avisos.");
    return;
  }
  const permiso = await Notification.requestPermission();
  pintarBotonAvisos();
  if (permiso === "granted") toast("Listo: te avisamos aunque estés en otra pestaña.");
}

function pintarBotonAvisos() {
  const permiso = "Notification" in window ? Notification.permission : "denied";
  $("btnAvisos").textContent = permiso === "granted" ? "Avisos del navegador: activos" : permiso === "denied" ? "Avisos bloqueados" : "Activar avisos del navegador";
  $("btnAvisos").disabled = permiso !== "default";
}

document.addEventListener("click", (e) => {
  const boton = e.target.closest("[data-estado]");
  if (boton) {
    const id = Number(boton.dataset.id);
    const estado = boton.dataset.estado;
    T.cambiarEstado(id, estado);
    pintarTablero();
    toast(estado === "entregado" ? `#${id} entregado: entra como venta (stock, informes y cierre).` : `#${id}: ${T.estados[estado]}`);
    return;
  }
  const aviso = e.target.closest("[data-aviso]");
  if (aviso) {
    const a = avisos.find((x) => x.id === Number(aviso.dataset.aviso));
    if (a) a.leida = true;
    guardarAvisos();
    pintarAvisos();
    $("listaAvisos").hidden = true;
    recienLlegado = a?.pedido ?? null;
    pintarTablero();
  }
  if (!e.target.closest(".campanita-wrap")) $("listaAvisos").hidden = true;
});

$("btnCampana").addEventListener("click", () => ($("listaAvisos").hidden = !$("listaAvisos").hidden));
$("marcarTodas").addEventListener("click", () => {
  avisos.forEach((a) => (a.leida = true));
  guardarAvisos();
  pintarAvisos();
});
$("btnSimular").addEventListener("click", simularPedido);
$("btnAvisos").addEventListener("click", activarAvisos);

T.escuchar((evento) => {
  if (evento.tipo === "pedido-nuevo") llegoPedido(evento.id);
  else pintarTablero();
});

pintarBotonAvisos();
pintarTablero();
pintarAvisos();
