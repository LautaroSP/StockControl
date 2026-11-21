import { Component, Input } from '@angular/core';
import { IconoComponent } from './icono.component';

/** WhatsApp, mail e Instagram de la tienda pública. */
@Component({
  selector: 'sc-contacto-tienda',
  standalone: true,
  imports: [IconoComponent],
  template: `
    @if (whatsApp || correo || instagram) {
      <div class="tienda-contacto">
        @if (titulo) { <p class="tienda-contacto-titulo">{{ titulo }}</p> }
        <span class="row">
          @if (whatsApp) {
            <a class="btn btn-primary" [href]="linkWhatsApp" target="_blank" rel="noopener"><sc-icono nombre="whatsapp" /> {{ textoWhatsApp }}</a>
          }
          @if (correo) {
            <a class="btn" [href]="linkCorreo"><sc-icono nombre="correo" /> {{ correo }}</a>
          }
          @if (instagram) {
            <a class="btn" [href]="linkInstagram" target="_blank" rel="noopener"><sc-icono nombre="instagram" /> Instagram</a>
          }
        </span>
      </div>
    }
  `,
  styles: `
    .tienda-contacto { display: flex; flex-direction: column; gap: 0.5rem; }
    .tienda-contacto-titulo { margin: 0; }
  `
})
export class ContactoTiendaComponent {
  @Input() whatsApp = '';
  @Input() correo = '';
  @Input() instagram = '';
  @Input() titulo = '';
  @Input() textoWhatsApp = 'Escribir por WhatsApp';
  /** Si hay producto, el mail y el WhatsApp llevan el nombre y el link. */
  @Input() producto = '';
  @Input() urlProducto = '';

  get linkWhatsApp(): string {
    if (!this.whatsApp) return '';
    if (!this.producto) return `https://wa.me/${this.whatsApp}`;
    const texto = `Hola, me gustaría saber acerca de: ${this.producto}\n${this.urlProducto || location.href}`;
    return `https://wa.me/${this.whatsApp}?text=${encodeURIComponent(texto)}`;
  }

  get linkCorreo(): string {
    if (!this.correo) return '';
    if (!this.producto) return `mailto:${this.correo}`;
    const asunto = `Consulta: ${this.producto}`;
    const cuerpo = `Hola, me gustaría saber acerca de: ${this.producto}\n${this.urlProducto || location.href}`;
    return `mailto:${this.correo}?subject=${encodeURIComponent(asunto)}&body=${encodeURIComponent(cuerpo)}`;
  }

  get linkInstagram(): string {
    return this.instagram ? `https://www.instagram.com/${this.instagram}/` : '';
  }
}
