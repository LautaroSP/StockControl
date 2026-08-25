import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TemaService } from './servicios/tema.service';

@Component({
  selector: 'sc-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />'
})
export class AppComponent {
  constructor(tema: TemaService) {
    tema.aplicar();
  }
}
