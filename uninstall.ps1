# Shellmate entfernen
# powershell -c "irm https://raw.githubusercontent.com/Elias-Phy-Ja/Lernperiode-2/main/uninstall.ps1 | iex"
# Loescht shellmate.exe, den PATH-Eintrag und die Config (inkl. gespeichertem API Key).

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\Shellmate'
$configDir  = Join-Path $env:APPDATA 'Shellmate'

# 1. Programm loeschen
if (Test-Path $installDir) {
    Remove-Item -Recurse -Force $installDir
}

# 2. Config mit API Key loeschen
if (Test-Path $configDir) {
    Remove-Item -Recurse -Force $configDir
}

# 3. PATH-Eintrag entfernen
$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
if ($userPath) {
    $newPath = ($userPath -split ';' | Where-Object { $_ -and $_ -ne $installDir }) -join ';'
    [Environment]::SetEnvironmentVariable('Path', $newPath, 'User')
}

Write-Host ''
Write-Host '  Shellmate wurde entfernt.' -ForegroundColor Green
Write-Host ''
