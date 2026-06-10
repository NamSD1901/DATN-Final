# 🗃️ State Management (Pinia Store) - Online Examination Booking

## 1. Quản lý trạng thái Biểu mẫu đa bước (Wizard State Architecture)
Đặt lịch khám bệnh y tế trực tuyến yêu cầu thu thập nhiều dữ liệu từ nhiều bước khác nhau trước khi đóng gói gửi đi. Pinia Store (`bookingStore`) quản lý các biến trạng thái:
1.  **Lưu trữ dữ liệu biểu mẫu tạm thời:** Theo dõi đối tượng `formData` qua từng bước (chọn pet, chọn dịch vụ, chọn giờ) mà không bị mất dữ liệu khi người dùng bấm "Quay lại" bước trước để chỉnh sửa.
2.  **Đo lường tiến trình (Step State):** Lưu giữ trạng thái bước hiện tại (`step` từ 1 đến 5).
3.  **Lưu cache danh mục y tế:** Chứa danh sách các dịch vụ khám bệnh (`services`) và bác sĩ trực ca (`doctors`) tải từ API về để làm nguồn dữ liệu lựa chọn trên form.

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là mã nguồn đặc tả đầy đủ cho `bookingStore` viết bằng TypeScript:

```typescript
import { defineStore } from 'pinia';
import api from '@/services/api';

// Định nghĩa Interface cho dữ liệu Dịch vụ y tế
export interface ClinicService {
  id: number;
  name: string;
  price: number;
}

// Định nghĩa Interface cho Bác sĩ thú y
export interface Doctor {
  id: string;
  fullName: string;
  specialization?: string;
  avatar?: string;
}

// Cấu trúc Dữ liệu biểu mẫu đặt lịch hẹn
export interface BookingFormData {
  petId: number | null;
  doctorId: string | null; // null đại diện cho "Bất kỳ bác sĩ"
  serviceId: number | null;
  appointmentDate: string | null; // YYYY-MM-DDTHH:mm:ssZ
  symptom: string;
  note: string;
}

interface BookingState {
  step: number; // 1 - 5
  formData: BookingFormData;
  services: ClinicService[];
  doctors: Doctor[];
  loading: boolean;
  booking: boolean;
  error: string | null;
  successMessage: string | null;
}

export const useBookingStore = defineStore('booking', {
  state: (): BookingState => ({
    step: 1,
    formData: {
      petId: null,
      doctorId: null,
      serviceId: null,
      appointmentDate: null,
      symptom: '',
      note: ''
    },
    services: [],
    doctors: [],
    loading: false,
    booking: false,
    error: null,
    successMessage: null
  }),

  getters: {
    /**
     * Lấy thông tin dịch vụ được chọn để hiển thị ở bước Xác nhận
     */
    selectedService(state): ClinicService | null {
      if (!state.formData.serviceId) return null;
      return state.services.find(s => s.id === state.formData.serviceId) || null;
    },

    /**
     * Lấy thông tin bác sĩ được chọn
     */
    selectedDoctor(state): Doctor | null {
      if (!state.formData.doctorId) return null;
      return state.doctors.find(d => d.id === state.formData.doctorId) || null;
    }
  },

  actions: {
    /**
     * Di chuyển tiến trình biểu mẫu
     */
    nextStep() {
      if (this.step < 5) this.step++;
    },

    prevStep() {
      if (this.step > 1) this.step--;
    },

    /**
     * Tải danh mục dịch vụ từ API phục vụ lựa chọn bước 2
     */
    async fetchServices() {
      this.loading = true;
      try {
        const response = await api.get<ClinicService[]>('/my-appointments/services');
        this.services = response.data;
      } catch (err: any) {
        this.error = 'Không thể tải danh sách dịch vụ y tế.';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Tải danh sách bác sĩ thú y trực ca phục vụ lựa chọn bước 3
     */
    async fetchDoctors() {
      this.loading = true;
      try {
        // Tái sử dụng API Doctors công khai
        const response = await api.get<Doctor[]>('/doctors');
        this.doctors = response.data;
      } catch (err: any) {
        this.error = 'Không thể tải danh sách bác sĩ.';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Gửi request đặt lịch khám lên server
     */
    async submitBooking(): Promise<boolean> {
      this.booking = true;
      this.error = null;
      this.successMessage = null;

      try {
        const response = await api.post<{
          success: boolean;
          message: string;
          id: number;
        }>('/my-appointments', this.formData);

        if (response.data.success) {
          this.successMessage = response.data.message;
          this.resetBookingForm();
          return true;
        }
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Có lỗi xảy ra khi đặt lịch hẹn khám.';
        return false;
      } finally {
        this.booking = false;
      }
    },

    /**
     * Xóa sạch dữ liệu biểu mẫu khi thành công hoặc hủy bỏ
     */
    resetBookingForm() {
      this.step = 1;
      this.formData = {
        petId: null,
        doctorId: null,
        serviceId: null,
        appointmentDate: null,
        symptom: '',
        note: ''
      };
      this.error = null;
    }
  }
});
```
