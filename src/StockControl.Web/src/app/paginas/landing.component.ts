import { Component, NgZone, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../servicios/api.service';
import { montarCaptcha } from '../servicios/turnstile';
import { TemaService } from '../servicios/tema.service';
import { TemaComponent } from '../tema.component';

const PANTALLAS = [
  'El carrito',
  'Cargar un producto',
  'Estadísticas',
  'Cargar un empleado',
  'El abono con Mercado Pago'
];

@Component({
  selector: 'sc-landing',
  standalone: true,
  imports: [FormsModule, RouterLink, TemaComponent],
  template: `
    <div class="landing">
      <nav class="landing-nav">
        <a class="brand" href="#inicio">
          <strong>StockControl</strong>
          <span>Caja y stock en el navegador</span>
        </a>
        <div class="nav-public">
          <a class="enlace" href="#recorrido">La app</a>
          <a class="enlace" href="#precio">Precio</a>
          <a class="enlace" href="#preguntas">Preguntas</a>
          <a class="enlace" routerLink="/sobre-nosotros">Nosotros</a>
          <a class="enlace" href="#contacto">Contacto</a>
          <a class="btn btn-ghost" routerLink="/login">Entrar</a>
          <a class="btn btn-primary" routerLink="/registro">Registrate</a>
          <sc-tema [fijo]="false" />
        </div>
      </nav>

      <main id="inicio">
        <section class="hero">
          <div>
            <p class="eyebrow">7 días gratuitos</p>
            <h1>Registrate y tené tu prueba gratuita.</h1>
            <p class="lead">
              Cargá productos, cobrá en el carrito, mirá las estadísticas y sumá empleados.
              La caja tiene pasarela de Mercado Pago: generás un QR para el cobro, cobrás con el Point o usás un QR impreso.
              Tiene modo claro y modo oscuro, y lo podés usar desde el celular.
              Siete días para probar un local. Después, el abono se cobra con Mercado Pago.
            </p>
            <div class="marca-mp">
              <img class="logo-mp logo-mp-color" src="mercadopago.svg" alt="Mercado Pago" />
              <img class="logo-mp logo-mp-pluma" src="mercadopago-claro.svg" alt="Mercado Pago" />
            </div>
            <div class="row" style="margin-top:18px">
              <a class="btn btn-primary" routerLink="/registro" style="padding:12px 16px;font-size:15px">Crear cuenta gratis</a>
              <a class="btn" routerLink="/login">Ya tengo cuenta</a>
            </div>
            <p class="meta" style="margin-top:14px">Un local durante la prueba. $ 14.999 por mes si seguís.</p>
          </div>

          <div class="reel" id="recorrido" tabindex="0" aria-roledescription="carrusel" aria-label="Recorrido de la app"
               (keydown)="tecla($event)" (pointerdown)="punteroAbajo($event)" (pointerup)="punteroArriba($event)">
            <div class="reel-marco">
              <div class="reel-bar">
                <span>{{ titulo }}</span>
                <span>Tu local</span>
              </div>
              <div class="reel-vista">
                <div class="reel-track" [style.transform]="'translateX(-' + indice * 100 + '%)'">
                  <article class="slide" [inert]="indice !== 0">
                    <div class="slide-app">
                      <div class="linea"><span>Código + Enter</span><span class="meta">Caja 1</span></div>
                      <div class="linea"><div><b>Coca 500</b><div class="meta">1 × $ 2.500</div></div><span class="num">$ 2.500</span></div>
                      <div class="linea"><div><b>Pan lacteal</b><div class="meta">1 × $ 1.800</div></div><span class="num">$ 1.800</span></div>
                      <div class="linea"><div><b>Agua 2 L</b><div class="meta">1 × $ 1.600</div></div><span class="num">$ 1.600</span></div>
                      <div class="total-box">
                        <div><div class="lbl">Ítems</div><div>3</div></div>
                        <div><div class="lbl">Total</div><div class="val">$ 5.900</div></div>
                      </div>
                      <button class="btn btn-charge" type="button">Cobrar</button>
                    </div>
                  </article>
                  <article class="slide" [inert]="indice !== 1">
                    <div class="slide-app">
                      <div class="campo-fijo">Código <span>7790895001234</span></div>
                      <div class="campo-fijo">Nombre <span>Coca 500</span></div>
                      <div class="campo-fijo">Costo <span>$ 1.000</span></div>
                      <p class="hint" style="margin:0">Precio = costo × factor de ganancia × IVA</p>
                      <div class="precio-listo">
                        <span>Precio de venta</span>
                        <strong>$ 1.996,50</strong>
                      </div>
                      <p class="meta" style="margin:0">Factor 1,65 · IVA 1,21</p>
                      <button class="btn btn-primary" type="button">Guardar producto</button>
                    </div>
                  </article>
                  <article class="slide" [inert]="indice !== 2">
                    <div class="slide-app">
                      <div class="slide-kpis">
                        <div class="panel kpi"><div class="n">$ 3.359.600</div><div class="l">Vendido en el mes</div></div>
                        <div class="panel kpi"><div class="n">1.842</div><div class="l">Tickets</div></div>
                        <div class="panel kpi"><div class="n">$ 1.825</div><div class="l">Ticket promedio</div></div>
                        <div class="panel kpi"><div class="n">$ 980.000</div><div class="l">Margen</div></div>
                      </div>
                      <div class="linea"><div><b>Coca 500</b><div class="meta">420 unidades</div></div><span class="num">$ 1.050.000</span></div>
                      <div class="linea"><div><b>Pan lacteal</b><div class="meta">156 unidades</div></div><span class="num">$ 280.800</span></div>
                      <div class="bar-row"><span>Efectivo</span><div class="bar"><b style="width:55%"></b></div></div>
                      <div class="bar-row"><span>Mercado Pago</span><div class="bar"><b style="width:33%"></b></div></div>
                    </div>
                  </article>
                  <article class="slide" [inert]="indice !== 3">
                    <div class="slide-app">
                      <div class="linea">
                        <div><b>Ana López</b><div class="meta">ana · este local</div></div>
                        <span class="badge">Activa</span>
                      </div>
                      <div class="campo-fijo">Nombre <span>Pedro Ruiz</span></div>
                      <div class="campo-fijo">Usuario <span>pedro</span></div>
                      <div class="campo-fijo">Contraseña <span>••••••</span></div>
                      <p class="hint" style="margin:0">Los empleados no se cobran aparte. Pueden cobrar y no cambian precios.</p>
                      <button class="btn btn-primary" type="button">Guardar empleado</button>
                    </div>
                  </article>
                  <article class="slide" [inert]="indice !== 4">
                    <div class="slide-app">
                      <p class="meta" style="margin:0">Abono de este local</p>
                      <div class="marca-mp">
                        <img class="logo-mp logo-mp-color" src="mercadopago.svg" alt="Mercado Pago" />
                        <img class="logo-mp logo-mp-pluma" src="mercadopago-claro.svg" alt="Mercado Pago" />
                      </div>
                      <div class="price" style="margin:0">$ 14.999 <small>/ mes</small></div>
                      <p style="margin:0">Podés pagar el mes o dejar el débito automático.</p>
                      <div class="banner" style="margin:0">
                        Antes, 7 días gratuitos. El primer cobro es cuando termina la prueba.
                      </div>
                      <a class="btn btn-primary" routerLink="/registro">Pagar con Mercado Pago</a>
                    </div>
                  </article>
                </div>
              </div>
            </div>
            <div class="reel-controles">
              <button class="btn" type="button" (click)="ir(indice - 1)">Anterior</button>
              <div class="puntos" role="tablist" aria-label="Pantallas">
                @for (nombre of pantallas; track nombre; let i = $index) {
                  <button type="button" [attr.aria-label]="nombre" [attr.aria-current]="i === indice ? 'true' : null" (click)="ir(i)"></button>
                }
              </div>
              <button class="btn" type="button" (click)="ir(indice + 1)">Siguiente</button>
            </div>
            <p class="caption">Pasá las pantallas: el carrito, un producto, las estadísticas, un empleado y el abono.</p>
          </div>
        </section>

        <section class="features" aria-label="Qué incluye">
          <article class="panel feat">
            <div class="marca-mp">
              <img class="logo-mp logo-mp-color" src="mercadopago.svg" alt="Mercado Pago" />
              <img class="logo-mp logo-mp-pluma" src="mercadopago-claro.svg" alt="Mercado Pago" />
            </div>
            <h3>Pasarela de pago</h3>
            <p>En el mostrador generás un QR para esa venta, cobrás con el Point o dejás un QR impreso en el puesto. Conectás la cuenta del local una vez. Ese dinero es de la venta, aparte del abono.</p>
          </article>
          <article class="panel feat">
            <h3>Tienda online</h3>
            <p>Cada local tiene su página con el catálogo. El comprador arma el carrito, elige retiro o envío y paga con Mercado Pago, transferencia o al recibir. Los pedidos te avisan en la app.</p>
          </article>
          <article class="panel feat">
            <h3>Ofertas y combos</h3>
            <p>Descuentos por fecha, llevás 3 y pagás 2, y combos de varios productos. La caja los aplica sola.</p>
          </article>
          <article class="panel feat">
            <h3>Cuentas pendientes</h3>
            <p>El cliente se lleva la mercadería y paga después. Ves cuánto debe cada uno y cobrás de a partes.</p>
          </article>
          <article class="panel feat">
            <h3>Balanza</h3>
            <p>Para lo que se vende por kilo: el peso por el precio por kilo entra al carrito.</p>
          </article>
          <article class="panel feat">
            <h3>Todos tus locales</h3>
            <p>Gestionás los locales que tengas desde un solo lugar. Creás cuentas de empleados o de socios.</p>
          </article>
          <article class="panel feat">
            <h3>Un local, los empleados que necesites</h3>
            <p>La prueba es de un local. Otro local es otro abono. Los empleados no se cobran aparte.</p>
          </article>
          <article class="panel feat">
            <h3>Estadísticas y cierre de caja</h3>
            <p>Ves lo vendido, los tickets y el margen. Al cerrar la caja queda el resumen del turno.</p>
          </article>
          <article class="panel feat">
            <h3>Proveedores</h3>
            <p>Cargás a quién le comprás, con teléfono y tipo de productos.</p>
          </article>
          <article class="panel feat">
            <h3>Modo claro y modo oscuro</h3>
            <p>Elegís el fondo claro o el oscuro. Si no elegiste, sigue el de tu celular o tu compu.</p>
          </article>
          <article class="panel feat">
            <h3>Desde el celular</h3>
            <p>La caja, los productos y el resto se abren en el navegador del teléfono. No hace falta instalar nada.</p>
          </article>
        </section>

        <section class="bloque" id="precio">
          <div class="panel pad">
            <h2>La prueba y el abono</h2>
            <p class="lead" style="margin-top:0">¡Tus primeros 7 días son gratis!</p>
            <p>Si te gusta, te quedás por tan solo</p>
            <div class="price">$ 14.999 <small>/ mes por local</small></div>
            <div class="marca-mp">
              <img class="logo-mp logo-mp-color" src="mercadopago.svg" alt="Mercado Pago" />
              <img class="logo-mp logo-mp-pluma" src="mercadopago-claro.svg" alt="Mercado Pago" />
            </div>
            <p>Podés abonar desde Mercado Pago o, si no, contactándote con <a href="#contacto">ventas</a>.</p>
            <p class="meta">Pago del mes o débito automático. Si el abono vence, ese local no puede cobrar hasta renovarlo.</p>
            <a class="btn btn-primary" routerLink="/registro" style="margin-top:8px">Registrate y empezá la prueba</a>
          </div>
        </section>

        <section class="bloque" id="preguntas">
          <h2>Preguntas frecuentes</h2>
          <div class="faq">
            <details class="panel">
              <summary>¿Cuánto cuesta?</summary>
              <p>$ 14.999 por mes por local. Los primeros 7 días son gratis.</p>
            </details>
            <details class="panel">
              <summary>¿Cómo funciona la prueba?</summary>
              <p>Te registrás y usás un local durante 7 días. Después pagás el mes o dejás el débito automático con Mercado Pago. Si el abono vence, ese local no puede cobrar hasta renovarlo.</p>
            </details>
            <details class="panel">
              <summary>¿Los empleados se pagan aparte?</summary>
              <p>No. Sumás los empleados que necesites sin costo. Cobran en la caja y no cambian precios, salvo que se lo habilites.</p>
            </details>
            <details class="panel">
              <summary>¿Puedo tener más de un local?</summary>
              <p>Sí. Los ves todos desde la misma cuenta. Cada local es un abono aparte, y la prueba es solo para el primero.</p>
            </details>
            <details class="panel">
              <summary>¿Cómo cobro con Mercado Pago?</summary>
              <p>Conectás la cuenta de Mercado Pago del local una vez. En la caja generás un QR para la venta, cobrás con el Point o usás un QR impreso. El dinero va a la cuenta del local.</p>
            </details>
            <details class="panel">
              <summary>¿La tienda online tiene costo?</summary>
              <p>Viene con el abono. Por cada pedido online hay un cargo de gestión, y el local elige si lo paga el comprador.</p>
            </details>
            <details class="panel">
              <summary>¿Hay que instalar algo?</summary>
              <p>No. Se usa desde el navegador de la compu o del celular.</p>
            </details>
            <details class="panel">
              <summary>¿Puedo dar de baja?</summary>
              <p>Sí, cuando quieras. El mes pagado sigue hasta su vencimiento y no se renueva.</p>
            </details>
          </div>
        </section>

        <section class="bloque" id="contacto">
          <div class="contacto-publico">
            <div>
              <h2>Contacto</h2>
              <p class="lead" style="margin-top:0">Escribinos por la prueba, el abono o el uso de la caja. Usamos tu nombre, mail y mensaje solo para contestarte.</p>
              @if (url) {
                <p><a [href]="url">{{ telefono }}</a></p>
              } @else if (contactoCargado) {
                <p class="meta">Todavía no hay un teléfono de ventas.</p>
              }
              @if (email) {
                <p><a [href]="'mailto:' + email">{{ email }}</a></p>
              }
            </div>
            @if (formularioActivo) {
              <form class="panel pad" (ngSubmit)="enviar()">
                <label class="field">Nombre <input class="wide" type="text" name="nombre" maxlength="120" autocomplete="name" [(ngModel)]="nombre" /></label>
                <label class="field">Correo <input class="wide" type="email" name="correo" maxlength="200" autocomplete="email" [(ngModel)]="correo" /></label>
                <label class="field">Mensaje <textarea class="wide" name="mensaje" rows="4" maxlength="2000" [(ngModel)]="mensaje"></textarea></label>
                <input type="hidden" name="apertura" [(ngModel)]="apertura" />
                <input class="campo-oculto" type="text" name="sitio" [(ngModel)]="sitio" tabindex="-1" autocomplete="off" aria-hidden="true" />
                @if (captchaActivo) {
                  <div id="captcha-landing" class="captcha"></div>
                }
                <button class="btn btn-primary wide" type="submit" [disabled]="enviando || (captchaActivo && !captcha)">{{ enviando ? 'Enviando...' : 'Enviar' }}</button>
                @if (aviso) { <p class="meta">{{ aviso }}</p> }
                @if (error) { <p class="error">{{ error }}</p> }
              </form>
            } @else {
              <p class="meta">El formulario está desactivado. Escribinos por teléfono o por mail.</p>
            }
          </div>
        </section>
      </main>

      <footer class="site-foot" id="terminos">
        <h2>Términos y condiciones</h2>
        <div class="terminos">
          <p>StockControl es el servicio de caja y stock. La prueba dura 7 días, en un local. Después, el abono es $ 14.999 por mes.</p>
          <p><a routerLink="/terminos">Leer los términos</a> y la <a routerLink="/privacidad">política de privacidad</a>.</p>
        </div>
        <div class="foot-row">
          <span>StockControl · Argentina</span>
          <nav aria-label="Pie">
            <a routerLink="/registro">Registrate</a>
            <a routerLink="/login">Entrar</a>
            <a routerLink="/sobre-nosotros">Sobre nosotros</a>
            <a href="#contacto">Contacto</a>
            <a routerLink="/terminos">Términos</a>
            <a routerLink="/privacidad">Privacidad</a>
            <a routerLink="/arrepentimiento">Botón de arrepentimiento</a>
          </nav>
        </div>
      </footer>
    </div>
  `
})
export class LandingComponent implements OnInit, OnDestroy {
  readonly pantallas = PANTALLAS;
  indice = 0;
  telefono = '';
  email = '';
  url: string | null = null;
  contactoCargado = false;
  formularioActivo = true;
  nombre = '';
  correo = '';
  mensaje = '';
  sitio = '';
  apertura = '';
  captcha = '';
  captchaActivo = false;
  enviando = false;
  aviso = '';
  error = '';
  private origen: number | null = null;
  private reloj: ReturnType<typeof setInterval> | null = null;
  private readonly tituloAnterior = document.title;

  private limpiarCaptcha: (() => void) | null = null;

  constructor(
    private readonly api: ApiService,
    private readonly tema: TemaService,
    private readonly zona: NgZone
  ) {
    if (tema.elegido()) tema.aplicar();
    else tema.mostrar('oscuro');
  }

  get titulo(): string {
    return this.pantallas[this.indice];
  }

  ngOnInit(): void {
    document.title = 'StockControl — caja y stock para tu local';
    this.api.contactoVentas().subscribe({
      next: (r) => {
        this.telefono = r.telefono;
        this.email = r.email;
        this.url = r.url;
        this.formularioActivo = r.formularioActivo;
        this.apertura = r.apertura ?? '';
        this.contactoCargado = true;
      },
      error: () => (this.contactoCargado = true)
    });
    this.api.captchaRegistro().subscribe({
      next: (r) => {
        if (!r.siteKey) return;
        this.captchaActivo = true;
        setTimeout(() => {
          this.limpiarCaptcha = montarCaptcha(this.zona, 'captcha-landing', r.siteKey!, (token) => { this.captcha = token; });
        });
      }
    });
    if (!this.tema.elegido() && !window.matchMedia('(prefers-reduced-motion: reduce)').matches)
      this.reloj = setInterval(() => this.alternarTema(), 8000);
  }

  ngOnDestroy(): void {
    this.limpiarCaptcha?.();
    if (this.reloj) clearInterval(this.reloj);
    document.documentElement.classList.remove('tema-suave');
    document.title = this.tituloAnterior;
    this.tema.aplicar();
  }

  enviar(): void {
    if (!this.nombre.trim() || !this.correo.trim() || !this.mensaje.trim()) {
      this.error = 'Completá nombre, correo y mensaje.';
      this.aviso = '';
      return;
    }
    this.enviando = true;
    this.error = '';
    this.api.enviarContacto({
      nombre: this.nombre.trim(),
      correo: this.correo.trim(),
      mensaje: this.mensaje.trim(),
      sitio: this.sitio,
      apertura: this.apertura,
      captcha: this.captcha
    }).subscribe({
      next: () => {
        this.nombre = '';
        this.correo = '';
        this.mensaje = '';
        this.sitio = '';
        this.captcha = '';
        this.enviando = false;
        this.aviso = 'Mensaje enviado.';
      },
      error: (e) => {
        this.enviando = false;
        this.aviso = '';
        this.error = e.status === 429
          ? 'Demasiados envíos. Probá de nuevo en unos minutos.'
          : typeof e.error === 'string' ? e.error : 'No se pudo enviar el mensaje.';
      }
    });
  }

  ir(n: number): void {
    this.indice = (n + this.pantallas.length) % this.pantallas.length;
  }

  tecla(evento: KeyboardEvent): void {
    if (evento.key === 'ArrowLeft') this.ir(this.indice - 1);
    if (evento.key === 'ArrowRight') this.ir(this.indice + 1);
  }

  punteroAbajo(evento: PointerEvent): void {
    if ((evento.target as HTMLElement).closest('button, a')) return;
    this.origen = evento.clientX;
  }

  punteroArriba(evento: PointerEvent): void {
    if (this.origen == null) return;
    const delta = evento.clientX - this.origen;
    if (delta > 48) this.ir(this.indice - 1);
    if (delta < -48) this.ir(this.indice + 1);
    this.origen = null;
  }

  private alternarTema(): void {
    if (this.tema.elegido()) {
      if (this.reloj) clearInterval(this.reloj);
      this.reloj = null;
      return;
    }
    const html = document.documentElement;
    html.classList.add('tema-suave');
    this.tema.mostrar(this.tema.tema() === 'oscuro' ? 'claro' : 'oscuro');
    window.setTimeout(() => html.classList.remove('tema-suave'), 1100);
  }
}
