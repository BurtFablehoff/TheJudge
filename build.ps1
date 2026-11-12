param([switch]$Dev,[string]$ExecutableName)
if (!$ExecutableName) { $ExecutableName = if ($Dev) { 'TheJudge-Dev-0.1.7.exe' } else { 'TheJudge-0.1.7.exe' } }
$ErrorActionPreference = 'Stop'
if ([IO.Path]::GetFileName($ExecutableName) -ne $ExecutableName -or -not $ExecutableName.EndsWith('.exe')) { throw 'ExecutableName must be an EXE filename.' }
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'Windows .NET Framework C# compiler is required.' }
$output = Join-Path $PSScriptRoot 'test-exes'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$manifestPath = Join-Path $PSScriptRoot 'art/manifest.json'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$arguments = @('/nologo','/target:winexe','/platform:x64','/optimize+','/main:Judge.Program','/r:System.Drawing.dll','/r:System.Windows.Forms.dll','/r:System.Web.Extensions.dll',('/out:'+(Join-Path $output $ExecutableName)),('/win32manifest:'+(Join-Path $PSScriptRoot 'app/app.manifest')),('/resource:'+$manifestPath+',Judge.Manifest'))
foreach($entry in $manifest.frames.PSObject.Properties) {
    $path = Join-Path $PSScriptRoot ('art/'+$entry.Value.file)
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $entry.Value.sha256) { throw "Artwork hash differs: $($entry.Name)" }
    $arguments += '/resource:'+$path+',Judge.Art.'+$entry.Name
}
if ($Dev) { $arguments += '/define:JUDGE_DEV' }
$sources = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'app') -Filter '*.cs' | ForEach-Object {$_.FullName})
& $compiler @arguments @sources
if ($LASTEXITCODE -ne 0) { throw 'The Judge build failed.' }
Write-Host ('Built: '+(Join-Path $output $ExecutableName))
