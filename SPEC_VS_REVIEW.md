# Spec vs. Review — Gap Comparison

> **Date**: 2026-07-15
> **Sources**: `docs/architecture-overview.md`, `docs/plan/masterplan.md`, `docs/GAP_ANALYSIS.md` (prior audit from 2026-06-25), `PROJECT_REVIEW.md` (today's review)
> **Purpose**: Cross-reference the senior PM review against the project spec to identify what's spec violation vs. what's engineering quality concern.

---

## How to Read This

- **SPEC GAP** = The spec requires it, but the implementation doesn't have it (or has it wrong)
- **QUALITY GAP** = The spec doesn't mandate it, but good engineering practice requires it
- **OVER-ENGINEERED** = The implementation adds something the spec explicitly says NOT to do
- **ALREADY FLAGGED** = The prior GAP_ANALYSIS.md already identified this (note date — has it been fixed?)

---

## 1. Security — Review Grade: F

| Finding | Spec Says | Verdict | Already Flagged? |
|---------|-----------|---------|-----------------|
| No JWT tokens issued | Spec says **"No real login system"** — simple role selection with localStorage. Auth is explicitly out of scope. | **OVER-ENGINEERED** — The team built a full username/password auth system (AuthService, AuthController, AppUser model, password hashing) that the spec explicitly says not to build. The "F" grade for missing JWT is unfair — the spec doesn't want JWT at all. | Yes (2.1 in GAP_ANALYSIS) |
| No `[Authorize]` attributes | Spec doesn't require endpoint protection | **QUALITY GAP** — Not a spec violation, but if you build auth, you should use it | No |
| CORS wide open | Spec doesn't specify CORS policy | **QUALITY GAP** — Fine for dev, not for production | No |
| Hardcoded admin/guest creds | Spec says no real login. Credentials are part of the over-engineered auth. | **OVER-ENGINEERED** — Should be removed entirely per spec | Yes (1.12 in GAP_ANALYSIS) |

**Verdict**: The security "F" is misleading. The spec says "no real auth." The team over-engineered auth and then didn't finish it. The correct fix is to **delete AuthService, AuthController, AppUser** and replace with a simple role picker (localStorage), which is what the spec actually wants.

---

## 2. Ship Creation — Review Doesn't Call Out, Spec Makes This CRITICAL

| Finding | Spec Says | Verdict | Already Flagged? |
|---------|-----------|---------|-----------------|
| Operator manually enters Size, ArrivalDay, Duration | Spec: "System auto-assigns: random dimension, random arrival day, random duration. Operator enters: ship name, notes." | **SPEC GAP — CRITICAL** | Yes (1.1 in GAP_ANALYSIS) — **NOT FIXED** |
| CreateShipRequest has Size, ArrivalDay, Duration as required fields | Spec: DTO should only have Name + Notes | **SPEC GAP — CRITICAL** | Yes — **NOT FIXED** |
| Duration validation is [Range(1,90)] | Spec: Duration is 3–15 days | **SPEC GAP** | Yes (1.10 in GAP_ANALYSIS) — **NOT FIXED** |

**Verdict**: This is the single biggest spec violation. The review doesn't even mention it. The Operator role is supposed to just type a name and notes — the system generates everything else.

---

## 3. API Endpoints — Review Grade: C+

| Endpoint | Spec | Implementation | Verdict |
|----------|------|---------------|---------|
| `PUT /api/ships/{id}` | Required (edit name/notes, Pending only) | **MISSING** — `ShipService.Update()` exists but no controller action | **SPEC GAP — CRITICAL** |
| `GET /api/ships/{id}/suggest` | Spec route: `/suggest` | Implementation: `/suggestion` | **SPEC GAP — MEDIUM** |
| `POST /api/docks/{id}/assign` | Spec: dock ID in URL, shipId in body | Implementation: `POST /api/assignments` with both IDs in body | **SPEC GAP — MEDIUM** |
| `GET /api/terminal/day` | Required | Exists ✓ | OK |
| `POST /api/terminal/next-day` | Required | Exists ✓ | OK |
| `GET /api/ships` | Required | Exists ✓ | OK |
| `POST /api/ships` | Required (auto-generate) | Exists but wrong contract | **SPEC GAP** |
| `DELETE /api/ships/{id}` | Spec says **"No deletion of ships"** | Implementation has DELETE | **OVER-ENGINEERED** |
| `SystemController` endpoints | Not in spec — only TerminalController is specified | Duplicate functionality | **OVER-ENGINEERED** |
| `AuthController` endpoints | Not in spec | Full login/register | **OVER-ENGINEERED** |
| `GET /api/assignments` | Not in spec | Exists | Extra (harmless) |

**Verdict**: The review correctly identifies the duplicate controller and missing PUT. But it misses that DELETE is spec-violating and that the assign route format is wrong per spec.

---

## 4. Error Handling — Review Grade: D+

| Finding | Spec Says | Verdict | Already Flagged? |
|---------|-----------|---------|-----------------|
| Missing ValidationFilter | Spec explicitly requires `ValidationFilter` (IActionFilter) | **SPEC GAP — CRITICAL** | Yes (1.3 in GAP_ANALYSIS) — **NOT FIXED** |
| Missing ExceptionFilter | Spec explicitly requires `ApiExceptionFilter` (IExceptionFilter) | **SPEC GAP — CRITICAL** | Yes (1.3) — **NOT FIXED** |
| Wrong ApiError structure | Spec: `{ statusCode: int, message: string, errors: { field: [messages] } }`. Implementation: `{ code: string, message: string }` | **SPEC GAP — HIGH** | Yes (1.4) — **NOT FIXED** |
| Result<T> defined but unused | Spec requires it for business errors | **SPEC GAP — HIGH** | Yes (1.12) — **NOT FIXED** |
| GetCurrentDay() crash bug | Not in spec, but a quality issue | **QUALITY GAP** | No (new finding) |

**Verdict**: The review correctly identifies the error handling problems. The spec is very explicit about filters and ApiError format — these are required deliverables.

---

## 5. Data Model — Review Grade: C

| Finding | Spec Says | Verdict | Already Flagged? |
|---------|-----------|---------|-----------------|
| 10 docks (2XL, 2L, 2M, 4S) | Spec: 8 docks (1XL, 1L, 2M, 4S) | **SPEC GAP — HIGH** | Yes (1.5) — **NOT FIXED** |
| `Berth.cs` dead model | Not in spec | **QUALITY GAP** (dead code) | Yes (1.12) — **NOT FIXED** |
| No database indexes | Not in spec | **QUALITY GAP** | No |
| TerminalState singleton without constraint | Not in spec detail | **QUALITY GAP** | No |
| Role is a plain string | Not in spec | **QUALITY GAP** | No |
| No cascade delete config | Not in spec | **QUALITY GAP** | No |
| Ship-ArrivalDay must be >= CurrentDay | Spec: "Arrival day must be ≥ current virtual day" | Not validated in service | **SPEC GAP** |

**Verdict**: The dock count is a clear spec violation. The review correctly identifies the dead Berth model.

---

## 6. Scheduling Algorithm — Review Doesn't Evaluate, Spec Makes This CORE

| Finding | Spec Says | Verdict | Already Flagged? |
|---------|-----------|---------|-----------------|
| Best-fit (smallest dock first) | Spec: **First-fit greedy** — scan docks in order, first that fits | **SPEC GAP — HIGH** | Yes (1.6 in GAP_ANALYSIS) — **NOT FIXED** |
| No planning horizon check | Spec: `CanAssign()` must check `finalEnd <= maxDay` (CurrentDay + PlanningHorizon) | Not verified | **SPEC GAP** |
| No "ship already arrived" check | Spec: `if (ship.ArrivalDay < terminal.CurrentDay) return (false, -1)` | Not verified | **SPEC GAP** |
| Race condition in AssignShip | Spec: "Assignment endpoint uses database transactions" | No transaction wrapping | **SPEC GAP — HIGH** | Yes (1.11) — **NOT FIXED** |
| No concurrency protection | Spec: Use `BeginTransactionAsync()` | Implementation: plain `SaveChanges()` | **SPEC GAP** |

**Verdict**: The review doesn't evaluate the scheduling algorithm at all — this is the heart of the application per the spec. The first-fit vs best-fit issue and missing transactions are critical spec violations.

---

## 7. Frontend — Review Grade: C-

| Finding | Spec Says | Verdict | Already Flagged? |
|---------|-----------|---------|-----------------|
| Full login page with username/password | Spec: Simple role selection (Operator/Scheduler) | **OVER-ENGINEERED** | Yes (2.1) — **NOT FIXED** |
| Operator page is "Berth Operations Center" | Spec: Operator = ship creation form + ship list | **SPEC GAP — CRITICAL** | Yes (2.2) — **NOT FIXED** |
| No timeline on Scheduler page | Spec: Scheduler = pending ships + timeline + assign | **SPEC GAP — HIGH** | Yes (2.3) — **NOT FIXED** |
| No ship editing | Spec: Operator can edit name/notes of Pending ships | **SPEC GAP — CRITICAL** | Yes (2.4) — **NOT FIXED** |
| Missing shared UI components | Spec: LoadingSpinner, ErrorBanner, Toast, SkeletonRows | **SPEC GAP — HIGH** | Yes (2.5) — **NOT FIXED** |
| No role-based route guards | Spec: Operator and Scheduler see different pages | **SPEC GAP — MEDIUM** | Yes (2.6) — **NOT FIXED** |
| Sidebar broken links | Not in spec | **QUALITY GAP** | Yes (2.7) — **NOT FIXED** |
| Client-side suggestion logic | Spec: Backend provides suggestion endpoint | **SPEC GAP — MEDIUM** | Yes (2.8) — **NOT FIXED** |
| Italian text ("Tutte") | Not in spec | **QUALITY GAP** | Yes (2.10) — **NOT FIXED** |
| Frontend calls nonexistent `/api/ships/pending` | Spec: `GET /api/ships?status=Pending` | **QUALITY GAP** | No (new finding) |

**Verdict**: The review identifies most frontend issues correctly. The spec violations here are severe — the Operator and Scheduler pages don't match the spec at all.

---

## 8. Test Coverage — Review Grade: B-

| What | Spec Says | Verdict |
|------|-----------|---------|
| 87 tests added | Spec: "xUnit tests for scheduling algorithm" required | Tests cover services/controllers but **NOT the scheduling algorithm specifically** |
| No tests for auto-generation | Spec requires auto-generation | Can't test what doesn't exist |
| No frontend tests | Spec says skip frontend tests | OK per spec |
| E2E lifecycle test | Spec: "Manual E2E checklist" required | We have automated E2E tests — exceeds spec |

**Verdict**: The test suite is good engineering but doesn't test the spec's core requirement (scheduling algorithm) because the algorithm implementation deviates from spec.

---

## 9. Configuration & Deployment — Review Grade: D

| Finding | Spec Says | Verdict |
|---------|-----------|---------|
| Hardcoded connection string | Spec: Use `appsettings.Development.json` | **SPEC GAP** |
| Docker SDK mismatch (.NET 8 vs net10.0) | Spec: .NET 8 | **QUALITY GAP** (project upgraded to net10.0 without updating Docker) |
| Migrate/seed runs in all environments | Spec: Only in Development | **SPEC GAP** | Yes (1.9) — **NOT FIXED** |
| No health check | Not in spec | **QUALITY GAP** |

---

## 10. Architecture Documentation — NOT REVIEWED, SPEC REQUIRES IT

| Deliverable | Spec Requires | Status |
|-------------|---------------|--------|
| `ARCHITECTURE.md` with C4-lite diagrams | Yes | **MISSING** |
| `DATA_MODEL.md` with ER diagram | Yes | **MISSING** |
| 3 ADRs (SQLite, Simulated Auth, Virtual Time) | Yes | **MISSING** |
| Presentation slides (5-6 slides) | Yes | `presentation.pptx` exists but not reviewed |
| Clean README with setup instructions | Yes | Exists but not verified |

**Verdict**: The review doesn't evaluate documentation deliverables at all. The spec explicitly requires them.

---

## Summary: What the Review Got Right vs. Wrong

### Review Got Right ✅
- Dead code identification (Berth.cs, unused DTOs, Result<T>)
- Naming chaos (Berth vs Dock)
- Error handling gaps (crash bug, no global handler)
- Frontend-backend mismatches (broken routes, hardcoded values)
- Missing CRUD operations (PUT ships, DELETE assignments)
- Duplicate controllers (System + Terminal)

### Review Got Wrong / Missed ❌
- **Security grade "F" is misleading** — spec says "no real auth," team over-engineered it. Grade should be "over-engineered, not incomplete."
- **Didn't evaluate scheduling algorithm** — this is the CORE of the app per spec. First-fit vs best-fit is a spec violation.
- **Didn't catch ship auto-generation** — the #1 CRITICAL spec gap (Operator should only enter name+notes)
- **Didn't catch spec-required filters** — ValidationFilter and ExceptionFilter are explicit deliverables
- **Didn't evaluate spec-required documentation** — ARCHITECTURE.md, DATA_MODEL.md, ADRs are missing
- **Didn't catch DELETE ships is spec-violating** — spec says "no deletion of ships"
- **Didn't catch transaction requirement** — spec explicitly requires database transactions for assignments
- **Didn't evaluate dock count** — 10 docks vs spec's 8

---

## Priority: What Actually Costs Evaluation Points

Per the spec's evaluation criteria: "Correctness, clarity, and architectural quality. Simplicity and coherence preferred over complexity."

### Will Lose Points For Sure (CRITICAL spec violations)

| # | Gap | Effort |
|---|-----|--------|
| 1 | Ship creation should auto-generate size/arrival/duration | 2h |
| 2 | Add PUT /api/ships/{id} for editing Pending ships | 1h |
| 3 | Simplify login to role selection (delete AuthService) | 3h |
| 4 | Move ship form+list into Operator page | 3h |
| 5 | Add timeline view to Scheduler page | 2h |
| 6 | Add ValidationFilter + ExceptionFilter + fix ApiError | 2h |
| 7 | Add ship editing UI (Pending only) | 3h |

### Will Lose Points Likely (HIGH spec violations)

| # | Gap | Effort |
|---|-----|--------|
| 8 | Fix dock count to 8 (1XL, 1L, 2M, 4S) | 15m |
| 9 | Fix scheduling to first-fit | 1h |
| 10 | Add database transactions to assignment | 1h |
| 11 | Create shared UI components (Spinner, ErrorBanner, Toast) | 4h |
| 12 | Wire frontend suggestion to backend endpoint | 30m |

### Won't Lose Points But Should Fix (engineering quality)

| # | Gap | Effort |
|---|-----|--------|
| 13 | Fix GetCurrentDay() crash | 5m |
| 14 | Remove dead code (Berth.cs, unused DTOs) | 15m |
| 15 | Fix naming (Berth → Dock in frontend) | 1h |
| 16 | Fix sidebar broken links + hardcoded stats | 1h |
| 17 | Write ARCHITECTURE.md + DATA_MODEL.md | 4h |

---

## Bottom Line

The review gave a **C-** overall, but that grade weighs security heavily (which is over-engineered per spec). A spec-compliance grade would be closer to **D+** — the core flows work but the implementation deviates from the spec in fundamental ways:

1. **Operator role doesn't work as specified** (manual fields instead of auto-generation)
2. **Scheduler role is incomplete** (no timeline, no editing)
3. **Error handling infrastructure is missing** (filters, ApiError format)
4. **Scheduling algorithm is wrong** (best-fit instead of first-fit)
5. **Auth is over-engineered** (full login system instead of role picker)
6. **Required documentation is missing**

The good news: most fixes are 1-3 hours each. The total effort to reach spec compliance is roughly **25-30 hours** of focused work.
