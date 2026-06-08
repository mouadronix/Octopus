# OCTOPUS

Octopus is a port operations starter architecture with an Angular frontend and a .NET Web API backend.

## Structure

- `frontend/octopus-ui` - Angular UI for operators, scheduling, ships, and berths.
- `backend/Octopus.Api` - ASP.NET Core API with controllers, models, services, and seed data.
- `database` - Local SQLite database location and database notes.
- `docs` - Architecture and data model documentation.
- `.github` - Pull request template and CI workflow.

## Sprint 1 Scope

- GitHub repository workflow and branch guidance.
- Project architecture and documentation.
- ASP.NET Core API with Swagger, CORS, and dependency injection.
- EF Core SQLite database context and first migration.
- Ship, Berth, Assignment, and SystemState models.

## Quick Start

### Backend

```powershell
cd backend/Octopus.Api
dotnet restore
dotnet run
```

The API listens on `http://localhost:5000` by default.

Swagger is available at `http://localhost:5000/swagger` in development.

### Frontend

```powershell
cd frontend/octopus-ui
npm install
npm start
```

The UI listens on `http://localhost:4200`.

### Docker

```powershell
docker compose up --build
```
