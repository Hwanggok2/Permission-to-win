param([switch]$FreePlanConfirmed)
$ErrorActionPreference='Stop'
# Run this yourself in a trusted terminal. No credential files are read by this script.
if(!$FreePlanConfirmed){throw 'Confirm Workers Free in your own Cloudflare dashboard, then run with -FreePlanConfirmed. No plan upgrade is performed.'}
$repoRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$serverRoot=Join-Path $repoRoot 'Server'
$pagesRoot='C:\Users\JW\.codex\worktrees\github-pages\StageGame'
$evidence=Join-Path $repoRoot 'Client\Evidence\multiplayer-public-20261018'
$node='C:\Users\JW\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe'
$expected=(Get-Content -LiteralPath (Join-Path $evidence 'deployment-commit.txt') -Raw).Trim()
if((git -C $pagesRoot branch --show-current) -ne 'codex/github-pages'){throw 'Unexpected Pages branch'}
if((git -C $pagesRoot rev-parse HEAD) -ne $expected){throw 'Pages HEAD changed; review the prepared commit before publishing'}
if(@(git -C $pagesRoot status --porcelain).Count){throw 'Pages worktree has uncommitted changes'}
$manifest=Get-Content -LiteralPath (Join-Path $evidence 'published-files.json') -Raw | ConvertFrom-Json
foreach($file in $manifest){
 $path=[IO.Path]::GetFullPath((Join-Path $pagesRoot $file.path))
 if(!$path.StartsWith($pagesRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Invalid deployment path'}
 if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $file.sha256){throw 'Prepared build hash mismatch'}
}
$remoteConfig=Join-Path $serverRoot 'wrangler.multiplayer.remote.jsonc'
if(!(Test-Path -LiteralPath $remoteConfig)){throw 'Local public Worker configuration is missing'}
$config=Get-Content -LiteralPath $remoteConfig -Raw | ConvertFrom-Json
if($config.name -ne 'permission-to-win-multiplayer-dev' -or !$config.workers_dev -or $config.vars.ALLOWED_ORIGINS -notlike '*https://hwanggok2.github.io*'){throw 'Unexpected public Worker configuration'}
Write-Host 'Deploying only permission-to-win-multiplayer-dev. The user pushes Pages separately.'
$env:WRANGLER_SEND_METRICS='false'
Push-Location $serverRoot
try {
 & $node node_modules/wrangler/bin/wrangler.js deploy --config wrangler.multiplayer.remote.jsonc
 if($LASTEXITCODE -ne 0){throw 'Worker deployment failed; Pages was not pushed'}
 & $node scripts/multiplayer-smoke.mjs 'wss://permission-to-win-multiplayer-dev.hsj2014528.workers.dev/v1/play' --public
 if($LASTEXITCODE -ne 0){throw 'Public multiplayer check failed; Pages was not pushed'}
} finally { Pop-Location }
Write-Host "Server checks completed. To publish the prepared web build yourself: git -C `"$pagesRoot`" push origin codex/github-pages"
