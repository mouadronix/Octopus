import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AuthResponse {
  id: number;
  fullName: string;
  username: string;
  role: string;
  signedInAtUtc: string;
}

export interface AuthSession {
  id: number;
  fullName: string;
  username: string;
  role: string;
  signedInAt: string;
}

export interface RegisterRequest {
  fullName: string;
  username: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly sessionStorageKey = 'octopus-session';
  private readonly apiUrl = `${environment.apiBaseUrl}/auth`;
  // The BehaviorSubject keeps header, guards, and future components synchronized after login/logout.
  private readonly currentSessionSubject = new BehaviorSubject<AuthSession | null>(this.readSession());

  readonly currentSession$ = this.currentSessionSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  login(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { username, password });
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request);
  }

  get currentSession(): AuthSession | null {
    return this.currentSessionSubject.value;
  }

  isAuthenticated(): boolean {
    return this.currentSession !== null;
  }

  saveSession(user: AuthResponse, persist: boolean): void {
    const session: AuthSession = {
      id: user.id,
      fullName: user.fullName,
      username: user.username,
      role: user.role,
      signedInAt: user.signedInAtUtc
    };

    const serializedSession = JSON.stringify(session);
    // sessionStorage is always written for the active tab; localStorage is used only for "Remember me".
    sessionStorage.setItem(this.sessionStorageKey, serializedSession);

    if (persist) {
      localStorage.setItem(this.sessionStorageKey, serializedSession);
    } else {
      localStorage.removeItem(this.sessionStorageKey);
    }

    this.currentSessionSubject.next(session);
  }

  logout(): void {
    sessionStorage.removeItem(this.sessionStorageKey);
    localStorage.removeItem(this.sessionStorageKey);
    this.currentSessionSubject.next(null);
  }

  private readSession(): AuthSession | null {
    // Prefer the active-tab session, then fall back to the remembered session after a browser refresh.
    const rawSession = sessionStorage.getItem(this.sessionStorageKey) ?? localStorage.getItem(this.sessionStorageKey);

    if (!rawSession) {
      return null;
    }

    try {
      const session = JSON.parse(rawSession) as AuthSession;
      return session?.username ? session : null;
    } catch {
      sessionStorage.removeItem(this.sessionStorageKey);
      localStorage.removeItem(this.sessionStorageKey);
      return null;
    }
  }
}
