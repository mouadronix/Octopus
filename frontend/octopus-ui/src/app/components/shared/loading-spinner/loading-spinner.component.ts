import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="loading-spinner" [class.overlay]="overlay">
      <div class="spinner"></div>
      <p *ngIf="message" class="loading-message">{{ message }}</p>
    </div>
  `,
  styles: [`
    .loading-spinner {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      gap: 0.75rem;
    }
    .loading-spinner.overlay {
      position: absolute;
      inset: 0;
      background: rgba(10, 14, 26, 0.7);
      z-index: 10;
    }
    .spinner {
      width: 32px;
      height: 32px;
      border: 3px solid rgba(41, 183, 255, 0.2);
      border-top-color: #29b7ff;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
    .loading-message {
      color: #8da0b8;
      font-size: 0.875rem;
      margin: 0;
    }
  `]
})
export class LoadingSpinnerComponent {
  @Input() message = 'Loading...';
  @Input() overlay = false;
}
