import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService, UserRole } from '../../../services/auth.service';

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
export class AppSidebarComponent {
  private readonly allNavItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
    { label: 'Ships', route: '/ships', icon: 'ship', roles: ['operator'] },
    { label: 'Pending Assignments', route: '/scheduler', icon: 'pending', roles: ['scheduler'] },
    { label: 'Ship Operations', route: '/operator', icon: 'berth', roles: ['operator'] },
    { label: 'Planning Calendar', route: '/berths', icon: 'calendar', roles: ['scheduler'] }
  ];

  stats: SidebarStat[] = [
    { label: 'Ships Pending', value: '6', color: 'orange' },
    { label: 'Berths Occupied', value: '3', color: 'green' },
    { label: 'Berths Available', value: '7', color: 'cyan' },
    { label: 'Next Arrival Day 13', value: '', color: 'blue' }
  ];

  constructor(private readonly authService: AuthService) {}

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
}
