<template>
  <div>
    <div class="split-layout">
    <!-- Bagan Kiri: CRUD Manual -->
    <div class="panel-card">
      <!-- Header & Summary Stats -->
      <div class="panel-header">
        <div class="panel-title-area">
          <h2>Kategori Product</h2>
          <p>Manajemen data produk, harga, stok, dan status ketersediaan</p>
        </div>

        <div style="display: flex; gap: 0.75rem;">
          <button type="button" class="btn btn-outline btn-sm" @click="exportCSV" title="Ekspor data ke file CSV">
            📥 Ekspor CSV
          </button>
          <button type="button" class="btn btn-primary" @click="openAddModal" id="btn-add-product">
            <span>➕</span>
            <span>Tambah Produk</span>
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
            placeholder="Cari nama produk..."
            @input="handleSearch"
          />
        </div>

        <div class="filter-actions">
          <!-- Availability Filter -->
          <select
            v-model="availabilityFilter"
            class="form-control form-select"
            style="width: 170px; height: 38px;"
            @change="fetchProducts(1)"
          >
            <option value="">Semua Status</option>
            <option value="1">Tersedia (Available)</option>
            <option value="0">Habis (Unavailable)</option>
          </select>

          <!-- Reset Filter -->
          <button
            v-if="searchQuery || availabilityFilter !== ''"
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
              <th>Nama Produk</th>
              <th style="width: 130px;">Harga</th>
              <th style="width: 90px;">Stok</th>
              <th style="width: 120px;">Status</th>
              <th style="width: 130px; text-align: right;">Aksi</th>
            </tr>
          </thead>
          <tbody v-if="loading">
            <tr>
              <td colspan="6" style="text-align: center; padding: 3rem;">
                <span style="color: var(--text-muted);">Memuat data produk...</span>
              </td>
            </tr>
          </tbody>
          <tbody v-else-if="products.length === 0">
            <tr>
              <td colspan="6">
                <div class="empty-state">
                  <div class="empty-icon">📦</div>
                  <div class="empty-title">Tidak ada data produk</div>
                  <p style="font-size: 0.85rem; margin-bottom: 1rem;">
                    {{ searchQuery ? 'Tidak ada produk yang cocok.' : 'Belum ada produk yang tersimpan.' }}
                  </p>
                  <button type="button" class="btn btn-primary btn-sm" @click="openAddModal">
                    Tambah Produk Pertama
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
          <tbody v-else>
            <tr v-for="item in products" :key="item.id" :id="'product-row-' + item.id">
              <td style="font-weight: 700; color: var(--text-muted);">#{{ item.id }}</td>
              <td style="font-weight: 600;">
                {{ item.productname }}
              </td>
              <td style="font-weight: 600; color: var(--text-main);">
                {{ formatCurrency(item.price) }}
              </td>
              <td>
                <span
                  class="badge"
                  :class="item.stok > 0 ? 'badge-info' : 'badge-danger'"
                >
                  {{ item.stok }}
                </span>
              </td>
              <td>
                <span
                  class="badge"
                  :class="item.isavailable ? 'badge-success' : 'badge-danger'"
                >
                  {{ item.isavailable ? '● Tersedia' : '○ Habis' }}
                </span>
              </td>
              <td style="text-align: right;">
                <div style="display: inline-flex; gap: 0.35rem;">
                  <button
                    type="button"
                    class="btn btn-outline btn-sm"
                    title="Edit Produk"
                    @click="openEditModal(item)"
                    :id="'btn-edit-product-' + item.id"
                  >
                    ✏️
                  </button>
                  <button
                    type="button"
                    class="btn btn-danger-outline btn-sm"
                    title="Hapus Produk"
                    @click="openDeleteConfirm(item)"
                    :id="'btn-delete-product-' + item.id"
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
        category="product"
        title="Product"
        base-endpoint="/api/products"
        sample-search="Keyboard"
        :fields="productFields"
        :extra-get-param="{ name: 'isavailable', type: '0 atau 1', desc: 'Filter status produk: 1 (Tersedia) atau 0 (Habis)' }"
        :sample-post="{
          productname: 'Wireless Gaming Mouse',
          price: 450000,
          stok: 25,
          isavailable: 1
        }"
        :sample-put="{
          productname: 'Mechanical Keyboard RGB Pro',
          price: 920000,
          stok: 18,
          isavailable: 1
        }"
        :sample-response="{
          id: 1,
          productname: 'Mechanical Keyboard RGB',
          price: 850000,
          stok: 25,
          isavailable: 1,
          created_at: '2026-09-15 14:00:00',
          updated_at: '2026-09-15 14:00:00'
        }"
      />
    </div>
  </div>

    <!-- Add/Edit Modal Dialog -->
    <ModalDialog
      :show="showModal"
      :title="isEdit ? 'Edit Produk #' + form.id : 'Tambah Produk Baru'"
      @close="closeModal"
    >
      <form @submit.prevent="saveProduct">
        <div class="form-group">
          <label class="form-label">Nama Produk (productname) *</label>
          <input
            v-model="form.productname"
            type="text"
            class="form-control"
            placeholder="Contoh: Mechanical Keyboard RGB"
            required
            id="input-productname"
          />
        </div>

        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
          <div class="form-group">
            <label class="form-label">Harga (price) *</label>
            <input
              v-model.number="form.price"
              type="number"
              min="0"
              step="1000"
              class="form-control"
              placeholder="Contoh: 850000"
              required
              id="input-price"
            />
          </div>

          <div class="form-group">
            <label class="form-label">Stok (stok) *</label>
            <input
              v-model.number="form.stok"
              type="number"
              min="0"
              class="form-control"
              placeholder="Contoh: 10"
              required
              id="input-stok"
            />
          </div>
        </div>

        <div class="form-group" style="margin-top: 0.5rem;">
          <label class="switch-label">
            <input
              v-model="form.isavailable"
              type="checkbox"
              class="switch-input"
              id="input-isavailable"
            />
            <span class="switch-slider"></span>
            <span>
              Status Ketersediaan:
              <strong :style="{ color: form.isavailable ? 'var(--success)' : 'var(--danger)' }">
                {{ form.isavailable ? 'Tersedia (isavailable = 1)' : 'Tidak Tersedia (isavailable = 0)' }}
              </strong>
            </span>
          </label>
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
          @click="saveProduct"
          id="btn-save-product"
        >
          {{ submitting ? 'Menyimpan...' : (isEdit ? 'Simpan Perubahan' : 'Tambah Produk') }}
        </button>
      </template>
    </ModalDialog>

    <!-- Delete Confirm Modal -->
    <ConfirmModal
      :show="showDeleteModal"
      title="Hapus Produk"
      :message="`Apakah Anda yakin ingin menghapus produk '${selectedItem?.productname}' (ID #${selectedItem?.id})?`"
      :loading="deleting"
      @confirm="confirmDelete"
      @cancel="showDeleteModal = false"
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import api from '../services/api.js';
import ModalDialog from './ModalDialog.vue';
import ConfirmModal from './ConfirmModal.vue';
import PaginationBar from './PaginationBar.vue';
import CategoryApiDocs from './CategoryApiDocs.vue';

const productFields = [
  { name: 'productname', type: 'String', required: true, desc: 'Nama produk yang akan disimpan.' },
  { name: 'price', type: 'Number (Float)', required: true, desc: 'Harga jual produk (angka >= 0).' },
  { name: 'stok', type: 'Integer', required: false, default: '0', desc: 'Jumlah ketersediaan stok fisik.' },
  { name: 'isavailable', type: 'Integer (0 atau 1) / Boolean', required: false, default: '1', desc: 'Status ketersediaan (1: Tersedia, 0: Habis).' }
];

const emit = defineEmits(['notify', 'update-count']);

const products = ref([]);
const loading = ref(false);
const submitting = ref(false);
const deleting = ref(false);

const searchQuery = ref('');
const availabilityFilter = ref('');

const pagination = reactive({
  page: 1,
  limit: 10,
  total: 0,
  totalPages: 1
});

// Modal state
const showModal = ref(false);
const isEdit = ref(false);
const form = reactive({
  id: null,
  productname: '',
  price: 0,
  stok: 0,
  isavailable: true
});

// Delete confirm state
const showDeleteModal = ref(false);
const selectedItem = ref(null);

let searchTimeout = null;

function handleSearch() {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    fetchProducts(1);
  }, 300);
}

function resetFilters() {
  searchQuery.value = '';
  availabilityFilter.value = '';
  fetchProducts(1);
}

function handlePageChange(newPage) {
  fetchProducts(newPage);
}

function handleLimitChange(newLimit) {
  pagination.limit = newLimit;
  fetchProducts(1);
}

async function fetchProducts(page = pagination.page) {
  loading.value = true;
  try {
    const res = await api.getProducts({
      page,
      limit: pagination.limit,
      search: searchQuery.value,
      isavailable: availabilityFilter.value
    });

    if (res.pagination) {
      products.value = res.data;
      pagination.page = res.pagination.page;
      pagination.total = res.pagination.total;
      pagination.totalPages = res.pagination.totalPages;
    } else {
      products.value = res.data;
      pagination.total = res.total || res.data.length;
      pagination.totalPages = 1;
    }

    emit('update-count', pagination.total);
  } catch (err) {
    emit('notify', { type: 'error', message: `Gagal memuat produk: ${err.message}` });
  } finally {
    loading.value = false;
  }
}

function openAddModal() {
  isEdit.value = false;
  form.id = null;
  form.productname = '';
  form.price = 0;
  form.stok = 0;
  form.isavailable = true;
  showModal.value = true;
}

function openEditModal(item) {
  isEdit.value = true;
  form.id = item.id;
  form.productname = item.productname;
  form.price = item.price;
  form.stok = item.stok;
  form.isavailable = Boolean(item.isavailable);
  showModal.value = true;
}

function closeModal() {
  showModal.value = false;
}

async function saveProduct() {
  if (!form.productname.trim()) {
    emit('notify', { type: 'error', message: 'Nama produk wajib diisi' });
    return;
  }

  submitting.value = true;
  try {
    const payload = {
      productname: form.productname.trim(),
      price: Number(form.price),
      stok: Number(form.stok),
      isavailable: form.isavailable ? 1 : 0
    };

    if (isEdit.value) {
      await api.updateProduct(form.id, payload);
      emit('notify', { type: 'success', message: `Produk #${form.id} berhasil diperbarui!` });
    } else {
      await api.createProduct(payload);
      emit('notify', { type: 'success', message: 'Produk baru berhasil ditambahkan!' });
    }

    closeModal();
    fetchProducts();
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
    await api.deleteProduct(selectedItem.value.id);
    emit('notify', { type: 'success', message: `Produk #${selectedItem.value.id} berhasil dihapus!` });
    showDeleteModal.value = false;
    fetchProducts();
  } catch (err) {
    emit('notify', { type: 'error', message: `Gagal menghapus: ${err.message}` });
  } finally {
    deleting.value = false;
  }
}

function formatCurrency(val) {
  return new Intl.NumberFormat('id-ID', {
    style: 'currency',
    currency: 'IDR',
    maximumFractionDigits: 0
  }).format(val);
}

function exportCSV() {
  if (!products.value.length) return;
  const headers = ['ID', 'Product Name', 'Price', 'Stok', 'Is Available', 'Created At'];
  const rows = products.value.map(p => [
    p.id,
    `"${(p.productname || '').replace(/"/g, '""')}"`,
    p.price,
    p.stok,
    p.isavailable ? 'Yes' : 'No',
    `"${p.created_at || ''}"`
  ]);

  const csvContent = 'data:text/csv;charset=utf-8,' + [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
  const encodedUri = encodeURI(csvContent);
  const link = document.createElement('a');
  link.setAttribute('href', encodedUri);
  link.setAttribute('download', `products_${new Date().toISOString().slice(0, 10)}.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

onMounted(() => {
  fetchProducts(1);
});

defineExpose({
  fetchProducts
});
</script>
