$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/RatingCurveAudit.cs","$stageRoot/Client/Tools/RatingMatchAudit.cs","$stageRoot/Client/Tools/ShortGameAudit.cs")
$profileRows=Get-Content -Raw -LiteralPath "$PSScriptRoot/candidate-b-profiles.json" | ConvertFrom-Json
$profileRows[4].aiDepth=4;$profileRows[4].aiNodeBudget=4000
$profileRows[5].aiDepth=4;$profileRows[5].aiNodeBudget=6000
$profileRows | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath "$PSScriptRoot/final-profiles.json" -Encoding utf8
$primary=Get-Content -Raw -LiteralPath "$PSScriptRoot/candidate-b-tactics.json" | ConvertFrom-Json
$secondary=Get-Content -Raw -LiteralPath "$PSScriptRoot/holdout-comparison.json" | ConvertFrom-Json
$matches=Get-Content -Raw -LiteralPath "$PSScriptRoot/candidate-b-matches.json" | ConvertFrom-Json
$primaryBoards=[RatingCurveAudit+Fixture[]](Get-Content -Raw -LiteralPath "$PSScriptRoot/tactical-suite.json" | ConvertFrom-Json)
$secondaryBoards=[RatingCurveAudit+Fixture[]](Get-Content -Raw -LiteralPath "$PSScriptRoot/holdout-suite.json" | ConvertFrom-Json)
foreach($i in @(4,5)){
    $p=$profileRows[$i]
    $primary.results[$i].profile=$p
    $primary.results[$i].result=[RatingCurveAudit]::Evaluate($primaryBoards,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,64)
    $secondary.results[$i].after=[RatingCurveAudit]::Evaluate($secondaryBoards,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,32,312007)
    $matches.results[$i].profile=$p
    $matches.results[$i].result=[RatingMatchAudit]::Run($p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,8)
    Write-Host ("Rating "+$p.aiRating+": "+(($secondary.results[$i].after.metrics | ForEach-Object {$_.category+'='+[math]::Round($_.percent,1)}) -join ', ')+', matchScore='+$matches.results[$i].result.scorePercent)
}
$secondary.method='96-board secondary suite (16 per horizon, 32 hashed seeds). Initially held out from candidate B; its 600 attack regression prompted the final depth-4 change at 600/700. Values for unchanged profiles reuse the exact candidate B evaluation. Final independent validation is separate in validation-tactics.json. Bounded tactical scores, not human win rates.'
$primary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath "$PSScriptRoot/final-tactics.json" -Encoding utf8
$secondary | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath "$PSScriptRoot/final-secondary-comparison.json" -Encoding utf8
$matches | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath "$PSScriptRoot/final-matches.json" -Encoding utf8
