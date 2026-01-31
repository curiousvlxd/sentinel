# Reset Postgres/TimescaleDB volumes (fixes corrupt data, e.g. pg_filenode.map errors).
# Stop AppHost before running. Run from repo root: .\infra\scripts\reset-db-volumes.ps1

$ErrorActionPreference = "Stop"
$repoRoot = if ($PSScriptRoot) { (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path } else { (Get-Location).Path }
$volumesDir = Join-Path $repoRoot "volumes"

$dirs = @(
    (Join-Path $volumesDir "ground-db"),
    (Join-Path $volumesDir "satellite-db-airbus-sentinel-1"),
    (Join-Path $volumesDir "satellite-db-airbus-sentinel-2"),
    (Join-Path $volumesDir "satellite-db-airbus-sentinel-3")
)

foreach ($d in $dirs) {
    if (Test-Path $d) {
        Remove-Item -Path (Join-Path $d "*") -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Cleared: $d"
    }
}

if (Test-Path $volumesDir) {
    try { icacls $volumesDir /grant:r "Users:(OI)(CI)F" /T | Out-Null } catch { }
    Write-Host "Permissions set on $volumesDir"
}

Write-Host "Done. Start AppHost again for fresh DB init."
