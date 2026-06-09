import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { Berth } from '../../models/berth.model';
import { Ship } from '../../models/ship.model';
import { BerthService } from '../../services/berth.service';
import { ShipService } from '../../services/ship.service';

type ViewMode = 'Map' | 'List';
type FilterValue = 'ALL' | 'Pending' | 'Assigned' | 'In Port' | 'Departed';
type SizeFilter = 'ALL' | 'XL' | 'L' | 'M' | 'S';
type AvailabilityFilter = 'Any' | 'Available' | 'Unavailable';
type UiShipStatus = 'pending' | 'assigned' | 'in-port' | 'departed';
type UiShipSize = 'XL' | 'L' | 'M' | 'S';

interface StatusLegend {
  label: string;
  value: UiShipStatus;
  color: string;
}

interface SummaryCard {
  label: string;
  value: number;
  color: string;
}

interface PortShip {
  id: string;
  name: string;
  notes: string;
  status: UiShipStatus;
  statusLabel: string;
  size: UiShipSize;
  arrivalDay: number;
  duration: number;
  berthLabel: string;
  available: boolean;
  x: number;
  y: number;
  rotation?: number;
}

interface BerthLane {
  label: string;
  x: number;
}

interface CreateShipForm {
  name: string;
  notes: string;
}

@Component({
  selector: 'app-operator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operator.component.html',
  styleUrl: './operator.component.scss'
})
export class OperatorComponent implements OnInit {
  viewMode: ViewMode = 'Map';
  viewModes: ViewMode[] = ['Map', 'List'];

  statusOptions: FilterValue[] = ['ALL', 'Pending', 'Assigned', 'In Port', 'Departed'];
  sizeOptions: SizeFilter[] = ['ALL', 'XL', 'L', 'M', 'S'];
  availabilityOptions: AvailabilityFilter[] = ['Any', 'Available', 'Unavailable'];

  selectedStatus: FilterValue = 'ALL';
  selectedSize: SizeFilter = 'ALL';
  selectedAvailability: AvailabilityFilter = 'Any';
  searchTerm = '';
  showUnavailable = true;
  currentDay = 12;
  selectedDate = '12 Jun 2025';
  isLoading = false;
  errorMessage = '';
  isCreatePanelOpen = false;
  createForm: CreateShipForm = { name: '', notes: '' };

  legends: StatusLegend[] = [
    { label: 'Pending', value: 'pending', color: '#f5b82e' },
    { label: 'Assigned', value: 'assigned', color: '#55ad67' },
    { label: 'In Port', value: 'in-port', color: '#3478db' },
    { label: 'Departed', value: 'departed', color: '#a7b0ba' }
  ];

  berths: BerthLane[] = [
    { label: 'F', x: 27 },
    { label: 'E', x: 38 },
    { label: 'D', x: 49 },
    { label: 'C', x: 59 },
    { label: 'B', x: 68 },
    { label: 'A', x: 78 }
  ];

  ships: PortShip[] = this.createDemoShips();
  apiBerths: Berth[] = [];

  private readonly shipPositions = [
    { x: 25, y: 50 }, { x: 25, y: 56 }, { x: 25, y: 62 }, { x: 25, y: 68 },
    { x: 36, y: 49 }, { x: 36, y: 56 }, { x: 36, y: 64 }, { x: 47, y: 51 },
    { x: 58, y: 57, rotation: -90 }, { x: 66, y: 50 }, { x: 66, y: 57 },
    { x: 66, y: 64 }, { x: 66, y: 71 }, { x: 76, y: 50 }, { x: 76, y: 57 },
    { x: 76, y: 64 }, { x: 76, y: 71 }, { x: 76, y: 78 }
  ];

  constructor(
    private readonly shipService: ShipService,
    private readonly berthService: BerthService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  get filteredShips(): PortShip[] {
    const query = this.searchTerm.trim().toLowerCase();

    return this.ships.filter((ship) => {
      const matchesStatus = this.selectedStatus === 'ALL' || ship.statusLabel === this.selectedStatus;
      const matchesSize = this.selectedSize === 'ALL' || ship.size === this.selectedSize;
      const matchesAvailability =
        this.selectedAvailability === 'Any' ||
        (this.selectedAvailability === 'Available' && ship.available) ||
        (this.selectedAvailability === 'Unavailable' && !ship.available);
      const matchesUnavailableToggle = this.showUnavailable || ship.available;
      const matchesSearch =
        !query ||
        ship.id.toLowerCase().includes(query) ||
        ship.name.toLowerCase().includes(query) ||
        ship.notes.toLowerCase().includes(query);

      return matchesStatus && matchesSize && matchesAvailability && matchesUnavailableToggle && matchesSearch;
    });
  }

  get summaryCards(): SummaryCard[] {
    const pending = this.ships.filter((ship) => ship.status === 'pending').length;
    const inPort = this.ships.filter((ship) => ship.status === 'in-port').length;
    const departed = this.ships.filter((ship) => ship.status === 'departed').length;
    const availableBerths = Math.max(this.berths.length - inPort, 0);

    return [
      { label: 'Total Ships', value: this.ships.length, color: '#111827' },
      { label: 'Pending', value: pending, color: '#f5b82e' },
      { label: 'In Port', value: inPort, color: '#3478db' },
      { label: 'Departed', value: departed, color: '#6b7280' },
      { label: 'Available Berths', value: availableBerths, color: '#55ad67' }
    ];
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      ships: this.shipService.getShips().pipe(catchError(() => of([] as Ship[]))),
      berths: this.berthService.getBerths().pipe(catchError(() => of([] as Berth[])))
    })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe(({ ships, berths }) => {
        this.apiBerths = berths;

        if (ships.length > 0) {
          this.ships = ships.map((ship, index) => this.mapApiShip(ship, index));
          return;
        }

        this.errorMessage = 'Demo data shown until the backend returns ships.';
      });
  }

  setViewMode(mode: ViewMode): void {
    this.viewMode = mode;
  }

  openCreatePanel(): void {
    this.isCreatePanelOpen = true;
  }

  closeCreatePanel(): void {
    this.isCreatePanelOpen = false;
    this.createForm = { name: '', notes: '' };
  }

  createShip(): void {
    const name = this.createForm.name.trim();

    if (!name) {
      return;
    }

    this.shipService
      .createShip({ name, notes: this.createForm.notes.trim() })
      .pipe(catchError(() => of(null)))
      .subscribe((ship) => {
        if (ship) {
          this.ships = [this.mapApiShip(ship, this.ships.length), ...this.ships];
          this.errorMessage = '';
        } else {
          this.ships = [this.createLocalShip(name, this.createForm.notes), ...this.ships];
          this.errorMessage = 'Ship added locally. Backend create endpoint is not available yet.';
        }

        this.closeCreatePanel();
      });
  }

  focusPendingShips(): void {
    this.selectedStatus = 'Pending';
    this.viewMode = 'List';
  }

  showShipList(): void {
    this.viewMode = 'List';
    this.selectedStatus = 'ALL';
  }

  resetFilters(): void {
    this.selectedStatus = 'ALL';
    this.selectedSize = 'ALL';
    this.selectedAvailability = 'Any';
    this.searchTerm = '';
    this.showUnavailable = true;
  }

  trackByLabel(_index: number, item: { label: string }): string {
    return item.label;
  }

  trackByValue(_index: number, value: string): string {
    return value;
  }

  trackByShipId(_index: number, ship: PortShip): string {
    return ship.id;
  }

  private mapApiShip(ship: Ship, index: number): PortShip {
    const position = this.shipPositions[index % this.shipPositions.length];
    const status = this.normalizeStatus(ship.status);
    const size = this.normalizeSize(ship.size);

    return {
      id: this.formatShipId(ship.id),
      name: ship.name || this.formatShipId(ship.id),
      notes: ship.notes || '',
      status,
      statusLabel: this.toStatusLabel(status),
      size,
      arrivalDay: ship.arrivalDay,
      duration: ship.duration,
      berthLabel: this.berths[index % this.berths.length].label,
      available: status !== 'in-port',
      ...position
    };
  }

  private createLocalShip(name: string, notes: string): PortShip {
    const index = this.ships.length;
    const position = this.shipPositions[index % this.shipPositions.length];

    return {
      id: `NF${400 + index}`,
      name,
      notes,
      status: 'pending',
      statusLabel: 'Pending',
      size: 'M',
      arrivalDay: this.currentDay,
      duration: 4,
      berthLabel: '-',
      available: true,
      ...position
    };
  }

  private createDemoShips(): PortShip[] {
    const demoShips: Array<Omit<PortShip, 'statusLabel' | 'available'>> = [
      { id: 'NF238', name: 'MSC Aurora', notes: 'Priority cargo', status: 'assigned', size: 'M', arrivalDay: 3, duration: 10, berthLabel: 'F', x: 25, y: 50 },
      { id: 'NF184', name: 'Costa Marina', notes: '', status: 'in-port', size: 'L', arrivalDay: 5, duration: 7, berthLabel: 'F', x: 25, y: 56 },
      { id: 'NF256', name: 'Ever Glory', notes: 'Refrigerated', status: 'in-port', size: 'L', arrivalDay: 2, duration: 6, berthLabel: 'F', x: 25, y: 62 },
      { id: 'NF301', name: 'Pacific Star', notes: '', status: 'departed', size: 'L', arrivalDay: 1, duration: 4, berthLabel: 'F', x: 25, y: 68 },
      { id: 'NF176', name: 'North Wind', notes: 'Late arrival', status: 'pending', size: 'S', arrivalDay: 12, duration: 3, berthLabel: 'E', x: 36, y: 49 },
      { id: 'NF259', name: 'Blue Horizon', notes: '', status: 'assigned', size: 'M', arrivalDay: 11, duration: 5, berthLabel: 'E', x: 36, y: 56 },
      { id: 'NF332', name: 'Atlas Pearl', notes: '', status: 'in-port', size: 'XL', arrivalDay: 10, duration: 8, berthLabel: 'E', x: 36, y: 64 },
      { id: 'NF210', name: 'Sea Falcon', notes: '', status: 'assigned', size: 'XL', arrivalDay: 9, duration: 5, berthLabel: 'D', x: 47, y: 51 },
      { id: 'NF423', name: 'Port Runner', notes: '', status: 'pending', size: 'S', arrivalDay: 13, duration: 2, berthLabel: 'C', x: 58, y: 57, rotation: -90 },
      { id: 'NF145', name: 'Green Tide', notes: '', status: 'assigned', size: 'M', arrivalDay: 8, duration: 4, berthLabel: 'B', x: 66, y: 50 },
      { id: 'NF187', name: 'Ocean Metric', notes: '', status: 'in-port', size: 'L', arrivalDay: 7, duration: 6, berthLabel: 'B', x: 66, y: 57 },
      { id: 'NF233', name: 'Dock Light', notes: '', status: 'assigned', size: 'S', arrivalDay: 12, duration: 2, berthLabel: 'B', x: 66, y: 64 },
      { id: 'NF295', name: 'Wave Prime', notes: '', status: 'in-port', size: 'S', arrivalDay: 11, duration: 4, berthLabel: 'B', x: 66, y: 71 },
      { id: 'NF130', name: 'Adriatic Sun', notes: '', status: 'in-port', size: 'L', arrivalDay: 6, duration: 6, berthLabel: 'A', x: 76, y: 50 },
      { id: 'NF167', name: 'Marble Coast', notes: '', status: 'in-port', size: 'M', arrivalDay: 6, duration: 5, berthLabel: 'A', x: 76, y: 57 },
      { id: 'NF222', name: 'Silver Dock', notes: '', status: 'assigned', size: 'M', arrivalDay: 14, duration: 3, berthLabel: 'A', x: 76, y: 64 },
      { id: 'NF278', name: 'Harbor Line', notes: '', status: 'in-port', size: 'L', arrivalDay: 8, duration: 7, berthLabel: 'A', x: 76, y: 71 },
      { id: 'NF319', name: 'Old Compass', notes: '', status: 'departed', size: 'XL', arrivalDay: 1, duration: 5, berthLabel: 'A', x: 76, y: 78 }
    ];

    return demoShips.map((ship) => ({
      ...ship,
      statusLabel: this.toStatusLabel(ship.status),
      available: ship.status !== 'in-port'
    }));
  }

  private normalizeStatus(status: Ship['status']): UiShipStatus {
    if (status === 0 || status === 'Pending') {
      return 'pending';
    }

    if (status === 1 || status === 'Assigned') {
      return 'assigned';
    }

    if (status === 2 || status === 'Departed') {
      return 'departed';
    }

    return 'pending';
  }

  private normalizeSize(size: Ship['size']): UiShipSize {
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

  private toStatusLabel(status: UiShipStatus): string {
    const labels: Record<UiShipStatus, string> = {
      pending: 'Pending',
      assigned: 'Assigned',
      'in-port': 'In Port',
      departed: 'Departed'
    };

    return labels[status];
  }

  private formatShipId(id: number): string {
    return `NF${String(100 + id).padStart(3, '0')}`;
  }
}
