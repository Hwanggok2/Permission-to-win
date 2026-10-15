param([string]$Label='baseline',[string]$Profiles='')
$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/RatingCurveAudit.cs")
$fixturePath="$PSScriptRoot/tactical-suite.json"
if(-not (Test-Path -LiteralPath $fixturePath)){
    $suite=[RatingCurveAudit]::Generate(8)
    $suite | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $fixturePath -Encoding utf8
    Write-Host "Generated $($suite.Count) independently solved tactical boards."
}
$fixtures=[RatingCurveAudit+Fixture[]](Get-Content -Raw -LiteralPath $fixturePath | ConvertFrom-Json)
if($Profiles){$profileRows=Get-Content -Raw -LiteralPath $Profiles | ConvertFrom-Json}
else{$profileRows=(Get-Content -Raw -LiteralPath "$stageRoot/GameData/Generated/stages.json" | ConvertFrom-Json).stages | Where-Object {($_.id%2) -eq 1}}
$results=foreach($p in $profileRows){
    $result=[RatingCurveAudit]::Evaluate($fixtures,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,64)
    Write-Host ("Rating "+$p.aiRating+": "+(($result.metrics | ForEach-Object {$_.category+'='+[math]::Round($_.percent,1)}) -join ', '))
    [pscustomobject]@{profile=$p;result=$result}
}
[ordered]@{method='48 fixed boards generated independently with exact bounded win/loss minimax before calibration; 8 boards each for immediate attack/defense and 3/4/5/6-ply tactics. 64 seeds per stochastic profile; deterministic profiles run once per board. Percent is best bounded tactical outcome, not Elo or human win rate.';results=$results} | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath "$PSScriptRoot/$Label-tactics.json" -Encoding utf8
