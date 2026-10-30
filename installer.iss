; xRAR Installer Script — Inno Setup 6
; Build: iscc installer.iss
; Output: xRAR-Setup-1.0.0.exe

#define MyAppName "xRAR"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "PIXWAR"
#define MyAppURL "https://github.com/PIXWAR/xRAR"
#define MyAppExeName "xRAR.exe"

[Setup]
AppId={{7A3B8C2D-4E5F-4A6B-8C9D-1E2F3A4B5C6D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir={#SourcePath}\dist
OutputBaseFilename=xRAR-Setup-{#MyAppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} {#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "fileassoc"; Description: "Associate xRAR with archive files (.rar, .zip, .7z, etc.)"; GroupDescription: "File Associations:"

[Files]
Source: "src\xRAR\bin\Release\net7.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; --- File Associations (only if user selects the task) ---
; Extensions extracted from WinRAR 7.13 binary (Setup\.* registry paths)
; .rar
Root: HKA; Subkey: ".rar"; ValueType: string; ValueName: ""; ValueData: "xRAR.rar"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.rar"; ValueType: string; ValueName: ""; ValueData: "WinRAR Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.rar"; ValueType: string; ValueName: "AppUserModelID"; ValueData: "xRAR"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.rar\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.rar\shell\open"; ValueType: string; ValueName: ""; ValueData: "Open with xRAR"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.rar\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .zip
Root: HKA; Subkey: ".zip"; ValueType: string; ValueName: ""; ValueData: "xRAR.zip"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.zip"; ValueType: string; ValueName: ""; ValueData: "ZIP Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.zip\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.zip\shell\open"; ValueType: string; ValueName: ""; ValueData: "Open with xRAR"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.zip\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .zipx
Root: HKA; Subkey: ".zipx"; ValueType: string; ValueName: ""; ValueData: "xRAR.zipx"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.zipx"; ValueType: string; ValueName: ""; ValueData: "ZIPX Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.zipx\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.zipx\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .7z
Root: HKA; Subkey: ".7z"; ValueType: string; ValueName: ""; ValueData: "xRAR.7z"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.7z"; ValueType: string; ValueName: ""; ValueData: "7z Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.7z\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.7z\shell\open"; ValueType: string; ValueName: ""; ValueData: "Open with xRAR"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.7z\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .tar
Root: HKA; Subkey: ".tar"; ValueType: string; ValueName: ""; ValueData: "xRAR.tar"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.tar"; ValueType: string; ValueName: ""; ValueData: "TAR Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.tar\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.tar\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .gz
Root: HKA; Subkey: ".gz"; ValueType: string; ValueName: ""; ValueData: "xRAR.gz"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.gz"; ValueType: string; ValueName: ""; ValueData: "GZ Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.gz\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.gz\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .bz2
Root: HKA; Subkey: ".bz2"; ValueType: string; ValueName: ""; ValueData: "xRAR.bz2"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.bz2"; ValueType: string; ValueName: ""; ValueData: "BZIP2 Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.bz2\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.bz2\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .xz
Root: HKA; Subkey: ".xz"; ValueType: string; ValueName: ""; ValueData: "xRAR.xz"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.xz"; ValueType: string; ValueName: ""; ValueData: "XZ Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.xz\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.xz\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .cab
Root: HKA; Subkey: ".cab"; ValueType: string; ValueName: ""; ValueData: "xRAR.cab"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.cab"; ValueType: string; ValueName: ""; ValueData: "CAB Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.cab\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.cab\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .arj
Root: HKA; Subkey: ".arj"; ValueType: string; ValueName: ""; ValueData: "xRAR.arj"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.arj"; ValueType: string; ValueName: ""; ValueData: "ARJ Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.arj\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.arj\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .lzh
Root: HKA; Subkey: ".lzh"; ValueType: string; ValueName: ""; ValueData: "xRAR.lzh"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.lzh"; ValueType: string; ValueName: ""; ValueData: "LZH Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.lzh\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.lzh\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .lz
Root: HKA; Subkey: ".lz"; ValueType: string; ValueName: ""; ValueData: "xRAR.lz"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.lz"; ValueType: string; ValueName: ""; ValueData: "LZ Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.lz\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.lz\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .z
Root: HKA; Subkey: ".z"; ValueType: string; ValueName: ""; ValueData: "xRAR.z"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.z"; ValueType: string; ValueName: ""; ValueData: "Z Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.z\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.z\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .zst
Root: HKA; Subkey: ".zst"; ValueType: string; ValueName: ""; ValueData: "xRAR.zst"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.zst"; ValueType: string; ValueName: ""; ValueData: "Zstandard Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.zst\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.zst\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .uue
Root: HKA; Subkey: ".uue"; ValueType: string; ValueName: ""; ValueData: "xRAR.uue"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.uue"; ValueType: string; ValueName: ""; ValueData: "UUEncoded Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.uue\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.uue\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .jar
Root: HKA; Subkey: ".jar"; ValueType: string; ValueName: ""; ValueData: "xRAR.jar"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.jar"; ValueType: string; ValueName: ""; ValueData: "Java Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.jar\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.jar\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .001
Root: HKA; Subkey: ".001"; ValueType: string; ValueName: ""; ValueData: "xRAR.001"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.001"; ValueType: string; ValueName: ""; ValueData: "Split Archive"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.001\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.001\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .iso
Root: HKA; Subkey: ".iso"; ValueType: string; ValueName: ""; ValueData: "xRAR.iso"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.iso"; ValueType: string; ValueName: ""; ValueData: "ISO Image"; Tasks: fileassoc; Flags: uninsdeletekey
Root: HKA; Subkey: "xRAR.iso\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"; Tasks: fileassoc
Root: HKA; Subkey: "xRAR.iso\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; .tgz .tbz2 .txz .taz .tbz .tzst (compound extensions)
Root: HKA; Subkey: ".tgz"; ValueType: string; ValueName: ""; ValueData: "xRAR.tgz"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.tgz\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
Root: HKA; Subkey: ".tbz2"; ValueType: string; ValueName: ""; ValueData: "xRAR.tbz2"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.tbz2\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
Root: HKA; Subkey: ".txz"; ValueType: string; ValueName: ""; ValueData: "xRAR.txz"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.txz\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
Root: HKA; Subkey: ".taz"; ValueType: string; ValueName: ""; ValueData: "xRAR.taz"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.taz\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
Root: HKA; Subkey: ".tbz"; ValueType: string; ValueName: ""; ValueData: "xRAR.tbz"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.tbz\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc
Root: HKA; Subkey: ".tzst"; ValueType: string; ValueName: ""; ValueData: "xRAR.tzst"; Tasks: fileassoc; Flags: uninsdeletevalue
Root: HKA; Subkey: "xRAR.tzst\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

; --- "Open with xRAR" context menu on all archive files (from WinRAR OpenWithProgids pattern) ---
Root: HKA; Subkey: ".rar\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.rar"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".zip\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.zip"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".zipx\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.zipx"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".7z\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.7z"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".tar\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.tar"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".gz\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.gz"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".bz2\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.bz2"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".xz\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.xz"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".cab\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.cab"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".arj\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.arj"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".lzh\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.lzh"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".lz\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.lz"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".z\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.z"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".zst\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.zst"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".uue\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.uue"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".jar\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.jar"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".001\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.001"; ValueData: "00"; Tasks: fileassoc
Root: HKA; Subkey: ".iso\OpenWithProgids"; ValueType: binary; ValueName: "xRAR.iso"; ValueData: "00"; Tasks: fileassoc

; --- Right-click context menu on folders (from WinRAR Directory\shell pattern) ---
Root: HKA; Subkey: "Directory\shell\xRAR"; ValueType: string; ValueName: ""; ValueData: "Open with xRAR"; Tasks: fileassoc
Root: HKA; Subkey: "Directory\shell\xRAR"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppExeName}"",0"; Tasks: fileassoc
Root: HKA; Subkey: "Directory\shell\xRAR\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; Tasks: fileassoc

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
// Refresh shell icons after install/uninstall
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Notify Windows of association changes
  end;
end;
