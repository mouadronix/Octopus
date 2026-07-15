import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-error-banner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="error-banner" *ngIf="message" role="alert">
      <span class="error-icon" aria-hidden="true">!</span>
      <p>{{ message }}</p>
      <button *ngIf="dismissible" type="button" class="dismiss-btn" aria-label="Dismiss" (click)="dismiss.emit()">x</button>
    </div>
  `,
  styles: [`
    .error-banner {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem 1rem;
      background: rgba(255, 59, 48, 0.12);
      border: 1px solid rgba(255, 59, 48, 0.3);
      border-radius: 8px;
      color: #ff6b6b;
      font-size: 0.875rem;
    }
    .error-icon {
      flex-shrink: 0;
      width: 20px;
      height: 20px;
      border-radius: 50%;
      background: rgba(255, 59, 48, 0.2);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 0.75rem;
    }
    .error-banner p {
      flex: 1;
      margin: 0;
    }
    .dismiss-btn {
      flex-shrink: 0;
      background: none;
      border: none;
      color: #ff6b6b;
      cursor: pointer;
      font-size: 1.1rem;
      padding: 0 0.25rem;
      opacity: 0.7;
    }
    .dismiss-btn:hover {
      opacity: 1;
    }
  `]
})
export class ErrorBannerComponent {
  @Input() message = '';
  @Input() dismissible = false;
  @Output() dismiss = new EventEmitter<void>();
}
