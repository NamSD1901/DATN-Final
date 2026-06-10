# 🗃️ State Management (Pinia Store) - Receptionist Portal & Queue Management

## 1. Quản lý trạng thái Kanban & Tích hợp SignalR Real-time

Quản trị hàng đợi y tế yêu cầu tính đồng bộ tức thời giữa các quầy tiếp nhận và phòng bác sĩ. Pinia Store (`receptionistQueueStore`) quản trị trạng thái hàng chờ thông qua:

1. **Kanban Reactive Data Source:** Lưu trữ toàn bộ danh sách hàng chờ khám trong ngày (`queueItems`).
2. **SignalR Connection State:** Lưu giữ đối tượng kết nối WebSocket Hub (`hubConnection`) để điều phối lắng nghe sự kiện phát sóng tự động từ máy chủ API.
3. **Quick Cache:** Cache danh mục bác sĩ trực ca, phòng khám và các lịch hẹn đang chờ duyệt (`pendingAppointments`).
4. **Interaction Indicators:** Các cờ `loading`, `submitting`, trạng thái kết nối `signalrConnected`.

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là đặc tả mã nguồn đầy đủ cho `receptionistQueueStore` viết bằng TypeScript, có tích hợp kết nối SignalR Client:

```typescript
import { defineStore } from 'pinia';
import * as signalR from '@microsoft/signalr';
import api from '@/services/api';

// Định nghĩa Interface cho một ca trong hàng đợi khám
export interface QueueItem {
  appointmentId: string;
  queueNumber: string;
  petId: number;
  petName: string;
  petSpecies: 'Dog' | 'Cat';
  customerName: string;
  doctorName: string;
  roomName: string;
  status: 'waiting' | 'in_progress' | 'completed';
  checkInTime: string; // ISO 8601
}

// Cấu trúc DTO đăng ký khách vãng lai (Walk-in)
export interface QuickWalkInRequest {
  customerName: string;
  customerPhone: string;
  petName: string;
  species: 'Dog' | 'Cat';
  breed: string;
  serviceId: number;
  assignedDoctorId?: string;
  clinicRoom: string;
  symptom: string;
}

interface QueueState {
  queueItems: QueueItem[];
  loading: boolean;
  submitting: boolean;
  error: string | null;
  hubConnection: signalR.HubConnection | null;
  signalrConnected: boolean;
}

export const useReceptionistQueueStore = defineStore('receptionistQueue', {
  state: (): QueueState => ({
    queueItems: [],
    loading: false,
    submitting: false,
    error: null,
    hubConnection: null,
    signalrConnected: false
  }),

  getters: {
    // Lọc danh sách thú cưng đang chờ khám (Cột 1 Kanban)
    waitingItems(state): QueueItem[] {
      return state.queueItems
        .filter(item => item.status === 'waiting')
        .sort((a, b) => a.queueNumber.localeCompare(b.queueNumber));
    },

    // Lọc danh sách thú cưng đang trong phòng khám (Cột 2 Kanban)
    inProgressItems(state): QueueItem[] {
      return state.queueItems.filter(item => item.status === 'in_progress');
    },

    // Lọc danh sách thú cưng đã khám xong (Cột 3 Kanban)
    completedItems(state): QueueItem[] {
      return state.queueItems.filter(item => item.status === 'completed');
    }
  },

  actions: {
    /**
     * Khởi tạo kết nối SignalR Hub lắng nghe cập nhật Real-time từ Server
     */
    async initializeSignalR() {
      if (this.hubConnection) return;

      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/queue', {
          accessTokenFactory: () => localStorage.getItem('token') || ''
        })
        .withAutomaticReconnect()
        .build();

      // Lắng nghe sự kiện phát sóng hàng đợi thay đổi
      this.hubConnection.on('QueueUpdated', (updatedItem: QueueItem) => {
        const index = this.queueItems.findIndex(q => q.appointmentId === updatedItem.appointmentId);
        if (index !== -1) {
          // Cập nhật in-place bản ghi đang có
          this.queueItems[index] = updatedItem;
        } else {
          // Thêm mới nếu là ca check-in mới tinh
          this.queueItems.push(updatedItem);
        }
      });

      try {
        await this.hubConnection.start();
        this.signalrConnected = true;
        console.log('SignalR Queue Hub connected successfully.');
      } catch (err) {
        console.error('SignalR connection failed:', err);
        this.signalrConnected = false;
      }
    },

    /**
     * Tải danh sách hàng chờ khám trong ngày hôm nay (RESTful API fallback)
     */
    async fetchTodayQueue() {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.get<QueueItem[]>('/receptionist/queue');
        this.queueItems = response.data;
      } catch (err: any) {
        this.error = 'Không thể tải danh sách hàng đợi hôm nay.';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Check-in lịch hẹn đã xác nhận và xếp vào hàng chờ
     */
    async checkInAppointment(appointmentId: string, roomName: string): Promise<boolean> {
      this.submitting = true;
      try {
        const response = await api.post<QueueItem>('/receptionist/check-in', {
          appointmentId,
          clinicRoom: roomName
        });
        
        // Cập nhật local state trước khi SignalR broadcast
        const index = this.queueItems.findIndex(q => q.appointmentId === appointmentId);
        if (index === -1) {
          this.queueItems.push(response.data);
        }
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi xảy ra khi thực hiện check-in.';
        return false;
      } finally {
        this.submitting = false;
      }
    },

    /**
     * Cập nhật trạng thái hàng đợi khi lễ tân thực hiện kéo thả trên Kanban
     */
    async updateQueueStatus(appointmentId: string, newStatus: 'waiting' | 'in_progress' | 'completed', clinicRoom?: string) {
      // Optimistic Update: Cập nhật UI trước để tạo cảm giác phản hồi tức thì
      const originalItems = [...this.queueItems];
      const item = this.queueItems.find(q => q.appointmentId === appointmentId);
      if (item) {
        item.status = newStatus;
        if (clinicRoom) item.roomName = clinicRoom;
      }

      try {
        await api.put(`/receptionist/queue/${appointmentId}/status`, {
          status: newStatus,
          clinicRoom: clinicRoom
        });
      } catch (err: any) {
        // Rollback state nếu API báo lỗi (lỗi phân quyền/hoặc ca khám đã bị khóa)
        this.queueItems = originalItems;
        this.error = 'Không thể đồng bộ trạng thái hàng đợi lên máy chủ.';
      }
    },

    /**
     * Đăng ký nhanh khách vãng lai (Quick Walk-in)
     */
    async registerWalkIn(payload: QuickWalkInRequest): Promise<boolean> {
      this.submitting = true;
      this.error = null;
      try {
        const response = await api.post<QueueItem>('/receptionist/walk-in', payload);
        this.queueItems.push(response.data);
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Đăng ký khách vãng lai thất bại.';
        return false;
      } finally {
        this.submitting = false;
      }
    },

    /**
     * Ngắt kết nối SignalR khi rời Dashboard
     */
    async cleanup() {
      if (this.hubConnection) {
        await this.hubConnection.stop();
        this.hubConnection = null;
        this.signalrConnected = false;
      }
      this.queueItems = [];
    }
  }
});
```
