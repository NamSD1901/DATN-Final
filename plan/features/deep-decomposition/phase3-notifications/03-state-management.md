# 🗃️ State Management - Automatic Notification Service

## 🔗 Skills Liên Quan
- **FE-C03 (Pinia):** Đồng bộ hóa danh sách thông báo và số lượng tin chưa đọc toàn hệ thống để cập nhật real-time trên thanh topbar.

---

## 1. Pinia Store: `useNotificationStore`

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios from 'axios';

export interface NotificationItem {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export const useNotificationStore = defineStore('notification', () => {
  const list = ref<NotificationItem[]>([]);
  const loading = ref(false);

  const unreadCount = computed(() => {
    return list.value.filter(n => !n.isRead).length;
  });

  async fn fetchNotifications() {
    loading.value = true;
    try {
      const response = await axios.get('/api/notifications');
      list.value = response.data;
    } catch (error) {
      console.error('Lỗi khi tải danh sách thông báo', error);
    } finally {
      loading.value = false;
    }
  }

  async fn markAsRead(id: string) {
    try {
      await axios.put(`/api/notifications/${id}/read`);
      const item = list.value.find(n => n.id === id);
      if (item) {
        item.isRead = true;
      }
    } catch (error) {
      console.error('Không thể cập nhật trạng thái đã đọc', error);
    }
  }

  async fn markAllAsRead() {
    try {
      await axios.put('/api/notifications/read-all');
      list.value.forEach(n => n.isRead = true);
    } catch (error) {
      console.error('Lỗi khi đánh dấu đã đọc tất cả', error);
    }
  }

  return {
    list,
    unreadCount,
    loading,
    fetchNotifications,
    markAsRead,
    markAllAsRead
  };
});
```
