# Octopus Data Model

## Ship

- `id` - Unique ship identifier.
- `name` - Vessel name.
- `notes` - Optional operator notes.
- `size` - `Small`, `Medium`, or `Large`.
- `arrivalDay` - Day the ship enters the system.
- `occupationDurationDays` - Number of days the ship needs a berth.
- `status` - `Pending`, `Assigned`, or `Departed`.

## Berth

- `id` - Unique berth identifier.
- `name` - Berth label.
- `size` - `Small`, `Medium`, or `Large`.

## Assignment

- `id` - Unique assignment identifier.
- `shipId` - Assigned ship.
- `berthId` - Assigned berth.
- `startDay` - First occupied day.
- `endDay` - Last occupied day.

## SystemState

- `id` - Unique state identifier.
- `currentDay` - Current simulation day. Initial value is `1`.
