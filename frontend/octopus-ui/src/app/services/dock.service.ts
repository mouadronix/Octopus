import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Dock } from '../models/dock.model';

@Injectable({ providedIn: 'root' })
export class DockService {
  private readonly apiUrl = `${environment.apiBaseUrl}/docks`;

  constructor(private readonly http: HttpClient) {}

  getDocks(): Observable<Dock[]> {
    return this.http.get<Dock[]>(this.apiUrl);
  }
}
