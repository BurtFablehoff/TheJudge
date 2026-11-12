param([switch]$ActionReview)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'Windows .NET Framework C# compiler is required.' }
New-Item -ItemType Directory -Path (Join-Path $projectRoot 'test-exes'),(Join-Path $projectRoot 'build') -Force | Out-Null
$exe = Join-Path $projectRoot 'test-exes/Judge-ArtReview-v1.exe'
$inputs = [ordered]@{
    sit='draft-assets/approved-sit-v1/sit.png'
    lay='draft-assets/lay-v2/lay.png'
    sleep='draft-assets/sleep-v2/sleep.png'
    early='draft-assets/lay-blink-v1/early.png'
    late='draft-assets/lay-blink-v1/late.png'
    closed='draft-assets/sleep-v1/sleep.png'
}
$reportStem = 'art-review-v1'
$gifStem = 'blink-review-v1'
$frameFolder = 'blink-review-frames'
$frameMode = '--blink-frames'
$sourceFile = 'ArtReview.cs'
$delays = @(220,6,6,8,6,6,30)
if ($ActionReview) {
    $exe = Join-Path $projectRoot 'test-exes/Judge-ActionReview-v1.exe'
    $inputs = [ordered]@{
        sit='draft-assets/approved-sit-v1/sit.png'
        tiltMid='draft-assets/sit-tilt-v1/mid.png'
        tiltPeak='draft-assets/sit-tilt-v1/peak.png'
        smileMid='draft-assets/sit-smile-v1/mid.png'
        smilePeak='draft-assets/sit-smile-v1/peak.png'
        lickStart='draft-assets/sit-lick-v1/start.png'
        lickPeak='draft-assets/sit-lick-v1/peak.png'
        lickReturn='draft-assets/sit-lick-v1/return.png'
    }
    $reportStem = 'action-review-v1'
    $gifStem = 'actions-review-v1'
    $frameFolder = 'action-review-frames'
    $frameMode = '--action-frames'
    $sourceFile = 'ActionReview.cs'
    $delays = @(200,8,4,4,8,76,12,48)
}
$compilerArguments = @('/nologo','/target:winexe','/optimize+','/r:System.Drawing.dll','/r:System.Windows.Forms.dll',('/out:'+$exe))
foreach ($entry in $inputs.GetEnumerator()) {
    $path = Join-Path $projectRoot $entry.Value
    if (-not (Test-Path -LiteralPath $path)) { throw "Missing artwork: $path" }
    $compilerArguments += ('/resource:'+$path+',JudgeReview.'+$entry.Key)
}
$compilerArguments += (Join-Path $PSScriptRoot $sourceFile)
& $compiler @compilerArguments
if ($LASTEXITCODE -ne 0) { throw 'Artwork review build failed.' }
# These modes do not create windows or change the desktop.
foreach ($mode in @('check','render')) {
    $extension = if ($mode -eq 'check') { 'json' } else { 'png' }
    $report = Join-Path $projectRoot ('build/'+$reportStem+'.'+$extension)
    $process = Start-Process -FilePath $exe -ArgumentList @('--'+$mode,('"'+$report+'"')) -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Artwork review $mode failed." }
}
Write-Host "Ready: $exe"
$frames = Join-Path $projectRoot ('build/'+$frameFolder)
$process = Start-Process -FilePath $exe -ArgumentList @($frameMode,('"'+$frames+'"')) -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw 'Blink preview frame render failed.' }
& (Join-Path $PSScriptRoot 'SharedWrite-AnimatedGif.ps1') -FramesDirectory $frames -OutputPath (Join-Path $projectRoot ('build/'+$gifStem+'.gif')) -Delays $delays
