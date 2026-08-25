(function () {
  const locales = [
    { id: "almagro", nombre: "Almagro", full: "Lo de Pepe — Almagro", estado: "ok", vence: "12 sep" },
    { id: "caballito", nombre: "Caballito", full: "Lo de Pepe — Caballito", estado: "ok", vence: "12 sep" },
    { id: "flores", nombre: "Flores", full: "Lo de Pepe — Flores", estado: "trial", vence: "31 ago" }
  ];

  function rol() {
    return sessionStorage.getItem("sc_rol") || "dueno";
  }
  function localId() {
    return sessionStorage.getItem("sc_local") || "almagro";
  }
  function local() {
    return locales.find((l) => l.id === localId()) || locales[0];
  }
  function esDueño() {
    return rol() === "dueno";
  }
  function esEmpleado() {
    return rol() === "empleado";
  }
  function esTrial() {
    return sessionStorage.getItem("sc_trial") === "1" || local().estado === "trial";
  }

  function loginAs(r, opts) {
    sessionStorage.setItem("sc_rol", r);
    sessionStorage.removeItem("sc_trial");
    if (r === "admin") {
      location.href = "admin.html";
      return;
    }
    if (r === "empleado") {
      sessionStorage.setItem("sc_local", "almagro");
      location.href = "caja.html";
      return;
    }
    sessionStorage.setItem("sc_local", "almagro");
    if (opts && opts.trial) {
      sessionStorage.setItem("sc_trial", "1");
      sessionStorage.setItem("sc_local", "flores");
      location.href = "caja.html";
      return;
    }
    location.href = "locales.html";
  }

  function setLocal(id) {
    sessionStorage.setItem("sc_local", id);
  }

  function navLocal(active) {
    const d = esDueño();
    const loc = local();
    const switcher = d
      ? `<select class="local-switch" id="scLocal">${locales
          .map((l) => `<option value="${l.id}" ${l.id === loc.id ? "selected" : ""}>${l.full}</option>`)
          .join("")}</select>`
      : "";
    const negocio = d
      ? `<div class="section">Negocio</div>
        <a class="${active === "estadisticas" ? "active" : ""}" href="estadisticas.html">Estadísticas</a>
        <a class="${active === "empleados" ? "active" : ""}" href="empleados.html">Empleados</a>
        <a class="${active === "config" ? "active" : ""}" href="configuracion.html">Configuración</a>
        <a class="${active === "plan" ? "active" : ""}" href="plan.html">Plan y pago</a>`
      : "";
    const localesLink = d
      ? `<a class="${active === "locales" ? "active" : ""}" href="locales.html">Mis locales</a>`
      : "";
    const quien = esEmpleado() ? "Ana · empleado" : "Jorge · dueño";
    const pie = esTrial()
      ? `${quien}<br />Prueba · queda 4 días<br /><a href="plan.html" style="color:#fff">Activar $ 9.999</a>`
      : `${quien}<br />$ 9.999 / mes · vence ${loc.vence}`;

    return `
      <div class="brand">
        <strong>StockControl</strong>
        <span>${loc.full}</span>
      </div>
      ${switcher}
      <nav class="nav">
        <div class="section">Mostrador</div>
        <a class="${active === "caja" ? "active" : ""}" href="caja.html">Caja</a>
        <a class="${active === "productos" ? "active" : ""}" href="productos.html">Productos</a>
        ${d ? `<a class="${active === "grupos" ? "active" : ""}" href="grupos.html">Grupos</a>` : ""}
        <a class="${active === "informes" ? "active" : ""}" href="informes.html">Informes</a>
        ${negocio}
        <div class="section">Cuenta</div>
        ${localesLink}
        <a href="login.html">Salir</a>
      </nav>
      <div class="sidebar-foot">${pie}</div>`;
  }

  function navAdmin(active) {
    return `
      <div class="brand">
        <strong>StockControl</strong>
        <span>Admin · el servicio</span>
      </div>
      <nav class="nav">
        <div class="section">Servicio</div>
        <a class="${active === "locales" ? "active" : ""}" href="admin.html">Locales</a>
        <a class="${active === "alta" ? "active" : ""}" href="admin-alta.html">Alta de local</a>
        <div class="section">Prototipo</div>
        <a href="index.html">Sitio público</a>
        <a href="login.html">Salir / cambiar rol</a>
      </nav>
      <div class="sidebar-foot">
        $ 9.999 por local.<br />
        Ventas: 11 5555-0101
      </div>`;
  }

  function bannerHtml() {
    if (rol() === "admin") return "";
    if (esTrial()) {
      return `<div class="banner trial">
        <span>Prueba de 7 días · te quedan <strong>4 días</strong> en ${local().nombre}. Un local, sin contratar otro.</span>
        <a class="btn btn-primary" href="plan.html">Pagar $ 9.999</a>
      </div>`;
    }
    return `<div class="banner ok">
      <span>${local().full} · al día · vence ${local().vence}</span>
      ${esDueño() ? `<a class="btn" href="plan.html">Ver plan</a>` : ""}
    </div>`;
  }

  function paintQR(el) {
    if (!el) return;
    el.innerHTML = "";
    for (let i = 0; i < 121; i++) {
      const cell = document.createElement("i");
      const edge = i < 11 || i >= 110 || i % 11 === 0 || i % 11 === 10;
      if (edge || Math.random() > 0.55) cell.className = "on";
      el.appendChild(cell);
    }
  }

  function mount() {
    const aside = document.querySelector("[data-shell]");
    if (!aside) return;
    const kind = aside.getAttribute("data-shell") || "local";
    const active = aside.getAttribute("data-active") || "";
    const min = document.body.getAttribute("data-min");

    if (min === "dueno" && esEmpleado()) {
      location.href = "caja.html";
      return;
    }
    if (min === "admin" && rol() !== "admin") {
      location.href = "login.html";
      return;
    }

    aside.classList.add("sidebar");
    aside.innerHTML = kind === "admin" ? navAdmin(active) : navLocal(active);

    const sel = document.getElementById("scLocal");
    if (sel) {
      sel.addEventListener("change", () => {
        setLocal(sel.value);
        location.reload();
      });
    }

    const slot = document.querySelector("[data-banner]");
    if (slot) slot.outerHTML = bannerHtml();

    document.querySelectorAll("[data-dueno-only]").forEach((el) => {
      if (!esDueño()) el.remove();
    });
    document.querySelectorAll("[data-empleado-only]").forEach((el) => {
      if (!esEmpleado()) el.remove();
    });

    document.querySelectorAll(".qr").forEach(paintQR);
  }

  window.SC = { rol, local, localId, locales, esDueño, esEmpleado, esTrial, loginAs, setLocal, paintQR };
  document.addEventListener("DOMContentLoaded", mount);
})();
