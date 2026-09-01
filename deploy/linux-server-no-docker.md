# Debian-server utan Docker

Den här varianten passar en svagare Linux-server: bygg Angular och .NET på utvecklingsmaskinen, kopiera en färdig publish-katalog till Debian-servern och kör appen som en `systemd`-tjänst.

Servern behöver inte Node.js, npm, Angular CLI, .NET SDK eller Docker. Den behöver bara kunna köra den färdiga ASP.NET Core-appen.

## 1. Bygg på utvecklingsmaskinen

Från repo-roten på utvecklingsmaskinen:

```powershell
Push-Location frontend
npm ci
npm run build -- --configuration production
Pop-Location

Remove-Item -Recurse -Force src/HouseholdPanel.Api/wwwroot -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force src/HouseholdPanel.Api/wwwroot | Out-Null
Copy-Item -Recurse frontend/dist/frontend/browser/* src/HouseholdPanel.Api/wwwroot/

dotnet publish src/HouseholdPanel.Api/HouseholdPanel.Api.csproj `
  -c Release `
  -r linux-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -o publish/homepanel-linux-x64
```

Detta skapar en färdig release i `publish/homepanel-linux-x64`.

För en ARM-baserad server, byt runtime identifier till exempelvis `linux-arm64` eller `linux-arm`.

## 2. Kopiera till Debian-servern

Exempel med `scp` från utvecklingsmaskinen:

```powershell
scp -r publish/homepanel-linux-x64/* andy@linuxserver:/tmp/homepanel/
```

På servern:

```bash
sudo mkdir -p /opt/homepanel
sudo cp -r /tmp/homepanel/* /opt/homepanel/
sudo chown -R root:root /opt/homepanel
sudo chmod +x /opt/homepanel/HouseholdPanel.Api
```

## 3. Skapa systemd-tjänst

På servern:

```bash
sudo tee /etc/systemd/system/homepanel.service > /dev/null <<'EOF'
[Unit]
Description=Household Panel
After=network-online.target
Wants=network-online.target

[Service]
WorkingDirectory=/opt/homepanel
ExecStart=/opt/homepanel/HouseholdPanel.Api
Restart=always
RestartSec=5
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080

[Install]
WantedBy=multi-user.target
EOF
```

Starta tjänsten:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now homepanel
sudo systemctl status homepanel
```

## 4. Testa

På servern:

```bash
curl http://localhost:8080/api/dashboard
```

Från en annan dator på nätverket:

```bash
curl http://linuxserver:8080/api/dashboard
```

Om det fungerar lokalt men inte från nätverket, kontrollera brandväggen:

```bash
sudo ufw allow 8080/tcp
```

## 5. Uppdatera Raspberry Pi

Peka kioskklienten mot Debian-servern:

```bash
echo 'export DASHBOARD_URL="http://linuxserver:8080"' >> ~/.bash_profile
```

Använd gärna ett stabilt lokalt DNS-namn i Pi-hole, exempelvis `homepanel.lan`, och sätt då URL:en till:

```bash
echo 'export DASHBOARD_URL="http://homepanel.lan:8080"' >> ~/.bash_profile
```

## Uppdatera appen senare

Det enklaste är att köra deploy-scriptet från repo-roten på utvecklingsmaskinen. Det bygger frontend, publicerar backend för Linux, kopierar artefakten till `homepanel.lan`, installerar eller uppdaterar `systemd`-tjänsten och testar `/api/dashboard` lokalt på servern. Efter omstart väntar scriptet upp till 20 sekunder på att appen ska börja svara.

```powershell
.\deploy\deploy-homepanel.ps1
```

Första gången kan du få ange SSH-lösenord för uppladdningen, SSH-lösenord för remote-körningen och `sudo`-lösenord för användaren på servern. Scriptet kör remote-installationen med TTY så att `sudo` kan fråga efter lösenord normalt. Vill du slippa SSH-lösenorden helt kan du lägga in en SSH-nyckel för `andy@homepanel.lan`.

Scriptets standardvärden är:

```powershell
.\deploy\deploy-homepanel.ps1 `
  -HostName homepanel.lan `
  -User andy `
  -Runtime linux-x64 `
  -Port 8080
```

Om servern är ARM-baserad, byt runtime:

```powershell
.\deploy\deploy-homepanel.ps1 -Runtime linux-arm64
```

Om `npm ci` redan är kört och `package-lock.json` inte har ändrats kan du hoppa över installationen för snabbare deploy:

```powershell
.\deploy\deploy-homepanel.ps1 -SkipNpmCi
```

Scriptet motsvarar dessa manuella steg på utvecklingsmaskinen:

```powershell
Push-Location frontend
npm ci
npm run build -- --configuration production
Pop-Location

Remove-Item -Recurse -Force src/HouseholdPanel.Api/wwwroot -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force src/HouseholdPanel.Api/wwwroot | Out-Null
Copy-Item -Recurse frontend/dist/frontend/browser/* src/HouseholdPanel.Api/wwwroot/

dotnet publish src/HouseholdPanel.Api/HouseholdPanel.Api.csproj `
  -c Release `
  -r linux-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -o publish/homepanel-linux-x64

tar -czf publish/homepanel-linux-x64.tar.gz -C publish/homepanel-linux-x64 .
scp publish/homepanel-linux-x64.tar.gz andy@homepanel.lan:/tmp/homepanel-linux-x64.tar.gz
```

Och dessa steg på servern:

```bash
sudo mkdir -p /opt/homepanel
sudo tar -xzf /tmp/homepanel-linux-x64.tar.gz -C /opt/homepanel
sudo chown -R root:root /opt/homepanel
sudo chmod +x /opt/homepanel/HouseholdPanel.Api
sudo systemctl daemon-reload
sudo systemctl enable --now homepanel
sudo systemctl restart homepanel
curl http://localhost:8080/api/dashboard
```