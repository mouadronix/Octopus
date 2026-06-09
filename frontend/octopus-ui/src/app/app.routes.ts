import { Routes } from '@angular/router';
import { AppShellComponent } from './components/layout/app-shell/app-shell.component';
import { HomeComponent } from './pages/home/home.component';
import { OperatorComponent } from './pages/operator/operator.component';
import { SchedulerComponent } from './pages/scheduler/scheduler.component';
import { ShipsComponent } from './pages/ships/ships.component';
import { BerthsComponent } from './pages/berths/berths.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
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
