const root = document.querySelector("[data-carrusel]");
const track = root.querySelector(".reel-track");
const slides = [...root.querySelectorAll(".slide")];
const dots = [...root.querySelectorAll("[data-punto]")];
const titulo = document.querySelector("[data-titulo]");
let indice = 0;
let origen = null;

function ir(n) {
  indice = (n + slides.length) % slides.length;
  track.style.transform = `translateX(-${indice * 100}%)`;
  slides.forEach((slide, i) => slide.toggleAttribute("inert", i !== indice));
  dots.forEach((punto, i) => {
    if (i === indice) punto.setAttribute("aria-current", "true");
    else punto.removeAttribute("aria-current");
  });
  titulo.textContent = slides[indice].dataset.titulo;
}

root.querySelector("[data-anterior]").addEventListener("click", () => ir(indice - 1));
root.querySelector("[data-siguiente]").addEventListener("click", () => ir(indice + 1));
dots.forEach((punto) => punto.addEventListener("click", () => ir(Number(punto.dataset.punto))));

root.addEventListener("keydown", (evento) => {
  if (evento.key === "ArrowLeft") ir(indice - 1);
  if (evento.key === "ArrowRight") ir(indice + 1);
});

root.addEventListener("pointerdown", (evento) => {
  if (evento.target.closest("button")) return;
  origen = evento.clientX;
});

root.addEventListener("pointerup", (evento) => {
  if (origen == null) return;
  const delta = evento.clientX - origen;
  if (delta > 48) ir(indice - 1);
  if (delta < -48) ir(indice + 1);
  origen = null;
});

ir(0);

const html = document.documentElement;
const prefiereQuieto = window.matchMedia("(prefers-reduced-motion: reduce)");

function alternarTema() {
  html.classList.add("tema-suave");
  const oscuro = html.getAttribute("data-tema") === "oscuro";
  html.setAttribute("data-tema", oscuro ? "claro" : "oscuro");
  window.setTimeout(() => html.classList.remove("tema-suave"), 1100);
}

if (!prefiereQuieto.matches) {
  window.setInterval(alternarTema, 8000);
}
