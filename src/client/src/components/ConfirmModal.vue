<template>
  <div v-if="show" class="modal-backdrop">
    <div class="modal-dialog" style="max-width: 440px;">
      <div class="modal-header">
        <h3 class="modal-title" style="color: var(--danger);">{{ title }}</h3>
        <button
          type="button"
          class="btn btn-outline btn-icon-only"
          style="border: none; font-size: 1.25rem; line-height: 1; padding: 0.25rem 0.5rem;"
          @click="$emit('cancel')"
        >
          ✕
        </button>
      </div>

      <div class="modal-body">
        <p style="font-size: 0.925rem; color: var(--text-muted); line-height: 1.6;">
          {{ message }}
        </p>
      </div>

      <div class="modal-footer">
        <button type="button" class="btn btn-outline" @click="$emit('cancel')">
          Batal
        </button>
        <button
          type="button"
          class="btn btn-danger"
          :disabled="loading"
          @click="$emit('confirm')"
        >
          {{ loading ? 'Menghapus...' : 'Hapus Sekarang' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  show: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: 'Konfirmasi Hapus'
  },
  message: {
    type: String,
    default: 'Apakah Anda yakin ingin menghapus data ini? Tindakan ini tidak dapat dibatalkan.'
  },
  loading: {
    type: Boolean,
    default: false
  }
});

defineEmits(['confirm', 'cancel']);
</script>
