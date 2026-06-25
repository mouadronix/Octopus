import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface SidebarStat {
  label: string;
  value: string;
  color: 'orange' | 'green' | 'cyan' | 'blue';
}

interface NavItem {
  label: string;
  route: string;
  icon: 'dashboard' | 'ship' | 'pending' | 'berth' | 'calendar' | 'log' | 'time';
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './app-sidebar.component.html',
  styleUrl: './app-sidebar.component.scss'
})
export class AppSidebarComponent {
  navItems: NavItem[] = [
    { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
    { label: 'Ships', route: '/ships', icon: 'ship' },
    { label: 'New Ship', route: '/ships/new', icon: 'ship' },
    { label: 'Pending Assignments', route: '/scheduler', icon: 'pending' },
    { label: 'Berth Board', route: '/operator', icon: 'berth' },
    { label: 'Planning Calendar', route: '/berths', icon: 'calendar' },
    { label: 'Activity Log', route: '/activity-log', icon: 'log' },
    { label: 'Virtual Time', route: '/virtual-time', icon: 'time' }
  ];

  stats: SidebarStat[] = [
    { label: 'Ships Pending', value: '6', color: 'orange' },
    { label: 'Berths Occupied', value: '3', color: 'green' },
    { label: 'Berths Available', value: '7', color: 'cyan' },
    { label: 'Next Arrival Day 13', value: '', color: 'blue' }
  ];

  trackByNavLabel(_index: number, item: NavItem): string {
    return item.label;
  }

  trackByStatLabel(_index: number, stat: SidebarStat): string {
    return stat.label;
  }
}
