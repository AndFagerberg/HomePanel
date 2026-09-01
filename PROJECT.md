# Household Information Panel

## 1. Översikt

Detta projekt är en digital informationspanel för hushållet.

Panelen består av:

- En Linux-server som kör all backendlogik och datainsamling.
- En Raspberry Pi Zero 2 W som fungerar som ren displayklient.
- En 3,5" touchskärm ansluten till Raspberry Pi.
- En Angular-baserad frontend som körs i Chromium i kiosk mode.

Målet är att skapa ett snyggt, minimalistiskt och lättanvänt GUI anpassat specifikt för en liten 3,5"-skärm.

Informationen visas i flera vyer som automatiskt roterar.

Exempel på information:

- Klocka och datum
- Inomhustemperatur
- Utomhustemperatur
- Väder
- Väderprognos
- Nästa buss till stan
- Bussavgångar och eventuell realtidsinformation
- Kalender
- Schema
- Övrig hushållsinformation
- System-/nätverksstatus

Projektet ska vara utformat så att ytterligare informationskällor enkelt kan läggas till senare.

---

# 2. Övergripande arkitektur

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
                 │                      │
                 │    ASP.NET Core      │
                 │                      │
                 │  WeatherService      │
                 │  TransportService    │
                 │  CalendarService     │
                 │  ScheduleService     │
                 │                      │
                 │  Background Workers  │
                 │                      │
                 │     REST API         │
                 └──────────┬───────────┘
                            │
                         LAN/WiFi
                            │
                            ▼
                 ┌──────────────────────┐
                 │ Raspberry Pi Zero 2 W│
                 │                      │
                 │       Chromium       │
                 │     kiosk mode       │
                 │          │           │
                 │       Angular        │
                 │          │           │
                 │     3.5" touchscreen │
                 └──────────────────────┘
```

## Viktigt arkitekturbeslut

Raspberry Pi ska **inte** ansvara för datainsamling, API-anrop, databearbetning eller annan tung logik.

Pi:n ska endast:

1. Starta.
2. Ansluta till WiFi.
3. Starta Chromium i kiosk mode.
4. Visa Angular-applikationen.
5. Hämta färdig data från backend.
6. Hantera touch, sidväxling och presentation.

All tung logik ska ligga på Linux-servern.

---

# 3. Teknisk stack

## Backend

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core vid behov
- SQLite initialt om persistent lagring behövs
- Docker
- Docker Compose

Backend ska följa Clean Architecture kombinerat med vertical slice där det är lämpligt.

Rekommenderad struktur:

```text
src/
├── HouseholdPanel.Api/
├── HouseholdPanel.Application/
├── HouseholdPanel.Domain/
└── HouseholdPanel.Infrastructure/

tests/
├── HouseholdPanel.UnitTests/
└── HouseholdPanel.IntegrationTests/
```

Det är viktigt att inte överarkitektera projektet. Förhållandevis små features ska hållas enkla.

---

# 4. Frontend

Frontend ska byggas med:

- Angular
- TypeScript
- HTML
- CSS
- Angular standalone components
- Angular Signals där det är lämpligt

Frontend ska byggas till statiska filer.

Linux-servern ska kunna leverera frontendens statiska filer tillsammans med API:t eller via separat webbserver/container.

Raspberry Pi ska inte behöva:

- Node.js
- npm
- Angular CLI
- TypeScript compiler

Dessa används endast under utveckling/build.

Efter build ska Raspberry Pi endast behöva Chromium.

---

# 5. Raspberry Pi

Hårdvara:

- Raspberry Pi Zero 2 W
- 3,5" touchscreen
- WiFi
- MicroSD

Raspberry Pi ska köras som en appliance/dedikerad informationspanel.

Rekommenderad miljö:

- Raspberry Pi OS
- Chromium
- automatisk login
- automatisk start av Chromium
- kiosk mode
- automatisk återstart av Chromium vid krasch
- automatisk återanslutning till WiFi

Pi:n ska inte användas som utvecklingsmiljö.

All utveckling sker i Visual Studio Code på utvecklingsdatorn.

---

# 6. Chromium kiosk mode

Vid boot ska Raspberry Pi automatiskt starta Chromium mot dashboardens URL.

Exempel:

```text
http://household-panel.local
```

eller:

```text
http://192.168.x.x:8080
```

Den slutliga lösningen bör använda ett lokalt DNS-namn eller hostname istället för hårdkodad IP-adress.

Chromium ska köras utan adressfält, menyer eller andra browser-element.

Exempel:

```bash
chromium \
  --kiosk \
  --noerrdialogs \
  --disable-infobars \
  --disable-session-crashed-bubble \
  http://household-panel.local
```

Den exakta Chromium-konfigurationen ska anpassas efter Raspberry Pi OS-versionen.

---

# 7. Dashboardens design

GUI:t ska inte se ut som en vanlig webbapplikation.

Det ska se ut som en dedikerad fysisk informationspanel.

Designprinciper:

- minimalistiskt
- rent
- modernt
- hög läsbarhet
- stora siffror
- få element per vy
- inga scrollbars
- inga onödiga menyer
- touch-vänligt
- anpassat för 3,5"
- mörkt tema som grund
- diskreta animationer
- tydlig visuell hierarki

Skärmen ska användas i landscape om hårdvaran stödjer detta.

Frontendens layout ska vara specifikt anpassad för den faktiska skärmupplösningen.

Använd inte en generisk desktop-layout som skalas ner.

---

# 8. Informationsvyer

Panelen ska bestå av flera fullskärmsvyer.

Initialt föreslås:

## View 1 – Home

Visar:

- aktuell tid
- datum
- inomhustemperatur
- utomhustemperatur
- vädersymbol
- temperatur
- nästa buss
- nästa kalenderhändelse

Exempel:

```text
┌─────────────────────┐
│                     │
│       18:47         │
│   FREDAG 28 AUG     │
│                     │
│       21.4°         │
│        INNE         │
│                     │
│     ☁️  19°         │
│       VÄXJÖ         │
│                     │
│ ─────────────────── │
│ 🚌 Nästa buss       │
│    18:51  •  6 min  │
│                     │
│ 📅 Middag 19:00     │
└─────────────────────┘
```

---

# 9. View – Weather

Visar:

- aktuell temperatur
- vädersymbol
- min/max
- nederbörd
- vind
- kommande prognos

Exempel:

```text
┌─────────────────────┐
│       VÄDER         │
│                     │
│        ☁️           │
│        19°          │
│                     │
│     20° / 12°       │
│                     │
│   💧 20%            │
│   💨 4 m/s          │
│                     │
│ Lör  ☀️ 22°         │
│ Sön  🌦️ 18°         │
└─────────────────────┘
```

SMHI ska användas som primär svensk väderkälla om möjligt.

Backend ansvarar för kommunikation med SMHI.

Frontend ska aldrig anropa SMHI direkt.

---

# 10. View – Buss

Visar kommande bussavgångar.

Exempel:

```text
┌─────────────────────┐
│     🚌 TILL STAN    │
│                     │
│ 18:51   3    6 min  │
│ 19:21   3   36 min  │
│ 19:51   3   66 min  │
│                     │
│ Hållplats: XXXXX    │
└─────────────────────┘
```

Backend ansvarar för:

- hämtning av avgångar
- realtidsinformation
- filtrering
- sortering
- eventuell cache
- beräkning av återstående minuter

Frontend ska endast presentera resultatet.

Transportleverantör/API ska implementeras bakom ett interface så att datakällan kan bytas senare.

Exempel:

```csharp
public interface ITransportService
{
    Task<IReadOnlyList<Departure>> GetDeparturesAsync(
        CancellationToken cancellationToken);
}
```

---

# 11. View – Kalender

Kalendervyn ska visa:

- dagens händelser
- kommande händelser
- morgondagens händelser

Exempel:

```text
┌─────────────────────┐
│       📅 IDAG       │
│                     │
│ 19:00  Middag       │
│                     │
│ 20:30  Aktivitet    │
│                     │
│                     │
│      IMORGON        │
│ 08:00  ...          │
└─────────────────────┘
```

Kalenderintegration ska kapslas bakom ett interface:

```csharp
public interface ICalendarService
{
    Task<IReadOnlyList<CalendarEvent>> GetUpcomingEventsAsync(
        CancellationToken cancellationToken);
}
```

Det ska vara möjligt att senare stödja exempelvis:

- Microsoft 365
- Google Calendar
- CalDAV
- annan kalenderkälla

utan att ändra frontendens modell.

---

# 12. View – Schema

Schema är en separat informationskälla från kalendern.

Den kan exempelvis användas för:

- familjeschema
- arbetsschema
- veckoschema
- aktiviteter
- sophämtning
- andra återkommande händelser

Initial implementation kan vara enkel.

Schemat ska kunna konfigureras i backend.

---

# 13. Automatisk rotation

Vyerna ska automatiskt rotera.

Exempel:

```text
HOME
  ↓
WEATHER
  ↓
BUS
  ↓
CALENDAR
  ↓
SCHEDULE
  ↓
HOME
```

Normal visningstid:

- Home: 15 sekunder
- Weather: 10 sekunder
- Bus: 15 sekunder
- Calendar: 15 sekunder
- Schedule: 10 sekunder

Tiderna ska vara konfigurerbara.

---

# 14. Intelligent rotation

Rotationen ska senare kunna ta hänsyn till innehållet.

Exempel:

Om nästa buss går om två minuter:

```text
HOME
BUS
BUS
WEATHER
BUS
CALENDAR
```

Om en kalenderhändelse börjar snart kan kalendervyn prioriteras.

Detta ska implementeras i frontendens presentationslager och inte påverka backendens datamodell.

---

# 15. Touch

Touch ska stödja:

### Tryck

Tryck på skärmen:

```text
→ nästa vy
```

### Swipe

Swipe vänster:

```text
nästa vy
```

Swipe höger:

```text
föregående vy
```

### Långtryck

Långtryck kan senare användas för att:

- pausa rotation
- visa status
- öppna inställningar

Touchfunktionalitet ska implementeras med vanliga browser-events och inte kräva något tungt UI-framework.

---

# 16. Backend API

Frontend ska ha ett primärt dashboard-endpoint.

Exempel:

```http
GET /api/dashboard
```

Svar:

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

API-kontraktet ska vara ett presentationsorienterat DTO-kontrakt.

Frontend ska inte behöva förstå interna domänmodeller.

---

# 17. Datauppdatering

Backend ska använda Background Services för att hämta och cacha extern information.

Exempel:

```text
Weather
→ uppdateras var 15:e minut

Bus
→ uppdateras varje minut

Calendar
→ uppdateras var 5:e minut

Schedule
→ uppdateras exempelvis en gång per dag
```

Dessa intervall ska vara konfigurerbara.

Frontend ska exempelvis hämta `/api/dashboard` var 30–60 sekund.

Backend ska returnera cachad data.

Frontend ska aldrig behöva vänta på externa API-anrop.

---

# 18. Offline-hantering

Systemet ska tåla tillfälliga nätverksproblem.

Backend ska behålla senaste lyckade data.

Frontend ska också behålla senast kända dashboard-data.

Om servern inte kan nå externa API:er ska panelen fortfarande kunna visa:

```text
Senast uppdaterad:
18:32
```

Frontend ska tydligt kunna indikera:

- online
- backend unavailable
- data stale

Men statusindikeringen ska vara diskret.

---

# 19. Frontendstruktur

Rekommenderad Angular-struktur:

```text
src/
├── app/
│   ├── core/
│   │   ├── services/
│   │   ├── models/
│   │   └── api/
│   │
│   ├── dashboard/
│   │   ├── dashboard.component.ts
│   │   ├── dashboard.component.html
│   │   └── dashboard.component.css
│   │
│   ├── views/
│   │   ├── home/
│   │   ├── weather/
│   │   ├── transport/
│   │   ├── calendar/
│   │   └── schedule/
│   │
│   └── shared/
│       ├── components/
│       └── pipes/
│
├── assets/
└── styles.css
```

Undvik onödigt komplex state management.

Angular Signals och enkla services räcker initialt.

---

# 20. Frontend state

Dashboard-data ska ligga i en central service.

Exempel:

```typescript
@Injectable({
  providedIn: 'root'
})
export class DashboardService {
    private readonly dashboard = signal<Dashboard | null>(null);

    readonly data = this.dashboard.asReadonly();

    async refresh(): Promise<void> {
        // Fetch API and update signal
    }
}
```

Vyerna ska läsa från samma state.

Det ska inte finnas separata API-anrop från varje vy.

---

# 21. Prestanda

Raspberry Pi Zero 2 W har begränsade resurser.

Frontend ska därför följa dessa regler:

- minimera JavaScript
- minimera externa dependencies
- undvik stora UI-frameworks
- undvik stora bildbibliotek
- optimera CSS
- minimera DOM
- använd CSS-animationer sparsamt
- använd SVG för ikoner där lämpligt
- använd lokal font om möjligt
- inga stora bakgrundsbilder
- inga kontinuerliga CPU-intensiva animationer
- inga onödiga timers

Angular-applikationen ska byggas som production build.

Exempel:

```bash
ng build --configuration production
```

---

# 22. GUI-animationer

Animationerna ska vara diskreta.

Exempel:

```text
fade-out
    ↓
fade-in
```

eller en mycket kort slide-animation.

Animationer ska inte köras kontinuerligt.

Undvik:

- parallax
- partiklar
- avancerade canvas-animationer
- videobakgrunder
- stora CSS-effekter

Panelen ska kännas modern utan att kännas tung.

---

# 23. Ikoner

Använd i första hand:

- SVG
- CSS
- enkla lokala ikoner

Undvik att ladda stora ikonbibliotek i runtime om det inte behövs.

Väderikoner ska vara lokala assets.

---

# 24. Konfiguration

Backendens konfiguration ska ligga i configuration/environment variables.

Exempel:

```text
Weather:
  Latitude
  Longitude

Transport:
  StopId
  Direction

Calendar:
  Provider
  CalendarId

Dashboard:
  WeatherUpdateInterval
  TransportUpdateInterval
  CalendarUpdateInterval
```

Secrets ska aldrig ligga i Git.

---

# 25. Docker

Backend ska kunna köras med Docker.

Exempel:

```text
docker-compose.yml

services:

  household-panel:
    image: household-panel
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

Frontendens production-build ska kunna levereras av ASP.NET Core eller av en separat minimal webbserver-container.

En enkel lösning är att låta ASP.NET Core servera Angularens statiska filer.

---

# 26. Lokal åtkomst

Panelen ska nå backend via det lokala nätverket.

Exempel:

```text
http://household-panel.local
```

eller:

```text
http://server-ip:8080
```

Preferera ett stabilt hostname/DNS-namn framför en hårdkodad IP-adress.

Om möjligt ska servern ha en statisk DHCP lease.

---

# 27. Säkerhet

Eftersom panelen initialt endast används på hemnätverket kan API:t vara internt.

Extern exponering mot internet ska inte göras.

Backend ska ändå följa god praxis:

- inga secrets i Git
- HTTPS om praktiskt möjligt
- input validation
- timeout på externa HTTP-anrop
- cancellation tokens
- logging
- rate limiting där det behövs

Externa API-anrop ska alltid ha timeout.

---

# 28. Logging

Backend ska logga:

- lyckade API-anrop på lämplig nivå
- API-fel
- timeout
- cacheuppdateringar
- background worker-fel
- systemstatus

Logging ska inte fylla loggarna med meddelanden varje sekund.

---

# 29. Testning

Backend ska ha unit tests för:

- vädermappning
- transportmappning
- kalenderlogik
- cachelogik

Integration tests ska testa:

```text
API → service → DTO
```

Frontend ska åtminstone ha tester för:

- rendering av dashboard-data
- view rotation
- touch/swipe
- hantering av saknad data
- offline/stale status

---

# 30. Git-struktur

Repository:

```text
household-panel/
│
├── src/
├── tests/
├── frontend/
├── deploy/
├── docker-compose.yml
├── Dockerfile
├── README.md
├── PROJECT.md
├── .gitignore
└── .github/
    └── workflows/
```

Git ska användas från första dagen.

Commits ska vara små och begripliga.

Exempel:

```text
feat: add weather service
feat: add dashboard API
feat: add home view
feat: add automatic view rotation
feat: add transport departures
fix: handle unavailable weather API
```

---

# 31. Utvecklingsmiljö

Utvecklingen sker i:

- Visual Studio Code
- Git
- GitHub
- GitHub Copilot

GitHub Copilot ska användas som programmeringsassistent men projektets arkitektur och designbeslut ska dokumenteras i repositoryt.

Copilot ska följa befintlig arkitektur och inte introducera nya frameworks eller patterns utan anledning.

---

# 32. Copilot-instruktioner

Skapa exempelvis:

```text
.github/copilot-instructions.md
```

med projektets viktigaste regler.

Copilot ska följa dessa principer:

1. Följ befintlig arkitektur.
2. Undvik överengineering.
3. Håll frontend extremt lätt.
4. All extern datainsamling sker i backend.
5. Frontend ska aldrig anropa externa API:er direkt.
6. Använd dependency injection i backend.
7. Använd async/await.
8. Använd CancellationToken i backend-anrop.
9. Lägg secrets i configuration/environment variables.
10. Skriv tester för ny backendlogik.
11. Använd Angular standalone components.
12. Använd Signals där det förenklar state management.
13. Undvik onödiga dependencies.
14. Optimera för Raspberry Pi Zero 2 W.
15. GUI:t ska vara touch-first.
16. GUI:t ska vara anpassat för 3,5"-skärmen.
17. Inga scrollbars.
18. Undvik stora bilder och tunga animationer.
19. Håll komponenterna små och fokuserade.
20. Följ SOLID där det ger verkligt värde.
21. Använd tydliga namn.
22. Lägg inte affärslogik i controllers.
23. Lägg inte API-anrop i Angular-komponenter.
24. Frontend ska endast känna till frontend-DTO-modeller.
25. Externa datakällor ska kapslas bakom interfaces.

---

# 33. Första implementationen

Projektet ska byggas stegvis.

## Fas 1 – Minimal vertical slice

Bygg först:

```text
Linux server
    ↓
ASP.NET Core
    ↓
GET /api/dashboard
    ↓
Angular
    ↓
Home view
    ↓
Raspberry Pi
    ↓
Chromium kiosk
```

Dashboarden ska initialt returnera testdata.

Exempel:

```json
{
  "timestamp": "2026-08-28T18:47:00+02:00",
  "weather": {
    "temperature": 19
  },
  "indoor": {
    "temperature": 20.5
  },
  "transport": {
    "departures": []
  },
  "calendar": [],
  "schedule": []
}
```

När detta fungerar på den riktiga Raspberry Pi:n byggs funktionerna ut.

---

# 34. Fas 2 – Väder

Implementera:

```text
SMHI
 ↓
WeatherService
 ↓
cache
 ↓
Dashboard API
 ↓
Angular
 ↓
Weather View
```

---

# 35. Fas 3 – Transport

Implementera transportleverantör och hållplats.

Backend ska exponera ett generiskt transportobjekt och inte exponera leverantörens interna API-modell.

---

# 36. Fas 4 – Kalender

Implementera kalenderintegration.

Kalendern ska kunna bytas utan att frontend behöver ändras.

---

# 37. Fas 5 – Rotation och touch

Implementera:

- automatisk rotation
- swipe
- tap
- långtryck
- pausad rotation

---

# 38. Fas 6 – Raspberry Pi appliance

Konfigurera:

- Raspberry Pi OS
- WiFi
- Chromium
- kiosk mode
- autostart
- watchdog
- automatisk restart
- skärmrotation
- eventuell skärmsläckning

---

# 39. Fas 7 – Polish

När funktionaliteten fungerar ska fokus ligga på:

- typografi
- spacing
- ikoner
- animationer
- färger
- mörkt tema
- läsbarhet
- touch targets
- prestanda

Det visuella arbetet ska göras efter att den funktionella vertical slicen fungerar.

---

# 40. Framtida möjligheter

Arkitekturen ska göra det enkelt att senare lägga till:

- Home Assistant
- MQTT
- fler temperaturgivare
- luftfuktighet
- elförbrukning
- solceller
- sophämtning
- post/paket
- väckarklocka
- familjemeddelanden
- RSS/nyheter
- TV/streaming-status
- kalender från flera familjemedlemmar
- flera Raspberry Pi-paneler

Flera paneler ska kunna använda samma backend.

Exempel:

```text
                    Backend
                       │
            ┌──────────┼──────────┐
            │          │          │
          Panel 1    Panel 2    Panel 3
          Köket      Hallen     Sovrum
```

Backend ska därför inte byggas specifikt kring en enda Raspberry Pi.

---

# 41. Slutligt mål

Slutprodukten ska kännas som en färdig konsumentprodukt snarare än ett hobbyprojekt.

När Raspberry Pi startar ska användaren inte behöva göra någonting.

```text
Power ON
   ↓
Linux startar
   ↓
WiFi ansluts
   ↓
Chromium startar
   ↓
Dashboard laddas
   ↓
HOME
   ↓
WEATHER
   ↓
BUS
   ↓
CALENDAR
   ↓
SCHEDULE
   ↓
HOME
```

All data ska hämtas från den lokala backend-servern.

Raspberry Pi ska vara så resurssnål som möjligt och huvudsakligen fungera som en tunn presentation-/touch-klient.

Den visuella designen ska vara ren, modern, minimalistisk och specifikt optimerad för den lilla 3,5"-touchskärmen.
