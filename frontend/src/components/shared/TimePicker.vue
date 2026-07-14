<template>
  <div class="tp-root" ref="rootRef">
    <!-- Trigger Button (looks like an input) -->
    <button
      type="button"
      class="tp-trigger"
      :class="{ 'tp-open': isOpen, 'tp-disabled': disabled }"
      :disabled="disabled"
      @click="toggle"
    >
      <span class="tp-icon-wrap">
        <i class="bi bi-clock tp-icon"></i>
      </span>
      <span class="tp-value" :class="{ 'tp-placeholder': !modelValue }">
        {{ modelValue || 'Chọn giờ' }}
      </span>
      <i class="bi bi-chevron-down tp-caret" :class="{ 'tp-caret-open': isOpen }"></i>
    </button>

    <!-- Dropdown Panel -->
    <Transition name="tp-drop">
      <div v-if="isOpen" class="tp-panel">
        <div class="tp-grid">
          <button
            v-for="slot in slots"
            :key="slot"
            type="button"
            class="tp-slot"
            :class="{
              'tp-slot-selected': modelValue === slot,
              'tp-slot-hour': slot.endsWith(':00'),
              'tp-slot-half': slot.endsWith(':30'),
            }"
            @click="pick(slot)"
          >
            <i v-if="modelValue === slot" class="bi bi-check2 tp-check"></i>
            {{ slot }}
          </button>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue';

const props = defineProps<{
  modelValue: string;
  slots: string[];
  disabled?: boolean;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', val: string): void;
  (e: 'change', val: string): void;
}>();

const isOpen = ref(false);
const rootRef = ref<HTMLElement | null>(null);

const toggle = () => {
  if (!props.disabled) isOpen.value = !isOpen.value;
};

const pick = (slot: string) => {
  emit('update:modelValue', slot);
  emit('change', slot);
  isOpen.value = false;
};

const onClickOutside = (e: MouseEvent) => {
  if (rootRef.value && !rootRef.value.contains(e.target as Node)) {
    isOpen.value = false;
  }
};

onMounted(() => document.addEventListener('mousedown', onClickOutside));
onBeforeUnmount(() => document.removeEventListener('mousedown', onClickOutside));
</script>

<style scoped>
/* === Root === */
.tp-root {
  position: relative;
  width: 100%;
  user-select: none;
}

/* === Trigger === */
.tp-trigger {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 9px 12px;
  background: rgba(255, 255, 255, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.8);
  border-radius: 10px;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.9rem;
  color: #1e293b;
  backdrop-filter: blur(5px);
  transition: all 0.25s ease;
  text-align: left;
}
.tp-trigger:hover:not(.tp-disabled) {
  background: rgba(255, 255, 255, 0.9);
  border-color: rgba(220, 53, 69, 0.35);
  box-shadow: 0 2px 8px rgba(220, 53, 69, 0.08);
}
.tp-trigger.tp-open {
  background: rgba(255, 255, 255, 0.95);
  border-color: #dc3545;
  box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.15);
}
.tp-trigger.tp-disabled {
  opacity: 0.65;
  cursor: not-allowed;
  background: #f8f9fa;
}

.tp-icon-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  flex-shrink: 0;
}
.tp-icon {
  color: #9ca3af;
  font-size: 0.85rem;
  transition: color 0.2s;
}
.tp-open .tp-icon { color: #dc3545; }

.tp-value {
  flex: 1;
  font-size: 0.9rem;
}
.tp-placeholder { color: #9ca3af; font-weight: 400; }

.tp-caret {
  font-size: 0.75rem;
  color: #9ca3af;
  transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1), color 0.2s;
}
.tp-caret-open {
  transform: rotate(180deg);
  color: #dc3545;
}

/* === Dropdown Panel === */
.tp-panel {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  width: 100%;
  min-width: 180px;
  background: rgba(255, 255, 255, 0.97);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(220, 53, 69, 0.15);
  border-radius: 14px;
  box-shadow:
    0 10px 40px rgba(0, 0, 0, 0.12),
    0 4px 12px rgba(220, 53, 69, 0.08);
  z-index: 9999;
  overflow: hidden;
}

/* === Time Grid === */
.tp-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
  padding: 10px;
  max-height: 240px;
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: rgba(220, 53, 69, 0.3) transparent;
}
.tp-grid::-webkit-scrollbar { width: 4px; }
.tp-grid::-webkit-scrollbar-thumb {
  background: rgba(220, 53, 69, 0.25);
  border-radius: 2px;
}

/* === Slot Buttons === */
.tp-slot {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 7px 4px;
  border: 1px solid transparent;
  border-radius: 8px;
  background: transparent;
  font-size: 0.82rem;
  font-weight: 600;
  color: #374151;
  cursor: pointer;
  transition: all 0.15s ease;
  position: relative;
  overflow: hidden;
}
.tp-slot:hover {
  background: rgba(220, 53, 69, 0.07);
  border-color: rgba(220, 53, 69, 0.2);
  color: #dc3545;
  transform: translateY(-1px);
}

/* :00 = darker tint; :30 = lighter */
.tp-slot-hour { color: #1e293b; }
.tp-slot-half { color: #64748b; }

.tp-slot-selected {
  background: linear-gradient(135deg, #dc3545, #c0392b) !important;
  color: #fff !important;
  border-color: transparent !important;
  box-shadow: 0 3px 10px rgba(220, 53, 69, 0.35);
  transform: translateY(-1px);
}
.tp-check { font-size: 0.7rem; }

/* === Animation === */
.tp-drop-enter-active {
  animation: tpSlideIn 0.22s cubic-bezier(0.16, 1, 0.3, 1);
}
.tp-drop-leave-active {
  animation: tpSlideOut 0.15s ease-in forwards;
}
@keyframes tpSlideIn {
  from { opacity: 0; transform: translateY(-8px) scale(0.97); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}
@keyframes tpSlideOut {
  from { opacity: 1; transform: translateY(0) scale(1); }
  to   { opacity: 0; transform: translateY(-6px) scale(0.97); }
}
</style>
