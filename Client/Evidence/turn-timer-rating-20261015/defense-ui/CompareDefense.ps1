$ErrorActionPreference='Stop'
$stageRoot='C:/Github/StageGame'
Add-Type -Path @("$stageRoot/Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs","$stageRoot/Client/Assets/StageGame/Scripts/Core/AiSearch.cs","$stageRoot/Client/Tools/DifficultyBenchmark.cs")
$before=(Get-Content -Raw -LiteralPath "$PSScriptRoot/stages-before.json" | ConvertFrom-Json).stages
$after=(Get-Content -Raw -LiteralPath "$stageRoot/GameData/Generated/stages.json" | ConvertFrom-Json).stages
function Get-Blocks($profile,[bool]$vertical) {
    $board=[StageGame.ConnectFourBoard]::new()
    $moves=if($vertical){@(2,3,2,4,2)}else{@(0,6,1,6,2)}
    foreach($move in $moves){if(-not $board.TryDrop($move)){throw 'Invalid fixture'}}
    $target=if($vertical){2}else{3};$blocks=0
    foreach($seed in 0..255){
        $search=[StageGame.AiSearch]::new($board,$profile.aiDepth,$profile.aiNodeBudget,$profile.aiRandomPermille,$seed,$false,$profile.aiOversightPermille)
        try { while(-not $search.Complete){$search.Step(256)};if($search.ChosenColumn -eq $target){$blocks++} }
        finally {$search.Dispose()}
    }
    return $blocks
}
$results=foreach($i in 0..29){
    $old=$before[$i];$new=$after[$i]
    $oldResult=[DifficultyBenchmark]::Run($old.starter,$old.openingMoves,$old.aiDepth,$old.aiNodeBudget,$old.aiRandomPermille,128,$false,$old.aiOversightPermille)
    $newResult=[DifficultyBenchmark]::Run($new.starter,$new.openingMoves,$new.aiDepth,$new.aiNodeBudget,$new.aiRandomPermille,128,$false,$new.aiOversightPermille)
    [pscustomobject]@{stage=$new.id;rating=$new.aiRating;before=$oldResult;after=$newResult}
}
$defense=foreach($i in (0..14 | ForEach-Object {$_*2})){
    [pscustomobject]@{rating=$after[$i].aiRating;gamesPerFixture=256;beforeHorizontal=(Get-Blocks $before[$i] $false);afterHorizontal=(Get-Blocks $after[$i] $false);beforeVertical=(Get-Blocks $before[$i] $true);afterVertical=(Get-Blocks $after[$i] $true)}
}
[ordered]@{
    method='Production AiSearch, same seeds 0..127, all 30 stages before/after. Opponent is the existing immediate-win/block reactive bot. Both use the authored profile throughout, with no opening protection.'
    limitation='Bot outcomes are not human win rates or measured Elo.'
    fullGames=7680;stages=$results;immediateThreatDefense=$defense
} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath "$PSScriptRoot/defense-comparison.json" -Encoding utf8
$results | Where-Object {$_.stage -in 1,2,11,12,29,30} | ForEach-Object {[pscustomobject]@{stage=$_.stage;rating=$_.rating;beforePlayerWins=$_.before.playerWins;afterPlayerWins=$_.after.playerWins;games=128}} | Format-Table
$defense | Format-Table
