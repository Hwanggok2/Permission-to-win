$ErrorActionPreference = 'Stop'
$stageRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Add-Type -Path @(
    "$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs",
    "$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs",
    "$PSScriptRoot/DifficultyBenchmark.cs"
)
$stageEvidence = "$stageRoot/Client/Evidence/difficulty-20261015"
$stageBefore = (Get-Content -Raw -LiteralPath "$stageEvidence/stages-before.json" | ConvertFrom-Json).stages
$stageAfter = (Get-Content -Raw -LiteralPath "$stageRoot/GameData/Generated/stages.json" | ConvertFrom-Json).stages
$stageResults = foreach ($stageIndex in 0..9) {
    $stageRecord = [ordered]@{ id = $stageIndex+1 }
    foreach ($stageVersion in @('before','after')) {
        $stageDefinition = if ($stageVersion -eq 'before') { $stageBefore[$stageIndex] } else { $stageAfter[$stageIndex] }
        $stageRecord[$stageVersion] = [DifficultyBenchmark]::Run($stageDefinition.starter,$stageDefinition.openingMoves,$stageDefinition.aiDepth,$stageDefinition.aiNodeBudget,$stageDefinition.aiRandomPermille,256)
    }
    [pscustomobject]$stageRecord
}
$stageReport = [ordered]@{
    method = 'Production AiSearch vs a reactive player that wins/blocks immediately, otherwise chooses random safe moves; seeds 0..255 per stage and revision.'
    limitation = 'Bot comparison only. These percentages are not human win rates or proof of perceived difficulty.'
    stages = $stageResults
}
$stageReport | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath "$stageEvidence/bot-comparison.json" -Encoding utf8
$stageResults | ForEach-Object { [pscustomobject]@{stage=$_.id;beforePlayerWins=$_.before.playerWins;afterPlayerWins=$_.after.playerWins;games=256} } | Format-Table
