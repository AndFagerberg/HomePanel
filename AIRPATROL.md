# HomePanel – AirPatrol .NET integration

## Syfte

Implementera native AirPatrol-stöd i HomePanel-backend.

Målet är att läsa och senare styra luftvärmepumpen i stugan via den befintliga AirPatrol WiFi-enheten. Raspberry Pi-klienten ska aldrig kommunicera direkt med AirPatrol; all kommunikation ska ske via HomePanel-backend.

## Känd hårdvara

Den faktiska enheten är:

- Product: AirPatrol WiFi
- Hardware: 5.1.0
- Region: World
- Serial number: 301094
- FCC ID: `2AC7Z-ESP32SMINI1`
- IC: `21098-ESP32SMINI1`

Enheten är redan konfigurerad och fungerar med den officiella AirPatrol-appen.

## Viktig slutsats

Den aktuella Home Assistant-integrationen är liten och använder ett separat Python-paket, `airpatrol`. Det finns inget behov av att introducera Home Assistant i HomePanel.

**Porta i stället det faktiska AirPatrol HTTP-protokollet till native .NET.** Översätt inte Python-koden rad för rad; implementera en ren .NET-client med `HttpClient` och befintliga HomePanel-konventioner.

## Referensimplementation

Använd följande som teknisk referens innan implementationen påbörjas:

- Home Assistant AirPatrol integration: https://github.com/home-assistant/core/tree/dev/homeassistant/components/airpatrol
- Python AirPatrol client: https://github.com/antondalgren/airpatrol
- Home Assistant-dokumentation: https://www.home-assistant.io/integrations/airpatrol/

Python-paketet är MIT-licensierat.

## Kända API-servrar och endpoints

Referensimplementationen använder:

```text
Authentication:
https://auth.apsrvd.io

API:
https://api.apsrvd.io
```

Kända endpoints:

```text
POST https://auth.apsrvd.io/v1/login
GET  https://auth.apsrvd.io/v1/pairings
GET  https://api.apsrvd.io/12/command
POST https://api.apsrvd.io/12/command
```

Verifiera alltid den aktuella referenskoden innan implementationen låses. Isolera base URLs i client/configuration så att en ändring inte kräver genomgripande kodändringar.

## Authentication

Login sker med AirPatrol-kontots e-postadress och lösenord.

```http
POST https://auth.apsrvd.io/v1/login
Content-Type: application/json
```

Exempel:

```json
{
  "email": "user@example.com",
  "password": "secret"
}
```

Referensklienten hämtar bland annat:

```text
entities.users.list[0].id
misc.accessToken
```

Spara access token i minnet där det är praktiskt. Lägg inte tokens i loggar eller repository.

Om ett autentiserat anrop ger 401/403 enligt samma semantik som referensimplementationen ska clienten autentisera om och försöka operationen **en gång** till. Undvik oändliga retry-loopar.

## Device discovery

Efter authentication:

```http
GET https://auth.apsrvd.io/v1/pairings
Authorization: Bearer <access-token>
```

Mappar resultatet till en HomePanel-modell i stället för att sprida AirPatrols råa JSON-struktur genom applikationen.

Exempel:

```csharp
public sealed record AirPatrolDevice(
    string Id,
    string Name,
    string? Type,
    string? HardwareId,
    string? AppId);
```

Anpassa modellen efter den faktiska response-strukturen.

## Climate/status API

Referensklienten hämtar klimatstatus med:

```http
GET https://api.apsrvd.io/12/command
Authorization: Bearer <access-token>
X-Pairing-Id: <device-id>
```

Home Assistant använder bland annat dessa AirPatrol-fält:

```text
RoomTemp
RoomHumidity
ParametersData.PumpTemp
ParametersData.PumpPower
ParametersData.PumpMode
ParametersData.FanSpeed
ParametersData.Swing
```

Dessa är protokollfält och ska mappas till egna HomePanel-modeller.

Exempel:

```csharp
public sealed record AirPatrolStatus(
    string DeviceId,
    string Name,
    bool IsOnline,
    bool IsOn,
    decimal? CurrentTemperature,
    decimal? Humidity,
    decimal? TargetTemperature,
    AirPatrolMode? Mode,
    AirPatrolFanMode? FanMode,
    bool? Swing,
    DateTimeOffset RetrievedAt);
```

Använd nullable properties för fält som kan saknas beroende på värmepump eller firmware.

## Control API

Referensimplementationen skickar styrkommandon till:

```http
POST https://api.apsrvd.io/12/command
Authorization: Bearer <access-token>
X-Pairing-Id: <device-id>
Content-Type: application/json
```

Request body ska följa **den aktuella Python-referensens exakta JSON-struktur**. Gissa inte strukturen.

Kända kontrollvärden i referensimplementationen inkluderar:

```text
Power: on / off
Mode: heat / cool / off
Fan: auto / min / max
Swing: on / off
Temperature: exempelvis "21.000"
```

Alla värmepumpar behöver inte stödja alla funktioner. Kontrollera capabilities och hantera unsupported operations.

## .NET-arkitektur

Föreslagen struktur:

```text
HomePanel
└── Infrastructure
    └── AirPatrol
        ├── AirPatrolClient.cs
        ├── AirPatrolModels.cs
        ├── AirPatrolOptions.cs
        └── AirPatrolExceptions.cs
```

`AirPatrolClient` ansvarar endast för:

- authentication
- tokenhantering
- device discovery
- status requests
- control commands
- JSON serialization/deserialization
- AirPatrol-specifik felhantering

Använd `HttpClient`, async I/O, `CancellationToken` och timeout.

## HomePanel service

Lägg HomePanel-logik bakom en separat service-abstraktion.

Exempel:

```csharp
public interface IAirPatrolService
{
    Task<IReadOnlyList<AirPatrolDevice>> GetDevicesAsync(
        CancellationToken cancellationToken);

    Task<AirPatrolStatus?> GetStatusAsync(
        string deviceId,
        CancellationToken cancellationToken);

    Task SetPowerAsync(
        string deviceId,
        bool on,
        CancellationToken cancellationToken);

    Task SetTargetTemperatureAsync(
        string deviceId,
        decimal temperature,
        CancellationToken cancellationToken);

    Task SetModeAsync(
        string deviceId,
        AirPatrolMode mode,
        CancellationToken cancellationToken);

    Task SetFanModeAsync(
        string deviceId,
        AirPatrolFanMode mode,
        CancellationToken cancellationToken);
}
```

Anpassa detta till HomePanels befintliga vertical-slice/DI-arkitektur.

## Configuration

Credentials ska vara server-side secrets.

Exempel:

```text
AirPatrol:
  Enabled: true
  Email: <secret>
  Password: <secret>
  PollIntervalSeconds: 60
```

Alternativt environment variables enligt befintliga HomePanel-konventioner.

Aldrig:

- credentials i Git
- credentials i frontend
- password i loggar
- access token i loggar
- Authorization-header i loggar

## Polling och cache

AirPatrol är en cloud-polling-integration. Gör inte ett AirPatrol-anrop varje gång dashboarden renderas.

Föreslagen modell:

```text
AirPatrolClient
      |
      v
AirPatrolService
      |
      v
Cached latest status
      |
      +---- Dashboard API
      |
      +---- Detail API
```

Startvärde: 60 sekunder, konfigurerbart.

Efter lyckat styrkommando:

1. Skicka command.
2. Kontrollera response.
3. Invalidera/uppdatera cache.
4. Hämta status igen när lämpligt.
5. Returnera aktuell status.

Frontend ska aldrig polla AirPatrol direkt.

## API

Följ HomePanels befintliga API-konventioner. Möjlig design:

```http
GET /api/airpatrol
GET /api/airpatrol/{deviceId}
POST /api/airpatrol/{deviceId}/power
POST /api/airpatrol/{deviceId}/temperature
POST /api/airpatrol/{deviceId}/mode
POST /api/airpatrol/{deviceId}/fan
```

Exakt endpointdesign ska följa resten av projektet.

## Capabilities

Backend ska exponera vilka funktioner som faktiskt stöds.

Exempel:

```json
{
  "deviceId": "123",
  "name": "Stugan",
  "capabilities": {
    "power": true,
    "temperature": true,
    "mode": true,
    "fan": true,
    "swing": false
  }
}
```

Frontend visar endast relevanta kontroller.

## Frontend

Dashboardkortet ska visa exempelvis:

```text
┌─────────────────────────────┐
│ 🏠 Stugan                   │
│                             │
│        21.4 °C              │
│        48 %                 │
│                             │
│ 🔥 Värme     21 °C          │
│ ● På                        │
└─────────────────────────────┘
```

Detaljvyn ska kunna visa och, när stödet finns, styra:

- aktuell temperatur
- luftfuktighet
- power
- driftläge
- börtemperatur
- fläktläge
- swing
- temperatur +/-
- mode
- power

UI ska vara touch-first och fungera på HomePanels 1024x600-display.

## Offline/fel

HomePanel ska fungera även när AirPatrol Cloud är otillgängligt.

Behåll senaste giltiga status och visa timestamp:

```text
Stugan
21.4 °C
⚠ Kunde inte uppdatera
Senast uppdaterad 09:18
```

Töm inte hela dashboarden på grund av AirPatrol-fel.

Visa inte ett control command som lyckat om backend inte fått ett framgångsrikt svar.

Hantera minst:

- invalid credentials
- authentication failure
- expired token
- 401/403
- timeout
- network failure
- AirPatrol Cloud unavailable
- device offline
- unknown device
- unsupported operation
- invalid temperature
- invalid mode
- rate limiting
- unexpected response

## Loggning

Använd strukturerad loggning med exempelvis:

- device ID
- operation
- duration
- success/failure
- error category

Exempel:

```text
AirPatrol authentication successful
AirPatrol device discovered
AirPatrol status updated
AirPatrol device offline
AirPatrol command failed
```

Logga aldrig credentials, tokens eller autentiserade payloads.

## Fas 1 – Read-only proof of concept

Innan integration med hela HomePanel byggs ska en minimal .NET-testklient eller diagnostic command skapas.

Den ska endast:

1. Authenticate.
2. Lista AirPatrol devices.
3. Hämta status.
4. Skriva ut normaliserade värden.

Exempel:

```text
AirPatrol authentication: OK

Devices:
  Stugan
  ID: xxxxxxxx

Climate:
  Temperature: 21.4 °C
  Humidity:    47 %
  Power:       on
  Mode:        heat
  Target:      21.0 °C
  Fan:         auto
  Swing:       off
```

**Inga styrkommandon får skickas i fas 1.**

Syftet är att verifiera den native .NET-portningen mot användarens verkliga AirPatrol WiFi Hardware 5.1.0.

## Fas 2 – HomePanel

När fas 1 fungerar:

1. Flytta clienten till HomePanel.
2. Registrera via dependency injection.
3. Lägg till configuration.
4. Implementera polling/cache.
5. Lägg till read-only API.
6. Lägg till dashboardkort.
7. Lägg till detaljvy.

## Fas 3 – Styrning

Implementera separat och testa i denna ordning:

1. Power
2. Target temperature
3. Operating mode
4. Fan
5. Swing om det stöds och behövs

Implementera inte alla kontrollfunktioner samtidigt.

## Tester

### Unit tests

Testa:

- login response deserialization
- pairing/device mapping
- status mapping
- saknade optional fields
- command serialization
- capabilities
- authentication errors
- token refresh
- timeout
- offline device
- command failure
- invalid input

### Integration/manual tests

Verifiera mot riktig AirPatrol:

- authentication
- device discovery
- temperature
- humidity
- power state
- mode
- target temperature
- fan
- power control
- temperature change
- mode change
- fan change

Control tests ska inte köras automatiskt i CI.

## Copilot-regler

1. Inspektera först befintlig HomePanel-arkitektur och följ dess conventions.
2. Inspektera aktuell Home Assistant AirPatrol-integration och Python-klienten innan protokollet implementeras.
3. Porta det faktiska HTTP-protokollet, inte bara funktionaliteten.
4. Introducera inte Home Assistant.
5. Introducera inte Python om det inte finns ett starkt arkitekturskäl.
6. Använd native .NET och `HttpClient`.
7. Isolera AirPatrol-protokollet.
8. Använd HomePanel-specifika modeller utanför clienten.
9. Exponera inte tredjeparts-DTO:er genom HomePanel API.
10. Håll credentials server-side.
11. Håll access tokens i minnet där det är praktiskt.
12. Implementera authentication retry en gång vid expired/invalid session.
13. Undvik oändliga retries.
14. Använd async I/O, cancellation tokens och timeout.
15. Cacha status och undvik onödig cloud polling.
16. Hantera capabilities dynamiskt.
17. Anta inte att alla HVAC-funktioner stöds.
18. Rapportera aldrig control success utan framgångsrikt svar.
19. Skriv tester för serialization/deserialization och felhantering.
20. Börja med read-only proof of concept.
21. Ändra inte befintlig HomePanel-funktionalitet i onödan.
22. Håll implementationen liten och underhållbar.

## Definition of done

### Fas 1

- [ ] Native .NET authentication fungerar.
- [ ] AirPatrol device discovery fungerar.
- [ ] Riktig AirPatrol WiFi 5.1.0 identifieras.
- [ ] Temperatur läses.
- [ ] Luftfuktighet läses.
- [ ] Power state läses.
- [ ] Mode läses.
- [ ] Target temperature läses.
- [ ] Fan läses när tillgängligt.
- [ ] Inga control commands skickas.

### Full integration

- [ ] `AirPatrolClient` implementerad.
- [ ] `IAirPatrolService` implementerad.
- [ ] Configuration implementerad.
- [ ] Token handling implementerad.
- [ ] Polling/cache implementerad.
- [ ] Read API implementerat.
- [ ] Dashboardkort implementerat.
- [ ] Detail view implementerad.
- [ ] Power control implementerad.
- [ ] Temperature control implementerad.
- [ ] Mode control implementerad.
- [ ] Fan control implementerad när stöds.
- [ ] Swing implementerad när relevant/stöds.
- [ ] Offline handling implementerad.
- [ ] Unit tests implementerade.
- [ ] Credentials/tokens exponeras eller loggas aldrig.
- [ ] HomePanel fungerar när AirPatrol är otillgängligt.

## Referenser

- Home Assistant AirPatrol integration: https://github.com/home-assistant/core/tree/dev/homeassistant/components/airpatrol
- Python AirPatrol client: https://github.com/antondalgren/airpatrol
- Home Assistant AirPatrol documentation: https://www.home-assistant.io/integrations/airpatrol/
- AirPatrol: https://airpatrol.eu/
