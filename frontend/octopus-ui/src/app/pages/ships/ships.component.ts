import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-ships',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './ships.component.html',
  styleUrl: './ships.component.scss'
})
export class ShipsComponent {}
