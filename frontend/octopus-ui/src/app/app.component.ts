import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterOutlet],
  template: `
    <header class="shell-header">
      <h1>Octopus</h1>
      <nav>
        <a routerLink="/operator">Operator</a>
        <a routerLink="/scheduler">Scheduler</a>
        <a routerLink="/ships">Ships</a>
        <a routerLink="/berths">Berths</a>
      </nav>
    </header>
    <main class="shell-main">
      <router-outlet />
    </main>
  `,
  styles: [`
    .shell-header {
      align-items: center;
      background: #0f172a;
      color: #ffffff;
      display: flex;
      gap: 2rem;
      justify-content: space-between;
      padding: 1rem 1.5rem;
    }

    h1 {
      font-size: 1.25rem;
      margin: 0;
    }

    nav {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
    }

    nav a {
      border: 1px solid rgba(255, 255, 255, 0.24);
      border-radius: 6px;
      padding: 0.45rem 0.7rem;
    }

    .shell-main {
      padding: 1.5rem;
    }
  `]
})
export class AppComponent {}
