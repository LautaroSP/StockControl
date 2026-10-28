import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { ApiService } from '../servicios/api.service';
import { DialogoService } from '../servicios/dialogo.service';
import { SesionService } from '../servicios/sesion.service';
import { IconoComponent } from '../icono.component';
import { TemaComponent } from '../tema.component';

type Grupo = 'reportes' | 'tienda' | 'administracion' | 'configuracion';

const CLAVE_GRUPOS = 'sc-barra-grupos';

const RUTAS_GRUPO: Record<Grupo, string[]> = {
  reportes: ['/informes', '/cajas', '/estadisticas'],
  tienda: ['/mi-tienda'],
  administracion: ['/proveedores', '/usuarios'],
  configuracion: ['/configuracion', '/medios-pago', '/activar']
};

function leerGrupos(): Record<Grupo, boolean> {
  const cerrados = { reportes: false, tienda: false, administracion: false, configuracion: false };
  try {
    return { ...cerrados, ...JSON.parse(localStorage.getItem(CLAVE_GRUPOS) ?? '{}') };
  } catch {
    return cerrados;
  }
}

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
          @if (puedeFiar) {
            <a routerLink="/cuentas" routerLinkActive="active"><sc-icono nombre="tickets" /> Cuentas</a>
          }
          @if (sesion.esDueno()) {
            <a routerLink="/grupos" routerLinkActive="active"><sc-icono nombre="grupos" /> Grupos</a>
            <a routerLink="/ofertas" routerLinkActive="active"><sc-icono nombre="precios" /> Ofertas</a>
            <a routerLink="/combos" routerLinkActive="active"><sc-icono nombre="precios" /> Combos</a>
          }

          <button class="nav-grupo" type="button" [attr.aria-expanded]="abierto('reportes')" (click)="alternar('reportes', $event)">
            Reportes <span class="flecha">▸</span>
          </button>
          @if (abierto('reportes')) {
            <div class="nav-sub">
              <a routerLink="/informes" routerLinkActive="active"><sc-icono nombre="informes" /> Informes</a>
              <a routerLink="/cajas" routerLinkActive="active"><sc-icono nombre="cajas" /> Cajas</a>
              @if (sesion.esDueno()) {
                <a routerLink="/estadisticas" routerLinkActive="active"><sc-icono nombre="estadisticas" /> Estadísticas</a>
              }
            </div>
          }

          @if (sesion.puedeConfigurarLocal()) {
            <button class="nav-grupo" type="button" [attr.aria-expanded]="abierto('tienda')" (click)="alternar('tienda', $event)">
              Tienda <span class="flecha">▸</span>
            </button>
            @if (abierto('tienda')) {
              <div class="nav-sub">
                <a routerLink="/mi-tienda" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }"><sc-icono nombre="tienda" /> Tienda online</a>
                <a routerLink="/mi-tienda/configuracion" routerLinkActive="active"><sc-icono nombre="configuracion" /> Configuración</a>
                <a routerLink="/mi-tienda/imagenes" routerLinkActive="active"><sc-icono nombre="imagenes" /> Imágenes de productos</a>
              </div>
            }
          }

          @if (sesion.puedeGestionarProveedores() || sesion.puedeGestionarUsuarios()) {
            <button class="nav-grupo" type="button" [attr.aria-expanded]="abierto('administracion')" (click)="alternar('administracion', $event)">
              Administración <span class="flecha">▸</span>
            </button>
            @if (abierto('administracion')) {
              <div class="nav-sub">
                @if (sesion.puedeGestionarProveedores()) {
                  <a routerLink="/proveedores" routerLinkActive="active"><sc-icono nombre="proveedores" /> Proveedores</a>
                }
                @if (sesion.puedeGestionarUsuarios()) {
                  <a routerLink="/usuarios" routerLinkActive="active"><sc-icono nombre="usuarios" /> Usuarios</a>
                }
              </div>
            }
          }

          @if (sesion.esDueno() || sesion.puedeConfigurarLocal()) {
            <button class="nav-grupo" type="button" [attr.aria-expanded]="abierto('configuracion')" (click)="alternar('configuracion', $event)">
              Configuración <span class="flecha">▸</span>
            </button>
            @if (abierto('configuracion')) {
              <div class="nav-sub">
                @if (sesion.puedeConfigurarLocal()) {
                  <a routerLink="/configuracion" routerLinkActive="active"><sc-icono nombre="configuracion" /> Local</a>
                }
                @if (sesion.esDueno()) {
                  <a routerLink="/medios-pago" routerLinkActive="active"><sc-icono nombre="medios" /> Medios de pago</a>
                }
                @if (sesion.puedeConfigurarLocal()) {
                  <a routerLink="/activar" routerLinkActive="active"><sc-icono nombre="abono" /> Abono</a>
                }
              </div>
            }
          }
        </nav>
        <div class="sidebar-foot">
          <div class="foot-links" (click)="menuAbierto = false">
            <a routerLink="/elegir-caja"><sc-icono nombre="cambiar" /> Cambiar caja</a>
            <a routerLink="/locales"><sc-icono nombre="locales" /> Cambiar local</a>
          </div>
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
  puedeFiar = false;
  private grupos: Record<Grupo, boolean> = leerGrupos();

  constructor(
    readonly sesion: SesionService,
    private readonly api: ApiService,
    private readonly router: Router,
    private readonly dialogo: DialogoService,
    private readonly destruir: DestroyRef
  ) {}

  ngOnInit(): void {
    this.api.abonoActual().subscribe({ next: (a) => this.sesion.abono.set(a) });
    this.api.permisosCuentas().subscribe({ next: (p) => (this.puedeFiar = p.puedeFiar) });
    this.abrirGrupoDe(this.router.url);
    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd), takeUntilDestroyed(this.destruir))
      .subscribe((e) => this.abrirGrupoDe((e as NavigationEnd).urlAfterRedirects));
  }

  abierto(grupo: Grupo): boolean {
    return this.grupos[grupo];
  }

  alternar(grupo: Grupo, e: Event): void {
    e.stopPropagation();
    this.grupos = { ...this.grupos, [grupo]: !this.grupos[grupo] };
    localStorage.setItem(CLAVE_GRUPOS, JSON.stringify(this.grupos));
  }

  private abrirGrupoDe(url: string): void {
    const grupo = (Object.keys(RUTAS_GRUPO) as Grupo[]).find((g) => RUTAS_GRUPO[g].some((r) => url.startsWith(r)));
    if (grupo && !this.grupos[grupo]) this.grupos = { ...this.grupos, [grupo]: true };
  }

  async salir(): Promise<void> {
    if (!await this.dialogo.confirmar('¿Cerrar la sesión?')) return;
    this.sesion.salir();
    void this.router.navigateByUrl('/login');
  }
}
