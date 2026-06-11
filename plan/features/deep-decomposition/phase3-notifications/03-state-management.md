# 03. State Management (Pinia Store) - Automatic Notification Service

Tài liệu thiết kế Vue 3 Pinia Store sử dụng TypeScript cho phân hệ Nhắc lịch tiêm phòng & Thông báo tự động.

---

## 1. Vai trò của Pinia Store trong Phân hệ

Pinia Store `useNotificationStore` chịu trách nhiệm quản lý mảng thông báo in-app hiện có của người dùng, lưu giữ đếm số lượng tin chưa đọc để hiển thị Badge số màu đỏ trên thanh điều hướng, và trực tiếp khởi tạo kết nối **SignalR WebSocket Connection** sang API Gateway để lắng nghe các sự kiện đẩy tin nhắn thời gian thực từ Backend.

---

## 2. Mã nguồn TypeScript hoàn chỉnh cho Pinia Store

Dưới đây là cài đặt chi tiết của file `useNotificationStore.ts` triển khai tại thư mục `frontend/src/stores/useNotificationStore.ts`:

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';
import * as signalR from '@microsoft/signalr';

// Định nghĩa cấu trúc thông báo nhận từ API
export interface NotificationItem {
  id: string;
  title: string;
  content: string;
  isRead: boolean;
  type: 'AppointmentUpdate' | 'VaccineReminder' | 'SystemAlert';
  createdAt: string;
}

interface NotificationState {
  notifications: NotificationItem[];
  connection: signalR.HubConnection | null;
  isLoading: boolean;
  error: string | null;
  bellRinging: boolean; // Cờ kích hoạt hoạt ảnh rung chuông
}

export const useNotificationStore = defineStore('notification', {
  state: (): NotificationState => ({
    notifications: [],
    connection: null,
    isLoading: false,
    error: null,
    bellRinging: false
  }),

  getters: {
    // Đếm số lượng thông báo chưa đọc phục vụ hiển thị Badge số đỏ
    unreadCount(state): number {
      return state.notifications.filter(n => !n.isRead).length;
    },

    // Lấy danh sách thông báo sắp xếp theo thời gian mới nhất xếp đầu
    sortedNotifications(state): NotificationItem[] {
      return [...state.notifications].sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );
    }
  },

  actions: {
    // 1. Tải danh sách thông báo in-app từ database
    async fetchNotifications() {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await axios.get<NotificationItem[]>('/api/customer/notifications');
        this.notifications = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải hộp thư thông báo.';
        console.error(err);
      } finally {
        this.isLoading = false;
      }
    },

    // 2. Đánh dấu đã đọc một thông báo cụ thể
    async markAsRead(notificationId: string) {
      // Optimistic Update: Cập nhật nhanh trên UI trước khi API hoàn tất
      const index = this.notifications.findIndex(n => n.id === notificationId);
      if (index !== -1 && !this.notifications[index].isRead) {
        this.notifications[index].isRead = true;
      }

      try {
        await axios.put(`/api/customer/notifications/${notificationId}/read`);
      } catch (err: any) {
        console.error('Lỗi khi đánh dấu đã đọc thông báo:', err);
        // Rollback lại UI nếu API thất bại
        if (index !== -1) {
          this.notifications[index].isRead = false;
        }
      }
    },

    // 3. Đánh dấu đã đọc tất cả thông báo
    async markAllAsRead() {
      // Tạm thời chuyển tất cả chưa đọc thành đã đọc ở Client
      this.notifications.forEach(n => n.isRead = true);

      try {
        await axios.put('/api/customer/notifications/read-all');
      } catch (err: any) {
        console.error('Lỗi khi đánh dấu đọc tất cả:', err);
        // Tải lại dữ liệu gốc từ DB nếu lỗi
        this.fetchNotifications();
      }
    },

    // 4. Khởi tạo kết nối SignalR WebSocket
    initializeSignalRConnection(token: string) {
      if (this.connection) return; // Tránh tạo trùng lặp kết nối

      // Tạo đối tượng HubConnection
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/notifications', {
          accessTokenFactory: () => token, // Gửi kèm JWT token để xác thực
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets // Ép sử dụng WebSocket
        })
        .withAutomaticReconnect() // Tự động kết nối lại khi mất mạng
        .build();

      // Lắng nghe sự kiện "ReceiveNotification" đẩy từ Backend Hub
      this.connection.on('ReceiveNotification', (newNotif: NotificationItem) => {
        // Thêm thông báo mới vào đầu mảng danh sách
        this.notifications.unshift(newNotif);
        
        // Kích hoạt hiệu ứng rung lắc chuông UI
        this.bellRinging = true;
        setTimeout(() => {
          this.bellRinging = false;
        }, 800); // Tắt cờ rung chuông sau khi hoạt ảnh kết thúc
      });

      // Bắt đầu kết nối
      this.connection.start()
        .then(() => console.log('Đã kết nối thành công SignalR WebSockets.'))
        .catch(err => console.error('Lỗi khởi tạo kết nối SignalR:', err));
    },

    // 5. Ngắt kết nối SignalR khi người dùng đăng xuất
    terminateSignalRConnection() {
      if (this.connection) {
        this.connection.stop();
        this.connection = null;
        this.notifications = [];
        console.log('Đã đóng kết nối SignalR WebSockets.');
      }
    }
  }
});
```
