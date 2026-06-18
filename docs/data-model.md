# Octopus Data Model

## Ship

- `id` - Unique ship identifier.
- `name` - Vessel name.
- `imoNumber` - External IMO-style identifier.
- `cargoType` - Cargo classification.
- `estimatedArrival` - Expected arrival time in UTC.
- `status` - Current operational status.

## Berth

- `id` - Unique berth identifier.
- `name` - Berth label.
- `maxDraftMeters` - Maximum supported vessel draft.
- `isAvailable` - Whether the berth is available.

## Assignment

- `id` - Unique assignment identifier.
- `shipId` - Assigned ship.
- `berthId` - Assigned berth.
- `startsAt` - Assignment start time in UTC.
- `endsAt` - Optional completion time.
- `status` - Assignment state.

## SystemState

- `environment` - Runtime environment name.
- `serverTimeUtc` - API server time.
- `shipCount` - Number of known ships.
- `berthCount` - Number of known berths.
- `activeAssignmentCount` - Assignments without an end time.
    