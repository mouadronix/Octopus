import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton-rows',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skeleton-rows" aria-hidden="true">
      <div *ngFor="let row of rows; let i = index" class="skeleton-row" [style.animation-delay.ms]="i * 80">
        <span *ngFor="let col of columns" class="skeleton-cell" [style.width]="col"></span>
      </div>
    </div>
  `,
  styles: [`
    .skeleton-rows {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .skeleton-row {
      display: flex;
      gap: 1rem;
      padding: 0.75rem 1rem;
      border-radius: 6px;
      background: rgba(255, 255, 255, 0.03);
      animation: pulse 1.5s ease-in-out infinite;
    }
    .skeleton-cell {
      height: 14px;
      border-radius: 4px;
      background: rgba(255, 255, 255, 0.06);
    }
    @keyframes pulse {
      0%, 100% { opacity: 0.6; }
      50% { opacity: 1; }
    }
  `]
})
export class SkeletonRowsComponent {
  @Input() count = 5;
  @Input() columns: string[] = ['20%', '15%', '15%', '12%', '12%', '12%', '14%'];

  get rows(): number[] {
    return Array.from({ length: this.count }, (_, i) => i);
  }
}
