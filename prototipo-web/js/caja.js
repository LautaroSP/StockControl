const productos = [
  { codigo: "7790895001234", nombre: "Coca Cola 500ml", stock: 12, precio: 2500 },
  { codigo: "7790895005678", nombre: "Pan lacteal", stock: 8, precio: 1800 },
  { codigo: "7790895009012", nombre: "Agua Villavicencio 2L", stock: 3, precio: 1600 },
  { codigo: "7790895003456", nombre: "Galletitas Oreo", stock: 20, precio: 2200 },
  { codigo: "7790895007890", nombre: "Cerveza Quilmes 473", stock: 4, precio: 1900 },
  { codigo: "7790895002468", nombre: "Yerba Playadito 1kg", stock: 15, precio: 4200 },
  { codigo: "7790895001357", nombre: "Leche La Serísima 1L", stock: 2, precio: 1450 },
  { codigo: "7790895008642", nombre: "Fideos Matarazzo", stock: 18, precio: 980 },
  { codigo: "SEC-FIAMBRE", nombre: "Fiambre (sector)", stock: 1, precio: 0, sector: true },
  { codigo: "7790895001111", nombre: "Huevos x 6", stock: 9, precio: 2100 },
  { codigo: "7790895002222", nombre: "Azúcar Ledesma 1kg", stock: 11, precio: 1750 },
  { codigo: "7790895003333", nombre: "Papas Lays 145g", stock: 6, precio: 2800 }
];

let carrito = [];

const $ = (id) => document.getElementById(id);
const money = (n) => "$ " + Math.round(n).toLocaleString("es-AR");

function renderProductos() {
  const q = ($("buscar").value || "").trim().toLowerCase();
  const tipo = $("filtroTipo") ? $("filtroTipo").value : "";
  const tbody = document.querySelector("#tablaProd tbody");
  const lista = productos.filter((p) => {
    const okQ = !q || p.nombre.toLowerCase().includes(q) || p.codigo.toLowerCase().includes(q);
    const okT = !tipo || (tipo === "sector" ? p.sector : !p.sector);
    return okQ && okT;
  });
  tbody.innerHTML = lista.map((p) => `
    <tr class="${p.stock < 5 ? "low" : ""}" data-codigo="${p.codigo}">
      <td>${p.codigo}</td>
      <td>${p.nombre}</td>
      <td class="num">${p.stock}</td>
      <td class="num">${p.sector ? "a definir" : money(p.precio)}</td>
    </tr>
  `).join("");
  $("cantProd").textContent = lista.length + " en lista";
  tbody.querySelectorAll("tr").forEach((tr) => {
    tr.addEventListener("dblclick", () => agregar(tr.dataset.codigo));
  });
}

function renderCarrito() {
  const tbody = document.querySelector("#tablaCart tbody");
  tbody.innerHTML = carrito.map((i, idx) => `
    <tr data-idx="${idx}">
      <td>${i.nombre}</td>
      <td class="num">${i.cantidad}</td>
      <td class="num">${money(i.precio * i.cantidad)}</td>
    </tr>
  `).join("");
  tbody.querySelectorAll("tr").forEach((tr) => {
    tr.addEventListener("dblclick", () => {
      carrito.splice(Number(tr.dataset.idx), 1);
      renderCarrito();
    });
  });
  const items = carrito.reduce((a, i) => a + i.cantidad, 0);
  $("lblItems").textContent = items;
  $("lblTotal").textContent = money(totalConDescuento());
}

function totalBruto() {
  return carrito.reduce((a, i) => a + i.precio * i.cantidad, 0);
}

function totalConDescuento() {
  const bruto = totalBruto();
  if (!$("chkDesc").checked) return bruto;
  const pct = Number($("txtDesc").value) || 0;
  return bruto * (1 - pct / 100);
}

function agregar(codigo) {
  const p = productos.find((x) => x.codigo === codigo);
  if (!p) {
    toast("Código no encontrado. El dueño puede crear el producto.");
    return;
  }
  const existente = carrito.find((i) => i.codigo === codigo && !p.sector);
  const precio = p.sector ? Number(prompt("Precio del sector", "2500")) || 0 : p.precio;
  if (existente) existente.cantidad += 1;
  else carrito.push({ codigo: p.codigo, nombre: p.nombre, precio, cantidad: 1 });
  renderCarrito();
  $("scanner").focus();
}

function toast(msg) {
  const el = $("toast");
  el.textContent = msg;
  el.classList.add("show");
  setTimeout(() => el.classList.remove("show"), 2200);
}

function cerrarVenta() {
  toast("Venta cobrada · " + $("lblTotal").textContent + " (prototipo)");
  carrito = [];
  renderCarrito();
  $("scanner").focus();
}

$("buscar").addEventListener("input", renderProductos);
if ($("filtroTipo")) $("filtroTipo").addEventListener("change", renderProductos);
$("scanner").addEventListener("keydown", (e) => {
  if (e.key === "Enter") {
    e.preventDefault();
    agregar($("scanner").value.trim());
    $("scanner").value = "";
  }
});
$("btnGeneric").addEventListener("click", () => {
  const nombre = prompt("Nombre del producto genérico", "Producto") || "Producto";
  const precio = Number(prompt("Precio", "0")) || 0;
  carrito.push({ codigo: "GEN", nombre, precio, cantidad: 1 });
  renderCarrito();
});
$("chkDesc").addEventListener("change", () => {
  $("txtDesc").disabled = !$("chkDesc").checked;
  renderCarrito();
});
$("txtDesc").addEventListener("input", renderCarrito);
$("btnCancelar").addEventListener("click", () => {
  carrito = [];
  renderCarrito();
});
$("btnCobrar").addEventListener("click", () => {
  if (!carrito.length) {
    toast("El carrito está vacío");
    return;
  }
  const medio = $("medio") ? $("medio").value : "";
  if (medio.includes("QR")) {
    $("qrMonto").textContent = "Total " + $("lblTotal").textContent + " · esperando pago";
    SC.paintQR($("qrCaja"));
    $("modalQr").classList.add("show");
    return;
  }
  cerrarVenta();
});

if ($("qrOk")) {
  $("qrOk").onclick = () => {
    $("modalQr").classList.remove("show");
    cerrarVenta();
  };
  $("qrNo").onclick = () => $("modalQr").classList.remove("show");
}

document.querySelectorAll(".tab").forEach((tab) => {
  tab.addEventListener("click", () => {
    document.querySelectorAll(".tab").forEach((t) => t.classList.remove("on"));
    tab.classList.add("on");
    toast("Carrito " + tab.textContent);
  });
});

renderProductos();
renderCarrito();
