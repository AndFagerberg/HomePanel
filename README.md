# HomePanel

HomePanel är en touchbaserad informationspanel för hemmet. En Linux-server kör ASP.NET Core-API:t och hämtar extern data, medan en Raspberry Pi Zero 2 W visar Angular-frontend i Chromium kiosk mode på en 10,1-tums pekskärm (`1024x600`) ansluten med HDMI och USB.

Panelen är avsedd att visa bland annat tid, väder, kollektivtrafik, kalender, schema och timers. Raspberry Pi:n är en tunn klient: den bygger inte frontend och anropar inga externa tjänster.

## Status

Den grundläggande dashboarden är implementerad. Väder hämtas från SMHI och kollektivtrafik från Trafiklab via backendens abstraherade tjänster. Frontend har vyer för startsida, väder och avgångar; kalender, schema, timers och musik utvecklas vidare enligt projektplanen.

## Arkitektur

Backend följer Clean Architecture med beroenden inåt:

```text
src/
├── HouseholdPanel.Domain/
├── HouseholdPanel.Application/
├── HouseholdPanel.Infrastructure/
└── HouseholdPanel.Api/

frontend/                         # Angular standalone-app
tests/                            # Enhets- och integrationstester
deploy/                           # Docker, Linux-server och Raspberry Pi
```

All insamling från externa källor sker på servern. API:t exponerar ett presentationsorienterat dashboard-endpoint på `GET /api/dashboard`, som frontend hämtar via servern.

## Lokal utveckling

Kör API:t från repositoryts rot:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/HouseholdPanel.Api
```

API:t lyssnar normalt på `http://localhost:5188`. Starta sedan Angular-appen i ett separat terminalfönster:

```bash
cd frontend
npm install
npm start
```

Utvecklingsservern finns på `http://localhost:4200` och vidarebefordrar `/api` till API:t. För frontendtester och produktionsbuild används `npm test` respektive `npm run build`.

## Drift

Docker bygger frontend och API i samma container:

```bash
docker compose up -d --build
```

Applikationen blir då tillgänglig på `http://localhost:8080`. Raspberry Pi:n ansluter endast till denna adress, eller ett stabilt lokalt hostname för servern, och startar Chromium automatiskt i kiosk mode.

## Dokumentation

- [PROJECT.md](PROJECT.md) - arkitektur, API-kontrakt och implementationsplan.
- [FRONTEND.md](FRONTEND.md) - GUI-specifikation för den 10,1-tums stora touchpanelen.
- [frontend/README.md](frontend/README.md) - Angular-kommandon och frontendutveckling.
- [deploy/README.md](deploy/README.md) - översikt av driftsättning för Linux-server och Raspberry Pi.
- [deploy/linux-server-no-docker.md](deploy/linux-server-no-docker.md) - installation på Linux-server utan Docker.
- [deploy/raspberry-pi/SETUP.md](deploy/raspberry-pi/SETUP.md) - stegvis installation av Raspberry Pi med HDMI-bild och USB-touch.
- [.github/copilot-instructions.md](.github/copilot-instructions.md) - kod- och arkitekturregler för Copilot i repositoryt.
