param([string]$RunId = ('independent-' + (Get-Date -Format 'yyyyMMdd-HHmmss')), [string]$Scope = '', [switch]$SkipStageExploration)
$ErrorActionPreference = 'Stop'
if ($RunId -notmatch '^[A-Za-z0-9_-]+$') { throw 'RunId must be a safe folder name.' }
$qaRoot = Split-Path $PSScriptRoot -Parent
$qaEvidence = Join-Path $qaRoot ('Docs/QA/Evidence/' + $RunId)
if (Test-Path -LiteralPath $qaEvidence) { throw 'Use a new run ID to preserve prior evidence.' }
$qaSnapshot = Join-Path $qaEvidence 'Sources'
$qaData = Join-Path $qaEvidence 'ClientData'
$qaGenerated = Join-Path $qaEvidence 'GeneratedData'
New-Item -ItemType Directory -Path $qaSnapshot, $qaData, $qaGenerated -Force | Out-Null
$qaSources = @(
    'Client/Assets/StageGame/Scripts/Core/ConnectFourBoard.cs',
    'Client/Assets/StageGame/Scripts/Core/AiSearch.cs',
    'Client/Assets/StageGame/Scripts/Core/GameData.cs',
    'Client/Assets/StageGame/Scripts/Core/StringTable.cs',
    'Client/Assets/StageGame/Scripts/Core/ProgressStore.cs',
    'Client/Assets/StageGame/Scripts/Platform/Advertising.cs',
    'Client/Assets/StageGame/Scripts/UI/GameApp.cs',
    'Client/Assets/StageGame/Scripts/UI/TabletopTheme.cs'
)
$qaManifest = foreach ($qaSource in $qaSources) {
    $qaOriginal = Join-Path $qaRoot $qaSource
    $qaCopy = Join-Path $qaSnapshot (Split-Path $qaSource -Leaf)
    Copy-Item -LiteralPath $qaOriginal -Destination $qaCopy
    [pscustomobject]@{ source=$qaSource; sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash }
}
foreach ($qaName in @('stages', 'ads', 'strings')) {
    $qaOriginal = Join-Path $qaRoot ('Client/Assets/StageGame/Resources/GameData/' + $qaName + '.json')
    $qaCopy = Join-Path $qaData ($qaName + '.json')
    Copy-Item -LiteralPath $qaOriginal -Destination $qaCopy
    $qaManifest += [pscustomobject]@{source=$qaOriginal; sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
    $qaOriginal = Join-Path $qaRoot ('GameData/Generated/' + $qaName + '.json')
    if (Test-Path -LiteralPath $qaOriginal) {
        $qaCopy = Join-Path $qaGenerated ($qaName + '.json')
        Copy-Item -LiteralPath $qaOriginal -Destination $qaCopy
        $qaManifest += [pscustomobject]@{source=$qaOriginal; sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
    }
}
$qaAuditInputs = Join-Path $qaEvidence 'AuditInputs'
New-Item -ItemType Directory -Path $qaAuditInputs | Out-Null
foreach ($qaInput in @(
    @{source='Client/LOCALIZATION-REQUEST.json'; name='localization-request.json'},
    @{source='Client/Assets/StageGame/Editor/ProjectBuilder.cs'; name='ProjectBuilder.cs'},
    @{source='GameData/LOCALIZATION-CONTRACT.md'; name='localization-contract.md'},
    @{source='Docs/QA/Evidence/qa-ui-v1-20261014/ClientData/strings.json'; name='previous-strings.json'},
    @{source='Docs/QA/Evidence/qa-l10n-core-v1-20261014/ClientData/strings.json'; name='previous-123-strings.json'},
    @{source='Client/STAGE-MAP-REQUEST.json'; name='stage-map-request.json'},
    @{source='Tests/Fixtures/stage-ux-approved-copy-changes.json'; name='approved-copy-changes.json'},
    @{source='Tests/Fixtures/rank-ui-interaction-cases.json'; name='rank-ui-interaction-cases.json'},
    @{source='Client/Assets/StageGame/Editor/StageMapBuilder.cs'; name='StageMapBuilder.cs'}
)) {
    $qaCopy = Join-Path $qaAuditInputs $qaInput.name
    Copy-Item -LiteralPath (Join-Path $qaRoot $qaInput.source) -Destination $qaCopy
    $qaManifest += [pscustomobject]@{source=$qaInput.source; sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
}
foreach ($qaTool in @('Tests/run-independent.ps1','Tests/IndependentHarness/Program.cs','Tests/IndependentHarness/LocalizationChecks.cs','Tests/IndependentHarness/StageUxChecks.cs','Tests/IndependentHarness/GraphicsChecks.cs','Tests/IndependentHarness/UnityStubs.cs','Tests/IndependentHarness/IndependentHarness.csproj')) {
    $qaCopy = Join-Path $qaAuditInputs (Split-Path $qaTool -Leaf)
    Copy-Item -LiteralPath (Join-Path $qaRoot $qaTool) -Destination $qaCopy
    $qaManifest += [pscustomobject]@{source=$qaTool; sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
}
$qaManifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $qaEvidence 'source-manifest.json') -Encoding utf8
$env:DOTNET_CLI_HOME = Join-Path $PSScriptRoot '.work/dotnet-home'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_NOLOGO = 'true'
$env:NUGET_PACKAGES = Join-Path $PSScriptRoot '.work/nuget'
$qaProject = Join-Path $PSScriptRoot 'IndependentHarness/IndependentHarness.csproj'
& dotnet restore $qaProject --configfile (Join-Path $PSScriptRoot 'NuGet.Config') "-p:SourceSnapshotPath=$qaSnapshot" 2>&1 | Tee-Object -FilePath (Join-Path $qaEvidence 'restore.log')
if ($LASTEXITCODE -ne 0) { throw 'Offline restore failed.' }
& dotnet build $qaProject --no-restore --configuration Release '-p:UseSharedCompilation=false' "-p:SourceSnapshotPath=$qaSnapshot" 2>&1 | Tee-Object -FilePath (Join-Path $qaEvidence 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Independent compilation failed; no tests ran.' }
$qaArguments = @((Join-Path $qaEvidence 'results.json'), $qaData)
if (Test-Path -LiteralPath (Join-Path $qaGenerated 'stages.json')) { $qaArguments += $qaGenerated }
$qaArguments += $(if ($Scope) { $Scope } else { 'all' })
if ($SkipStageExploration) { $qaArguments += '--skip-stage-exploration' }
& dotnet (Join-Path $PSScriptRoot 'IndependentHarness/bin/Release/net10.0/IndependentHarness.dll') @qaArguments 2>&1 | Tee-Object -FilePath (Join-Path $qaEvidence 'run.log')
$qaExit = $LASTEXITCODE
$qaManifest | ForEach-Object {
    $qaOriginal = if ([System.IO.Path]::IsPathRooted($_.source)) { $_.source } else { Join-Path $qaRoot $_.source }
    [pscustomobject]@{source=$_.source; snapshotSha256=$_.sha256; currentSha256=(Get-FileHash -LiteralPath $qaOriginal -Algorithm SHA256).Hash; changedDuringRun=((Get-FileHash -LiteralPath $qaOriginal -Algorithm SHA256).Hash -ne $_.sha256)}
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $qaEvidence 'post-run-source-check.json') -Encoding utf8
Write-Output "Evidence: $qaEvidence"
exit $qaExit
