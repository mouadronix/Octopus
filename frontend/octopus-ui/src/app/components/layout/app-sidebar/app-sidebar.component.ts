import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService, UserRole } from '../../../services/auth.service';
import { DockService } from '../../../services/dock.service';
import { ShipService } from '../../../services/ship.service';
import { SystemService } from '../../../services/system.service';
import { Ship, ShipStatus } from '../../../models/ship.model';
import { Dock } from '../../../models/dock.model';
import { SystemState } from '../../../models/system-state.model';

interface SidebarStat {
  label: string;
  value: string;
  color: 'orange' | 'green' | 'cyan' | 'blue';
}

interface NavItem {
  label: string;
  route: string;
  icon: 'dashboard' | 'ship' | 'pending' | 'berth' | 'calendar';
  roles?: UserRole[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './app-sidebar.component.html',
  styleUrl: './app-sidebar.component.scss'
})
export class AppSidebarComponent implements OnInit {
  private readonly allNavItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
    { label: 'Ships', route: '/ships', icon: 'ship', roles: ['operator'] },
    { label: 'Pending Assignments', route: '/scheduler', icon: 'pending', roles: ['scheduler'] },
    { label: 'Ship Operations', route: '/operator', icon: 'berth', roles: ['operator'] },
    { label: 'Planning Calendar', route: '/docks', icon: 'calendar', roles: ['scheduler'] }
  ];

  collapsed = false;
  @Output() collapsedChange = new EventEmitter<boolean>();

  stats: SidebarStat[] = [
    { label: 'Ships Pending', value: '6', color: 'orange' },
    { label: 'Docks Occupied', value: '3', color: 'green' },
    { label: 'Docks Available', value: '7', color: 'cyan' },
    { label: 'Next Arrival Day 13', value: '', color: 'blue' }
  ];

  constructor(
    private readonly authService: AuthService,
    private readonly shipService: ShipService,
    private readonly dockService: DockService,
    private readonly systemService: SystemService,
  ) {}

  get navItems(): NavItem[] {
    const role = this.authService.currentRole;
    return this.allNavItems.filter((item) => !item.roles || item.roles.includes(role!));
  }

  trackByNavLabel(_index: number, item: NavItem): string {
    return item.label;
  }

  trackByStatLabel(_index: number, stat: SidebarStat): string {
    return stat.label;
  }

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }

  ngOnInit(): void {
    forkJoin({
      ships: this.shipService.getShips(),
      docks: this.dockService.getDocks(),
      state: this.systemService.getState(),
    }).subscribe(({ ships, docks, state }) => {
      const pendingShips = ships.filter((s) => this.normalizeStatus(s.status) === 'Pending');
      const occupiedDocks = docks.filter((d) => (d.assignments?.length ?? 0) > 0).length;
      const availableDocks = docks.filter((d) => (d.assignments?.length ?? 0) === 0).length;

      const nextArrival = pendingShips
        .filter((s) => s.arrivalDay > state.currentDay)
        .sort((a, b) => a.arrivalDay - b.arrivalDay)[0];

      this.stats = [
        { label: 'Ships Pending', value: String(pendingShips.length), color: 'orange' },
        { label: 'Docks Occupied', value: String(occupiedDocks), color: 'green' },
        { label: 'Docks Available', value: String(availableDocks), color: 'cyan' },
        { label: nextArrival ? `Next Arrival Day ${nextArrival.arrivalDay}` : 'No Upcoming', value: '', color: 'blue' },
      ];
    });
  }

  private normalizeStatus(status: ShipStatus): 'Pending' | 'Assigned' | 'Departed' {
    if (status === 1 || status === 'Assigned') return 'Assigned';
    if (status === 2 || status === 'Departed') return 'Departed';
    return 'Pending';
  }
}
