import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService, ConfiguracionLocalDto, CuentaMercadoPagoDto, PlantillasCartelDto, TamanoCartel } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { SesionService } from '../servicios/sesion.service';
import { IconoComponent } from '../icono.component';

@Component({
  selector: 'sc-configuracion',
  standalone: true,
  imports: [FormsModule, IconoComponent],
  template: `
    <div class="topbar">
      <h1>Configuración del local</h1>
    </div>
    @if (error) { <p class="error">{{ error }}</p> }
    <section class="panel config-form">
      <h2><sc-icono nombre="locales" /> Datos del local</h2>
      <label class="field">Nombre del local <input class="wide" type="text" [(ngModel)]="form.nombreLocal" /></label>

      <h2><sc-icono nombre="precios" /> Precios</h2>
      <div class="row">
        <label class="field grow">Factor de ganancia <input class="wide" type="number" min="0.0001" step="0.01" [(ngModel)]="form.factorGanancia" /></label>
        <label class="field grow">IVA <input class="wide" type="number" min="0" step="0.01" [(ngModel)]="form.iva" /></label>
      </div>
      <p class="hint">El precio de lista se calcula como costo × factor de ganancia × IVA.</p>
      <div class="row">
        <label class="field grow">Factor sector <input class="wide" type="number" min="0.0001" step="0.01" [(ngModel)]="form.factorGananciaSector" /></label>
        <label class="field grow">Factor genérico <input class="wide" type="number" min="0.0001" step="0.01" [(ngModel)]="form.factorGananciaGenerico" /></label>
      </div>
      <p class="hint">Sector y genérico se cobran a precio libre. La ganancia se estima como precio − precio / factor. Si el factor genérico queda en 1, usa el factor de ganancia. Con los dos en 1 no se estima.</p>

      <h2><sc-icono nombre="stock" /> Stock</h2>
      <label class="check"><input type="checkbox" [(ngModel)]="form.stockRigido" /> No permitir vender sin stock suficiente</label>
      <label class="field">Umbral de stock bajo <input class="wide" type="number" min="0" step="1" [(ngModel)]="form.umbralStockBajo" /></label>

      <h2><sc-icono nombre="empleados" /> Empleados</h2>
      <label class="check"><input type="checkbox" [(ngModel)]="form.empleadoPuedeModificarPrecios" /> El empleado puede modificar precios de lista</label>
      <label class="check"><input type="checkbox" [(ngModel)]="form.empleadoPuedeFiar" /> El empleado puede fiar</label>

      <h2><sc-icono nombre="tickets" /> Tickets</h2>
      <label class="field">Formato
        <select class="wide" [(ngModel)]="form.formatoTicket">
          <option value="POS-58">POS 58 mm</option>
          <option value="POS-80">POS 80 mm</option>
          <option value="A4">A4</option>
        </select>
      </label>
      <label class="check"><input type="checkbox" [(ngModel)]="form.imprimirTicketAlCobrar" /> Imprimir ticket al cobrar por defecto</label>

      <h2><sc-icono nombre="cajas" /> Cajas</h2>
      <label class="field">Cantidad de cajas/puestos <input class="wide" type="number" min="1" max="50" [(ngModel)]="form.cantidadCajas" /></label>

      <h2 class="titulo-mp">
        <img class="logo-mp logo-mp-color" src="mercadopago.svg" alt="Mercado Pago" />
        <img class="logo-mp logo-mp-pluma" src="mercadopago-claro.svg" alt="Mercado Pago" />
      </h2>
      <p class="hint">La cuenta de este local. El mostrador cobra ahí. El empleado no la cambia.</p>
      @if (mpVinculada) {
        <p class="cuenta-mp"><sc-icono nombre="check" /> Cuenta conectada</p>
      } @else {
        <p class="meta">Sin conectar.</p>
      }
      @if (sesion.puedeConfigurarLocal()) {
        <button class="btn" type="button" (click)="conectarMp()">{{ mpVinculada ? 'Conectar otra cuenta' : 'Conectar Mercado Pago' }}</button>
      }
      @for (nro of puestos(); track nro) {
        <div class="row" style="align-items:center; margin-top:8px">
          <strong style="width:80px">Puesto {{ nro }}</strong>
          @if (puesto(nro)?.listo) {
            <a class="btn" [href]="puesto(nro)?.urlQr" target="_blank" rel="noopener">QR para imprimir</a>
          } @else {
            <span class="meta">Sin QR</span>
          }
          <button class="btn" type="button" (click)="prepararPuesto(nro)">Preparar QR</button>
          <label class="field grow">Lector Point
            <input class="wide" type="text" [ngModel]="lectores[nro] || ''" (ngModelChange)="lector(nro, $event)" />
          </label>
          <button class="btn" type="button" (click)="guardarLector(nro)">Guardar lector</button>
        </div>
      }

      <h2><sc-icono nombre="carteles" /> Carteles</h2>
      @for (t of tamanos; track t) {
        <div class="row" style="align-items:center; margin-bottom:8px">
          <strong style="width:70px">{{ t === 'grande' ? 'Grande' : 'Chico' }}</strong>
          @if (plantillas[t]) {
            <img [src]="plantillas[t]" alt="Plantilla {{ t }}" style="height:48px; border:1px solid var(--line); border-radius:4px" />
            <span class="meta">Plantilla propia</span>
            <button class="btn" type="button" (click)="quitarPlantilla(t)">Usar genérica</button>
          } @else {
            <span class="meta">Plantilla genérica</span>
          }
          <label class="btn">
            Subir {{ t }}
            <input type="file" accept="image/png,image/jpeg" hidden (change)="subirPlantilla(t, $event)" />
          </label>
        </div>
      }
      <p class="hint">PNG o JPG de hasta 1 MB. Se usa de fondo en ese tamaño de cartel.</p>

      <div class="row" style="margin-top:16px">
        <button class="btn btn-primary" type="button" (click)="guardar()">Guardar configuración</button>
      </div>
      @if (mensaje) { <p class="meta">{{ mensaje }}</p> }
    </section>
  `
})
export class ConfiguracionComponent implements OnInit {
  form: ConfiguracionLocalDto & { recalcularPrecios: boolean } = {
    nombreLocal: '', factorGanancia: 1, factorGananciaSector: 1, factorGananciaGenerico: 1, iva: 1.21, stockRigido: false, umbralStockBajo: 5,
    empleadoPuedeModificarPrecios: false, empleadoPuedeFiar: false, formatoTicket: 'POS-80', imprimirTicketAlCobrar: true,
    cantidadCajas: 1, recalcularPrecios: false
  };
  original = { factorGanancia: 1, iva: 1.21 };
  readonly tamanos: TamanoCartel[] = ['grande', 'chico'];
  plantillas: PlantillasCartelDto = { grande: null, chico: null };
  error = '';
  mensaje = '';
  mpVinculada = false;
  mpPuestos: CuentaMercadoPagoDto['puestos'] = [];
  lectores: Record<number, string> = {};

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
    private readonly ruta: ActivatedRoute,
    readonly sesion: SesionService,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    if (!this.sesion.puedeConfigurarLocal()) {
      void this.router.navigateByUrl('/caja');
      return;
    }
    this.api.configuracionLocal().subscribe({
      next: (r) => {
        this.form = { ...r, recalcularPrecios: false };
        this.original = { factorGanancia: r.factorGanancia, iva: r.iva };
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo cargar la configuración.'))
    });
    this.api.plantillasCartel().subscribe({
      next: (r) => (this.plantillas = r),
      error: (e) => (this.error = this.mensajeError(e, 'No se pudieron cargar las plantillas de cartel.'))
    });
    const aviso = this.ruta.snapshot.queryParamMap.get('mp');
    if (aviso === 'ok') this.mensaje = 'Mercado Pago conectado.';
    if (aviso === 'error') this.error = 'No se pudo conectar Mercado Pago.';
    this.cargarMp();
  }

  puestos(): number[] {
    return Array.from({ length: Math.max(1, Number(this.form.cantidadCajas) || 1) }, (_, i) => i + 1);
  }

  puesto(nro: number) {
    return this.mpPuestos.find((p) => p.nroCaja === nro);
  }

  lector(nro: number, valor: string): void {
    this.lectores[nro] = valor;
  }

  conectarMp(): void {
    this.api.conectarMercadoPago().subscribe({
      next: (r) => { window.location.href = r.url; },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo iniciar la conexión.'))
    });
  }

  prepararPuesto(nro: number): void {
    this.api.prepararPuestoMercadoPago(nro).subscribe({
      next: () => {
        this.error = '';
        this.mensaje = `QR del puesto ${nro} listo.`;
        this.cargarMp();
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo preparar el puesto.'))
    });
  }

  guardarLector(nro: number): void {
    const id = this.lectores[nro] ?? this.puesto(nro)?.idLector ?? '';
    this.api.guardarLectorMercadoPago(nro, id).subscribe({
      next: () => {
        this.mensaje = `Lector del puesto ${nro} guardado.`;
        this.cargarMp();
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo guardar el lector.'))
    });
  }

  private cargarMp(): void {
    this.api.cuentaMercadoPago().subscribe({
      next: (r) => {
        this.mpVinculada = r.vinculada;
        this.mpPuestos = r.puestos;
        this.lectores = Object.fromEntries(r.puestos.map((p) => [p.nroCaja, p.idLector]));
      }
    });
  }

  subirPlantilla(tamano: TamanoCartel, evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const archivo = input.files?.[0];
    input.value = '';
    if (!archivo) return;
    if (archivo.size > 1024 * 1024) {
      this.error = 'La imagen no puede superar 1 MB.';
      return;
    }
    const lector = new FileReader();
    lector.onload = () => {
      this.api.subirPlantillaCartel(tamano, String(lector.result)).subscribe({
        next: (r) => {
          this.plantillas = { ...this.plantillas, [tamano]: r.plantilla };
          this.error = '';
          this.mensaje = `Plantilla ${tamano} guardada.`;
        },
        error: (e) => (this.error = this.mensajeError(e, 'No se pudo subir la plantilla.'))
      });
    };
    lector.readAsDataURL(archivo);
  }

  quitarPlantilla(tamano: TamanoCartel): void {
    this.api.quitarPlantillaCartel(tamano).subscribe({
      next: () => {
        this.plantillas = { ...this.plantillas, [tamano]: null };
        this.error = '';
        this.mensaje = `Cartel ${tamano}: vuelve a la plantilla genérica.`;
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo quitar la plantilla.'))
    });
  }

  async guardar(): Promise<void> {
    if (!this.form.nombreLocal.trim() || this.form.factorGanancia <= 0 || this.form.factorGananciaSector <= 0 || this.form.factorGananciaGenerico <= 0 || this.form.iva < 0 || this.form.umbralStockBajo < 0 || this.form.cantidadCajas < 1 || this.form.cantidadCajas > 50) {
      this.error = 'Revisá los valores de configuración.';
      return;
    }
    const cambioPrecios = this.form.factorGanancia !== this.original.factorGanancia || this.form.iva !== this.original.iva;
    const recalcular = cambioPrecios && await this.dialogo.confirmar('Cambiaste el factor o el IVA. ¿Querés actualizar los precios de los productos?');
    this.api.guardarConfiguracionLocal({ ...this.form, recalcularPrecios: recalcular }).subscribe({
      next: (r) => {
        this.original = { factorGanancia: this.form.factorGanancia, iva: this.form.iva };
        this.error = '';
        this.mensaje = r.recalculados > 0 ? `Configuración guardada. Productos actualizados: ${r.recalculados}.` : 'Configuración guardada.';
      },
      error: (e) => (this.error = this.mensajeError(e, 'No se pudo guardar la configuración.'))
    });
  }

  private mensajeError(error: { error?: unknown }, defecto: string): string {
    return typeof error.error === 'string' ? error.error : defecto;
  }
}
