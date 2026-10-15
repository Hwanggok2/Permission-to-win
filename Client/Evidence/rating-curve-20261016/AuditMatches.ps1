param([string]$Label='candidate-b',[string]$Profiles="$PSScriptRoot/candidate-b-profiles.json")
$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/RatingMatchAudit.cs")
$profileRows=Get-Content -Raw -LiteralPath $Profiles | ConvertFrom-Json
if($profileRows.PSObject.Properties.Name -contains 'stages'){$profileRows=$profileRows.stages | Where-Object {($_.id%2) -eq 1}}
if($profileRows.Count -ne 15){throw 'Expected all 15 rating profiles'}
$results=foreach($p in $profileRows){
    $result=[RatingMatchAudit]::Run($p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,8)
    Write-Host ("Rating "+$p.aiRating+": "+$result.wins+'W '+$result.draws+'D '+$result.losses+'L; score='+[math]::Round($result.scorePercent,1)+'%')
    [pscustomobject]@{profile=$p;result=$result}
}
[ordered]@{method='128 games per rating: 8 fixed openings x 8 paired seeds x 2 colors. Fixed opponent: depth 4, 10000 nodes, 25% safe random, no oversight. Score counts a draw as half a win. Same openings/colors/seeds across ratings. Bot proxy only, not human win rate or Elo.';results=$results} | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath "$PSScriptRoot/$Label-matches.json" -Encoding utf8
