# API Documentation

Swagger is enabled in development at:

```text
http://localhost:5000/swagger
```

## Ships

- `GET /api/ships` - Returns all ships.
- `GET /api/ships/{id}` - Returns one ship.
- `POST /api/ships` - Creates a ship.

## Berths

- `GET /api/berths` - Returns all berths with assignments.
- `GET /api/berths/{id}` - Returns one berth with assignments.

## Assignments

- `GET /api/assignments` - Returns all assignments with ship and berth details.
- `POST /api/assignments` - Creates an assignment.

## System

- `GET /api/system/state` - Returns the persisted system state.
