import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-privacidad',
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
        <h1>Política de privacidad</h1>
        <p>StockControl lo presta Lautaro Parandelli, persona física, identificación 23-40133767-9, con domicilio en Avenida Rivadavia 15240, 4.º D, Haedo, Partido de Morón, Provincia de Buenos Aires.</p>
        <p>StockControl guarda datos para prestar el servicio.</p>
        <ul>
          <li>Dueño y empleados: nombre, mail y clave. La clave se guarda con el hash de ASP.NET Identity, nunca en texto plano. Sirven para entrar y operar el local.</li>
          <li>Contacto: nombre, mail y mensaje. Sirven para contestar la consulta.</li>
          <li>Clientes de cuentas pendientes: nombre y teléfono. El responsable de esos datos es el local. StockControl los guarda por él.</li>
          <li>Compradores de la tienda: nombre, teléfono y mail. El responsable es el local. StockControl los guarda por él.</li>
        </ul>
        <p>En el navegador se guarda la sesión, el local, la caja y el tema. No hay cookies de publicidad. El captcha es de Cloudflare Turnstile y el mapa, de OpenStreetMap. La búsqueda de dirección usa Nominatim. Los mails salen por Resend. Los cobros, por Mercado Pago.</p>
        <p>Para acceder, corregir o borrar datos personales, escribir a <a href="mailto:lautaroparandelli&#64;gmail.com">lautaroparandelli&#64;gmail.com</a>.</p>
        <p>La Agencia de Acceso a la Información Pública, órgano de control de la Ley 25.326, atiende las denuncias y reclamos de quienes vean afectados sus derechos por el incumplimiento de las normas de protección de datos personales.</p>
        <p class="meta"><a routerLink="/terminos">Términos y condiciones</a></p>
        <p class="meta"><a routerLink="/registro">Registrate</a> · <a routerLink="/login">Entrar</a> · <a routerLink="/">Inicio</a></p>
      </article>
    </div>
  `
})
export class PrivacidadComponent implements OnInit {
  ngOnInit(): void {
    document.title = 'StockControl — privacidad';
  }
}
