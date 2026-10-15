param([Parameter(Mandatory=$true)][string]$RunId, [string]$CandidateId = '', [string]$Scope = '', [switch]$SnapshotOnly)
$ErrorActionPreference = 'Stop'
if ($RunId -notmatch '^[A-Za-z0-9_-]+$' -or ($CandidateId -and $CandidateId -notmatch '^[A-Za-z0-9_-]+$')) { throw 'Use safe run IDs.' }
$qaRoot = Split-Path $PSScriptRoot -Parent
$qaEvidence = Join-Path $qaRoot ('Docs/QA/Evidence/' + $RunId)
if (Test-Path -LiteralPath $qaEvidence) { throw 'Use a new run ID; previous evidence is immutable.' }
New-Item -ItemType Directory -Path $qaEvidence | Out-Null
$qaManifest = @()
if ($CandidateId) {
    $qaCandidate = Join-Path $qaRoot ('Docs/QA/Evidence/' + $CandidateId)
    $qaInputs = Get-Content -LiteralPath (Join-Path $qaCandidate 'source-manifest.json') -Raw | ConvertFrom-Json
    foreach ($qaInput in $qaInputs) {
        $qaCopy = Join-Path $qaEvidence $qaInput.snapshot
        New-Item -ItemType Directory -Path (Split-Path $qaCopy -Parent) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $qaCandidate $qaInput.snapshot) -Destination $qaCopy
        if ((Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash -ne $qaInput.sha256) { throw 'Candidate hash mismatch.' }
        $qaManifest += $qaInput
    }
} else {
    $qaPaths = @(
        'Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs',
        'Client/Assets/StageGame/Scripts/Core/AiSearch.cs',
        'Client/Assets/StageGame/Scripts/Core/GameData.cs',
        'Client/Assets/StageGame/Scripts/Core/StringTable.cs',
        'Client/Assets/StageGame/Scripts/Core/ProgressStore.cs',
        'Client/Assets/StageGame/Scripts/Platform/Advertising.cs',
        'Client/Assets/StageGame/Scripts/UI/RankGameApp.cs',
        'Client/Assets/StageGame/Scripts/UI/RankTheme.cs',
        'Client/Assets/StageGame/Scripts/UI/RankStageCard.cs',
        'Client/Assets/StageGame/Scripts/UI/RankDetailBlocker.cs',
        'Client/Assets/StageGame/Scripts/UI/GameApp.cs',
        'Client/Assets/StageGame/Scripts/UI/TabletopTheme.cs'
    )
    $qaInputs = @($qaPaths | ForEach-Object { @{source=$_; snapshot=('Sources/' + (Split-Path $_ -Leaf))} })
    foreach ($qaName in @('stages','strings','ads')) {
        $qaInputs += @{source=('Client/Assets/StageGame/Resources/GameData/' + $qaName + '.json');snapshot=('ClientData/' + $qaName + '.json')}
        $qaInputs += @{source=('GameData/Generated/' + $qaName + '.json');snapshot=('GeneratedData/' + $qaName + '.json')}
    }
    foreach ($qaPath in @('Docs/Management/CONTRACT.md','GameData/RANK-UI-CONTRACT.md','Tests/Fixtures/rank-ui-interaction-cases.json','Tests/Fixtures/rank-ui-crossing-win-cases.json','Tests/Fixtures/stage-ux-legacy-saves.json')) {
        $qaInputs += @{source=$qaPath;snapshot=('AuditInputs/' + (Split-Path $qaPath -Leaf))}
    }
    foreach ($qaPath in @('GameData/V9-UI-CONTRACT.md','GameData/STORY-CONTRACT.md','GameData/Validation/v9-ui-changes.json','GameData/Validation/story-changes.json','GameData/Validation/story-results.json','Tests/Fixtures/rank-v9-flow-cases.json')) {
        $qaInputs += @{source=$qaPath;snapshot=('AuditInputs/' + (Split-Path $qaPath -Leaf))}
    }
    foreach ($qaName in @('stages','strings','ads')) {
        $qaInputs += @{source=('Docs/QA/Evidence/qa-rank-runtime-impact-v3-20261014/ClientData/' + $qaName + '.json');snapshot=('V8Data/' + $qaName + '.json')}
    }
    foreach ($qaInput in $qaInputs) {
        $qaOriginal = Join-Path $qaRoot $qaInput.source
        $qaCopy = Join-Path $qaEvidence $qaInput.snapshot
        New-Item -ItemType Directory -Path (Split-Path $qaCopy -Parent) -Force | Out-Null
        $qaBefore = (Get-FileHash -LiteralPath $qaOriginal -Algorithm SHA256).Hash
        Copy-Item -LiteralPath $qaOriginal -Destination $qaCopy
        $qaHash = (Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash
        if ($qaBefore -ne $qaHash -or $qaHash -ne (Get-FileHash -LiteralPath $qaOriginal -Algorithm SHA256).Hash) { throw 'Source changed during snapshot.' }
        $qaManifest += [pscustomobject]@{source=$qaInput.source;snapshot=$qaInput.snapshot;sha256=$qaHash}
    }
}
if (-not $SnapshotOnly) {
    foreach ($qaName in @('Program.cs','V9Checks.cs','RankHarness.csproj','UnityStubs.cs','WinningSegmentOracle.cs')) {
        $qaOriginal = Join-Path $PSScriptRoot ('RankHarness/' + $qaName)
        $qaCopy = Join-Path $qaEvidence ('TestTools/' + $qaName)
        New-Item -ItemType Directory -Path (Split-Path $qaCopy -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $qaOriginal -Destination $qaCopy
        $qaManifest += [pscustomobject]@{source=('Tests/RankHarness/' + $qaName);snapshot=('TestTools/' + $qaName);sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
    }
    Copy-Item -LiteralPath $PSCommandPath -Destination (Join-Path $qaEvidence 'TestTools/run-rank-independent.ps1')
    $qaManifest += [pscustomobject]@{source='Tests/run-rank-independent.ps1';snapshot='TestTools/run-rank-independent.ps1';sha256=(Get-FileHash -LiteralPath $PSCommandPath -Algorithm SHA256).Hash}
}
$qaManifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $qaEvidence 'source-manifest.json') -Encoding utf8
if ($SnapshotOnly) { Write-Output "Candidate snapshot: $qaEvidence"; exit 0 }
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.work/dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_NOLOGO = 'true'
$env:NUGET_PACKAGES = Join-Path $PSScriptRoot '.work/nuget'
$qaProject = Join-Path $PSScriptRoot 'RankHarness/RankHarness.csproj'
$qaCompileArgs = @("-p:SourceSnapshotPath=$(Join-Path $qaEvidence 'Sources')", "-p:TestSnapshotPath=$(Join-Path $qaEvidence 'TestTools')")
& dotnet restore $qaProject --configfile (Join-Path $PSScriptRoot 'NuGet.Config') @qaCompileArgs 2>&1 | Tee-Object -FilePath (Join-Path $qaEvidence 'restore.log')
if ($LASTEXITCODE -ne 0) { throw 'Offline restore failed; no tests executed.' }
& dotnet build $qaProject --no-restore --configuration Release '-p:UseSharedCompilation=false' @qaCompileArgs 2>&1 | Tee-Object -FilePath (Join-Path $qaEvidence 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed; no tests executed.' }
& dotnet (Join-Path $PSScriptRoot 'RankHarness/bin/Release/net10.0/RankHarness.dll') $qaEvidence $Scope 2>&1 | Tee-Object -FilePath (Join-Path $qaEvidence 'run.log')
$qaExit = $LASTEXITCODE
$qaManifest | ForEach-Object {
    $qaCurrentHash = (Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash
    [pscustomobject]@{source=$_.source;snapshotSha256=$_.sha256;currentSha256=$qaCurrentHash;changedSinceSnapshot=($qaCurrentHash -ne $_.sha256)}
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $qaEvidence 'post-run-source-check.json') -Encoding utf8
Write-Output "Evidence: $qaEvidence"
exit $qaExit
