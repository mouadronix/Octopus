import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Assignment } from '../../models/assignment.model';
import { Berth } from '../../models/berth.model';
import { Ship, ShipSize, ShipStatus } from '../../models/ship.model';
import { SystemState } from '../../models/system-state.model';
import { AssignmentService } from '../../services/assignment.service';
import { BerthService } from '../../services/berth.service';
import { ShipService } from '../../services/ship.service';
import { SystemService } from '../../services/system.service';

interface DashboardMetric {
  label: string;
  value: number;
  caption: string;
  tone: 'purple' | 'orange' | 'green' | 'cyan';
  icon: 'ship' | 'crane' | 'berth';
  trend: number[];
}

interface StatusBreakdown {
  label: 'Pending' | 'Assigned' | 'Departed' | 'Total';
  value: number;
  percentage: number;
  color: 'orange' | 'green' | 'muted' | 'blue';
}

interface ArrivalPoint {
  day: number;
  label: string;
  value: number;
}

interface ActivityItem {
  message: string;
  meta: string;
  tone: 'success' | 'warning' | 'info' | 'purple' | 'muted';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  ships: Ship[] = [];
  berths: Berth[] = [];
  assignments: Assignment[] = [];
  state: SystemState | null = null;
  isLoading = true;
  errorMessage = '';

  constructor(
    private readonly shipService: ShipService,
    private readonly berthService: BerthService,
    private readonly assignmentService: AssignmentService,
    private readonly systemService: SystemService
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  get currentDay(): number {
    return this.state?.currentDay ?? 1;
  }

  get pendingShips(): Ship[] {
    return this.ships.filter((ship) => this.normalizeStatus(ship.status) === 'Pending');
  }

  get assignedShips(): Ship[] {
    return this.ships.filter((ship) => this.normalizeStatus(ship.status) === 'Assigned');
  }

  get departedShips(): Ship[] {
    return this.ships.filter((ship) => this.normalizeStatus(ship.status) === 'Departed');
  }

  get occupiedBerths(): number {
    return this.berths.filter((berth) => (berth.assignments?.length ?? 0) > 0).length;
  }

  get availableBerths(): number {
    return Math.max(this.berths.length - this.occupiedBerths, 0);
  }

  get utilizationPercentage(): number {
    return this.berths.length ? Math.round((this.occupiedBerths / this.berths.length) * 100) : 0;
  }

  get metrics(): DashboardMetric[] {
    return [
      {
        label: 'Total Ships',
        value: this.ships.length,
        caption: 'All registered ships',
        tone: 'purple',
        icon: 'ship',
        trend: [2, 4, 3, 5, 4, 6, 5]
      },
      {
        label: 'Pending Ships',
        value: this.pendingShips.length,
        caption: 'Awaiting assignment',
        tone: 'orange',
        icon: 'ship',
        trend: [4, 3, 3, 4, 2, 5, 3]
      },
      {
        label: 'Occupied Berths',
        value: this.occupiedBerths,
        caption: 'Currently in use',
        tone: 'green',
        icon: 'crane',
        trend: [3, 4, 2, 4, 3, 5, 4]
      },
      {
        label: 'Available Berths',
        value: this.availableBerths,
        caption: 'Ready for allocation',
        tone: 'cyan',
        icon: 'berth',
        trend: [5, 4, 6, 4, 5, 3, 6]
      }
    ];
  }

  get statusBreakdown(): StatusBreakdown[] {
    const total = Math.max(this.ships.length, 1);

    return [
      { label: 'Pending', value: this.pendingShips.length, percentage: this.percent(this.pendingShips.length, total), color: 'orange' },
      { label: 'Assigned', value: this.assignedShips.length, percentage: this.percent(this.assignedShips.length, total), color: 'green' },
      { label: 'Departed', value: this.departedShips.length, percentage: this.percent(this.departedShips.length, total), color: 'muted' },
      { label: 'Total', value: this.ships.length, percentage: 100, color: 'blue' }
    ];
  }

  get upcomingArrivals(): Ship[] {
    return this.ships
      .filter((ship) => ship.arrivalDay >= this.currentDay)
      .sort((left, right) => left.arrivalDay - right.arrivalDay || left.name.localeCompare(right.name))
      .slice(0, 5);
  }

  get arrivalChart(): ArrivalPoint[] {
    return Array.from({ length: 7 }, (_item, index) => {
      const day = this.currentDay + index;
      return {
        day,
        label: `Day ${day}`,
        value: this.ships.filter((ship) => ship.arrivalDay === day).length
      };
    });
  }

  get maxArrivalValue(): number {
    return Math.max(...this.arrivalChart.map((point) => point.value), 1);
  }

  get recentActivity(): ActivityItem[] {
    const assigned = this.assignments
      .slice()
      .sort((left, right) => right.startDay - left.startDay)
      .slice(0, 3)
      .map((assignment) => ({
        message: `Ship ${assignment.ship?.name ?? `#${assignment.shipId}`} assigned to berth ${this.findBerthName(assignment.dockId)}`,
        meta: `Day ${assignment.startDay}`,
        tone: 'success' as const
      }));

    const departed = this.departedShips.slice(0, 1).map((ship) => ({
      message: `Ship ${ship.name} departed from berth ${ship.berthName ?? '-'}`,
      meta: `Day ${Math.max(ship.arrivalDay + ship.duration - 1, 1)}`,
      tone: 'info' as const
    }));

    return [
      ...assigned,
      ...departed,
      {
        message: `Virtual day advanced to Day ${this.currentDay}`,
        meta: `Day ${this.currentDay}`,
        tone: 'muted' as const
      }
    ].slice(0, 5);
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      ships: this.shipService.getShips(),
      berths: this.berthService.getBerths(),
      assignments: this.assignmentService.getAssignments(),
      state: this.systemService.getState()
    }).subscribe({
      next: ({ ships, berths, assignments, state }) => {
        this.ships = ships;
        this.berths = berths;
        this.assignments = assignments;
        this.state = state;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Dashboard data is not available from the backend.';
        this.isLoading = false;
      }
    });
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

  getImo(ship: Ship): string {
    const match = ship.notes?.match(/IMO:\s*([A-Za-z0-9-]+)/i);
    return match?.[1] ?? ship.notes ?? '-';
  }

  getMetricPath(points: number[]): string {
    const max = Math.max(...points, 1);
    return points
      .map((point, index) => {
        const x = index * (100 / Math.max(points.length - 1, 1));
        const y = 34 - (point / max) * 26;
        return `${index === 0 ? 'M' : 'L'} ${x.toFixed(1)} ${y.toFixed(1)}`;
      })
      .join(' ');
  }

  getChartPoint(point: ArrivalPoint, index: number): string {
    const x = this.chartX(index);
    const y = this.chartY(point.value);
    return `${x.toFixed(1)},${y.toFixed(1)}`;
  }

  getChartPolyline(): string {
    return this.arrivalChart
      .map((point, index) => this.getChartPoint(point, index))
      .join(' ');
  }

  getChartArea(): string {
    const line = this.getChartPolyline();
    return `0,160 ${line} 100,160`;
  }

  trackByMetric(_index: number, metric: DashboardMetric): string {
    return metric.label;
  }

  trackByShip(_index: number, ship: Ship): number {
    return ship.id;
  }

  trackByActivity(_index: number, activity: ActivityItem): string {
    return `${activity.message}-${activity.meta}`;
  }

  trackByArrival(_index: number, point: ArrivalPoint): number {
    return point.day;
  }

  chartX(index: number): number {
    return index * (100 / Math.max(this.arrivalChart.length - 1, 1));
  }

  chartY(value: number): number {
    return 160 - (value / this.maxArrivalValue) * 130;
  }

  private findBerthName(dockId: number): string {
    return this.berths.find((berth) => berth.id === dockId)?.name ?? `#${dockId}`;
  }

  private percent(value: number, total: number): number {
    return Math.round((value / total) * 100);
  }
}
