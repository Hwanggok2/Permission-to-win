param([ValidateSet('short','matches')][string]$Mode='short',[string]$Label='baseline')
$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
$auditTool=if($Mode -eq 'short'){'ShortGameAudit'}else{'RatingMatchAudit'}
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/$auditTool.cs")
$profiles=Get-Content -Raw -LiteralPath "$PSScriptRoot/$Label-profiles.json" | ConvertFrom-Json
$results=foreach($p in $profiles){
    if($Mode -eq 'short'){
        if($p.aiRating -notin @(400,500,600)){continue}
        foreach($starter in @(1,-1)){
            $result=[ShortGameAudit]::Run($starter,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,16)
            Write-Host ("Rating "+$p.aiRating+", starter="+$starter+": "+$result.earlyPlayerWins+' early losses; '+$result.firstLoss)
            [pscustomobject]@{profile=$p;result=$result}
        }
    }else{
        if($p.aiRating -gt 800){continue}
        $result=[RatingMatchAudit]::Run($p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,8)
        Write-Host ("Rating "+$p.aiRating+": "+$result.wins+'W '+$result.draws+'D '+$result.losses+'L; mean plies='+[math]::Round($result.meanPlies,1))
        [pscustomobject]@{profile=$p;result=$result}
    }
}
[ordered]@{mode=$Mode;label=$Label;results=$results} | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath "$PSScriptRoot/$Label-$Mode.json" -Encoding utf8
