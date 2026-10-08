<template>
  <div class="toast-container">
    <div
      v-for="toast in toasts"
      :key="toast.id"
      class="toast"
      :class="{
        'toast-success': toast.type === 'success',
        'toast-error': toast.type === 'error'
      }"
    >
      <span style="font-size: 1.1rem;">
        {{ toast.type === 'success' ? '✅' : '❌' }}
      </span>
      <div style="flex: 1;">
        <div style="font-weight: 600; font-size: 0.875rem;">
          {{ toast.type === 'success' ? 'Berhasil' : 'Pemberitahuan' }}
        </div>
        <div style="font-size: 0.8rem; color: var(--text-muted); margin-top: 2px;">
          {{ toast.message }}
        </div>
      </div>
      <button
        type="button"
        style="background: none; border: none; cursor: pointer; color: var(--text-subtle); font-size: 1rem;"
        @click="removeToast(toast.id)"
      >
        ✕
      </button>
    </div>
  </div>
</template>

<script setup>
defineProps({
  toasts: {
    type: Array,
    default: () => []
  }
});

const emit = defineEmits(['remove']);

function removeToast(id) {
  emit('remove', id);
}
</script>
