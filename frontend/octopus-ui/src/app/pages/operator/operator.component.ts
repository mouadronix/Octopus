import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { AssignmentService } from '../../services/assignment.service';
import { BerthService } from '../../services/berth.service';
import { ShipService } from '../../services/ship.service';
import { SystemService } from '../../services/system.service';
import { Berth } from '../../models/berth.model';
import { Ship, ShipSize, ShipStatus } from '../../models/ship.model';

type BerthStatus = 'available' | 'occupied';
type MetricIcon = 'ship' | 'crane' | 'berth' | 'calendar';
type MetricTone = 'purple' | 'green' | 'cyan' | 'blue';

interface BoardShip {
  id: number;
  displayId: string;
  name: string;
  imo: string;
  size: ShipSize;
  arrivalDay: number;
  duration: number;
  status: ShipStatus;
}

interface BerthSlot {
  id: number;
  name: string;
  size: ShipSize;
  status: BerthStatus;
  ship?: BoardShip;
}

interface BerthGroup {
  size: ShipSize;
  label: string;
  accent: string;
  berths: BerthSlot[];
}

interface MetricCard {
  label: string;
  value: number;
  icon: MetricIcon;
  tone: MetricTone;
}

@Component({
  selector: 'app-operator',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './operator.component.html',
  styleUrl: './operator.component.scss'
})
export class OperatorComponent implements OnInit {
  currentDay = 12;
  ships: BoardShip[] = [];
  berthGroups: BerthGroup[] = [];
  selectedShip: BoardShip | null = null;
  selectedCompatibleBerth = 0;
  assignmentMessage = '';
  isLoading = true;
  errorMessage = '';

  constructor(
    private readonly shipService: ShipService,
    private readonly berthService: BerthService,
    private readonly assignmentService: AssignmentService,
    private readonly systemService: SystemService
  ) {}

  ngOnInit(): void {
    this.loadBoard();
  }

  get metrics(): MetricCard[] {
    return [
      { label: 'Pending Ships', value: this.pendingShips.length, icon: 'ship', tone: 'purple' },
      { label: 'Occupied Berths', value: this.occupiedBerths, icon: 'crane', tone: 'green' },
      { label: 'Available Berths', value: this.availableBerths, icon: 'berth', tone: 'cyan' },
      { label: 'Current Day', value: this.currentDay, icon: 'calendar', tone: 'blue' }
    ];
  }

  //get ships that are pending assignment
  get pendingShips(): BoardShip[] {
    return this.ships.filter((ship) => this.normalizeStatus(ship.status) === 'Pending');
  }

  //get ships that are assigned to a berth
  get occupiedBerths(): number {
    return this.allBerths.filter((berth) => berth.status === 'occupied').length;
  }

  //get berths that are available for assignment
  get availableBerths(): number {
    return this.allBerths.filter((berth) => berth.status === 'available').length;
  }


  //get berths that can fit the selected ship
  get compatibleBerths(): BerthSlot[] {
    if (!this.selectedShip) return [];
    return this.allBerths.filter((berth) => this.canFitShip(berth.size, this.selectedShip!.size));
  }


  //get all berths across groups
  private get allBerths(): BerthSlot[] {
    return this.berthGroups.flatMap((group) => group.berths);
  }


  //load the board data
  loadBoard(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      ships: this.shipService.getShips(),
      berths: this.berthService.getBerths(),
      state: this.systemService.getState()
    }).subscribe({
      next: ({ ships, berths, state }) => {
        this.currentDay = state.currentDay;
        this.ships = ships.map((ship) => this.mapShip(ship));
        this.berthGroups = this.buildBerthGroups(berths);
        this.selectedShip = this.pendingShips[0] ?? this.ships[0] ?? null;
        this.selectedCompatibleBerth =
          this.compatibleBerths.find((berth) => berth.status === 'available')?.id ?? this.compatibleBerths[0]?.id ?? 0;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Backend data is not available. Start the API on port 5000.';
        this.isLoading = false;
      }
    });
  }


  //advance to the next day and refresh the board state
  nextDay(): void {
    this.systemService.nextDay().subscribe({
      next: (state) => {
        this.currentDay = state.currentDay;
        this.assignmentMessage = `Planning advanced to day ${state.currentDay}.`;
      },
      error: () => {
        this.assignmentMessage = 'Unable to advance the current day from the backend.';
      }
    });
  }


  //handle ship selection and update compatible berths
  selectShip(ship: BoardShip): void {
    this.selectedShip = ship;
    this.selectedCompatibleBerth =
      this.compatibleBerths.find((berth) => berth.status === 'available')?.id ?? this.compatibleBerths[0]?.id ?? 0;
    this.assignmentMessage = '';
  }


  //handle berth selection for assignment
  selectCompatibleBerth(berthId: number): void {
    this.selectedCompatibleBerth = berthId;
    this.assignmentMessage = '';
  }


  //attempt to assign the selected ship to the selected compatible berth
  assignSelectedShip(): void {
    if (!this.selectedShip || !this.selectedCompatibleBerth) {
      this.assignmentMessage = 'Select a ship and compatible berth first.';
      return;
    }

    this.assignmentService
      .createAssignment({ shipId: this.selectedShip.id, dockId: this.selectedCompatibleBerth })
      .subscribe({
        next: () => {
          this.assignmentMessage = `${this.selectedShip?.name} assigned successfully.`;
          this.loadBoard();
        },
        error: () => {
          this.assignmentMessage = 'The selected berth is occupied or incompatible for this ship.';
        }
      });
  }


  trackByMetric(_index: number, metric: MetricCard): string {
    return metric.label;
  }

  trackByGroup(_index: number, group: BerthGroup): string {
    return String(group.size);
  }

  trackByBerth(_index: number, berth: BerthSlot): number {
    return berth.id;
  }

  private buildBerthGroups(berths: Berth[]): BerthGroup[] {
    const order: ShipSize[] = ['XL', 'L', 'M', 'S'];

    return order
      .map((size) => {
        const groupBerths = berths
          .filter((berth) => this.normalizeSize(berth.size) === size)
          .sort((a, b) => a.name.localeCompare(b.name))
          .map((berth) => this.mapBerth(berth));

        return {
          size,
          label: `${size} Berths`,
          accent: size === 'XL' ? 'purple' : size === 'S' ? 'orange' : 'cyan',
          berths: groupBerths
        };
      })
      .filter((group) => group.berths.length > 0);
  }

  private mapBerth(berth: Berth): BerthSlot {
    const activeAssignment = berth.assignments?.[0];

    return {
      id: berth.id,
      name: berth.name,
      size: this.normalizeSize(berth.size),
      status: activeAssignment ? 'occupied' : 'available',
      ship: activeAssignment?.ship ? this.mapShip(activeAssignment.ship) : undefined
    };
  }

  private mapShip(ship: Ship): BoardShip {
    return {
      id: ship.id,
      displayId: `NF${String(100 + ship.id).padStart(3, '0')}`,
      name: ship.name,
      imo: this.extractImo(ship),
      size: this.normalizeSize(ship.size),
      arrivalDay: ship.arrivalDay,
      duration: ship.duration,
      status: this.normalizeStatus(ship.status)
    };
  }

  private extractImo(ship: Ship): string {
    const match = ship.notes?.match(/IMO:\s*([A-Za-z0-9-]+)/i);
    return match?.[1] ?? String(9000000 + ship.id);
  }

  private normalizeStatus(status: ShipStatus): 'Pending' | 'Assigned' | 'Departed' {
    if (status === 1 || status === 'Assigned') return 'Assigned';
    if (status === 2 || status === 'Departed') return 'Departed';
    return 'Pending';
  }

  private normalizeSize(size: ShipSize): 'XL' | 'L' | 'M' | 'S' {
    if (size === 0 || size === 'XL') return 'XL';
    if (size === 1 || size === 'L') return 'L';
    if (size === 2 || size === 'M') return 'M';
    return 'S';
  }

  private canFitShip(berthSize: ShipSize, shipSize: ShipSize): boolean {
    const rank: Record<'S' | 'M' | 'L' | 'XL', number> = { S: 1, M: 2, L: 3, XL: 4 };
    return rank[this.normalizeSize(berthSize)] >= rank[this.normalizeSize(shipSize)];
  }
}
