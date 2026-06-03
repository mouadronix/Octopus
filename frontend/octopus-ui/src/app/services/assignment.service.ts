import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Assignment } from '../models/assignment.model';

@Injectable({ providedIn: 'root' })
export class AssignmentService {
  constructor(private readonly http: HttpClient) {}

  getAssignments(): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(`${environment.apiBaseUrl}/assignments`);
  }
}
