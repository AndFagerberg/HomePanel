#!/bin/sh
# Starts Chromium in kiosk mode pointed at the local dashboard backend.
# Intended to be invoked by an autostart entry / systemd unit on the Raspberry Pi (see PROJECT.md section 6/38).

DASHBOARD_URL="${DASHBOARD_URL:-http://household-panel.local}"

exec chromium-browser \
  --kiosk \
  --noerrdialogs \
  --disable-infobars \
  --disable-session-crashed-bubble \
  --check-for-update-interval=31536000 \
  "$DASHBOARD_URL"
