<#
.SYNOPSIS
    Lists base-game item prefab addresses from the packed Addressables catalog
    (catalog_bas.json), so we can build new Valuable items (rings, gems, etc.)
    that reference base art with zero mod dependencies — exactly how the coin
    pouch references Bas.Item.Valuable.LootBag.

.DESCRIPTION
    Base-game items are packed in AssetBundles, so they don't show up in
    List-ItemIds.ps1 (which only reads loose mod JSON). Their prefab addresses,
    however, are listed as plain strings inside catalog_bas.json. This pulls
    them out and filters to the interesting ones.

.PARAMETER GameDir
    Path to your Blade & Sorcery install (folder containing BladeAndSorcery_Data).

.PARAMETER Filter
    Substring to match (default "Valuable"). Try "Valuable", "Ring", "Gem",
    "Jewel", "Goblet", "Crown", "Chalice", or "" for every Bas.Item.* address.

.EXAMPLE
    ./tools/Find-BaseLoot.ps1 -GameDir "E:\SteamLibrary\steamapps\common\Blade & Sorcery"

.EXAMPLE
    ./tools/Find-BaseLoot.ps1 -GameDir "E:\...\Blade & Sorcery" -Filter Ring
#>
param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Blade & Sorcery",
    [string]$Filter = "Valuable"
)

$catalog = Join-Path $GameDir "BladeAndSorcery_Data\StreamingAssets\Default\catalog_bas.json"
if (-not (Test-Path $catalog)) {
    Write-Error "catalog_bas.json not found at '$catalog'. Pass the correct -GameDir."
    exit 1
}

Write-Host "Scanning $catalog ..." -ForegroundColor Cyan

# Pull every "Bas.Item.<...>" address token out of the packed catalog.
$addresses = Select-String -Path $catalog -Pattern 'Bas\.Item\.[A-Za-z0-9_.]+' -AllMatches |
    ForEach-Object { $_.Matches } |
    ForEach-Object { $_.Value } |
    Sort-Object -Unique

if ($Filter) {
    $addresses = $addresses | Where-Object { $_ -match [regex]::Escape($Filter) }
}

Write-Host ("Found {0} address(es) matching '{1}':" -f @($addresses).Count, $Filter) -ForegroundColor Green
$addresses | ForEach-Object { Write-Host "  $_" }
