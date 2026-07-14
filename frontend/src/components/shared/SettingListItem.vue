<template>
  <!--
    SettingListItem — Component tái sử dụng cho các dòng cài đặt trong Settings.
    Layout: Icon + Title/Desc (left) | Control slot (right)
    Tuân thủ WCAG 2.1: Touch target min 44px, Keyboard accessible.
  -->
  <div class="setting-list-item" :class="{ 'item-divider': divider, 'item-danger': danger }">
    <!-- Left: Icon + Text -->
    <div class="item-text-group">
      <div v-if="icon" class="item-icon" :class="{ 'icon-danger': danger }">
        <i :class="icon"></i>
      </div>
      <div class="item-labels">
        <span class="item-title" :class="{ 'text-danger': danger }">{{ title }}</span>
        <span v-if="description" class="item-description">{{ description }}</span>
      </div>
    </div>

    <!-- Right: Control slot (Switch, Radio, Button...) -->
    <div class="item-control">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    title: string;
    description?: string;
    icon?: string;
    divider?: boolean; // Có đường kẻ dưới (separator)
    danger?: boolean;  // Chế độ nguy hiểm (màu đỏ - Danger Zone)
  }>(),
  {
    description: '',
    icon: '',
    divider: true,
    danger: false,
  }
);
</script>

<style scoped>
.setting-list-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 0;
  min-height: 44px; /* WCAG Touch Target */
}

/* Đường kẻ phân cách (trừ item cuối cùng) */
.setting-list-item.item-divider {
  border-bottom: 1px solid rgba(226, 232, 240, 0.8);
}

/* Ẩn border khi là item cuối */
.setting-list-item.item-divider:last-child {
  border-bottom: none;
}

/* Left group: icon + label block */
.item-text-group {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  flex: 1;
  min-width: 0; /* Cho phép text truncate */
}

.item-icon {
  width: 36px;
  height: 36px;
  background: rgba(245, 158, 11, 0.1);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
  color: #d97706;
  flex-shrink: 0;
}

.item-icon.icon-danger {
  background: rgba(239, 68, 68, 0.08);
  color: #ef4444;
}

.item-labels {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  min-width: 0;
}

.item-title {
  font-size: 0.9rem;
  font-weight: 500;
  color: #1e293b;
  line-height: 1.3;
}

.item-description {
  font-size: 0.775rem;
  color: #64748b;
  line-height: 1.4;
}

/* Danger state */
.item-danger .item-title {
  color: #ef4444;
}

/* Right control slot */
.item-control {
  flex-shrink: 0;
}

/* Responsive: trên mobile icon ẩn đi để tiết kiệm không gian */
@media (max-width: 575.98px) {
  .item-icon {
    display: none;
  }
}
</style>
