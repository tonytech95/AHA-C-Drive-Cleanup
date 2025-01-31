; Define the name of the installer
OutFile "CDriveCleanupInstaller.exe"

; Define the installation directory
InstallDir "C:\ProgramData\C Drive Cleanup"

; Add version information
VIProductVersion "1.0.0.0"
VIAddVersionKey /LANG=1033 "ProductName" "C Drive Cleanup"
VIAddVersionKey /LANG=1033 "FileDescription" "Installer for C Drive Cleanup"
VIAddVersionKey /LANG=1033 "FileVersion" "1.0.0.0"
VIAddVersionKey /LANG=1033 "CompanyName" "Techordia"
VIAddVersionKey /LANG=1033 "LegalCopyright" "© 2025 Techordia. All rights reserved."

; Silent installation
SilentInstall silent

; Define the sections
Section "MainSection"

    ; Create the installation directory if it doesn't exist
    CreateDirectory "$INSTDIR"

    ; Copy files to the installation directory
    SetOutPath "$INSTDIR"
    File /r "C:\Users\tony.yu\OneDrive - Techordia\Documents\Projects\AHA C Drive Cleanup\bin\Release\net8.0-windows\win-x64\publish\*.*"

    SetOutPath "$INSTDIR\Misc Files"
    File "C:\Users\tony.yu\OneDrive - Techordia\Documents\Projects\AHA C Drive Cleanup\Misc Files\CreateUserTask.ps1"
    File "C:\Users\tony.yu\OneDrive - Techordia\Documents\Projects\AHA C Drive Cleanup\Misc Files\TriggerUserTask.ps1"

    ; Define uninstaller name
    WriteUninstaller "$INSTDIR\uninstaller.exe"
SectionEnd

; Define the uninstaller
Section "Uninstall"

    ; Remove the installation directory and all its contents
    RMDir /r "$INSTDIR"

SectionEnd