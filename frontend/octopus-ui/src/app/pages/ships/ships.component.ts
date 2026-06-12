import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { ShipService } from '../../services/ship.service';

type ShipSize = 'XL' | 'L' | 'M' | 'S';
type FeedbackType = 'info' | 'success' | 'warning';

interface Step {
  number: number;
  title: string;
}

interface GeneratedDetails {
  size: ShipSize;
  arrivalDay: number;
  duration: number;
}

interface RuleCard {
  title: string;
  description: string;
  tone: 'purple' | 'cyan' | 'green';
}

interface SizeCategory {
  size: ShipSize;
  label: string;
  description: string;
}

@Component({
  selector: 'app-ships',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './ships.component.html',
  styleUrl: './ships.component.scss'
})
export class ShipsComponent {
  currentStep = 1;
  currentDay = 12;
  shipName = '';
  imoNumber = '';
  notes = '';
  isSaving = false;
  feedbackMessage = '';
  feedbackType: FeedbackType = 'info';

  steps: Step[] = [
    { number: 1, title: 'Basic Information' },
    { number: 2, title: 'Ship Details' },
    { number: 3, title: 'Confirmation' }
  ];

  rules: RuleCard[] = [
    { title: 'Ship Size', description: 'Determined from ship name hash and system rules.', tone: 'purple' },
    { title: 'Arrival Day', description: 'Calculated from current day and simulation rules.', tone: 'cyan' },
    { title: 'Occupation Duration', description: 'Generated from ship size and deterministic randomization.', tone: 'green' }
  ];

  sizeCategories: SizeCategory[] = [
    { size: 'XL', label: 'Extra Large', description: 'For the largest vessels' },
    { size: 'L', label: 'Large', description: 'For large vessels' },
    { size: 'M', label: 'Medium', description: 'For medium vessels' },
    { size: 'S', label: 'Small', description: 'For small vessels' }
  ];

  constructor(
    private readonly shipService: ShipService,
    private readonly router: Router
  ) {}

  get generatedDetails(): GeneratedDetails {
    const hash = this.hashName(this.shipName || 'OCTOPUS');
    const sizes: ShipSize[] = ['XL', 'L', 'M', 'S'];
    const size = sizes[hash % sizes.length];
    const baseDuration: Record<ShipSize, number> = { XL: 8, L: 6, M: 4, S: 2 };

    return {
      size,
      arrivalDay: this.currentDay + 1 + (hash % 5),
      duration: baseDuration[size] + (hash % 3)
    };
  }

  get notesLength(): number {
    return this.notes.length;
  }

  get canGoNext(): boolean {
    return this.shipName.trim().length >= 2;
  }

  get cleanShipName(): string {
    return this.shipName.trim() || 'Not provided';
  }

  get carouselTransform(): string {
    return `translateX(-${(this.currentStep - 1) * 100}%)`;
  }

  get progressWidth(): string {
    return `${((this.currentStep - 1) / (this.steps.length - 1)) * 100}%`;
  }

  setStep(step: number): void {
    if (!this.canAccessStep(step)) {
      this.setFeedback('Insert a ship name before continuing.', 'warning');
      return;
    }

    this.currentStep = step;
    this.feedbackMessage = '';
  }

  nextStep(): void {
    if (!this.canGoNext) {
      this.setFeedback('Insert a ship name before continuing.', 'warning');
      return;
    }

    this.currentStep = Math.min(this.currentStep + 1, 3);
    this.feedbackMessage = '';
  }

  previousStep(): void {
    this.currentStep = Math.max(this.currentStep - 1, 1);
    this.feedbackMessage = '';
  }

  createShip(): void {
    if (!this.canGoNext) {
      this.currentStep = 1;
      this.setFeedback('Ship name is required.', 'warning');
      return;
    }

    this.isSaving = true;
    this.feedbackMessage = '';

    this.shipService
      .createShip({ name: this.shipName.trim(), notes: this.notes.trim() })
      .pipe(
        catchError(() => of(null)),
        finalize(() => (this.isSaving = false))
      )
      .subscribe((ship) => {
        if (ship) {
          void this.router.navigate(['/operator']);
          return;
        }

        this.saveLocalDraft();
        this.setFeedback('Backend create endpoint is not ready yet. Ship saved locally as a frontend draft.', 'warning');
      });
  }

  resetForm(): void {
    this.currentStep = 1;
    this.shipName = '';
    this.imoNumber = '';
    this.notes = '';
    this.feedbackMessage = '';
  }

  canAccessStep(step: number): boolean {
    return step === 1 || this.canGoNext;
  }

  isCompleted(step: number): boolean {
    return step < this.currentStep && this.canAccessStep(step);
  }

  trackByStepNumber(_index: number, step: Step): number {
    return step.number;
  }

  trackByRuleTitle(_index: number, rule: RuleCard): string {
    return rule.title;
  }

  trackBySize(_index: number, category: SizeCategory): ShipSize {
    return category.size;
  }

  private setFeedback(message: string, type: FeedbackType): void {
    this.feedbackMessage = message;
    this.feedbackType = type;
  }

  private saveLocalDraft(): void {
    const details = this.generatedDetails;
    const key = 'octopus.localShips';
    const current = JSON.parse(localStorage.getItem(key) || '[]');
    localStorage.setItem(
      key,
      JSON.stringify([
        {
          id: Date.now(),
          name: this.shipName.trim(),
          imoNumber: this.imoNumber.trim(),
          notes: this.notes.trim(),
          status: 'Pending',
          ...details,
          createdAt: new Date().toISOString()
        },
        ...current
      ])
    );
  }

  private hashName(value: string): number {
    return value
      .trim()
      .toUpperCase()
      .split('')
      .reduce((hash, char) => hash + char.charCodeAt(0), 0);
  }
}
