# 🗃️ State Management Specification - Vets Team (Pinia)

Tài liệu này đặc tả chi tiết cơ chế quản lý danh sách bác sĩ trực ca, lọc danh sách theo chuyên khoa lâm sàng tại Client sử dụng Vue 3 Pinia Store.

---

## 🔗 Skills & Quy tắc lập trình liên quan
*   **FE-C01 (Vue 3 Composition API):** Sử dụng `ref` phản ứng và `computed` để tính toán bộ lọc tức thời.
*   **FE-C03 (Pinia State Management):** Đóng gói logic gọi API lấy danh sách bác sĩ vào `useDoctorsStore`, thực hiện lọc cục bộ tại RAM Client để nâng cao hiệu năng trải nghiệm người dùng.

---

## 1. Khai báo Pinia Store (`useDoctorsStore.ts`)

Chúng ta xây dựng Store quản trị trạng thái danh sách bác sĩ và bộ lọc chuyên khoa:

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios, { AxiosError } from 'axios';

// Định nghĩa Interface Doctor
export interface DoctorItem {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  specialty: string;
  specialtyCode: string;
  experienceYears: number;
  qualifications: string;
  biography: string;
  avatarUrl: string;
  isOnDuty: boolean;
}

export const useDoctorsStore = defineStore('doctors', () => {
  // --- STATE ---
  const doctors = ref<DoctorItem[]>([]);
  const isLoading = ref<boolean>(false);
  const errorMessage = ref<string>('');
  
  // Bộ lọc chuyên khoa ở Client
  const selectedSpecialtyCode = ref<string>(''); // Rỗng tương ứng "Tất cả"

  // --- GETTERS ---
  
  /**
   * Tính toán bộ lọc bác sĩ tức thời tại bộ nhớ trình duyệt
   */
  const filteredDoctors = computed<DoctorItem[]>(() => {
    if (selectedSpecialtyCode.value === '') {
      return doctors.value;
    }
    return doctors.value.filter(
      d => d.specialtyCode.toLowerCase() === selectedSpecialtyCode.value.toLowerCase()
    );
  });

  // --- ACTIONS ---

  /**
   * Gọi API tải toàn bộ danh sách bác sĩ công khai
   */
  async function fetchDoctors() {
    // Client Caching: Nếu đã tải dữ liệu rồi thì bỏ qua không gọi lại API
    if (doctors.value.length > 0) return;

    isLoading.value = true;
    errorMessage.value = '';

    try {
      const response = await axios.get('/api/doctors');
      doctors.value = response.data;
    } catch (error) {
      const err = error as AxiosError<{ message?: string }>;
      errorMessage.value = err.response?.data?.message || 'Không thể tải danh sách bác sĩ.';
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * Thiết lập bộ lọc chuyên khoa
   */
  function setSpecialty(specialtyCode: string) {
    selectedSpecialtyCode.value = specialtyCode.trim();
  }

  function clearFilters() {
    selectedSpecialtyCode.value = '';
  }

  return {
    doctors,
    isLoading,
    errorMessage,
    selectedSpecialtyCode,
    filteredDoctors,
    fetchDoctors,
    setSpecialty,
    clearFilters
  };
});
```

---

## 2. Đồng bộ luồng Đặt lịch khám nhanh
*   Khi người dùng click nút "Đặt lịch khám" trên thẻ của ThS. BS. Vy (Chuyên khoa Nội khoa):
    *   Hệ thống chuyển hướng người dùng sang Modal đặt lịch hẹn.
    *   Store đặt lịch sẽ nhận diện tham số `doctorId` và tự động chọn sẵn **Bác sĩ Vy** làm bác sĩ khám chính, đồng thời khóa cứng trường lựa chọn bác sĩ đó để nâng cao trải nghiệm đặt lịch nhanh.
