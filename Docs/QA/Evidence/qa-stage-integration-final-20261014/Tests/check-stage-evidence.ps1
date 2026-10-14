param([Parameter(Mandatory=$true)][string]$RunId, [switch]$IncludeFinalBrowser)
$ErrorActionPreference='Stop'
if($RunId -notmatch '^[A-Za-z0-9_-]+$'){throw 'Safe new run ID required.'}
$qaRoot=Split-Path $PSScriptRoot -Parent
$qaOut=Join-Path $qaRoot ('Docs/QA/Evidence/'+$RunId)
if(Test-Path -LiteralPath $qaOut){throw 'Use a new run ID to preserve prior evidence.'}
$qaBrowser='Docs/Management/Evidence/reference-ux-browser-20261014'
if($IncludeFinalBrowser){foreach($qaName in @('README.md','final-artifacts.json','final-console.json')){if(-not(Test-Path -LiteralPath (Join-Path $qaRoot ($qaBrowser+'/'+$qaName)))){throw ('Final browser evidence missing: '+$qaName)}}}
New-Item -ItemType Directory -Path $qaOut | Out-Null
$qaManifest=@{}
function Copy-Evidence([string]$Relative){
    $qaTarget=Join-Path $qaOut $Relative
    if(-not $script:qaManifest.ContainsKey($Relative)){
        New-Item -ItemType Directory -Path (Split-Path $qaTarget -Parent) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $qaRoot $Relative) -Destination $qaTarget
        $script:qaManifest[$Relative]=(Get-FileHash -LiteralPath $qaTarget -Algorithm SHA256).Hash
    }
    return $qaTarget
}
$qaMap=Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/stage-map-verification.json') | ConvertFrom-Json
$qaV=Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/verification.json') | ConvertFrom-Json
$qaIdempotence=Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/stage-map-idempotence.json') | ConvertFrom-Json
$qaNotices=Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/apk-notices.json') | ConvertFrom-Json
$qaCopyCorrection=Get-Content -Raw -LiteralPath (Copy-Evidence 'Docs/QA/Evidence/qa-stage-loss-copy-fixed-v1-20261014/results.json') | ConvertFrom-Json
Copy-Evidence 'Tests/Fixtures/stage-ux-approved-copy-changes.json' | Out-Null
[xml]$qaXml=Get-Content -Raw -LiteralPath (Copy-Evidence ('Client/'+$qaMap.tests.file))
$qaClientCopy=Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/loss-copy-change.json') | ConvertFrom-Json
[xml]$qaCopyXml=Get-Content -Raw -LiteralPath (Copy-Evidence ('Client/'+$qaMap.tests.finalCopyImpact.file))
foreach($qaPath in @('Client/Evidence/android-build-summary.txt','Client/Evidence/webgl-build-summary.txt','Client/STAGE-MAP-REQUEST.json','Client/Assets/StageGame/Tests/EditMode/StageMapTests.cs','Client/Assets/StageGame/Tests/EditMode/PlayFlowTests.cs','Client/Assets/StageGame/Tests/EditMode/LocalizationTests.cs','Client/Assets/StageGame/Prefabs/UI/Common/StageNode.prefab.meta','Docs/Management/CONTRACT.md','Tests/check-stage-evidence.ps1')){Copy-Evidence $qaPath | Out-Null}
$qaArtifacts=@($qaMap.artifacts | ForEach-Object{
    $qaPath=Join-Path (Join-Path $qaRoot 'Client') $_.path
    $qaHash=(Get-FileHash -LiteralPath $qaPath -Algorithm SHA256).Hash
    $qaBytes=(Get-Item -LiteralPath $qaPath).Length
    $qaOther=$qaV.artifacts | Where-Object path -eq $_.path
    [pscustomobject]@{path=$_.path;sha256=$qaHash;bytes=$qaBytes;matchesMap=($qaHash -eq $_.sha256 -and $qaBytes -eq $_.bytes);matchesVerification=($null -ne $qaOther -and $qaHash -eq $qaOther.sha256 -and $qaBytes -eq $qaOther.bytes)}
})
$qaDeclared=@(@($qaMap.sourceFiles)+@($qaMap.captures)+@($qaV.runtimeSources) | ForEach-Object{
    $qaCopy=Copy-Evidence ('Client/'+$_.path)
    [pscustomobject]@{path=$_.path;matches=((Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash -eq $_.sha256 -and ($null -eq $_.bytes -or (Get-Item -LiteralPath $qaCopy).Length -eq $_.bytes))}
})
$qaIndependent=Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-stage-ui-pre-v1-20261014/source-manifest.json') | ConvertFrom-Json
$qaRuntimeAndData=@($qaIndependent | Where-Object { $_.source -match '^Client/Assets/StageGame/Scripts/|[/\\]Resources[/\\]GameData[/\\]|[/\\]GameData[/\\]Generated[/\\]' } | ForEach-Object{
    $qaPath=if([IO.Path]::IsPathRooted($_.source)){$_.source}else{Join-Path $qaRoot $_.source}
    $qaHash=(Get-FileHash -LiteralPath $qaPath -Algorithm SHA256).Hash
    Copy-Evidence ([IO.Path]::GetRelativePath($qaRoot,$qaPath)) | Out-Null
    $qaApprovedCopy=$_.source -match '(^|[/\\])strings\.json$' -and $qaCopyCorrection.result -eq 'PASS' -and $qaCopyCorrection.unchangedRows -eq 134 -and $qaHash -eq $qaCopyCorrection.clientSha256
    [pscustomobject]@{source=$_.source;independentSha256=$_.sha256;currentSha256=$qaHash;matches=($qaHash -eq $_.sha256);approvedCopyOnlyChange=$qaApprovedCopy}
})
$qaPrefabBefore=Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-stage-prefab-pre-v1-20261014/source-manifest.json') | ConvertFrom-Json
$qaPrefabChanges=@($qaPrefabBefore | Where-Object { $_.source -match '^Client/Assets/' } | Where-Object { (Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256 } | ForEach-Object source)
$qaReapply=@($qaIdempotence.files | ForEach-Object{
    $qaPath=Join-Path (Join-Path $qaRoot 'Client') $_.path
    [pscustomobject]@{path=$_.path;reportedUnchanged=($_.before -eq $_.after -and $_.unchanged);currentMatchesAfter=((Get-FileHash -LiteralPath $qaPath -Algorithm SHA256).Hash -eq $_.after)}
})
$qaArchive=[IO.Compression.ZipFile]::OpenRead((Join-Path $qaRoot 'Client/Builds/Android/StageGame-development.apk'))
try{
    $qaApkNotices=@($qaNotices.entries | ForEach-Object{
        $qaEntry=$qaArchive.GetEntry($_.entry); if($null -eq $qaEntry){throw ('Missing APK notice '+$_.entry)}
        $qaStream=$qaEntry.Open(); try{$qaHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($qaStream))}finally{$qaStream.Dispose()}
        $qaSource=Copy-Evidence ('Client/Assets/StreamingAssets/Notices/'+(Split-Path $_.entry -Leaf))
        [pscustomobject]@{entry=$_.entry;matches=($qaHash -eq $_.sha256 -and $qaHash -eq (Get-FileHash -LiteralPath $qaSource -Algorithm SHA256).Hash -and $qaEntry.Length -eq $_.bytes)}
    })
}finally{$qaArchive.Dispose()}
$qaBrowserResult=$null
if($IncludeFinalBrowser){
    foreach($qaFile in Get-ChildItem -LiteralPath (Join-Path $qaRoot $qaBrowser) -File){Copy-Evidence ([IO.Path]::GetRelativePath($qaRoot,$qaFile.FullName)) | Out-Null}
    $qaRootArtifacts=Get-Content -Raw -LiteralPath (Join-Path $qaOut ($qaBrowser+'/final-artifacts.json')) | ConvertFrom-Json
    $qaLinks=@($qaRootArtifacts | ForEach-Object{
        $qaPath=$_.path -replace '^Client/',''; $qaActual=$qaArtifacts | Where-Object path -eq $qaPath
        [pscustomobject]@{path=$_.path;matches=($null -ne $qaActual -and $qaActual.sha256 -eq $_.sha256 -and $qaActual.bytes -eq $_.bytes)}
    })
    $qaConsole=Get-Content -Raw -LiteralPath (Join-Path $qaOut ($qaBrowser+'/final-console.json')) | ConvertFrom-Json
    $qaBrowserResult=[pscustomobject]@{rootArtifactMatches=$qaLinks;consoleEntryCount=@($qaConsole).Count;report=($qaBrowser+'/README.md');scope='Root executes real localhost pointers. QA only reads evidence. Native migration/final30/draw automatic tests are distinct from human browser play.'}
}
$qaCases=@($qaXml.SelectNodes('//test-case') | ForEach-Object{[pscustomobject]@{name=$_.fullname;result=$_.result}})
$qaCopyCases=@($qaCopyXml.SelectNodes('//test-case') | ForEach-Object{[pscustomobject]@{name=$_.fullname;result=$_.result}})
$qaRefreshedCaptures=@($qaMap.captures | Where-Object path -match '^Evidence/map-(ko|en)-320-(result-[012]|all-cleared)\.png$' | ForEach-Object path)
$qaCopySources=@($qaClientCopy.unchangedSourceFiles | ForEach-Object{
    [pscustomobject]@{path=$_.path;matches=((Get-FileHash -LiteralPath (Join-Path $qaRoot ('Client/'+$_.path)) -Algorithm SHA256).Hash -eq $_.sha256)}
})
$qaPost=@($qaManifest.Keys | Sort-Object | ForEach-Object{[pscustomobject]@{source=$_;changedDuringRead=((Get-FileHash -LiteralPath (Join-Path $qaRoot $_) -Algorithm SHA256).Hash -ne $qaManifest[$_])}})
$qaReport=[pscustomobject]@{
    method='Read-only Client Unity/build/capture and optional root browser evidence; no QA Unity/build/browser/device execution'
    unity=[pscustomobject]@{total=[int]$qaXml.'test-run'.total;passed=[int]$qaXml.'test-run'.passed;failed=[int]$qaXml.'test-run'.failed;endUtc=$qaXml.'test-run'.'end-time';cases=$qaCases;matchesDeclaration=([int]$qaXml.'test-run'.total -eq $qaMap.tests.total -and [int]$qaXml.'test-run'.passed -eq $qaMap.tests.passed)}
    finalCopyUnity=[pscustomobject]@{total=[int]$qaCopyXml.'test-run'.total;passed=[int]$qaCopyXml.'test-run'.passed;failed=[int]$qaCopyXml.'test-run'.failed;endUtc=$qaCopyXml.'test-run'.'end-time';cases=$qaCopyCases;matchesDeclaration=([int]$qaCopyXml.'test-run'.total -eq $qaMap.tests.finalCopyImpact.total -and [int]$qaCopyXml.'test-run'.passed -eq $qaMap.tests.finalCopyImpact.passed);scope='Separate copy-impact run; overlaps initial57, never sum as68';unchangedSourceMatches=$qaCopySources}
    captureProvenance=[pscustomobject]@{initialRunRetained=($qaMap.captures.Count-$qaRefreshedCaptures.Count);finalCopyRunRefreshed=$qaRefreshedCaptures;basis='ResultPriorityAndFinalStageClearDriveReplayMap passed in final11; PlayFlowTests writes six outcome and two all-cleared images; captured file times agree with final XML'}
    artifacts=$qaArtifacts;declaredSourceCaptureMatches=$qaDeclared;runtimeAndDataVsIndependent=$qaRuntimeAndData;changesSinceStaticPrefabReview=$qaPrefabChanges;clientReapplyEvidence=$qaReapply;copyCorrection=$qaCopyCorrection
    apkNotices=$qaApkNotices;apkMatchesNoticeReport=($qaArtifacts[0].sha256 -eq $qaNotices.apkSha256);captureCount=$qaMap.captures.Count;buildReports=[pscustomobject]@{android=$qaV.android;webgl=$qaV.webgl};browser=$qaBrowserResult;postCopy=$qaPost
    limits='Unity57 precedes the approved3 loss-text correction; a separate final11 overlaps that suite and refreshed8 of40 captures, leaving32 initial captures. Runtime/prefabs remain unchanged. Final text uses exact bilingual JSON comparison plus final ko/en vertical-loss render images and root Korean vertical-loss pointer retest. Native JSON/PlayerPrefs and render events are not device durability or Toss SDK. Final30 images use injected terminal fixtures, not a human30-stage campaign. BuildReport bytes are not APK file size. Existing release gates remain.'
}
$qaReport | ConvertTo-Json -Depth 9 | Set-Content -LiteralPath (Join-Path $qaOut 'results.json') -Encoding utf8
@($qaManifest.Keys | Sort-Object | ForEach-Object{[pscustomobject]@{source=$_;sha256=$qaManifest[$_]}}) | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
[pscustomobject]@{unityPassed=$qaReport.unity.passed;unityTotal=$qaReport.unity.total;declarationMatches=$qaReport.unity.matchesDeclaration;finalCopyUnityPassed=$qaReport.finalCopyUnity.passed;finalCopyUnityTotal=$qaReport.finalCopyUnity.total;finalCopyDeclarationMatches=$qaReport.finalCopyUnity.matchesDeclaration;copySourceMismatches=@($qaCopySources | Where-Object { -not $_.matches });artifactMismatches=@($qaArtifacts | Where-Object { -not $_.matchesMap -or -not $_.matchesVerification });sourceCaptureMismatches=@($qaDeclared | Where-Object { -not $_.matches });unapprovedRuntimeDataChanges=@($qaRuntimeAndData | Where-Object { -not $_.matches -and -not $_.approvedCopyOnlyChange });approvedDataChanges=@($qaRuntimeAndData | Where-Object { -not $_.matches -and $_.approvedCopyOnlyChange } | ForEach-Object source);changesSinceStaticPrefabReview=$qaPrefabChanges;reapplyMismatch=@($qaReapply | Where-Object { -not $_.reportedUnchanged -or -not $_.currentMatchesAfter });apkNoticeMismatch=@($qaApkNotices | Where-Object { -not $_.matches });captureCount=$qaMap.captures.Count;refreshedCaptureCount=$qaRefreshedCaptures.Count;browser=$qaBrowserResult;changedDuringRead=@($qaPost | Where-Object changedDuringRead);evidence=$qaOut} | ConvertTo-Json -Depth 6
