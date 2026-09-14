# Deploy

Deployment artifacts for the two runtime targets described in [PROJECT.md](../PROJECT.md).

## Linux server (backend + frontend)

Normal drift är en färdig self-contained release som körs direkt med systemd på den minimala Linux-servern. Deploy-scriptet bygger på utvecklingsmaskinen, installerar till `/opt/homepanel` och behåller SQLite-data i `/var/lib/homepanel`:

```powershell
.\deploy\configure-homepanel-env.ps1
.\deploy\deploy-homepanel.ps1
```

Det första scriptet är lokalt och ignoreras av Git eftersom det innehåller serverns fullständiga konfiguration och hemligheter. Det uppdaterar `/etc/homepanel/homepanel.env` och frontendens lokala API-nyckel. Det andra scriptet publicerar själva applikationen och är helt oberoende av `appsettings.Development.json`.

Fullständig installation och konfiguration finns i [`linux-server-no-docker.md`](linux-server-no-docker.md).

Docker är ett alternativ för utveckling eller andra miljöer:

```bash
docker compose up -d --build
```

## Raspberry Pi (display client)

The Pi never builds or runs the backend — it only runs Chromium in kiosk mode against the server's URL.

- [`raspberry-pi/SETUP.md`](raspberry-pi/SETUP.md) — full step-by-step installation guide (in Swedish).
- [`raspberry-pi/kiosk.sh`](raspberry-pi/kiosk.sh) — starts Chromium in kiosk mode against `DASHBOARD_URL`.
- [`raspberry-pi/xinitrc`](raspberry-pi/xinitrc) — copied to `~/.xinitrc`; disables screen blanking and restarts Chromium automatically if it crashes.
