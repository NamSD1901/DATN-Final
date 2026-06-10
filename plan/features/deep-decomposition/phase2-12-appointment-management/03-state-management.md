# 🗃️ State Management (Pinia Store) - Customer Appointment Management

## 1. Quản lý trạng thái Dashboard Lịch hẹn (Dashboard State Architecture)

Bảng điều khiển quản lý lịch hẹn yêu cầu quản lý trạng thái động phục vụ phân trang, lọc theo danh mục trạng thái và cập nhật tức thời (In-place state sync) khi khách hàng thực hiện hủy lịch:

1. **Paginated Data Source:** Lưu trữ danh sách lịch hẹn hiện tại (`appointments`) cùng thông tin phân trang (tổng số bản ghi, trang hiện tại, kích thước trang).
2. **Dynamic Filter State:** Theo dõi trạng thái bộ lọc đang được chọn (`selectedStatusFilter`) để tự động kích hoạt API tải lại dữ liệu thích hợp.
3. **Selected Item Focus:** Lưu giữ lịch hẹn đang được chọn (`activeAppointmentId`) để hiển thị thông tin chi tiết trên panel bên phải (hoặc mở modal trên mobile).
4. **Interaction Flags:** Quản lý trạng thái gọi API (`loading`), trạng thái đang hủy lịch (`cancelling`), và lưu trữ thông điệp lỗi (`error`)/thành công (`successMessage`).

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là đặc tả mã nguồn đầy đủ cho `customerAppointmentsStore` viết bằng TypeScript:

```typescript
import { defineStore } from 'pinia';
import api from '@/services/api';

// Định nghĩa Interface cho Lịch hẹn chi tiết
export interface Appointment {
  id: string;
  petId: number;
  petName: string;
  petSpecies: 'Dog' | 'Cat';
  doctorId: string;
  doctorName: string;
  serviceId: number | null;
  serviceName: string;
  vaccineId: number | null;
  vaccineName: string | null;
  appointmentDate: string; // ISO 8601 UTC
  status: 'pending' | 'confirmed' | 'waiting' | 'in_progress' | 'completed' | 'cancelled';
  symptom: string;
  note: string;
  cancelledReason: string | null;
  cancelledAt: string | null;
  qrToken: string;
}

// Cấu trúc phân trang trả về từ API
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

interface CustomerAppointmentsState {
  appointments: Appointment[];
  activeAppointmentId: string | null;
  selectedStatusFilter: string; // '' đại diện cho 'Tất cả'
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  loading: boolean;
  cancelling: boolean;
  error: string | null;
  successMessage: string | null;
}

export const useCustomerAppointmentsStore = defineStore('customerAppointments', {
  state: (): CustomerAppointmentsState => ({
    appointments: [],
    activeAppointmentId: null,
    selectedStatusFilter: '',
    currentPage: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
    loading: false,
    cancelling: false,
    error: null,
    successMessage: null
  }),

  getters: {
    /**
     * Lấy thông tin lịch hẹn đang được chọn để hiển thị chi tiết
     */
    activeAppointment(state): Appointment | null {
      if (!state.activeAppointmentId) return null;
      return state.appointments.find(a => a.id === state.activeAppointmentId) || null;
    },

    /**
     * Kiểm tra danh sách lịch hẹn có đang trống không
     */
    isListEmpty(state): boolean {
      return state.appointments.length === 0;
    }
  },

  actions: {
    /**
     * Thiết lập bộ lọc trạng thái và reset trang về 1
     */
    setStatusFilter(status: string) {
      this.selectedStatusFilter = status;
      this.currentPage = 1;
      this.fetchAppointments();
    },

    /**
     * Thay đổi trang hiển thị
     */
    setPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        this.fetchAppointments();
      }
    },

    /**
     * Tải danh sách lịch hẹn từ API có lọc và phân trang
     */
    async fetchAppointments() {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.get<PaginatedResult<Appointment>>('/my-appointments', {
          params: {
            status: this.selectedStatusFilter || undefined,
            page: this.currentPage,
            pageSize: this.pageSize
          }
        });
        
        this.appointments = response.data.items;
        this.totalCount = response.data.totalCount;
        this.totalPages = response.data.totalPages;
        
        // Tự động focus vào lịch hẹn đầu tiên nếu có danh sách trên Desktop
        if (this.appointments.length > 0 && !this.activeAppointmentId) {
          this.activeAppointmentId = this.appointments[0].id;
        } else if (this.appointments.length === 0) {
          this.activeAppointmentId = null;
        }
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải danh sách lịch hẹn của bạn.';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Thực hiện hủy lịch hẹn trực tuyến
     */
    async cancelAppointment(appointmentId: string, reason: string): Promise<boolean> {
      this.cancelling = true;
      this.error = null;
      this.successMessage = null;

      try {
        const response = await api.put<{ success: boolean; message: string }>(
          `/my-appointments/${appointmentId}/cancel`,
          { reason }
        );

        if (response.data.success) {
          this.successMessage = response.data.message;
          
          // Đồng bộ state local (In-place update) thay vì tải lại toàn bộ danh sách từ DB
          const index = this.appointments.findIndex(a => a.id === appointmentId);
          if (index !== -1) {
            this.appointments[index].status = 'cancelled';
            this.appointments[index].cancelledReason = reason;
            this.appointments[index].cancelledAt = new Date().toISOString();
          }
          return true;
        }
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Có lỗi xảy ra khi yêu cầu hủy lịch hẹn.';
        return false;
      } finally {
        this.cancelling = false;
      }
    },

    /**
     * Chọn lịch hẹn để xem chi tiết
     */
    selectAppointment(id: string) {
      this.activeAppointmentId = id;
    },

    /**
     * Reset dọn dẹp bộ nhớ store khi unmount dashboard
     */
    cleanup() {
      this.appointments = [];
      this.activeAppointmentId = null;
      this.selectedStatusFilter = '';
      this.currentPage = 1;
      this.error = null;
      this.successMessage = null;
    }
  }
});
```

---

## 3. Quy tắc Đồng bộ hóa Dữ liệu (Data Synchronization Rules)

* **Tránh Over-fetching:** Khi hủy lịch thành công, hệ thống chỉ cập nhật thuộc tính `status = 'cancelled'` của phần tử trong mảng `appointments` tại Client. Tránh việc gọi lại toàn bộ API `fetchAppointments` làm lãng phí băng thông và tài nguyên CPU đĩa của máy chủ Database.
* **Tự động Focus:** Khi chuyển đổi tab bộ lọc (ví dụ từ `Đã duyệt` sang `Đã hủy`), store tự động xóa tiêu điểm `activeAppointmentId` cũ và trỏ tiêu điểm mới vào bản ghi đầu tiên của danh sách mới tải để tránh giao diện hiển thị thông tin lệch pha.
