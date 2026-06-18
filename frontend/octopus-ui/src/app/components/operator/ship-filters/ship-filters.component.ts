import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-ship-filters',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ship-filters.component.html',
  styleUrl: './ship-filters.component.scss'
})
export class ShipFiltersComponent {
  filters = ['Tutte', 'Pending', 'Assigned', 'Departed'];
  activeFilter = 'Tutte';

  trackByFilter(_index: number, filter: string): string {
    return filter;
  }
}
