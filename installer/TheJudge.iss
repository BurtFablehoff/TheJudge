#ifndef ProductionExecutable
  #define ProductionExecutable "..\test-exes\TheJudge.exe"
#endif

[Setup]
AppId={{A967887A-7911-43EA-BC40-ABF89FF3E4D5}
AppName=The Judge
AppVersion=0.1.7
AppVerName=The Judge 0.1.7
AppPublisher=BurtFablehoff
AppPublisherURL=https://github.com/BurtFablehoff/TheJudge
AppSupportURL=https://github.com/BurtFablehoff/TheJudge/issues
AppUpdatesURL=https://github.com/BurtFablehoff/TheJudge/releases/latest
DefaultDirName={localappdata}\Programs\TheJudge
DefaultGroupName=The Judge
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
DisableProgramGroupPage=yes
AppMutex=Local\TheJudgeCompanionV1
CloseApplications=yes
RestartApplications=no
UninstallDisplayName=The Judge
UninstallDisplayIcon={app}\TheJudge.exe
OutputDir=..\build\release
OutputBaseFilename=TheJudge-Setup-0.1.7
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
VersionInfoVersion=0.1.7.0

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#ProductionExecutable}"; DestDir: "{app}"; DestName: "TheJudge.exe"

[Icons]
Name: "{group}\The Judge"; Filename: "{app}\TheJudge.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\The Judge"; Filename: "{app}\TheJudge.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\TheJudge.exe"; Description: "Launch The Judge"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  StartupCommand: String;
begin
  if CurUninstallStep = usUninstall then
  begin
    if RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'TheJudge', StartupCommand) then
      if CompareText(StartupCommand, '"' + ExpandConstant('{app}\TheJudge.exe') + '"') = 0 then
        RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'TheJudge');
  end;
end;
