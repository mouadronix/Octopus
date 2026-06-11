import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateShipRequest, Ship } from '../models/ship.model';

@Injectable({ providedIn: 'root' })
export class ShipService {
  private readonly apiUrl = `${environment.apiBaseUrl}/ships`;

  constructor(private readonly http: HttpClient) {}

  getShips(): Observable<Ship[]> {
    return this.http.get<Ship[]>(this.apiUrl);
  }

  getPendingShips(): Observable<Ship[]> {
    return this.http.get<Ship[]>(`${this.apiUrl}/pending`);
  }

  createShip(request: CreateShipRequest): Observable<Ship> {
    return this.http.post<Ship>(this.apiUrl, request);
  }
}
