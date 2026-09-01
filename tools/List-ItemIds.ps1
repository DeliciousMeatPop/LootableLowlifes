<#
.SYNOPSIS
    Lists real Blade & Sorcery item IDs by scanning the game's (and mods')
    ThunderRoad JSON catalog. Use the printed "id" values directly in
    mod/Item_LootTables.json so you never have to guess or test blindly.

.DESCRIPTION
    Every item in Blade & Sorcery is defined by an ItemData JSON file on disk.
    This script recurses the StreamingAssets tree, finds those files, and prints
    each item's id, type, and (when present) a readable name. It covers the base
    game AND every installed mod (Gilded Goons, etc.).

.PARAMETER GameDir
    Path to your Blade & Sorcery install (the folder containing
    BladeAndSorcery_Data). Defaults to a common Steam location.

.PARAMETER Filter
    Optional case-insensitive substring to match against id/type/name
    (e.g. "sword", "ring", "currency", "gold").

.PARAMETER IncludeMods
    Also scan StreamingAssets\Mods. On by default; use -IncludeMods:$false to
    restrict to base-game items only.

.EXAMPLE
    ./tools/List-ItemIds.ps1 -GameDir "D:\SteamLibrary\steamapps\common\Blade & Sorcery"

.EXAMPLE
    ./tools/List-ItemIds.ps1 -GameDir "D:\...\Blade & Sorcery" -Filter sword | Sort-Object Id
#>
param(
    [string]$GameDir = "D:\SteamLibrary\steamapps\common\Blade & Sorcery",
    [string]$Filter = "",
    [bool]$IncludeMods = $true
)

$streaming = Join-Path $GameDir "BladeAndSorcery_Data\StreamingAssets"
if (-not (Test-Path $streaming)) {
    Write-Error "StreamingAssets not found under '$GameDir'. Pass the correct -GameDir."
    exit 1
}

# Scan the base catalog plus (optionally) installed mods.
$roots = @((Join-Path $streaming "Default"))
if ($IncludeMods) { $roots += (Join-Path $streaming "Mods") }
$roots = $roots | Where-Object { Test-Path $_ }

$results = New-Object System.Collections.Generic.List[object]

foreach ($root in $roots) {
    Get-ChildItem -Path $root -Recurse -Filter *.json -File -ErrorAction SilentlyContinue | ForEach-Object {
        $file = $_
        try {
            $json = Get-Content -Raw -LiteralPath $file.FullName | ConvertFrom-Json
        } catch {
            return  # skip files that aren't valid JSON
        }

        # ItemData files carry a "$type" naming ThunderRoad.ItemData and an "id".
        $type = $json.'$type'
        if ($type -notlike "*ItemData*") { return }
        if (-not $json.id) { return }

        $results.Add([pscustomobject]@{
            Id       = [string]$json.id
            Type     = [string]$json.type          # Weapon / Misc / Quiver / etc.
            Name     = [string]$json.displayName    # often a localization key, sometimes readable
            Source   = if ($file.FullName -like "*\Mods\*") {
                           ($file.FullName -replace ".*\\Mods\\([^\\]+)\\.*", '$1')
                       } else { "Base" }
        }) | Out-Null
    }
}

$out = $results | Sort-Object Id -Unique
if ($Filter) {
    $out = $out | Where-Object {
        $_.Id -match [regex]::Escape($Filter) -or
        $_.Type -match [regex]::Escape($Filter) -or
        $_.Name -match [regex]::Escape($Filter)
    }
}

Write-Host ("Found {0} item id(s)." -f @($out).Count) -ForegroundColor Cyan
$out | Format-Table -AutoSize
