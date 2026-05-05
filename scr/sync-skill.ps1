[CmdletBinding()]
param(
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$workspaceRoot = (Get-Item $repoRoot).Parent.Parent.FullName

$source = Join-Path $workspaceRoot '.github/skills/parse-swagger'
$target = Join-Path $repoRoot 'src/KoreForge.SwaggerControllers.Template/templates/koreforge-swagger-controllers/.github/skills/parse-swagger'

if (-not (Test-Path $source)) {
    Write-Host "Source skill not found at $source; nothing to sync (this is normal outside the KoreForge2 workspace)." -ForegroundColor Yellow
    exit 0
}

$sourceFiles = Get-ChildItem -Path $source -Recurse -File | Sort-Object FullName
$differences = @()

foreach ($sourceFile in $sourceFiles) {
    $relative = $sourceFile.FullName.Substring($source.Length).TrimStart('\','/')
    $targetFile = Join-Path $target $relative

    if (-not (Test-Path $targetFile)) {
        $differences += "missing: $relative"
        if (-not $Check) {
            New-Item -ItemType Directory -Force -Path (Split-Path $targetFile -Parent) | Out-Null
            Copy-Item $sourceFile.FullName $targetFile -Force
        }
        continue
    }

    $sourceHash = (Get-FileHash $sourceFile.FullName -Algorithm SHA256).Hash
    $targetHash = (Get-FileHash $targetFile -Algorithm SHA256).Hash
    if ($sourceHash -ne $targetHash) {
        $differences += "drift: $relative"
        if (-not $Check) {
            Copy-Item $sourceFile.FullName $targetFile -Force
        }
    }
}

# Detect orphans in target
if (Test-Path $target) {
    $targetFiles = Get-ChildItem -Path $target -Recurse -File | Sort-Object FullName
    foreach ($targetFile in $targetFiles) {
        $relative = $targetFile.FullName.Substring($target.Length).TrimStart('\','/')
        $sourceFile = Join-Path $source $relative
        if (-not (Test-Path $sourceFile)) {
            $differences += "orphan: $relative"
            if (-not $Check) {
                Remove-Item $targetFile.FullName -Force
            }
        }
    }
}

if ($differences.Count -eq 0) {
    Write-Host 'parse-swagger skill is in sync.' -ForegroundColor Green
    exit 0
}

if ($Check) {
    Write-Host 'parse-swagger skill is OUT OF SYNC:' -ForegroundColor Red
    $differences | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    Write-Host "Run 'pwsh scr/sync-skill.ps1' to repair." -ForegroundColor Yellow
    exit 1
}

Write-Host 'parse-swagger skill synced:' -ForegroundColor Green
$differences | ForEach-Object { Write-Host "  $_" -ForegroundColor Cyan }
