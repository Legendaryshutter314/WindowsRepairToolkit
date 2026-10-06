#ifndef MyAppName
  #define MyAppName "Windows Repair Toolkit"
#endif
#ifndef MyAppPublisher
  #define MyAppPublisher "JK"
#endif
#ifndef MyAppDescription
  #define MyAppDescription "Windows system health, repair, cleanup, and performance maintenance."
#endif
#ifndef MyAppCopyright
  #define MyAppCopyright "Copyright © 2026 JK"
#endif
#define MyAppExeName "WindowsRepairToolkit.exe"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\..\artifacts\publish\win-x64"
#endif
#ifndef OutputDir
  #define OutputDir "..\..\artifacts\installer"
#endif

[Setup]
AppId={{D4C89D66-826E-4B74-94CB-017583F6827D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppComments={#MyAppDescription}
DefaultDirName={autopf}\Windows Repair Toolkit
DefaultGroupName=Windows Repair Toolkit
DisableProgramGroupPage=yes
UninstallDisplayName={#MyAppName}
OutputDir={#OutputDir}
OutputBaseFilename=WindowsRepairToolkit-Setup-{#MyAppVersion}-x64
SetupIconFile=..\..\Assets\logo.ico
UninstallDisplayIcon={app}\WindowsRepairToolkit.exe
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.19041
CloseApplications=yes
RestartApplications=no
VersionInfoVersion={#MyAppVersion}.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppDescription}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
VersionInfoCopyright={#MyAppCopyright}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Windows Repair Toolkit"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Windows Repair Toolkit"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Windows Repair Toolkit"; Flags: nowait postinstall skipifsilent runasoriginaluser

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
