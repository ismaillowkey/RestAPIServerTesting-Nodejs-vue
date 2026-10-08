<template>
  <div
    v-if="show"
    class="modal-backdrop"
    @click.self="handleBackdropClick"
  >
    <div
      class="modal-dialog"
      :class="{ 'shake-animation': shake }"
      role="dialog"
      aria-modal="true"
    >
      <div class="modal-header">
        <h3 class="modal-title">{{ title }}</h3>
        <button
          type="button"
          class="btn btn-outline btn-icon-only"
          style="border: none; font-size: 1.25rem; line-height: 1; padding: 0.25rem 0.5rem;"
          @click="$emit('close')"
          title="Tutup (Batal)"
        >
          ✕
        </button>
      </div>

      <div class="modal-body">
        <slot></slot>
      </div>

      <div v-if="$slots.footer" class="modal-footer">
        <slot name="footer"></slot>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: 'Form'
  },
  closeOnBackdrop: {
    type: Boolean,
    default: false // Default false: klik di luar TIDAK akan menutup modal agar data form aman
  }
});

const emit = defineEmits(['close']);

const shake = ref(false);

function handleBackdropClick() {
  if (props.closeOnBackdrop) {
    emit('close');
  } else {
    // Animasi getar halus untuk memberi tahu user bahwa popup tidak tertutup otomatis jika klik di luar
    shake.value = true;
    setTimeout(() => {
      shake.value = false;
    }, 400);
  }
}
</script>

<style scoped>
.shake-animation {
  animation: modalShake 0.35s ease-in-out;
}

@keyframes modalShake {
  0%, 100% { transform: translateX(0); }
  20%, 60% { transform: translateX(-6px); }
  40%, 80% { transform: translateX(6px); }
}
</style>
