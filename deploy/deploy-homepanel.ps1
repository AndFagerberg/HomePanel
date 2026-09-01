param(
    [string]$HostName = "homepanel.lan",
    [string]$User = "andy",
    [string]$Runtime = "linux-x64",
    [int]$Port = 8080,
    [switch]$SkipNpmCi
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$frontendDir = Join-Path $repoRoot "frontend"
$apiProject = Join-Path $repoRoot "src/HouseholdPanel.Api/HouseholdPanel.Api.csproj"
$apiWwwroot = Join-Path $repoRoot "src/HouseholdPanel.Api/wwwroot"
$frontendBuild = Join-Path $frontendDir "dist/frontend/browser"
$publishRoot = Join-Path $repoRoot "publish"
$publishDir = Join-Path $publishRoot "homepanel-$Runtime"
$archivePath = Join-Path $publishRoot "homepanel-$Runtime.tar.gz"
$remoteScriptPath = Join-Path $publishRoot "homepanel-install-$Runtime.sh"
$remote = "$User@$HostName"
$remoteArchivePath = "/tmp/homepanel-$Runtime.tar.gz"
$remoteInstallScriptPath = "/tmp/homepanel-install-$Runtime.sh"

function Invoke-Step {
    param(
        [string]$Title,
        [scriptblock]$Command
    )

    Write-Host "`n==> $Title"
    & $Command
}

function Invoke-Native {
    param(
        [string]$FilePath,
        [string[]]$Arguments = @()
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
    }
}

Push-Location $repoRoot
try {
    Invoke-Step "Build frontend" {
        Push-Location $frontendDir
        try {
            if (-not $SkipNpmCi) {
                Invoke-Native npm @("ci")
            }

            Invoke-Native npm @("run", "build", "--", "--configuration", "production")
        }
        finally {
            Pop-Location
        }
    }

    Invoke-Step "Copy frontend build into API wwwroot" {
        Remove-Item -Recurse -Force $apiWwwroot -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force $apiWwwroot | Out-Null
        Copy-Item -Recurse (Join-Path $frontendBuild "*") $apiWwwroot
    }

    Invoke-Step "Publish backend for $Runtime" {
        Remove-Item -Recurse -Force $publishDir -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force $publishDir | Out-Null

        Invoke-Native dotnet @(
            "publish",
            $apiProject,
            "-c",
            "Release",
            "-r",
            $Runtime,
            "--self-contained",
            "true",
            "-p:PublishSingleFile=true",
            "-p:EnableCompressionInSingleFile=true",
            "-o",
            $publishDir
        )
    }

    Invoke-Step "Create deployment archive" {
        Remove-Item -Force $archivePath -ErrorAction SilentlyContinue
        Invoke-Native tar @("-czf", $archivePath, "-C", $publishDir, ".")
    }

    Invoke-Step "Create remote install script" {
        $remoteScript = @"
set -e
sudo mkdir -p /opt/homepanel
if systemctl list-unit-files homepanel.service > /dev/null 2>&1; then
    sudo systemctl stop homepanel || true
fi
sudo find /opt/homepanel -mindepth 1 -maxdepth 1 -exec rm -rf {} +
sudo tar -xzf '$remoteArchivePath' -C /opt/homepanel
sudo chown -R root:root /opt/homepanel
sudo chmod +x /opt/homepanel/HouseholdPanel.Api
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
Environment=ASPNETCORE_URLS=http://0.0.0.0:$Port

[Install]
WantedBy=multi-user.target
EOF
sudo systemctl daemon-reload
sudo systemctl enable --now homepanel
sudo systemctl restart homepanel
rm -f '$remoteArchivePath' '$remoteInstallScriptPath'
for attempt in {1..20}; do
    if curl -fsS 'http://localhost:$Port/api/dashboard' > /dev/null 2>&1; then
        exit 0
    fi

    if ! systemctl is-active --quiet homepanel; then
        sudo systemctl status homepanel --no-pager
        sudo journalctl -u homepanel -n 80 --no-pager
        exit 1
    fi

    sleep 1
done

sudo systemctl status homepanel --no-pager
sudo journalctl -u homepanel -n 80 --no-pager
exit 1
"@

    $remoteScript = $remoteScript -replace "`r`n", "`n"
    [System.IO.File]::WriteAllText($remoteScriptPath, $remoteScript, [System.Text.UTF8Encoding]::new($false))
    }

    Invoke-Step "Upload archive and install script to $remote" {
        Invoke-Native scp @($archivePath, $remoteScriptPath, "$remote`:/tmp/")
    }

    Invoke-Step "Install and restart service on $HostName" {
        Invoke-Native ssh @("-tt", $remote, "bash '$remoteInstallScriptPath'")
    }

    Write-Host "`nDeployment complete: http://$HostName`:$Port"
}
finally {
    Pop-Location
}