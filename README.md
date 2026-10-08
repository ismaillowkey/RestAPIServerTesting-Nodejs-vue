# ⚡ Unified Node.js, Vue 3 & NeDB NoSQL Server (Windows Portable)

Aplikasi fullstack terpadu yang menggabungkan **Frontend Vue 3**, **Backend Express REST API**, dan **NeDB NoSQL Database (mirip LiteDB / MongoDB)** dalam satu port, dilengkapi dengan **Command Center Desktop (C# .NET Framework 4.7.2)** sebagai panel kontrol lokal.

Aplikasi ini **100% PORTABLE** — siap didistribusikan ke komputer Windows mana pun **tanpa perlu menginstall Node.js sama sekali**.

---

## 📁 Struktur Direktori Proyek

```text
Nodejs-vue-RestAPIServer/
│
├── 🚀 create_portableapps.bat <-- Script 1-klik membuat aplikasi Portable
├── 📦 create_installer.bat    <-- Script 1-klik membuat Setup Installer (.exe)
│
├── 📦 publish/                <-- FOLDER OUTPUT HASIL BUILD
│   ├── 📁 portable/           <-- Aplikasi Portable (Tinggal copy/kirim)
│   │   ├── 🎮 CommandCenter.exe
│   │   ├── 🎨 app.ico
│   │   ├── 📜 run-test.bat
│   │   ├── 📁 data/           <-- Database NeDB (Aman, tidak ditimpa saat update)
│   │   └── 📁 engine/
│   │       ├── node.exe       <-- Node.js standalone runtime
│   │       └── 📁 dist/       <-- Backend Express + Vue 3 bundle
│   │
│   └── 📁 installer/          <-- Setup Installer Windows
│       └── 💿 setup_restapiservertest_nodejsvue_v<version>.exe  <-- Installer resmi dengan Start Menu
│
├── 📂 src/                    <-- SOURCE CODE LENGKAP PROYEK
│   ├── 📁 client/             <-- Frontend Vue 3 + Vite
│   ├── 📁 server/             <-- Backend Express REST API + NeDB
│   ├── 📁 csharp-command-center/ <-- Source C# Command Center & Icon
│   ├── 📁 scripts/            <-- Build & installer compiler scripts
│   ├── config.js              <-- Konfigurasi port (default: 3500)
│   └── package.json           <-- Dependensi project
│
├── ⚙️ .gitignore              <-- Konfigurasi file git yang diabaikan
└── 📖 README.md               <-- Dokumentasi proyek ini
```

---

## 🔧 Prasyarat & Tool yang Diperlukan (Build Tools)

Sebelum menjalankan script pembuat aplikasi (`create_*.bat`), pastikan tool-tool berikut tersedia di komputer developer:

| No | Tool / Kebutuhan | Fungsi | Status & Cara Mendapatkan |
| :--- | :--- | :--- | :--- |
| **1** | **Node.js** (v18, v20, atau v22) | Meng-compile Frontend Vue 3 (`vite`) & bundling file server | Wajib untuk build. Unduh di [nodejs.org](https://nodejs.org) atau jalankan `winget install OpenJS.NodeJS -e` |
| **2** | **.NET Framework 4.5+ (`csc.exe`)** | Meng-compile C# Command Center (`CommandCenter.exe`) | **Sudah bawaan Windows** di `C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe` (Tidak perlu install Visual Studio) |
| **3** | **NSIS (Nullsoft Scriptable Install System)** | Mengompilasi installer `.exe` dengan Start Menu | Diperlukan untuk `create_installer.bat`. Unduh di [nsis.sourceforge.io](https://nsis.sourceforge.io/Download) atau `winget install --id NSIS.NSIS -e` |
| **4** | **PowerShell** | Menjalankan script utilitas icon Windows | **Sudah bawaan Windows** |

> 💡 **Catatan untuk End User (Pengguna Akhir):** Komputer pengguna **TIDAK PERLU** menginstall Node.js maupun NSIS sama sekali. Aplikasi sudah 100% portable dan mandiri (standalone runtime).

---

## 🛠️ Cara Membuat File Output

### 1. Membuat Versi Portable (`create_portableapps.bat`)
* Double-click file `create_portableapps.bat`.
* Hasil tersimpan di **`publish/portable/`**.
* **Keamanan Database:** File database eksisting di folder `publish/portable/data/` **tidak akan pernah ditimpa/dihapus** saat Anda melakukan update aplikasi.

### 2. Membuat Setup Installer (`create_installer.bat`)
* Double-click file `create_installer.bat`.
* Menghasilkan file installer tunggal di **`publish/installer/setup_restapiservertest_nodejsvue_v<version>.exe`**.
* **Detail Installer:**
  * **Publisher**: `Ismail Lowkey`
  * **Start Menu**: `Rest API server` ➔ `nodejs vue testing`
  * **Proteksi Database**: Saat user meng-install versi pembaruan (*update*), database lama **tidak akan tertimpa**. Saat di-*uninstall*, data user juga tidak terhapus.
  * **Otomatis Menutup Instance Lama**: Installer akan otomatis menutup aplikasi jika sedang berjalan sebelum melakukan update file.

---

## 🎮 Fitur Command Center (`CommandCenter.exe`)

Aplikasi C# Windows Forms (.NET Framework 4.7.2) yang berfungsi sebagai pusat kendali:

* **Single Instance Enforcement**:
  * Aplikasi **hanya bisa berjalan 1x**.
  * Jika pengguna mencoba membuka aplikasi lagi saat sudah berjalan, sistem akan otomatis membawa jendela yang sudah aktif ke depan dan menampilkan notifikasi ramah (tidak ada window ganda).

* **Pilihan Port Fleksibel**:
  * Default port: **3500**.
  * Dapat diubah sewaktu-waktu (misal: 3000, 8080, 5000). Nilai port otomatis tersimpan di `settings.ini`.
* **Deteksi Port Real-Time**:
  * Otomatis memeriksa apakah port sedang digunakan aplikasi lain di Windows.
* **Integrasi Windows Firewall**:
  * **Status Merah**: `⚠️ Firewall belum terdaftar, tidak bisa diakses dari luar PC ini`
  * **Status Hijau Terang**: `🛡️ Firewall sudah terdaftar, bisa diakses dari luar PC ini`
  * **Tombol `[🛡️ Add to Firewall]`**: Mendaftarkan port yang dipilih ke Windows Firewall dengan 1 klik (meminta konfirmasi Administrator UAC).
* **Kontrol Server**:
  * Tombol **▶ Start**, **⏹ Stop**, dan **🌐 Buka Browser**.
* **Live Console Output**:
  * Menampilkan log query database, rute API, dan aktivitas server secara langsung.
* **System Tray Integration**:
  * Dapat di-minimize ke background (pojok kanan bawah Taskbar) agar server tetap berjalan senyap.
* **Clean Process Exit**:
  * Saat Command Center ditutup, proses `node.exe` otomatis dihentikan bersih (tidak ada zombie process).

---

## 💻 Panduan Pengembangan (Developer Mode)

Jika Anda ingin mengubah kode atau menambah fitur di masa mendatang:

### 1. Masuk ke Folder `src`
```bash
cd src
```

### 2. Install Dependensi (jika diperlukan)
```bash
pnpm install
# atau
npm install
```

### 3. Jalankan Mode Development (Hot-Reload)
```bash
pnpm dev
# atau
npm run dev
```
Server dan Vue akan berjalan serentak di satu port pada `http://localhost:3500`.

### 4. Build & Package Manual
```bash
pnpm run package:portable
```

---

## 📚 Endpoint REST API Bawaan

| Method | Endpoint | Keterangan |
| :--- | :--- | :--- |
| `GET` | `/` | Tampilan Web Frontend Vue 3 |
| `GET` | `/api/products` | REST API Master Data Produk |
| `GET` | `/api/plan-downtimes` | REST API Planned Downtime |
| `GET` | `/api/unplan-downtimes`| REST API Unplanned Downtime |
| `GET` | `/api/docs` | Dokumentasi Interaktif Swagger / OpenAPI |
| `GET` | `/api/health` | Health check status & uptime server |
