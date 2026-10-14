param([Parameter(Mandatory=$true)][string]$RunId)
$ErrorActionPreference = 'Stop'
if ($RunId -notmatch '^[A-Za-z0-9_-]+$') { throw 'Safe new run ID required.' }
$qaRoot = Split-Path $PSScriptRoot -Parent
$qaOut = Join-Path $qaRoot ('Docs/QA/Evidence/' + $RunId)
if (Test-Path -LiteralPath $qaOut) { throw 'Existing evidence is immutable; choose a new run ID.' }
New-Item -ItemType Directory -Path $qaOut | Out-Null
$qaManifest = @()
function Copy-Evidence([string]$Relative) {
    $qaTarget = Join-Path $qaOut $Relative
    New-Item -ItemType Directory -Path (Split-Path $qaTarget -Parent) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $qaRoot $Relative) -Destination $qaTarget
    $script:qaManifest += [pscustomobject]@{source=$Relative;sha256=(Get-FileHash -LiteralPath $qaTarget -Algorithm SHA256).Hash}
    return $qaTarget
}
$qaReports = foreach ($qaPath in @('Client/Logs/host-ui-tests.xml','Client/Logs/host-ui-boundary-tests.xml')) {
    [xml]$qaXml = Get-Content -Raw -LiteralPath (Copy-Evidence $qaPath)
    $qaRun = $qaXml.'test-run'
    [pscustomobject]@{source=$qaPath;result=$qaRun.result;total=[int]$qaRun.total;passed=[int]$qaRun.passed;failed=[int]$qaRun.failed;startUtc=$qaRun.'start-time';endUtc=$qaRun.'end-time';cases=@($qaRun.SelectNodes('.//test-case') | ForEach-Object { [pscustomobject]@{name=$_.fullname;result=$_.result} })}
}
foreach ($qaPath in @('Client/HOST-UI-REQUEST.json','Client/Assets/StageGame/Scripts/UI/HostLayout.cs','Client/Assets/StageGame/Scripts/UI/SafeAreaFitter.cs','Client/Assets/WebGLTemplates/StageGamePortrait/index.html','Client/Assets/WebGLTemplates/StageGamePortrait/host-layout.js','Client/Assets/StageGame/Plugins/WebGL/HostInsets.jslib','Client/Assets/StageGame/Editor/SoftUiBuilder.cs','Client/Assets/StageGame/Tests/EditMode/HostLayoutTests.cs','Client/Assets/StageGame/Tests/EditMode/PlayFlowTests.cs','Client/Assets/StageGame/Tests/EditMode/LocalizationTests.cs','Tests/check-host-evidence.ps1')) {
    Copy-Evidence $qaPath | Out-Null
}
$qaFiles = @((Get-ChildItem -LiteralPath (Join-Path $qaRoot 'Client/Assets/StageGame/Prefabs') -Recurse -File | Where-Object { $_.Extension -in @('.prefab','.meta') }).FullName)
$qaFiles += @((Get-ChildItem -LiteralPath (Join-Path $qaRoot 'Client/Assets/StageGame/Art/SoftUI') -File).FullName)
$qaImages = @(Get-ChildItem -LiteralPath (Join-Path $qaRoot 'Client/Evidence') -File | Where-Object { $_.Name -match '^host-(ko|en)-(320|390)-.*\.png$' })
$qaFiles += @($qaImages.FullName)
foreach ($qaPath in $qaFiles) { Copy-Evidence ([IO.Path]::GetRelativePath($qaRoot,$qaPath)) | Out-Null }
$qaIndependent = Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-host-fixed-v1-20261014/source-manifest.json') | ConvertFrom-Json
$qaHostMatch = @($qaIndependent | Where-Object { $_.source -match '^Client/Assets/' } | ForEach-Object {
    $qaCurrent = (Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash
    [pscustomobject]@{source=$_.source;independentSha256=$_.sha256;currentSha256=$qaCurrent;matches=($qaCurrent -eq $_.sha256)}
})
$qaBaseline = Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-l10n-core-v1-20261014/source-manifest.json') | ConvertFrom-Json
$qaCoreData = @($qaBaseline | Where-Object { $_.source -match '^Client/Assets/StageGame/Scripts/|[/\\]Resources[/\\]GameData[/\\]|[/\\]GameData[/\\]Generated[/\\]' } | ForEach-Object {
    $qaPath = if([IO.Path]::IsPathRooted($_.source)) { $_.source } else { Join-Path $qaRoot $_.source }
    $qaCurrent = (Get-FileHash -LiteralPath $qaPath -Algorithm SHA256).Hash
    [pscustomobject]@{source=$_.source;baselineSha256=$_.sha256;currentSha256=$qaCurrent;matches=($qaCurrent -eq $_.sha256)}
})
$qaPostCopy = @($qaManifest | ForEach-Object {
    [pscustomobject]@{source=$_.source;changedDuringRead=((Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256)}
})
$qaReport = [pscustomobject]@{
    dateSeoul='2026-10-14';method='Read-only Client evidence review; no QA Unity/browser/build execution'
    unity=$qaReports;hostSourcesVsIndependent=$qaHostMatch;coreAndDataVsPrevious=$qaCoreData
    captures=$qaImages.Name;captureCount=$qaImages.Count;postCopy=$qaPostCopy
    limits='The full 49-test report precedes QA-009 normalization; the final targeted 8-test report includes the new normalization regression and host UI profiles. Do not sum them into unique tests. Captures are Editor render targets, not browser/device. Build hashes and root browser evidence are pending.'
}
$qaReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $qaOut 'results.json') -Encoding utf8
$qaManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
[pscustomobject]@{unity=@($qaReports | Select-Object source,total,passed,failed);hostSourcesMatch=(@($qaHostMatch | Where-Object { -not $_.matches }).Count -eq 0);changedCoreOrData=@($qaCoreData | Where-Object { -not $_.matches });captureCount=$qaImages.Count;changedDuringRead=@($qaPostCopy | Where-Object changedDuringRead);evidence=$qaOut} | ConvertTo-Json -Depth 5
