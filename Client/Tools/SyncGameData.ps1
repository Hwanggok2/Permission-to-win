param([string]$Source = (Join-Path $PSScriptRoot '../../GameData/Generated'))
$ErrorActionPreference = 'Stop'
$destination = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../Assets/StageGame/Resources/GameData'))
foreach ($name in @('stages.json', 'ads.json', 'strings.json')) {
    $file = Join-Path $Source $name
    $parsed = Get-Content -LiteralPath $file -Raw -Encoding utf8 | ConvertFrom-Json
    if ($parsed.schemaVersion -ne 1) { throw "Unsupported schemaVersion: $name" }
    if ($name -eq 'stages.json' -and $parsed.stages.Count -ne 30) { throw 'Expected exactly 30 stages.' }
    Copy-Item -LiteralPath $file -Destination (Join-Path $destination $name) -Force
    $originalHash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
    $copiedHash = (Get-FileHash -LiteralPath (Join-Path $destination $name) -Algorithm SHA256).Hash
    if ($originalHash -ne $copiedHash) { throw "Copy hash mismatch: $name" }
    Write-Output "$name $copiedHash"
}
