<template>
  <div class="pagination-container">
    <div class="pagination-info">
      <span v-if="total > 0">
        Menampilkan <strong>{{ startItem }}</strong> - <strong>{{ endItem }}</strong> dari <strong>{{ total }}</strong> data
      </span>
      <span v-else>Tidak ada data</span>
    </div>

    <div style="display: flex; align-items: center; gap: 1rem; flex-wrap: wrap;">
      <!-- Page Limit Selector -->
      <div style="display: flex; align-items: center; gap: 0.5rem; font-size: 0.825rem; color: var(--text-muted);">
        <span>Baris:</span>
        <select
          :value="limit"
          class="form-control form-select"
          style="width: auto; padding: 0.25rem 2rem 0.25rem 0.65rem; font-size: 0.825rem; height: 32px;"
          @change="$emit('update:limit', Number($event.target.value))"
        >
          <option :value="5">5</option>
          <option :value="10">10</option>
          <option :value="20">20</option>
          <option :value="50">50</option>
        </select>
      </div>

      <!-- Pagination Buttons -->
      <div class="pagination-controls" v-if="totalPages > 1">
        <button
          type="button"
          class="page-btn"
          :disabled="page <= 1"
          @click="$emit('update:page', page - 1)"
          title="Halaman Sebelumnya"
        >
          &lt;
        </button>

        <button
          v-for="p in visiblePages"
          :key="p"
          type="button"
          class="page-btn"
          :class="{ active: p === page }"
          @click="$emit('update:page', p)"
        >
          {{ p }}
        </button>

        <button
          type="button"
          class="page-btn"
          :disabled="page >= totalPages"
          @click="$emit('update:page', page + 1)"
          title="Halaman Selanjutnya"
        >
          &gt;
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  page: {
    type: Number,
    default: 1
  },
  limit: {
    type: Number,
    default: 10
  },
  total: {
    type: Number,
    default: 0
  },
  totalPages: {
    type: Number,
    default: 1
  }
});

defineEmits(['update:page', 'update:limit']);

const startItem = computed(() => {
  if (props.total === 0) return 0;
  return (props.page - 1) * props.limit + 1;
});

const endItem = computed(() => {
  return Math.min(props.page * props.limit, props.total);
});

const visiblePages = computed(() => {
  const pages = [];
  const maxButtons = 5;
  let start = Math.max(1, props.page - Math.floor(maxButtons / 2));
  let end = Math.min(props.totalPages, start + maxButtons - 1);

  if (end - start + 1 < maxButtons) {
    start = Math.max(1, end - maxButtons + 1);
  }

  for (let i = start; i <= end; i++) {
    pages.push(i);
  }
  return pages;
});
</script>
