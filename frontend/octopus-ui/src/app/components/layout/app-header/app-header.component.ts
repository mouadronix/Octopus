import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService, AuthSession } from '../../../services/auth.service';
import { SystemService } from '../../../services/system.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss'
})
export class AppHeaderComponent {
  currentDay = 12;
  readonly currentSession$: Observable<AuthSession | null>;

  constructor(
    private readonly systemService: SystemService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.currentSession$ = this.authService.currentSession$;

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

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
