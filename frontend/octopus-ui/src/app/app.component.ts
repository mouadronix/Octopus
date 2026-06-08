import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { APP_NAVIGATION } from './app.navigation';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="app-shell">
      <aside class="sidebar" aria-label="Main navigation">
        <a class="brand" routerLink="/home" aria-label="Octopus home">
          <span class="brand-mark">O</span>
          <span>
            <strong>Octopus</strong>
            <small>BlueHarbor Terminal</small>
          </span>
        </a>

        <nav>
          @for (item of navigation; track item.path) {
            <a
              [routerLink]="item.path"
              routerLinkActive="active"
              [routerLinkActiveOptions]="{ exact: true }"
              [title]="item.description"
            >
              {{ item.label }}
            </a>
          }
        </nav>
      </aside>

      <section class="workspace">
        <header class="topbar">
          <div>
            <span class="eyebrow">Internal operations</span>
            <h1>BlueHarbor Registro Terminal</h1>
          </div>
          <span class="status">Development</span>
        </header>

        <main class="content">
          <router-outlet />
        </main>
      </section>
    </div>
  `,
  styles: [`
    .app-shell {
      background: #f5f7fb;
      display: grid;
      grid-template-columns: 260px minmax(0, 1fr);
      min-height: 100vh;
    }

    .sidebar {
      background: #111827;
      color: #ffffff;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      padding: 1.25rem;
    }

    .brand {
      align-items: center;
      display: flex;
      gap: 0.75rem;
      min-height: 44px;
    }

    .brand-mark {
      align-items: center;
      background: #2dd4bf;
      border-radius: 8px;
      color: #042f2e;
      display: inline-flex;
      font-weight: 800;
      height: 40px;
      justify-content: center;
      width: 40px;
    }

    .brand strong,
    .brand small {
      display: block;
    }

    .brand strong {
      font-size: 1rem;
      line-height: 1.2;
    }

    .brand small {
      color: #cbd5e1;
      font-size: 0.78rem;
      margin-top: 0.12rem;
    }

    nav {
      display: grid;
      gap: 0.35rem;
    }

    nav a {
      align-items: center;
      border-radius: 6px;
      color: #dbeafe;
      display: flex;
      min-height: 40px;
      padding: 0 0.75rem;
    }

    nav a:hover,
    nav a.active {
      background: rgba(255, 255, 255, 0.1);
      color: #ffffff;
    }

    .workspace {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .topbar {
      align-items: center;
      background: #ffffff;
      border-bottom: 1px solid #dbe3ef;
      display: flex;
      justify-content: space-between;
      min-height: 84px;
      padding: 1rem 1.5rem;
    }

    .eyebrow {
      color: #64748b;
      display: block;
      font-size: 0.78rem;
      font-weight: 700;
      letter-spacing: 0;
      text-transform: uppercase;
    }

    h1 {
      color: #182230;
      font-size: 1.35rem;
      line-height: 1.2;
      margin: 0.2rem 0 0;
    }

    .status {
      background: #ecfdf5;
      border: 1px solid #bbf7d0;
      border-radius: 999px;
      color: #166534;
      font-size: 0.82rem;
      font-weight: 700;
      padding: 0.35rem 0.65rem;
    }

    .content {
      padding: 1.5rem;
    }

    @media (max-width: 760px) {
      .app-shell {
        grid-template-columns: 1fr;
      }

      .sidebar {
        gap: 1rem;
        padding: 1rem;
      }

      nav {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }

      .topbar {
        align-items: flex-start;
        gap: 1rem;
        min-height: 0;
      }

      .content {
        padding: 1rem;
      }
    }
  `]
})
export class AppComponent {
  navigation = APP_NAVIGATION;
}
