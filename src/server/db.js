import Datastore from '@seald-io/nedb';
import path from 'node:path';
import fs from 'node:fs';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Tentukan direktori penyimpanan file database NoSQL (.db)
// Prioritaskan folder 'data' di working directory (misal: di publish/data/ saat aplikasi portable berjalan)
const dataDir = path.resolve(process.cwd(), 'data');
if (!fs.existsSync(dataDir)) {
  fs.mkdirSync(dataDir, { recursive: true });
}

// Inisialisasi koleksi datastore NeDB (mirip Collections di LiteDB/MongoDB)
export const productsDb = new Datastore({
  filename: path.join(dataDir, 'products.db'),
  autoload: true
});

export const planDowntimesDb = new Datastore({
  filename: path.join(dataDir, 'plan_downtimes.db'),
  autoload: true
});

export const unplanDowntimesDb = new Datastore({
  filename: path.join(dataDir, 'unplan_downtimes.db'),
  autoload: true
});

/**
 * Helper untuk memastikan setiap dokumen memiliki field `id` (alias dari `_id`)
 * agar kompatibel sempurna dengan Frontend Vue dan API consumer.
 */
export function formatDoc(doc) {
  if (!doc) return doc;
  if (Array.isArray(doc)) {
    return doc.map(formatDoc);
  }
  return { ...doc, id: doc._id };
}

export function initDatabase() {
  console.log(`[NeDB] Portable NoSQL Datastore initialized at: ${dataDir}`);
}

export default {
  productsDb,
  planDowntimesDb,
  unplanDowntimesDb,
  formatDoc,
  initDatabase
};
