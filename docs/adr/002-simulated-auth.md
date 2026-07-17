# ADR 002: Simulated Authentication via Role Selection

## Status

Accepted

## Context

The spec requires two user roles — **Operator** and **Scheduler** — with different dashboards and capabilities. However, the spec explicitly states: *"Simple role selection page — pick 'Operator' or 'Scheduler', stored in localStorage. No real login system."*

The team initially implemented a full username/password authentication system with login/register forms, guest login, and "Remember me" toggle. This was identified as a spec gap during review.

## Decision

Replace the full auth system with a **two-button role selection page**:

- Login page shows two role cards: "Operator" and "Scheduler"
- Clicking a role stores the choice in `localStorage` under key `role`
- Angular route guards check `localStorage.getItem('role')` to enforce access
- No backend authentication endpoints — no JWT, no passwords, no sessions
- The `AuthService` is simplified to a thin wrapper around `localStorage`

## Consequences

**Positive:**
- Matches the spec exactly — no over-engineering
- Zero security overhead — no token management, refresh flows, or password hashing
- Instant demo start — evaluators pick a role and see the dashboard immediately
- Simpler codebase — fewer files, fewer edge cases

**Negative:**
- No real security — anyone can open DevTools and change their role
- No audit trail of who performed actions
- Cannot distinguish between different operators or schedulers

**Neutral:**
- If real auth is needed later, the role guard pattern stays the same — only the storage mechanism changes (localStorage → JWT claim)
- The backend has no auth middleware, so all API endpoints are accessible regardless of frontend role — this is intentional per spec
