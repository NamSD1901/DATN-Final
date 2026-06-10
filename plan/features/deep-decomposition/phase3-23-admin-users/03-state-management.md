# 🗃️ State Management - Admin Staff Management

## 🔗 Skills Liên Quan
- **FE-C03 (Pinia):** Quản lý store danh sách nhân viên, hỗ trợ filter tại client theo Tên/SĐT và phân trang.

---

## 1. Pinia Store: `useAdminStaffStore`

```typescript
import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

export interface StaffUser {
  id: string;
  email: string;
  fullName: string;
  role: string;
  isLockedOut: boolean;
}

export const useAdminStaffStore = defineStore('adminStaff', () => {
  const staffList = ref<StaffUser[]>([]);
  const loading = ref(false);

  async fn fetchStaff() {
    loading.value = true;
    try {
      const response = await axios.get('/api/admin/users');
      staffList.value = response.data;
    } catch (error) {
      console.error('Không thể lấy danh sách nhân viên', error);
    } finally {
      loading.value = false;
    }
  }

  async fn lockUser(id: string) {
    try {
      await axios.post(`/api/admin/users/${id}/lock`);
      const user = staffList.value.find(u => u.id === id);
      if (user) user.isLockedOut = true;
    } catch (error) {
      console.error('Lỗi khi khóa tài khoản', error);
      throw error;
    }
  }

  async fn unlockUser(id: string) {
    try {
      await axios.post(`/api/admin/users/${id}/unlock`);
      const user = staffList.value.find(u => u.id === id);
      if (user) user.isLockedOut = false;
    } catch (error) {
      console.error('Lỗi khi mở khóa tài khoản', error);
      throw error;
    }
  }

  return {
    staffList,
    loading,
    fetchStaff,
    lockUser,
    unlockUser
  };
});
```
