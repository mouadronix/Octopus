import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-page-placeholder',
  standalone: true,
  template: `
    <section class="page-view">
      <header class="page-header">
        <span class="section-label">{{ sectionLabel }}</span>
        <h2>This is the page of {{ pageName }}</h2>
        <p>{{ description }}</p>
      </header>

      <div class="panel">
        <p>{{ nextStep }}</p>
      </div>
    </section>
  `,
  styles: [`
    .page-view {
      display: grid;
      gap: 1.25rem;
    }

    .page-header {
      display: grid;
      gap: 0.35rem;
    }

    .section-label {
      color: #2563eb;
      font-size: 0.78rem;
      font-weight: 800;
      letter-spacing: 0;
      text-transform: uppercase;
    }

    h2 {
      color: #182230;
      font-size: 1.45rem;
      line-height: 1.2;
      margin: 0;
    }

    .page-header p {
      color: #475467;
      margin: 0;
      max-width: 720px;
    }

    .panel {
      background: #ffffff;
      border: 1px solid #dbe3ef;
      border-radius: 8px;
      padding: 1rem;
    }

    .panel p {
      color: #344054;
      margin: 0;
    }
  `]
})
export class PagePlaceholderComponent {
  @Input({ required: true }) pageName = '';
  @Input({ required: true }) sectionLabel = '';
  @Input({ required: true }) description = '';
  @Input() nextStep = 'This page is ready for the next feature implementation.';
}
