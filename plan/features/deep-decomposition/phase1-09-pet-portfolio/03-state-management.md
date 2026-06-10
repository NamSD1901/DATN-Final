# 🗃️ State Management (Pinia Store) - Pet Portfolio Management

## 1. Vai trò của Pinia Store trong Nghiệp vụ Quản lý Thú cưng
Pinia Store (`petsStore`) đóng vai trò quản lý bộ nhớ đệm (Client-side cache) và điều hành các hoạt động CRUD đối với danh sách thú cưng của khách hàng:
1.  **Hạn chế truy vấn dư thừa:** Khi người dùng di chuyển giữa trang Dashboard đặt lịch khám và trang quản lý thú cưng, store lưu giữ danh sách thú cưng đã tải từ API để tránh gửi request GET liên tục lên server.
2.  **Đồng bộ dữ liệu trực quan (Reactive List Update):** Khi thêm mới, chỉnh sửa hoặc xóa mềm một thú cưng, store thực hiện cập nhật cục bộ trực tiếp trên mảng `pets` của state. Giao diện lập tức phản ánh thay đổi mà không cần tải lại toàn bộ danh sách từ cơ sở dữ liệu.
3.  **Lưu trữ thú cưng đang chọn (Active Pet State):** Lưu trữ đối tượng thú cưng đang được người dùng bấm xem chi tiết hoặc chỉnh sửa để binding trực tiếp vào Modal Form.

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là mã nguồn đặc tả đầy đủ cho `mypetsStore` viết bằng TypeScript:

```typescript
import { defineStore } from 'pinia';
import api from '@/services/api';

// Định nghĩa Interface cho dữ liệu Thú cưng
export interface Pet {
  id: number;
  ownerId: string;
  name: string;
  species: string; // dog, cat, other
  breed: string;
  gender: string; // Male, Female
  birthDate: string | null; // YYYY-MM-DD
  weight: number | null; // Kg
  color: string | null;
  bloodType: string | null;
  sterilized: boolean;
  microchipCode: string | null;
  allergyNote: string | null;
  createdAt?: string;
}

// Định nghĩa DTO đầu vào cho Create
export interface CreatePetPayload {
  name: string;
  species: string;
  breed: string;
  gender: string;
  birthDate: string | null;
  weight: number | null;
  color: string | null;
  bloodType: string | null;
  sterilized: boolean;
  microchipCode: string | null;
  allergyNote: string | null;
}

// Định nghĩa DTO đầu vào cho Update
export interface UpdatePetPayload extends CreatePetPayload {
  id: number;
}

interface PetsState {
  pets: Pet[];
  activePet: Pet | null; // Dùng cho modal sửa/xem chi tiết
  loading: boolean;
  saving: boolean;
  error: string | null;
}

export const usePetsStore = defineStore('pets', {
  state: (): PetsState => ({
    pets: [],
    activePet: null,
    loading: false,
    saving: false,
    error: null,
  }),

  getters: {
    /**
     * Trả về tổng số thú cưng hiện tại trong tài khoản chủ nuôi
     */
    totalPets(state): number {
      return state.pets.length;
    },

    /**
     * Lọc danh sách thú cưng là Chó
     */
    dogsList(state): Pet[] {
      return state.pets.filter(pet => pet.species === 'dog');
    },

    /**
     * Lọc danh sách thú cưng là Mèo
     */
    catsList(state): Pet[] {
      return state.pets.filter(pet => pet.species === 'cat');
    }
  },

  actions: {
    /**
     * Lấy toàn bộ danh sách thú cưng của chủ nuôi đang đăng nhập
     */
    async fetchMyPets() {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.get<Pet[]>('/mypets');
        this.pets = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải danh sách thú cưng.';
      } finally {
        this.loading = false;
      }
    },

    /**
     * Thêm mới một thú cưng
     */
    async addPet(payload: CreatePetPayload): Promise<boolean> {
      this.saving = true;
      this.error = null;
      try {
        const response = await api.post<{ success: boolean; message: string }>('/mypets', payload);
        if (response.status === 200) {
          // Tải lại danh sách để đảm bảo nhận ID tự tăng chính xác từ DB
          await this.fetchMyPets();
          return true;
        }
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi khi thêm thú cưng.';
        return false;
      } finally {
        this.saving = false;
      }
    },

    /**
     * Cập nhật thông tin thú cưng
     */
    async updatePet(payload: UpdatePetPayload): Promise<boolean> {
      this.saving = true;
      this.error = null;
      try {
        const response = await api.put<{ success: boolean; message: string }>(`/mypets/${payload.id}`, payload);
        if (response.status === 200) {
          // Cập nhật phần tử trực tiếp trong mảng reactive để UI thay đổi ngay
          const index = this.pets.findIndex(p => p.id === payload.id);
          if (index !== -1) {
            this.pets[index] = { ...this.pets[index], ...payload };
          }
          return true;
        }
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi khi cập nhật thông tin thú cưng.';
        return false;
      } finally {
        this.saving = false;
      }
    },

    /**
     * Xóa mềm thú cưng (Soft delete)
     */
    async deletePet(id: number): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.delete<{ success: boolean; message: string }>(`/mypets/${id}`);
        if (response.status === 200) {
          // Lọc bỏ thú cưng vừa xóa khỏi state cục bộ
          this.pets = this.pets.filter(p => p.id !== id);
          return true;
        }
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi khi xóa hồ sơ thú cưng.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Đặt thú cưng được chọn để hiển thị trên modal
     */
    setActivePet(pet: Pet | null) {
      this.activePet = pet ? JSON.parse(JSON.stringify(pet)) : null;
    }
  }
});
```
