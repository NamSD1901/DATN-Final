# 🗃️ State Management (Pinia Store) - Online Vaccination Booking

## 1. Quản lý trạng thái Đặt lịch Tiêm chủng (Vaccination Booking State Architecture)

Tính năng đặt lịch tiêm chủng trực tuyến yêu cầu xử lý trạng thái phức tạp hơn đặt lịch khám thông thường do phải liên kết phác đồ y khoa của từng loài thú cưng và kiểm tra lịch sử tiêm phòng. Pinia Store (`vaccinationBookingStore`) thực hiện các nhiệm vụ quản trị sau:

1. **Wizard Multi-step Form State:** Lưu trữ biểu mẫu đa bước (bước 1: chọn thú cưng, bước 2: chọn loại vắc-xin khả dụng, bước 3: chọn thời gian & bác sĩ, bước 4: xác nhận & kiểm tra cảnh báo y khoa).
2. **Dynamic Vaccine Stock & Spec Cache:** Lưu trữ danh sách vắc-xin tải từ kho dược (`vaccines`) đảm bảo còn số lượng tồn kho khả dụng (>0) và lọc trực tiếp theo loài thú cưng (chó/mèo) của thú cưng được chọn.
3. **Medical Safety Verification State:** Lưu lịch sử tiêm chủng (`vaccinationHistory`) của thú cưng và lưu giữ cảnh báo y tế (`intervalWarning`) nếu mũi đặt lịch mới vi phạm khoảng cách thời gian tiêm an toàn so với mũi tiêm cũ.
4. **Token & Flow Handling:** Quản lý trạng thái tải (`loading`), trạng thái gửi đơn (`submitting`), và thông điệp lỗi (`error`)/thành công (`successMessage`).

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là mã nguồn đặc tả đầy đủ cho `vaccinationBookingStore` viết bằng TypeScript:

```typescript
import { defineStore } from 'pinia';
import api from '@/services/api';

// Định nghĩa Interface cho thông tin Vắc-xin trong kho dược
export interface Vaccine {
  id: number;
  name: string;
  targetSpecies: 'Dog' | 'Cat' | 'All';
  description: string;
  price: number;
  stockQuantity: number; // Tồn kho thực tế tại phòng khám
  minAgeWeeks: number; // Tuổi tối thiểu của thú cưng để tiêm (tuần)
  intervalDays: number; // Khoảng cách khuyến nghị giữa các mũi tiêm (ngày)
}

// Định nghĩa Interface cho bản ghi lịch sử tiêm chủng
export interface VaccinationRecord {
  id: number;
  petId: number;
  vaccineId: number;
  vaccineName: string;
  vaccinatedDate: string; // YYYY-MM-DD
  nextDueDate: string; // YYYY-MM-DD
  notes?: string;
}

// Dữ liệu kiểm tra phác đồ tiêm y tế
export interface IntervalValidationResult {
  isValid: boolean;
  lastVaccinatedDate: string | null;
  recommendedDate: string | null;
  warningMessage: string | null;
  requiresDoctorOverride: boolean; // Flag cờ cảnh báo cho Lễ tân/Bác sĩ
}

// Cấu trúc Dữ liệu biểu mẫu đặt lịch tiêm phòng
export interface VaccinationBookingFormData {
  petId: number | null;
  vaccineId: number | null;
  doctorId: string | null; // null đại diện cho "Bất kỳ bác sĩ"
  appointmentDate: string | null; // YYYY-MM-DDTHH:mm:ssZ
  symptom: string; // Mặc định là "Tiêm phòng vắc-xin định kỳ"
  note: string;
  bypassWarning: boolean; // Khách hàng chấp nhận bỏ qua cảnh báo y khoa dưới sự tư vấn
}

interface VaccinationBookingState {
  step: number; // 1 - 4
  formData: VaccinationBookingFormData;
  vaccines: Vaccine[];
  vaccinationHistory: VaccinationRecord[];
  validationResult: IntervalValidationResult | null;
  loading: boolean;
  submitting: boolean;
  checkingInterval: boolean;
  error: string | null;
  successMessage: string | null;
}

export const useVaccinationBookingStore = defineStore('vaccinationBooking', {
  state: (): VaccinationBookingState => ({
    step: 1,
    formData: {
      petId: null,
      vaccineId: null,
      doctorId: null,
      appointmentDate: null,
      symptom: 'Tiêm phòng vắc-xin định kỳ theo phác đồ',
      note: '',
      bypassWarning: false
    },
    vaccines: [],
    vaccinationHistory: [],
    validationResult: null,
    loading: false,
    submitting: false,
    checkingInterval: false,
    error: null,
    successMessage: null
  }),

  getters: {
    /**
     * Lấy thông tin vắc-xin đang được chọn
     */
    selectedVaccine(state): Vaccine | null {
      if (!state.formData.vaccineId) return null;
      return state.vaccines.find(v => v.id === state.formData.vaccineId) || null;
    },

    /**
     * Lọc danh sách vắc-xin phù hợp với thú cưng hiện tại (còn hàng và đúng chủng loại)
     */
    availableVaccines(state) {
      return (petSpecies: 'Dog' | 'Cat') => {
        return state.vaccines.filter(v => 
          (v.targetSpecies === petSpecies || v.targetSpecies === 'All') && 
          v.stockQuantity > 0
        );
      };
    }
  },

  actions: {
    nextStep() {
      if (this.step < 4) this.step++;
    },

    prevStep() {
      if (this.step > 1) this.step--;
    },

    /**
     * Tải danh mục vắc-xin còn hàng
     */
    async fetchVaccines() {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.get<Vaccine[]>('/vaccination/available-vaccines');
        this.vaccines = response.data;
      } catch (err: any) {
        this.error = 'Không thể tải danh sách vắc-xin khả dụng từ kho.';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Tải lịch sử tiêm phòng của thú cưng
     */
    async fetchPetVaccinationHistory(petId: number) {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.get<VaccinationRecord[]>(`/vaccination/pet-history/${petId}`);
        this.vaccinationHistory = response.data;
      } catch (err: any) {
        this.error = 'Không thể tải lịch sử tiêm chủng của thú cưng.';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Kiểm tra khoảng cách tiêm phòng an toàn trước khi xác nhận
     */
    async validateVaccinationInterval(): Promise<boolean> {
      if (!this.formData.petId || !this.formData.vaccineId || !this.formData.appointmentDate) {
        return true;
      }

      this.checkingInterval = true;
      this.error = null;
      try {
        const response = await api.post<IntervalValidationResult>('/vaccination/validate-interval', {
          petId: this.formData.petId,
          vaccineId: this.formData.vaccineId,
          proposedDate: this.formData.appointmentDate
        });
        this.validationResult = response.data;
        return response.data.isValid;
      } catch (err: any) {
        this.error = 'Không thể xác thực phác đồ tiêm chủng tự động.';
        return false;
      } finally {
        this.checkingInterval = false;
      }
    },

    /**
     * Thực hiện gửi đơn đăng ký đặt lịch tiêm phòng
     */
    async submitVaccinationBooking(): Promise<boolean> {
      this.submitting = true;
      this.error = null;
      this.successMessage = null;

      try {
        // Gửi thông tin kèm theo cờ xác nhận bỏ qua cảnh báo nếu có
        const payload = {
          ...this.formData,
          requiresOverride: this.validationResult?.requiresDoctorOverride || false
        };

        const response = await api.post<{
          success: boolean;
          message: string;
          appointmentId: number;
        }>('/vaccination/book', payload);

        if (response.data.success) {
          this.successMessage = response.data.message;
          this.resetStore();
          return true;
        }
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Có lỗi xảy ra khi tạo lịch đặt tiêm phòng.';
        return false;
      } finally {
        this.submitting = false;
      }
    },

    /**
     * Reset toàn bộ dữ liệu store
     */
    resetStore() {
      this.step = 1;
      this.formData = {
        petId: null,
        vaccineId: null,
        doctorId: null,
        appointmentDate: null,
        symptom: 'Tiêm phòng vắc-xin định kỳ theo phác đồ',
        note: '',
        bypassWarning: false
      };
      this.validationResult = null;
      this.vaccinationHistory = [];
      this.error = null;
    }
  }
});
```

---

## 3. Quản lý Vòng đời State (State Lifecycle & Sync Rules)

Để tránh tình trạng dữ liệu hiển thị không nhất quán và rò rỉ bộ nhớ:
* **Hủy liên kết (Clean up on Unmount):** Khi Component đặt lịch tiêm chủng Unmount, cần gọi `resetStore()` để dọn dẹp form data, tránh lưu giữ thông tin của thú cưng cũ cho phiên đặt lịch tiếp theo.
* **Đồng bộ hóa Tồn kho (Inventory Syncing):** Trước khi chuyển từ Bước 1 sang Bước 2, hành động `fetchVaccines` bắt buộc phải được gọi để đảm bảo dữ liệu tồn kho dược là mới nhất và chỉ những loại vắc-xin thực tế có hàng tại clinic mới được hiển thị.
* **Kiểm tra Phác đồ Realtime:** Hành động `validateVaccinationInterval` sẽ được tự động kích hoạt bất cứ khi nào có sự thay đổi đồng thời của cả 3 yếu tố: `petId`, `vaccineId`, và `appointmentDate` ở các bước tương ứng để đưa ra cảnh báo kịp thời cho người dùng trước khi họ bấm nút "Xác nhận lịch hẹn".
