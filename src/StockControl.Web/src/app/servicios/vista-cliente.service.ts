import { Injectable, OnDestroy, signal } from '@angular/core';

export interface LineaVistaCliente {
  nombre: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
  oferta: string;
}

export interface EstadoVistaCliente {
  tipo: 'carrito' | 'cobrando' | 'gracias';
  lineas: LineaVistaCliente[];
  descuentoPorcentaje: number;
  ahorroCombos: number;
  total: number;
  ultimaLinea: number;
  marcaUltima: number;
  qrDataUrl: string;
  medio: string;
}

@Injectable({ providedIn: 'root' })
export class VistaClienteService implements OnDestroy {
  readonly estado = signal<EstadoVistaCliente | null>(null);
  readonly recibidoEn = signal(0);
  private canal: BroadcastChannel | null = null;

  abrir(idLocal: number, nroCaja: number): void {
    this.cerrar();
    this.canal = new BroadcastChannel(`vista-cliente-${idLocal}-${nroCaja}`);
  }

  emitir(estado: EstadoVistaCliente): void {
    this.canal?.postMessage(estado);
  }

  escuchar(): void {
    this.canal!.onmessage = (e: MessageEvent<EstadoVistaCliente>) => {
      this.estado.set(e.data);
      this.recibidoEn.set(Date.now());
    };
  }

  cerrar(): void {
    this.canal?.close();
    this.canal = null;
  }

  ngOnDestroy(): void {
    this.cerrar();
  }
}
