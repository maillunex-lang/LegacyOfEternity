# Port-FromResources.ps1 — GUID-safe copy from C:\Source\Resources into LoE OriginalPort
# Usage:
#   .\Docs\scripts\Port-FromResources.ps1 -Slice 12_Shaders -RelativePath 'Resources\Shaders\Custom-Character.shader'
#   .\Docs\scripts\Port-FromResources.ps1 -Slice 07_Materials -RelativePath 'AssetsRaw\Animation\player\zhanshi\ZS_M_01\Materials'
# Does NOT modify the Resources tree. Always copies .meta beside the asset when present.

param(
    [Parameter(Mandatory = $true)][string]$Slice,
    [Parameter(Mandatory = $true)][string]$RelativePath,
    [string]$ResourcesRoot = 'C:\Source\Resources',
    [string]$DestRoot = 'C:\LegacyOfEternity\Client\Assets\OriginalPort',
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$src = Join-Path (Join-Path $ResourcesRoot $Slice) $RelativePath
if (-not (Test-Path $src)) { throw "Source not found: $src" }

$dest = Join-Path (Join-Path $DestRoot $Slice) $RelativePath
$destParent = if (Test-Path $src -PathType Container) { $dest } else { Split-Path $dest -Parent }
if (-not $DryRun) {
    New-Item -ItemType Directory -Force -Path $destParent | Out-Null
}

function Copy-Pair([string]$from) {
    $rel = $from.Substring((Join-Path $ResourcesRoot $Slice).Length).TrimStart('\','/')
    $to = Join-Path (Join-Path $DestRoot $Slice) $rel
    $toDir = Split-Path $to -Parent
    Write-Host "COPY $from -> $to"
    if ($DryRun) { return }
    New-Item -ItemType Directory -Force -Path $toDir | Out-Null
    Copy-Item -LiteralPath $from -Destination $to -Force
    $meta = "$from.meta"
    if (Test-Path -LiteralPath $meta) {
        Copy-Item -LiteralPath $meta -Destination "$to.meta" -Force
        Write-Host "  + meta"
    }
}

if (Test-Path $src -PathType Container) {
    Get-ChildItem -LiteralPath $src -Recurse -File | Where-Object { $_.Extension -ne '.meta' } | ForEach-Object {
        Copy-Pair $_.FullName
    }
} else {
    Copy-Pair $src
}

Write-Host 'Done. Update Docs/PORT-MANIFEST.md and Docs/ROADMAP.md. Open Unity once, then BuildOfflineAndroid if gameplay changed.'
Write-Host 'WARNING: never duplicate an asset that already exists in LegacyAssets with the same GUID.'
