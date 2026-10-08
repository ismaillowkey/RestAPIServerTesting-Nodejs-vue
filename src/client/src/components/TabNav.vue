<template>
  <nav class="tabs-wrapper" aria-label="Navigation Tabs">
    <button
      type="button"
      class="tab-btn"
      :class="{ active: currentTab === 'product' }"
      @click="$emit('update:currentTab', 'product')"
      id="tab-product"
    >
      <span>📦</span>
      <span>Product</span>
      <span class="tab-badge" v-if="counts.product !== undefined">{{ counts.product }}</span>
    </button>

    <button
      type="button"
      class="tab-btn"
      :class="{ active: currentTab === 'plan' }"
      @click="$emit('update:currentTab', 'plan')"
      id="tab-plan"
    >
      <span>📅</span>
      <span>Plan Downtime</span>
      <span class="tab-badge" v-if="counts.plan !== undefined">{{ counts.plan }}</span>
    </button>

    <button
      type="button"
      class="tab-btn"
      :class="{ active: currentTab === 'unplan' }"
      @click="$emit('update:currentTab', 'unplan')"
      id="tab-unplan"
    >
      <span>⚠️</span>
      <span>Unplan Downtime</span>
      <span class="tab-badge" v-if="counts.unplan !== undefined">{{ counts.unplan }}</span>
    </button>

    <button
      type="button"
      class="tab-btn"
      :class="{ active: currentTab === 'docs' }"
      @click="$emit('update:currentTab', 'docs')"
      id="tab-docs"
      style="margin-left: auto;"
    >
      <span>📖</span>
      <span>REST API Docs</span>
      <span class="tab-badge" style="background: #e0e7ff; color: #4338ca;">Swagger</span>
    </button>
  </nav>
</template>

<script setup>
defineProps({
  currentTab: {
    type: String,
    required: true
  },
  counts: {
    type: Object,
    default: () => ({ product: 0, plan: 0, unplan: 0 })
  }
});

defineEmits(['update:currentTab']);
</script>
