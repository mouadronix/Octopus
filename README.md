# OCTOPUS

Octopus is a port operations starter architecture with a modern React (TanStack Start + Tailwind CSS) frontend and an ASP.NET Core Web API backend.

## Structure

- `frontend/lovable-project-9b49f94a` - Enterprise Control Tower UI (React, TanStack, Tailwind CSS) for operators, scheduling, ships, berths, tides, and AIS.
- `backend/Octopus.Api` - ASP.NET Core API with controllers, models, services, and seed data.
- `database` - Local SQLite database location and database notes.
- `docs` - Architecture and data model documentation.

## Quick Start

### Backend

```powershell
cd backend/Octopus.Api
dotnet restore
dotnet run
```

The API listens on `http://localhost:5000` by default.

### Frontend

```powershell
cd frontend/lovable-project-9b49f94a
npm install
npm run dev
```

The UI listens on `http://localhost:3000` (or `http://localhost:5173`).

### Docker

```powershell
docker compose up --build
```

