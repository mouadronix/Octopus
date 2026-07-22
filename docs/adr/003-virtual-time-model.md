# ADR 003: Virtual Time Model

## Status

Accepted

## Context

The terminal simulation needs a controllable time dimension. Ships arrive on specific days, assignments span day ranges, and the scheduler needs to advance time to process departures and free up dock slots. Using real system time would make the demo non-deterministic and impossible to test.

The spec describes a "virtual clock" that the scheduler advances by clicking a "Next Day" button.

## Decision

Implement a **virtual time model** using a singleton `TerminalState` entity:

- `CurrentDay` (int) — the current simulation day, starting at 1
- `PlanningHorizon` (int) — how far ahead scheduling is allowed (default: 30 days)
- `POST /api/terminal/next-day` — increments `CurrentDay` by 1
- `GET /api/terminal` — returns the current state

All scheduling logic uses `CurrentDay` instead of `DateTime.Now`:
- Ship arrival: `ArrivalDay` is an absolute virtual day
- Assignment window: `[StartDay, EndDay]` based on virtual days
- Scheduling constraint: `StartDay >= max(ArrivalDay, CurrentDay)`
- Planning horizon: `StartDay + Duration - 1 <= CurrentDay + PlanningHorizon`

## Consequences

**Positive:**
- Deterministic — same inputs always produce the same schedule
- Demo-friendly — evaluator can step through days and see assignments change
- Testable — unit tests can set `CurrentDay` to any value
- Simple — one integer column, no timezone complexity
- Spec-compliant — matches the described "Next Day" button behavior

**Negative:**
- No real-time events — nothing happens automatically when time passes
- No persistence of "what happened on day X" — only current state is stored
- Must manually advance time — no cron job or scheduler to auto-advance

**Neutral:**
- The virtual day is just an integer counter — it has no relationship to calendar dates
- `POST /api/terminal/next-day` could be extended to auto-expire assignments that have passed their `EndDay`, changing ship status to `Departed`
- The planning horizon acts as a sliding window — assignments beyond `CurrentDay + 30` are rejected
