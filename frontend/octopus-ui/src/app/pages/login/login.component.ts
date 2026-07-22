import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, UserRole } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  constructor(
    private readonly router: Router,
    private readonly authService: AuthService
  ) {
    if (this.authService.isAuthenticated()) {
      const role = this.authService.currentRole;
      void this.router.navigateByUrl(role === 'scheduler' ? '/scheduler' : '/operator');
    }
  }

  selectRole(role: UserRole): void {
    this.authService.setRole(role);
    void this.router.navigateByUrl(role === 'scheduler' ? '/scheduler' : '/operator');
  }
}
