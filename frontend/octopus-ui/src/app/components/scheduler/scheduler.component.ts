import { Component } from '@angular/core';
import { AsyncPipe, DatePipe } from '@angular/common';
import { AssignmentService } from '../../services/assignment.service';

@Component({
  selector: 'app-scheduler',
  standalone: true,
  imports: [AsyncPipe, DatePipe],
  template: `
    <section class="page">
      <h2>Scheduler</h2>
      <div class="panel">
        @for (assignment of assignments$ | async; track assignment.id) {
          <p>
            Ship {{ assignment.shipId }} assigned to berth {{ assignment.berthId }}
            from {{ assignment.startsAt | date:'short' }} - {{ assignment.status }}
          </p>
        }
      </div>
    </section>
  `
})
export class SchedulerComponent {
  assignments$ = this.assignments.getAssignments();

  constructor(private readonly assignments: AssignmentService) {}
}
