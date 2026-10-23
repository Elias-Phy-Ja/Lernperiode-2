# Shellmate Installer
# Installieren:  powershell -c "irm https://raw.githubusercontent.com/Elias-Phy-Ja/Lernperiode-2/main/install.ps1 | iex"
# Laedt die neueste shellmate.exe von GitHub, legt sie in %LOCALAPPDATA%\Programs\Shellmate
# und fuegt diesen Ordner zum PATH hinzu, damit "shellmate" in jedem Terminal funktioniert.
# Nochmal ausfuehren = auf die neueste Version aktualisieren.

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'   # macht den Download deutlich schneller

$repo        = 'Elias-Phy-Ja/Lernperiode-2'
$installDir  = Join-Path $env:LOCALAPPDATA 'Programs\Shellmate'
$exePath     = Join-Path $installDir 'shellmate.exe'
$downloadUrl = "https://github.com/$repo/releases/latest/download/shellmate.exe"

Write-Host ''
Write-Host '  Shellmate wird installiert...' -ForegroundColor Cyan

# 1. Ordner anlegen
New-Item -ItemType Directory -Force -Path $installDir | Out-Null

# 2. shellmate.exe herunterladen
try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest -Uri $downloadUrl -OutFile $exePath -UseBasicParsing
}
catch {
    Write-Host ''
    Write-Host '  Download fehlgeschlagen.' -ForegroundColor Red
    Write-Host '  Laeuft Shellmate gerade? Dann zuerst schliessen und nochmal versuchen.'
    Write-Host "  Details: $($_.Exception.Message)"
    return
}

# 3. Ordner zum PATH des Benutzers hinzufuegen (nur einmal)
$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
$paths = if ($userPath) { $userPath -split ';' } else { @() }

if ($paths -notcontains $installDir) {
    $newPath = (@($paths | Where-Object { $_ }) + $installDir) -join ';'
    [Environment]::SetEnvironmentVariable('Path', $newPath, 'User')
    $env:Path = "$env:Path;$installDir"   # gilt sofort auch in diesem Fenster
}

Write-Host ''
Write-Host '  Shellmate wurde installiert!' -ForegroundColor Green
Write-Host ''
Write-Host '  Oeffne ein neues Terminal (CMD oder PowerShell) und tippe:'
Write-Host '      shellmate' -ForegroundColor Cyan
Write-Host ''
