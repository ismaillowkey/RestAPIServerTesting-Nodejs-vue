import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { execSync } from 'node:child_process';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const rootDir = path.resolve(__dirname, '..');
const distDir = path.resolve(rootDir, 'dist');

// Folder output: publish/portable
const publishDir = path.resolve(rootDir, '../publish');
const portableDir = path.resolve(publishDir, 'portable');
const engineDir = path.resolve(portableDir, 'engine');
const engineDistDir = path.resolve(engineDir, 'dist');
const dataDir = path.resolve(portableDir, 'data');

// Baca file version.conf dari root
const versionConfPath = path.resolve(rootDir, '../version.conf');
let appVersion = '1.0.0';
if (fs.existsSync(versionConfPath)) {
  const content = fs.readFileSync(versionConfPath, 'utf-8');
  for (const line of content.split('\n')) {
    const trimmed = line.trim();
    if (trimmed.startsWith('VERSION=')) {
      appVersion = trimmed.split('=')[1].trim();
      break;
    }
  }
}

async function packagePortable() {
  console.log('====================================================');
  console.log(`[PORTABLE] Membuat Paket Windows Portable v${appVersion}`);
  console.log('====================================================\n');

  // 1. Jalankan build utama terlebih dahulu
  console.log('[1/4] Menjalankan build frontend & backend...');
  execSync('node scripts/build.js', { cwd: rootDir, stdio: 'inherit' });

  // 2. Siapkan folder publish/portable tanpa menghapus database
  console.log('\n[2/4] Menyiapkan struktur folder publish/portable/engine...');
  fs.mkdirSync(portableDir, { recursive: true });

  // Salin version.conf ke publish/portable
  if (fs.existsSync(versionConfPath)) {
    fs.copyFileSync(versionConfPath, path.resolve(portableDir, 'version.conf'));
  }

  // PROTEKSI DATABASE: Bersihkan hanya folder engine, jangan sentuh folder data!
  if (fs.existsSync(engineDir)) {
    fs.rmSync(engineDir, { recursive: true, force: true });
  }
  fs.mkdirSync(engineDistDir, { recursive: true });

  // Pastikan folder data ada dan data .db lama tidak tertimpa
  fs.mkdirSync(dataDir, { recursive: true });
  const existingDbs = fs.readdirSync(dataDir).filter(f => f.endsWith('.db'));
  if (existingDbs.length > 0) {
    console.log(`      [DATABASE] Ditemukan ${existingDbs.length} file database eksisting. File database DIPERTAHANKAN (tidak ditimpa).`);
  } else {
    console.log('      [DATABASE] Database baru akan diinisialisasi otomatis saat pertama kali aplikasi dijalankan.');
  }

  // 3. Salin node.exe dari sistem lokal (Node v22 yang sedang aktif)
  console.log('[3/4] Menyalin node.exe portable...');
  const currentNodeExe = process.execPath;
  const targetNodeExe = path.resolve(engineDir, 'node.exe');
  fs.copyFileSync(currentNodeExe, targetNodeExe);
  console.log(`      [OK] node.exe disalin dari: ${currentNodeExe}`);

  // 4. Salin isi folder dist ke engine/dist
  console.log('[4/4] Menyalin file dist aplikasi...');
  fs.cpSync(distDir, engineDistDir, { recursive: true, dereference: true });

  // 5. Compile C# Command Center (.NET Framework 4.x / 4.7.2)
  console.log('[5/5] Meng-compile C# Command Center dengan Icon...');
  const csharpProgram = path.resolve(rootDir, 'csharp-command-center/Program.cs');
  const iconPath = path.resolve(rootDir, 'csharp-command-center/app.ico');
  const targetExe = path.resolve(portableDir, 'CommandCenter.exe');
  const targetIcon = path.resolve(portableDir, 'app.ico');
  const cscPath = 'C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\csc.exe';

  // Salin icon ke publish/portable
  if (fs.existsSync(iconPath)) {
    fs.copyFileSync(iconPath, targetIcon);
  }

  if (fs.existsSync(csharpProgram) && fs.existsSync(cscPath)) {
    try {
      const iconArg = fs.existsSync(iconPath) ? ` /win32icon:"${iconPath}"` : '';
      execSync(`"${cscPath}" /target:winexe /out:"${targetExe}"${iconArg} "${csharpProgram}"`, { stdio: 'pipe' });
      console.log('      [OK] CommandCenter.exe berhasil dikompilasi ke publish/portable/');
    } catch (err) {
      console.warn('      [WARN] Gagal meng-compile CommandCenter.exe dengan csc:', err.message);
    }
  }

  // Buat file test runner sederhana run-test.bat
  const testBatContent = `@echo off
echo ==============================================
echo Menjalankan Node.js Engine Portable (Port 3500)
echo ==============================================
start http://localhost:3500
"engine\\node.exe" "engine\\dist\\index.js" --port=3500
pause
`;
  fs.writeFileSync(path.resolve(portableDir, 'run-test.bat'), testBatContent, 'utf-8');

  console.log('\n====================================================');
  console.log('[SUKSES] PAKET PORTABLE SIAP!');
  console.log(`Lokasi Output: ${portableDir}`);
  console.log('   ├── CommandCenter.exe   (GUI Control Panel C# .NET)');
  console.log('   ├── run-test.bat        (Batch file runner alternatif)');
  console.log('   ├── data/               (Penyimpanan Database NoSQL NeDB)');
  console.log('   └── engine/');
  console.log('       ├── node.exe        (Node 22 standalone binary)');
  console.log('       └── dist/           (Aplikasi Vue + Express + NeDB)');
  console.log('====================================================\n');
}

packagePortable().catch(err => {
  console.error('[ERROR] Gagal membuat paket portable:', err);
  process.exit(1);
});
