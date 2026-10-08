@echo off
setlocal EnableExtensions
title Build Windows NSIS Installer (publish/installer)

:: Pindah ke direktori root script
cd /d "%~dp0"

:: Baca versi dari version.conf
set "APP_VERSION=1.0.0"
if exist "%~dp0version.conf" (
    for /f "tokens=1,2 delims==" %%A in ('type "%~dp0version.conf"') do (
        if /i "%%A"=="VERSION" set "APP_VERSION=%%B"
    )
)

echo ==========================================================
echo        MEMBUAT WINDOWS SETUP INSTALLER (NSIS)
echo ==========================================================
echo Versi       : v%APP_VERSION%
echo Publisher   : Ismail Lowkey
echo Start Menu  : Rest API server -^> nodejs vue testing
echo Database    : Aman / Tidak akan tertimpa saat update
echo ==========================================================
echo.

:: 1. Cek Node.js
where node >nul 2>nul
if %errorlevel% neq 0 goto :err_node

:: 2. Cari compiler NSIS (makensis.exe)
set "MAKENSIS_EXE="
where makensis.exe >nul 2>nul
if %errorlevel% equ 0 (
    for /f "delims=" %%I in ('where makensis.exe') do set "MAKENSIS_EXE=%%I"
)

if not defined MAKENSIS_EXE if exist "%ProgramFiles(x86)%\NSIS\makensis.exe" set "MAKENSIS_EXE=%ProgramFiles(x86)%\NSIS\makensis.exe"
if not defined MAKENSIS_EXE if exist "%ProgramFiles%\NSIS\makensis.exe" set "MAKENSIS_EXE=%ProgramFiles%\NSIS\makensis.exe"
if not defined MAKENSIS_EXE if exist "%LocalAppData%\Programs\NSIS\makensis.exe" set "MAKENSIS_EXE=%LocalAppData%\Programs\NSIS\makensis.exe"

if not defined MAKENSIS_EXE goto :no_nsis

:: 3. Tutup CommandCenter.exe jika sedang terbuka agar file tidak terkunci (locked)
tasklist /fi "imagename eq CommandCenter.exe" 2>nul | find /i "CommandCenter.exe" >nul
if %errorlevel% equ 0 (
    echo [INFO] Menutup CommandCenter.exe yang sedang berjalan agar file tidak terkunci...
    taskkill /f /im CommandCenter.exe >nul 2>nul
    timeout /t 1 /nobreak >nul
)

:: 4. Build file portable terbaru terlebih dahulu
echo [1/2] Menyiapkan dan mem-bundle file aplikasi...
cd /d "%~dp0src"
call node scripts/package-portable.js
if %errorlevel% neq 0 goto :err_portable

:: 5. Kompilasi NSIS Installer
echo.
echo [2/2] Mengompilasi Setup Installer dengan NSIS...
echo Compiler: "%MAKENSIS_EXE%"
"%MAKENSIS_EXE%" /DVERSION=%APP_VERSION% "%~dp0src\scripts\installer.nsi"
if %errorlevel% neq 0 goto :err_nsis

echo.
echo ==========================================================
echo [SUKSES] Windows Installer NSIS berhasil dibuat!
echo ==========================================================
echo Lokasi folder : "%~dp0publish\installer"
echo File Setup    : setup_restapiservertest_nodejsvue_v%APP_VERSION%.exe
echo.
echo Membuka folder output...
explorer "%~dp0publish\installer"

echo.
pause
exit /b 0

:err_node
echo.
echo ==========================================================
echo [ERROR] Node.js tidak ditemukan di sistem ini!
echo ==========================================================
echo Pastikan Node.js sudah terinstall agar aplikasi bisa dibuild.
echo Download Node.js: https://nodejs.org
echo ==========================================================
echo.
pause
exit /b 1

:no_nsis
echo.
echo ==========================================================
echo [ERROR] NSIS (Nullsoft Scriptable Install System) belum terinstall!
echo ==========================================================
echo Untuk membuat file installer Setup.exe menggunakan NSIS, Anda
echo perlu menginstall NSIS terlebih dahulu.
echo.
echo Silakan download dan install NSIS melalui link resmi ini:
echo   --^> https://nsis.sourceforge.io/Download
echo.
echo Atau install otomatis melalui Windows Terminal / CMD:
echo   --^> winget install --id NSIS.NSIS -e
echo ==========================================================
echo.
set /p DO_INSTALL="Apakah Anda ingin menginstall NSIS sekarang via winget? (Y/T): "
if /i "%DO_INSTALL%"=="Y" (
    echo.
    echo Mengunduh dan menginstall NSIS via winget...
    winget install --id NSIS.NSIS -e --accept-source-agreements --accept-package-agreements
    if %errorlevel% equ 0 (
        echo.
        echo [INFO] NSIS berhasil diinstall! Silakan jalankan ulang file batch ini.
        echo.
    ) else (
        echo.
        echo [WARN] Gagal menginstall via winget. Silakan download manual dari https://nsis.sourceforge.io/Download
        echo.
    )
)
pause
exit /b 1

:err_portable
echo.
echo ==========================================================
echo [ERROR] Gagal mem-bundle file aplikasi di folder src!
echo ==========================================================
echo Cek pesan error di atas untuk detail penyebab kegagalan.
echo ==========================================================
echo.
pause
exit /b 1

:err_nsis
echo.
echo ==========================================================
echo [ERROR] Gagal mengompilasi installer dengan NSIS!
echo ==========================================================
echo Cek pesan error kompilasi NSIS di atas.
echo ==========================================================
echo.
pause
exit /b 1
