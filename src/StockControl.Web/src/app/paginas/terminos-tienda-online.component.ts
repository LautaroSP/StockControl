import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-terminos-tienda-online',
  standalone: true,
  imports: [RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="padding-top:72px;align-items:start">
      <article class="panel legal">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Tienda online · versión 1 · 20 de noviembre de 2026</span>
        </div>
        <h1>Términos de la tienda online</h1>
        <p>Estos términos complementan los <a routerLink="/terminos">términos generales</a> de StockControl. Aplican al dueño o socio que publica la tienda online de un local. El prestador del servicio es Lautaro Parandelli (CUIT 23-40133767-9). Mail legal: <a href="mailto:lautaroparandelli&#64;gmail.com">lautaroparandelli&#64;gmail.com</a>.</p>

        <h2>1. Qué es la gestión</h2>
        <p>Por cada pedido de la tienda online que se concreta, StockControl percibe un cargo de gestión a cargo del local. El importe lo fija StockControl por local (por defecto $ 500). Un pedido cancelado o reembolsado no genera gestión.</p>
        <p>Se considera concretado: el pago acreditado en Mercado Pago; la transferencia confirmada por el local; o la entrega del pedido cuando el medio es “coordinar con el local”.</p>

        <h2>2. Quién la paga frente al comprador</h2>
        <p>El dueño elige en la configuración:</p>
        <ul>
          <li><strong>La paga el comprador:</strong> el importe se muestra en el carrito y en el ticket, y se suma al total que paga el comprador.</li>
          <li><strong>La absorbe el local:</strong> el comprador no ve el cargo; el local igual debe la gestión a StockControl.</li>
        </ul>

        <h2>3. Cómo se cobra</h2>
        <p>Con Mercado Pago, el cargo se separa como comisión de la plataforma (marketplace fee) en esa operación. Con transferencia o “coordinar”, el importe queda registrado como deuda del local hacia StockControl y se paga aparte, desde Abono, no junto con el abono mensual del servicio de caja.</p>

        <h2>4. Plazo e incumplimiento</h2>
        <p>Cada deuda tiene plazo hasta el vencimiento del período de abono del local vigente al nacer la deuda, más 7 días corridos. Si hay varias deudas, rige el plazo más próximo.</p>
        <p>Vencido el plazo sin pago, StockControl puede suspender la tienda online (el catálogo público). La caja del mostrador puede seguir operando si el abono del servicio está al día.</p>
        <p>El impago no extingue la deuda. El local reconoce que ese saldo es una obligación dineraria determinada (líquida) y exigible a favor del prestador, en los términos de los arts. 724 y siguientes del Código Civil y Comercial de la Nación. StockControl puede reclamar el cobro por las vías legales, sin perjuicio de la suspensión de la tienda.</p>

        <h2>5. Aceptación</h2>
        <p>Al activar la tienda online y marcar que aceptás estos términos, el local presta consentimiento expreso (arts. 957, 961 y 971 del Código Civil y Comercial). Queda registrado el momento de la aceptación. Si StockControl publica una versión nueva, habrá que aceptarla de nuevo para mantener la tienda activa.</p>

        <h2>6. Responsabilidad del local</h2>
        <p>Los productos, precios, stock, envíos y la relación con el comprador son del local. StockControl presta la herramienta tecnológica; no es el vendedor frente al comprador.</p>

        <p class="meta"><a routerLink="/terminos">Términos generales</a> · <a routerLink="/privacidad">Privacidad</a> · <a routerLink="/">Inicio</a></p>
      </article>
    </div>
  `
})
export class TerminosTiendaOnlineComponent implements OnInit {
  ngOnInit(): void {
    document.title = 'StockControl — términos tienda online';
  }
}
