import { Component } from '@angular/core';
import { PagePlaceholderComponent } from '../shared/page-placeholder/page-placeholder.component';

@Component({
  selector: 'app-berths',
  standalone: true,
  imports: [PagePlaceholderComponent],
  template: `
    <app-page-placeholder
      pageName="Berths"
      sectionLabel="Berth board"
      description="This page will show fixed BlueHarbor berths and their occupancy windows."
      nextStep="Next implementation: connect the berth board to GET /api/berths."
    />
  `
})
export class BerthsComponent {}
