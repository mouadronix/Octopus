import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Berth } from '../models/berth.model';

@Injectable({ providedIn: 'root' })
export class BerthService {
  constructor(private readonly http: HttpClient) {}

  getBerths(): Observable<Berth[]> {
    return this.http.get<Berth[]>(`${environment.apiBaseUrl}/berths`);
  }
}
