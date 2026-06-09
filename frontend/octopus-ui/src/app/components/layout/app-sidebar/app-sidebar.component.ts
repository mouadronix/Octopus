import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface SidebarStat {
  label: string;
  value: number;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './app-sidebar.component.html',
  styleUrl: './app-sidebar.component.scss'
})
export class AppSidebarComponent {
  stats: SidebarStat[] = [
    { label: 'Pending', value: 0 },
    { label: 'Assigned', value: 0 },
    { label: 'Departed', value: 0 }
  ];

  trackByStatLabel(_index: number, stat: SidebarStat): string {
    return stat.label;
  }
}
