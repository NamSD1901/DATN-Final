# 🗃️ State Management (Pinia Store) - Profile Details Update

## 1. Vai trò của Pinia Store trong Tính năng Profile
Pinia Store đảm nhận các vai trò quản trị trạng thái giao diện bao gồm:
1.  **Đồng bộ dữ liệu:** Lưu trữ thông tin cá nhân hiện tại được tải về từ API `/api/profile` để chia sẻ giữa các component (ví dụ: Sidebar hiển thị avatar/tên khách hàng, Header và trang Profile chính).
2.  **Quản lý trạng thái sửa đổi (Dirty Checking):** Lưu một bản sao lưu (Backup) nguyên bản của thông tin hồ sơ ngay khi tải từ DB. Khi người dùng nhập liệu, ta so sánh giá trị hiện tại trên form với bản sao lưu này để kích hoạt trạng thái "đã sửa đổi nhưng chưa lưu" (Dirty).
3.  **Điều khiển trạng thái API (Loading/Updating/Error):** Cung cấp các cờ Boolean giúp UI kích hoạt vòng quay Spinner hoặc hiển thị Toast thông báo lỗi/thành công một cách đồng bộ.

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là mã nguồn đặc tả đầy đủ cho `profileStore` viết bằng TypeScript:

```typescript
import { defineStore } from 'pinia';
import api from '@/services/api';

// Định nghĩa Interface cho dữ liệu Hồ sơ cá nhân
export interface UserProfile {
  id: string;
  roleId: number;
  fullName: string;
  email: string;
  phone: string;
  address: string;
  gender: number | null; // 0: Nữ, 1: Nam, 2: Khác
  dateOfBirth: string | null; // YYYY-MM-DD
  avatar: string | null;
  roleName?: string;
}

// Định nghĩa Interface cho DTO gửi lên API cập nhật
export interface UpdateProfilePayload {
  fullName: string;
  phone: string;
  address: string;
  gender: number | null;
  dateOfBirth: string | null;
}

// Định nghĩa cấu trúc State của Store
interface ProfileState {
  profile: UserProfile | null;
  originalProfile: UserProfile | null; // Bản sao lưu để kiểm tra dirty state
  loading: boolean;
  updating: boolean;
  error: string | null;
  successMessage: string | null;
}

export const useProfileStore = defineStore('profile', {
  state: (): ProfileState => ({
    profile: null,
    originalProfile: null,
    loading: false,
    updating: false,
    error: null,
    successMessage: null,
  }),

  getters: {
    /**
     * Kiểm tra xem người dùng đã thực hiện bất kỳ thay đổi nào trên form hay chưa.
     * So sánh từng trường của object 'profile' hiện tại với 'originalProfile' gốc.
     */
    isDirty(state): boolean {
      if (!state.profile || !state.originalProfile) return false;
      
      return (
        state.profile.fullName !== state.originalProfile.fullName ||
        state.profile.phone !== state.originalProfile.phone ||
        state.profile.address !== state.originalProfile.address ||
        state.profile.gender !== state.originalProfile.gender ||
        state.profile.dateOfBirth !== state.originalProfile.dateOfBirth
      );
    },

    /**
     * Trả về định dạng ngày sinh chuẩn Việt Nam DD/MM/YYYY để hiển thị trên UI.
     */
    formattedDateOfBirth(state): string {
      if (!state.profile?.dateOfBirth) return 'Chưa cập nhật';
      const date = new Date(state.profile.dateOfBirth);
      if (isNaN(date.getTime())) return 'Chưa cập nhật';
      
      const day = String(date.getDate()).padStart(2, '0');
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const year = date.getFullYear();
      return `${day}/${month}/${year}`;
    }
  },

  actions: {
    /**
     * Tải thông tin hồ sơ của người dùng hiện tại từ API
     */
    async fetchProfile() {
      this.loading = true;
      this.error = null;
      this.successMessage = null;
      
      try {
        const response = await api.get<UserProfile>('/profile');
        
        // Chuẩn hóa định dạng ngày sinh YYYY-MM-DD từ chuỗi ISO trả về từ database
        if (response.data.dateOfBirth) {
          response.data.dateOfBirth = response.data.dateOfBirth.split('T')[0];
        }

        this.profile = response.data;
        // Tạo bản sao sâu (Deep Copy) lưu vào originalProfile
        this.originalProfile = JSON.parse(JSON.stringify(response.data));
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải thông tin hồ sơ cá nhân.';
        this.profile = null;
        this.originalProfile = null;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Cập nhật thông tin hồ sơ lên server
     */
    async updateProfile(payload: UpdateProfilePayload): Promise<boolean> {
      this.updating = true;
      this.error = null;
      this.successMessage = null;

      try {
        const response = await api.put<{ success: boolean; message: string }>('/profile', payload);
        
        if (response.data.success) {
          this.successMessage = response.data.message;
          
          // Đồng bộ lại originalProfile với dữ liệu vừa cập nhật thành công
          if (this.profile) {
            Object.assign(this.profile, payload);
            this.originalProfile = JSON.parse(JSON.stringify(this.profile));
          }
          return true;
        }
        
        this.error = 'Cập nhật thất bại.';
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Có lỗi xảy ra trong quá trình cập nhật.';
        return false;
      } finally {
        this.updating = false;
      }
    },

    /**
     * Khôi phục form về trạng thái dữ liệu gốc được fetch gần nhất từ API (Hủy bỏ sửa đổi)
     */
    resetForm() {
      if (this.originalProfile) {
        this.profile = JSON.parse(JSON.stringify(this.originalProfile));
      }
      this.error = null;
      this.successMessage = null;
    },

    /**
     * Xóa sạch trạng thái store khi người dùng đăng xuất
     */
    clearStore() {
      this.profile = null;
      this.originalProfile = null;
      this.error = null;
      this.successMessage = null;
    }
  }
});
```
