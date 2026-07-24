import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Ship } from '../models/ship.model';

@Injectable({ providedIn: 'root' })
export class ShipService {
  constructor(private readonly http: HttpClient) {}

  getShips(): Observable<Ship[]> {
    return this.http.get<Ship[]>(`${environment.apiBaseUrl}/ships`);
  }
}
