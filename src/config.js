/**
 * File Konfigurasi Server Aplikasi
 * Anda dapat mengubah port server, host, atau pengaturan lainnya di sini.
 */

// Cek apakah ada argumen --port=XXXX dari command line (dari C# launcher)
const cliPortArg = process.argv.find(arg => arg.startsWith('--port='));
const parsedCliPort = cliPortArg ? parseInt(cliPortArg.split('=')[1], 10) : null;

export const config = {
  // Urutan prioritas: argumen CLI > environment variable > default 3500
  port: parsedCliPort || (process.env.PORT ? parseInt(process.env.PORT, 10) : 3500),

  // Host aplikasi (0.0.0.0 agar bisa diakses dari LAN / IP luar)
  host: process.env.HOST || '0.0.0.0',

  // Nama file database SQLite
  dbFile: 'data.sqlite',

  // Environment mode ('development' | 'production')
  env: process.env.NODE_ENV || 'development'
};

export default config;
