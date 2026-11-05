import { Component, HostListener, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toDataURL } from 'qrcode';
import { ApiService } from '../servicios/api.service';

/** Anchos del afiche: los mismos que ServicioQrTienda. */
type TamanoQr = 'chico' | 'mediano' | 'grande';

const LADO_MM: Record<TamanoQr, number> = { chico: 80, mediano: 120, grande: 160 };
const LADO_PX: Record<TamanoQr, number> = { chico: 800, mediano: 1200, grande: 1600 };
const LADO_POS_MM = 50;
const VERDE = '#124538';
const MENTA = '#73d0aa';
const ORO = '#c4a06a';
const CREMA = '#f7f4ef';

const OPCIONES_QR = {
  width: 800,
  margin: 1,
  errorCorrectionLevel: 'Q' as const,
  color: { dark: '#000000', light: '#ffffff' }
};

@Component({
  selector: 'sc-qr-tienda',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="topbar">
      <h1>QR de la tienda</h1>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    @if (cargada && !alias) {
      <section class="panel config-form">
        <div class="aviso-abono">
          Tu tienda todavía no tiene alias, así que no hay QR.
          <a class="btn btn-primary" routerLink="/mi-tienda/configuracion">Ir a Configuración</a>
        </div>
      </section>
    }
    @if (cargada && alias) {
      <section class="panel config-form">
        <div class="qr-form">
          <p class="meta">Al escanear abre {{ link() }}</p>
          <div class="qr-grupo">
            <span class="meta">Tamaño</span>
            <div class="row">
              <label class="check"><input type="radio" name="tamanoQr" value="chico" [(ngModel)]="tamano" /> Chico (80 mm)</label>
              <label class="check"><input type="radio" name="tamanoQr" value="mediano" [(ngModel)]="tamano" /> Mediano (120 mm)</label>
              <label class="check"><input type="radio" name="tamanoQr" value="grande" [(ngModel)]="tamano" /> Grande (160 mm)</label>
            </div>
          </div>
          <div class="row">
            <button class="btn" type="button" [disabled]="!qr || descargando" (click)="descargar()">
              {{ descargando ? 'Descargando...' : 'Descargar' }}
            </button>
            <button class="btn btn-primary" type="button" [disabled]="!qr" (click)="imprimir(false)">Imprimir</button>
            <button class="btn" type="button" [disabled]="!qr" (click)="imprimir(true)">Imprimir en la POS</button>
          </div>
          <p class="hint">El afiche lleva el recuadro y, abajo, StockControl. En la POS mide siempre {{ ladoPos }} mm, en papel {{ papelPos() }}.</p>
          @if (mensaje) { <p class="meta">{{ mensaje }}</p> }
        </div>
      </section>
      <p class="meta qr-leyenda">Vista previa</p>
      <div
        class="qr-tienda-print"
        [class.qr-hoja]="!pos"
        [class.qr-pos]="pos"
        [class.qr-pos58]="pos && anchoPos === 58"
        [class.qr-pos80]="pos && anchoPos !== 58"
      >
        <div class="qr-medida" [style.width.mm]="ladoMm()">
        <div class="qr-afiche">
          <div class="qr-cabecera">
            <span class="qr-regla"></span>
            <svg class="qr-carro" viewBox="0 0 24 24" aria-hidden="true">
              <path d="M4 6h2l1.2 9h11.3l1.5-7H8" />
              <circle cx="10" cy="18.5" r="1.2" />
              <circle cx="17" cy="18.5" r="1.2" />
            </svg>
            <span class="qr-regla"></span>
          </div>
          <p class="qr-titulo"><span>TIENDA</span> <span class="qr-online">ONLINE</span></p>
          <div class="qr-titulo-linea"></div>
          <div class="qr-cuerpo">
            <span class="qr-rayos qr-rayos-izq" aria-hidden="true"><i></i><i></i><i></i></span>
            <div class="qr-marco">
              @if (qr) {
                <img [src]="qr" alt="QR de la tienda" />
              }
            </div>
            <span class="qr-rayos" aria-hidden="true"><i></i><i></i><i></i></span>
          </div>
          <div class="qr-titulo-linea"></div>
          <p class="qr-pie">StockControl</p>
        </div>
        </div>
      </div>
    }
  `,
  styles: `
    .qr-form { display: flex; flex-direction: column; gap: 22px; }
    .qr-grupo { display: flex; flex-direction: column; align-items: flex-start; gap: 10px; }
    .qr-form .row { gap: 12px 20px; }
    .qr-form .hint, .qr-form .meta { margin: 0; }
  `
})
export class QrTiendaComponent implements OnInit {
  readonly ladoPos = LADO_POS_MM;
  alias = '';
  tamano: TamanoQr = 'mediano';
  pos = false;
  anchoPos = 80;
  qr = '';
  cargada = false;
  descargando = false;
  error = '';
  mensaje = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.api.configuracionTienda().subscribe({
      next: (r) => {
        this.alias = r.alias;
        this.cargada = true;
        if (this.alias) void this.armarQr();
      },
      error: (e) => (this.error = typeof e.error === 'string' ? e.error : 'No se pudo cargar la tienda.')
    });
    this.api.configuracionTicket().subscribe({
      next: (r) => (this.anchoPos = r.formatoTicket === 'POS-58' ? 58 : 80),
      error: () => (this.anchoPos = 80)
    });
  }

  link(): string {
    return `${location.origin}/tienda/${this.alias}`;
  }

  ladoMm(): number {
    return this.pos ? LADO_POS_MM : LADO_MM[this.tamano];
  }

  papelPos(): string {
    return this.anchoPos === 58 ? 'POS 58 mm' : 'POS 80 mm';
  }

  async descargar(): Promise<void> {
    this.descargando = true;
    this.mensaje = '';
    this.error = '';
    try {
      const ancho = LADO_PX[this.tamano];
      const data = await toDataURL(this.link(), { ...OPCIONES_QR, width: Math.round(ancho * 0.5), margin: 1 });
      const png = await dibujarAfiche(data, ancho);
      const a = document.createElement('a');
      a.href = png;
      a.download = `qr-tienda-${this.tamano}.png`;
      a.click();
      this.mensaje = 'Afiche descargado.';
    } catch {
      this.error = 'No se pudo descargar el afiche.';
    } finally {
      this.descargando = false;
    }
  }

  imprimir(enPos: boolean): void {
    this.pos = enPos;
    this.error = '';
    setTimeout(() => {
      try {
        window.print();
      } catch {
        this.error = 'No se pudo abrir el diálogo de impresión.';
        this.pos = false;
      }
    }, 50);
  }

  @HostListener('window:afterprint')
  alCerrarImpresion(): void {
    this.pos = false;
  }

  private async armarQr(): Promise<void> {
    try {
      this.qr = await toDataURL(this.link(), OPCIONES_QR);
    } catch {
      this.error = 'No se pudo armar el QR.';
    }
  }
}

function cargarImagen(src: string): Promise<HTMLImageElement> {
  return new Promise((ok, fallo) => {
    const img = new Image();
    img.onload = () => ok(img);
    img.onerror = () => fallo();
    img.src = src;
  });
}

async function dibujarAfiche(qrData: string, ancho: number): Promise<string> {
  const qr = await cargarImagen(qrData);
  const letra = ancho * 0.066;
  const caja = ancho * 0.72;
  const yCarro = letra * 1.2;
  const yTitulo = yCarro + letra * 1.45;
  const yLinea = yTitulo + letra * 0.85;
  const cajaY = yLinea + letra * 0.6;
  const yLinea2 = cajaY + caja + letra * 0.6;
  const yPie = yLinea2 + letra * 0.95;
  const alto = Math.round(yPie + letra * 1.15);
  const canvas = document.createElement('canvas');
  canvas.width = ancho;
  canvas.height = alto;
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('sin lienzo');

  ctx.fillStyle = VERDE;
  redondeado(ctx, 0, 0, ancho, alto, ancho * 0.06);
  ctx.fill();
  ctx.save();
  redondeado(ctx, 0, 0, ancho, alto, ancho * 0.06);
  ctx.clip();
  ctx.fillStyle = 'rgba(255,255,255,0.07)';
  ctx.beginPath();
  ctx.moveTo(0, 0);
  ctx.lineTo(ancho * 0.72, 0);
  ctx.lineTo(0, alto * 0.55);
  ctx.fill();
  ctx.fillStyle = 'rgba(0,0,0,0.2)';
  ctx.beginPath();
  ctx.moveTo(ancho, alto);
  ctx.lineTo(ancho * 0.35, alto);
  ctx.lineTo(ancho, alto * 0.48);
  ctx.fill();
  ctx.restore();

  ctx.strokeStyle = '#2d7a64';
  ctx.lineWidth = ancho * 0.012;
  redondeado(ctx, ancho * 0.03, alto * 0.024, ancho * 0.94, alto * 0.952, ancho * 0.045);
  ctx.stroke();

  regla(ctx, ancho * 0.14, yCarro, ancho * 0.22);
  carro(ctx, ancho / 2, yCarro, letra * 1.65);
  regla(ctx, ancho * 0.64, yCarro, ancho * 0.22);

  ctx.font = `800 ${letra}px "Segoe UI", sans-serif`;
  ctx.textBaseline = 'middle';
  ctx.letterSpacing = `${letra * 0.04}px`;
  const tienda = 'TIENDA';
  const online = 'ONLINE';
  const espacio = letra * 0.35;
  const anchoTitulo = ctx.measureText(tienda).width + espacio + ctx.measureText(online).width;
  let x = (ancho - anchoTitulo) / 2;
  ctx.textAlign = 'left';
  ctx.fillStyle = CREMA;
  ctx.fillText(tienda, x, yTitulo);
  x += ctx.measureText(tienda).width + espacio;
  ctx.fillStyle = ORO;
  ctx.fillText(online, x, yTitulo);
  regla(ctx, ancho * 0.11, yLinea, ancho * 0.78);

  const cajaX = (ancho - caja) / 2;
  ctx.fillStyle = '#fff';
  ctx.strokeStyle = ORO;
  ctx.lineWidth = ancho * 0.012;
  redondeado(ctx, cajaX, cajaY, caja, caja, ancho * 0.035);
  ctx.fill();
  ctx.stroke();
  const pad = ancho * 0.028;
  ctx.drawImage(qr, cajaX + pad, cajaY + pad, caja - pad * 2, caja - pad * 2);
  rayos(ctx, cajaX - ancho * 0.03, cajaY + caja / 2, -1, ancho);
  rayos(ctx, cajaX + caja + ancho * 0.03, cajaY + caja / 2, 1, ancho);

  regla(ctx, ancho * 0.11, yLinea2, ancho * 0.78);
  ctx.fillStyle = CREMA;
  ctx.textAlign = 'center';
  ctx.font = `800 ${letra}px "Segoe UI", sans-serif`;
  ctx.fillText('StockControl', ancho / 2, yPie);

  return canvas.toDataURL('image/png');
}

function redondeado(ctx: CanvasRenderingContext2D, x: number, y: number, w: number, h: number, r: number): void {
  ctx.beginPath();
  ctx.roundRect(x, y, w, h, r);
}

function regla(ctx: CanvasRenderingContext2D, x: number, y: number, w: number): void {
  ctx.strokeStyle = ORO;
  ctx.lineWidth = Math.max(2, w * 0.008);
  ctx.lineCap = 'round';
  ctx.beginPath();
  ctx.moveTo(x, y);
  ctx.lineTo(x + w, y);
  ctx.stroke();
}

function carro(ctx: CanvasRenderingContext2D, cx: number, cy: number, s: number): void {
  ctx.save();
  ctx.translate(cx - s / 2, cy - s / 2);
  ctx.scale(s / 24, s / 24);
  ctx.strokeStyle = MENTA;
  ctx.lineWidth = 1.8;
  ctx.lineCap = 'round';
  ctx.lineJoin = 'round';
  ctx.beginPath();
  ctx.moveTo(4, 6);
  ctx.lineTo(6, 6);
  ctx.lineTo(7.2, 15);
  ctx.lineTo(18.5, 15);
  ctx.lineTo(20, 8);
  ctx.lineTo(8, 8);
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(10, 18.5, 1.2, 0, Math.PI * 2);
  ctx.stroke();
  ctx.beginPath();
  ctx.arc(17, 18.5, 1.2, 0, Math.PI * 2);
  ctx.stroke();
  ctx.restore();
}

function rayos(ctx: CanvasRenderingContext2D, x: number, y: number, hacia: 1 | -1, ancho: number): void {
  ctx.strokeStyle = ORO;
  ctx.lineWidth = Math.max(2, ancho * 0.012);
  ctx.lineCap = 'round';
  for (const angulo of [-0.55, 0, 0.55]) {
    const largo = ancho * (angulo === 0 ? 0.055 : 0.04);
    ctx.beginPath();
    ctx.moveTo(x, y);
    ctx.lineTo(x + Math.cos(angulo) * largo * hacia, y + Math.sin(angulo) * largo);
    ctx.stroke();
  }
}
