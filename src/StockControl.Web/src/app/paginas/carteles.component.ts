import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService, CartelesDto, ProductoDto, TamanoCartel } from '../servicios/api.service';

const POR_HOJA: Record<TamanoCartel, number> = { grande: 6, chico: 30 };

@Component({
  selector: 'sc-carteles',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <div class="topbar">
      <h1>Carteles de precio</h1>
      <a class="btn" routerLink="/productos">Volver a Productos</a>
    </div>
    <div class="toolbar" style="margin-bottom:12px">
      <input class="grow" type="search" [ngModel]="q" (ngModelChange)="buscar($event)" placeholder="Buscar nombre o código" />
      <label class="check"><input type="radio" name="tamano" value="grande" [(ngModel)]="tamano" /> Grande (6 por hoja)</label>
      <label class="check"><input type="radio" name="tamano" value="chico" [(ngModel)]="tamano" /> Chico (30 por hoja)</label>
      <label class="check"><input type="checkbox" name="llenar" [(ngModel)]="llenar" /> Llenar la hoja repitiendo los carteles</label>
      <span class="meta">{{ elegidos.size }} seleccionados · {{ hojas() }} hoja(s)</span>
      <button class="btn btn-primary" type="button" [disabled]="elegidos.size === 0 || generando" (click)="generar()">
        {{ generando ? 'Generando...' : 'Generar' }}
      </button>
    </div>
    @if (error) {
      <p class="error">{{ error }}</p>
    }
    <section class="panel">
      <div class="toolbar" style="padding:12px 12px 0">
        <button class="btn" type="button" (click)="marcarVisibles()">Marcar visibles</button>
        <button class="btn" type="button" [disabled]="elegidos.size === 0" (click)="elegidos.clear()">Limpiar selección</button>
      </div>
      <table class="data">
        <thead>
          <tr>
            <th style="width:40px"></th>
            <th>Código</th>
            <th>Nombre</th>
            <th class="num">Precio</th>
          </tr>
        </thead>
        <tbody>
          @for (p of productos; track p.id) {
            <tr class="clickable" [class.selected]="elegidos.has(p.id)" (click)="alternar(p)">
              <td><input type="checkbox" [checked]="elegidos.has(p.id)" (click)="$event.stopPropagation(); alternar(p)" /></td>
              <td>{{ p.codigo }}</td>
              <td>{{ p.nombre }}</td>
              <td class="num">{{ precio(p.precio) }}</td>
            </tr>
          }
        </tbody>
      </table>
    </section>
    @if (carteles) {
      <div class="carteles-print">
        @for (hoja of carteles.hojas; track $index) {
          <div class="cartel-hoja" [class.cartel-hoja-chico]="carteles.tamano === 'chico'">
            @for (c of hoja; track $index) {
              <div
                class="cartel"
                [class.cartel-generico]="!carteles.plantilla"
                [style.background-image]="carteles.plantilla ? 'url(' + carteles.plantilla + ')' : null"
              >
                @if (!carteles.plantilla) {
                  <div class="cartel-local">{{ carteles.nombreLocal }}</div>
                }
                <div class="cartel-nombre">{{ c.nombre }}</div>
                <div class="cartel-precio">{{ precio(c.precio) }}</div>
              </div>
            }
          </div>
        }
      </div>
    }
  `
})
export class CartelesComponent implements OnInit {
  q = '';
  tamano: TamanoCartel = 'grande';
  llenar = false;
  productos: ProductoDto[] = [];
  elegidos = new Set<number>();
  carteles: CartelesDto | null = null;
  generando = false;
  error = '';
  private temporizador?: ReturnType<typeof setTimeout>;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.cargar();
  }

  buscar(texto: string): void {
    this.q = texto;
    clearTimeout(this.temporizador);
    this.temporizador = setTimeout(() => this.cargar(), 250);
  }

  cargar(): void {
    this.api.productos(this.q).subscribe({
      next: (r) => (this.productos = r.items),
      error: () => (this.error = 'No se pudieron cargar los productos.')
    });
  }

  alternar(p: ProductoDto): void {
    if (this.elegidos.has(p.id)) this.elegidos.delete(p.id);
    else this.elegidos.add(p.id);
  }

  marcarVisibles(): void {
    this.productos.forEach((p) => this.elegidos.add(p.id));
  }

  hojas(): number {
    return Math.ceil(this.elegidos.size / POR_HOJA[this.tamano]);
  }

  generar(): void {
    if (this.elegidos.size > 300) {
      this.error = 'Máximo 300 carteles por vez.';
      return;
    }
    this.generando = true;
    this.error = '';
    this.api.generarCarteles([...this.elegidos], this.tamano).subscribe({
      next: (r) => {
        this.generando = false;
        this.carteles = this.llenar ? { ...r, hojas: r.hojas.map((h) => this.repetir(h)) } : r;
        setTimeout(() => {
          try {
            window.print();
          } catch {
            this.error = 'No se pudo abrir el diálogo de impresión.';
          }
          this.carteles = null;
        }, 50);
      },
      error: (e) => {
        this.generando = false;
        this.error = typeof e.error === 'string' ? e.error : 'No se pudieron generar los carteles.';
      }
    });
  }

  private repetir<T>(hoja: T[]): T[] {
    const total = POR_HOJA[this.tamano];
    return hoja.length === 0 ? hoja : Array.from({ length: total }, (_, i) => hoja[i % hoja.length]);
  }

  precio(n: number): string {
    return `$ ${Math.trunc(Number(n)).toLocaleString('es-AR')}`;
  }
}
