$ErrorActionPreference='Stop'
$stageRoot=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$PSScriptRoot/DifficultyBenchmark.cs")
$stageEvidence="$stageRoot/Client/Evidence/turn-timer-rating-20261015"
$before=(Get-Content -Raw -LiteralPath "$stageEvidence/stages-before.json" | ConvertFrom-Json).stages
$after=(Get-Content -Raw -LiteralPath "$stageRoot/GameData/Generated/stages.json" | ConvertFrom-Json).stages
$results=foreach($i in 0..29){
    $old=$before[$i];$new=$after[$i]
    $oldResult=[DifficultyBenchmark]::Run($old.starter,$old.openingMoves,$old.aiDepth,$old.aiNodeBudget,$old.aiRandomPermille,256,$true,0)
    $newResult=[DifficultyBenchmark]::Run($new.starter,$new.openingMoves,$new.aiDepth,$new.aiNodeBudget,$new.aiRandomPermille,256,$false,$new.aiOversightPermille)
    [pscustomobject]@{stage=$new.id;rating=$new.aiRating;before=$oldResult;after=$newResult}
}
$report=[ordered]@{
    method='Production AiSearch vs an immediate-win/block reactive proxy, otherwise random safe moves; same seeds 0..255. Before includes the actual first-five-turn depth>=4/random=0 override; after uses authored profiles throughout.'
    limitation='Internal rating labels are design tiers, not measured Elo. Bot win rates are not human win rates.'
    games=15360
    stages=$results
}
$report | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath "$stageEvidence/bot-comparison.json" -Encoding utf8
$results | ForEach-Object {[pscustomobject]@{stage=$_.stage;rating=$_.rating;beforePlayerWins=$_.before.playerWins;afterPlayerWins=$_.after.playerWins;games=256}} | Format-Table
if($results[1].after.playerWins -le $results[1].before.playerWins){throw 'Stage 2 did not become easier for the fixed proxy.'}
if(($results | Where-Object {$_.after.playerWins -lt $_.before.playerWins}).Count -gt 0){throw 'A stage became harder for the fixed proxy; inspect calibration.'}
