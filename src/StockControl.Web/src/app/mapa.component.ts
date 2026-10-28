import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnChanges, OnDestroy, Output, ViewChild, ViewEncapsulation } from '@angular/core';
import type { Map as MapaLeaflet, Marker } from 'leaflet';

export interface Ubicacion {
  latitud: number;
  longitud: number;
}

const ATRIBUCION = '© <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a> contributors';

@Component({
  selector: 'sc-mapa',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  styleUrl: '../../node_modules/leaflet/dist/leaflet.css',
  template: `<div #contenedor class="mapa" role="region" aria-label="Mapa con la ubicación del local"></div>`
})
export class MapaComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input({ required: true }) latitud!: number;
  @Input({ required: true }) longitud!: number;
  @Input() editable = false;
  @Output() readonly movido = new EventEmitter<Ubicacion>();
  @ViewChild('contenedor', { static: true }) private readonly contenedor!: ElementRef<HTMLDivElement>;

  private mapa?: MapaLeaflet;
  private marcador?: Marker;

  async ngAfterViewInit(): Promise<void> {
    const L = await import('leaflet');
    const punto: [number, number] = [this.latitud, this.longitud];
    this.mapa = L.map(this.contenedor.nativeElement, { scrollWheelZoom: false }).setView(punto, 16);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: ATRIBUCION }).addTo(this.mapa);
    const icono = L.divIcon({ className: 'marcador-tienda', html: '<span></span>', iconSize: [22, 22], iconAnchor: [11, 22] });
    this.marcador = L.marker(punto, { icon: icono, draggable: this.editable, title: 'Ubicación del local' }).addTo(this.mapa);
    if (!this.editable) return;
    this.marcador.on('dragend', () => this.avisar());
    this.mapa.on('click', (e) => {
      this.marcador?.setLatLng(e.latlng);
      this.avisar();
    });
  }

  ngOnChanges(): void {
    if (!this.mapa || !this.marcador) return;
    const actual = this.marcador.getLatLng();
    if (actual.lat === this.latitud && actual.lng === this.longitud) return;
    this.marcador.setLatLng([this.latitud, this.longitud]);
    this.mapa.setView([this.latitud, this.longitud], this.mapa.getZoom());
  }

  ngOnDestroy(): void {
    this.mapa?.remove();
  }

  private avisar(): void {
    const p = this.marcador!.getLatLng();
    this.movido.emit({ latitud: redondear(p.lat), longitud: redondear(p.lng) });
  }
}

function redondear(valor: number): number {
  return Math.round(valor * 1e6) / 1e6;
}
