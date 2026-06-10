# 🗃️ State Management (Pinia Store) - Profile Avatar Upload

## 1. Quản lý Trạng thái Tải lên nhị phân (Upload State Architecture)
Tải ảnh đại diện yêu cầu xử lý luồng truyền dữ liệu bất đồng bộ (Stream upload) từ Client lên API Server. Pinia Store đóng vai trò quản lý vòng đời của tiến trình upload:
1.  **Theo dõi tiến độ (%) thực tế:** Sử dụng cờ `progress` (kiểu số từ 0 đến 100) để liên tục cập nhật thanh tiến trình trên giao diện người dùng qua hàm callback `onUploadProgress` của thư viện Axios.
2.  **Khóa đồng bộ (State Locking):** Kích hoạt trạng thái `uploading = true` để vô hiệu hóa các nút điều hướng và nút gửi dữ liệu trong lúc truyền file, tránh hiện tượng gửi trùng lặp tệp tin (Double-submit).
3.  **Lưu cache đường dẫn ảnh:** Sau khi tải lên thành công, cập nhật ngay lập tức trường `avatar` trong `profileStore` (đã nâng cấp ở Feature 07) để đồng bộ hiển thị ảnh đại diện mới trên thanh công cụ Navbar và các cấu phần liên quan khác mà không cần gọi lại API tải toàn bộ Profile.

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là mã nguồn đặc tả đầy đủ cho `avatarUploadStore` viết bằng TypeScript, có cấu hình đo lường dung lượng tải lên thời gian thực:

```typescript
import { defineStore } from 'pinia';
import api from '@/services/api';
import { useProfileStore } from './profile'; // Import profileStore để đồng bộ ảnh đại diện

interface AvatarState {
  uploading: boolean;
  progress: number; // Tiến trình từ 0 - 100 %
  error: string | null;
  success: boolean;
  tempPreviewUrl: string | null; // Đường dẫn ảnh preview local (Blob URL)
}

export const useAvatarStore = defineStore('avatar', {
  state: (): AvatarState => ({
    uploading: false,
    progress: 0,
    error: null,
    success: false,
    tempPreviewUrl: null,
  }),

  actions: {
    /**
     * Tạo đường dẫn xem trước cục bộ (Local Object URL) cho file ảnh vừa chọn
     */
    generatePreview(file: File) {
      this.clearPreview();
      
      // Tạo URL xem trước tạm thời bằng APIs trình duyệt
      this.tempPreviewUrl = URL.createObjectURL(file);
      this.success = false;
      this.error = null;
    },

    /**
     * Giải phóng URL xem trước để tránh rò rỉ bộ nhớ RAM trình duyệt
     */
    clearPreview() {
      if (this.tempPreviewUrl) {
        URL.revokeObjectURL(this.tempPreviewUrl);
        this.tempPreviewUrl = null;
      }
    },

    /**
     * Gửi file nhị phân lên server API
     */
    async uploadAvatarFile(file: File): Promise<boolean> {
      // 1. Khởi tạo trạng thái
      this.uploading = true;
      this.progress = 0;
      this.error = null;
      this.success = false;

      // 2. Chuẩn bị FormData (Bắt buộc đối với tải file nhị phân)
      const formData = new FormData();
      formData.append('avatarFile', file); // Khớp chính xác tên tham số "avatarFile" trong API Controller

      try {
        // 3. Thực hiện gọi API kèm callback đo lường tiến trình
        const response = await api.post<{ success: boolean; avatarUrl: string; message: string }>(
          '/profile/avatar', 
          formData, 
          {
            headers: {
              'Content-Type': 'multipart/form-data' // Thiết lập header tải file nhị phân
            },
            onUploadProgress: (progressEvent) => {
              if (progressEvent.total) {
                // Tính toán phần trăm tiến độ tải lên thực tế
                this.progress = Math.round((progressEvent.loaded * 100) / progressEvent.total);
              }
            }
          }
        );

        if (response.data.success) {
          this.success = true;
          
          // 4. Đồng bộ ảnh đại diện mới sang profileStore ngay lập tức để UI cập nhật
          const profileStore = useProfileStore();
          if (profileStore.profile) {
            profileStore.profile.avatar = response.data.avatarUrl;
            
            // Cập nhật cả bản sao lưu gốc để tránh form profile bị coi là Dirty
            if (profileStore.originalProfile) {
              profileStore.originalProfile.avatar = response.data.avatarUrl;
            }
          }
          
          this.clearPreview();
          return true;
        }

        this.error = response.data.message || 'Tải ảnh lên thất bại.';
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Có lỗi xảy ra trong quá trình truyền tải tệp tin.';
        return false;
      } finally {
        this.uploading = false;
        this.progress = 0;
      }
    },

    /**
     * Khởi động lại toàn bộ trạng thái upload
     */
    resetUploadState() {
      this.clearPreview();
      this.uploading = false;
      this.progress = 0;
      this.error = null;
      this.success = false;
    }
  }
});
```
