<template>
  <!--
    UiSwitch — Premium Toggle Switch Component
    Hỗ trợ v-model, WCAG compliant (Keyboard toggle bằng phím Space/Enter)
  -->
  <button
    type="button"
    role="switch"
    :aria-checked="modelValue"
    :disabled="disabled"
    class="ui-switch"
    :class="{ 'is-checked': modelValue, 'is-disabled': disabled }"
    @click="toggle"
    @keydown.space.prevent="toggle"
  >
    <span class="ui-switch-track">
      <span class="ui-switch-thumb">
        <!-- Icon nhỏ bên trong thumb (tuỳ chọn) -->
        <i v-if="modelValue" class="bi bi-check check-icon"></i>
        <i v-else class="bi bi-x uncheck-icon"></i>
      </span>
    </span>
  </button>
</template>

<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    modelValue: boolean;
    disabled?: boolean;
  }>(),
  {
    disabled: false,
  }
);

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void;
  (e: 'change', value: boolean): void;
}>();

const toggle = () => {
  if (props.disabled) return;
  const newValue = !props.modelValue;
  emit('update:modelValue', newValue);
  emit('change', newValue);
};
</script>

<style scoped>
/* ── Container (button reset) ── */
.ui-switch {
  appearance: none;
  background: transparent;
  border: none;
  padding: 0;
  margin: 0;
  cursor: pointer;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  transition: all 0.2s ease;
  position: relative;
  /* Touch target min size */
  min-width: 44px;
  min-height: 44px;
  justify-content: center;
}

.ui-switch.is-disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

/* ── Focus Outline (Accessibility) ── */
.ui-switch:focus-visible {
  outline: 2px solid #f59e0b;
  outline-offset: 2px;
}

/* ── Track (Nền của switch) ── */
.ui-switch-track {
  width: 46px;
  height: 24px;
  background-color: #e2e8f0;
  border-radius: 999px;
  position: relative;
  transition: background-color 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.05);
}

.ui-switch.is-checked .ui-switch-track {
  background-color: #10b981; /* Xanh lá khi bật */
}

/* ── Thumb (Cục tròn di chuyển) ── */
.ui-switch-thumb {
  width: 20px;
  height: 20px;
  background-color: #ffffff;
  border-radius: 50%;
  position: absolute;
  top: 2px;
  left: 2px;
  transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
}

.ui-switch.is-checked .ui-switch-thumb {
  transform: translateX(22px);
}

/* ── Icons inside thumb ── */
.check-icon {
  color: #10b981;
  font-size: 14px;
  opacity: 0;
  animation: fadeIn 0.2s ease forwards 0.1s;
}

.uncheck-icon {
  color: #94a3b8;
  font-size: 14px;
  opacity: 0;
  animation: fadeIn 0.2s ease forwards 0.1s;
}

@keyframes fadeIn {
  to { opacity: 1; }
}

/* ── Hover effect ── */
.ui-switch:hover:not(.is-disabled) .ui-switch-thumb {
  box-shadow: 0 0 0 4px rgba(255, 255, 255, 0.4);
}
</style>
