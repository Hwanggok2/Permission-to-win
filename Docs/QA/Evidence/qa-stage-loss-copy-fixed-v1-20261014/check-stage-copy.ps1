param([Parameter(Mandatory=$true)][string]$RunId)
$ErrorActionPreference='Stop'
if($RunId -notmatch '^[A-Za-z0-9_-]+$'){throw 'Safe new run ID required.'}
$qaRoot=Split-Path $PSScriptRoot -Parent
$qaOut=Join-Path $qaRoot ('Docs/QA/Evidence/'+$RunId)
if(Test-Path -LiteralPath $qaOut){throw 'Use a new run ID to preserve prior evidence.'}
New-Item -ItemType Directory -Path $qaOut | Out-Null
$qaManifest=@()
foreach($qaItem in @(
    @{source='Docs/QA/Evidence/qa-stage-ui-pre-v1-20261014/ClientData/strings.json';name='before-strings.json'},
    @{source='GameData/Generated/strings.json';name='generated-strings.json'},
    @{source='Client/Assets/StageGame/Resources/GameData/strings.json';name='client-strings.json'},
    @{source='Tests/Fixtures/stage-ux-approved-copy-changes.json';name='approved-copy-changes.json'},
    @{source='Tests/check-stage-copy.ps1';name='check-stage-copy.ps1'}
)){
    $qaCopy=Join-Path $qaOut $qaItem.name; Copy-Item -LiteralPath (Join-Path $qaRoot $qaItem.source) -Destination $qaCopy
    $qaManifest += [pscustomobject]@{source=$qaItem.source;sha256=(Get-FileHash -LiteralPath $qaCopy -Algorithm SHA256).Hash}
}
$qaBefore=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'before-strings.json') | ConvertFrom-Json
$qaGenerated=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'generated-strings.json') | ConvertFrom-Json
$qaClient=Get-Content -Raw -LiteralPath (Join-Path $qaOut 'client-strings.json') | ConvertFrom-Json
$qaApproved=(Get-Content -Raw -LiteralPath (Join-Path $qaOut 'approved-copy-changes.json') | ConvertFrom-Json).changes
$qaExceptions=@{}; foreach($qaChange in $qaApproved){$qaExceptions[$qaChange.key]=$qaChange}
$qaFailures=@()
if((@($qaApproved.key | Sort-Object) -join ',') -cne 'result_loss_diagonal,result_loss_horizontal,result_loss_vertical'){$qaFailures+='Unexpected approved keys'}
if($qaBefore.strings.Count -ne 137 -or $qaGenerated.strings.Count -ne 137 -or $qaClient.strings.Count -ne 137){$qaFailures+='Expected137 rows'}
if($qaGenerated.schemaVersion -ne 1 -or $qaClient.schemaVersion -ne 1){$qaFailures+='Schema changed'}
$qaBeforeRows=@{}; foreach($qaRow in $qaBefore.strings){$qaBeforeRows[$qaRow.key]=$qaRow}
$qaCurrentRows=@{}; foreach($qaRow in $qaGenerated.strings){if($qaCurrentRows.ContainsKey($qaRow.key)){$qaFailures+=('Duplicate key: '+$qaRow.key)};$qaCurrentRows[$qaRow.key]=$qaRow}
$qaChanged=@()
foreach($qaKey in $qaBeforeRows.Keys){
    $qaOld=$qaBeforeRows[$qaKey]; $qaNew=$qaCurrentRows[$qaKey]
    if($null -eq $qaNew){$qaFailures+=('Missing key: '+$qaKey);continue}
    foreach($qaLanguage in @('ko','en')){
        $qaExpected=$qaOld.$qaLanguage
        if($qaExceptions.ContainsKey($qaKey)){
            $qaChange=$qaExceptions[$qaKey]; $qaExpected=$qaChange.after.$qaLanguage
            if($qaOld.$qaLanguage -cne $qaChange.before.$qaLanguage -or $qaExpected -cne ($qaOld.$qaLanguage -split "`n")[0]){$qaFailures+=('Approved baseline/first sentence mismatch: '+$qaKey+'/'+$qaLanguage)}
        }
        if($qaNew.$qaLanguage -cne $qaExpected){$qaFailures+=('Unexpected copy: '+$qaKey+'/'+$qaLanguage)}
    }
    if($qaNew.ko -cne $qaOld.ko -or $qaNew.en -cne $qaOld.en){$qaChanged+=$qaKey}
}
if((@($qaChanged | Sort-Object) -join ',') -cne 'result_loss_diagonal,result_loss_horizontal,result_loss_vertical'){$qaFailures+='Delta must be exactly3 approved keys'}
$qaGeneratedHash=(Get-FileHash -LiteralPath (Join-Path $qaOut 'generated-strings.json') -Algorithm SHA256).Hash
$qaClientHash=(Get-FileHash -LiteralPath (Join-Path $qaOut 'client-strings.json') -Algorithm SHA256).Hash
if($qaGeneratedHash -ne $qaClientHash){$qaFailures+='Generated/Client bytes differ'}
$qaPost=@($qaManifest | Where-Object {(Get-FileHash -LiteralPath (Join-Path $qaRoot $_.source) -Algorithm SHA256).Hash -ne $_.sha256} | ForEach-Object source)
if($qaPost.Count -gt 0){$qaFailures+='Source changed during comparison'}
$qaReport=[pscustomobject]@{method='Independent exact JSON comparison, no game/Unity/Excel/exporter execution';result=$(if($qaFailures.Count -eq 0){'PASS'}else{'FAIL'});rowCount=$qaGenerated.strings.Count;approvedChangedKeys=@($qaChanged | Sort-Object);unchangedRows=(137-$qaChanged.Count);generatedSha256=$qaGeneratedHash;clientSha256=$qaClientHash;failures=$qaFailures;changedDuringRead=$qaPost;limits='Exact copy/data change verification only. Root must confirm final loss UI and final artifact linkage before QA-010 closure.'}
$qaReport | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $qaOut 'results.json') -Encoding utf8
$qaManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qaOut 'source-manifest.json') -Encoding utf8
$qaReport | ConvertTo-Json -Depth 5
if($qaFailures.Count -gt 0){exit 1}
