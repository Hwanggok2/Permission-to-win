param([Parameter(Mandatory=$true)][string]$RunId, [switch]$IncludeBuilds)
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
$qaFinal = $null
if ($IncludeBuilds) {
    $qaVerification = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/verification.json') | ConvertFrom-Json
    $qaHost = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/host-ui-verification.json') | ConvertFrom-Json
    $qaArt = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/soft-ui-assets.json') | ConvertFrom-Json
    $qaNotices = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/apk-notices.json') | ConvertFrom-Json
    foreach ($qaPath in @('Client/Evidence/android-build-summary.txt','Client/Evidence/webgl-build-summary.txt','Client/Evidence/unity-tests.xml','Client/Evidence/host-ui-boundary-tests.xml')) { Copy-Evidence $qaPath | Out-Null }
    [xml]$qaUi = Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/host-ui-final-render.xml')
    $qaArtifacts = @($qaVerification.artifacts | ForEach-Object {
        $qaPath = Join-Path (Join-Path $qaRoot 'Client') $_.path
        $qaHash = (Get-FileHash -LiteralPath $qaPath -Algorithm SHA256).Hash
        $qaBytes = (Get-Item -LiteralPath $qaPath).Length
        [pscustomobject]@{path=$_.path;sha256=$qaHash;bytes=$qaBytes;matches=($qaHash -eq $_.sha256 -and $qaBytes -eq $_.bytes)}
    })
    $qaDeclaredSources = @($qaVerification.runtimeSources) + @($qaHost.sourceFiles) + @($qaArt.files) + @($qaHost.captures)
    $qaDeclaredMatches = @($qaDeclaredSources | ForEach-Object {
        $qaCopy = Copy-Evidence ('Client/' + $_.path)
        $qaHash = (Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash
        [pscustomobject]@{path=$_.path;matches=($qaHash -eq $_.sha256 -and ($null -eq $_.bytes -or (Get-Item -LiteralPath $qaCopy).Length -eq $_.bytes))}
    })
    $qaPrevious = Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-host-editor-v1-20261014/source-manifest.json') | ConvertFrom-Json
    $qaChangedAssets = @($qaPrevious | Where-Object { $_.source -match 'Assets' } | ForEach-Object {
        if ((Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256) { $_.source }
    })
    $qaOldBuilder = Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-host-editor-v1-20261014/Client/Assets/StageGame/Editor/SoftUiBuilder.cs')
    $qaNewBuilder = Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Client/Assets/StageGame/Editor/SoftUiBuilder.cs')
    $qaOldModal = Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-host-editor-v1-20261014/Client/Assets/StageGame/Prefabs/UI/Screens/NoticesModal.prefab')
    $qaNewModal = Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Client/Assets/StageGame/Prefabs/UI/Screens/NoticesModal.prefab')
    $qaFontOnly = $qaChangedAssets.Count -eq 2 -and $qaOldBuilder.Replace('fontSize = 24;','fontSize = 30;') -ceq $qaNewBuilder -and $qaOldModal.Replace('m_FontSize: 24','m_FontSize: 30') -ceq $qaNewModal
    $qaArchive = [IO.Compression.ZipFile]::OpenRead((Join-Path $qaRoot 'Client/Builds/Android/StageGame-development.apk'))
    try {
        $qaApkNotices = @($qaNotices.entries | ForEach-Object {
            $qaEntry = $qaArchive.GetEntry($_.entry)
            if ($null -eq $qaEntry) { throw ('Missing APK notice: ' + $_.entry) }
            $qaStream = $qaEntry.Open()
            try { $qaHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($qaStream)) } finally { $qaStream.Dispose() }
            $qaCopy = Copy-Evidence ('Client/Assets/StreamingAssets/Notices/' + (Split-Path $_.entry -Leaf))
            $qaSourceHash = (Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash
            [pscustomobject]@{entry=$_.entry;bytes=$qaEntry.Length;sha256=$qaHash;matches=($qaHash -eq $_.sha256 -and $qaHash -eq $qaSourceHash -and $qaEntry.Length -eq $_.bytes)}
        })
    } finally { $qaArchive.Dispose() }
    $qaBrowserFiles = @(Get-ChildItem -LiteralPath (Join-Path $qaRoot 'Docs/Management/Evidence/host-ui-browser-20261014') -File)
    foreach ($qaFile in $qaBrowserFiles) { Copy-Evidence ([IO.Path]::GetRelativePath($qaRoot,$qaFile.FullName)) | Out-Null }
    $qaFinal = [pscustomobject]@{
        artifacts=$qaArtifacts;declaredSourceAndCaptureMatches=$qaDeclaredMatches;changedAssetsSinceEditorReview=$qaChangedAssets;onlyLegalBody24To30=$qaFontOnly
        finalUi=[pscustomobject]@{total=[int]$qaUi.'test-run'.total;passed=[int]$qaUi.'test-run'.passed;failed=[int]$qaUi.'test-run'.failed;endUtc=$qaUi.'test-run'.'end-time';cases=@($qaUi.SelectNodes('//test-case') | ForEach-Object { $_.fullname })}
        apkNoticeEntries=$qaApkNotices;apkMatchesNoticeReport=($qaArtifacts[0].sha256 -eq $qaNotices.apkSha256);browserFiles=$qaBrowserFiles.Name
        buildReports=[pscustomobject]@{android=$qaVerification.android;webgl=$qaVerification.webgl}
        scope='Body24 browser functionality and final body30 text/scroll retest must remain separate; read the preserved root browser report for execution and build attribution. No QA browser/device execution.'
    }
}
$qaPostCopy = @($qaManifest | ForEach-Object {
    [pscustomobject]@{source=$_.source;changedDuringRead=((Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256)}
})
$qaReport = [pscustomobject]@{
    dateSeoul='2026-10-14';method='Read-only Client evidence review; no QA Unity/browser/build execution'
    unity=$qaReports;hostSourcesVsIndependent=$qaHostMatch;coreAndDataVsPrevious=$qaCoreData;final=$qaFinal
    captures=$qaImages.Name;captureCount=$qaImages.Count;postCopy=$qaPostCopy
    limits='The full 49-test report precedes QA-009 normalization; targeted 8 includes normalization regression and host UI profiles. Optional final UI 1 covers body30. These runs overlap; do not sum them as distinct tests. Captures are Editor render targets. Optional final property links builds and supplied browser evidence, not QA browser/device execution.'
}
$qaReport | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $qaOut 'results.json') -Encoding utf8
$qaManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
[pscustomobject]@{unity=@($qaReports | Select-Object source,total,passed,failed);hostSourcesMatch=(@($qaHostMatch | Where-Object { -not $_.matches }).Count -eq 0);changedCoreOrData=@($qaCoreData | Where-Object { -not $_.matches });captureCount=$qaImages.Count;changedDuringRead=@($qaPostCopy | Where-Object changedDuringRead);finalUi=$qaFinal.finalUi;artifactMismatches=@($qaFinal.artifacts | Where-Object { -not $_.matches });sourceMismatches=@($qaFinal.declaredSourceAndCaptureMatches | Where-Object { -not $_.matches });onlyLegalBody24To30=$qaFinal.onlyLegalBody24To30;apkNoticeMismatches=@($qaFinal.apkNoticeEntries | Where-Object { -not $_.matches });evidence=$qaOut} | ConvertTo-Json -Depth 5
