import { Component } from '@angular/core';
import { PagePlaceholderComponent } from '../shared/page-placeholder/page-placeholder.component';

@Component({
  selector: 'app-scheduler',
  standalone: true,
  imports: [PagePlaceholderComponent],
  template: `
    <app-page-placeholder
      pageName="Scheduler"
      sectionLabel="Scheduler area"
      description="This page will help schedulers assign pending ships to compatible berths."
      nextStep="Next implementation: show pending ships and create assignments through POST /api/assignments."
    />
  `
})
export class SchedulerComponent {}
