$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
$toolSource=Get-Content -Raw -LiteralPath "$stageRoot/Client/Tools/ShortGameAudit.cs"
$toolSource.Replace('using StageGame;','using StageGameBefore;').Replace('class ShortGameAudit','class ShortGameAuditBefore') | Set-Content -LiteralPath "$PSScriptRoot/ShortGameAudit-before.cs" -Encoding utf8
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/ShortGameAudit.cs","$PSScriptRoot/ConnectFourBoard-baseline.cs","$PSScriptRoot/AiSearch-baseline.cs","$PSScriptRoot/ShortGameAudit-before.cs")
$before=(Get-Content -Raw -LiteralPath "$PSScriptRoot/stages-before.json" | ConvertFrom-Json).stages
$after=(Get-Content -Raw -LiteralPath "$stageRoot/GameData/Generated/stages.json" | ConvertFrom-Json).stages
$results=foreach($index in @(10,12)){
    foreach($starter in @(1,-1)){
        $a=$before[$index];$b=$after[$index]
        $old=[ShortGameAuditBefore]::Run($starter,$a.aiDepth,$a.aiNodeBudget,$a.aiRandomPermille,$a.aiOversightPermille,32)
        $current=[ShortGameAudit]::Run($starter,$b.aiDepth,$b.aiNodeBudget,$b.aiRandomPermille,$b.aiOversightPermille,32)
        Write-Host "Rating $($b.aiRating), starter $starter : early losses $($old.earlyPlayerWins) -> $($current.earlyPlayerWins)"
        [pscustomobject]@{rating=$b.aiRating;starter=$starter;before=$old;after=$current}
    }
}
[ordered]@{method='Exact archived previous AI and current production AI. Empty boards, both starters, all legal player sequences through their fourth placement, 32 match seeds per rating/starter. Counts are traversal branches, not human games or unique board states.';results=$results} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath "$PSScriptRoot/short-game-comparison.json" -Encoding utf8
if(($results | Where-Object {$_.after.earlyPlayerWins -gt 0}).Count){throw 'A checked seven/eight-ply player win remains.'}
