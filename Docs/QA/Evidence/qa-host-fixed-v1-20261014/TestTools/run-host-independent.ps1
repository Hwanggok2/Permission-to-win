param([Parameter(Mandatory=$true)][string]$RunId)
$ErrorActionPreference = 'Stop'
if ($RunId -notmatch '^[A-Za-z0-9_-]+$') { throw 'Safe new run ID required.' }
$qaRoot = Split-Path $PSScriptRoot -Parent
$qaOut = Join-Path $qaRoot ('Docs/QA/Evidence/' + $RunId)
if (Test-Path -LiteralPath $qaOut) { throw 'Existing evidence is immutable; choose a new run ID.' }
$qaSources = Join-Path $qaOut 'Sources'
$qaTools = Join-Path $qaOut 'TestTools'
New-Item -ItemType Directory -Path $qaSources,$qaTools | Out-Null
$qaManifest = @()
foreach($qaPath in @('Client/Assets/StageGame/Scripts/UI/HostLayout.cs','Client/Assets/StageGame/Scripts/UI/SafeAreaFitter.cs','Client/Assets/WebGLTemplates/StageGamePortrait/host-layout.js','Client/Assets/StageGame/Plugins/WebGL/HostInsets.jslib','Client/HOST-UI-REQUEST.json','Docs/Management/CONTRACT.md','Docs/Design/HOST-UI.md')) {
    $qaCopy=Join-Path $qaSources (Split-Path $qaPath -Leaf)
    Copy-Item -LiteralPath (Join-Path $qaRoot $qaPath) -Destination $qaCopy
    $qaManifest += [pscustomobject]@{source=$qaPath;sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
}
foreach($qaPath in @('Tests/HostLayoutHarness/HostLayoutHarness.csproj','Tests/HostLayoutHarness/Program.cs','Tests/HostLayoutHarness/GeometryStubs.cs','Tests/test-host-bridge.cjs','Tests/run-host-independent.ps1')) {
    $qaCopy=Join-Path $qaTools (Split-Path $qaPath -Leaf)
    Copy-Item -LiteralPath (Join-Path $qaRoot $qaPath) -Destination $qaCopy
    $qaManifest += [pscustomobject]@{source=$qaPath;sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
}
$qaManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
$env:DOTNET_CLI_HOME=Join-Path $PSScriptRoot '.work/dotnet-home'
$env:NUGET_PACKAGES=Join-Path $PSScriptRoot '.work/nuget'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
$qaProject=Join-Path $PSScriptRoot 'HostLayoutHarness/HostLayoutHarness.csproj'
$qaSourceArg='-p:HostSource='+(Join-Path $qaSources 'HostLayout.cs')
& dotnet restore $qaProject --configfile (Join-Path $PSScriptRoot 'NuGet.Config') $qaSourceArg 2>&1 | Tee-Object -FilePath (Join-Path $qaOut 'restore.log')
if($LASTEXITCODE -ne 0){throw 'Restore failed; no tests executed.'}
& dotnet build $qaProject --no-restore --configuration Release '-p:UseSharedCompilation=false' $qaSourceArg 2>&1 | Tee-Object -FilePath (Join-Path $qaOut 'build.log')
if($LASTEXITCODE -ne 0){throw 'Harness compile failed; not a product test failure.'}
& dotnet (Join-Path $PSScriptRoot 'HostLayoutHarness/bin/Release/net10.0/HostLayoutHarness.dll') (Join-Path $qaOut 'geometry-results.json') 2>&1 | Tee-Object -FilePath (Join-Path $qaOut 'geometry.log')
$qaGeometryExit=$LASTEXITCODE
& node (Join-Path $qaTools 'test-host-bridge.cjs') $qaSources (Join-Path $qaOut 'bridge-results.json') 2>&1 | Tee-Object -FilePath (Join-Path $qaOut 'bridge.log')
$qaBridgeExit=$LASTEXITCODE
$qaManifest | ForEach-Object { $qaHash=(Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash; [pscustomobject]@{source=$_.source;snapshotSha256=$_.sha256;currentSha256=$qaHash;changedDuringRun=($qaHash -ne $_.sha256)} } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'post-run-source-check.json') -Encoding utf8
Write-Output "Evidence: $qaOut"
if($qaGeometryExit -ne 0 -or $qaBridgeExit -ne 0){exit 1}
