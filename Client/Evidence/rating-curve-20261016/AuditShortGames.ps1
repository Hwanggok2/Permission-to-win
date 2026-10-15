$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/ShortGameAudit.cs")
$profileRows=(Get-Content -Raw -LiteralPath "$stageRoot/GameData/Generated/stages.json" | ConvertFrom-Json).stages | Where-Object {($_.id%2) -eq 1 -and $_.aiRating -ge 700}
$results=foreach($p in $profileRows){
    foreach($starter in @(1,-1)){
        $result=[ShortGameAudit]::Run($starter,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,2)
        Write-Host ("Rating "+$p.aiRating+", starter="+$starter+": "+$result.playerBranches+' branches, '+$result.earlyPlayerWins+' early losses')
        [pscustomobject]@{rating=$p.aiRating;result=$result}
    }
}
[ordered]@{method='Every legal human move through their fourth placement, empty board, both starter colors, seeds 0 and 1, all ten authored ratings 700..1600. Bounded regression only; not a guarantee for all seeds or middle/late game positions.';results=$results} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath "$PSScriptRoot/short-games.json" -Encoding utf8
if($results | Where-Object {$_.result.earlyPlayerWins -gt 0}){throw 'An authored mid/high rating lost within seven/eight plies'}
