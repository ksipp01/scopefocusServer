;
; Script generated by the ASCOM Driver Installer Script Generator 6.2.0.0
; Generated by Kevin Sipprell on 4/25/2017 (UTC)
;
[Setup]
#define AppVer GetFileVersion("C:\Users\ksipp_000\Documents\Visual Studio 2015\Projects\scopefocusServer\scopefocusServer\bin\Debug\ASCOM.scopefocusServer.Server.exe")
#define AppNameD "ASCOM scopefocusServer Rotator Driver"
AppID={{1bac7065-ff45-4d60-9a6a-767401cf1144}
AppName={#AppNameD}
AppVerName={#AppNameD} version {#AppVer}
AppVersion={#AppVer}
AppPublisher=Kevin Sipprell <k.sipprell@mchsi.com>
AppPublisherURL=mailto:k.sipprell@mchsi.com
AppSupportURL=http://tech.groups.yahoo.com/group/ASCOM-Talk/
AppUpdatesURL=http://ascom-standards.org/
VersionInfoVersion={#AppVer}
MinVersion=0,5.0.2195sp4
DefaultDirName="{cf}\ASCOM\Rotator\scopefocus"
DisableDirPage=yes
DisableProgramGroupPage=yes
OutputDir="."
OutputBaseFilename="scopefocusServer Setup"
Compression=lzma
SolidCompression=yes
; Put there by Platform if Driver Installer Support selected
WizardImageFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\WizardImage.bmp"
LicenseFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\CreativeCommons.txt"
; {cf}\ASCOM\Uninstall\Rotator folder created by Platform, always
UninstallFilesDir="{cf}\ASCOM\Uninstall\Rotator\scopefocusServer"

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: "{cf}\ASCOM\Uninstall\Rotator\scopefocusServer"
; TODO: Add subfolders below {app} as needed (e.g. Name: "{app}\MyFolder")

[Files]
Source: "C:\Users\ksipp_000\Documents\Visual Studio 2015\Projects\scopefocusServer\scopefocusServer\bin\Debug\ASCOM.scopefocusServer.Server.exe"; DestDir: {app}; Flags: IgnoreVersion; 
; Require a read-me HTML to appear after installation, maybe driver's Help doc
Source: "C:\Users\ksipp_000\Documents\Visual Studio 2015\Projects\scopefocusServer\scopefocusServer\bin\Debug\sfServerReadme.txt"; DestDir: {app}; Flags: isreadme
; TODO: Add other files needed by your driver here (add subfolders above)
Source: "C:\Users\ksipp_000\Documents\Visual Studio 2015\Projects\scopefocusServer\scopefocusServer\bin\Debug\ASCOM.scopefocusServer.Rotator.dll"; DestDir: {app}; Flags: IgnoreVersion;  
;Source: "C:\Users\ksipp_000\Documents\Visual Studio 2015\Projects\scopefocusServer\scopefocusServer\bin\Debug\ASCOM.scopefocusServer.Server.exe.config"; DestDir: "{app}" 

;Only if COM Local Server
[Run]
Filename: "{app}\ASCOM.scopefocusServer.Server.exe"; Parameters: "/regserver"




;Only if COM Local Server
[UninstallRun]
Filename: "{app}\ASCOM.scopefocusServer.Server.exe"; Parameters: "/unregserver"



;  DCOM setup for COM local Server, needed for TheSky
[Registry]
; TODO: If needed set this value to the Rotator CLSID of your driver (mind the leading/extra '{')
#define AppClsid "{{750c8652-30c7-427a-bbf4-e36ebbccd125}"

; set the DCOM access control for TheSky on the Rotator interface
;Root: HKCR; Subkey: CLSID\{#AppClsid}; ValueType: string; ValueName: AppID; ValueData: {#AppClsid}
;Root: HKCR; Subkey: AppId\{#AppClsid}; ValueType: string; ValueData: "ASCOM scopefocusServer Rotator Driver"
;Root: HKCR; Subkey: AppId\{#AppClsid}; ValueType: string; ValueName: AppID; ValueData: {#AppClsid}
;Root: HKCR; Subkey: AppId\{#AppClsid}; ValueType: dword; ValueName: AuthenticationLevel; ValueData: 1
; set the DCOM key for the executable as a whole
Root: HKCR; Subkey: AppId\ASCOM.scopefocusServer.Server.exe; ValueType: string; ValueName: AppID; ValueData: {#AppClsid}
; CAUTION! DO NOT EDIT - DELETING ENTIRE APPID TREE WILL BREAK WINDOWS!
Root: HKCR; Subkey: AppId\{#AppClsid}; Flags: uninsdeletekey
Root: HKCR; Subkey: AppId\ASCOM.scopefocusServer.Server.exe; Flags: uninsdeletekey

[CODE]
//
// Before the installer UI appears, verify that the (prerequisite)
// ASCOM Platform 6.2 or greater is installed, including both Helper
// components. Utility is required for all types (COM and .NET)!
//
function InitializeSetup(): Boolean;
var
   U : Variant;
   H : Variant;
begin
   Result := FALSE;  // Assume failure
   // check that the DriverHelper and Utilities objects exist, report errors if they don't
   try
      H := CreateOLEObject('DriverHelper.Util');
   except
      MsgBox('The ASCOM DriverHelper object has failed to load, this indicates a serious problem with the ASCOM installation', mbInformation, MB_OK);
   end;
   try
      U := CreateOLEObject('ASCOM.Utilities.Util');
   except
      MsgBox('The ASCOM Utilities object has failed to load, this indicates that the ASCOM Platform has not been installed correctly', mbInformation, MB_OK);
   end;
   try
      if (U.IsMinimumRequiredVersion(6,2)) then	// this will work in all locales
         Result := TRUE;
   except
   end;
   if(not Result) then
      MsgBox('The ASCOM Platform 6.2 or greater is required for this driver.', mbInformation, MB_OK);
end;

// Code to enable the installer to uninstall previous versions of itself when a new version is installed
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallExe: String;
  UninstallRegistry: String;
begin
  if (CurStep = ssInstall) then // Install step has started
	begin
      // Create the correct registry location name, which is based on the AppId
      UninstallRegistry := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}' + '_is1');
      // Check whether an extry exists
      if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then
        begin // Entry exists and previous version is installed so run its uninstaller quietly after informing the user
          MsgBox('Setup will now remove the previous version.', mbInformation, MB_OK);
          Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
          sleep(1000);    //Give enough time for the install screen to be repainted before continuing
        end
  end;
end;

//
// Register and unregister the driver with the Chooser
// We already know that the Helper is available
//
procedure RegASCOM();
var
   P: Variant;
begin
   P := CreateOleObject('ASCOM.Utilities.Profile');
   P.DeviceType := 'Rotator';
   P.Register('scopefocusServer.Rotator', 'ASCOM Rotator Driver for scopefocusServer');
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
   P: Variant;
begin
   if CurUninstallStep = usUninstall then
   begin
     P := CreateOleObject('ASCOM.Utilities.Profile');
     P.DeviceType := 'Rotator';
     P.Unregister('scopefocusServer.Rotator');
  end;
end;
