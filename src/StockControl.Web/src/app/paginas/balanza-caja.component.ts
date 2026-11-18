import { Component, ElementRef, EventEmitter, HostListener, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, ProductoDto } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { LineaCarritoPendiente } from '../servicios/sesion.service';

interface PesadaBalanza {
  idProducto: number;
  codigo: string;
  nombre: string;
  kilos: number;
  precioPorKilo: number;
  importe: number;
  costo: number;
  sector: boolean;
}

const CLAVE_PUERTO = 'sc_puerto_balanza';

@Component({
  selector: 'sc-balanza-caja',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="balanza-fondo" (keydown.escape)="salir()">
      <div class="balanza-pantalla">
        <section class="balanza-col">
          <label class="field">
            Buscador
            <input
              #buscador
              class="wide"
              type="text"
              [(ngModel)]="q"
              (ngModelChange)="filtrar()"
              (keydown)="teclaBuscador($event)"
              placeholder="Nombre, código o peso"
            />
          </label>
          <div class="balanza-lista" tabindex="0" (keydown)="teclaLista($event)">
            @for (p of visibles; track p.id; let i = $index) {
              <button
                type="button"
                class="balanza-item"
                [class.on]="elegido?.id === p.id"
                (click)="clickProducto(p)"
              >
                <span>{{ p.nombre }}</span>
                <span class="meta">{{ dinero(p.precioPorKilo ?? 0) }}/kg</span>
              </button>
            } @empty {
              <p class="meta" style="padding:12px">No hay productos con precio por kilo.</p>
            }
          </div>
        </section>

        <section class="balanza-centro">
          <div class="balanza-peso">Pesando… {{ textoPeso }}</div>
          <div class="balanza-total">Total: {{ textoTotal }}</div>
          @if (avisoPuerto) {
            <p class="meta aviso">{{ avisoPuerto }}</p>
            <button class="btn" type="button" (click)="elegirPuerto()">Elegir puerto</button>
          }
        </section>

        <section class="balanza-col balanza-derecha">
          <h2>Carrito</h2>
          <div class="balanza-carrito">
            <table class="data">
              <thead>
                <tr>
                  <th>Producto</th>
                  <th class="num">Kg</th>
                  <th class="num">Importe</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                @for (p of pesadas; track $index) {
                  <tr>
                    <td>{{ p.nombre }}</td>
                    <td class="num">{{ kilos(p.kilos) }}</td>
                    <td class="num">{{ dinero(p.importe) }}</td>
                    <td><button class="btn" type="button" (click)="sacar($index)" aria-label="Quitar">×</button></td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
          <div class="balanza-total-carrito">Total: {{ dinero(totalCarrito) }}</div>
          <button class="btn btn-primary grow" type="button" [disabled]="pesadas.length === 0" (click)="aceptar()">Aceptar</button>
          <button class="btn grow" type="button" (click)="salir()">Salir</button>
        </section>
      </div>
    </div>
  `,
  styles: `
    .balanza-fondo {
      position: fixed;
      inset: 0;
      z-index: 40;
      background: var(--bg);
      color: var(--text);
      padding: 12px;
      box-sizing: border-box;
    }
    .balanza-pantalla {
      display: grid;
      grid-template-columns: 32% 1fr 25%;
      gap: 12px;
      height: 100%;
      min-height: 0;
    }
    .balanza-col {
      display: flex;
      flex-direction: column;
      gap: 8px;
      min-height: 0;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 10px;
      padding: 12px;
    }
    .balanza-lista {
      flex: 1;
      overflow: auto;
      border: 1px solid var(--line);
      border-radius: 8px;
      min-height: 0;
    }
    .balanza-item {
      display: flex;
      justify-content: space-between;
      gap: 8px;
      width: 100%;
      text-align: left;
      padding: 10px 12px;
      border: 0;
      border-bottom: 1px solid var(--line);
      background: transparent;
      color: inherit;
      cursor: pointer;
      font: inherit;
    }
    .balanza-item.on,
    .balanza-item:hover {
      background: var(--hover, color-mix(in srgb, var(--accent) 12%, transparent));
    }
    .balanza-centro {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 16px;
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 10px;
      padding: 24px;
      text-align: center;
    }
    .balanza-peso {
      font-size: clamp(28px, 5vw, 48px);
      font-weight: 700;
    }
    .balanza-total {
      font-size: clamp(22px, 3vw, 32px);
      font-weight: 600;
    }
    .aviso { color: var(--warn, #c47b00); }
    .balanza-derecha h2 {
      margin: 0;
      font-size: 16px;
    }
    .balanza-carrito {
      flex: 1;
      overflow: auto;
      border: 1px solid var(--line);
      border-radius: 8px;
      min-height: 0;
    }
    .balanza-total-carrito {
      font-weight: 700;
      text-align: right;
    }
    .grow { width: 100%; }
    @media (max-width: 900px) {
      .balanza-pantalla {
        grid-template-columns: 1fr;
        grid-template-rows: auto auto 1fr;
        overflow: auto;
      }
    }
  `
})
export class BalanzaCajaComponent implements OnInit, OnDestroy {
  @Output() readonly aceptarPesadas = new EventEmitter<LineaCarritoPendiente[]>();
  @Output() readonly cerrar = new EventEmitter<void>();
  @ViewChild('buscador') buscador?: ElementRef<HTMLInputElement>;

  todos: ProductoDto[] = [];
  visibles: ProductoDto[] = [];
  pesadas: PesadaBalanza[] = [];
  elegido: ProductoDto | null = null;
  q = '';
  peso = 0;
  avisoPuerto = '';
  error = '';

  private puerto: any = null;
  private lector: ReadableStreamDefaultReader<string> | null = null;
  private buffer = '';
  private cerrado = false;

  constructor(
    private readonly api: ApiService,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.api.productos('').subscribe({
      next: (r) => {
        this.todos = r.items.filter((p) => Number(p.precioPorKilo) > 0);
        this.filtrar();
        setTimeout(() => this.buscador?.nativeElement.focus());
      },
      error: () => (this.error = 'No se pudieron cargar los productos.')
    });
    void this.conectarPuertoGuardado();
  }

  ngOnDestroy(): void {
    this.cerrado = true;
    void this.cerrarPuerto();
  }

  get textoPeso(): string {
    return this.pesoValido ? `${this.kilos(this.peso)} kg` : '';
  }

  get textoTotal(): string {
    if (!this.elegido || !this.pesoValido) return '';
    return this.dinero(this.importe(this.peso, Number(this.elegido.precioPorKilo)));
  }

  get totalCarrito(): number {
    return Math.round(this.pesadas.reduce((s, p) => s + p.importe, 0) * 100) / 100;
  }

  get pesoValido(): boolean {
    return this.peso > 0 && this.peso <= 100;
  }

  dinero(n: number): string {
    return `$ ${Number(n).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  kilos(n: number): string {
    return Number(n).toLocaleString('es-AR', { minimumFractionDigits: 0, maximumFractionDigits: 3 });
  }

  filtrar(): void {
    const q = this.q.trim().toLowerCase();
    this.visibles = !q
      ? [...this.todos]
      : this.todos.filter(
          (p) => p.nombre.toLowerCase().includes(q) || p.codigo.toLowerCase().includes(q)
        );
    if (this.elegido && !this.visibles.some((p) => p.id === this.elegido!.id)) this.elegido = null;
  }

  clickProducto(p: ProductoDto): void {
    this.elegido = p;
    this.intentarAgregar();
  }

  teclaBuscador(ev: KeyboardEvent): void {
    if (ev.key === 'ArrowDown') {
      ev.preventDefault();
      if (this.visibles.length) this.elegido = this.visibles[0];
      return;
    }
    if (ev.key !== 'Enter') return;
    ev.preventDefault();
    const texto = this.q.trim();
    this.q = '';
    this.filtrar();
    if (!texto) {
      this.intentarAgregar();
      return;
    }
    const delListado = this.todos.find((p) => p.codigo.toLowerCase() === texto.toLowerCase());
    if (delListado) {
      this.elegido = delListado;
      this.filtrar();
      return;
    }
    this.api.productos('', { codigo: texto }).subscribe({
      next: (r) => {
        const p = r.items[0];
        if (p) {
          if (Number(p.precioPorKilo) > 0) {
            if (!this.todos.some((t) => t.id === p.id)) this.todos = [...this.todos, p];
            this.elegido = p;
            this.filtrar();
          } else {
            void this.dialogo.confirmar('No es un producto por kilo.', 'Entendido', 'Cerrar');
          }
          return;
        }
        const kilos = this.parsePeso(texto);
        if (kilos != null && kilos > 0 && kilos <= 100) this.peso = kilos;
      },
      error: () => {
        const kilos = this.parsePeso(texto);
        if (kilos != null && kilos > 0 && kilos <= 100) this.peso = kilos;
      }
    });
  }

  teclaLista(ev: KeyboardEvent): void {
    if (!this.visibles.length) return;
    const i = this.elegido ? this.visibles.findIndex((p) => p.id === this.elegido!.id) : -1;
    if (ev.key === 'ArrowDown') {
      ev.preventDefault();
      this.elegido = this.visibles[Math.min(i + 1, this.visibles.length - 1)];
    } else if (ev.key === 'ArrowUp') {
      ev.preventDefault();
      this.elegido = this.visibles[Math.max(i - 1, 0)];
    } else if (ev.key === 'Enter') {
      ev.preventDefault();
      this.intentarAgregar();
    }
  }

  @HostListener('document:keydown.escape')
  onEsc(): void {
    void this.salir();
  }

  intentarAgregar(): void {
    if (!this.elegido || !this.pesoValido) return;
    const kilos = Math.round(this.peso * 1000) / 1000;
    const precioPorKilo = Number(this.elegido.precioPorKilo);
    this.pesadas = [
      {
        idProducto: this.elegido.id,
        codigo: this.elegido.codigo,
        nombre: this.elegido.nombre,
        kilos,
        precioPorKilo,
        importe: this.importe(kilos, precioPorKilo),
        costo: Number(this.elegido.costo) || 0,
        sector: !!this.elegido.productoSector
      },
      ...this.pesadas
    ];
    setTimeout(() => this.buscador?.nativeElement.focus());
  }

  sacar(i: number): void {
    this.pesadas = this.pesadas.filter((_, n) => n !== i);
  }

  aceptar(): void {
    if (!this.pesadas.length) return;
    const lineas: LineaCarritoPendiente[] = this.pesadas
      .slice()
      .reverse()
      .map((p) => ({
        idProducto: p.idProducto,
        codigo: p.codigo,
        nombre: p.nombre,
        cantidad: p.kilos,
        precioUnitario: p.precioPorKilo,
        costo: p.costo,
        sector: p.sector,
        generico: false,
        porKilo: true
      }));
    this.aceptarPesadas.emit(lineas);
  }

  async salir(): Promise<void> {
    if (this.pesadas.length) {
      const ok = await this.dialogo.confirmar('Hay pesadas sin pasar al carrito. ¿Salir igual?');
      if (!ok) return;
    }
    this.cerrar.emit();
  }

  async elegirPuerto(): Promise<void> {
    const serial = (navigator as any).serial;
    if (!serial) {
      this.avisoPuerto = 'Este navegador no soporta puerto serie. Usá el lector o escribí el peso.';
      return;
    }
    try {
      const port = await serial.requestPort();
      await this.abrirPuerto(port);
      const info = port.getInfo?.() ?? {};
      localStorage.setItem(CLAVE_PUERTO, JSON.stringify({
        usbVendorId: info.usbVendorId,
        usbProductId: info.usbProductId
      }));
      this.avisoPuerto = '';
    } catch {
      this.avisoPuerto = 'Sin balanza por puerto. Usá el lector o escribí el peso.';
    }
  }

  private async conectarPuertoGuardado(): Promise<void> {
    const serial = (navigator as any).serial;
    if (!serial) {
      this.avisoPuerto = 'Sin balanza por puerto. Usá el lector o escribí el peso.';
      return;
    }
    const raw = localStorage.getItem(CLAVE_PUERTO);
    try {
      const ports: any[] = await serial.getPorts();
      let port = ports[0];
      if (raw) {
        const guardado = JSON.parse(raw) as { usbVendorId?: number; usbProductId?: number };
        port =
          ports.find((p) => {
            const info = p.getInfo?.() ?? {};
            return info.usbVendorId === guardado.usbVendorId && info.usbProductId === guardado.usbProductId;
          }) ?? port;
      }
      if (!port) {
        this.avisoPuerto = 'Sin balanza por puerto. Usá el lector o escribí el peso.';
        return;
      }
      await this.abrirPuerto(port);
      this.avisoPuerto = '';
    } catch {
      this.avisoPuerto = 'Sin balanza por puerto. Usá el lector o escribí el peso.';
    }
  }

  private async abrirPuerto(port: any): Promise<void> {
    await this.cerrarPuerto();
    await port.open({ baudRate: 9600 });
    this.puerto = port;
    const decoder = new TextDecoderStream();
    port.readable.pipeTo(decoder.writable).catch(() => undefined);
    this.lector = decoder.readable.getReader();
    void this.leerSerie();
  }

  private async leerSerie(): Promise<void> {
    if (!this.lector) return;
    try {
      while (!this.cerrado) {
        const { value, done } = await this.lector.read();
        if (done) break;
        if (!value) continue;
        this.buffer += value;
        let corte: number;
        while ((corte = this.buffer.search(/[\r\n]/)) >= 0) {
          const linea = this.buffer.slice(0, corte).trim();
          this.buffer = this.buffer.slice(corte + 1);
          const kilos = this.parsePeso(linea);
          if (kilos != null && kilos > 0 && kilos <= 100) this.peso = kilos;
        }
      }
    } catch {
      /* puerto cerrado */
    }
  }

  private async cerrarPuerto(): Promise<void> {
    try {
      await this.lector?.cancel();
      this.lector?.releaseLock();
    } catch {
      /* ignore */
    }
    this.lector = null;
    try {
      await this.puerto?.close();
    } catch {
      /* ignore */
    }
    this.puerto = null;
  }

  private importe(kilos: number, precioPorKilo: number): number {
    return Math.round(kilos * precioPorKilo * 100) / 100;
  }

  private parsePeso(texto: string): number | null {
    const m = texto.match(/-?\d+[.,]?\d*/);
    if (!m) return null;
    const n = Number(m[0].replace(',', '.'));
    return Number.isFinite(n) ? n : null;
  }
}
