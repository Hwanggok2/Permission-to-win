param([Parameter(Mandatory=$true)][string]$RunId)
$ErrorActionPreference='Stop'
if($RunId -notmatch '^[A-Za-z0-9_-]+$'){throw 'Safe new run ID required.'}
$qaRoot=Split-Path $PSScriptRoot -Parent
$qaOut=Join-Path $qaRoot ('Docs/QA/Evidence/'+$RunId)
if(Test-Path -LiteralPath $qaOut){throw 'Preserve prior evidence; use a new run ID.'}
New-Item -ItemType Directory -Path $qaOut | Out-Null
$qaFiles=@(foreach($qaDir in @('Scripts','Prefabs','Art','Editor','Scenes','Tests/EditMode','Resources/GameData')){
    Get-ChildItem -LiteralPath (Join-Path $qaRoot ('Client/Assets/StageGame/'+$qaDir)) -File -Recurse
})
$qaFiles+=@(Get-ChildItem -LiteralPath (Join-Path $qaRoot 'GameData/Generated') -File -Filter '*.json')
$qaFiles+=@(Get-ChildItem -LiteralPath (Join-Path $qaRoot 'Client/Evidence/graphics-refresh-20261014') -File)
$qaFiles+=@(foreach($qaPath in @('Client/Evidence/graphics-refresh-tests.xml','Client/Evidence/graphics-refresh-idempotence.json','Tests/check-graphics-assets.ps1','Docs/QA/GRAPHICS-RESULTS.md','Docs/Management/CONTRACT.md')){Get-Item -LiteralPath (Join-Path $qaRoot $qaPath)})
$qaManifest=@($qaFiles | Sort-Object FullName -Unique | ForEach-Object{
    $qaRelative=[IO.Path]::GetRelativePath($qaRoot,$_.FullName).Replace('\','/')
    $qaTarget=Join-Path $qaOut $qaRelative
    New-Item -ItemType Directory -Path (Split-Path $qaTarget -Parent) -Force | Out-Null
    Copy-Item -LiteralPath $_.FullName -Destination $qaTarget
    [pscustomobject]@{source=$qaRelative;sha256=(Get-FileHash -LiteralPath $qaTarget -Algorithm SHA256).Hash;bytes=(Get-Item -LiteralPath $qaTarget).Length}
})
$qaMeta=@{}
Get-ChildItem -LiteralPath (Join-Path $qaOut 'Client/Assets/StageGame') -Filter '*.meta' -File -Recurse | ForEach-Object{
    $qaMatch=[regex]::Match((Get-Content -Raw -LiteralPath $_.FullName),'(?m)^guid: ([0-9a-f]+)$')
    if($qaMatch.Success){$qaMeta[$qaMatch.Groups[1].Value]=$_.FullName}
}
$qaThemePath=Join-Path $qaOut 'Client/Assets/StageGame/Art/TabletopUI/TabletopTheme.asset'
$qaThemeRefs=@([regex]::Matches((Get-Content -Raw -LiteralPath $qaThemePath),'(?m)^  (\w+): \{fileID: 21300000, guid: ([0-9a-f]+), type: 3\}') | ForEach-Object{
    $qaMetaPath=$qaMeta[$_.Groups[2].Value]
    [pscustomobject]@{field=$_.Groups[1].Value;guid=$_.Groups[2].Value;resolved=($null -ne $qaMetaPath -and (Test-Path -LiteralPath ($qaMetaPath -replace '\.meta$','')))}
})
$qaPrefabSprites=@(Get-ChildItem -LiteralPath (Join-Path $qaOut 'Client/Assets/StageGame/Prefabs') -Filter '*.prefab' -File -Recurse | ForEach-Object{
    $qaPrefab=$_.Name
    [regex]::Matches((Get-Content -Raw -LiteralPath $_.FullName),'m_Sprite: \{fileID: (\d+), guid: ([0-9a-f]+), type: 3\}') | ForEach-Object{
        [pscustomobject]@{prefab=$qaPrefab;fileID=$_.Groups[1].Value;guid=$_.Groups[2].Value;resolved=$qaMeta.ContainsKey($_.Groups[2].Value)}
    }
})
$qaApp=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'Client/Assets/StageGame/Prefabs/AppRoot.prefab')
$qaThemeGuid=([regex]::Match((Get-Content -Raw -LiteralPath ($qaThemePath+'.meta')),'(?m)^guid: ([0-9a-f]+)$')).Groups[1].Value
$qaIcons=[regex]::Match($qaApp,'(?ms)^  stageNodeIcons:\s*\r?\n(?<refs>(?:  - \{fileID: \d+\}\r?\n)+)')
$qaIconIds=@([regex]::Matches($qaIcons.Groups['refs'].Value,'fileID: (\d+)') | ForEach-Object {$_.Groups[1].Value})
$qaDeclaredIds=@([regex]::Matches($qaApp,'(?m)^--- !u!\d+ &(\d+)') | ForEach-Object {$_.Groups[1].Value})
$qaBaseline=Join-Path $qaRoot 'Docs/QA/Evidence/qa-graphics-baseline-20261014'
$qaOld=Get-Content -Raw -LiteralPath (Join-Path $qaBaseline 'baseline-manifest.json') | ConvertFrom-Json
$qaComparisons=@($qaOld | Sort-Object path -Unique | Where-Object { $_.path -match '^Client/Assets/StageGame/|^GameData/Generated/' } | ForEach-Object{
    $qaCurrent=$qaManifest | Where-Object source -eq $_.path
    [pscustomobject]@{path=$_.path;unchanged=($null -ne $qaCurrent -and $qaCurrent.sha256 -eq $_.sha256)}
})
[xml]$qaXml=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'Client/Evidence/graphics-refresh-tests.xml')
$qaIdempotence=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'Client/Evidence/graphics-refresh-idempotence.json') | ConvertFrom-Json
$qaReapply=@($qaIdempotence | ForEach-Object {
    $qaCurrent=$qaManifest | Where-Object source -eq ('Client/'+$_.path)
    [pscustomobject]@{path=$_.path;reportedUnchanged=($_.before -eq $_.after -and $_.identical);matchesCurrent=($null -ne $qaCurrent -and $_.after -eq $qaCurrent.sha256)}
})
$qaChanged=@($qaManifest | Where-Object { (Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256 } | ForEach-Object source)
$qaReport=[pscustomobject]@{
    method='Snapshot and static serialized-reference read only; no QA Unity/browser/render execution'
    files=$qaManifest.Count;themeReferences=$qaThemeRefs;prefabSpriteReferences=$qaPrefabSprites
    appThemeBound=($qaApp -match ('theme: \{fileID: 11400000, guid: '+$qaThemeGuid+', type: 2\}'))
    stageNodeIconCount=$qaIconIds.Count;missingIconLocalIds=@($qaIconIds | Where-Object {$qaDeclaredIds -notcontains $_})
    priorComparisons=$qaComparisons;reapply=$qaReapply
    unity=[pscustomobject]@{total=[int]$qaXml.'test-run'.total;passed=[int]$qaXml.'test-run'.passed;failed=[int]$qaXml.'test-run'.failed;startUtc=$qaXml.'test-run'.'start-time';endUtc=$qaXml.'test-run'.'end-time';cases=@($qaXml.SelectNodes('//test-case') | ForEach-Object{[pscustomobject]@{name=$_.fullname;result=$_.result}})}
    changedDuringRead=$qaChanged
    limits='GUID/fileID existence does not prove Unity reference type resolution, sprite pixels,9-slice shape,glyph visibility,hitbox or visual acceptance. Reapply is Client-provided evidence; QA did not run builder. Builds and root browser follow separately.'
}
$qaManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
$qaReport | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $qaOut 'results.json') -Encoding utf8
[pscustomobject]@{files=$qaManifest.Count;themeRefs=$qaThemeRefs.Count;unresolvedTheme=@($qaThemeRefs | Where-Object { -not $_.resolved });prefabSpriteRefs=$qaPrefabSprites.Count;unresolvedSprites=@($qaPrefabSprites | Where-Object { -not $_.resolved });appThemeBound=$qaReport.appThemeBound;nodeIcons=$qaIconIds.Count;missingIconIds=$qaReport.missingIconLocalIds;changedBaseline=@($qaComparisons | Where-Object { -not $_.unchanged } | ForEach-Object path);reapplyMismatch=@($qaReapply | Where-Object { -not $_.reportedUnchanged -or -not $_.matchesCurrent });unityTotal=$qaReport.unity.total;unityPassed=$qaReport.unity.passed;changedDuringRead=$qaChanged;evidence=$qaOut} | ConvertTo-Json -Depth 4
