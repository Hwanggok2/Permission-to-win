param([Parameter(Mandatory=$true)][string]$RunId)
$ErrorActionPreference='Stop'
if ($RunId -notmatch '^[A-Za-z0-9_-]+$') { throw 'Safe new run ID required.' }
$qaRoot=Split-Path $PSScriptRoot -Parent
$qaOut=Join-Path $qaRoot ('Docs/QA/Evidence/'+$RunId)
if(Test-Path -LiteralPath $qaOut){throw 'Existing evidence is immutable; use a new run ID.'}
$qaSources=Join-Path $qaOut 'Sources'
$qaInputs=Join-Path $qaOut 'Inputs'
$qaTools=Join-Path $qaOut 'TestTools'
New-Item -ItemType Directory -Path $qaSources,$qaInputs,$qaTools | Out-Null
$qaManifest=@()
foreach($qaItem in @(
    @{path='Client/Assets/StageGame/Scripts/Core/ProgressStore.cs';directory=$qaSources},
    @{path='Client/Assets/StageGame/Scripts/Core/StringTable.cs';directory=$qaSources},
    @{path='Tests/IndependentHarness/UnityStubs.cs';directory=$qaSources},
    @{path='Tests/Fixtures/stage-ux-legacy-saves.json';directory=$qaInputs},
    @{path='Client/STAGE-MAP-REQUEST.json';directory=$qaInputs},
    @{path='Docs/Management/CONTRACT.md';directory=$qaInputs},
    @{path='Tests/ProgressHarness/ProgressHarness.csproj';directory=$qaTools},
    @{path='Tests/ProgressHarness/Program.cs';directory=$qaTools},
    @{path='Tests/run-progress-independent.ps1';directory=$qaTools}
)) {
    $qaCopy=Join-Path $qaItem.directory (Split-Path $qaItem.path -Leaf)
    Copy-Item -LiteralPath (Join-Path $qaRoot $qaItem.path) -Destination $qaCopy
    $qaManifest += [pscustomobject]@{source=$qaItem.path;sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
}
$qaManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
$env:DOTNET_CLI_HOME=Join-Path $PSScriptRoot '.work/dotnet-home'
$env:NUGET_PACKAGES=Join-Path $PSScriptRoot '.work/nuget'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
$qaProject=Join-Path $qaTools 'ProgressHarness.csproj'
$qaArg='-p:SourceSnapshotPath='+$qaSources
$qaBuild=Join-Path $PSScriptRoot ('.work/progress/'+$RunId)
$qaObj='-p:BaseIntermediateOutputPath='+(Join-Path $qaBuild 'obj/')
$qaBin='-p:OutputPath='+(Join-Path $qaBuild 'bin/')
& dotnet restore $qaProject --configfile (Join-Path $PSScriptRoot 'NuGet.Config') $qaArg $qaObj $qaBin 2>&1 | Tee-Object -FilePath (Join-Path $qaOut 'restore.log')
if($LASTEXITCODE -ne 0){throw 'Restore failed; no tests ran.'}
& dotnet build $qaProject --no-restore --configuration Release '-p:UseSharedCompilation=false' $qaArg $qaObj $qaBin 2>&1 | Tee-Object -FilePath (Join-Path $qaOut 'build.log')
if($LASTEXITCODE -ne 0){throw 'Harness compile failed; not a product test failure.'}
& dotnet (Join-Path $qaBuild 'bin/ProgressHarness.dll') (Join-Path $qaInputs 'stage-ux-legacy-saves.json') (Join-Path $qaOut 'results.json') 2>&1 | Tee-Object -FilePath (Join-Path $qaOut 'run.log')
$qaExit=$LASTEXITCODE
$qaManifest | ForEach-Object { $qaHash=(Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash; [pscustomobject]@{source=$_.source;snapshotSha256=$_.sha256;currentSha256=$qaHash;changedDuringRun=($qaHash -ne $_.sha256)} } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'post-run-source-check.json') -Encoding utf8
Write-Output "Evidence: $qaOut"
exit $qaExit
