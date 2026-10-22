import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiService } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { SesionService } from '../servicios/sesion.service';
import { IconoComponent } from '../icono.component';
import { TemaComponent } from '../tema.component';

@Component({
  selector: 'sc-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, DatePipe, IconoComponent, TemaComponent],
  template: `
    <div class="app">
      <aside class="sidebar" [class.abierto]="menuAbierto">
        <sc-tema [fijo]="false" />
        <div class="brand">
          <strong>StockControl</strong>
          <span>{{ sesion.nombreLocal() }}</span>
          @if (sesion.hayCaja()) {
            <span>Caja {{ sesion.nroCaja() }}</span>
          }
        </div>
        <button class="menu-btn" type="button" (click)="menuAbierto = !menuAbierto" [attr.aria-expanded]="menuAbierto" aria-label="Menú">
          <span></span><span></span><span></span>
        </button>
        <nav class="nav" (click)="menuAbierto = false">
          <a routerLink="/caja" routerLinkActive="active"><sc-icono nombre="caja" /> Caja</a>
          <a routerLink="/productos" routerLinkActive="active"><sc-icono nombre="productos" /> Productos</a>
          @if (sesion.esDueno()) {
            <a routerLink="/estadisticas" routerLinkActive="active"><sc-icono nombre="estadisticas" /> Estadísticas</a>
            <a routerLink="/grupos" routerLinkActive="active"><sc-icono nombre="grupos" /> Grupos</a>
            <a routerLink="/medios-pago" routerLinkActive="active"><sc-icono nombre="medios" /> Medios de pago</a>
          }
          @if (sesion.puedeGestionarProveedores()) {
            <a routerLink="/proveedores" routerLinkActive="active"><sc-icono nombre="proveedores" /> Proveedores</a>
          }
          @if (sesion.puedeGestionarUsuarios()) {
            <a routerLink="/usuarios" routerLinkActive="active"><sc-icono nombre="usuarios" /> Usuarios</a>
          }
          @if (sesion.puedeConfigurarLocal()) {
            <a routerLink="/configuracion" routerLinkActive="active"><sc-icono nombre="configuracion" /> Configuración</a>
            <a routerLink="/activar" routerLinkActive="active"><sc-icono nombre="abono" /> Abono</a>
          }
          <a routerLink="/informes" routerLinkActive="active"><sc-icono nombre="informes" /> Informes</a>
          <a routerLink="/cajas" routerLinkActive="active"><sc-icono nombre="cajas" /> Cajas</a>
          <a routerLink="/elegir-caja"><sc-icono nombre="cambiar" /> Cambiar caja</a>
          <a routerLink="/locales"><sc-icono nombre="locales" /> Locales</a>
        </nav>
        <div class="sidebar-foot">
          <div>{{ sesion.nombre() }} · {{ sesion.rol() }}</div>
          <button class="btn btn-ghost" type="button" (click)="salir()">Salir</button>
        </div>
      </aside>
      <main class="main">
        @if (sesion.abono(); as abono) {
          @if (abono.enGracia) {
            <div class="aviso-abono aviso-abono-vencido">
              No pudimos cobrar el débito. La caja se bloquea el {{ abono.limite | date: 'dd/MM' }} a las {{ abono.limite | date: 'HH:mm' }}.
              @if (sesion.puedeConfigurarLocal()) {
                <a class="btn btn-primary" routerLink="/activar">Abono</a>
              }
            </div>
          } @else {
            @switch (abono.estado) {
              @case ('trial') {
                <div class="aviso-abono">
                  Prueba: te quedan {{ abono.diasRestantes }} días.
                  @if (sesion.puedeConfigurarLocal()) {
                    <a class="btn btn-primary" routerLink="/activar">Pagar</a>
                  }
                </div>
              }
              @case ('por_vencer') {
                @if (abono.debito !== 'activa') {
                  <div class="aviso-abono">
                    Tu abono vence el {{ abono.vence | date: 'dd/MM' }} a las {{ abono.vence | date: 'HH:mm' }}
                    ({{ abono.diasRestantes }} {{ abono.diasRestantes === 1 ? 'día' : 'días' }}).
                    @if (sesion.puedeConfigurarLocal()) {
                      <a class="btn btn-primary" routerLink="/activar">Pagar</a>
                    }
                  </div>
                }
              }
              @case ('vencido') {
                <div class="aviso-abono aviso-abono-vencido">
                  Local vencido: no se puede cobrar.
                  @if (sesion.puedeConfigurarLocal()) {
                    <a class="btn btn-primary" routerLink="/activar">Pagar</a>
                  } @else {
                    <a routerLink="/contacto">Contactá a ventas</a>
                  }
                </div>
              }
            }
          }
        }
        <router-outlet />
      </main>
    </div>
  `
})
export class ShellComponent implements OnInit {
  menuAbierto = false;

  constructor(
    readonly sesion: SesionService,
    private readonly api: ApiService,
    private readonly router: Router,
    private readonly dialogo: DialogoService
  ) {}

  ngOnInit(): void {
    this.api.abonoActual().subscribe({ next: (a) => this.sesion.abono.set(a) });
  }

  async salir(): Promise<void> {
    if (!await this.dialogo.confirmar('¿Cerrar la sesión?')) return;
    this.sesion.salir();
    void this.router.navigateByUrl('/login');
  }
}
