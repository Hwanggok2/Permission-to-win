param([Parameter(Mandatory=$true)][string]$RunId)
$ErrorActionPreference='Stop'
if($RunId -notmatch '^[A-Za-z0-9_-]+$'){throw 'Safe new run ID required.'}
$qaRoot=Split-Path $PSScriptRoot -Parent
$qaOut=Join-Path $qaRoot ('Docs/QA/Evidence/'+$RunId)
if(Test-Path -LiteralPath $qaOut){throw 'Use a new run ID to preserve prior evidence.'}
New-Item -ItemType Directory -Path $qaOut | Out-Null
$qaManifest=@()
foreach($qaPath in @('Client/Assets/StageGame/Prefabs/AppRoot.prefab','Client/Assets/StageGame/Prefabs/UI/Screens/LobbyScreen.prefab','Client/Assets/StageGame/Prefabs/UI/Screens/HelpModal.prefab','Client/Assets/StageGame/Prefabs/UI/Screens/ResultModal.prefab','Client/Assets/StageGame/Prefabs/UI/Common/StageNode.prefab','Client/Assets/StageGame/Prefabs/UI/Common/StageNode.prefab.meta','Client/Assets/StageGame/Editor/StageMapBuilder.cs','Client/Assets/StageGame/Scripts/UI/GameApp.cs','Client/Assets/StageGame/Tests/EditMode/StageMapTests.cs','Client/Assets/StageGame/Tests/EditMode/PlayFlowTests.cs','Client/Assets/StageGame/Resources/GameData/strings.json','Client/STAGE-MAP-REQUEST.json','Tests/check-stage-prefab.ps1')) {
    $qaCopy=Join-Path $qaOut $qaPath
    New-Item -ItemType Directory -Path (Split-Path $qaCopy -Parent) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $qaRoot $qaPath) -Destination $qaCopy
    $qaManifest += [pscustomobject]@{source=$qaPath;sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
}
$qaApp=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'Client/Assets/StageGame/Prefabs/AppRoot.prefab')
$qaLobby=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'Client/Assets/StageGame/Prefabs/UI/Screens/LobbyScreen.prefab')
$qaMeta=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'Client/Assets/StageGame/Prefabs/UI/Common/StageNode.prefab.meta')
$qaRuntime=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'Client/Assets/StageGame/Scripts/UI/GameApp.cs')
$qaStrings=(Get-Content -Raw -LiteralPath (Join-Path $qaOut 'Client/Assets/StageGame/Resources/GameData/strings.json') | ConvertFrom-Json).strings
$qaDeclared=@{}; foreach($qaId in [regex]::Matches($qaApp,'(?m)^--- !u!\d+ &(-?\d+)')){$qaDeclared[$qaId.Groups[1].Value]=$true}
$qaArrays=@(foreach($qaField in @('stageNodes','stageNodeNumbers','stageNodeStates')) {
    $qaMatch=[regex]::Match($qaApp,'(?m)^  '+$qaField+':\r?\n((?:  - \{fileID: -?\d+\}\r?\n)+)')
    $qaIds=@([regex]::Matches($qaMatch.Groups[1].Value,'fileID: (-?\d+)') | ForEach-Object {$_.Groups[1].Value})
    [pscustomobject]@{field=$qaField;count=$qaIds.Count;uniqueCount=@($qaIds | Select-Object -Unique).Count;allDeclared=(@($qaIds | Where-Object { -not $qaDeclared.ContainsKey($_) }).Count -eq 0);ids=$qaIds}
})
$qaScalars=@(foreach($qaField in @('chapterLabel','chapterTier','retryLabel','nextStageLabel','resultLobbyLabel','resultPrimarySlot','resultSecondarySlot')) {
    $qaMatch=[regex]::Match($qaApp,'(?m)^  '+$qaField+': \{fileID: (-?\d+)\}')
    [pscustomobject]@{field=$qaField;declared=($qaMatch.Success -and $qaDeclared.ContainsKey($qaMatch.Groups[1].Value));id=$qaMatch.Groups[1].Value}
})
$qaBindings=@(foreach($qaKey in @('help_drop_title','help_drop_caption','help_connect_title','help_connect_caption')) {
    $qaMatch=[regex]::Match($qaApp,'(?m)^  - target: \{fileID: (-?\d+)\}\r?\n    key: '+$qaKey+'\r?$')
    [pscustomobject]@{key=$qaKey;targetDeclared=($qaMatch.Success -and $qaDeclared.ContainsKey($qaMatch.Groups[1].Value));dataKeyExists=($qaKey -in $qaStrings.key)}
})
$qaGuid=[regex]::Match($qaMeta,'(?m)^guid: ([a-f0-9]+)').Groups[1].Value
$qaNodeInstances=[regex]::Matches($qaLobby,'m_SourcePrefab: \{fileID: 100100000, guid: '+$qaGuid+', type: 3\}').Count
$qaReport=[pscustomobject]@{method='Static saved YAML/reference read; no Unity, rendering, pointer or GUID-resolution execution';arrays=$qaArrays;scalars=$qaScalars;helpBindings=$qaBindings;nodePrefabGuid=$qaGuid;nodeInstancesInLobby=$qaNodeInstances;runtimeUiConstructionMatches=@([regex]::Matches($qaRuntime,'new\s+GameObject\b|AddComponent\s*[<(]|\bInstantiate\s*\(') | ForEach-Object Value);limits='Local fileID declarations and source prefab GUID counts are necessary but do not prove native reference types/resolution, 5x2 geometry, hierarchy activation, raycast targets or44px. Supplied Client native tests/captures and root browser evidence are separate.'}
$qaReport | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $qaOut 'results.json') -Encoding utf8
$qaManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
$qaChanged=@($qaManifest | Where-Object { (Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256 } | ForEach-Object source)
ConvertTo-Json -InputObject $qaChanged | Set-Content -LiteralPath (Join-Path $qaOut 'changed-during-read.json') -Encoding utf8
[pscustomobject]@{arrays=@($qaArrays | Select-Object field,count,uniqueCount,allDeclared);invalidScalars=@($qaScalars | Where-Object { -not $_.declared });invalidHelp=@($qaBindings | Where-Object { -not $_.targetDeclared -or -not $_.dataKeyExists });nodeInstances=$qaNodeInstances;runtimeUiConstructionMatches=$qaReport.runtimeUiConstructionMatches;changedDuringRead=$qaChanged;evidence=$qaOut} | ConvertTo-Json -Depth 5
