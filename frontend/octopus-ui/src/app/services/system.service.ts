import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SystemState } from '../models/system-state.model';

@Injectable({ providedIn: 'root' })
export class SystemService {
  private readonly apiUrl = `${environment.apiBaseUrl}/terminal`;

  constructor(private readonly http: HttpClient) {}


  //get current system state
  getState(): Observable<SystemState> {
    return this.http.get<SystemState>(`${this.apiUrl}/day`);
  }


  //advance to the next day
  nextDay(): Observable<SystemState> {
    return this.http.post<SystemState>(`${this.apiUrl}/next-day`, {});
  }
}
