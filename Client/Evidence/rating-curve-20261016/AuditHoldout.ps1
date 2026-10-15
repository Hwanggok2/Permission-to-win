$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/RatingCurveAudit.cs")
$fixtures=[RatingCurveAudit]::Generate(16,16002026)
$fixtures | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath "$PSScriptRoot/holdout-suite.json" -Encoding utf8
$before=(Get-Content -Raw -LiteralPath "$PSScriptRoot/stages-before.json" | ConvertFrom-Json).stages | Where-Object {($_.id%2) -eq 1}
$after=Get-Content -Raw -LiteralPath "$PSScriptRoot/candidate-b-profiles.json" | ConvertFrom-Json
$results=foreach($i in 0..14){
    $pair=foreach($p in @($before[$i],$after[$i])){
        [RatingCurveAudit]::Evaluate($fixtures,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,32,312007)
    }
    Write-Host ("Rating "+$after[$i].aiRating+": "+(($pair[1].metrics | ForEach-Object {$_.category+'='+[math]::Round($_.percent,1)}) -join ', '))
    [pscustomobject]@{rating=$after[$i].aiRating;before=$pair[0];after=$pair[1]}
}
[ordered]@{method='Fresh holdout generated after candidate B was fixed; 96 independently solved boards, 16 per tactical horizon 1..6. 32 hashed seeds per stochastic profile; deterministic profiles once per board. Same boards and seeds before/after. No profile tuning from this holdout. Bounded tactical outcomes, not human win rates.';results=$results} | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath "$PSScriptRoot/holdout-comparison.json" -Encoding utf8
