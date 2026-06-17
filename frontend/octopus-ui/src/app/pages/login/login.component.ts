import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface FeatureCard {
  title: string;
  description: string;
  icon: 'ship' | 'crane' | 'calendar' | 'chart';
}

type AuthMode = 'login' | 'register';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  mode: AuthMode = 'login';
  fullName = '';
  username = '';
  password = '';
  confirmPassword = '';
  rememberMe = false;
  showPassword = false;
  showConfirmPassword = false;
  formMessage = '';
  formMessageType: 'error' | 'success' = 'error';
  isSubmitting = false;

  features: FeatureCard[] = [
    {
      title: 'Ship Management',
      description: 'Register and track ships with real-time status and schedules.',
      icon: 'ship'
    },
    {
      title: 'Berth Allocation',
      description: 'Smart allocation ensuring optimal berth utilization.',
      icon: 'crane'
    },
    {
      title: 'Scheduling',
      description: 'Plan, assign and manage ship schedules efficiently.',
      icon: 'calendar'
    },
    {
      title: 'Dashboards',
      description: 'Real-time insights and operational analytics.',
      icon: 'chart'
    }
  ];

  constructor(
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly authService: AuthService
  ) {
    if (this.authService.isAuthenticated()) {
      void this.router.navigateByUrl(this.returnUrl);
    }
  }

  get isRegisterMode(): boolean {
    return this.mode === 'register';
  }

  switchMode(mode: AuthMode): void {
    this.mode = mode;
    this.formMessage = '';
    this.password = '';
    this.confirmPassword = '';
    this.showPassword = false;
    this.showConfirmPassword = false;
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  signIn(): void {
    this.formMessage = '';

    if (this.isRegisterMode) {
      this.register();
      return;
    }

    const username = this.username.trim();
    const password = this.password;

    if (!username || !password) {
      this.setError('Enter your username and password.');
      return;
    }

    this.isSubmitting = true;
    this.authService.login(username, password).subscribe({
      next: (user) => {
        this.authService.saveSession(user, this.rememberMe);
        void this.router.navigateByUrl(this.returnUrl);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.setError(error?.error?.message || 'Invalid username or password.');
      }
    });
  }

  signInAsGuest(): void {
    this.isSubmitting = true;
    this.authService.login('guest', 'guest').subscribe({
      next: (user) => {
        this.authService.saveSession(user, false);
        void this.router.navigateByUrl(this.returnUrl);
      },
      error: () => {
        this.isSubmitting = false;
        this.setError('Guest access is not available from the backend.');
      }
    });
  }

  trackByFeature(_index: number, feature: FeatureCard): string {
    return feature.title;
  }

  private register(): void {
    const fullName = this.fullName.trim();
    const username = this.username.trim();
    const password = this.password;
    const confirmPassword = this.confirmPassword;

    if (!fullName || !username || !password || !confirmPassword) {
      this.setError('Complete all fields to create an account.');
      return;
    }

    if (username.length < 3) {
      this.setError('Username must be at least 3 characters.');
      return;
    }

    if (password.length < 6) {
      this.setError('Password must be at least 6 characters.');
      return;
    }

    if (password !== confirmPassword) {
      this.setError('Passwords do not match.');
      return;
    }

    this.isSubmitting = true;
    this.authService.register({ fullName, username, password }).subscribe({
      next: (user) => {
        this.authService.saveSession(user, this.rememberMe);
        this.setSuccess('Account created. Opening the operator dashboard.');
        void this.router.navigateByUrl(this.returnUrl);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.setError(error?.error?.message || 'Account could not be created.');
      }
    });
  }

  private get returnUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return returnUrl && returnUrl.startsWith('/') ? returnUrl : '/operator';
  }

  private setError(message: string): void {
    this.formMessageType = 'error';
    this.formMessage = message;
  }

  private setSuccess(message: string): void {
    this.formMessageType = 'success';
    this.formMessage = message;
  }
}
