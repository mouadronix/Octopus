import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Ship, ShipSize, ShipStatus } from '../../models/ship.model';
import { ShipService } from '../../services/ship.service';

type StatusFilter = 'All' | 'Pending' | 'Assigned' | 'Departed';
type SizeFilter = 'All' | 'XL' | 'L' | 'M' | 'S';

interface ShipMetric {
  label: string;
  value: number;
  tone: 'cyan' | 'orange' | 'green' | 'muted';
}

@Component({
  selector: 'app-ships',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './ships.component.html',
  styleUrl: './ships.component.scss'
})
export class ShipsComponent implements OnInit {
  ships: Ship[] = [];
  searchTerm = '';
  statusFilter: StatusFilter = 'All';
  sizeFilter: SizeFilter = 'All';
  isLoading = true;
  errorMessage = '';
  successMessage = '';
  selectedShip: Ship | null = null;

  readonly statusOptions: StatusFilter[] = ['All', 'Pending', 'Assigned', 'Departed'];
  readonly sizeOptions: SizeFilter[] = ['All', 'XL', 'L', 'M', 'S'];
  readonly defaultShipImage = 'assets/default-ship.svg';

  constructor(private readonly shipService: ShipService) {}

  ngOnInit(): void {
    this.loadShips();
  }

  get metrics(): ShipMetric[] {
    return [
      { label: 'Total Ships', value: this.ships.length, tone: 'cyan' },
      { label: 'Pending', value: this.countByStatus('Pending'), tone: 'orange' },
      { label: 'Assigned', value: this.countByStatus('Assigned'), tone: 'green' },
      { label: 'Departed', value: this.countByStatus('Departed'), tone: 'muted' }
    ];
  }

  get filteredShips(): Ship[] {
    const term = this.searchTerm.trim().toLowerCase();

    return this.ships.filter((ship) => {
      const matchesSearch =
        !term ||
        ship.name.toLowerCase().includes(term) ||
        this.getImo(ship).toLowerCase().includes(term) ||
        (ship.notes ?? '').toLowerCase().includes(term);
      const matchesStatus = this.statusFilter === 'All' || this.normalizeStatus(ship.status) === this.statusFilter;
      const matchesSize = this.sizeFilter === 'All' || this.normalizeSize(ship.size) === this.sizeFilter;
      return matchesSearch && matchesStatus && matchesSize;
    });
  }

  loadShips(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.shipService.getShips().subscribe({
      next: (ships) => {
        this.ships = ships;
        this.selectedShip = ships[0] ?? null;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load ships. Start the backend API and try again.';
        this.isLoading = false;
      }
    });
  }

  selectShip(ship: Ship): void {
    this.selectedShip = ship;
  }

  deleteShip(ship: Ship): void {
    this.shipService.deleteShip(ship.id).subscribe({
      next: () => {
        this.successMessage = `${ship.name} deleted successfully.`;
        this.loadShips();
      },
      error: () => {
        this.errorMessage = `Unable to delete ${ship.name}.`;
      }
    });
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.statusFilter = 'All';
    this.sizeFilter = 'All';
  }

  getImo(ship: Ship): string {
    const match = ship.notes?.match(/IMO:\s*([A-Za-z0-9-]+)/i);
    return match?.[1] ?? `NF-${String(ship.id).padStart(4, '0')}`;
  }

  getShipImage(ship: Ship): string {
    return ship.imageUrl?.trim() || this.defaultShipImage;
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

  trackByMetric(_index: number, metric: ShipMetric): string {
    return metric.label;
  }

  trackByOption(_index: number, option: string): string {
    return option;
  }

  private countByStatus(status: StatusFilter): number {
    return this.ships.filter((ship) => this.normalizeStatus(ship.status) === status).length;
  }
}
