import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-berths',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './berths.component.html',
  styleUrl: './berths.component.scss'
})
export class BerthsComponent {
  berths = [
    { name: 'Banchina XL', size: 'XL' },
    { name: 'Banchina L', size: 'L' },
    { name: 'Banchina M1', size: 'M' },
    { name: 'Banchina M2', size: 'M' },
    { name: 'Banchina S1', size: 'S' },
    { name: 'Banchina S2', size: 'S' },
    { name: 'Banchina S3', size: 'S' },
    { name: 'Banchina S4', size: 'S' }
  ];

  trackByBerthName(_index: number, berth: { name: string }): string {
    return berth.name;
  }
}
