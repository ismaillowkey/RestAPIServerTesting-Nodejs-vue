<template>
  <div class="panel-card category-docs-panel">
    <!-- Header -->
    <div class="panel-header" style="background: #ffffff;">
      <div class="panel-title-area">
        <div style="display: flex; align-items: center; gap: 0.5rem;">
          <span style="font-size: 1.15rem;">📘</span>
          <h2 style="font-size: 1.1rem; font-weight: 700; color: var(--text-main);">
            Dokumentasi REST API: {{ title }}
          </h2>
        </div>
        <p style="font-size: 0.8rem; color: var(--text-muted); margin-top: 0.2rem;">
          Panduan integrasi CRUD, query pagination opsional & format data
        </p>
      </div>

      <span class="badge badge-info" style="font-family: monospace; font-size: 0.8rem;">
        {{ baseEndpoint }}
      </span>
    </div>

    <!-- Method Selector Pills -->
    <div style="padding: 0.85rem 1.25rem; background: #fafbfc; border-bottom: 1px solid var(--border-color);">
      <div class="method-nav">
        <button
          v-for="m in methods"
          :key="m.name"
          type="button"
          class="method-btn"
          :class="{ active: selectedMethod === m.name }"
          :style="selectedMethod === m.name ? { borderColor: m.color, color: m.color, backgroundColor: m.bg } : {}"
          @click="selectedMethod = m.name"
        >
          <span class="method-badge" :style="{ backgroundColor: m.color }">
            {{ m.badge }}
          </span>
          <span>{{ m.label }}</span>
        </button>
      </div>
    </div>

    <!-- Doc Content Body -->
    <div class="docs-scroll-body">
      <!-- ==================== 1. GET ALL (READ LIST) ==================== -->
      <div v-if="selectedMethod === 'GET_ALL'" class="doc-section">
        <div class="endpoint-bar">
          <span class="badge" style="background: #eff6ff; color: #2563eb; border-color: #bfdbfe; font-family: monospace; font-weight: 700;">
            GET
          </span>
          <code class="endpoint-url">{{ baseEndpoint }}</code>
          <span class="badge badge-info" style="margin-left: auto; font-size: 0.725rem;">Read All / List</span>
        </div>

        <p class="section-desc">
          Mengambil seluruh daftar data dari tabel SQLite. Endpoint ini mendukung <strong>query parameter pagination opsional</strong> (<code>?page=1&limit=10</code>). Jika query pagination diabaikan, server akan mengembalikan seluruh data.
        </p>

        <!-- Query Parameters -->
        <h4 class="sub-title">Query Parameters (Opsional):</h4>
        <div class="table-responsive" style="margin-bottom: 1rem;">
          <table class="data-table compact-table">
            <thead>
              <tr>
                <th style="width: 130px;">Parameter</th>
                <th style="width: 100px;">Tipe</th>
                <th style="width: 90px;">Status</th>
                <th>Keterangan</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td><code>page</code></td>
                <td>Integer</td>
                <td><span class="badge" style="background: #f1f5f9; color: #64748b;">Opsional</span></td>
                <td>Nomor halaman data (contoh: <code>?page=1</code>).</td>
              </tr>
              <tr>
                <td><code>limit</code></td>
                <td>Integer</td>
                <td><span class="badge" style="background: #f1f5f9; color: #64748b;">Opsional</span></td>
                <td>Jumlah baris per halaman (contoh: <code>?limit=10</code>, default: 10).</td>
              </tr>
              <tr>
                <td><code>search</code></td>
                <td>String</td>
                <td><span class="badge" style="background: #f1f5f9; color: #64748b;">Opsional</span></td>
                <td>Kata kunci pencarian (contoh: <code>?search={{ sampleSearch }}</code>).</td>
              </tr>
              <tr v-if="extraGetParam">
                <td><code>{{ extraGetParam.name }}</code></td>
                <td>{{ extraGetParam.type }}</td>
                <td><span class="badge" style="background: #f1f5f9; color: #64748b;">Opsional</span></td>
                <td>{{ extraGetParam.desc }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Example Response -->
        <div style="margin-top: 0.75rem;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.4rem;">
            <h4 class="sub-title" style="margin: 0;">Contoh Response dengan Pagination (Status 200 OK):</h4>
            <button type="button" class="btn btn-outline btn-sm" style="font-size: 0.75rem; padding: 0.2rem 0.5rem;" @click="testLive('GET_ALL')">
              ⚡ Uji GET ALL
            </button>
          </div>
          <pre class="code-block"><code>{{ formattedGetResponse }}</code></pre>
        </div>
      </div>

      <!-- ==================== 2. GET BY ID (READ SINGLE) ==================== -->
      <div v-if="selectedMethod === 'GET_ID'" class="doc-section">
        <div class="endpoint-bar">
          <span class="badge" style="background: #f0f9ff; color: #0284c7; border-color: #bae6fd; font-family: monospace; font-weight: 700;">
            GET
          </span>
          <code class="endpoint-url">{{ baseEndpoint }}/:id</code>
          <span class="badge" style="background: #e0f2fe; color: #0369a1; margin-left: auto; font-size: 0.725rem;">Read by ID</span>
        </div>

        <p class="section-desc">
          Mengambil detail 1 record data spesifik dari database SQLite berdasarkan nilai <code>id</code>.
        </p>

        <h4 class="sub-title">Parameter URL (Path Variable):</h4>
        <div class="table-responsive" style="margin-bottom: 1rem;">
          <table class="data-table compact-table">
            <thead>
              <tr>
                <th style="width: 140px;">Parameter</th>
                <th style="width: 110px;">Tipe</th>
                <th style="width: 100px;">Status</th>
                <th>Keterangan</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td><code>:id</code></td>
                <td>Integer</td>
                <td><span class="badge badge-danger">Wajib (Path)</span></td>
                <td>ID unik record yang dicari (contoh: <code>{{ baseEndpoint }}/1</code>).</td>
              </tr>
            </tbody>
          </table>
        </div>

        <h4 class="sub-title">Status HTTP Response:</h4>
        <ul style="font-size: 0.825rem; color: var(--text-muted); margin-left: 1.25rem; margin-bottom: 1rem; line-height: 1.6;">
          <li><strong style="color: var(--success-text);">200 OK</strong>: Record ditemukan dan data dikembalikan dalam objek <code>data</code>.</li>
          <li><strong style="color: var(--danger-text);">404 Not Found</strong>: Record dengan ID tersebut tidak ditemukan di database.</li>
        </ul>

        <div>
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.4rem;">
            <h4 class="sub-title" style="margin: 0;">Contoh Response (Status 200 OK):</h4>
            <button type="button" class="btn btn-outline btn-sm" style="font-size: 0.75rem; padding: 0.2rem 0.5rem;" @click="testLive('GET_ID')">
              ⚡ Uji GET BY ID
            </button>
          </div>
          <pre class="code-block"><code>{
  "success": true,
  "data": {{ JSON.stringify(sampleResponse, null, 2) }}
}</code></pre>
        </div>
      </div>

      <!-- ==================== 3. POST (CREATE) ==================== -->
      <div v-if="selectedMethod === 'POST'" class="doc-section">
        <div class="endpoint-bar">
          <span class="badge" style="background: #ecfdf5; color: #10b981; border-color: #a7f3d0; font-family: monospace; font-weight: 700;">
            POST
          </span>
          <code class="endpoint-url">{{ baseEndpoint }}</code>
          <span class="badge badge-success" style="margin-left: auto; font-size: 0.725rem;">Create</span>
        </div>

        <p class="section-desc">
          Menambahkan data baru ke dalam database. Kirimkan data melalui <strong>Request Body</strong> dengan header <code>Content-Type: application/json</code>.
        </p>

        <!-- Required Fields Table -->
        <h4 class="sub-title">Data yang Dibutuhkan (Request Body Fields):</h4>
        <div class="table-responsive" style="margin-bottom: 1rem;">
          <table class="data-table compact-table">
            <thead>
              <tr>
                <th>Field Name</th>
                <th>Tipe</th>
                <th>Status</th>
                <th>Keterangan</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="f in fields" :key="f.name">
                <td><code>{{ f.name }}</code></td>
                <td>{{ f.type }}</td>
                <td>
                  <span class="badge" :class="f.required ? 'badge-danger' : 'badge-info'">
                    {{ f.required ? 'Wajib' : 'Opsional' }}
                  </span>
                </td>
                <td>{{ f.desc }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Example Request Body -->
        <div style="margin-bottom: 1rem;">
          <h4 class="sub-title">Contoh Request Body (JSON):</h4>
          <pre class="code-block"><code>{{ JSON.stringify(samplePost, null, 2) }}</code></pre>
        </div>

        <!-- Example Response -->
        <div>
          <h4 class="sub-title">Contoh Response (Status 201 Created):</h4>
          <pre class="code-block"><code>{{ formattedPostResponse }}</code></pre>
        </div>
      </div>

      <!-- ==================== 4. PUT (UPDATE) ==================== -->
      <div v-if="selectedMethod === 'PUT'" class="doc-section">
        <div class="endpoint-bar">
          <span class="badge" style="background: #fffbeb; color: #d97706; border-color: #fde68a; font-family: monospace; font-weight: 700;">
            PUT
          </span>
          <code class="endpoint-url">{{ baseEndpoint }}/:id</code>
          <span class="badge badge-warning" style="margin-left: auto; font-size: 0.725rem;">Update</span>
        </div>

        <p class="section-desc">
          Memperbarui record yang sudah ada berdasarkan parameter <code>id</code>. Kirimkan field yang ingin diubah melalui Request Body (JSON).
        </p>

        <h4 class="sub-title">Parameter URL (Path Variable):</h4>
        <div class="table-responsive" style="margin-bottom: 1rem;">
          <table class="data-table compact-table">
            <thead>
              <tr>
                <th style="width: 140px;">Parameter</th>
                <th style="width: 110px;">Tipe</th>
                <th style="width: 100px;">Status</th>
                <th>Keterangan</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td><code>:id</code></td>
                <td>Integer</td>
                <td><span class="badge badge-danger">Wajib (Path)</span></td>
                <td>ID record yang akan diupdate (contoh: <code>{{ baseEndpoint }}/1</code>).</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Example Request Body -->
        <div style="margin-bottom: 1rem;">
          <h4 class="sub-title">Contoh Request Body Update (JSON):</h4>
          <pre class="code-block"><code>{{ JSON.stringify(samplePut, null, 2) }}</code></pre>
        </div>

        <div>
          <h4 class="sub-title">Contoh Response (Status 200 OK):</h4>
          <pre class="code-block"><code>{{ formattedPutResponse }}</code></pre>
        </div>
      </div>

      <!-- ==================== 5. DELETE ==================== -->
      <div v-if="selectedMethod === 'DELETE'" class="doc-section">
        <div class="endpoint-bar">
          <span class="badge" style="background: #fef2f2; color: #ef4444; border-color: #fecaca; font-family: monospace; font-weight: 700;">
            DELETE
          </span>
          <code class="endpoint-url">{{ baseEndpoint }}/:id</code>
          <span class="badge badge-danger" style="margin-left: auto; font-size: 0.725rem;">Delete</span>
        </div>

        <p class="section-desc">
          Menghapus data secara permanen dari database SQLite berdasarkan <code>id</code> yang disertakan pada URL.
        </p>

        <h4 class="sub-title">Parameter URL (Path Variable):</h4>
        <div class="table-responsive" style="margin-bottom: 1rem;">
          <table class="data-table compact-table">
            <thead>
              <tr>
                <th style="width: 140px;">Parameter</th>
                <th style="width: 110px;">Tipe</th>
                <th style="width: 100px;">Status</th>
                <th>Keterangan</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td><code>:id</code></td>
                <td>Integer</td>
                <td><span class="badge badge-danger">Wajib (Path)</span></td>
                <td>ID record yang akan dihapus (contoh: <code>{{ baseEndpoint }}/1</code>).</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div>
          <h4 class="sub-title">Contoh Response (Status 200 OK):</h4>
          <pre class="code-block"><code>{
  "success": true,
  "message": "{{ title }} berhasil dihapus",
  "data": {{ JSON.stringify(sampleResponse, null, 2) }}
}</code></pre>
        </div>
      </div>

      <!-- Live Test Result Modal / Drawer -->
      <div v-if="liveTestResult" style="margin-top: 1rem; border-top: 1px dashed var(--border-color); padding-top: 1rem;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.4rem;">
          <span style="font-size: 0.8rem; font-weight: 700; color: var(--text-main);">
            🟢 Hasil Uji Langsung ({{ liveTestResult.status }} OK):
          </span>
          <button type="button" class="btn btn-outline btn-sm" style="font-size: 0.7rem; padding: 0.15rem 0.4rem;" @click="liveTestResult = null">
            Tutup
          </button>
        </div>
        <pre class="code-block" style="max-height: 180px; overflow-y: auto;"><code>{{ JSON.stringify(liveTestResult.data, null, 2) }}</code></pre>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';

const props = defineProps({
  category: {
    type: String,
    required: true
  },
  title: {
    type: String,
    required: true
  },
  baseEndpoint: {
    type: String,
    required: true
  },
  fields: {
    type: Array,
    required: true
  },
  sampleSearch: {
    type: String,
    default: 'sample'
  },
  extraGetParam: {
    type: Object,
    default: null
  },
  samplePost: {
    type: Object,
    required: true
  },
  samplePut: {
    type: Object,
    required: true
  },
  sampleResponse: {
    type: Object,
    required: true
  }
});

const selectedMethod = ref('GET_ALL');
const liveTestResult = ref(null);

const methods = [
  { name: 'GET_ALL', badge: 'GET', label: 'GET ALL', color: '#2563eb', bg: '#eff6ff' },
  { name: 'GET_ID', badge: 'GET', label: 'GET BY ID', color: '#0284c7', bg: '#f0f9ff' },
  { name: 'POST', badge: 'POST', label: 'POST', color: '#10b981', bg: '#ecfdf5' },
  { name: 'PUT', badge: 'PUT', label: 'PUT', color: '#d97706', bg: '#fffbeb' },
  { name: 'DELETE', badge: 'DEL', label: 'DELETE', color: '#ef4444', bg: '#fef2f2' }
];

const formattedGetResponse = computed(() => {
  return JSON.stringify(
    {
      success: true,
      data: [props.sampleResponse],
      pagination: {
        page: 1,
        limit: 10,
        total: 25,
        totalPages: 3,
        hasNext: true,
        hasPrev: false
      }
    },
    null,
    2
  );
});

const formattedPostResponse = computed(() => {
  return JSON.stringify(
    {
      success: true,
      message: `${props.title} berhasil ditambahkan`,
      data: {
        id: 15,
        ...props.samplePost,
        created_at: '2026-09-15 14:30:00',
        updated_at: '2026-09-15 14:30:00'
      }
    },
    null,
    2
  );
});

const formattedPutResponse = computed(() => {
  return JSON.stringify(
    {
      success: true,
      message: `${props.title} berhasil diperbarui`,
      data: {
        id: 1,
        ...props.sampleResponse,
        ...props.samplePut,
        updated_at: '2026-09-15 14:45:00'
      }
    },
    null,
    2
  );
});

async function testLive(type) {
  try {
    let url = `${props.baseEndpoint}?limit=2`;
    if (type === 'GET_ID') {
      url = `${props.baseEndpoint}/1`;
    }
    const res = await fetch(url);
    const data = await res.json();
    liveTestResult.value = {
      status: res.status,
      data
    };
  } catch (err) {
    liveTestResult.value = {
      status: 500,
      data: { error: err.message }
    };
  }
}
</script>

<style scoped>
.category-docs-panel {
  background: #ffffff;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  display: flex;
  flex-direction: column;
}

.docs-scroll-body {
  padding: 1.25rem 1.5rem;
  max-height: calc(100vh - 200px);
  overflow-y: auto;
}

.method-nav {
  display: flex;
  gap: 0.35rem;
  flex-wrap: wrap;
}

.method-btn {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.65rem;
  font-size: 0.775rem;
  font-weight: 600;
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  background: #fff;
  cursor: pointer;
  transition: var(--transition);
  font-family: inherit;
  color: var(--text-muted);
}

.method-badge {
  color: #fff;
  font-size: 0.675rem;
  padding: 0.1rem 0.35rem;
  border-radius: 4px;
  font-weight: 700;
  font-family: monospace;
}

.endpoint-bar {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.5rem 0.85rem;
  background: #f8fafc;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-color);
  margin-bottom: 0.75rem;
  flex-wrap: wrap;
}

.endpoint-url {
  font-family: monospace;
  font-size: 0.875rem;
  font-weight: 700;
  color: var(--text-main);
}

.section-desc {
  font-size: 0.825rem;
  color: var(--text-muted);
  line-height: 1.5;
  margin-bottom: 0.85rem;
}

.sub-title {
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--text-main);
  margin-bottom: 0.4rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.compact-table th {
  padding: 0.5rem 0.75rem;
  font-size: 0.725rem;
}

.compact-table td {
  padding: 0.5rem 0.75rem;
  font-size: 0.8rem;
}

.code-block {
  background: #0f172a;
  color: #38bdf8;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-md);
  font-size: 0.785rem;
  line-height: 1.4;
  overflow-x: auto;
}
</style>
