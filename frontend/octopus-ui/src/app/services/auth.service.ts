import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type UserRole = 'operator' | 'scheduler';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'octopus-role';
  private readonly roleSubject = new BehaviorSubject<UserRole | null>(this.readRole());

  readonly role$ = this.roleSubject.asObservable();

  get currentRole(): UserRole | null {
    return this.roleSubject.value;
  }

  isAuthenticated(): boolean {
    return this.roleSubject.value !== null;
  }

  setRole(role: UserRole): void {
    localStorage.setItem(this.storageKey, role);
    this.roleSubject.next(role);
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.roleSubject.next(null);
  }

  private readRole(): UserRole | null {
    const raw = localStorage.getItem(this.storageKey);
    if (raw === 'operator' || raw === 'scheduler') {
      return raw;
    }
    return null;
  }
}
