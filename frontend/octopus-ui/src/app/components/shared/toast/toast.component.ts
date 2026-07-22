import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast" [class]="type" *ngIf="message" role="status">
      <span class="toast-icon" aria-hidden="true">{{ type === 'success' ? '&#10003;' : '!' }}</span>
      <p>{{ message }}</p>
    </div>
  `,
  styles: [`
    .toast {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.625rem 1rem;
      border-radius: 8px;
      font-size: 0.875rem;
      animation: slideIn 0.2s ease-out;
    }
    .toast.success {
      background: rgba(40, 217, 130, 0.12);
      border: 1px solid rgba(40, 217, 130, 0.3);
      color: #28d982;
    }
    .toast.error {
      background: rgba(255, 59, 48, 0.12);
      border: 1px solid rgba(255, 59, 48, 0.3);
      color: #ff6b6b;
    }
    .toast-icon {
      flex-shrink: 0;
      font-weight: 700;
    }
    .toast p {
      flex: 1;
      margin: 0;
    }
    @keyframes slideIn {
      from { opacity: 0; transform: translateY(-8px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class ToastComponent {
  @Input() message = '';
  @Input() type: 'success' | 'error' = 'success';
}
