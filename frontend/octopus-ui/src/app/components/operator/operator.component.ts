import { Component } from '@angular/core';
import { PagePlaceholderComponent } from '../shared/page-placeholder/page-placeholder.component';

@Component({
  selector: 'app-operator',
  standalone: true,
  imports: [PagePlaceholderComponent],
  template: `
    <app-page-placeholder
      pageName="Operator"
      sectionLabel="Operator area"
      description="This page will let operators register ships and review terminal state."
      nextStep="Next implementation: create the ship form and connect it to POST /api/ships."
    />
  `
})
export class OperatorComponent {}
