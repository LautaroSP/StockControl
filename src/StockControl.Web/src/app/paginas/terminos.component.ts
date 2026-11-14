import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-terminos',
  standalone: true,
  imports: [RouterLink, TemaComponent],
  template: `
    <sc-tema />
    <div class="auth" style="padding-top:72px;align-items:start">
      <article class="panel legal">
        <div class="brand" style="border:0;padding:0 0 8px">
          <strong>StockControl</strong>
          <span>Versión 1 · 11 de noviembre de 2026</span>
        </div>
        <h1>Términos y condiciones</h1>
        <p>StockControl lo presta Lautaro Parandelli, persona física, identificación 23-40133767-9, con domicilio en Avenida Rivadavia 15240, 4.º D, Haedo, Partido de Morón, Provincia de Buenos Aires. Mail para temas legales y de datos personales: <a href="mailto:lautaroparandelli&#64;gmail.com">lautaroparandelli&#64;gmail.com</a>.</p>
        <p>StockControl es el servicio de caja y stock. Al crear la cuenta hay una prueba de 7 días, en un local. En la prueba se pueden cargar productos y cobrar. Pasados los 7 días, el local sigue si se abonan $ 14.999 por mes.</p>
        <p>El abono se cobra con Mercado Pago: un pago del mes o el débito automático. Ese cobro es el servicio, no las ventas del mostrador. El débito se cancela desde Mercado Pago. Con débito activo hay 3 días de gracia después del vencimiento. Cada local se abona por separado. Un local nuevo que contrata el dueño no tiene prueba: no opera hasta el pago. Los empleados no tienen un cargo aparte.</p>
        <p>Si el abono está vencido o pendiente, ese local no puede cobrar. La tienda, si estaba publicada, muestra que está temporalmente cerrada.</p>
        <p>El precio puede cambiar. Se avisa al dueño antes de aplicarlo.</p>
        <p>El servicio puede tener mantenimiento. No hay garantía de que esté disponible todo el tiempo.</p>
        <p>Los productos, las ventas y los clientes son del local. El dueño responde por sus empleados y por lo que cargan. Al darse de baja puede pedir una copia de esos datos al mail legal. Se borran cuando el local ya no está activo, salvo lo que haga falta guardar mientras haya un reclamo abierto.</p>
        <p>Mercado Pago es un tercero. El dinero del mostrador y de la tienda entra en la cuenta de Mercado Pago del local, no en la de StockControl. El ticket no es factura. La factura la emite el local, si corresponde.</p>
        <p>No está permitido copiar el sitio ni el sistema, ni republicarlos para armar otro. Ver la página no autoriza a hacer una copia. StockControl puede suspender la cuenta por un uso que perjudique el servicio o a otros locales.</p>
        <p>Los reclamos del servicio se rigen por la ley argentina. La jurisdicción es la Provincia de Buenos Aires.</p>
        <p class="meta"><a routerLink="/privacidad">Política de privacidad</a> · <a routerLink="/arrepentimiento">Arrepentimiento</a></p>
        <p class="meta"><a routerLink="/registro">Registrate</a> · <a routerLink="/login">Entrar</a> · <a routerLink="/">Inicio</a></p>
      </article>
    </div>
  `
})
export class TerminosComponent implements OnInit {
  ngOnInit(): void {
    document.title = 'StockControl — términos';
  }
}
