$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
New-Item -ItemType Directory -Path (Join-Path $root 'build') -Force | Out-Null
$exe = Join-Path $root 'build/Export-RuntimeArt.exe'
$arguments = @('/nologo','/target:exe','/main:ExportRuntimeArt','/r:System.Drawing.dll','/r:System.Windows.Forms.dll',('/out:'+$exe))
$resources = [ordered]@{
    sit='draft-assets/approved-sit-v1/sit.png'; lay='draft-assets/approved-rest-v1/lay.png'; sleep='draft-assets/approved-rest-v1/sleep.png'
    early='draft-assets/approved-rest-v1/blink-early.png';late='draft-assets/approved-rest-v1/blink-late.png';closed='draft-assets/approved-rest-v1/blink-closed-source.png'
    tiltMid='draft-assets/approved-actions-v1/sit-tilt-v1-mid.png';tiltPeak='draft-assets/approved-actions-v1/sit-tilt-v1-peak.png'
    smileMid='draft-assets/approved-actions-v1/sit-smile-v1-mid.png';smilePeak='draft-assets/approved-actions-v1/sit-smile-v1-peak.png'
    lickStart='draft-assets/approved-actions-v1/sit-lick-v1-start.png';lickPeak='draft-assets/approved-actions-v1/sit-lick-v1-peak.png';lickReturn='draft-assets/approved-actions-v1/sit-lick-v1-return.png'
}
foreach($entry in $resources.GetEnumerator()){$arguments += '/resource:'+(Join-Path $root $entry.Value)+',JudgeReview.'+$entry.Key}
$arguments += (Join-Path $root 'draft-assets/approved-rest-v1/ArtReview.cs'),(Join-Path $root 'draft-assets/approved-actions-v1/ActionReview.cs'),(Join-Path $PSScriptRoot 'Export-RuntimeArt.cs')
& $compiler @arguments
if($LASTEXITCODE -ne 0){throw 'Approved artwork exporter failed to compile.'}
& $exe (Join-Path $root 'art/frames') $root
if($LASTEXITCODE -ne 0){throw 'Approved artwork promotion failed.'}
Write-Host 'Explicit promotion complete; existing different pixels are never overwritten.'
