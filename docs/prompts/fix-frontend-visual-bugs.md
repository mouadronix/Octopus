# Prompt: Fix Frontend Visual Bugs

## Context

The OCTOPUS Port & Dock Management System (Angular 17 standalone components, SCSS, dark theme) has several visual bugs caused by **CSS class name mismatches** between HTML templates and SCSS stylesheets. The most critical bugs break the Planning Calendar and the Scheduler Timeline entirely. Additionally, there are minor inconsistencies across page hero sections.

The frontend lives at `frontend/octopus-ui/src/`.

---

## Bug 1 — Planning Calendar: Ships Stack Vertically (CRITICAL)

**Symptom:** In the Planning Calendar (`/berths`), assignment bars that should span horizontally across day columns instead stack vertically in a single column. The entire calendar layout is broken.

**Root Cause:** CSS class name mismatch between the HTML template and the SCSS stylesheet.

**File:** `app/pages/berths/berths.component.html` lines 64–80
**File:** `app/pages/berths/berths.component.scss` lines 236–237, 308–340

The HTML template uses:
```html
<div class="dock-label">    <!-- line 64 -->
<div class="dock-track">     <!-- line 69 -->
```

But the SCSS only defines styles for the OLD class names:
```scss
.berth-label { ... }   /* line 308 */
.berth-track { ... }    /* line 326 */
```

The `.dock-track` element is supposed to be a CSS Grid with `grid-template-columns: repeat(var(--day-count), minmax(112px, 1fr))` and `grid-column: 2 / -1`. Without these styles, it falls back to `display: block`, causing vertical stacking.

**Fix:** In `berths.component.scss`, rename all `.berth-label` selectors to `.dock-label` and all `.berth-track` selectors to `.dock-track`. The affected lines are:
- Line 236: `.berth-label,` → `.dock-label,`
- Line 237: `.berth-track,` (this is part of a shared border rule with other selectors — only change `.berth-track`)
- Line 308: `.berth-label {` → `.dock-label {`
- Line 315: `.berth-label span {` → `.dock-label span {`
- Line 320: `.berth-label small {` → `.dock-label small {`
- Line 326: `.berth-track {` → `.dock-track {`

Also update the shared border-bottom rule at line 236–237 which currently reads:
```scss
.month-cell,
.day-cell,
.berth-label,
.berth-track,
.group-row {
```
Change `.berth-label` to `.dock-label` and `.berth-track` to `.dock-track`.

---

## Bug 2 — Scheduler Timeline: Ship Names Truncated in Cells

**Symptom:** In the Dock Timeline at the bottom of the Scheduler page (`/scheduler`), ship names inside timeline cells are clipped/truncated. The cell is too narrow to display the full name.

**Root Cause:** Each timeline cell is only 1 grid column wide (`minmax(60px, 1fr)`), and the ship name is only rendered in the `isStart` cell. The `.timeline-ship` span has `text-overflow: ellipsis` but the cell is ~60px wide — far too narrow for a ship name.

**File:** `app/pages/scheduler/scheduler.component.html` lines 219–237
**File:** `app/pages/scheduler/scheduler.component.scss` lines 753–795

**Fix (Option A — CSS Grid spanning, recommended):** Instead of rendering the ship name only in the start cell, make the `.timeline-ship` span across the full duration of the assignment using CSS Grid. This requires changes to both the template and the SCSS:

1. In the SCSS, the `.timeline-cell` needs to become a grid container (or use `position: relative` with absolutely-positioned ship bars). The cleanest approach: overlay ship bars on top of the day cells.

2. Change the template so that ship assignment bars are rendered as absolutely-positioned overlays that span across the correct number of day columns, rather than being content inside individual cells.

3. Alternatively (simpler approach): keep the current cell-based rendering but make the ship name span use CSS to visually extend across adjacent cells:
   - On the `.timeline-ship` element, add `position: absolute; left: 0; white-space: nowrap;` and set a `z-index` so it overflows visually into adjacent cells.
   - Add `overflow: visible` to `.timeline-cell` (remove `overflow: hidden` if present).
   - Ensure `.timeline-cell` has `position: relative`.

**Fix (Option B — simpler, less elegant):** Increase the minimum column width from `60px` to `120px` to give ship names more room, and add `font-size: 0.62rem` to `.timeline-ship` for a tighter fit.

---

## Bug 3 — Scheduler: Dock Option Buttons Unstyled

**Symptom:** In the Scheduler's Assignment Preview panel, the "Compatible Docks" buttons appear as unstyled browser-default buttons.

**Root Cause:** CSS class name mismatch.

**File:** `app/pages/scheduler/scheduler.component.html` line 194 (uses `class="dock-option"`)
**File:** `app/pages/scheduler/scheduler.component.scss` line 485 (defines `.berth-option`)

The HTML template uses:
```html
<button class="dock-option" ...>
```

But the SCSS defines:
```scss
.berth-option { ... }
```

Similarly, line 196 uses `class="dock-icon"` but the SCSS defines `.berth-icon`.

**Fix:** In `scheduler.component.scss`, rename:
- `.berth-option` → `.dock-option` (lines 485, 501, 505-506, 512, 517)
- `.berth-icon` → `.dock-icon` (line 522)

---

## Bug 4 — Scheduler Timeline: Dock Labels Unstyled

**Symptom:** The dock name labels in the timeline grid header column have no styling (no background, no padding, no font).

**Root Cause:** CSS class name mismatch.

**File:** `app/pages/scheduler/scheduler.component.html` line 226 (uses `class="timeline-dock-label"`)
**File:** `app/pages/scheduler/scheduler.component.scss` line 770 (defines `.timeline-berth-label`)

**Fix:** In `scheduler.component.scss`, rename `.timeline-berth-label` to `.timeline-dock-label` at line 770.

---

## Bug 5 — Operator Page: Hero Section Uses Wrong Font

**Symptom:** The "Ship Operations" hero heading uses a monospace font instead of the display font used by all other pages.

**File:** `app/pages/operator/operator.component.scss` line 47

```scss
.operator-hero h1 {
  font-family: var(--font-mono);  /* BUG: should be var(--font-display) */
```

**Fix:** Change `var(--font-mono)` to `var(--font-display)` on line 47.

---

## Bug 6 — Dashboard Page: Hero Section Missing Visual Depth

**Symptom:** The Dashboard hero banner appears flatter than other page heroes — it lacks the `box-shadow`, `position: relative`, and `overflow: hidden` that give other heroes their depth effect.

**File:** `app/pages/dashboard/dashboard.component.scss` lines 10–16

The `.dashboard-hero` rule is missing:
```scss
box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.06), 0 18px 50px rgba(0, 0, 0, 0.24);
position: relative;
overflow: hidden;
```

**Fix:** Add these three properties to `.dashboard-hero` to match the pattern used in `scheduler.component.scss` (`.assignments-hero`) and `berths.component.scss` (`.planning-hero`).

---

## Files to Modify (Summary)

| File | Changes |
|------|---------|
| `frontend/octopus-ui/src/app/pages/berths/berths.component.scss` | Rename `.berth-label` → `.dock-label`, `.berth-track` → `.dock-track` |
| `frontend/octopus-ui/src/app/pages/scheduler/scheduler.component.scss` | Rename `.berth-option` → `.dock-option`, `.berth-icon` → `.dock-icon`, `.timeline-berth-label` → `.timeline-dock-label` |
| `frontend/octopus-ui/src/app/pages/scheduler/scheduler.component.scss` | Fix timeline ship name overflow (see Bug 2) |
| `frontend/octopus-ui/src/app/pages/operator/operator.component.scss` | Fix hero font-family |
| `frontend/octopus-ui/src/app/pages/dashboard/dashboard.component.scss` | Add missing hero box-shadow/position/overflow |

---

## Constraints & Practices

1. **Create a new branch** before making changes: `fix/frontend-visual-bugs`
2. **Atomic commits**: group related changes into logical commits:
   - Commit 1: Fix planning calendar class name mismatch (Bug 1)
   - Commit 2: Fix scheduler class name mismatches (Bugs 3 + 4)
   - Commit 3: Fix timeline ship name overflow (Bug 2)
   - Commit 4: Fix hero section inconsistencies (Bugs 5 + 6)
3. **Do not change any TypeScript logic** — these are purely CSS/template fixes
4. **Do not rename classes in the HTML templates** — the HTML uses the correct "dock" naming; the SCSS is outdated with "berth" naming
5. **Verify** the app still builds: `cd frontend/octopus-ui && npx ng build`
6. **Visual verification**: start the dev server and check all pages render correctly
