# AirPatrol – Integration Notes

## Syfte

Detta dokument sammanfattar vad vi har verifierat genom AirPatrol WiFi-POC:en och beskriver den information som ska användas vid en framtida integration i HomePanel.

POC:en kördes mot en verklig AirPatrol-enhet och lyckades:
- autentisera mot AirPatrols API
- hitta den kopplade värmepumpen
- läsa aktuell status
- läsa temperatur och luftfuktighet
- läsa driftläge, börtemperatur, fläkt och swing
- verifiera den verkliga API-strukturen för `/command`

## AirPatrol WiFi

Testad hårdvara:
- AirPatrol WiFi
- Hardware: 5.1.0
- Region: World
- Device type: `apw`
- AirPatrol API version: `12`

AirPatrol fungerar via en molntjänst. Kommunikation från HomePanel-backend ska ske mot AirPatrols HTTPS-API, inte direkt mot värmepumpen på det lokala nätverket.

## API

### Basadresser

Authentication API:
```text
https://auth.apsrvd.io/v1
```

Device API:
```text
https://api.apsrvd.io/12
```

### Login

```http
POST https://auth.apsrvd.io/v1/login
Content-Type: application/json
```

Request:
```json
{
  "email": "EMAIL",
  "password": "PASSWORD"
}
```

Login-svaret innehåller bland annat `entities.users.list[0].id` och `misc.accessToken`.

Access token och lösenord ska aldrig loggas, skickas till frontend, läggas i Git eller hårdkodas i källkod.

## Pairings / enheter

```http
GET https://auth.apsrvd.io/v1/pairings
Authorization: Bearer <accessToken>
```

Relevant struktur:
```text
entities.pairingUser.list
        │
        └── pairingId
                │
                ▼
        entities.pairings.list
```

`pairingId` är det numeriska ID som ska användas vid anrop mot enheten. Det är inte samma sak som HWID.

Exempel från POC:
```text
Device name: Kirigamine
Pairing ID: 44223
HWID: CAEVT-PGS67-CQ9SB-V2SV4-DFQYG
Type: apw
```

Vid climate-anrop används:
```http
X-Pairing-Id: 44223
```

## Climate / Command API

### Hämta status

```http
GET https://api.apsrvd.io/12/command
Authorization: Bearer <accessToken>
X-Pairing-Id: <pairingId>
```

Verifierat svar:
```json
{
  "ApiVersion": "12",
  "CommandMode": "parameters",
  "ParametersData": {
    "PumpPower": "on",
    "PumpTemp": "10.000",
    "PumpMode": "lowheat",
    "FanSpeed": "auto",
    "Swing": "on"
  },
  "RoomTemp": "22.090",
  "RoomHumidity": "54"
}
```

## Statusfält

`RoomTemp` är aktuell rumstemperatur, exempelvis `22.090` = 22.09 °C.

`RoomHumidity` är relativ luftfuktighet, exempelvis `54` = 54 %.

## ParametersData

### PumpPower

Av/på:
```text
on
off
```

### PumpTemp

Börtemperatur, som sträng med decimaler:
```json
"PumpTemp": "10.000"
```

### PumpMode

Relevanta värden:
```text
heat
cool
off
lowheat
```

`lowheat` är ett faktiskt AirPatrol-läge och ska behandlas separat från vanligt `heat`.

### FanSpeed

Relevanta värden:
```text
min
max
auto
```

### Swing

```text
on
off
```

## Styrning

```http
POST https://api.apsrvd.io/12/command
Authorization: Bearer <accessToken>
X-Pairing-Id: <pairingId>
Content-Type: application/json
```

Rekommenderad strategi är att först läsa aktuell status, ändra önskat värde i `ParametersData` och därefter skicka tillbaka hela parameteruppsättningen.

Exempel:
```json
{
  "ApiVersion": "12",
  "CommandMode": "parameters",
  "ParametersData": {
    "PumpPower": "on",
    "PumpTemp": "20.000",
    "PumpMode": "heat",
    "FanSpeed": "auto",
    "Swing": "on"
  },
  "RoomTemp": "22.090",
  "RoomHumidity": "54"
}
```

Detta är också i linje med hur Home Assistants AirPatrol-integration hanterar kommandon.

Efter POST bör POC:en göra ett nytt GET för att verifiera resultatet.

## POC-resultat

```text
AirPatrol POC
=============

Authentication: OK

Devices:
  Kirigamine
  Pairing ID: 44223
  HWID:       CAEVT-PGS67-CQ9SB-V2SV4-DFQYG
  Type:       apw

Status:
  Temperature: 22,1 °C
  Humidity:    54,0 %
  Power:       on
  Mode:        lowheat
  Target:      10,0 °C
  Fan:         auto
  Swing:       on
```

Grundläggande read-only-kommunikation är därmed verifierad mot den riktiga AirPatrol-installationen.

## Rekommenderad .NET-implementation

HomePanel behöver inte Home Assistant eller Python. API:t är tillräckligt enkelt för direkt implementation med .NET `HttpClient`.

Föreslagen struktur:
```text
Infrastructure/
└── AirPatrol/
    ├── AirPatrolClient.cs
    ├── AirPatrolModels.cs
    ├── AirPatrolOptions.cs
    └── AirPatrolExceptions.cs
```

Applikationsnivån kan exempelvis exponera:
```text
GetDevices
GetStatus
SetPower
SetTemperature
SetMode
SetFanSpeed
SetSwing
```

## Polling och cache

HomePanel ska inte anropa AirPatrols moln-API varje gång dashboarden renderas.

Rekommendation: polling exempelvis var 60:e sekund och cache av aktuell status på backend.

```text
AirPatrol cloud
      │
      │ polling
      ▼
HomePanel backend
      │
      ├── cache aktuell status
      ▼
HomePanel frontend
```

## HomePanel API

Frontend ska inte känna till AirPatrols interna API eller credentials.

Exempel:
```http
GET /api/airpatrol
```

Svar:
```json
{
  "name": "Kirigamine",
  "temperature": 22.09,
  "humidity": 54,
  "power": true,
  "mode": "lowheat",
  "targetTemperature": 10,
  "fanSpeed": "auto",
  "swing": true
}
```

Styrning kan senare exponeras som exempelvis:
```text
POST /api/airpatrol/temperature
POST /api/airpatrol/power
POST /api/airpatrol/mode
POST /api/airpatrol/fan
POST /api/airpatrol/swing
```

HomePanel frontend ska prata med HomePanel-backend, inte direkt med `apsrvd.io`.

## Nästa steg

1. Implementera `SetTemperature()` i POC.
2. Verifiera POST + efterföljande GET.
3. Implementera `SetFanSpeed()`.
4. Implementera `SetMode()`.
5. Implementera `SetPower()`.
6. Implementera `SetSwing()`.
7. Testa `lowheat` separat.
8. Integrera därefter i HomePanel-backend och frontend.

## Viktiga slutsatser

1. AirPatrol WiFi kan kommuniceras med direkt från .NET.
2. Home Assistant/Python behövs inte.
3. AirPatrol använder separata auth- och device-API:er.
4. API-version `12` används av den testade integrationen.
5. Enheten identifieras via numeriskt `pairingId`.
6. `X-Pairing-Id` ska innehålla pairing-ID:t.
7. HWID används inte som `X-Pairing-Id`.
8. Temperatur och luftfuktighet kan läsas från `/command`.
9. `lowheat` är ett faktiskt AirPatrol-läge.
10. `PumpTemp` representeras som sträng med decimaler.
11. Styrning sker via POST till `/command`.
12. Vid styrning bör hela `ParametersData` bevaras och skickas tillbaka, inte bara det ändrade fältet.
13. HomePanel bör cacha/polla AirPatrol-status på serversidan.
14. Credentials och tokens ska aldrig exponeras för frontend.
