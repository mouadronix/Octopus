# Octopus

Octopus is a port terminal simulation. It models a small harbour where cargo ships arrive on scheduled days, get assigned to docks by size, and eventually depart. The whole thing runs on a virtual clock — you click "Next Day" to advance time instead of waiting for actual days to pass.

We built this as a school project (ITS Learning by Project). It's not meant for production — it's meant to show we understand how to wire up a full-stack app with a real domain model behind it.

## What it actually does

There are two roles: **Operator** and **Scheduler**. You pick one on the login page (no passwords, just a role button — it's simulated auth).

**Operator** sees the ship registry. They can register new ships (name and notes only — size, arrival day, and duration get auto-generated), edit ships that are still "Pending", and see which ships are docked where.

**Scheduler** sees the scheduling board. They pick a pending ship, the system suggests the best available dock based on a first-fit greedy algorithm, and they confirm the assignment. There's also a planning calendar that shows a timeline view of all dock assignments.

The simulation clock drives everything. Ships arrive on specific virtual days. When you advance the day, ships whose assignment has ended automatically get marked as "Departed". There's a planning horizon concept (30 days) but it only controls what the calendar view shows — the actual scheduling algorithm can place ships arbitrarily far out if needed.

## Tech stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend | Angular (standalone components) | 18.2 |
| Backend | ASP.NET Core Web API | .NET 10 (preview) |
| ORM | Entity Framework Core | 10.0.8 |
| Database | SQLite | (file-based, zero config) |
| API docs | Swagger / Swashbuckle | 6.5.0 |
| Testing | xUnit 2.9.0 + WebApplicationFactory 10.0.0 | — |
| Containerization | Docker Compose | — |

## Project layout

```
Octopus/
├── backend/
│   ├── Octopus.Api/              # The API project
│   │   ├── Controllers/          # ShipsController, BerthsController, TerminalController
│   │   ├── Services/             # Business logic (ShipService, DockService, etc.)
│   │   ├── Models/               # Domain entities (Ship, Dock, Assignment, TerminalState)
│   │   ├── DTOs/                 # Request/response shapes
│   │   ├── Data/                 # EF DbContext, repository, seed data
│   │   ├── Common/               # Filters, error handling
│   │   └── Migrations/           # EF Core migrations
│   └── Octopus.Api.Tests/        # Integration + unit tests (xUnit + WebApplicationFactory)
├── frontend/
│   └── octopus-ui/               # Angular app
│       └── src/app/
│           ├── pages/            # Route views: login, dashboard, operator, ships, scheduler, berths, new-ship
│           ├── components/       # UI components: layout/ (sidebar, header), operator/, shared/
│           ├── services/         # HTTP services that talk to the API
│           ├── models/           # TypeScript interfaces (Ship, Dock, Assignment, etc.)
│           └── guards/           # Route guard for role-based access
├── docs/
│   └── adr/                      # Architecture Decision Records (SQLite, auth, virtual time)
├── postman/                      # Postman collection for testing API endpoints
├── database/                     # Notes about the database setup
├── docker-compose.yml            # Spins up both API and UI containers
└── .gitignore
```

Note: the codebase uses "Berths" and "Docks" interchangeably. The controller class is `BerthsController` but the API route is `/api/docks`. The model is `Dock`. Just how it ended up.

## Prerequisites

- **Docker path:** Docker Desktop (or Docker Engine + Compose). That's it.
- **Local path:** .NET 10 preview SDK (`dotnet --version` should show 10.x) and Node.js 20+ (`node --version`).

## Getting it running

### Option 1: Docker (easiest)

```bash
docker compose up --build
```

This starts two containers:
- **API** on `http://localhost:5000` — .NET SDK image running `dotnet run`
- **UI** on `http://localhost:4200` — Node 20 image running `npm start`

The frontend container waits for the API healthcheck to pass before starting. The proxy is configured automatically via `set-proxy.js` — it writes `proxy.conf.json` with the Docker network target (`http://api:5000`).

### Option 2: Run locally

**Backend:**

```bash
cd backend/Octopus.Api
dotnet restore
dotnet run
```

The API starts on `http://localhost:5000`. Swagger UI is at `/swagger/index.html`. On first run in Development mode, it auto-migrates the database and seeds 35 ships, 8 docks, and 3 pre-existing assignments.

**Frontend:**

```bash
cd frontend/octopus-ui
npm install
node set-proxy.js
npm start
```

Angular dev server starts on `http://localhost:4200`. The `set-proxy.js` script writes `proxy.conf.json` to point `/api` requests at `http://localhost:5000` (the local API). The committed version of that file targets the Docker network (`http://api:5000`) which won't resolve on your host machine, so you need to run the script once after cloning.

### Option 3: Tests only

```bash
cd backend/Octopus.Api.Tests
dotnet test
```

Tests use an in-memory SQLite database via `WebApplicationFactory<Program>`, so they don't touch the real `.db` file.

## API endpoints

### Ships (`/api/ships`)

| Method | Path | What it does |
|--------|------|-------------|
| GET | `/api/ships` | List all ships (optional `?status=Pending` filter) |
| GET | `/api/ships/{id}` | Get one ship by ID |
| POST | `/api/ships` | Register a new ship (body: `{ name, notes }`) |
| PUT | `/api/ships/{id}` | Edit name/notes (Pending ships only) |
| GET | `/api/ships/{id}/suggest` | Get the best dock assignment suggestion |

### Docks (`/api/docks`)

| Method | Path | What it does |
|--------|------|-------------|
| GET | `/api/docks` | List all docks with their assignments |
| POST | `/api/docks/{dockId}/assign` | Assign a ship to a dock (body: `{ shipId }`) |
| GET | `/api/docks/assignments` | List all assignments across all docks |

### Terminal (`/api/terminal`)

| Method | Path | What it does |
|--------|------|-------------|
| GET | `/api/terminal/day` | Get the current virtual day |
| POST | `/api/terminal/next-day` | Advance the simulation by one day |

## The scheduling algorithm

The scheduler uses a **first-fit greedy** approach. Here's roughly how it works:

1. Filter docks by size compatibility — a dock can host a ship if the dock's size rank is >= the ship's size rank (S=1, M=2, L=3, XL=4).
2. For each compatible dock, scan existing assignments chronologically and find the earliest gap where the ship fits (arrival day or current day, whichever is later, through arrival + duration - 1).
3. Pick the dock with the earliest available slot. If there's a tie, prefer the dock with fewer total assignments.

This is a pure function (`SchedulingModule`) — no database calls, no side effects. The caller pre-fetches all the data and passes it in. Makes it easy to test and reason about.

## The virtual time model

Time is just an integer counter in the `TerminalState` table. No real-world clocks, no cron jobs, no timezone headaches.

- Ships have an `ArrivalDay` (absolute virtual day)
- Assignments span `[StartDay, EndDay]`
- `POST /api/terminal/next-day` increments `CurrentDay` and auto-departs ships whose assignment has ended
- There's a `PlanningHorizon` field (30 days) but it only controls the frontend calendar view — the scheduling algorithm itself has no hard cap

See `docs/adr/003-virtual-time-model.md` for why we did it this way.

## Database

SQLite, file-based. Connection string is in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OctopusDb": "Data Source=blueharbor.db"
  }
}
```

The `.db` file is gitignored. In Development mode, the app auto-migrates and seeds on startup, so you never need to run `dotnet ef database update` manually.

**Seed data:** 8 docks (1 XL, 1 L, 2 M, 4 S) and 35 ships spread across a 30-day window. Three ships come pre-assigned to docks so the dashboard isn't empty on first load.

## Auth model

There's no real authentication. The login page is a two-button role picker — "Operator" or "Scheduler". The chosen role goes into `localStorage` and the Angular route guard checks it to decide which pages you can access.

See `docs/adr/002-simulated-auth.md` — we originally built a full login/register system with passwords and JWT tokens, then realized the spec explicitly said "simple role selection page" and stripped it back.

The backend has no auth middleware at all. Every API endpoint is accessible regardless of what role the frontend thinks you have. That's on purpose — it's a demo, not a bank.

## Design decisions

We wrote ADRs (Architecture Decision Records) for the non-obvious choices:

- **ADR 001** — Why SQLite instead of PostgreSQL/SQL Server (`docs/adr/001-use-sqlite.md`)
- **ADR 002** — Why simulated auth instead of real JWT (`docs/adr/002-simulated-auth.md`)
- **ADR 003** — Why virtual time instead of real clocks (`docs/adr/003-virtual-time-model.md`)

## Contributing

Branch off `main`, make your changes, open a PR. The `migration` branch is where database schema changes live — don't merge that into `main` without checking it first.

```bash
git checkout -b feature/my-thing
# make changes
git add .
git commit -m "description of what changed"
git push origin feature/my-thing
# open PR on GitHub
```

## What's not here

- **Real auth** — no JWT, no passwords, no sessions
- **Real-time updates** — no WebSockets, no SignalR
- **Production deployment** — no CI/CD pipeline, no cloud hosting config
- **Pagination** — the ship list loads everything at once (fine for 35 ships, not fine for 35,000)
- **File uploads** — ship images are URL-based, no blob storage
