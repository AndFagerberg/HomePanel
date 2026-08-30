# Installation av Raspberry Pi (displayklient)

Denna guide installerar Raspberry Pi Zero 2 W som en dedikerad kioskpanel enligt [PROJECT.md](../../PROJECT.md) sektion 5–6 och 38. Pi:n bygger ingenting och kör ingen backend-logik — den startar bara Chromium mot backendens URL.

Vi använder **Raspberry Pi OS Lite** (utan skrivbordsmiljö) + en minimal X-server, eftersom det är betydligt lättare för en Zero 2 W än en fullständig desktop.

## 1. Flasha SD-kortet

1. Ladda ner **Raspberry Pi Imager**: https://www.raspberrypi.com/software/
2. Välj **Raspberry Pi OS Lite (64-bit)**.
3. Klicka på kugghjulet (avancerade inställningar) innan du flashar och ange:
   - Hostname, t.ex. `panel`
   - Aktivera SSH
   - Användarnamn/lösenord
   - WiFi SSID/lösenord + WiFi-land
4. Flasha kortet och starta Raspberry Pi.

## 2. Första uppstart

```bash
ssh <användarnamn>@panel-kok.local
sudo apt update && sudo apt full-upgrade -y
sudo reboot
```

## 3. Aktivera automatisk inloggning på konsolen

```bash
sudo raspi-config
```

- **System Options → Boot / Auto Login → Console Autologin**
- **Localisation Options** → kontrollera att WiFi-land är satt (krävs för att radion ska slås på korrekt)

## 4. Installera minimal X-server, Chromium och verktyg

```bash
sudo apt install --no-install-recommends -y \
  xserver-xorg x11-xserver-utils xinit \
  chromium-browser unclutter
```

> På nyare Raspberry Pi OS heter paketet/kommandot ibland `chromium` istället för `chromium-browser`. Kontrollera med `command -v chromium || command -v chromium-browser` och justera `kiosk.sh` vid behov.

## 5. Hämta kiosk-skripten

```bash
git clone https://github.com/AndFagerberg/HomePanel.git
cp HomePanel/deploy/raspberry-pi/kiosk.sh ~/kiosk.sh
cp HomePanel/deploy/raspberry-pi/xinitrc ~/.xinitrc
chmod +x ~/kiosk.sh ~/.xinitrc
```

Sätt URL:en till backend (se avsnitt 26 i PROJECT.md — föredra ett stabilt hostname framför en hårdkodad IP):

```bash
echo 'export DASHBOARD_URL="http://household-panel.local:8080"' >> ~/.bash_profile
```

## 6. Starta X automatiskt vid inloggning

```bash
cat <<'EOF' >> ~/.bash_profile
if [ -z "$DISPLAY" ] && [ "$(tty)" = "/dev/tty1" ]; then
  exec startx
fi
EOF
```

## 7. Skärm och touch (endast vid behov)

Många 3,5"-skärmar kräver en tillverkarspecifik drivrutin (t.ex. Waveshare LCD-show) samt rotation. Följ skärmtillverkarens instruktioner och lägg till rotation i `/boot/firmware/config.txt`, t.ex.:

```text
display_rotate=1
```

Justera även touch-rotationen med `xinput` om touchpekaren hamnar fel efter skärmrotationen.

## 8. Stäng av WiFi-strömsparläge

Förhindrar att WiFi tappar anslutningen periodvis:

```bash
sudo iw wlan0 set power_save off
```

Lägg till samma kommando i `/etc/rc.local` (innan `exit 0`) så det körs vid varje omstart.

## 9. Testa

```bash
sudo reboot
```

Raspberry Pi ska nu starta direkt in i Chromium kiosk mode mot dashboarden, och Chromium ska starta om automatiskt om det kraschar (via loopen i `~/.xinitrc`).

## Felsökning

- Svart skärm vid boot → kontrollera `journalctl -xe` och att `startx` startar (`echo $DISPLAY` efter manuell `startx`).
- Kan inte nå backend → verifiera att servern svarar på `DASHBOARD_URL` från en annan dator på samma nätverk.
- WiFi tappas → kontrollera `iwconfig wlan0` för `Power Management:off`.
