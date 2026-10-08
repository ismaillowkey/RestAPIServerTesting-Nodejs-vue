@echo off
setlocal
title Build Portable Windows App (publish/portable)

:: Pindah ke direktori script
cd /d "%~dp0"

:: Baca versi dari version.conf
set "APP_VERSION=1.0.0"
if exist "%~dp0version.conf" (
    for /f "tokens=1,2 delims==" %%A in ('type "%~dp0version.conf"') do (
        if /i "%%A"=="VERSION" set "APP_VERSION=%%B"
    )
)

echo ==========================================================
echo        MEMBUAT PAKET APLIKASI WINDOWS PORTABLE
echo ==========================================================
echo Versi       : v%APP_VERSION%
echo Output      : publish\portable
echo Database    : Aman / Tidak akan tertimpa saat update
echo ==========================================================
echo.

:: Cek keberadaan Node.js
where node >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] Node.js tidak ditemukan di sistem ini!
    echo Pastikan Node.js terinstall untuk proses kompilasi.
    echo.
    pause
    exit /b 1
)

:: Tutup CommandCenter.exe jika sedang terbuka agar file tidak terkunci
tasklist /fi "imagename eq CommandCenter.exe" 2>nul | find /i "CommandCenter.exe" >nul
if %errorlevel% equ 0 (
    echo [INFO] Menutup CommandCenter.exe yang sedang berjalan agar file tidak terkunci...
    taskkill /f /im CommandCenter.exe >nul 2>nul
    timeout /t 1 /nobreak >nul
)

echo [INFO] Menjalankan proses bundle aplikasi ke folder portable...
echo [INFO] File database eksisting tidak akan ditimpa saat update.
echo.

cd /d "%~dp0src"
node scripts/package-portable.js

if %errorlevel% neq 0 (
    echo.
    echo ==========================================================
    echo [ERROR] Terjadi kesalahan saat pembuatan portable app!
    echo ==========================================================
    echo.
    pause
    exit /b 1
)

echo.
echo ==========================================================
echo [SUKSES] Paket Portable siap digunakan!
echo ==========================================================
echo Lokasi folder : "%~dp0publish\portable"
echo File eksekusi : CommandCenter.exe
echo.
echo Membuka folder output...
explorer "%~dp0publish\portable"

echo.
pause
