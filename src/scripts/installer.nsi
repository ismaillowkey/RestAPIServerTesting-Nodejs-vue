; NSIS Modern UI Installer Script
; Rest API Server - Installer Builder
!include "MUI2.nsh"
!include "FileFunc.nsh"

!ifndef VERSION
  !define VERSION "1.0.0"
!endif

!define APP_NAME "Control Center RASNodevue"
!define PUBLISHER "Ismail Lowkey"
!define EXE_NAME "CommandCenter.exe"

Name "${APP_NAME} v${VERSION}"
OutFile "..\..\publish\installer\setup_restapiservertest_nodejsvue_v${VERSION}.exe"
InstallDir "$LOCALAPPDATA\Programs\${PUBLISHER}\${APP_NAME}"
InstallDirRegKey HKCU "Software\${PUBLISHER}\${APP_NAME}" "InstallDir"
RequestExecutionLevel user

; Visual Settings
!define MUI_ICON "..\..\publish\portable\app.ico"
!define MUI_UNICON "..\..\publish\portable\app.ico"
!define MUI_ABORTWARNING

; Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

; Finish Page with option to run app
!define MUI_FINISHPAGE_RUN "$INSTDIR\${EXE_NAME}"
!define MUI_FINISHPAGE_RUN_TEXT "Jalankan ${APP_NAME} sekarang"
!insertmacro MUI_PAGE_FINISH

; Uninstaller Pages
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

; ----------------------------------------------------
; Inisialisasi: Tutup aplikasi yang sedang berjalan
; ----------------------------------------------------
Function .onInit
    nsExec::Exec 'taskkill /f /im ${EXE_NAME}'
FunctionEnd

Function un.onInit
    nsExec::Exec 'taskkill /f /im ${EXE_NAME}'
FunctionEnd

; ----------------------------------------------------
; Installer Section
; ----------------------------------------------------
Section "MainSection" SecMain
    SetOutPath "$INSTDIR"

    ; 1. Salin file aplikasi utama (selalu timpa dengan versi terbaru)
    SetOverwrite on
    File "..\..\publish\portable\CommandCenter.exe"
    File "..\..\publish\portable\run-test.bat"
    File "..\..\publish\portable\app.ico"
    File "..\..\publish\portable\version.conf"

    ; Salin engine (Node 22 standalone binary + bundle dist)
    SetOutPath "$INSTDIR\engine"
    File /r /x "*.log" "..\..\publish\portable\engine\*.*"

    ; 2. Folder Database: Buat folder data dan salin file awal jika ada (JANGAN TIMPA jika sudah ada!)
    CreateDirectory "$INSTDIR\data"
    SetOutPath "$INSTDIR\data"
    SetOverwrite off
    File /nonfatal /r /x "*.log" "..\..\publish\portable\data\*.*"
    SetOverwrite on

    ; Kembali ke direktori root instalasi
    SetOutPath "$INSTDIR"

    ; Simpan registry untuk uninstall
    WriteRegStr HKCU "Software\${PUBLISHER}\${APP_NAME}" "InstallDir" "$INSTDIR"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayName" "${APP_NAME} (${PUBLISHER})"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayVersion" "${VERSION}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "Publisher" "${PUBLISHER}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayIcon" "$INSTDIR\app.ico"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "UninstallString" "$INSTDIR\Uninstall.exe"
    WriteUninstaller "$INSTDIR\Uninstall.exe"

    ; Shortcut Start Menu: Rest API server -> nodejs vue testing
    CreateDirectory "$SMPROGRAMS\Rest API server\nodejs vue testing"
    CreateShortcut "$SMPROGRAMS\Rest API server\nodejs vue testing\Control Center RASNodevue.lnk" "$INSTDIR\${EXE_NAME}" "" "$INSTDIR\app.ico" 0
    CreateShortcut "$SMPROGRAMS\Rest API server\nodejs vue testing\Uninstall.lnk" "$INSTDIR\Uninstall.exe" "" "$INSTDIR\app.ico" 0

    ; Shortcut Desktop
    CreateShortcut "$DESKTOP\Control Center RASNodevue.lnk" "$INSTDIR\${EXE_NAME}" "" "$INSTDIR\app.ico" 0
SectionEnd

; ----------------------------------------------------
; Uninstaller Section
; (CATATAN: Database di folder $INSTDIR\data TIDAK dihapus agar data aman)
; ----------------------------------------------------
Section "Uninstall"
    ; Hapus shortcuts Start Menu
    Delete "$SMPROGRAMS\Rest API server\nodejs vue testing\Control Center RASNodevue.lnk"
    Delete "$SMPROGRAMS\Rest API server\nodejs vue testing\Command Center.lnk"
    Delete "$SMPROGRAMS\Rest API server\nodejs vue testing\Uninstall.lnk"
    RMDir "$SMPROGRAMS\Rest API server\nodejs vue testing"
    RMDir "$SMPROGRAMS\Rest API server"

    ; Hapus shortcut Desktop
    Delete "$DESKTOP\Control Center RASNodevue.lnk"
    Delete "$DESKTOP\Command Center.lnk"

    ; Hapus file aplikasi
    Delete "$INSTDIR\${EXE_NAME}"
    Delete "$INSTDIR\run-test.bat"
    Delete "$INSTDIR\app.ico"
    Delete "$INSTDIR\version.conf"
    Delete "$INSTDIR\settings.ini"
    Delete "$INSTDIR\Uninstall.exe"

    ; Hapus engine dan dist
    RMDir /r "$INSTDIR\engine"

    ; PENTING: Folder data/*.db TIDAK dihapus otomatis demi menjaga keamanan data pengguna
    ; Hanya hapus folder root jika sudah kosong
    RMDir "$INSTDIR"

    ; Bersihkan Registry
    DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"
    DeleteRegKey HKCU "Software\${PUBLISHER}\${APP_NAME}"
SectionEnd
