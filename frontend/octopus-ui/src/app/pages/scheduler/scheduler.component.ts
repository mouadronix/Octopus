import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Berth } from '../../models/berth.model';
import { CompatibleBerth, Ship, ShipSize, ShipStatus, SuggestionResponse } from '../../models/ship.model';
import { AssignmentService } from '../../services/assignment.service';
import { BerthService } from '../../services/berth.service';
import { ShipService } from '../../services/ship.service';
import { SystemService } from '../../services/system.service';

type SizeFilter = 'All' | 'XL' | 'L' | 'M' | 'S';
type DayFilter = 'All' | 'Today' | 'Next7' | 'Future';
type SortOption = 'ArrivalAsc' | 'ArrivalDesc' | 'SizeDesc' | 'NameAsc';

interface AssignmentMetric {
  label: string;
  value: number;
  caption: string;
  tone: 'orange' | 'purple' | 'blue' | 'green';
  trend: number[];
}

@Component({
  selector: 'app-scheduler',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './scheduler.component.html',
  styleUrl: './scheduler.component.scss'
})
export class SchedulerComponent implements OnInit {
  ships: Ship[] = [];
  berths: Berth[] = [];
  currentDay = 1;
  selectedShip: Ship | null = null;
  selectedBerthId = 0;
  searchTerm = '';
  sizeFilter: SizeFilter = 'All';
  dayFilter: DayFilter = 'All';
  sortOption: SortOption = 'ArrivalAsc';
  isLoading = true;
  isAssigning = false;
  isLoadingSuggestion = false;
  message = '';
  messageType: 'success' | 'error' = 'success';

  // Compatible berths from the API for the selected ship
  compatibleBerths: CompatibleBerth[] = [];

  readonly sizeOptions: SizeFilter[] = ['All', 'XL', 'L', 'M', 'S'];
  readonly dayOptions: Array<{ label: string; value: DayFilter }> = [
    { label: 'All', value: 'All' },
    { label: 'Today', value: 'Today' },
    { label: 'Next 7 Days', value: 'Next7' },
    { label: 'Future', value: 'Future' }
  ];
  readonly sortOptions: Array<{ label: string; value: SortOption }> = [
    { label: 'Arrival Day (Asc)', value: 'ArrivalAsc' },
    { label: 'Arrival Day (Desc)', value: 'ArrivalDesc' },
    { label: 'Largest Ship First', value: 'SizeDesc' },
    { label: 'Ship Name (A-Z)', value: 'NameAsc' }
  ];

  constructor(
    private readonly shipService: ShipService,
    private readonly berthService: BerthService,
    private readonly assignmentService: AssignmentService,
    private readonly systemService: SystemService
  ) {}

  ngOnInit(): void {
    this.loadAssignments();
  }

  get pendingShips(): Ship[] {
    return this.ships.filter((ship) => this.normalizeStatus(ship.status) === 'Pending');
  }

  get filteredShips(): Ship[] {
    const term = this.searchTerm.trim().toLowerCase();

    return this.pendingShips
      .filter((ship) => {
        const matchesSearch = !term || ship.name.toLowerCase().includes(term) || this.getImo(ship).toLowerCase().includes(term);
        const matchesSize = this.sizeFilter === 'All' || this.normalizeSize(ship.size) === this.sizeFilter;
        const matchesDay =
          this.dayFilter === 'All' ||
          (this.dayFilter === 'Today' && ship.arrivalDay <= this.currentDay) ||
          (this.dayFilter === 'Next7' && ship.arrivalDay >= this.currentDay && ship.arrivalDay <= this.currentDay + 7) ||
          (this.dayFilter === 'Future' && ship.arrivalDay > this.currentDay + 7);

        return matchesSearch && matchesSize && matchesDay;
      })
      .sort((left, right) => this.compareShips(left, right));
  }

  get selectedBerth(): CompatibleBerth | null {
    return this.compatibleBerths.find((berth) => berth.dockId === this.selectedBerthId) ?? null;
  }

  get metrics(): AssignmentMetric[] {
    const xlPending = this.pendingShips.filter((ship) => this.normalizeSize(ship.size) === 'XL').length;
    const arrivalsNext7 = this.ships.filter((ship) => ship.arrivalDay >= this.currentDay && ship.arrivalDay <= this.currentDay + 7).length;
    const availableBerths = this.berths.filter((berth) => (berth.assignments?.length ?? 0) === 0).length;

    return [
      { label: 'Pending Ships', value: this.pendingShips.length, caption: 'Awaiting assignment', tone: 'orange', trend: [3, 2, 4, 2, 5, 3, 4] },
      { label: 'XL Pending', value: xlPending, caption: 'Extra large vessels', tone: 'purple', trend: [2, 1, 3, 2, 1, 2, 2] },
      { label: 'Total Arrivals', value: arrivalsNext7, caption: 'Next 7 days', tone: 'blue', trend: [1, 2, 1, 3, 2, 4, 2] },
      { label: 'Available Berths', value: availableBerths, caption: 'Ready for allocation', tone: 'green', trend: [4, 5, 3, 4, 2, 3, 4] }
    ];
  }

  loadAssignments(): void {
    this.isLoading = true;
    this.message = '';

    forkJoin({
      ships: this.shipService.getShips(),
      berths: this.berthService.getBerths(),
      state: this.systemService.getState()
    }).subscribe({
      next: ({ ships, berths, state }) => {
        this.ships = ships;
        this.berths = berths;
        this.currentDay = state.currentDay;
        this.selectedShip = this.filteredShips[0] ?? null;
        if (this.selectedShip) {
          this.loadSuggestion(this.selectedShip.id);
        }
        this.isLoading = false;
      },
      error: () => {
        this.showMessage('Pending assignments data is not available from the backend.', 'error');
        this.isLoading = false;
      }
    });
  }

  selectShip(ship: Ship): void {
    this.selectedShip = ship;
    this.message = '';
    this.loadSuggestion(ship.id);
  }

  selectBerth(dockId: number): void {
    this.selectedBerthId = dockId;
    this.message = '';
  }

  applyFilters(): void {
    if (!this.selectedShip || !this.filteredShips.some((ship) => ship.id === this.selectedShip?.id)) {
      this.selectedShip = this.filteredShips[0] ?? null;
      if (this.selectedShip) {
        this.loadSuggestion(this.selectedShip.id);
      } else {
        this.compatibleBerths = [];
      }
    }
  }

  confirmAssignment(): void {
    if (!this.selectedShip || !this.selectedBerth) {
      this.showMessage('Select a ship and compatible berth before confirming.', 'error');
      return;
    }

    this.isAssigning = true;
    this.assignmentService.createAssignment({ shipId: this.selectedShip.id, dockId: this.selectedBerth.dockId }).subscribe({
      next: () => {
        this.showMessage(`${this.selectedShip?.name} assigned to ${this.selectedBerth?.dockName}.`, 'success');
        this.isAssigning = false;
        this.loadAssignments();
      },
      error: () => {
        this.showMessage('Assignment failed. The berth may be occupied or incompatible.', 'error');
        this.isAssigning = false;
      }
    });
  }

  /**
   * Count compatible berths by size only (for the ship list cards).
   * This is a lightweight client-side check — no conflict detection.
   */
  getCompatibleBerthCount(ship: Ship): number {
    return this.berths.filter((berth) => this.sizeRank(this.normalizeSize(berth.size)) >= this.sizeRank(this.normalizeSize(ship.size))).length;
  }

  getCompatibleBerthNames(ship: Ship): string {
    return this.berths
      .filter((berth) => this.sizeRank(this.normalizeSize(berth.size)) >= this.sizeRank(this.normalizeSize(ship.size)))
      .slice(0, 4)
      .map((berth) => berth.name)
      .join(', ');
  }

  getImo(ship: Ship): string {
    const match = ship.notes?.match(/IMO:\s*([A-Za-z0-9-]+)/i);
    return match?.[1] ?? ship.notes ?? '-';
  }

  getDaysFromNow(ship: Ship): string {
    const delta = ship.arrivalDay - this.currentDay;
    if (delta <= 0) return 'Ready now';
    if (delta === 1) return '1 day from now';
    return `${delta} days from now`;
  }

  getEndDay(ship: Ship): number {
    return Math.max(ship.arrivalDay, this.currentDay) + ship.duration - 1;
  }

  getMetricPath(points: number[]): string {
    const max = Math.max(...points, 1);
    return points
      .map((point, index) => {
        const x = index * (100 / Math.max(points.length - 1, 1));
        const y = 32 - (point / max) * 24;
        return `${index === 0 ? 'M' : 'L'} ${x.toFixed(1)} ${y.toFixed(1)}`;
      })
      .join(' ');
  }

  normalizeStatus(status: ShipStatus): 'Pending' | 'Assigned' | 'Departed' {
    if (status === 1 || status === 'Assigned') return 'Assigned';
    if (status === 2 || status === 'Departed') return 'Departed';
    return 'Pending';
  }

  normalizeSize(size: ShipSize): 'XL' | 'L' | 'M' | 'S' {
    if (size === 0 || size === 'XL') return 'XL';
    if (size === 1 || size === 'L') return 'L';
    if (size === 2 || size === 'M') return 'M';
    return 'S';
  }

  trackByShip(_index: number, ship: Ship): number {
    return ship.id;
  }

  trackByMetric(_index: number, metric: AssignmentMetric): string {
    return metric.label;
  }

  trackByBerth(_index: number, berth: CompatibleBerth): number {
    return berth.dockId;
  }

  /**
   * Load scheduling suggestion from the API for the selected ship.
   * The API returns the best suggestion + all compatible berths with availability.
   */
  private loadSuggestion(shipId: number): void {
    this.isLoadingSuggestion = true;
    this.compatibleBerths = [];
    this.selectedBerthId = 0;

    this.shipService.getSuggestion(shipId).subscribe({
      next: (suggestion: SuggestionResponse) => {
        this.compatibleBerths = suggestion.compatibleBerths;
        this.selectedBerthId = this.compatibleBerths[0]?.dockId ?? 0;
        this.isLoadingSuggestion = false;
      },
      error: () => {
        this.compatibleBerths = [];
        this.isLoadingSuggestion = false;
      }
    });
  }

  private compareShips(left: Ship, right: Ship): number {
    if (this.sortOption === 'ArrivalDesc') return right.arrivalDay - left.arrivalDay;
    if (this.sortOption === 'SizeDesc') return this.sizeRank(this.normalizeSize(right.size)) - this.sizeRank(this.normalizeSize(left.size));
    if (this.sortOption === 'NameAsc') return left.name.localeCompare(right.name);
    return left.arrivalDay - right.arrivalDay || left.name.localeCompare(right.name);
  }

  private sizeRank(size: 'XL' | 'L' | 'M' | 'S'): number {
    return { S: 1, M: 2, L: 3, XL: 4 }[size];
  }

  private showMessage(message: string, type: 'success' | 'error'): void {
    this.message = message;
    this.messageType = type;
  }
}
