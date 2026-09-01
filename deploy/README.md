# Deploy

Deployment artifacts for the two runtime targets described in [PROJECT.md](../PROJECT.md).

## Linux server (backend + frontend)

Build and run with Docker:

```bash
docker compose up -d --build
```

The container serves both the REST API (`/api/dashboard`) and the Angular production build on port 8080.

If the server is too small to build or run Docker comfortably, build the release on the development machine and run the published app directly with systemd instead: see [`linux-server-no-docker.md`](linux-server-no-docker.md).

## Raspberry Pi (display client)

The Pi never builds or runs the backend — it only runs Chromium in kiosk mode against the server's URL.

- [`raspberry-pi/SETUP.md`](raspberry-pi/SETUP.md) — full step-by-step installation guide (in Swedish).
- [`raspberry-pi/kiosk.sh`](raspberry-pi/kiosk.sh) — starts Chromium in kiosk mode against `DASHBOARD_URL`.
- [`raspberry-pi/xinitrc`](raspberry-pi/xinitrc) — copied to `~/.xinitrc`; disables screen blanking and restarts Chromium automatically if it crashes.
