import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Ship, ShipSize, ShipStatus } from '../../models/ship.model';
import { ShipService } from '../../services/ship.service';

type StatusFilter = 'All' | 'Pending' | 'Assigned' | 'Departed';
type SizeFilter = 'All' | 'XL' | 'L' | 'M' | 'S';

interface ShipMetric {
  label: string;
  value: number;
  caption: string;
  tone: 'blue' | 'orange' | 'green' | 'muted';
}

interface ShipForm {
  name: string;
  notes: string;
  size: 'XL' | 'L' | 'M' | 'S';
  arrivalDay: number;
  duration: number;
}

@Component({
  selector: 'app-ships',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ships.component.html',
  styleUrl: './ships.component.scss'
})
export class ShipsComponent implements OnInit {
  ships: Ship[] = [];
  searchTerm = '';
  statusFilter: StatusFilter = 'All';
  sizeFilter: SizeFilter = 'All';
  arrivalFrom: number | null = null;
  arrivalTo: number | null = null;
  rowsPerPage = 10;
  currentPage = 1;
  sortDirection: 'asc' | 'desc' = 'asc';
  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';
  showNewShipModal = false;
  selectedShip: Ship | null = null;
  shipForm: ShipForm = this.createEmptyForm();

  readonly statusOptions: StatusFilter[] = ['All', 'Pending', 'Assigned', 'Departed'];
  readonly sizeOptions: SizeFilter[] = ['All', 'XL', 'L', 'M', 'S'];
  readonly rowsPerPageOptions = [8, 10, 15, 20];

  constructor(private readonly shipService: ShipService) {}

  ngOnInit(): void {
    this.loadShips();
  }

  get metrics(): ShipMetric[] {
    return [
      { label: 'Total Ships', value: this.ships.length, caption: 'All registered ships', tone: 'blue' },
      { label: 'Pending', value: this.countByStatus('Pending'), caption: 'Awaiting assignment', tone: 'orange' },
      { label: 'Assigned', value: this.countByStatus('Assigned'), caption: 'On berth', tone: 'green' },
      { label: 'Departed', value: this.countByStatus('Departed'), caption: 'Completed', tone: 'muted' }
    ];
  }

  get filteredShips(): Ship[] {
    const term = this.searchTerm.trim().toLowerCase();

    return this.ships
      .filter((ship) => {
        const imo = this.getImo(ship).toLowerCase();
        const matchesSearch = !term || ship.name.toLowerCase().includes(term) || imo.includes(term);
        const matchesStatus = this.statusFilter === 'All' || this.normalizeStatus(ship.status) === this.statusFilter;
        const matchesSize = this.sizeFilter === 'All' || this.normalizeSize(ship.size) === this.sizeFilter;
        const matchesFrom = this.arrivalFrom === null || ship.arrivalDay >= this.arrivalFrom;
        const matchesTo = this.arrivalTo === null || ship.arrivalDay <= this.arrivalTo;
        return matchesSearch && matchesStatus && matchesSize && matchesFrom && matchesTo;
      })
      .sort((left, right) => {
        const comparison = left.name.localeCompare(right.name);
        return this.sortDirection === 'asc' ? comparison : -comparison;
      });
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredShips.length / this.rowsPerPage));
  }

  get pageShips(): Ship[] {
    const start = (this.currentPage - 1) * this.rowsPerPage;
    return this.filteredShips.slice(start, start + this.rowsPerPage);
  }

  get pageStart(): number {
    return this.filteredShips.length === 0 ? 0 : (this.currentPage - 1) * this.rowsPerPage + 1;
  }

  get pageEnd(): number {
    return Math.min(this.currentPage * this.rowsPerPage, this.filteredShips.length);
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages }, (_item, index) => index + 1);
  }

  loadShips(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.shipService.getShips().subscribe({
      next: (ships) => {
        this.ships = ships;
        this.currentPage = Math.min(this.currentPage, this.totalPages);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Ships data is not available from the backend.';
        this.isLoading = false;
      }
    });
  }

  openNewShip(): void {
    this.shipForm = this.createEmptyForm();
    this.successMessage = '';
    this.errorMessage = '';
    this.showNewShipModal = true;
  }

  closeNewShip(): void {
    if (this.isSaving) {
      return;
    }

    this.showNewShipModal = false;
  }

  createShip(): void {
    const name = this.shipForm.name.trim();

    if (!name) {
      this.errorMessage = 'Ship name is required.';
      return;
    }

    if (this.shipForm.arrivalDay < 1 || this.shipForm.duration < 1) {
      this.errorMessage = 'Arrival day and duration must be positive.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    this.shipService
      .createShip({
        name,
        notes: this.shipForm.notes.trim(),
        size: this.shipForm.size,
        arrivalDay: this.shipForm.arrivalDay,
        duration: this.shipForm.duration
      })
      .subscribe({
        next: () => {
          this.isSaving = false;
          this.showNewShipModal = false;
          this.successMessage = `${name} has been registered.`;
          this.loadShips();
        },
        error: () => {
          this.isSaving = false;
          this.errorMessage = 'Ship could not be registered.';
        }
      });
  }

  viewShip(ship: Ship): void {
    this.selectedShip = ship;
  }

  closeDetails(): void {
    this.selectedShip = null;
  }

  deleteShip(ship: Ship): void {
    if (!window.confirm(`Delete ${ship.name}?`)) {
      return;
    }

    this.shipService.deleteShip(ship.id).subscribe({
      next: () => {
        this.successMessage = `${ship.name} has been deleted.`;
        this.loadShips();
      },
      error: () => {
        this.errorMessage = `${ship.name} could not be deleted.`;
      }
    });
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.statusFilter = 'All';
    this.sizeFilter = 'All';
    this.arrivalFrom = null;
    this.arrivalTo = null;
    this.currentPage = 1;
  }

  applyFilters(): void {
    this.currentPage = 1;
  }

  setPage(page: number): void {
    this.currentPage = Math.min(Math.max(page, 1), this.totalPages);
  }

  toggleSort(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
  }

  normalizeStatus(status: ShipStatus): 'Pending' | 'Assigned' | 'Departed' {
    if (status === 1 || status === 'Assigned') {
      return 'Assigned';
    }

    if (status === 2 || status === 'Departed') {
      return 'Departed';
    }

    return 'Pending';
  }

  normalizeSize(size: ShipSize): 'XL' | 'L' | 'M' | 'S' {
    if (size === 0 || size === 'XL') {
      return 'XL';
    }

    if (size === 1 || size === 'L') {
      return 'L';
    }

    if (size === 2 || size === 'M') {
      return 'M';
    }

    return 'S';
  }

  getImo(ship: Ship): string {
    const match = ship.notes?.match(/IMO:\s*([A-Za-z0-9-]+)/i);
    return match?.[1] || ship.notes || '-';
  }

  getBerth(ship: Ship): string {
    return ship.berthName || '-';
  }

  trackByShip(_index: number, ship: Ship): number {
    return ship.id;
  }

  trackByMetric(_index: number, metric: ShipMetric): string {
    return metric.label;
  }

  private countByStatus(status: 'Pending' | 'Assigned' | 'Departed'): number {
    return this.ships.filter((ship) => this.normalizeStatus(ship.status) === status).length;
  }

  private createEmptyForm(): ShipForm {
    return {
      name: '',
      notes: '',
      size: 'M',
      arrivalDay: 13,
      duration: 4
    };
  }
}
