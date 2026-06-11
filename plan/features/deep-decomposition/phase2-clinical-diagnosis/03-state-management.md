# 🗃️ State Management (Pinia Store) - Clinical Diagnosis & Treatment

## 1. Quản lý trạng thái Phiên khám & Auto-save bản nháp (Clinical Session Architecture)

Màn hình khám bệnh của bác sĩ yêu cầu xử lý lượng thông tin y khoa lớn bao gồm triệu chứng lâm sàng, chẩn đoán, hướng điều trị và đơn thuốc kê động. Pinia Store (`doctorSessionStore`) thực hiện các nhiệm vụ quản trị trạng thái sau:

1. **Active Patient Focus:** Theo dõi thông tin ca khám đang tiếp nhận (`activeAppointmentId`, `activePetId`).
2. **Clinical Notes:** Lưu trữ dữ liệu ghi chép chẩn đoán (`diagnosis`, `treatmentPlan`) và triệu chứng.
3. **Prescription Grid State:** Quản lý mảng đơn thuốc tạm thời (`prescriptionItems`), tự động đối chiếu số lượng nhập và tồn kho khả dụng để hiển thị cảnh báo đỏ trên giao diện.
4. **Local Draft Auto-save:** Cơ chế tự động sao lưu dữ liệu khám lâm sàng tạm thời vào `localStorage` giúp bác sĩ bảo toàn thông tin chẩn đoán nếu trình duyệt bị tắt đột ngột.
5. **Real-time Search Suggestion:** Lưu trữ kết quả tìm kiếm biệt dược (`searchResults`) hỗ trợ Autocomplete.

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là đặc tả mã nguồn đầy đủ cho `doctorSessionStore` viết bằng TypeScript:

```typescript
import { defineStore } from 'pinia';
import api from '@/services/api';

// Chi tiết dược phẩm khi tìm kiếm
export interface MedicineSearchItem {
  id: number;
  name: string;
  activeIngredient: string;
  stockQuantity: number;
  price: number;
  unit: string;
}

// Một dòng thuốc trong đơn kê tạm thời
export interface PrescriptionItemInput {
  medicineId: number;
  name: string;
  quantity: number;
  dosageInstructions: string;
  price: number; // Lưu giá để hiển thị tổng số tiền tạm tính
  stockQuantity: number; // Lưu tồn kho để validate client-side
}

// Bản ghi lịch sử bệnh án tải về
export interface MedicalHistoryRecord {
  id: number;
  diagnosis: string;
  treatmentPlan: string;
  createdAt: string;
  prescriptionItems: {
    medicineName: string;
    quantity: number;
    dosageInstructions: string;
  }[];
}

interface DoctorSessionState {
  activeAppointmentId: number | null;
  activePetId: number | null;
  activePetName: string;
  diagnosis: string;
  treatmentPlan: string;
  prescriptionItems: PrescriptionItemInput[];
  medicalHistory: MedicalHistoryRecord[];
  searchResults: MedicineSearchItem[];
  searching: boolean;
  submitting: boolean;
  loadingHistory: boolean;
  error: string | null;
}

export const useDoctorSessionStore = defineStore('doctorSession', {
  state: (): DoctorSessionState => ({
    activeAppointmentId: null,
    activePetId: null,
    activePetName: '',
    diagnosis: '',
    treatmentPlan: '',
    prescriptionItems: [],
    medicalHistory: [],
    searchResults: [],
    searching: false,
    submitting: false,
    loadingHistory: false,
    error: null
  }),

  getters: {
    /**
     * Tính tổng chi phí thuốc tạm tính trong đơn
     */
    totalMedicineCost(state): decimal {
      return state.prescriptionItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    },

    /**
     * Kiểm tra nhanh xem đơn thuốc có lỗi kê vượt tồn kho hay không
     */
    hasInventoryError(state): boolean {
      return state.prescriptionItems.some(item => item.quantity > item.stockQuantity);
    },

    /**
     * Validation nhanh biểu mẫu khám trước khi submit
     */
    isFormValid(state): boolean {
      return (
        state.activeAppointmentId !== null &&
        state.activePetId !== null &&
        state.diagnosis.trim().length >= 10 &&
        state.treatmentPlan.trim().length > 0 &&
        !this.hasInventoryError
      );
    }
  },

  actions: {
    /**
     * Tải lịch sử bệnh án cũ của thú cưng
     */
    async fetchPetMedicalHistory(petId: number) {
      this.loadingHistory = true;
      this.error = null;
      try {
        const response = await api.get<MedicalHistoryRecord[]>(`/doctor/pets/${petId}/medical-history`);
        this.medicalHistory = response.data;
      } catch (err: any) {
        this.error = 'Không thể tải lịch sử bệnh án cũ của vật nuôi.';
      } finally {
        this.loadingHistory = false;
      }
    },

    /**
     * Tìm kiếm thuốc trong kho tự động điền (Autocomplete)
     */
    async searchMedicines(query: string) {
      if (!query.trim()) {
        this.searchResults = [];
        return;
      }
      this.searching = true;
      try {
        const response = await api.get<MedicineSearchItem[]>(`/medicines/autocomplete`, {
          params: { query }
        });
        this.searchResults = response.data;
      } catch (err) {
        console.error('Lỗi khi tải danh sách thuốc gợi ý:', err);
      } finally {
        this.searching = false;
      }
    },

    /**
     * Thêm thuốc an toàn vào đơn thuốc
     */
    addMedicineToPrescription(medicine: MedicineSearchItem) {
      const existing = this.prescriptionItems.find(item => item.medicineId === medicine.id);
      if (existing) {
        // Nếu đã có trong đơn, tự động cộng thêm 1
        if (existing.quantity < medicine.stockQuantity) {
          existing.quantity++;
        }
      } else {
        // Thêm mới dòng thuốc
        this.prescriptionItems.push({
          medicineId: medicine.id,
          name: medicine.name,
          quantity: 1,
          dosageInstructions: 'Uống ngày 2 lần, mỗi lần 1 viên sau ăn.',
          price: medicine.price,
          stockQuantity: medicine.stockQuantity
        });
      }
      this.saveDraftToLocal();
    },

    /**
     * Xóa dòng thuốc khỏi đơn
     */
    removeMedicine(medicineId: number) {
      this.prescriptionItems = this.prescriptionItems.filter(item => item.medicineId !== medicineId);
      this.saveDraftToLocal();
    },

    /**
     * Lưu trữ bản nháp tạm thời phòng trình duyệt bị tắt đột ngột
     */
    saveDraftToLocal() {
      if (!this.activeAppointmentId) return;
      const draft = {
        diagnosis: this.diagnosis,
        treatmentPlan: this.treatmentPlan,
        prescriptionItems: this.prescriptionItems
      };
      localStorage.setItem(`draft_exam_${this.activeAppointmentId}`, JSON.stringify(draft));
    },

    /**
     * Tải bản nháp lưu trữ từ LocalStorage
     */
    loadDraftFromLocal(appointmentId: number) {
      const saved = localStorage.getItem(`draft_exam_${appointmentId}`);
      if (saved) {
        try {
          const draft = JSON.parse(saved);
          this.diagnosis = draft.diagnosis;
          this.treatmentPlan = draft.treatmentPlan;
          this.prescriptionItems = draft.prescriptionItems;
        } catch (e) {
          console.error('Lỗi tải bản nháp khám:', e);
        }
      }
    },

    /**
     * Submit bệnh án chính thức lên backend
     */
    async submitMedicalRecord(): Promise<boolean> {
      if (!this.isFormValid) return false;
      
      this.submitting = true;
      this.error = null;
      try {
        const payload = {
          appointmentId: this.activeAppointmentId,
          petId: this.activePetId,
          diagnosis: this.diagnosis,
          treatmentPlan: this.treatmentPlan,
          prescriptionItems: this.prescriptionItems.map(item => ({
            medicineId: item.medicineId,
            quantity: item.quantity,
            dosageInstructions: item.dosageInstructions
          }))
        };

        const response = await api.post('/doctor/medical-records', payload);
        
        // Hủy bản nháp local sau khi lưu thành công
        if (this.activeAppointmentId) {
          localStorage.removeItem(`draft_exam_${this.activeAppointmentId}`);
        }
        
        this.clearSession();
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Có lỗi xảy ra khi lưu hồ sơ bệnh án.';
        return false;
      } finally {
        this.submitting = false;
      }
    },

    clearSession() {
      this.activeAppointmentId = null;
      this.activePetId = null;
      this.activePetName = '';
      this.diagnosis = '';
      this.treatmentPlan = '';
      this.prescriptionItems = [];
      this.medicalHistory = [];
      this.searchResults = [];
    }
  }
});
```
