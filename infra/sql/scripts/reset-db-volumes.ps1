$ErrorActionPreference = "Stop"
$repoRoot = if ($PSScriptRoot) { (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path } else { (Get-Location).Path }
$volumesDir = Join-Path $repoRoot "volumes"

if (-not (Test-Path $volumesDir)) {
    Write-Host "No volumes directory found. Nothing to clear."
    exit 0
}

$dirs = Get-ChildItem -Path $volumesDir -Directory -ErrorAction SilentlyContinue
foreach ($d in $dirs) {
    try {
        Remove-Item -Path (Join-Path $d.FullName "*") -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Cleared: $($d.FullName)"
    } catch {
        Write-Warning "Could not clear $($d.FullName): $_"
    }
}

try { icacls $volumesDir /grant:r "Users:(OI)(CI)F" /T | Out-Null } catch { }
Write-Host "Done. Start AppHost again for fresh DB init."
