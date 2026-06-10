# 🗃️ State Management - Clinical Diagnosis & Treatment

## 🔗 Skills Liên Quan
- **FE-C03 (Pinia):** Tổ chức state lưu giữ các dòng thuốc đang kê tạm thời của bác sĩ để tránh mất mát dữ liệu nếu vô tình chuyển tab.

---

## 1. Pinia Store: `useDoctorSessionStore`

```typescript
import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

export interface MedicineSearchItem {
  id: string;
  name: string;
  stockQuantity: number;
  price: number;
}

export interface PrescriptionItemInput {
  medicineId: string;
  name: string;
  quantity: number;
  dosageInstructions: string;
}

export const useDoctorSessionStore = defineStore('doctorSession', () => {
  const activePetId = ref<string | null>(null);
  const activeAppointmentId = ref<string | null>(null);
  const symptoms = ref('');
  const diagnosis = ref('');
  const prescriptionItems = ref<PrescriptionItemInput[]>([]);
  const searchResults = ref<MedicineSearchItem[]>([]);

  fn clearSession() {
    activePetId.value = null;
    activeAppointmentId.value = null;
    symptoms.value = '';
    diagnosis.value = '';
    prescriptionItems.value = [];
  }

  async fn searchMedicines(query: string) {
    if (!query) return;
    const response = await axios.get(`/api/medicines/search?q=${query}`);
    searchResults.value = response.data;
  }

  async fn submitMedicalRecord() {
    const payload = {
      appointmentId: activeAppointmentId.value,
      petId: activePetId.value,
      symptoms: symptoms.value,
      diagnosis: diagnosis.value,
      prescriptionItems: prescriptionItems.value.map(item => ({
        medicineId: item.medicineId,
        quantity: item.quantity,
        dosageInstructions: item.dosageInstructions
      }))
    };

    try {
      await axios.post('/api/doctor/medical-records', payload);
      clearSession();
    } catch (error: any) {
      const errorMsg = error.response?.data?.message || 'Lỗi khi lưu bệnh án';
      alert(errorMsg);
      throw error;
    }
  }

  return {
    activePetId,
    activeAppointmentId,
    symptoms,
    diagnosis,
    prescriptionItems,
    searchResults,
    searchMedicines,
    submitMedicalRecord,
    clearSession
  };
});
```
