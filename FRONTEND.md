# HomePanel Frontend – GUI-specifikation

Detta dokument beskriver målbilden, designprinciperna och den tekniska riktningen för HomePanels frontend. Dokumentet är skrivet som arbetsunderlag och kontext för utveckling i VS Code med GitHub Copilot.

---

# 1. Projektets mål

HomePanel är en touchbaserad informationspanel för ett hushåll.

Panelen ska snabbt visa viktig information:

- Tid och datum
- Aktuellt väder och temperatur
- Nästa buss till stan
- Kalender och dagens aktiviteter
- Aktiva timers
- Musik och Spotify
- Framtida hushållsfunktioner

Arkitektur:

```text
Linux-server
    │
    │ HomePanel Backend/API
    │ - externa API-anrop
    │ - väder
    │ - kollektivtrafik
    │ - kalender
    │ - timers
    │ - musik
    │ - cachning
    │
    ▼ WiFi/LAN
Raspberry Pi Zero 2 W
    │
    │ Raspberry Pi OS Lite
    │ Minimal X-server
    │ Chromium kiosk
    │
    ▼ HDMI + USB touch
10,1" touchskärm
1024 × 600
Landskap
Kapacitiv touch
```

Raspberry Pi ska vara en tunn klient. Tung logik, externa API-anrop och databehandling ska ligga i backend.

---

# 2. Målplattform

Primär målplattform:

```text
Skärm: 10,1 tum
Upplösning: 1024 × 600
Orientering: Landskap
Input: Touch
Webbläsare: Chromium kiosk
Klient: Raspberry Pi Zero 2 W
```

Frontend ska optimeras specifikt för 1024 × 600. Responsiv design ska fortfarande användas för utveckling på dator, men denna upplösning är den primära designytan.

HomePanel ska inte kännas som en vanlig webbplats eller mobilapp. Det är en fysisk informationspanel som ska kunna läsas på några sekunder.

---

# 3. Designprincip

Den viktigaste principen är:

> Man ska kunna kasta ett öga på skärmen och inom ett par sekunder förstå vad som är viktigt just nu.

GUI:t ska vara:

- Rent
- Lugnt
- Modernt
- Informationsrikt utan att vara plottrigt
- Touchvänligt
- Konsekvent
- Snabbt
- Läsbart på avstånd

Undvik:

- Små textlänkar
- Små ikoner som enda klickyta
- Komplicerad navigation
- Många nivåer
- Tunga visuella effekter
- Täta tabeller
- Onödiga animationer

Föredra:

- Stora klickbara kort
- Tydlig typografi
- God kontrast
- Stora touchytor
- Enkel navigation
- Tydlig visuell hierarki

---

# 4. Navigationsmodell

Navigation ska fungera på två sätt.

## 4.1 Dashboard → detaljvy

Dashboarden visar information som kort. Hela kortet är klickbart.

```text
Dashboard
    │
    ├── Väder ──────► Väderdetaljer
    ├── Buss ───────► Avgångar
    ├── Kalender ───► Kommande aktiviteter
    ├── Timers ─────► Timerhantering
    └── Musik ──────► Musikspelare
```

Dashboarden visar sammanfattningar. Detaljvyer visar mer information och funktioner.

## 4.2 Slide-out-meny

En menyknapp ska alltid vara tillgänglig:

```text
☰
```

Menyn glider in från vänster och innehåller:

```text
HomePanel

● Översikt

☀ Väder

🚌 Buss

📅 Kalender

⏱ Timers

🎵 Musik

⚙ Inställningar
```

Menyn ska:

- Ha stora touchytor
- Visa aktiv sida tydligt
- Kunna stängas genom att trycka utanför
- Inte ta permanent plats när den är stängd

Dashboard-korten är den snabbaste navigationen. Menyn är den universella navigationen.

---

# 5. Huvudvyer

Första versionen ska ha:

1. 🏠 Översikt
2. ☀️ Väder
3. 🚌 Buss
4. 📅 Kalender
5. ⏱️ Timers
6. 🎵 Musik

Systemet ska byggas modulärt så att fler funktioner enkelt kan läggas till senare, exempelvis:

- Husinformation
- Elförbrukning
- Temperaturgivare
- Meddelanden
- Sopkalender
- Tvätt
- Smart home-integrationer

---

# 6. Dashboard / Översikt

Dashboarden är startsidan och ska visa den viktigaste informationen.

Föreslagen konceptuell layout:

```text
┌──────────────────────────────────────────────────────────────┐
│ ☰  HOME                                   19:42  Lör 6 sep   │
├────────────────────────────┬─────────────────────────────────┤
│                            │                                 │
│          ☀                 │         🚌 NÄSTA BUSS           │
│                            │                                 │
│          18°               │         19:55                   │
│        Soligt              │         12 minuter              │
│                            │                                 │
├────────────────────────────┼─────────────────────────────────┤
│                            │                                 │
│        📅 IDAG             │          ⏱ TIMERS               │
│                            │                                 │
│   20:00 Träning            │    Pasta          07:42         │
│   Nästa: 08:00 Arbete      │    Ugn            18:05         │
│                            │                                 │
├────────────────────────────┴─────────────────────────────────┤
│ 🎵 NU SPELAS                                                 │
│                                                              │
│ Album  Artist – Låtnamn                      ⏮   ▶   ⏭       │
└──────────────────────────────────────────────────────────────┘
```

Viktiga krav:

- Tid och datum ska vara tydligt synliga
- Väder visar aktuell sammanfattning
- Buss prioriterar nästa relevanta avgång
- Kalender visar dagens/nästa aktivitet
- Timers visar aktiva timers
- Musik visar aktuell uppspelning
- Hela informationskort är klickbara

Dashboarden ska vara den vy som visas större delen av tiden.

---

# 7. Väder

## Dashboard

Visa exempelvis:

- Väderikon
- Aktuell temperatur
- Kort beskrivning
- Eventuellt ”känns som”

Exempel:

```text
☀
18°
Soligt
```

## Detaljvy

Visa:

- Aktuellt väder
- Temperatur
- Känns som
- Timprognos
- Dagens prognos
- Kommande dagar

```text
┌────────────────────────────────────────────────────────────┐
│ ←                    Väder                                 │
├────────────────────────────────────────────────────────────┤
│                                                            │
│                       ☀                                    │
│                      18°                                   │
│                  Soligt · Växjö                            │
├────────────────────────────────────────────────────────────┤
│ NU       +1h      +2h      +3h      +4h                    │
│ ☀ 18°    ☀ 19°    ☀ 18°    🌤 17°   🌥 16°                  │
├────────────────────────────────────────────────────────────┤
│ Idag         Imorgon       Nästa dag                       │
│ ☀ 12–20°     🌧 10–17°     🌤 9–16°                         │
└────────────────────────────────────────────────────────────┘
```

---

# 8. Buss

Bussfunktionen ska fokusera på vardagsnytta.

## Dashboard

Visa:

- Nästa avgång
- Tid kvar
- Eventuell försening

```text
NÄSTA BUSS

19:55

12 minuter
```

## Detaljvy

Visa flera kommande avgångar:

```text
┌────────────────────────────────────────────────────────────┐
│ ←                    Buss                                  │
├────────────────────────────────────────────────────────────┤
│ Till stan                                                  │
│                                                            │
│ 19:55                                  12 minuter          │
│ Buss 123                               I tid                │
│                                                            │
│ 20:25                                  42 minuter          │
│ Buss 123                                                   │
│                                                            │
│ 20:55                                  1 h 12 min           │
└────────────────────────────────────────────────────────────┘
```

Framtida stöd:

- Flera favoritresor
- Hem / till stan
- Flera hållplatser
- Förseningar
- Realtidsinformation

---

# 9. Kalender

Dashboarden ska visa nästa relevanta händelser, inte en fullständig månadsöversikt.

```text
📅 IDAG

20:00
Träning

Nästa:
Imorgon 08:00
Arbete
```

Detaljvyn fokuserar på:

- Idag
- Imorgon
- Kommande dagar

En liten klassisk månadskalender ska inte prioriteras i första versionen eftersom den blir svårläst på 1024 × 600.

---

# 10. Timers

Timers är en central funktion i HomePanel.

Panelen ska kunna hantera flera timers samtidigt.

Exempel:

- Pasta
- Pizza
- Ägg
- Ugn
- Tvätt

## Dashboard

```text
┌──────────────────────────────┐
│ ⏱ TIMERS                 +   │
│                              │
│ 🍝 Pasta         07:42        │
│ ████████████░░░░░░            │
│                              │
│ 🔥 Ugn            18:05       │
│ ███████░░░░░░░░░░             │
└──────────────────────────────┘
```

`+` öppnar ny timer.

## Ny timer

```text
NY TIMER

Namn
[ Pasta                       ]

Tid

[ 00 ] h     [ 10 ] min     [ 00 ] sek


SNABBTIDER

[ 3 min ]  [ 5 min ]  [ 10 min ]

[ 15 min ] [ 30 min ] [ 60 min ]


          [ STARTA ]
```

Det ska gå snabbt att starta vanliga timers.

Timerpresets kan exempelvis vara:

- Ägg – 7 min
- Pasta – 10 min
- Pizza – 12 min

## Timersida

```text
TIMERS
────────────────────────────────

             + NY TIMER


AKTIVA

🍝 Pasta

           07:42

[ PAUSA ]       [ STOPPA ]


🔥 Ugn

           18:05

[ PAUSA ]       [ STOPPA ]


SPARADE TIMERS

[ Pasta – 10 min ]

[ Ägg – 7 min ]

[ Pizza – 12 min ]
```

## Timer klar

När en timer är klar ska:

1. Ett tydligt visuellt meddelande visas
2. Ljud spelas i skärmens högtalare
3. Skärmen väckas om den är släckt
4. Timern kvitteras av användaren

```text
╔══════════════════════════════════════╗
║                                      ║
║                ⏰                    ║
║                                      ║
║         PASTA ÄR KLAR!               ║
║                                      ║
║            [ STOPPA ]                ║
║                                      ║
╚══════════════════════════════════════╝
```

## Teknisk timerprincip

Backend är den definitiva källan för timerstatus.

Timer ska lagra exempelvis:

```text
Timer
- Id
- Name
- StartedAt
- Duration
- EndsAt
- Status
```

Frontend räknar återstående tid från absoluta sluttider.

Timern ska fortsätta korrekt även om:

- Chromium laddas om
- Frontend startas om
- Raspberry Pi laggar
- Skärmen släcks
- Användaren navigerar till annan vy

Frontend får inte vara den enda platsen där en timer räknas.

---

# 11. Musik

Musikfunktionen ska kunna integreras med Spotify och hushållets befintliga uppspelningslösning.

Dashboarden visar:

- Aktuell låt
- Artist
- Albuminformation
- Grundläggande kontroller

```text
🎵 NU SPELAS

Artist – Låtnamn

⏮     ▶ / ❚❚     ⏭
```

Detaljvyn kan senare innehålla:

- Aktuell uppspelning
- Albumomslag
- Play/pause
- Nästa/föregående
- Sök
- Favoriter
- Spellistor
- Senast spelat

---

# 12. Top bar

Alla huvudvyer ska ha en konsekvent toppsektion.

```text
┌────────────────────────────────────────────────────────────┐
│ ☰     VYNAMN                           19:42   Lör 6 sep   │
└────────────────────────────────────────────────────────────┘
```

På detaljvyer:

```text
┌────────────────────────────────────────────────────────────┐
│ ←     Väder                            19:42               │
└────────────────────────────────────────────────────────────┘
```

Det ger konsekvent navigation och gör aktuell tid synlig.

---

# 13. Touch-design

HomePanel är primärt en touchapplikation.

Regler:

- Stora touchytor
- Undvik små knappar
- Hela kort ska vara klickbara
- Tydlig visuell feedback vid touch
- Tillräckligt avstånd mellan kontroller
- Viktiga funktioner ska kräva få tryckningar

Undvik en liten ”Visa”-knapp på ett informationskort. Hela kortet ska öppna detaljvyn.

---

# 14. Layoutprinciper

Designreferens:

```text
1024 × 600
```

Använd CSS Grid och Flexbox.

Undvik absoluta pixelpositioner för vanliga layoutkomponenter.

Rekommenderad struktur:

```text
App Shell
│
├── Top Bar
│
├── Main Content
│   ├── Dashboard Grid
│   └── Detail Views
│
└── Slide-out Navigation
```

Använd relativa CSS-enheter där det är lämpligt:

- fr
- %
- rem
- clamp()

1024 × 600 ska dock alltid användas som den primära visuella referensen.

---

# 15. Visuell stil

Stilen ska kännas som en modern fysisk informationspanel.

Prioritera:

- Lugna ytor
- Tydlig kontrast
- Få visuella nivåer
- Tydliga kort
- Konsekvent rundning
- Konsekvent spacing

Undvik:

- Överdrivna gradients
- Tunga blur-effekter
- Komplexa skuggor
- Tunga visuella effekter

Prestanda är viktigare än avancerade visuella specialeffekter.

---

# 16. Animationer

Animationer ska vara få och snabba.

Lämpliga:

- Slide-in/out för meny
- Diskret touch-feedback
- Enkel vyövergång
- Timer-varning

Undvik:

- Parallax
- Konstant rörliga bakgrunder
- Tunga canvas-animationer
- Onödiga transitions

Respektera gärna `prefers-reduced-motion`.

---

# 17. Skärmsläckning och väckning

På sikt ska HomePanel kunna spara energi.

Målet:

```text
Ingen aktivitet
       ↓
Dimma eller släck display
       ↓
Touch
       ↓
Panelen vaknar
```

Timers och backend ska fortsätta fungera även när skärmen är släckt.

Exakt implementation beror på HDMI-skärmens och Raspberry Pi-systemets stöd.

---

# 18. Datauppdatering

Backend ansvarar för:

- Externa API-anrop
- Cachning
- Normalisering
- Tidskritisk logik
- Integrationer

Frontend ansvarar för:

- Presentation
- Touch-interaktion
- Navigation
- Lokal UI-state
- Visning av data

Använd ett tydligt API-lager:

```text
Frontend
    │
    ▼
HomePanel API Client
    │
    ▼
Backend API
    ├── Weather Service
    ├── Transport Service
    ├── Calendar Service
    ├── Timer Service
    └── Music Service
```

Undvik att varje komponent själv gör externa API-anrop.

---

# 19. Komponentarkitektur

Bygg frontend modulärt och feature-baserat.

```text
app
│
├── core
│   ├── api
│   ├── models
│   └── services
│
├── layout
│   ├── app-shell
│   ├── top-bar
│   └── navigation-drawer
│
├── features
│   ├── dashboard
│   ├── weather
│   ├── transport
│   ├── calendar
│   ├── timers
│   └── music
│
└── shared
    ├── components
    ├── pipes
    └── utilities
```

Varje feature ska vara så självständig som möjligt.

Dashboarden återanvänder sammanfattningskomponenter där det är lämpligt.

---

# 20. Dashboard-widgets

Dashboarden ska bestå av återanvändbara widgets:

```text
Dashboard
│
├── Weather Widget
├── Transport Widget
├── Calendar Widget
├── Timer Widget
└── Music Widget
```

Varje widget ska:

- Visa en sammanfattning
- Vara lättviktig
- Vara klickbar
- Länka till detaljvyn

Widgets ska inte innehålla tung feature-logik.

---

# 21. Prestanda

Raspberry Pi Zero 2 W är en begränsad klient.

Optimera för:

- Snabb första rendering
- Låg CPU-användning
- Låg minnesanvändning
- Få DOM-element
- Begränsade animationer
- Effektiva API-anrop

Undvik:

- Onödigt stora bibliotek
- Onödigt stora bilder
- Onödiga beroenden
- Högfrekvent polling
- Komplex rendering varje sekund

---

# 22. Tid och klocka

Tid visas centralt i gränssnittet.

Implementera inte klocka eller timers på ett sätt som gradvis driver från verklig tid.

Använd aktuella absoluta tidsstämplar.

Timerlogik ska alltid baseras på absoluta sluttider från backend.

---

# 23. Felhantering

Frontend ska hantera temporära nätverksproblem snyggt.

Exempel:

- Visa senast kända data
- Visa diskret status om data är gammal
- Låt inte hela dashboarden bli tom om en tjänst misslyckas
- Widgets ska kunna visa fel oberoende av varandra

Om bussdata misslyckas ska exempelvis kalender och timers fortfarande fungera.

---

# 24. Läsbarhet

Panelen ska kunna läsas på avstånd.

Prioritera:

- Stor huvudinformation
- Tydliga rubriker
- Hög kontrast
- Läsbar typografi

Viktig information, exempelvis nästa buss och återstående timer, ska vara visuellt framträdande.

---

# 25. Rekommenderad utvecklingsordning

## Steg 1 – App Shell

Implementera:

- Grundlayout
- Top bar
- Slide-out navigation
- Routing
- 1024 × 600-layout

## Steg 2 – Dashboard

Implementera:

- Dashboard grid
- Mock-data
- Klickbara widgets
- Navigation till detaljvyer

## Steg 3 – Timers

Implementera:

- Timerlista
- Ny timer
- Snabbtimers
- Pausa
- Stoppa
- Timer klar-overlay

Timers är en bra tidig funktion eftersom den är mycket användbar och relativt självständig.

## Steg 4 – Väder

Implementera:

- Dashboard-widget
- Detaljvy
- Timprognos
- Flerdagarsprognos

## Steg 5 – Buss

Implementera:

- Nästa avgång
- Avgångslista
- Realtidsstatus

## Steg 6 – Kalender

Implementera:

- Dagens aktiviteter
- Nästa aktivitet
- Kommande dagar

## Steg 7 – Musik

Implementera:

- Nu spelas
- Grundläggande kontroller
- Senare Spotify-sökning och spellistor

---

# 26. Instruktioner för GitHub Copilot

När ny frontend-kod genereras ska följande principer följas:

1. Optimera primärt för 1024 × 600 i landskapsläge.
2. Frontend ska vara lättviktig eftersom den körs på Raspberry Pi Zero 2 W.
3. Undvik tunga UI-ramverk och onödiga beroenden.
4. Använd modulär feature-baserad struktur.
5. Använd stora touchytor.
6. Gör hela dashboard-kort klickbara.
7. Dashboard visar sammanfattningar och detaljvyer visar mer information.
8. Navigation ska fungera både via dashboard-kort och slide-out-meny.
9. Undvik små knappar och små textlänkar som primär interaktion.
10. Prioritera prestanda framför visuella specialeffekter.
11. Backend ansvarar för tung logik och externa integrationer.
12. Frontend ska vara tolerant mot temporära nätverksfel.
13. Timers ska baseras på absoluta sluttider och backend ska vara den definitiva källan.
14. Skapa återanvändbara widgets och komponenter.
15. Nya funktioner ska kunna läggas till som separata moduler.

---

# 27. Slutmål

Slutmålet är en modern, lugn och mycket användbar fysisk hushållspanel.

När någon passerar panelen ska man snabbt kunna se:

```text
Vad är klockan?
Hur är vädret?
När går nästa buss?
Vad händer idag?
Finns det aktiva timers?
Vad spelas just nu?
```

När mer information behövs ska användaren enkelt kunna:

- Trycka direkt på relevant informationskort
- Eller öppna navigationen

HomePanel ska kännas som en dedikerad produkt och informationspanel, inte som en vanlig webbplats som råkar visas på en skärm.
