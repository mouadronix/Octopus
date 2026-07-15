import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ShipService } from '../../services/ship.service';
import { SystemService } from '../../services/system.service';
import { Ship, ShipStatus } from '../../models/ship.model';

type StatusFilter = 'All' | 'Pending' | 'Assigned' | 'Departed';

@Component({
  selector: 'app-operator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operator.component.html',
  styleUrl: './operator.component.scss'
})
export class OperatorComponent implements OnInit {
  ships: Ship[] = [];
  currentDay = 1;
  isLoading = true;
  errorMessage = '';
  message = '';
  messageType: 'success' | 'error' = 'success';

  // Create ship form
  newShipName = '';
  newShipNotes = '';

  // Edit ship
  editingShipId: number | null = null;
  editName = '';
  editNotes = '';

  // Filters
  searchTerm = '';
  statusFilter: StatusFilter = 'All';

  readonly statusOptions: StatusFilter[] = ['All', 'Pending', 'Assigned', 'Departed'];

  constructor(
    private readonly shipService: ShipService,
    private readonly systemService: SystemService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  get filteredShips(): Ship[] {
    const term = this.searchTerm.trim().toLowerCase();
    return this.ships.filter((ship) => {
      const matchesSearch = !term || ship.name.toLowerCase().includes(term);
      const matchesStatus = this.statusFilter === 'All' || this.normalizeStatus(ship.status) === this.statusFilter;
      return matchesSearch && matchesStatus;
    });
  }

  get pendingCount(): number {
    return this.ships.filter((s) => this.normalizeStatus(s.status) === 'Pending').length;
  }

  get assignedCount(): number {
    return this.ships.filter((s) => this.normalizeStatus(s.status) === 'Assigned').length;
  }

  get departedCount(): number {
    return this.ships.filter((s) => this.normalizeStatus(s.status) === 'Departed').length;
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      ships: this.shipService.getShips(),
      state: this.systemService.getState()
    }).subscribe({
      next: ({ ships, state }) => {
        this.ships = ships;
        this.currentDay = state.currentDay;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Backend data is not available. Start the API on port 5000.';
        this.isLoading = false;
      }
    });
  }

  createShip(): void {
    const name = this.newShipName.trim();
    const notes = this.newShipNotes.trim();

    if (!name) {
      this.showMessage('Ship name is required.', 'error');
      return;
    }

    this.shipService.createShip({ name, notes, size: 'M', arrivalDay: this.currentDay, duration: 3 }).subscribe({
      next: () => {
        this.showMessage('Ship "' + name + '" created successfully.', 'success');
        this.newShipName = '';
        this.newShipNotes = '';
        this.loadData();
      },
      error: () => {
        this.showMessage('Failed to create ship.', 'error');
      }
    });
  }

  startEdit(ship: Ship): void {
    if (this.normalizeStatus(ship.status) !== 'Pending') {
      return;
    }
    this.editingShipId = ship.id;
    this.editName = ship.name;
    this.editNotes = ship.notes || '';
  }

  cancelEdit(): void {
    this.editingShipId = null;
    this.editName = '';
    this.editNotes = '';
  }

  saveEdit(ship: Ship): void {
    const name = this.editName.trim();
    if (!name) {
      this.showMessage('Ship name is required.', 'error');
      return;
    }

    this.shipService.updateShip(ship.id, { name, notes: this.editNotes.trim() }).subscribe({
      next: () => {
        this.showMessage('Ship "' + name + '" updated.', 'success');
        this.cancelEdit();
        this.loadData();
      },
      error: () => {
        this.showMessage('Failed to update ship.', 'error');
      }
    });
  }

  normalizeStatus(status: ShipStatus): 'Pending' | 'Assigned' | 'Departed' {
    if (status === 1 || status === 'Assigned') return 'Assigned';
    if (status === 2 || status === 'Departed') return 'Departed';
    return 'Pending';
  }

  trackByShip(_index: number, ship: Ship): number {
    return ship.id;
  }

  private showMessage(msg: string, type: 'success' | 'error'): void {
    this.message = msg;
    this.messageType = type;
  }
}
