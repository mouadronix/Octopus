import { Routes } from '@angular/router';
import { AppShellComponent } from './components/layout/app-shell/app-shell.component';
import { LoginComponent } from './pages/login/login.component';
import { OperatorComponent } from './pages/operator/operator.component';
import { SchedulerComponent } from './pages/scheduler/scheduler.component';
import { ShipsComponent } from './pages/ships/ships.component';
import { BerthsComponent } from './pages/berths/berths.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: AppShellComponent,
    children: [
      { path: 'operator', component: OperatorComponent },
      { path: 'scheduler', component: SchedulerComponent },
      { path: 'ships', component: ShipsComponent },
      { path: 'berths', component: BerthsComponent }
    ]
  },
  { path: '**', redirectTo: '' }
];
