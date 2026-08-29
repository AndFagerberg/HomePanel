# Household Panel

A digital information panel for the household: an ASP.NET Core backend running on a Linux server, and an Angular frontend displayed in Chromium kiosk mode on a Raspberry Pi Zero 2 W with a 3.5" touchscreen.

See [PROJECT.md](PROJECT.md) for the full architecture and design spec, and [.github/copilot-instructions.md](.github/copilot-instructions.md) for the rules Copilot follows in this repo.

## Structure

```text
household-panel/
├── src/                          # Backend (Clean Architecture)
│   ├── HouseholdPanel.Domain
│   ├── HouseholdPanel.Application
│   ├── HouseholdPanel.Infrastructure
│   └── HouseholdPanel.Api
├── tests/
│   ├── HouseholdPanel.UnitTests
│   └── HouseholdPanel.IntegrationTests
├── frontend/                     # Angular standalone app
├── deploy/                       # Docker & Raspberry Pi kiosk setup
├── docker-compose.yml
└── Dockerfile
```

## Current status

Fas 1 (minimal vertical slice) is implemented: `GET /api/dashboard` returns test data, and the Angular Home view displays it. Weather, transport, calendar and schedule are stubbed behind interfaces (`Application/Abstractions`) ready for Fas 2–4.

## Backend

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/HouseholdPanel.Api
```

The API listens on `http://localhost:5188` (see `launchSettings.json`) and exposes `GET /api/dashboard`.

## Frontend

```bash
cd frontend
npm install
npm start        # ng serve, proxies /api to http://localhost:5188
npm test
npm run build     # production build, output in frontend/dist/frontend/browser
```

## Docker

Builds the Angular app and the API into a single container that serves both:

```bash
docker compose up -d --build
```

Then browse to `http://localhost:8080`.

## Raspberry Pi

The Pi is a display-only appliance — it never runs backend logic or builds the frontend. See [deploy/README.md](deploy/README.md).
