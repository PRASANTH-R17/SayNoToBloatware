<#
.SYNOPSIS
    Decodes iconBase64 fields from /apps/full JSON and exports each app icon as a PNG.

.EXAMPLE
    .\Export-AppIcons.ps1
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$InputJson = 'C:\Users\Prasanth\Downloads\response\response.json'
$OutputDir =  'C:\Users\Prasanth\Downloads\response\'

function Get-SafeFileName {
    param([string] $PackageName)
    $invalid = [IO.Path]::GetInvalidFileNameChars() -join ''
    $pattern = "[{0}]" -f [Regex]::Escape($invalid)
    return ($PackageName -replace $pattern, '_')
}

if (-not (Test-Path -LiteralPath $InputJson)) {
    throw "Input file not found: $InputJson"
}

Write-Host "Reading: $InputJson"
$apps = Get-Content -LiteralPath $InputJson -Raw -Encoding UTF8 | ConvertFrom-Json

if (-not $apps) {
    throw 'JSON is empty or not a valid app array.'
}

if (-not (Test-Path -LiteralPath $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$exported = 0
$skipped = 0
$failed = 0

foreach ($app in $apps) {
    $packageName = $app.packageName
    $label = $app.label
    $iconBase64 = $app.iconBase64

    if ([string]::IsNullOrWhiteSpace($packageName)) {
        $skipped++
        continue
    }

    if ([string]::IsNullOrWhiteSpace($iconBase64)) {
        Write-Warning "No icon for: $packageName ($label)"
        $skipped++
        continue
    }

    $safeName = Get-SafeFileName -PackageName $packageName
    $outPath = Join-Path $OutputDir "$safeName.png"

    try {
        $bytes = [Convert]::FromBase64String($iconBase64)

        if ($bytes.Length -lt 4 -or $bytes[0] -ne 0x89 -or $bytes[1] -ne 0x50 -or $bytes[2] -ne 0x4E -or $bytes[3] -ne 0x47) {
            Write-Warning "Invalid PNG data for: $packageName"
            $failed++
            continue
        }

        [IO.File]::WriteAllBytes($outPath, $bytes)
        $exported++
    }
    catch {
        Write-Warning "Failed to decode $packageName : $_"
        $failed++
    }
}

Write-Host ''
Write-Host 'Export complete'
Write-Host "  Total apps : $($apps.Count)"
Write-Host "  Exported   : $exported"
Write-Host "  Skipped    : $skipped"
Write-Host "  Failed     : $failed"
Write-Host "  Output dir : $OutputDir"
