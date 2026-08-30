#!/bin/sh
# Starts Chromium in kiosk mode pointed at the local dashboard backend.
# Intended to be invoked by an autostart entry / systemd unit on the Raspberry Pi (see PROJECT.md section 6/38).

DASHBOARD_URL="${DASHBOARD_URL:-http://household-panel.local}"

exec chromium \
  --no-memcheck \
  --kiosk \
  --noerrdialogs \
  --disable-infobars \
  --disable-session-crashed-bubble \
  --disable-translate \
  --disable-sync \
  --disable-extensions \
  --disable-background-networking \
  "$DASHBOARD_URL"