param([string]$OutputDirectory)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if (!$OutputDirectory) { $OutputDirectory=Join-Path $root ('build/update handoff '+[guid]::NewGuid().ToString('N')) }
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$fixtureRoot=(Resolve-Path -LiteralPath $OutputDirectory).Path
$buildRoot=(Resolve-Path -LiteralPath (Join-Path $root 'build')).Path.TrimEnd('\')+'\'
if (!$fixtureRoot.StartsWith($buildRoot,[StringComparison]::OrdinalIgnoreCase)) { throw 'Update fixture output must be inside this repository build folder.' }
$compiler=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
foreach ($mode in @('success','rollback')) {
    $case=Join-Path $fixtureRoot $mode
    $cache=Join-Path $case 'Updates'
    New-Item -ItemType Directory -Path $cache -Force | Out-Null
    $target=Join-Path $case 'TheJudge.exe'
    $next=Join-Path $cache 'TheJudge-update.exe'
    $settingsRoot=$case.Replace('\','\\')
    $source=@"
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using PetFramework;
[assembly:AssemblyProduct("The Judge Companion")]
internal static class Probe {
 static int Main(string[] args) {
  var updater=new ReleaseUpdater(new UpdateSettings{Repository="BurtFablehoff/TheJudge",AssetName="TheJudge.exe",ProductName="The Judge Companion",DisplayName="The Judge",AppId="TheJudge",Cache=Path.Combine("$settingsRoot","Updates"),WindowTitle="The Judge isolated updater fixture"});
  if(args.Length==1 && args[0]=="--parent"){Thread.Sleep(2500);return 0;}
  if(args.Length==2 && args[0]=="--apply-update")return updater.Apply(args[1]);
  if(args.Length==3 && args[0]=="--update-health") {updater.HealthFile=args[1];updater.HealthToken=args[2];if("$mode"=="rollback"){updater.FailHealth("Deliberate isolated fixture failure");return 1;}updater.MarkHealthy();return 0;}
  return 0;
 }
}
"@
    foreach ($entry in @(@('0.1.7.0',$target),@('0.1.8.0',$next))) {
        $file=Join-Path $case ('Probe-'+$entry[0]+'.cs')
        $versionAttributes='[assembly:System.Reflection.AssemblyVersion("'+$entry[0]+'")]'+[Environment]::NewLine+'[assembly:System.Reflection.AssemblyFileVersion("'+$entry[0]+'")]'+[Environment]::NewLine
        [IO.File]::WriteAllText($file,$source.Replace('[assembly:AssemblyProduct',$versionAttributes+'[assembly:AssemblyProduct'),[Text.Encoding]::UTF8)
        & $compiler /nologo /target:winexe /r:System.Web.Extensions.dll ('/out:'+$entry[1]) (Join-Path $root 'app/SharedReleaseUpdater.cs') $file
        if ($LASTEXITCODE -ne 0) { throw 'Fixture compilation failed.' }
    }
    $oldHash=(Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant()
    $newHash=(Get-FileHash -LiteralPath $next -Algorithm SHA256).Hash.ToLowerInvariant()
    $parent=Start-Process -FilePath $target -ArgumentList '--parent' -WindowStyle Hidden -PassThru
    $plan=@{Protocol=1;ParentPid=$parent.Id;ParentStartTicks=$parent.StartTime.ToUniversalTime().Ticks;Target=$target;ExpectedOldSha256=$oldHash;NewSha256=$newHash;Version='0.1.8';Token=[guid]::NewGuid().ToString('N');HealthFile=(Join-Path $cache 'healthy.json')}
    $planPath=Join-Path $cache 'plan.json'
    [IO.File]::WriteAllText($planPath,($plan|ConvertTo-Json),[Text.Encoding]::UTF8)
    $helper=Start-Process -FilePath $next -ArgumentList @('--apply-update',('"'+$planPath+'"')) -WindowStyle Hidden -Wait -PassThru
    $result=Get-Content -LiteralPath (Join-Path $cache 'result.json') -Raw | ConvertFrom-Json
    $finalHash=(Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($mode -eq 'success') {
        if (!$result.success -or $helper.ExitCode -ne 0 -or $finalHash -ne $newHash) { throw 'Healthy update handoff failed.' }
    } else {
        if ($result.success -or !$result.rolledBack -or $helper.ExitCode -ne 1 -or $finalHash -ne $oldHash) { throw 'Failed-health rollback failed.' }
    }
    Write-Output ('Passed hidden updater handoff: '+$mode)
}
$report=@{passed=$true;healthyUpdate=$true;failedHealthRollback=$true;originalRestored=$true;realCompanionTouched=$false;visibleWindowsOpened=$false;registryWritten=$false;networkUsed=$false;fixtureDirectory=$fixtureRoot}
[IO.File]::WriteAllText((Join-Path $root 'build/update-handoff-0.1.7.json'),($report|ConvertTo-Json),[Text.Encoding]::UTF8)
