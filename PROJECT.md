# Household Information Panel

## 1. Översikt

HomePanel är en digital informationspanel för hushållet.

Systemet består av:

- En Linux-server som kör backendlogik och datainsamling.
- En Raspberry Pi Zero 2 W som tunn displayklient.
- En 10,1" (1024 × 600) touchskärm.
- En Angular-frontend som körs i Chromium kiosk mode.

Målet är att skapa en lättanvänd hushållspanel. Frontendens konkreta GUI-, layout- och interaktionskrav finns i [FRONTEND.md](FRONTEND.md).

Exempel på information:

- Tid och datum
- Inomhustemperatur
- Väder och prognos
- Bussavgångar
- Kalender och schema
- Timers
- Musik
- Övrig hushållsinformation

## 2. Övergripande arkitektur

```text
                         INTERNET
                             │
              ┌──────────────┼──────────────┐
              │              │              │
             SMHI          TRAFIK        KALENDER
              │              │              │
              └──────────────┼──────────────┘
                             │
                             ▼
                 ┌──────────────────────┐
                 │     LINUX SERVER     │
                 │      ASP.NET Core    │
                 │                      │
                 │  Backend services    │
                 │  Background workers  │
                 │  Cache                │
                 │  REST API             │
                 └──────────┬───────────┘
                            │
                         LAN/WiFi
                            │
                            ▼
                 ┌──────────────────────┐
                 │ Raspberry Pi Zero 2 W│
                 │ Chromium kiosk       │
                 │ Angular frontend     │
                 │ 10,1" touchscreen    │
                 └──────────────────────┘
```

Raspberry Pi ska inte ansvara för datainsamling, externa API-anrop, databearbetning eller annan tung logik. Den ska starta Chromium, hämta färdiga DTO:er från backend och hantera presentation och touch.

## 3. Teknisk stack och arkitektur

Backend:

- C# och .NET 10
- ASP.NET Core Web API
- Entity Framework Core vid behov
- SQLite initialt om persistent lagring behövs
- Docker och Docker Compose

Backend följer Clean Architecture. Beroenden pekar inåt:

```text
src/
├── HouseholdPanel.Domain/
├── HouseholdPanel.Application/
├── HouseholdPanel.Infrastructure/
└── HouseholdPanel.Api/

tests/
├── HouseholdPanel.UnitTests/
└── HouseholdPanel.IntegrationTests/
```

Externa datakällor kapslas bakom interfaces i `Application/Abstractions`. Infrastrukturens implementationer kan därför bytas utan att ändra domän eller API-kontrakt.

## 4. Frontendgräns

Frontend byggs med Angular, TypeScript och standalone components. Angular Signals och enkla services räcker initialt.

Frontend byggs till statiska filer. Linux-servern ska kunna leverera filerna tillsammans med API:t eller via separat webbserver/container. Raspberry Pi ska efter build endast behöva Chromium.

All frontendens struktur, vyer, widgets, navigation, touchbeteende, design, animationer och prestandaregler dokumenteras i [FRONTEND.md](FRONTEND.md).

Frontend får aldrig anropa externa datakällor direkt och ska endast känna till frontendens presentationsorienterade DTO-modeller.

## 5. Raspberry Pi och Chromium

Raspberry Pi fungerar som en dedikerad appliance:

- Raspberry Pi OS
- automatisk login
- WiFi-anslutning
- automatisk start av Chromium
- kiosk mode
- automatisk omstart av Chromium vid krasch
- automatisk återanslutning till WiFi

Exempel:

```bash
chromium \
  --kiosk \
  --noerrdialogs \
  --disable-infobars \
  --disable-session-crashed-bubble \
  http://household-panel.local
```

Den exakta konfigurationen anpassas efter Raspberry Pi OS-versionen. Pi:n ska inte användas som utvecklingsmiljö.

## 6. Backend API

Frontend ska ha ett primärt dashboard-endpoint:

```http
GET /api/dashboard
```

API-kontraktet ska vara presentationsorienterat och inte exponera interna domänmodeller.

```json
{
  "timestamp": "2026-08-28T18:47:00+02:00",
  "weather": {
    "temperature": 19.0,
    "minimumTemperature": 12.0,
    "maximumTemperature": 20.0,
    "symbol": "cloudy",
    "precipitationProbability": 20,
    "windSpeed": 4.0
  },
  "indoor": {
    "temperature": 20.5,
    "humidity": 45
  },
  "transport": {
    "stopName": "XXXXX",
    "departures": [
      {
        "departure": "18:51",
        "destination": "Centrum",
        "line": "3",
        "minutes": 6
      }
    ]
  },
  "calendar": [
    {
      "start": "19:00",
      "title": "Middag"
    }
  ],
  "schedule": []
}
```

## 7. Datauppdatering och offline-hantering

Backend ska använda background services för att hämta och cacha extern information. Uppdateringsintervall ska vara konfigurerbara. Exempel:

- Väder: var 15:e minut
- Buss: varje minut
- Kalender: var 5:e minut
- Schema: exempelvis en gång per dag

Frontend kan hämta `/api/dashboard` var 30–60 sekund. Backend ska returnera cachad data så att frontend aldrig behöver vänta på externa API-anrop.

Backend och frontend ska behålla senast lyckade data vid tillfälliga nätverksproblem. Systemet ska kunna visa senast uppdaterad tid och diskret indikera `online`, `stale` eller `unavailable`.

## 8. Konfiguration och säkerhet

Konfiguration ska ligga i konfigurationsfiler eller environment variables. Secrets får aldrig ligga i Git.

Exempel på konfigurerbara värden:

```text
Weather:
  Locations

Transport:
  StopId
  Direction
  ApiKey

Calendar:
  Provider
  CalendarId

Dashboard:
  WeatherUpdateInterval
  TransportUpdateInterval
  CalendarUpdateInterval
```

Externa HTTP-anrop ska ha timeout, cancellation tokens och lämplig logging. API:t ska initialt endast vara tillgängligt på hemnätverket och ska inte exponeras direkt mot internet.

## 9. Docker och lokal åtkomst

Backend ska kunna köras med Docker och Docker Compose. En enkel produktionslösning är att ASP.NET Core serverar Angularens statiska filer.

Panelen ska nås via ett stabilt lokalt hostname, exempelvis:

```text
http://household-panel.local
```

Ett hostname eller DNS-namn föredras framför en hårdkodad IP-adress.

## 10. Logging och testning

Backend ska logga API-fel, timeouts, cacheuppdateringar, background worker-fel och systemstatus utan att fylla loggarna med högfrekventa meddelanden.

Backendtester ska omfatta:

- vädermappning
- transportmappning
- kalenderlogik
- cachelogik
- API till service till DTO

Frontendtester ska täcka relevant rendering, navigation, saknad data och offline/stale-status enligt kraven i [FRONTEND.md](FRONTEND.md).

## 11. Repository och utvecklingsmiljö

```text
household-panel/
├── src/
├── tests/
├── frontend/
├── deploy/
├── docker-compose.yml
├── Dockerfile
├── README.md
├── PROJECT.md
├── FRONTEND.md
└── .github/
    └── copilot-instructions.md
```

Utvecklingen sker i Visual Studio Code med Git och GitHub. Commits ska vara små och begripliga. Copilot ska följa repositoryts instruktioner och inte introducera nya frameworks eller patterns utan anledning.

## 12. Implementationsordning

1. Minimal vertical slice: backend, `GET /api/dashboard`, Angular och HomePanel på Raspberry Pi.
2. Väder via backend och cache.
3. Transport via backend och vald leverantör.
4. Kalenderintegration.
5. Timers med backend som definitiv källa för status och absoluta sluttider.
6. Musik och Spotify-integration.
7. Raspberry Pi appliance, watchdog och eventuell skärmsläckning.

Frontendens detaljerade genomförandeplan finns i [FRONTEND.md](FRONTEND.md). Automatisk vyrotation är inte ett grundkrav för den nya navigationsmodellen; dashboarden ska vara stabil och navigation ske via kort och navigationsmeny.

## 13. Framtida möjligheter

Arkitekturen ska göra det enkelt att lägga till:

- Home Assistant och MQTT
- fler temperaturgivare
- luftfuktighet och elförbrukning
- solceller och sophämtning
- post, paket och familjemeddelanden
- RSS eller nyheter
- flera kalendrar
- flera Raspberry Pi-paneler mot samma backend

## 14. Slutligt mål

När Raspberry Pi startar ska användaren inte behöva göra någonting:

```text
Power ON
   ↓
Linux startar
   ↓
WiFi ansluts
   ↓
Chromium startar
   ↓
Översikten laddas
   ↓
Navigation via dashboardkort eller navigationsmeny
```

Raspberry Pi ska vara så resurssnål som möjligt och huvudsakligen fungera som en tunn presentation- och touchklient. Backend ska kunna betjäna flera paneler utan att byggas specifikt kring en enda Raspberry Pi.