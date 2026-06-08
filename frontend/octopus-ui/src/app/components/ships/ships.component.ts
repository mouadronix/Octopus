import { Component } from '@angular/core';
import { PagePlaceholderComponent } from '../shared/page-placeholder/page-placeholder.component';

@Component({
  selector: 'app-ships',
  standalone: true,
  imports: [PagePlaceholderComponent],
  template: `
    <app-page-placeholder
      pageName="Ships"
      sectionLabel="Ship registry"
      description="This page will display all ships, their size, arrival day, duration, and status."
      nextStep="Next implementation: connect the ship list to GET /api/ships."
    />
  `
})
export class ShipsComponent {}
