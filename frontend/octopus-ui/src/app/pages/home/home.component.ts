import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';

type RoleRoute = 'operator' | 'scheduler';

interface RoleOption {
  label: string;
  route: RoleRoute;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  selectedRole: RoleRoute = 'operator';

  roles: RoleOption[] = [
    {
      label: 'Operatore',
      route: 'operator',
      icon: 'tool',
      description: 'Registra e gestisce le navi'
    },
    {
      label: 'Scheduler',
      route: 'scheduler',
      icon: 'clipboard',
      description: 'Assegna navi alle banchine'
    }
  ];

  constructor(private readonly router: Router) {}

  selectRole(role: RoleRoute): void {
    this.selectedRole = role;
  }

  enterSystem(): void {
    void this.router.navigate([this.selectedRole]);
  }

  trackByRole(_index: number, role: RoleOption): RoleRoute {
    return role.route;
  }
}
