<template>
  <div class="notification-dropdown" ref="dropdownRef">
    <button class="bell-btn" @click="toggleDropdown">
      <Bell class="bell-icon" :class="{ 'ringing': unreadCount > 0 }" />
      <span v-if="unreadCount > 0" class="badge bg-danger rounded-pill pulse-badge">
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
    </button>

    <!-- Glassmorphism Dropdown Menu -->
    <transition name="dropdown-fade">
      <div v-if="isOpen" class="dropdown-menu-glass shadow-lg">
        <div class="dropdown-header d-flex justify-content-between align-items-center border-bottom pb-2 mb-2">
          <h6 class="mb-0 fw-bold text-dark">Thông báo</h6>
          <button v-if="unreadCount > 0" @click="markAllAsRead" class="btn btn-sm btn-link text-decoration-none p-0 text-primary fs-7">
            Đánh dấu đã đọc
          </button>
        </div>

        <div class="notification-list custom-scrollbar">
          <div v-if="notifications.length === 0" class="text-center py-4 text-muted">
            <BellOff class="mb-2 opacity-50" :size="24" />
            <p class="mb-0 fs-7">Không có thông báo nào</p>
          </div>

          <div v-else v-for="notif in notifications" :key="notif.id" 
               class="notification-item" :class="{ 'unread': !notif.isRead }"
               @click="handleNotificationClick(notif)">
            <div class="notification-icon-wrapper" :class="getIconClass(notif.type)">
              <component :is="getIconComponent(notif.type)" :size="16" />
            </div>
            <div class="notification-content">
              <p class="notification-title mb-1 text-dark fw-semibold">{{ notif.title }}</p>
              <p class="notification-text mb-1 text-muted fs-7">{{ notif.content }}</p>
              <small class="notification-time text-muted opacity-75">{{ formatTime(notif.createdAt) }}</small>
            </div>
            <div v-if="!notif.isRead" class="unread-dot"></div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { Bell, BellOff, Info, CalendarCheck, CalendarX, CheckCircle } from 'lucide-vue-next';
import { useNotificationStore } from '../../stores/notification.store';

const store = useNotificationStore();
const isOpen = ref(false);
const dropdownRef = ref<HTMLElement | null>(null);

const unreadCount = computed(() => store.unreadCount);
const notifications = computed(() => store.notifications);

const toggleDropdown = () => {
  isOpen.value = !isOpen.value;
  if (isOpen.value && notifications.value.length === 0) {
    store.fetchNotifications();
  }
};

const closeDropdown = (e: any) => {
  if (dropdownRef.value && !dropdownRef.value.contains(e.target)) {
    isOpen.value = false;
  }
};

const markAllAsRead = async () => {
  await store.markAllAsRead();
};

const handleNotificationClick = async (notif: any) => {
  if (!notif.isRead) {
    await store.markAsRead(notif.id);
  }
  // Thêm logic navigate nếu cần dựa vào notif.type
  isOpen.value = false;
};

const formatTime = (dateStr: string) => {
  const date = new Date(dateStr);
  const now = new Date();
  const diff = now.getTime() - date.getTime();
  
  if (diff < 60000) return 'Vừa xong';
  if (diff < 3600000) return `${Math.floor(diff / 60000)} phút trước`;
  if (diff < 86400000) return `${Math.floor(diff / 3600000)} giờ trước`;
  return date.toLocaleDateString('vi-VN');
};

const getIconComponent = (type: string) => {
  switch (type) {
    case 'AppointmentUpdate': return CalendarCheck;
    case 'AppointmentCancel': return CalendarX;
    case 'System': return Info;
    default: return CheckCircle;
  }
};

const getIconClass = (type: string) => {
  switch (type) {
    case 'AppointmentUpdate': return 'bg-primary-subtle text-primary';
    case 'AppointmentCancel': return 'bg-danger-subtle text-danger';
    case 'System': return 'bg-info-subtle text-info';
    default: return 'bg-success-subtle text-success';
  }
};

onMounted(() => {
  store.fetchUnreadCount();
  store.startHubConnection();
  document.addEventListener('click', closeDropdown);
});

onUnmounted(() => {
  document.removeEventListener('click', closeDropdown);
});
</script>

<style scoped>
.notification-dropdown {
  position: relative;
  display: flex;
  align-items: center;
}

.bell-btn {
  background: transparent;
  border: none;
  position: relative;
  padding: 8px;
  border-radius: 50%;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.bell-btn:hover {
  background: rgba(0, 0, 0, 0.05);
}

.bell-icon {
  width: 20px;
  height: 20px;
  color: var(--text-dark);
}

.bell-icon.ringing {
  animation: ring 2s infinite;
  transform-origin: top center;
}

.badge {
  position: absolute;
  top: 0px;
  right: 0px;
  font-size: 0.65rem;
  padding: 0.25em 0.4em;
  transform: translate(25%, -25%);
}

.pulse-badge {
  box-shadow: 0 0 0 0 rgba(220, 53, 69, 0.7);
  animation: pulse 2s infinite;
}

.dropdown-menu-glass {
  position: absolute;
  top: calc(100% + 10px);
  right: -10px;
  width: 380px;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 12px;
  padding: 12px;
  z-index: 1050;
  transform-origin: top right;
}

.dropdown-fade-enter-active,
.dropdown-fade-leave-active {
  transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
}

.dropdown-fade-enter-from,
.dropdown-fade-leave-to {
  opacity: 0;
  transform: scale(0.95) translateY(-10px);
}

.notification-list {
  max-height: 400px;
  overflow-y: auto;
  margin-right: -8px;
  padding-right: 8px;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.1);
  border-radius: 4px;
}

.notification-item {
  display: flex;
  gap: 12px;
  padding: 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  position: relative;
  align-items: flex-start;
}

.notification-item:hover {
  background: rgba(0, 0, 0, 0.03);
}

.notification-item.unread {
  background: var(--primary-cream);
}

.notification-item.unread:hover {
  background: rgba(245, 158, 11, 0.15); /* warning color alpha */
}

.notification-icon-wrapper {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  margin-top: 2px;
}

.notification-content {
  flex-grow: 1;
  min-width: 0;
  padding-right: 15px;
}

.notification-title {
  font-size: 0.95rem;
  color: #1f2937 !important;
}

.notification-text {
  word-wrap: break-word;
  white-space: normal;
  line-height: 1.4;
  color: #4b5563 !important;
  margin-top: 4px;
  margin-bottom: 6px !important;
}

.fs-7 {
  font-size: 0.8rem;
}

.notification-time {
  font-size: 0.75rem;
  display: block;
}

.unread-dot {
  width: 8px;
  height: 8px;
  background-color: var(--bs-primary);
  border-radius: 50%;
  position: absolute;
  top: 15px;
  right: 10px;
}

@keyframes ring {
  0% { transform: rotate(0); }
  5% { transform: rotate(15deg); }
  10% { transform: rotate(-15deg); }
  15% { transform: rotate(20deg); }
  20% { transform: rotate(-20deg); }
  25% { transform: rotate(15deg); }
  30% { transform: rotate(-15deg); }
  35% { transform: rotate(0); }
  100% { transform: rotate(0); }
}

@keyframes pulse {
  0% { transform: translate(25%, -25%) scale(0.95); box-shadow: 0 0 0 0 rgba(220, 53, 69, 0.7); }
  70% { transform: translate(25%, -25%) scale(1); box-shadow: 0 0 0 4px rgba(220, 53, 69, 0); }
  100% { transform: translate(25%, -25%) scale(0.95); box-shadow: 0 0 0 0 rgba(220, 53, 69, 0); }
}

@media (max-width: 991px) {
  .dropdown-menu-glass {
    position: fixed;
    top: 60px;
    right: 10px;
    left: 10px;
    width: auto;
  }
}
</style>
