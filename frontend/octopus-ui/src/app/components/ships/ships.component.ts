import { Component } from '@angular/core';
import { AsyncPipe, DatePipe } from '@angular/common';
import { ShipService } from '../../services/ship.service';

@Component({
  selector: 'app-ships',
  standalone: true,
  imports: [AsyncPipe, DatePipe],
  template: `
    <section class="page">
      <h2>Ships</h2>
      <div class="panel">
        @for (ship of ships$ | async; track ship.id) {
          <p>
            <strong>{{ ship.name }}</strong>
            {{ ship.imoNumber }} - {{ ship.cargoType }} - ETA {{ ship.estimatedArrival | date:'short' }}
          </p>
        }
      </div>
    </section>
  `
})
export class ShipsComponent {
  ships$ = this.ships.getShips();

  constructor(private readonly ships: ShipService) {}
}
