import { Routes } from '@angular/router';
import { AppShellComponent } from './components/layout/app-shell/app-shell.component';
import { LoginComponent } from './pages/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { OperatorComponent } from './pages/operator/operator.component';
import { SchedulerComponent } from './pages/scheduler/scheduler.component';
import { ShipsComponent } from './pages/ships/ships.component';
import { DocksComponent } from './pages/berths/berths.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'operator', component: OperatorComponent, data: { roles: ['operator'] } },
      { path: 'scheduler', component: SchedulerComponent, data: { roles: ['scheduler'] } },
      { path: 'ships', component: ShipsComponent, data: { roles: ['operator'] } },
      { path: 'docks', component: DocksComponent, data: { roles: ['scheduler'] } }
    ]
  },
  { path: '**', redirectTo: '' }
];
