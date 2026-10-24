const T = window.Tienda;
const $ = (id) => document.getElementById(id);
const COSTO_ENVIO = 1500;

let carrito = JSON.parse(localStorage.getItem("sc_tienda_carrito") || "{}");
let filtroGrupo = "";
let texto = "";

function guardarCarrito() {
  localStorage.setItem("sc_tienda_carrito", JSON.stringify(carrito));
}

function textoStock(stock) {
  if (stock === "sin") return '<span class="stock sin">Sin stock</span>';
  if (stock === "poco") return '<span class="stock poco">Últimas unidades</span>';
  return '<span class="stock ok">Hay stock</span>';
}

function pintarDatosLocal() {
  const l = T.local;
  $("direccion").textContent = l.direccion;
  $("horario").textContent = l.horario;
  $("envio").textContent = l.envio;
  $("direccionMapa").textContent = l.direccion;
  $("whatsappLocal").href = `https://wa.me/${l.whatsapp}`;
  $("comoLlegar").href = `https://www.openstreetmap.org/?mlat=${l.lat}&mlon=${l.lng}#map=17/${l.lat}/${l.lng}`;
}

function pintarGrupos() {
  const grupos = [...new Set(T.productos.map((p) => p.grupo))];
  $("grupos").innerHTML = ["", ...grupos]
    .map((g) => `<button type="button" class="chip ${g === filtroGrupo ? "on" : ""}" data-grupo="${g}">${g || "Todo"}</button>`)
    .join("");
}

function visibles() {
  const q = texto.trim().toLowerCase();
  return T.productos.filter(
    (p) => (!filtroGrupo || p.grupo === filtroGrupo) && (!q || p.nombre.toLowerCase().includes(q) || p.codigo.includes(q))
  );
}

function pintarProductos() {
  const lista = visibles();
  $("cantResultados").textContent = `${lista.length} ${lista.length === 1 ? "producto" : "productos"}`;
  $("grilla").innerHTML = lista
    .map((p) => {
      const cant = carrito[p.id] || 0;
      const control =
        p.stock === "sin"
          ? '<button class="btn wide" type="button" disabled>Sin stock</button>'
          : cant
            ? `<div class="cantidad"><button class="btn" data-menos="${p.id}" type="button">−</button><strong>${cant}</strong><button class="btn" data-mas="${p.id}" type="button">+</button></div>`
            : `<button class="btn btn-primary wide" data-mas="${p.id}" type="button">Agregar</button>`;
      return `<article class="panel producto-card ${p.stock === "sin" ? "agotado" : ""}">
        <div class="producto-img" aria-hidden="true">${T.iniciales(p.nombre)}</div>
        <div class="producto-info">
          <span class="meta">${p.grupo}</span>
          <h3>${p.nombre}</h3>
          <div class="producto-precio">${T.dinero(p.precio)}</div>
          ${textoStock(p.stock)}
        </div>
        ${control}
      </article>`;
    })
    .join("");
}

function lineas() {
  return Object.entries(carrito)
    .map(([id, cantidad]) => ({ producto: T.productos.find((p) => p.id === Number(id)), cantidad }))
    .filter((l) => l.producto && l.cantidad > 0);
}

function subtotal() {
  return lineas().reduce((s, l) => s + l.producto.precio * l.cantidad, 0);
}

function pintarCarrito() {
  const ls = lineas();
  $("lineasCarrito").innerHTML = ls.length
    ? ls
        .map(
          (l) => `<div class="linea">
            <span>${l.cantidad} × ${l.producto.nombre}</span>
            <span class="row"><b>${T.dinero(l.producto.precio * l.cantidad)}</b><button class="btn btn-ghost" data-sacar="${l.producto.id}" type="button" aria-label="Sacar">×</button></span>
          </div>`
        )
        .join("")
    : '<p class="meta">Todavía no agregaste productos.</p>';
  $("totalCarrito").textContent = T.dinero(subtotal());
  $("btnConfirmar").disabled = ls.length === 0;
  $("cantMovil").textContent = ls.reduce((s, l) => s + l.cantidad, 0);
}

function cambiar(id, delta) {
  carrito[id] = Math.max(0, (carrito[id] || 0) + delta);
  if (!carrito[id]) delete carrito[id];
  guardarCarrito();
  pintarProductos();
  pintarCarrito();
}

function totalCheckout() {
  const envio = document.querySelector('input[name="entrega"]:checked').value === "envio";
  return subtotal() + (envio ? COSTO_ENVIO : 0);
}

function abrirCheckout() {
  $("totalCheckout").textContent = T.dinero(totalCheckout());
  $("modalCheckout").classList.add("show");
}

function confirmar(e) {
  e.preventDefault();
  const datos = Object.fromEntries(new FormData($("formCheckout")));
  if (datos.sitio) return;
  const envio = datos.entrega === "envio";
  const pedido = T.crearPedido({
    cliente: datos.nombre.trim(),
    telefono: datos.telefono.trim(),
    mail: datos.mail.trim(),
    entrega: envio ? `Envío a ${datos.direccion || "domicilio"}` : "Retiro en el local",
    pago: { local: "Pago al retirar", mp: "Mercado Pago (pagado)", transferencia: "Transferencia a confirmar" }[datos.pago],
    items: lineas().map((l) => ({ nombre: l.producto.nombre, cantidad: l.cantidad, precio: l.producto.precio })),
    envio: envio ? COSTO_ENVIO : 0,
    total: totalCheckout()
  });
  carrito = {};
  guardarCarrito();
  $("modalCheckout").classList.remove("show");
  history.replaceState(null, "", `tienda.html?pedido=${pedido.codigo}`);
  mostrarSeguimiento(pedido.codigo);
}

function mostrarSeguimiento(codigo) {
  const p = T.porCodigo(codigo);
  if (!p) return;
  $("vistaCatalogo").hidden = true;
  $("vistaSeguimiento").hidden = false;
  $("segTitulo").textContent = `Pedido #${p.id}`;
  $("segSub").textContent = `${p.cliente} · ${p.entrega} · ${p.pago}`;
  const actual = T.recorrido.indexOf(p.estado);
  $("pasos").innerHTML = T.recorrido
    .map((e, i) => {
      const paso = p.historial.find((h) => h.estado === e);
      const clase = p.estado === "cancelado" ? "" : i < actual ? "hecho" : i === actual ? "actual" : "";
      return `<li class="${clase}"><strong>${T.estados[e]}</strong><span class="meta">${paso ? T.hora(paso.fecha) : ""}</span></li>`;
    })
    .join("");
  $("segCancelado").hidden = p.estado !== "cancelado";
  $("segPago").innerHTML = p.pago.startsWith("Transferencia")
    ? `<div class="banner trial"><span>Transferí ${T.dinero(p.total)} al alias <strong>${T.local.aliasTransferencia}</strong>. El local confirma cuando le llega.</span></div>`
    : "";
  $("segLineas").innerHTML =
    p.items.map((i) => `<tr><td>${i.cantidad} × ${i.nombre}</td><td class="num">${T.dinero(i.precio * i.cantidad)}</td></tr>`).join("") +
    (p.envio ? `<tr><td>Envío</td><td class="num">${T.dinero(p.envio)}</td></tr>` : "");
  $("segTotal").textContent = T.dinero(p.total);
  $("segLink").value = `https://stockcontrol.com.ar/tienda/${T.local.alias}/pedido/${p.codigo}`;
  const mensaje = `Hola! Hice el pedido #${p.id} en la tienda online: ${p.items.map((i) => `${i.cantidad} ${i.nombre}`).join(", ")}. Total ${T.dinero(p.total)}.`;
  $("segWhatsapp").href = `https://wa.me/${T.local.whatsapp}?text=${encodeURIComponent(mensaje)}`;
}

function iniciarMapa() {
  const l = T.local;
  if (!window.L) {
    $("mapa").innerHTML = '<p class="meta pad">Mapa de OpenStreetMap (necesita internet)</p>';
    return;
  }
  const mapa = L.map("mapa", { scrollWheelZoom: false }).setView([l.lat, l.lng], 16);
  L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
    maxZoom: 19,
    attribution: "© OpenStreetMap contributors"
  }).addTo(mapa);
  L.marker([l.lat, l.lng]).addTo(mapa).bindPopup(`${l.nombre} — ${l.sucursal}`);
}

function toast(msg) {
  $("toast").textContent = msg;
  $("toast").classList.add("show");
  setTimeout(() => $("toast").classList.remove("show"), 3000);
}

document.addEventListener("click", (e) => {
  const t = e.target.closest("[data-mas],[data-menos],[data-sacar],[data-grupo]");
  if (!t) return;
  if (t.dataset.mas) cambiar(Number(t.dataset.mas), 1);
  if (t.dataset.menos) cambiar(Number(t.dataset.menos), -1);
  if (t.dataset.sacar) cambiar(Number(t.dataset.sacar), -carrito[t.dataset.sacar]);
  if (t.dataset.grupo !== undefined) {
    filtroGrupo = t.dataset.grupo;
    pintarGrupos();
    pintarProductos();
  }
});

$("buscar").addEventListener("input", (e) => {
  texto = e.target.value;
  pintarProductos();
});
$("btnConfirmar").addEventListener("click", abrirCheckout);
$("cancelarCheckout").addEventListener("click", () => $("modalCheckout").classList.remove("show"));
$("formCheckout").addEventListener("submit", confirmar);
$("formCheckout").addEventListener("change", (e) => {
  if (e.target.name === "entrega") $("campoDireccion").hidden = e.target.value !== "envio";
  $("totalCheckout").textContent = T.dinero(totalCheckout());
});
function pintarBotonTema() {
  $("btnTema").textContent = document.documentElement.getAttribute("data-tema") === "oscuro" ? "Modo claro" : "Modo oscuro";
}
$("btnTema").addEventListener("click", () => {
  T.alternarTema();
  pintarBotonTema();
});
pintarBotonTema();
$("btnCarritoMovil").addEventListener("click", () => $("panelCarrito").classList.add("abierto"));
$("cerrarCarrito").addEventListener("click", () => $("panelCarrito").classList.remove("abierto"));
$("btnArrepentimiento").addEventListener("click", (e) => {
  e.preventDefault();
  toast("Botón de arrepentimiento: abre un formulario corto con el número de pedido (Res. 424/2020).");
});

T.escuchar((evento) => {
  const codigo = new URLSearchParams(location.search).get("pedido");
  if (!codigo || evento.tipo !== "estado") return;
  const p = T.porCodigo(codigo);
  if (!p || p.id !== evento.id) return;
  mostrarSeguimiento(codigo);
  toast(`Tu pedido ahora está: ${T.estados[p.estado]}`);
});

pintarDatosLocal();
pintarGrupos();
pintarProductos();
pintarCarrito();
iniciarMapa();
const codigoInicial = new URLSearchParams(location.search).get("pedido");
if (codigoInicial) mostrarSeguimiento(codigoInicial);
