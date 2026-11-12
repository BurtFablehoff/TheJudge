$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
& (Join-Path $root 'build.ps1')
& (Join-Path $root 'build.ps1') -Dev
New-Item -ItemType Directory -Path (Join-Path $root 'build') -Force | Out-Null
foreach ($kind in @('production','dev')) {
    $name = if ($kind -eq 'dev') { 'TheJudge-Dev-0.1.7.exe' } else { 'TheJudge-0.1.7.exe' }
    $suffix = if ($kind -eq 'dev') { 'dev-0.1.7' } else { '0.1.7' }
    $exe = Join-Path $root ('test-exes/'+$name)
    foreach ($mode in @('self-test','test-desktop','test-product')) {
        $target = Join-Path $root ('build/'+$mode+'-'+$suffix+'.json')
        $p = Start-Process -FilePath $exe -ArgumentList @('--'+$mode,('"'+$target+'"')) -WindowStyle Hidden -Wait -PassThru
        if ($p.ExitCode -ne 0) { throw ('Check failed: '+$kind+' '+$mode+'; see '+$target+'.error.txt') }
    }
}
$exe=Join-Path $root 'test-exes/TheJudge-0.1.7.exe'
foreach($command in @('render-review','render-transitions','render-awake-actions','render-sleep-transitions','render-pose-cycle')) {
    $target = switch($command) {
        'self-test' { Join-Path $root 'build/self-test-0.1.7.json' }
        'test-desktop' { Join-Path $root 'build/desktop-test-0.1.7.json' }
        'render-review' { Join-Path $root 'build/runtime-review' }
        'render-transitions' { Join-Path $root 'build/transition-review-frames' }
        'render-pose-cycle' { Join-Path $root 'build/pose-cycle-0.1.7' }
        'render-sleep-transitions' { Join-Path $root 'build/sleep-transition-review-v3' }
        'render-awake-actions' { Join-Path $root 'build/awake-action-review' }
    }
    $process = Start-Process -FilePath $exe -ArgumentList @('--'+$command,('"'+$target+'"')) -WindowStyle Hidden -Wait -PassThru
    if($process.ExitCode -ne 0){throw "Headless check failed: $command; see $target.error.txt"}
}
& (Join-Path $PSScriptRoot 'SharedWrite-AnimatedGif.ps1') -FramesDirectory (Join-Path $root 'build/transition-review-frames') -OutputPath (Join-Path $root 'build/sit-lie-review-v1.gif') -Delays @(100,12,12,12,100,12,12,12,100)
& (Join-Path $PSScriptRoot 'SharedWrite-AnimatedGif.ps1') -FramesDirectory (Join-Path $root 'build/awake-action-review') -OutputPath (Join-Path $root 'build/awake-actions-0.1.7.gif') -Delays @(100,12,6,24,18,252,12,100)
Write-Host 'Verified: embedded artwork, geometry, 96 simulated hours, sleep/pause/drag/menu gates, shared placement and z-order policy. No UI opened or registry changes.'

& (Join-Path $PSScriptRoot 'SharedWrite-AnimatedGif.ps1') -FramesDirectory (Join-Path $root 'build/sleep-transition-review-v3') -OutputPath (Join-Path $root 'build/lay-sleep-0.1.7.gif') -Delays @(100,10,14,20,22,26,150,16,18,16,12,10,100)

$cycleDelays = @(Get-Content -LiteralPath (Join-Path $root 'build/pose-cycle-0.1.7/delays.json') -Raw | ConvertFrom-Json)
& (Join-Path $PSScriptRoot 'SharedWrite-AnimatedGif.ps1') -FramesDirectory (Join-Path $root 'build/pose-cycle-0.1.7') -OutputPath (Join-Path $root 'build/pose-cycle-0.1.7.gif') -Delays $cycleDelays

$menuPreview=Join-Path $root 'build/production-menu-0.1.7.png'
$p=Start-Process -FilePath $exe -ArgumentList @('--render-menu',('"'+$menuPreview+'"')) -WindowStyle Hidden -Wait -PassThru
if ($p.ExitCode -ne 0) { throw 'Production menu preview failed.' }
& (Join-Path $PSScriptRoot 'Test-UpdateHandoff.ps1')
