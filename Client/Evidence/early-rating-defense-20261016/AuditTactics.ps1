$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/RatingCurveAudit.cs")
$fixtures=[RatingCurveAudit]::Generate(8,20261017)
$fixtures | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath "$PSScriptRoot/tactical-suite.json" -Encoding utf8
$profiles=Get-Content -Raw -LiteralPath "$PSScriptRoot/candidate-profiles.json" | ConvertFrom-Json
$results=foreach($p in $profiles){
    $result=[RatingCurveAudit]::Evaluate($fixtures,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,16,841032)
    Write-Host ("Rating "+$p.aiRating+": "+(($result.metrics | ForEach-Object {$_.category+'='+[math]::Round($_.percent,1)}) -join ', '))
    [pscustomobject]@{profile=$p;result=$result}
}
[ordered]@{method='48 fresh independently solved boards, 8 per horizon (attack 1/3/5 and defense 2/4/6); 16 hashed seeds per stochastic profile, one per deterministic profile. Bounded tactics, not human win rate or Elo.';results=$results} | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath "$PSScriptRoot/tactics.json" -Encoding utf8
