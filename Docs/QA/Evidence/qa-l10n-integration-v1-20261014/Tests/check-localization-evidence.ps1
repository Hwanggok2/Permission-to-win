param([Parameter(Mandatory=$true)][string]$RunId)
$ErrorActionPreference = 'Stop'
if ($RunId -notmatch '^[A-Za-z0-9_-]+$') { throw 'Safe new run ID required.' }
$qaRoot = Split-Path $PSScriptRoot -Parent
$qaOut = Join-Path $qaRoot ('Docs/QA/Evidence/' + $RunId)
if (Test-Path -LiteralPath $qaOut) { throw 'Evidence already exists.' }
New-Item -ItemType Directory -Path $qaOut | Out-Null
$qaManifest = @()
function Copy-Evidence([string]$Relative) {
    $qaSource = Join-Path $qaRoot $Relative
    $qaTarget = Join-Path $qaOut $Relative
    New-Item -ItemType Directory -Path (Split-Path $qaTarget -Parent) -Force | Out-Null
    Copy-Item -LiteralPath $qaSource -Destination $qaTarget
    $script:qaManifest += [pscustomobject]@{source=$Relative; sha256=(Get-FileHash -LiteralPath $qaTarget -Algorithm SHA256).Hash}
    return $qaTarget
}
$qaVerification = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/verification.json') | ConvertFrom-Json
[xml]$qaXml = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/unity-tests.xml')
$qaPrefab = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Assets/StageGame/Prefabs/AppRoot.prefab')
$qaStrings = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Assets/StageGame/Resources/GameData/strings.json') | ConvertFrom-Json
foreach ($qaPath in @('Client/Evidence/android-build-summary.txt','Client/Evidence/webgl-build-summary.txt','Client/Evidence/ui-ux-audit.json','Client/Assets/StageGame/Editor/LocalizationBuilder.cs','Client/Assets/StageGame/Editor/ProjectBuilder.cs','Client/Assets/StageGame/Tests/EditMode/PlayFlowTests.cs','Client/Assets/StageGame/Tests/EditMode/LocalizationTests.cs','Tests/check-localization-evidence.ps1')) { Copy-Evidence $qaPath | Out-Null }
foreach ($qaImage in @('ko-lobby.png','ko-settings.png','ko-match.png','en-lobby.png','en-settings.png','en-match.png','en-help.png','en-confirm.png','en-result.png','en-restored.png')) { Copy-Evidence ('Client/Evidence/' + $qaImage) | Out-Null }
foreach ($qaImage in @('en-settings-320.jpg','ko-settings-320-reloaded.jpg','ko-settings-390.jpg','en-settings-390.jpg','en-match-390.jpg','en-result-390.jpg','en-restored-390.jpg')) { Copy-Evidence ('Docs/Management/Evidence/localization-browser-20261014/' + $qaImage) | Out-Null }
$qaArtifacts = foreach ($qaArtifact in $qaVerification.artifacts) {
    $qaPath = Join-Path (Join-Path $qaRoot 'Client') $qaArtifact.path
    $qaHash = (Get-FileHash -LiteralPath $qaPath -Algorithm SHA256).Hash
    $qaBytes = (Get-Item -LiteralPath $qaPath).Length
    [pscustomobject]@{path=$qaArtifact.path;declaredSha256=$qaArtifact.sha256;actualSha256=$qaHash;bytes=$qaBytes;matches=($qaHash -eq $qaArtifact.sha256 -and $qaBytes -eq $qaArtifact.bytes)}
}
$qaSources = foreach ($qaSource in $qaVerification.runtimeSources) {
    $qaCopy = Copy-Evidence ('Client/' + $qaSource.path)
    $qaHash = (Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash
    [pscustomobject]@{path=$qaSource.path;declaredSha256=$qaSource.sha256;actualSha256=$qaHash;matches=($qaHash -eq $qaSource.sha256)}
}
$qaCore = Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-l10n-core-v1-20261014/source-manifest.json') | ConvertFrom-Json
$qaCoreChanges = @($qaCore | Where-Object { $_.source -match '^Client/Assets/StageGame/Scripts/' } | ForEach-Object {
    if ((Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256) { $_.source }
})
$qaCases = @(Select-Xml -Xml $qaXml -XPath '//test-case' | ForEach-Object { [pscustomobject]@{name=$_.Node.name;result=$_.Node.result;duration=$_.Node.duration} })
$qaKeys = @{}; foreach ($qaEntry in $qaStrings.strings) { $qaKeys[$qaEntry.key] = $true }
$qaIds = @{}; foreach ($qaMatch in [regex]::Matches($qaPrefab,'(?m)^--- !u!\d+ &(-?\d+)')) { $qaIds[$qaMatch.Groups[1].Value] = $true }
$qaBindings = @([regex]::Matches($qaPrefab,'(?m)^  - target: \{fileID: (-?\d+)\}\r?\n    key: ([A-Za-z0-9_]+)') | ForEach-Object {
    [pscustomobject]@{target=$_.Groups[1].Value;key=$_.Groups[2].Value;targetDeclared=$qaIds.ContainsKey($_.Groups[1].Value);keyExists=$qaKeys.ContainsKey($_.Groups[2].Value)}
})
$qaButtons = foreach ($qaField in @('korean','english','koreanLabel','englishLabel','playLabel','hintLabel')) {
    $qaMatch = [regex]::Match($qaPrefab, '(?m)^  '+$qaField+': \{fileID: (-?\d+)\}')
    [pscustomobject]@{field=$qaField;fileId=$qaMatch.Groups[1].Value;declared=($qaMatch.Success -and $qaIds.ContainsKey($qaMatch.Groups[1].Value))}
}
$qaColumns = [regex]::Match($qaPrefab, '(?m)^  columnLabels:\r?\n((?:  - \{fileID: -?\d+\}\r?\n)+)').Groups[1].Value
$qaColumnCount = [regex]::Matches($qaColumns,'fileID:').Count
$qaReport = [pscustomobject]@{
    dateSeoul='2026-10-14'; method='Read-only evidence review; QA did not run Unity/browser/builds'
    unity=[pscustomobject]@{total=[int]$qaXml.'test-run'.total;passed=[int]$qaXml.'test-run'.passed;failed=[int]$qaXml.'test-run'.failed;endUtc=$qaXml.'test-run'.'end-time';cases=$qaCases;matchesDeclared=([int]$qaXml.'test-run'.total -eq $qaVerification.tests.total -and [int]$qaXml.'test-run'.passed -eq $qaVerification.tests.passed)}
    artifacts=$qaArtifacts;runtimeSources=$qaSources;independentCoreChangedSources=$qaCoreChanges
    prefab=[pscustomobject]@{localizedBindingCount=$qaBindings.Count;columnLabelCount=$qaColumnCount;bindings=$qaBindings;newFields=$qaButtons;staticReferencesValid=($qaBindings.Count -gt 0 -and @($qaBindings | Where-Object { -not $_.targetDeclared -or -not $_.keyExists }).Count -eq 0 -and @($qaButtons | Where-Object { -not $_.declared }).Count -eq 0 -and $qaColumnCount -eq 7)}
    resizeDefect='QA-008: reported 390-to-320 resize leaves content smaller than direct/reloaded 320; repair retest pending'
    limitations='XML is Client execution evidence. Static YAML checks do not prove Unity GUID resolution, click targeting, layout timing, device safety, or accessibility. Pixel inspection is documented separately.'
}
$qaReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $qaOut 'results.json') -Encoding utf8
$qaManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
[pscustomobject]@{unityPassed=$qaReport.unity.passed;unityTotal=$qaReport.unity.total;artifactsMatch=(@($qaArtifacts | Where-Object { -not $_.matches }).Count -eq 0);runtimeChanges=@($qaSources | Where-Object { -not $_.matches });independentCoreChanges=$qaCoreChanges;bindings=$qaBindings.Count;prefabStaticReferencesValid=$qaReport.prefab.staticReferencesValid;evidence=$qaOut} | ConvertTo-Json -Depth 4
