import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { APP_NAVIGATION } from '../../app.navigation';

interface HomeMetric {
  label: string;
  value: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './app-home.component.html',
  styleUrl: './app-home.component.css'
})
export class AppHomeComponent {
  navigation = APP_NAVIGATION;

  metrics: HomeMetric[] = [
    { label: 'Pending', value: '0' },
    { label: 'Assigned', value: '0' },
    { label: 'Departed', value: '0' }
  ];
}
