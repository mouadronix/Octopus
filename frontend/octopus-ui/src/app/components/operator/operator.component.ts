import { Component, inject } from '@angular/core';
import { AsyncPipe, DatePipe } from '@angular/common';
import { SystemService } from '../../services/system.service';

@Component({
  selector: 'app-operator',
  standalone: true,
  imports: [AsyncPipe, DatePipe],
  template: `
    <section class="page">
      <h2>Operator Console</h2>
      @if (state$ | async; as state) {
        <div class="panel">
          <p>Environment: {{ state.environment }}</p>
          <p>Server time: {{ state.serverTimeUtc | date:'medium' }}</p>
          <p>Ships: {{ state.shipCount }}</p>
          <p>Berths: {{ state.berthCount }}</p>
          <p>Active assignments: {{ state.activeAssignmentCount }}</p>
        </div>
      }
    </section>
  `
})
export class OperatorComponent {
  private readonly system = inject(SystemService);
  state$ = this.system.getState();
}
