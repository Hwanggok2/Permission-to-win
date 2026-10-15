$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/ShortGameAudit.cs")
$report=Get-Content -Raw -LiteralPath "$PSScriptRoot/short-games.json" | ConvertFrom-Json
$profiles=Get-Content -Raw -LiteralPath "$PSScriptRoot/final-profiles.json" | ConvertFrom-Json
$oldProfiles=Get-Content -Raw -LiteralPath "$PSScriptRoot/candidate-b-profiles.json" | ConvertFrom-Json
foreach($row in $report.results){
    $p=$profiles | Where-Object {$_.aiRating -eq $row.rating}
    $old=$oldProfiles | Where-Object {$_.aiRating -eq $row.rating}
    if(($p | ConvertTo-Json -Compress) -ne ($old | ConvertTo-Json -Compress)){
        $row.result=[ShortGameAudit]::Run($row.result.starter,$p.aiDepth,$p.aiNodeBudget,$p.aiRandomPermille,$p.aiOversightPermille,2)
        Write-Host ("Refreshed rating "+$row.rating+", starter="+$row.result.starter+': '+$row.result.earlyPlayerWins+' early losses')
    }
    $row | Add-Member -NotePropertyName profile -NotePropertyValue $p
}
$report.method+=' Final profiles: reuse unchanged rows, rerun the changed Rating 700 profile; actual settings are recorded on every row.'
$report | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath "$PSScriptRoot/final-short-games.json" -Encoding utf8
if($report.results | Where-Object {$_.result.earlyPlayerWins -gt 0}){throw 'An authored mid/high rating lost within seven/eight plies'}
