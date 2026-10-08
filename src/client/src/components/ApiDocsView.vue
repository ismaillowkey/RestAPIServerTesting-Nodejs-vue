<template>
  <div class="panel-card">
    <div class="panel-header">
      <div class="panel-title-area">
        <h2>📖 Dokumentasi REST API</h2>
        <p>Spesifikasi teknis endpoint CRUD, parameter query pagination opsional, dan live request explorer</p>
      </div>

      <div style="display: flex; gap: 0.75rem; align-items: center;">
        <a
          href="/api/docs"
          target="_blank"
          class="btn btn-primary"
          style="text-decoration: none;"
          id="btn-open-swagger"
        >
          <span>🚀</span>
          <span>Buka Swagger UI (/api/docs)</span>
        </a>
        <a
          href="/api/openapi.json"
          target="_blank"
          class="btn btn-outline"
          style="text-decoration: none;"
        >
          <span>📄</span>
          <span>OpenAPI JSON</span>
        </a>
      </div>
    </div>

    <!-- Overview Banner -->
    <div style="padding: 1.5rem 1.75rem; background: #f8fafc; border-bottom: 1px solid var(--border-color);">
      <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1rem;">
        <div style="background: #fff; padding: 1rem; border-radius: var(--radius-md); border: 1px solid var(--border-color);">
          <div style="font-weight: 700; font-size: 0.9rem; color: var(--primary); margin-bottom: 0.35rem;">
            📌 Single-Port REST Server
          </div>
          <div style="font-size: 0.825rem; color: var(--text-muted);">
            REST API & Frontend Vue berjalan di port yang sama (default: <code>http://localhost:3000</code>) tanpa CORS issue. Port dapat diatur di file <code>config.js</code>.
          </div>
        </div>

        <div style="background: #fff; padding: 1rem; border-radius: var(--radius-md); border: 1px solid var(--border-color);">
          <div style="font-weight: 700; font-size: 0.9rem; color: var(--success-text); margin-bottom: 0.35rem;">
            ⚡ Query Pagination (Opsional)
          </div>
          <div style="font-size: 0.825rem; color: var(--text-muted);">
            Endpoint Read mendukung <code>?page=1&limit=10</code>. Jika query page tidak disertakan, API mengembalikan semua data dalam array data.
          </div>
        </div>
      </div>
    </div>

    <!-- Endpoints List -->
    <div style="padding: 1.5rem 1.75rem;">
      <h3 style="font-size: 1.1rem; font-weight: 700; margin-bottom: 1rem; color: var(--text-main);">
        Daftar Endpoints
      </h3>

      <div style="display: flex; flex-direction: column; gap: 1rem;">
        <div
          v-for="(ep, idx) in endpoints"
          :key="idx"
          style="border: 1px solid var(--border-color); border-radius: var(--radius-md); overflow: hidden; background: #fff;"
        >
          <!-- Endpoint Header Bar -->
          <div
            style="padding: 0.85rem 1.25rem; display: flex; align-items: center; justify-content: space-between; cursor: pointer; user-select: none; background: #fafbfc;"
            :style="{ borderLeft: `5px solid ${getMethodColor(ep.method)}` }"
            @click="toggleExpand(idx)"
          >
            <div style="display: flex; align-items: center; gap: 0.85rem; flex-wrap: wrap;">
              <span
                :style="{
                  backgroundColor: getMethodBg(ep.method),
                  color: getMethodColor(ep.method),
                  borderColor: getMethodBorder(ep.method)
                }"
                class="badge"
                style="font-family: monospace; font-size: 0.8rem; font-weight: 700; min-width: 65px; justify-content: center;"
              >
                {{ ep.method }}
              </span>
              <span style="font-family: monospace; font-weight: 600; font-size: 0.925rem; color: var(--text-main);">
                {{ ep.path }}
              </span>
              <span style="color: var(--text-muted); font-size: 0.85rem;">
                — {{ ep.summary }}
              </span>
            </div>

            <div style="display: flex; align-items: center; gap: 0.75rem;">
              <span style="font-size: 0.75rem; color: var(--text-subtle); background: var(--bg-muted); padding: 0.2rem 0.5rem; border-radius: 4px;">
                {{ ep.tag }}
              </span>
              <span style="font-size: 0.85rem; color: var(--text-subtle);">
                {{ expandedIndex === idx ? '▲' : '▼' }}
              </span>
            </div>
          </div>

          <!-- Expanded Endpoint Details & Live Tester -->
          <div v-if="expandedIndex === idx" style="padding: 1.25rem; border-top: 1px solid var(--border-color); background: #ffffff;">
            <p style="font-size: 0.875rem; color: var(--text-muted); margin-bottom: 1rem;">
              {{ ep.description }}
            </p>

            <!-- Query Parameters (if any) -->
            <div v-if="ep.parameters && ep.parameters.length" style="margin-bottom: 1.25rem;">
              <div style="font-weight: 700; font-size: 0.825rem; color: var(--text-main); margin-bottom: 0.5rem; text-transform: uppercase; letter-spacing: 0.05em;">
                Query Parameters (Opsional):
              </div>
              <div class="table-responsive">
                <table class="data-table" style="font-size: 0.8rem;">
                  <thead>
                    <tr>
                      <th>Nama Parameter</th>
                      <th>Tipe</th>
                      <th>Keterangan</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="p in ep.parameters" :key="p.name">
                      <td style="font-family: monospace; font-weight: 600; color: var(--primary);">{{ p.name }}</td>
                      <td>{{ p.type }}</td>
                      <td>{{ p.desc }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Body Payload Schema (for POST / PUT) -->
            <div v-if="ep.bodySchema" style="margin-bottom: 1.25rem;">
              <div style="font-weight: 700; font-size: 0.825rem; color: var(--text-main); margin-bottom: 0.5rem; text-transform: uppercase; letter-spacing: 0.05em;">
                Request Body (JSON):
              </div>
              <pre style="background: #f1f5f9; color: #0f172a; padding: 0.75rem 1rem; border-radius: var(--radius-md); font-size: 0.8rem; overflow-x: auto; border: 1px solid var(--border-color);"><code>{{ JSON.stringify(ep.bodySchema, null, 2) }}</code></pre>
            </div>

            <!-- Live Test / Execute Bar -->
            <div style="margin-top: 1rem; padding-top: 1rem; border-top: 1px dashed var(--border-color);">
              <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.75rem;">
                <span style="font-size: 0.85rem; font-weight: 700; color: var(--text-main);">
                  🧪 Live API Tester
                </span>
                <button
                  type="button"
                  class="btn btn-primary btn-sm"
                  :disabled="ep.loading"
                  @click="testEndpoint(ep)"
                  :id="`btn-test-${ep.id}`"
                >
                  {{ ep.loading ? 'Mengirim...' : 'Kirim Request (Execute)' }}
                </button>
              </div>

              <!-- Live Test Result -->
              <div v-if="ep.response" style="margin-top: 0.75rem;">
                <div style="display: flex; align-items: center; gap: 0.5rem; font-size: 0.8rem; margin-bottom: 0.35rem;">
                  <span>Status:</span>
                  <span class="badge" :class="ep.responseStatus < 300 ? 'badge-success' : 'badge-danger'">
                    {{ ep.responseStatus }}
                  </span>
                  <span style="color: var(--text-subtle); margin-left: auto;">Waktu: {{ ep.responseTime }}ms</span>
                </div>
                <pre style="background: #0f172a; color: #38bdf8; padding: 0.85rem 1rem; border-radius: var(--radius-md); font-size: 0.785rem; max-height: 240px; overflow-y: auto; line-height: 1.4;"><code>{{ JSON.stringify(ep.response, null, 2) }}</code></pre>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const expandedIndex = ref(0);

const endpoints = ref([
  {
    id: 'get-products',
    tag: 'Product',
    method: 'GET',
    path: '/api/products',
    summary: 'Daftar produk dengan pagination opsional',
    description: 'Mengambil daftar produk dari database SQLite. Mendukung pagination opsional melalui query page dan limit.',
    parameters: [
      { name: 'page', type: 'integer (opsional)', desc: 'Nomor halaman (contoh: 1). Jika dikosongkan, API akan mengembalikan seluruh data produk.' },
      { name: 'limit', type: 'integer (opsional)', desc: 'Batas jumlah item per halaman (default: 10).' },
      { name: 'search', type: 'string (opsional)', desc: 'Filter pencarian nama produk.' },
      { name: 'isavailable', type: '0 atau 1 (opsional)', desc: 'Filter ketersediaan produk (1: Tersedia, 0: Habis).' }
    ],
    testUrl: '/api/products?page=1&limit=5',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  },
  {
    id: 'post-product',
    tag: 'Product',
    method: 'POST',
    path: '/api/products',
    summary: 'Tambah produk baru (Create)',
    description: 'Menambahkan data produk baru ke dalam tabel products.',
    bodySchema: {
      productname: 'Mouse Wireless Gaming Pro',
      price: 350000,
      stok: 15,
      isavailable: 1
    },
    testUrl: '/api/products',
    testMethod: 'POST',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  },
  {
    id: 'get-product-id',
    tag: 'Product',
    method: 'GET',
    path: '/api/products/:id',
    summary: 'Detail satu produk',
    description: 'Mengambil detail satu produk berdasarkan ID.',
    testUrl: '/api/products/1',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  },
  {
    id: 'put-product',
    tag: 'Product',
    method: 'PUT',
    path: '/api/products/:id',
    summary: 'Perbarui produk (Update)',
    description: 'Mengupdate nama produk, harga, stok, atau status ketersediaan.',
    bodySchema: {
      productname: 'Mechanical Keyboard RGB (Updated)',
      price: 890000,
      stok: 30,
      isavailable: 1
    },
    testUrl: '/api/products/1',
    testMethod: 'PUT',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  },
  {
    id: 'delete-product',
    tag: 'Product',
    method: 'DELETE',
    path: '/api/products/:id',
    summary: 'Hapus produk (Delete)',
    description: 'Menghapus data produk secara permanen dari database SQLite.',
    testUrl: '/api/products/9999',
    testMethod: 'DELETE',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  },

  // Plan Downtimes
  {
    id: 'get-plan',
    tag: 'Plan Downtime',
    method: 'GET',
    path: '/api/plan-downtimes',
    summary: 'Daftar plan downtime dengan pagination opsional',
    description: 'Mengambil daftar downtime terencana. Mendukung query page, limit, search, dan department.',
    parameters: [
      { name: 'page', type: 'integer (opsional)', desc: 'Nomor halaman (contoh: 1).' },
      { name: 'limit', type: 'integer (opsional)', desc: 'Batas baris per halaman.' },
      { name: 'search', type: 'string (opsional)', desc: 'Pencarian kode downtime, deskripsi, atau departemen.' },
      { name: 'department', type: 'string (opsional)', desc: 'Filter berdasarkan departemen (Maintenance, Production, dll).' }
    ],
    testUrl: '/api/plan-downtimes?page=1&limit=5',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  },
  {
    id: 'post-plan',
    tag: 'Plan Downtime',
    method: 'POST',
    path: '/api/plan-downtimes',
    summary: 'Tambah plan downtime baru (Create)',
    description: 'Menyimpan jadwal plan downtime baru.',
    bodySchema: {
      department: 'Maintenance',
      downtime_code: 'PM-202',
      description: 'Pembersihan filter oli hidrolik rutin'
    },
    testUrl: '/api/plan-downtimes',
    testMethod: 'POST',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  },

  // Unplan Downtimes
  {
    id: 'get-unplan',
    tag: 'Unplan Downtime',
    method: 'GET',
    path: '/api/unplan-downtimes',
    summary: 'Daftar unplan downtime dengan pagination opsional',
    description: 'Mengambil daftar insiden downtime tak terencana dengan filter dan pagination opsional.',
    parameters: [
      { name: 'page', type: 'integer (opsional)', desc: 'Nomor halaman (contoh: 1).' },
      { name: 'limit', type: 'integer (opsional)', desc: 'Batas baris per halaman.' },
      { name: 'search', type: 'string (opsional)', desc: 'Pencarian kata kunci deskripsi/kode/dept.' },
      { name: 'department', type: 'string (opsional)', desc: 'Filter departemen terkait.' }
    ],
    testUrl: '/api/unplan-downtimes?page=1&limit=5',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  },
  {
    id: 'post-unplan',
    tag: 'Unplan Downtime',
    method: 'POST',
    path: '/api/unplan-downtimes',
    summary: 'Tambah unplan downtime baru (Create)',
    description: 'Mencatat breakdown mesin atau insiden tak terduga.',
    bodySchema: {
      department: 'Electrical',
      downtime_code: 'ERR-505',
      description: 'Sensor proximity spindle error mendadak'
    },
    testUrl: '/api/unplan-downtimes',
    testMethod: 'POST',
    loading: false,
    response: null,
    responseStatus: null,
    responseTime: null
  }
]);

function toggleExpand(index) {
  expandedIndex.value = expandedIndex.value === index ? -1 : index;
}

function getMethodColor(method) {
  switch (method) {
    case 'GET': return '#2563eb';
    case 'POST': return '#10b981';
    case 'PUT': return '#f59e0b';
    case 'DELETE': return '#ef4444';
    default: return '#64748b';
  }
}

function getMethodBg(method) {
  switch (method) {
    case 'GET': return '#eff6ff';
    case 'POST': return '#ecfdf5';
    case 'PUT': return '#fffbeb';
    case 'DELETE': return '#fef2f2';
    default: return '#f1f5f9';
  }
}

function getMethodBorder(method) {
  switch (method) {
    case 'GET': return '#bfdbfe';
    case 'POST': return '#a7f3d0';
    case 'PUT': return '#fde68a';
    case 'DELETE': return '#fecaca';
    default: return '#e2e8f0';
  }
}

async function testEndpoint(ep) {
  ep.loading = true;
  const start = performance.now();
  try {
    const method = ep.testMethod || ep.method;
    const options = {
      method,
      headers: { 'Content-Type': 'application/json' }
    };
    if ((method === 'POST' || method === 'PUT') && ep.bodySchema) {
      options.body = JSON.stringify(ep.bodySchema);
    }

    const res = await fetch(ep.testUrl, options);
    ep.responseStatus = res.status;
    ep.response = await res.json().catch(() => ({ message: 'No JSON body' }));
    ep.responseTime = Math.round(performance.now() - start);
  } catch (err) {
    ep.responseStatus = 500;
    ep.response = { error: err.message };
    ep.responseTime = Math.round(performance.now() - start);
  } finally {
    ep.loading = false;
  }
}
</script>
