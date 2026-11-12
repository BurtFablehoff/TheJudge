param([string]$CompilerPath,[string]$ProductionExecutable)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if (!$CompilerPath) {
    $command=Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command) { $CompilerPath=$command.Source }
    else { foreach ($version in @('7','6')) { $candidate=Join-Path ${env:ProgramFiles(x86)} ('Inno Setup '+$version+'/ISCC.exe'); if (Test-Path -LiteralPath $candidate) { $CompilerPath=$candidate; break } } }
}
if (!$CompilerPath -or !(Test-Path -LiteralPath $CompilerPath)) { throw 'Inno Setup is required. Supply -CompilerPath to an existing ISCC.exe; no installer/compiler is downloaded automatically.' }
if (!$ProductionExecutable) {
    & (Join-Path $root 'build.ps1') -ExecutableName 'TheJudge.exe'
    $ProductionExecutable=Join-Path $root 'test-exes/TheJudge.exe'
}
$ProductionExecutable=(Resolve-Path -LiteralPath $ProductionExecutable).Path
$info=[Diagnostics.FileVersionInfo]::GetVersionInfo($ProductionExecutable)
if ($info.ProductName -ne 'The Judge Companion' -or $info.FileVersion -ne '0.1.7.0') { throw 'Installer requires the 0.1.7 production EXE, never a developer or another pet build.' }
& $CompilerPath ('/DProductionExecutable='+$ProductionExecutable) (Join-Path $root 'installer/TheJudge.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
Write-Host ('Built installer: '+(Join-Path $root 'build/release/TheJudge-Setup-0.1.7.exe'))
Write-Host 'The installer was not run.'
