import { build as viteBuild } from 'vite';
import * as esbuild from 'esbuild';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const rootDir = path.resolve(__dirname, '..');
const distDir = path.resolve(rootDir, 'dist');
const distPublicDir = path.resolve(distDir, 'public');

async function runBuild() {
  console.log('====================================================');
  console.log('[BUILD] Memulai build: Kompilasi Frontend & Backend');
  console.log('====================================================\n');

  // 1. Bersihkan folder dist sebelumnya
  if (fs.existsSync(distDir)) {
    fs.rmSync(distDir, { recursive: true, force: true });
  }
  fs.mkdirSync(distPublicDir, { recursive: true });

  // 2. Build Frontend Vue 3 dengan Vite ke dalam dist/public
  console.log('[1/3] Meng-compile Frontend Vue 3 ke dist/public...');
  await viteBuild({
    root: path.resolve(rootDir, 'client'),
    base: './',
    build: {
      outDir: distPublicDir,
      emptyOutDir: true
    }
  });
  console.log('      [OK] Frontend Vue berhasil di-compile ke dist/public/\n');

  // 3. Bundle Backend Express Server ke dalam dist/index.js
  console.log('[2/3] Mem-bundle Backend Node.js Server ke dist/index.js...');
  await esbuild.build({
    entryPoints: [path.resolve(rootDir, 'server/index.js')],
    outfile: path.resolve(distDir, 'index.js'),
    bundle: true,
    platform: 'node',
    target: 'node22',
    format: 'esm',
    define: {
      '__dirname': 'import.meta.dirname',
      '__filename': 'import.meta.filename'
    },
    banner: {
      js: `import { createRequire as topLevelCreateRequire } from 'module'; const require = topLevelCreateRequire(import.meta.url);`
    },
    external: [
      'vite'
    ]
  });
  console.log('      [OK] Backend server berhasil dibundle ke dist/index.js\n');

  // 4. Salin config.js dan buat package.json mandiri di dalam dist/
  console.log('[3/3] Menyiapkan config.js & package.json di dalam dist/...');
  fs.copyFileSync(
    path.resolve(rootDir, 'config.js'),
    path.resolve(distDir, 'config.js')
  );

  const rootPkg = JSON.parse(fs.readFileSync(path.resolve(rootDir, 'package.json'), 'utf-8'));
  const distPkg = {
    name: rootPkg.name || 'nodejs-vue-restapi-server',
    version: rootPkg.version || '1.0.0',
    type: 'module',
    main: 'index.js',
    scripts: {
      start: 'node index.js'
    },
    dependencies: {
      cors: rootPkg.dependencies?.cors || '^2.8.6',
      express: rootPkg.dependencies?.express || '^5.2.1',
      'swagger-ui-express': rootPkg.dependencies?.['swagger-ui-express'] || '^5.0.1'
    }
  };

  fs.writeFileSync(
    path.resolve(distDir, 'package.json'),
    JSON.stringify(distPkg, null, 2),
    'utf-8'
  );

  // Bersihkan folder lama client/dist jika ada
  const oldClientDist = path.resolve(rootDir, 'client/dist');
  if (fs.existsSync(oldClientDist)) {
    fs.rmSync(oldClientDist, { recursive: true, force: true });
  }

  console.log('      [OK] File konfigurasi siap di dist/\n');
  console.log('====================================================');
  console.log('[SUKSES] Seluruh aplikasi berada di 1 folder dist:');
  console.log(`Folder: ${distDir}`);
  console.log('====================================================\n');
}

runBuild().catch(err => {
  console.error('❌ Build failed:', err);
  process.exit(1);
});
