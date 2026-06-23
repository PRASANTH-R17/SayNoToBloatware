; Inno Setup script for Say No to Bloatware (Windows installer)
;
; Build (after publishing with the WindowsPortable profile):
;   iscc /DAppVersion=1.0.0 /DSourceDir="..\..\SNB Desktop\publish\desktop\win-portable" /DOutputDir="..\..\dist" installer\windows\snb.iss
;
; Defines (overridable from the command line):
;   AppVersion - version string used in the installer and filename
;   SourceDir  - folder containing the published SNB.Desktop.exe + assets
;   OutputDir  - where the generated setup .exe is written

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#ifndef SourceDir
  #define SourceDir "..\..\SNB Desktop\publish\desktop\win-portable"
#endif

#ifndef OutputDir
  #define OutputDir "..\..\dist"
#endif

#define AppName "Say No to Bloatware"
#define AppPublisher "Prasanth R"
#define AppExeName "SNB.Desktop.exe"
#define AppUrl "https://github.com/PRASANTH-R17/SayNoToBloatware"

[Setup]
AppId={{8F2B6C4A-9D3E-4F1A-B7C8-SNB0BLOATWARE}}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}/issues
AppUpdatesURL={#AppUrl}/releases
DefaultDirName={autopf}\Say No to Bloatware
DefaultGroupName=Say No to Bloatware
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExeName}
OutputDir={#OutputDir}
OutputBaseFilename=SNB-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Recurse the entire published output (SNB.Desktop.exe + adb.exe + DLLs + Bridge/ + Default/ + Assets/Images)
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Remove anything the app generates next to itself (cache, extracted single-file bundles, etc.)
Type: filesandordirs; Name: "{app}\Cache"
