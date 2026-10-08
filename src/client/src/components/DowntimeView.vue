<template>
  <div>
    <div class="split-layout">
    <!-- Bagan Kiri: CRUD Manual -->
    <div class="panel-card">
      <!-- Header -->
      <div class="panel-header">
        <div class="panel-title-area">
          <h2>{{ icon }} Kategori {{ title }}</h2>
          <p>{{ descriptionText }}</p>
        </div>

        <div style="display: flex; gap: 0.75rem;">
          <button type="button" class="btn btn-outline btn-sm" @click="exportCSV" title="Ekspor data ke file CSV">
            📥 Ekspor CSV
          </button>
          <button
            type="button"
            class="btn btn-primary"
            @click="openAddModal"
            :id="`btn-add-${category}`"
          >
            <span>➕</span>
            <span>Tambah {{ title }}</span>
          </button>
        </div>
      </div>

      <!-- Toolbar & Filter -->
      <div class="toolbar-bar">
        <div class="search-box">
          <span class="search-icon">🔍</span>
          <input
            v-model="searchQuery"
            type="text"
            class="search-input"
            :placeholder="`Cari kode, deskripsi, atau departemen...`"
            @input="handleSearch"
          />
        </div>

        <div class="filter-actions">
          <!-- Department Filter -->
          <select
            v-model="departmentFilter"
            class="form-control form-select"
            style="width: 170px; height: 38px;"
            @change="fetchData(1)"
          >
            <option value="">Semua Departemen</option>
            <option v-for="d in departmentList" :key="d" :value="d">
              {{ d }}
            </option>
          </select>

          <button
            v-if="searchQuery || departmentFilter !== ''"
            type="button"
            class="btn btn-outline btn-sm"
            @click="resetFilters"
          >
            Reset
          </button>
        </div>
      </div>

      <!-- Table Section -->
      <div class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th style="width: 50px;">ID</th>
              <th style="width: 150px;">Departemen</th>
              <th style="width: 130px;">Kode</th>
              <th>Deskripsi</th>
              <th style="width: 130px; text-align: right;">Aksi</th>
            </tr>
          </thead>
          <tbody v-if="loading">
            <tr>
              <td colspan="5" style="text-align: center; padding: 3rem;">
                <span style="color: var(--text-muted);">Memuat data {{ title }}...</span>
              </td>
            </tr>
          </tbody>
          <tbody v-else-if="items.length === 0">
            <tr>
              <td colspan="5">
                <div class="empty-state">
                  <div class="empty-icon">{{ icon }}</div>
                  <div class="empty-title">Tidak ada data {{ title }}</div>
                  <p style="font-size: 0.85rem; margin-bottom: 1rem;">
                    {{ searchQuery ? 'Tidak ada data yang cocok.' : `Belum ada data ${title} yang tersimpan.` }}
                  </p>
                  <button type="button" class="btn btn-primary btn-sm" @click="openAddModal">
                    Tambah Data Pertama
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
          <tbody v-else>
            <tr v-for="item in items" :key="item.id" :id="`${category}-row-${item.id}`">
              <td style="font-weight: 700; color: var(--text-muted);">#{{ item.id }}</td>
              <td>
                <span class="badge badge-info" style="font-size: 0.775rem;">
                  🏢 {{ item.department }}
                </span>
              </td>
              <td>
                <span
                  class="badge"
                  :class="category === 'plan' ? 'badge-warning' : 'badge-danger'"
                  style="font-family: monospace; font-size: 0.8rem;"
                >
                  {{ item.downtime_code }}
                </span>
              </td>
              <td style="color: var(--text-main); font-size: 0.85rem;">
                {{ item.description || '-' }}
              </td>
              <td style="text-align: right;">
                <div style="display: inline-flex; gap: 0.35rem;">
                  <button
                    type="button"
                    class="btn btn-outline btn-sm"
                    title="Edit Data"
                    @click="openEditModal(item)"
                    :id="`btn-edit-${category}-${item.id}`"
                  >
                    ✏️
                  </button>
                  <button
                    type="button"
                    class="btn btn-danger-outline btn-sm"
                    title="Hapus Data"
                    @click="openDeleteConfirm(item)"
                    :id="`btn-delete-${category}-${item.id}`"
                  >
                    🗑️
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <PaginationBar
        :page="pagination.page"
        :limit="pagination.limit"
        :total="pagination.total"
        :total-pages="pagination.totalPages"
        @update:page="handlePageChange"
        @update:limit="handleLimitChange"
      />
    </div>

    <!-- Bagan Kanan: Dokumentasi REST API Langsung -->
    <div style="position: sticky; top: 90px;">
      <CategoryApiDocs
        :category="category"
        :title="title"
        :base-endpoint="category === 'plan' ? '/api/plan-downtimes' : '/api/unplan-downtimes'"
        :sample-search="category === 'plan' ? 'PM-001' : 'BRK-101'"
        :fields="downtimeFields"
        :extra-get-param="{ name: 'department', type: 'String', desc: 'Filter data berdasarkan nama departemen' }"
        :sample-post="{
          department: 'Maintenance',
          downtime_code: category === 'plan' ? 'PM-005' : 'BRK-303',
          description: category === 'plan' ? 'Preventive maintenance jalur kompresor angin' : 'Inverter motor feeder overload mendadak'
        }"
        :sample-put="{
          department: 'Maintenance',
          downtime_code: category === 'plan' ? 'PM-005' : 'BRK-303',
          description: category === 'plan' ? 'Preventive maintenance jalur kompresor angin (Selesai)' : 'Penggantian modul inverter yang rusak'
        }"
        :sample-response="{
          id: 1,
          department: 'Maintenance',
          downtime_code: category === 'plan' ? 'PM-001' : 'BRK-101',
          description: category === 'plan' ? 'Preventive maintenance mesin press bulanan' : 'Motor conveyor utama macet akibat bearing aus',
          created_at: '2026-09-15 14:00:00',
          updated_at: '2026-09-15 14:00:00'
        }"
      />
    </div>
  </div>

    <!-- Add/Edit Modal Dialog -->
    <ModalDialog
      :show="showModal"
      :title="isEdit ? `Edit ${title} #${form.id}` : `Tambah ${title} Baru`"
      @close="closeModal"
    >
      <form @submit.prevent="saveData">
        <div class="form-group">
          <label class="form-label">Departemen (Dapartment) *</label>
          <input
            v-model="form.department"
            type="text"
            list="dept-options"
            class="form-control"
            placeholder="Contoh: Maintenance, Production, IT"
            required
            :id="`input-${category}-department`"
          />
          <datalist id="dept-options">
            <option value="Maintenance"></option>
            <option value="Production"></option>
            <option value="Electrical"></option>
            <option value="IT & Infrastructure"></option>
            <option value="Quality Assurance"></option>
            <option value="Facility"></option>
            <option value="Logistics"></option>
          </datalist>
        </div>

        <div class="form-group">
          <label class="form-label">Downtime Code *</label>
          <input
            v-model="form.downtime_code"
            type="text"
            class="form-control"
            placeholder="Contoh: PM-001 atau BRK-101"
            style="text-transform: uppercase;"
            required
            :id="`input-${category}-code`"
          />
        </div>

        <div class="form-group">
          <label class="form-label">Deskripsi (Description)</label>
          <textarea
            v-model="form.description"
            class="form-control"
            rows="3"
            placeholder="Keterangan detail downtime atau penyebab/jadwal pelaksanaan..."
            :id="`input-${category}-desc`"
          ></textarea>
        </div>
      </form>

      <template #footer>
        <button type="button" class="btn btn-outline" @click="closeModal">
          Batal
        </button>
        <button
          type="button"
          class="btn btn-primary"
          :disabled="submitting"
          @click="saveData"
          :id="`btn-save-${category}`"
        >
          {{ submitting ? 'Menyimpan...' : (isEdit ? 'Simpan Perubahan' : 'Tambah Data') }}
        </button>
      </template>
    </ModalDialog>

    <!-- Delete Confirm Modal -->
    <ConfirmModal
      :show="showDeleteModal"
      :title="`Hapus ${title}`"
      :message="`Apakah Anda yakin ingin menghapus catatan ${title} '${selectedItem?.downtime_code} - ${selectedItem?.department}' (ID #${selectedItem?.id})?`"
      :loading="deleting"
      @confirm="confirmDelete"
      @cancel="showDeleteModal = false"
    />
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue';
import api from '../services/api.js';
import ModalDialog from './ModalDialog.vue';
import ConfirmModal from './ConfirmModal.vue';
import PaginationBar from './PaginationBar.vue';
import CategoryApiDocs from './CategoryApiDocs.vue';

const downtimeFields = [
  { name: 'department', type: 'String', required: true, desc: 'Nama departemen (contoh: Maintenance, Production, Electrical, IT).' },
  { name: 'downtime_code', type: 'String', required: true, desc: 'Kode alfanumerik downtime unik (contoh: PM-001, BRK-101).' },
  { name: 'description', type: 'String', required: false, default: "''", desc: 'Keterangan detail downtime, kerusakan atau rencana perbaikan.' }
];

const props = defineProps({
  category: {
    type: String, // 'plan' atau 'unplan'
    required: true
  },
  title: {
    type: String,
    default: 'Downtime'
  },
  icon: {
    type: String,
    default: '⏱️'
  },
  descriptionText: {
    type: String,
    default: 'Manajemen pencatatan downtime mesin dan sistem'
  }
});

const emit = defineEmits(['notify', 'update-count']);

const items = ref([]);
const loading = ref(false);
const submitting = ref(false);
const deleting = ref(false);

const searchQuery = ref('');
const departmentFilter = ref('');

const pagination = reactive({
  page: 1,
  limit: 10,
  total: 0,
  totalPages: 1
});

// Modal form state
const showModal = ref(false);
const isEdit = ref(false);
const form = reactive({
  id: null,
  department: '',
  downtime_code: '',
  description: ''
});

// Delete confirm state
const showDeleteModal = ref(false);
const selectedItem = ref(null);

let searchTimeout = null;

const departmentList = computed(() => {
  const depts = new Set(['Maintenance', 'Production', 'Electrical', 'IT & Infrastructure', 'Facility', 'Quality Assurance', 'Logistics']);
  items.value.forEach(i => {
    if (i.department) depts.add(i.department);
  });
  return Array.from(depts);
});

function handleSearch() {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    fetchData(1);
  }, 300);
}

function resetFilters() {
  searchQuery.value = '';
  departmentFilter.value = '';
  fetchData(1);
}

function handlePageChange(newPage) {
  fetchData(newPage);
}

function handleLimitChange(newLimit) {
  pagination.limit = newLimit;
  fetchData(1);
}

async function fetchData(page = pagination.page) {
  loading.value = true;
  try {
    const params = {
      page,
      limit: pagination.limit,
      search: searchQuery.value,
      department: departmentFilter.value
    };

    const res = props.category === 'plan'
      ? await api.getPlanDowntimes(params)
      : await api.getUnplanDowntimes(params);

    if (res.pagination) {
      items.value = res.data;
      pagination.page = res.pagination.page;
      pagination.total = res.pagination.total;
      pagination.totalPages = res.pagination.totalPages;
    } else {
      items.value = res.data;
      pagination.total = res.total || res.data.length;
      pagination.totalPages = 1;
    }

    emit('update-count', pagination.total);
  } catch (err) {
    emit('notify', { type: 'error', message: `Gagal memuat ${props.title}: ${err.message}` });
  } finally {
    loading.value = false;
  }
}

function openAddModal() {
  isEdit.value = false;
  form.id = null;
  form.department = '';
  form.downtime_code = '';
  form.description = '';
  showModal.value = true;
}

function openEditModal(item) {
  isEdit.value = true;
  form.id = item.id;
  form.department = item.department;
  form.downtime_code = item.downtime_code;
  form.description = item.description || '';
  showModal.value = true;
}

function closeModal() {
  showModal.value = false;
}

async function saveData() {
  if (!form.department.trim()) {
    emit('notify', { type: 'error', message: 'Departemen wajib diisi' });
    return;
  }
  if (!form.downtime_code.trim()) {
    emit('notify', { type: 'error', message: 'Downtime code wajib diisi' });
    return;
  }

  submitting.value = true;
  try {
    const payload = {
      department: form.department.trim(),
      downtime_code: form.downtime_code.trim(),
      description: form.description.trim()
    };

    if (isEdit.value) {
      if (props.category === 'plan') {
        await api.updatePlanDowntime(form.id, payload);
      } else {
        await api.updateUnplanDowntime(form.id, payload);
      }
      emit('notify', { type: 'success', message: `${props.title} #${form.id} berhasil diperbarui!` });
    } else {
      if (props.category === 'plan') {
        await api.createPlanDowntime(payload);
      } else {
        await api.createUnplanDowntime(payload);
      }
      emit('notify', { type: 'success', message: `${props.title} baru berhasil ditambahkan!` });
    }

    closeModal();
    fetchData();
  } catch (err) {
    emit('notify', { type: 'error', message: err.message });
  } finally {
    submitting.value = false;
  }
}

function openDeleteConfirm(item) {
  selectedItem.value = item;
  showDeleteModal.value = true;
}

async function confirmDelete() {
  if (!selectedItem.value) return;

  deleting.value = true;
  try {
    if (props.category === 'plan') {
      await api.deletePlanDowntime(selectedItem.value.id);
    } else {
      await api.deleteUnplanDowntime(selectedItem.value.id);
    }
    emit('notify', { type: 'success', message: `${props.title} #${selectedItem.value.id} berhasil dihapus!` });
    showDeleteModal.value = false;
    fetchData();
  } catch (err) {
    emit('notify', { type: 'error', message: `Gagal menghapus: ${err.message}` });
  } finally {
    deleting.value = false;
  }
}

function exportCSV() {
  if (!items.value.length) return;
  const headers = ['ID', 'Department', 'Downtime Code', 'Description', 'Created At'];
  const rows = items.value.map(i => [
    i.id,
    `"${(i.department || '').replace(/"/g, '""')}"`,
    `"${(i.downtime_code || '').replace(/"/g, '""')}"`,
    `"${(i.description || '').replace(/"/g, '""')}"`,
    `"${i.created_at || ''}"`
  ]);

  const csvContent = 'data:text/csv;charset=utf-8,' + [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
  const encodedUri = encodeURI(csvContent);
  const link = document.createElement('a');
  link.setAttribute('href', encodedUri);
  link.setAttribute('download', `${props.category}_downtime_${new Date().toISOString().slice(0, 10)}.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

onMounted(() => {
  fetchData(1);
});

defineExpose({
  fetchData
});
</script>
