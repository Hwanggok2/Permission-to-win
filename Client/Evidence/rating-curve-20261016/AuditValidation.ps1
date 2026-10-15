$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/RatingCurveAudit.cs")
$fixtures=[RatingCurveAudit]::Generate(8,16442226)
$fixtures | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath "$PSScriptRoot/validation-suite.json" -Encoding utf8
$profileRows=Get-Content -Raw -LiteralPath "$PSScriptRoot/final-profiles.json" | ConvertFrom-Json
$results=foreach($p in $profileRows){
    $result=[RatingCurveAudit]::Evaluate($fixtures,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,16,841031)
    Write-Host ("Rating "+$p.aiRating+": "+(($result.metrics | ForEach-Object {$_.category+'='+[math]::Round($_.percent,1)}) -join ', '))
    [pscustomobject]@{profile=$p;result=$result}
}
[ordered]@{method='Final held-out validation after all profiles were fixed. 48 new independently solved boards (8 per horizon), 16 hashed seeds per stochastic profile and one per deterministic profile. Same positions and seeds across all 15 ratings. No subsequent tuning. Bounded tactics, not human win rate or Elo.';results=$results} | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath "$PSScriptRoot/validation-tactics.json" -Encoding utf8
