import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Assignment, CreateAssignmentRequest } from '../models/assignment.model';

@Injectable({ providedIn: 'root' })
export class AssignmentService {
  private readonly apiUrl = `${environment.apiBaseUrl}/assignments`;

  constructor(private readonly http: HttpClient) {}

  getAssignments(): Observable<Assignment[]> {
    return this.http.get<Assignment[]>(this.apiUrl);
  }

  createAssignment(request: CreateAssignmentRequest): Observable<Assignment> {
    return this.http.post<Assignment>(this.apiUrl, request);
  }
}
