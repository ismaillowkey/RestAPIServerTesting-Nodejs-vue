<template>
  <div class="app-container">
    <!-- Header -->
    <header class="app-header">
      <div class="header-inner">
        <div class="brand-section">
          <div class="brand-icon-wrapper">
            ⚡
          </div>
          <div>
            <div style="display: flex; align-items: center;">
              <span class="brand-title">REST API Server & Dashboard</span>
              <span class="brand-badge">Node.js + Vue + SQLite</span>
            </div>
            <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 1px;">
              Single Port Architecture • Full CRUD System • OpenAPI Swagger
            </div>
          </div>
        </div>

        <div class="header-status">
          <div class="port-pill">
            <span class="status-dot"></span>
            <span>Port: <strong>{{ activePort }}</strong></span>
          </div>
          <a
            href="/api/docs"
            target="_blank"
            rel="noopener noreferrer"
            class="btn btn-primary"
            style="text-decoration: none; font-weight: 700; gap: 0.5rem;"
            id="btn-top-swagger"
          >
            <span>🚀</span>
            <span>API Documentation Swagger</span>
            <span style="font-size: 0.8rem;">↗</span>
          </a>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="main-content">
      <!-- Tab Navigation -->
      <TabNav
        :current-tab="activeTab"
        :counts="tabCounts"
        @update:current-tab="activeTab = $event"
      />

      <!-- Tab 1: Product -->
      <ProductView
        v-if="activeTab === 'product'"
        @notify="showToast"
        @update-count="tabCounts.product = $event"
      />

      <!-- Tab 2: Plan Downtime -->
      <DowntimeView
        v-if="activeTab === 'plan'"
        category="plan"
        title="Plan Downtime"
        icon="📅"
        description-text="Pencatatan dan penjadwalan downtime terencana (preventive maintenance, changeover, inspeksi kalibrasi, upgrade)"
        @notify="showToast"
        @update-count="tabCounts.plan = $event"
      />

      <!-- Tab 3: Unplan Downtime -->
      <DowntimeView
        v-if="activeTab === 'unplan'"
        category="unplan"
        title="Unplan Downtime"
        icon="⚠️"
        description-text="Pencatatan insiden breakdown tak terencana (kerusakan mekanik, trip breaker listrik, jaringan terputus, dsb.)"
        @notify="showToast"
        @update-count="tabCounts.unplan = $event"
      />

      <!-- Tab 4: REST API Documentation -->
      <ApiDocsView
        v-if="activeTab === 'docs'"
      />
    </main>

    <!-- Global Toast Notifications -->
    <ToastNotification
      :toasts="toasts"
      @remove="removeToast"
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import TabNav from './components/TabNav.vue';
import ProductView from './components/ProductView.vue';
import DowntimeView from './components/DowntimeView.vue';
import ApiDocsView from './components/ApiDocsView.vue';
import ToastNotification from './components/ToastNotification.vue';
import api from './services/api.js';

const activeTab = ref('product');
const activePort = ref(typeof window !== 'undefined' && window.location.port ? window.location.port : '3000');

const tabCounts = reactive({
  product: 0,
  plan: 0,
  unplan: 0
});

const toasts = ref([]);
let toastIdCounter = 0;

function showToast({ type = 'success', message = '' }) {
  const id = ++toastIdCounter;
  toasts.value.push({ id, type, message });

  setTimeout(() => {
    removeToast(id);
  }, 4000);
}

function removeToast(id) {
  const index = toasts.value.findIndex(t => t.id === id);
  if (index !== -1) {
    toasts.value.splice(index, 1);
  }
}

async function loadInitialCounts() {
  try {
    const [pRes, plRes, unRes] = await Promise.all([
      api.getProducts({ limit: 1 }),
      api.getPlanDowntimes({ limit: 1 }),
      api.getUnplanDowntimes({ limit: 1 })
    ]);

    tabCounts.product = pRes.pagination ? pRes.pagination.total : (pRes.total || 0);
    tabCounts.plan = plRes.pagination ? plRes.pagination.total : (plRes.total || 0);
    tabCounts.unplan = unRes.pagination ? unRes.pagination.total : (unRes.total || 0);
  } catch (err) {
    console.error('Error fetching initial counts:', err);
  }
}

onMounted(() => {
  loadInitialCounts();
});
</script>
