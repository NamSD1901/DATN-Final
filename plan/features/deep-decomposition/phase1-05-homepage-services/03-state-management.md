# 🗃️ State Management Specification - Services Catalog (Pinia)

Tài liệu này đặc tả chi tiết cơ chế quản lý danh sách dịch vụ y tế thú y, bộ lọc danh mục và từ khóa tìm kiếm thời gian thực tại Client sử dụng Vue 3 Pinia Store.

---

## 🔗 Skills & Quy tắc lập trình liên quan
*   **FE-C01 (Vue 3 Composition API):** Sử dụng các ref phản ứng và `computed` để xử lý lọc tức thời trên trình duyệt.
*   **FE-C03 (Pinia State Management):** Đóng gói logic gọi API lấy danh sách dịch vụ vào `useServicesStore` và thực hiện lọc cục bộ để tối ưu hiệu năng.

---

## 1. Thiết lập Pinia Store (`useServicesStore.ts`)

Chúng ta xây dựng Store quản trị trạng thái danh sách dịch vụ và bộ lọc tìm kiếm:

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios, { AxiosError } from 'axios';

// Định nghĩa cấu trúc Service
export interface ServiceItem {
  id: string;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  categoryName: string;
  categoryCode: string;
}

export const useServicesStore = defineStore('services', () => {
  // --- STATE ---
  const services = ref<ServiceItem[]>([]);
  const isLoading = ref<boolean>(false);
  const errorMessage = ref<string>('');
  
  // Bộ lọc ở Frontend
  const selectedCategoryCode = ref<string>(''); // Mặc định rỗng: "Tất cả"
  const searchKeyword = ref<string>('');

  // --- GETTERS (Lọc tức thời ở Client-side) ---
  
  /**
   * Trả về danh sách dịch vụ đã được lọc theo danh mục và từ khóa tìm kiếm
   */
  const filteredServices = computed<ServiceItem[]>(() => {
    let result = services.value;

    // 1. Lọc theo danh mục
    if (selectedCategoryCode.value !== '') {
      result = result.filter(
        s => s.categoryCode.toLowerCase() === selectedCategoryCode.value.toLowerCase()
      );
    }

    // 2. Lọc theo từ khóa tìm kiếm (Không phân biệt hoa thường)
    if (searchKeyword.value.trim() !== '') {
      const keyword = searchKeyword.value.trim().toLowerCase();
      result = result.filter(
        s => s.name.toLowerCase().includes(keyword) || 
             s.description.toLowerCase().includes(keyword)
      );
    }

    return result;
  });

  // --- ACTIONS ---

  /**
   * Gọi API tải toàn bộ danh sách dịch vụ công khai
   */
  async function fetchServices() {
    // Nếu đã có dữ liệu dịch vụ rồi thì không cần gọi lại API (Client Caching)
    if (services.value.length > 0) return;

    isLoading.value = true;
    errorMessage.value = '';

    try {
      const response = await axios.get('/api/services');
      services.value = response.data;
    } catch (error) {
      const err = error as AxiosError<{ message?: string }>;
      errorMessage.value = err.response?.data?.message || 'Không thể tải danh sách dịch vụ.';
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * Thiết lập bộ lọc danh mục
   */
  function setCategory(categoryCode: string) {
    selectedCategoryCode.value = categoryCode.trim();
  }

  /**
   * Thiết lập từ khóa tìm kiếm
   */
  function setSearchKeyword(keyword: string) {
    searchKeyword.value = keyword;
  }

  /**
   * Dọn dẹp trạng thái lọc
   */
  function clearFilters() {
    selectedCategoryCode.value = '';
    searchKeyword.value = '';
  }

  return {
    services,
    isLoading,
    errorMessage,
    selectedCategoryCode,
    searchKeyword,
    filteredServices,
    fetchServices,
    setCategory,
    setSearchKeyword,
    clearFilters
  };
});
```

---

## 2. Lợi ích của Lọc Cục bộ (Client-Side Memory Filtering)
Thay vì mỗi khi người dùng gõ 1 chữ cái hoặc click tab lọc danh mục, Frontend lại gửi 1 request HTTP lên API:
1. Store Pinia tải toàn bộ danh sách dịch vụ **duy nhất 1 lần** khi trang chủ được load thông qua `fetchServices()`.
2. Mọi thao tác gõ tìm kiếm, đổi tab danh mục đều được tính toán lại tức thời thông qua Getter `filteredServices` chạy trên RAM của trình duyệt khách hàng.
3. *Kết quả:* Hiển thị phản hồi UI nhanh đột ngột (<2ms), mang lại trải nghiệm cực kỳ cao cấp và triệt tiêu tải trọng không cần thiết lên API Gateway của hệ thống.
