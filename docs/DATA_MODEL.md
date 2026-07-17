# Data Model

## Entity-Relationship Diagram

```
┌──────────────────────┐       ┌──────────────────────┐
│        Ship          │       │        Dock           │
├──────────────────────┤       ├──────────────────────┤
│ Id          PK       │       │ Id          PK       │
│ Name        string   │       │ Name        string   │
│ Notes       string   │       │ Size        enum     │
│ Size        enum     │       │                      │
│ Status      enum     │       │                      │
│ ArrivalDay  int      │       │                      │
│ Duration    int      │       │                      │
└──────────┬───────────┘       └──────────┬───────────┘
           │ 1:1                          │ 1:N
           │                              │
           ▼                              ▼
┌──────────────────────────────────────────────────────┐
│                    Assignment                         │
├──────────────────────────────────────────────────────┤
│ Id          PK                                       │
│ ShipId      FK → Ship                                │
│ DockId      FK → Dock                                │
│ StartDay    int                                      │
│ EndDay      int                                      │
└──────────────────────────────────────────────────────┘

┌──────────────────────┐
│    TerminalState      │
├──────────────────────┤
│ Id          PK       │
│ CurrentDay  int      │
│ PlanningHorizon int  │
└──────────────────────┘
```

## Entities

### Ship

Represents a cargo ship requesting a dock slot at the terminal.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| Name | string | Ship name (entered by operator) |
| Notes | string | Optional notes (entered by operator) |
| Size | ShipSize | Auto-generated: S, M, L, or XL |
| Status | ShipStatus | Lifecycle state (default: Pending) |
| ArrivalDay | int | Auto-generated: virtual day the ship arrives |
| Duration | int | Auto-generated: days needed at dock (3–15) |

**Navigation:** `Assignment` (optional 1:1)

### Dock

Represents a physical berth at the terminal.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| Name | string | Dock identifier (e.g., "XL-01", "S-03") |
| Size | ShipSize | The size of ship this dock can host |

**Navigation:** `Assignments` (1:N)

### Assignment

Represents a ship scheduled at a dock for a time window.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| ShipId | int | FK → Ship |
| DockId | int | FK → Dock |
| StartDay | int | First day of the assignment |
| EndDay | int | Last day of the assignment (StartDay + Duration - 1) |

**Constraints:** One ship can have at most one assignment. A dock cannot have overlapping assignments.

### TerminalState

Singleton row holding the simulation clock.

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| CurrentDay | int | The current virtual day (default: 1) |
| PlanningHorizon | int | Max days ahead for scheduling (default: 30) |

## Enums

### ShipSize

| Value | Rank | Description |
|-------|------|-------------|
| S | 1 | Small |
| M | 2 | Medium |
| L | 3 | Large |
| XL | 4 | Extra Large |

### ShipStatus

| Value | Description |
|-------|-------------|
| Pending | Created, waiting for dock assignment |
| Assigned | Scheduled at a dock |
| Departed | Has left the terminal |

## Seed Data

| Entity | Count | Details |
|--------|-------|---------|
| Docks | 8 | 1× XL, 1× L, 2× M, 4× S |
| Ships | 18 | Mixed sizes and statuses |
| Assignments | 3 | Pre-assigned ships |
| TerminalState | 1 | CurrentDay = 12, PlanningHorizon = 30 |

## Business Rules

1. **Dock sizing:** A dock can only host ships of its own size (exact match: `dock.Size == ship.Size`).
2. **First-fit scheduling:** `GetSuggestion()` scans docks by ID order and returns the first dock where the ship fits without conflicts.
3. **Time window overlap:** Two assignments overlap if `a.StartDay <= b.EndDay && a.EndDay >= b.StartDay`.
4. **Ship lifecycle:** Pending → Assigned → Departed. Only Pending ships can be assigned.
5. **Duration:** Ships stay 3–15 days (validated by `[Range(3, 15)]`).
6. **Arrival day:** Auto-generated as `CurrentDay + random(0, 31)`.
7. **No deletion:** Ships cannot be deleted per spec requirements.
