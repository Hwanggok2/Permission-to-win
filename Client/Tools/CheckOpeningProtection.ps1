$ErrorActionPreference='Stop'
$stageRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$PSScriptRoot/OpeningAudit.cs")
$stageReports=@([OpeningAudit]::Run(1,''),[OpeningAudit]::Run(-1,''))
$stageCatalog=Get-Content -LiteralPath "$stageRoot/GameData/Generated/stages.json" -Raw | ConvertFrom-Json
foreach($stageOpening in ($stageCatalog.stages | Where-Object { $_.openingMoves } | Select-Object -ExpandProperty openingMoves -Unique)) {
    $stageReports += [OpeningAudit]::Run(1,$stageOpening)
}
$stageReports | ConvertTo-Json | Set-Content -LiteralPath "$stageRoot/Client/Evidence/opening-ranks-20261015/opening-protection-audit.json" -Encoding utf8
$stageReports | Format-List
if(@($stageReports | Where-Object { $_.earlyPlayerWins -gt 0 -or $_.maxNodes -gt 3500 }).Count -gt 0) {
    throw 'Opening protection regression failed; inspect opening-protection-audit.json.'
}
