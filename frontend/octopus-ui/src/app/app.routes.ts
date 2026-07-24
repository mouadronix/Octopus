import { Routes } from '@angular/router';
import { OperatorComponent } from './components/operator/operator.component';
import { SchedulerComponent } from './components/scheduler/scheduler.component';
import { ShipsComponent } from './components/ships/ships.component';
import { BerthsComponent } from './components/berths/berths.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'operator' },
  { path: 'operator', component: OperatorComponent },
  { path: 'scheduler', component: SchedulerComponent },
  { path: 'ships', component: ShipsComponent },
  { path: 'berths', component: BerthsComponent }
];
