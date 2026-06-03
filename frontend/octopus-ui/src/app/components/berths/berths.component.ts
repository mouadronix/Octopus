import { Component } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { BerthService } from '../../services/berth.service';

@Component({
  selector: 'app-berths',
  standalone: true,
  imports: [AsyncPipe],
  template: `
    <section class="page">
      <h2>Berths</h2>
      <div class="panel">
        @for (berth of berths$ | async; track berth.id) {
          <p>
            <strong>{{ berth.name }}</strong>
            max draft {{ berth.maxDraftMeters }}m -
            {{ berth.isAvailable ? 'Available' : 'Occupied' }}
          </p>
        }
      </div>
    </section>
  `
})
export class BerthsComponent {
  berths$ = this.berths.getBerths();

  constructor(private readonly berths: BerthService) {}
}
