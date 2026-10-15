param([Parameter(Mandatory=$true)][string]$RunId)
$ErrorActionPreference='Stop'
if($RunId -notmatch '^[A-Za-z0-9_-]+$'){throw 'Safe new run ID required.'}
$qaRoot=Split-Path $PSScriptRoot -Parent
$qaOut=Join-Path $qaRoot ('Docs/QA/Evidence/'+$RunId)
if(Test-Path -LiteralPath $qaOut){throw 'Preserve prior evidence; use a new run ID.'}
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
$qaV=Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/graphics-refresh-verification.json')|ConvertFrom-Json
$qaGeneric=Get-Content -Raw -LiteralPath (Copy-Evidence 'Client/Evidence/verification.json')|ConvertFrom-Json
$qaFreeze=Get-Content -Raw -LiteralPath (Copy-Evidence ('Client/'+$qaV.sourceFreeze.file))|ConvertFrom-Json
[xml]$qaXml=Get-Content -Raw -LiteralPath (Copy-Evidence ('Client/'+$qaV.tests.file))
[xml]$qaEditor=Get-Content -Raw -LiteralPath (Copy-Evidence ('Client/'+$qaV.editorSafety.file))
$qaNotices=Get-Content -Raw -LiteralPath (Copy-Evidence ('Client/'+$qaV.notices.file))|ConvertFrom-Json
foreach($qaPath in @('Client/Evidence/graphics-refresh-source-freeze-before-editor-guard.json','Client/Evidence/graphics-refresh-idempotence.json','Client/Evidence/graphics-refresh-ui-audit.json','Client/Assets/StageGame/Editor/TabletopUiBuilder.cs','Client/Assets/StageGame/Tests/EditMode/StageMapTests.cs','Client/Evidence/android-build-summary.txt','Client/Evidence/webgl-build-summary.txt','Tests/check-graphics-evidence.ps1')){Copy-Evidence $qaPath|Out-Null}
$qaSourceChecks=@($qaFreeze.files|ForEach-Object{
    $qaPath=Join-Path $qaRoot ('Client/'+$_.path)
    [pscustomobject]@{path=$_.path;matches=((Get-FileHash -LiteralPath $qaPath -Algorithm SHA256).Hash -eq $_.sha256 -and (Get-Item -LiteralPath $qaPath).Length -eq $_.bytes)}
})
$qaDeclared=@(@($qaV.captures)+@($qaV.animation.timeline)|ForEach-Object{
    $qaCopy=Copy-Evidence ('Client/'+$_.path)
    [pscustomobject]@{path=$_.path;matches=((Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash -eq $_.sha256 -and (Get-Item -LiteralPath $qaCopy).Length -eq $_.bytes)}
})
$qaDeclared+=@($qaV.gameData|ForEach-Object{
    $qaExpected=$_.sha256
    foreach($qaPath in @($_.source,$_.runtime)){
        $qaCopy=Copy-Evidence $qaPath
        [pscustomobject]@{path=$qaPath;matches=((Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash -eq $qaExpected)}
    }
})
$qaArtifacts=@($qaV.artifacts|ForEach-Object{
    $qaPath=Join-Path $qaRoot ('Client/'+$_.path);$qaHash=(Get-FileHash -LiteralPath $qaPath -Algorithm SHA256).Hash;$qaBytes=(Get-Item -LiteralPath $qaPath).Length
    $qaOther=$qaGeneric.artifacts|Where-Object path -eq $_.path
    [pscustomobject]@{path=$_.path;sha256=$qaHash;bytes=$qaBytes;matches=($qaHash -eq $_.sha256 -and $qaBytes -eq $_.bytes -and $qaHash -eq $qaOther.sha256 -and $qaBytes -eq $qaOther.bytes)}
})
$qaOld=Get-Content -Raw -LiteralPath (Join-Path $qaRoot 'Docs/QA/Evidence/qa-graphics-frozen-v1-20261014/source-manifest.json')|ConvertFrom-Json
$qaChanges=@($qaOld|Where-Object {$_.source -match '^Client/Assets/' -and (Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256}|ForEach-Object source)
$qaIndependent=@(foreach($qaRun in @('qa-graphics-core-v2-20261014','qa-graphics-l10n-final-20261014')){
    $qaResults=Get-Content -Raw -LiteralPath (Copy-Evidence ('Docs/QA/Evidence/'+$qaRun+'/results.json'))|ConvertFrom-Json
    $qaSources=Get-Content -Raw -LiteralPath (Copy-Evidence ('Docs/QA/Evidence/'+$qaRun+'/source-manifest.json'))|ConvertFrom-Json
    Copy-Evidence ('Docs/QA/Evidence/'+$qaRun+'/post-run-source-check.json')|Out-Null
    $qaDiff=@($qaSources|Where-Object {$_.source -match '^Client/Assets/StageGame/Scripts/|Resources.GameData|GameData.Generated'}|Where-Object{(Get-FileHash -LiteralPath (Join-Path $qaRoot ([IO.Path]::GetRelativePath($qaRoot,$_.source))) -Algorithm SHA256).Hash -ne $_.sha256}|ForEach-Object source)
    [pscustomobject]@{run=$qaRun;passed=$qaResults.passed;failed=$qaResults.failed;assertions=$qaResults.assertions;currentProductMismatches=$qaDiff}
})
$qaArchive=[IO.Compression.ZipFile]::OpenRead((Join-Path $qaRoot 'Client/Builds/Android/StageGame-development.apk'))
try{$qaNoticeChecks=@($qaNotices.entries|ForEach-Object{
    $qaEntry=$qaArchive.GetEntry($_.entry);if($null -eq $qaEntry){throw ('Missing notice '+$_.entry)}
    $qaStream=$qaEntry.Open();try{$qaHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($qaStream))}finally{$qaStream.Dispose()}
    $qaSource=Copy-Evidence ('Client/Assets/StreamingAssets/Notices/'+(Split-Path $_.entry -Leaf))
    [pscustomobject]@{entry=$_.entry;matches=($qaHash -eq $_.sha256 -and $qaHash -eq (Get-FileHash -LiteralPath $qaSource -Algorithm SHA256).Hash -and $qaEntry.Length -eq $_.bytes)}
})}finally{$qaArchive.Dispose()}
$qaBrowser='Docs/Management/Evidence/graphics-refresh-browser-20261014'
foreach($qaFile in Get-ChildItem -LiteralPath (Join-Path $qaRoot $qaBrowser) -File){Copy-Evidence ([IO.Path]::GetRelativePath($qaRoot,$qaFile.FullName))|Out-Null}
$qaBrowserArtifacts=Get-Content -Raw -LiteralPath (Join-Path $qaOut ($qaBrowser+'/webgl-artifacts.json'))|ConvertFrom-Json
$qaBrowserChecks=@($qaBrowserArtifacts|ForEach-Object{
    [pscustomobject]@{path=$_.path;matches=((Get-FileHash -LiteralPath (Join-Path $qaRoot $_.path) -Algorithm SHA256).Hash -eq $_.sha256 -and (Get-Item -LiteralPath (Join-Path $qaRoot $_.path)).Length -eq $_.bytes)}
})
$qaPost=@($qaManifest.Keys|Where-Object {(Get-FileHash -LiteralPath (Join-Path $qaRoot $_) -Algorithm SHA256).Hash -ne $qaManifest[$_]})
$qaReport=[pscustomobject]@{
    method='Read-only final Client artifacts and available root evidence; root report must be reviewed separately for completion and pointer scope'
    unity=[pscustomobject]@{total=[int]$qaXml.'test-run'.total;passed=[int]$qaXml.'test-run'.passed;failed=[int]$qaXml.'test-run'.failed}
    editorSafety=[pscustomobject]@{total=[int]$qaEditor.'test-run'.total;passed=[int]$qaEditor.'test-run'.passed;failed=[int]$qaEditor.'test-run'.failed;scope='Separate Editor-only guard run; do not add to59 as one run'}
    frozenAssetChecks=$qaSourceChecks;declaredCaptureDataChecks=$qaDeclared;artifacts=$qaArtifacts;changesAfterFirstFreeze=$qaChanges;independent=$qaIndependent;notices=$qaNoticeChecks
    apkMatchesNoticeReport=($qaArtifacts[0].sha256 -eq $qaNotices.apkSha256);rootWebglArtifactChecks=$qaBrowserChecks;changedDuringRead=$qaPost
    limits='No QA Unity/build/browser/device execution. Source-only editor guard update does not rerun59 or regenerate runtime. CAP-01 sparse capture close-icon observations require real browser confirmation. Existing live-ad,IAP,server,Toss,device and release gates remain.'
}
$qaReport|ConvertTo-Json -Depth 7|Set-Content -LiteralPath (Join-Path $qaOut 'results.json') -Encoding utf8
@($qaManifest.Keys|Sort-Object|ForEach-Object{[pscustomobject]@{source=$_;sha256=$qaManifest[$_]}})|ConvertTo-Json -Depth 3|Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
[pscustomobject]@{unity=$qaReport.unity;editor=$qaReport.editorSafety;assetCount=$qaSourceChecks.Count;assetMismatch=@($qaSourceChecks|Where-Object {-not $_.matches});declaredMismatch=@($qaDeclared|Where-Object {-not $_.matches});artifactMismatch=@($qaArtifacts|Where-Object {-not $_.matches});changes=$qaChanges;independent=$qaIndependent;noticeMismatch=@($qaNoticeChecks|Where-Object {-not $_.matches});rootArtifactMismatch=@($qaBrowserChecks|Where-Object {-not $_.matches});changedDuringRead=$qaPost;evidence=$qaOut}|ConvertTo-Json -Depth 5
