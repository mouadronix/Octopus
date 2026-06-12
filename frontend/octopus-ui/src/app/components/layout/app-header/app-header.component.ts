import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { SystemService } from '../../../services/system.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss'
})
export class AppHeaderComponent {
  currentDay = 12;

  constructor(private readonly systemService: SystemService) {
    this.systemService.getState().subscribe({
      next: (state) => (this.currentDay = state.currentDay),
      error: () => undefined
    });
  }

  nextDay(): void {
    this.systemService.nextDay().subscribe({
      next: (state) => (this.currentDay = state.currentDay),
      error: () => (this.currentDay += 1)
    });
  }
}
