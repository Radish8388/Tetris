[Setup]
AppName=Tetris
AppVersion=1.0.0
DefaultDirName={autopf}\Radish\Tetris
DefaultGroupName=Radish
SetupIconFile=images\Tetris.ico
UninstallDisplayIcon={app}\Tetris.exe
LicenseFile=LICENSE.txt
OutputBaseFilename=TetrisSetup
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
AppPublisher=Radish
AppPublisherURL=https://radish-vert.vercel.app
AppId={{379c4d24-c9c2-473e-aa4d-c0fed4d3acb8}

[Files]
Source: "bin\Release\net10.0-windows\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Tetris"; Filename: "{app}\Tetris.exe"
Name: "{commondesktop}\Tetris"; Filename: "{app}\Tetris.exe"; Tasks: desktopicon

[Tasks]
Name: desktopicon; Description: "Create a &desktop shortcut"; GroupDescription: "Additional icons:"

[Run]
Filename: "{app}\Tetris.exe"; Description: "Launch Tetris"; Flags: nowait postinstall skipifsilent
