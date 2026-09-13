# HomePanel – AirPatrol integration

## Syfte

Integrera en AirPatrol WiFi-styrd luftvärmepump i HomePanel.

Backend ska kunna läsa:
- aktuell rumstemperatur
- luftfuktighet
- driftläge
- börtemperatur
- fläktläge/hastighet när det stöds
- online/offline-status

Backend ska på sikt kunna styra:
- på/av
- driftläge
- börtemperatur
- fläktläge/hastighet när det stöds

Raspberry Pi-klienten ska aldrig kommunicera direkt med AirPatrol. All kommunikation ska gå via HomePanel-backend.

## Känd hårdvara

Användarens enhet är:

- AirPatrol WiFi
- Hardware: 5.1.0
- Region: World
- Serial: 301094
- FCC ID: `2AC7Z-ESP32SMINI1`
- IC: `21098-ESP32SMINI1`

Enheten är redan konfigurerad och fungerar med den vanliga AirPatrol-appen.

## Integration

AirPatrol ska integreras via AirPatrols molntjänst. Home Assistants aktuella officiella AirPatrol-integration använder cloud polling och kontoautentisering med samma AirPatrol-apps e-post/lösenord.

Home Assistant dokumenterar stöd för:
- HVAC-läge, inklusive off/heat/cool där enheten stöder det
- börtemperatur
- fläktläge där det stöds
- aktuell temperatur
- luftfuktighet

Integrationen är testad med AirPatrol WiFi v5. Funktionalitet kan skilja beroende på modell och firmware.

Home Assistant ska inte installeras som en runtime-dependency bara för AirPatrol.

## Arkitektur

```text
Raspberry Pi / HomePanel UI
          |
          | HTTP/JSON
          v
HomePanel Backend
          |
          +-- AirPatrolService
          |
          | HTTPS / AirPatrol cloud
          v
     AirPatrol Cloud
          |
          v
     AirPatrol WiFi
          |
          v
      Luftvärmepump
```

Skapa en isolerad `AirPatrolService` som kapslar in tredjepartsbibliotek, autentisering, API-anrop och AirPatrol-specifika DTO:er.

## Rekommenderad backend-abstraktion

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

    Task SetModeAsync(
        string deviceId,
        AirPatrolMode mode,
        CancellationToken cancellationToken);

    Task SetTargetTemperatureAsync(
        string deviceId,
        decimal temperature,
        CancellationToken cancellationToken);

    Task SetFanModeAsync(
        string deviceId,
        AirPatrolFanMode mode,
        CancellationToken cancellationToken);
}
```

Anpassa namn och signaturer till befintlig HomePanel-arkitektur.

## HomePanel-modeller

Använd egna modeller och exponera inte tredjepartsbibliotekets modeller direkt.

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
    DateTimeOffset RetrievedAt);
```

Exempel på enums:

```csharp
public enum AirPatrolMode
{
    Auto,
    Heat,
    Cool,
    Dry,
    FanOnly,
    Off,
    Unknown
}
```

```csharp
public enum AirPatrolFanMode
{
    Auto,
    Low,
    Medium,
    High,
    Unknown
}
```

Alla lägen behöver inte finnas på varje värmepump. Hantera unsupported capabilities dynamiskt.

## Bibliotek och protokoll

Det finns ett Python-bibliotek för AirPatrol som används i Home Assistant-ekosystemet. Innan implementationen låses ska Copilot kontrollera aktuell version och API-yta för biblioteket.

Om HomePanel-backend är .NET bör integrationen följa projektets befintliga teknikval. Om ett Python-bibliotek kräver separat runtime ska detta inte införas utan att först väga det mot en ren HTTP/API-integration eller separat liten integrationsprocess.

Målet är en så liten och robust server-side integration som möjligt.

## Konfiguration

Credentials får aldrig läggas i Git.

Exempel:

```text
AirPatrol:
  Enabled: true
  Email: <secret>
  Password: <secret>
  PollIntervalSeconds: 60
```

Alternativt:

```text
AIRPATROL_ENABLED=true
AIRPATROL_EMAIL=...
AIRPATROL_PASSWORD=...
AIRPATROL_POLL_INTERVAL_SECONDS=60
```

Använd HomePanels befintliga konfigurations- och secret-hantering.

Logga aldrig:
- lösenord
- access/refresh tokens
- cookies
- Authorization-header
- kompletta autentiserade HTTP-request/response-data

## Polling och cache

AirPatrol är en cloud-polling-integration. Gör inte ett cloud-anrop varje gång frontend renderar dashboarden.

Rekommenderad modell:

```text
AirPatrolService
       |
       | periodic polling
       v
  Cached status
       |
       +---- Dashboard API
       |
       +---- Detail API
```

Startvärde:

```text
60 sekunder
```

Gör intervallet konfigurerbart.

Vid styrning:
1. skicka kommando
2. kontrollera lyckat svar
3. uppdatera eller invalidiera cache
4. hämta status igen när det är lämpligt
5. returnera aktuell status till frontend

Frontend ska aldrig själv polla AirPatrol.

## API

Följ HomePanels befintliga API-konventioner. En möjlig design är:

```http
GET /api/airpatrol
GET /api/airpatrol/{deviceId}
POST /api/airpatrol/{deviceId}/power
POST /api/airpatrol/{deviceId}/temperature
POST /api/airpatrol/{deviceId}/mode
POST /api/airpatrol/{deviceId}/fan
```

Exempel:

```json
{
  "on": true
}
```

```json
{
  "temperature": 21
}
```

```json
{
  "mode": "heat"
}
```

```json
{
  "mode": "auto"
}
```

Exakt API-design ska följa resten av HomePanel.

## Capabilities

Backend ska exponera vilka funktioner den aktuella enheten faktiskt stöder.

Exempel:

```json
{
  "deviceId": "123",
  "name": "Stugan",
  "capabilities": {
    "power": true,
    "temperature": true,
    "mode": true,
    "fan": true
  }
}
```

Frontend visar endast relevanta kontroller.

## Dashboard

Lägg till ett touchvänligt kort, exempelvis:

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

Kortet öppnar en detaljvy med:
- aktuell temperatur
- luftfuktighet
- på/av
- driftläge
- börtemperatur
- fläktläge
- +/- temperatur
- lägesval
- av/på

## Offline och fel

HomePanel ska fungera även när AirPatrol Cloud är nere.

Visa senast kända värde:

```text
Stugan
21.4 °C
⚠ Kunde inte uppdatera
Senast uppdaterad 09:18
```

Töm inte hela dashboarden på grund av AirPatrol-fel.

Vid ett styrkommando får frontend inte visa att kommandot lyckades om backend inte fått ett framgångsrikt svar.

Hantera:
- felaktiga credentials
- autentiseringsfel
- utgången session/token
- timeout
- AirPatrol Cloud nere
- enhet offline
- okänd device
- unsupported operation
- ogiltig temperatur
- ogiltigt driftläge
- rate limiting
- oväntat API-svar

Använd timeout och cancellation tokens.

## Loggning

Logga strukturerat:
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

Logga aldrig credentials eller tokens.

## Implementationsordning

Implementera i denna ordning:

### 1. Read-only proof of concept

Verifiera:
- autentisering
- device discovery
- aktuell temperatur
- luftfuktighet
- on/off
- driftläge
- börtemperatur
- fläktläge om tillgängligt

Skicka inga styrkommandon ännu.

### 2. Backend

Skapa `IAirPatrolService`, implementation, modeller, cache och konfiguration.

### 3. Read API

Exponera normaliserad AirPatrol-status via HomePanel.

### 4. Frontend

Lägg till dashboardkort och detaljvy.

### 5. Styrning

Implementera separat och testa:
1. power
2. target temperature
3. mode
4. fan

## Tester

Unit tests ska täcka:
- mapping till HomePanel-modeller
- saknade optional fields
- unsupported capabilities
- authentication errors
- device offline
- timeout
- lyckat kommando
- misslyckat kommando
- ogiltiga värden

Manuella integrationstester:
- autentisering
- device discovery
- temperatur
- luftfuktighet
- driftläge
- börtemperatur
- on/off
- temperaturändring
- mode
- fan när det stöds

Kontrollkommandon ska inte köras automatiskt i CI.

## Copilot-regler

När denna funktion implementeras:

1. Följ befintlig HomePanel-arkitektur.
2. Isolera all AirPatrol-specifik kod.
3. Lägg inte till Home Assistant som runtime-dependency.
4. Kommunicera aldrig med AirPatrol från browsern.
5. Lägg aldrig AirPatrol credentials i frontend.
6. Normalisera tredjepartsdata till HomePanel-modeller.
7. Gör pollingintervallet konfigurerbart.
8. Cacha senaste giltiga status.
9. Hantera capabilities dynamiskt.
10. Hantera offline/cloud-fel utan att slå ut resten av HomePanel.
11. Rapportera aldrig ett styrkommando som lyckat utan framgångsrikt svar.
12. Använd async I/O.
13. Använd cancellation tokens och timeout.
14. Följ repositoryts befintliga DI- och configuration-konventioner.
15. Skriv tester innan integrationen byggs ut med fler styrfunktioner.

## Definition of done

- [ ] AirPatrol authentication fungerar.
- [ ] AirPatrol device kan hittas.
- [ ] Temperatur kan läsas.
- [ ] Luftfuktighet kan läsas.
- [ ] Power state kan läsas.
- [ ] Driftläge kan läsas.
- [ ] Börtemperatur kan läsas.
- [ ] Fläktläge kan läsas när det stöds.
- [ ] Dashboard visar status.
- [ ] Offline/stale state visas korrekt.
- [ ] Power control fungerar.
- [ ] Temperaturändring fungerar.
- [ ] Mode control fungerar.
- [ ] Fan control fungerar när det stöds.
- [ ] Credentials finns endast server-side.
- [ ] Credentials/tokens loggas inte.
- [ ] Unit tests finns.
- [ ] HomePanel fungerar även när AirPatrol är otillgängligt.

## Källor

- Home Assistant AirPatrol integration:
  https://www.home-assistant.io/integrations/airpatrol/
- Home Assistant 2026.1 release notes:
  https://www.home-assistant.io/blog/2026/01/07/release-20261
- AirPatrol:
  https://airpatrol.eu/
