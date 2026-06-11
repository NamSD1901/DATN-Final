# 03. State Management (Pinia Store) - Admin Staff Management

Tài liệu thiết kế Vue 3 Pinia Store sử dụng TypeScript cho phân hệ Quản lý Nhân sự & Phân quyền Admin.

---

## 1. Vai trò của Pinia Store trong Phân hệ

Pinia Store `useAdminStaffStore` giúp quản lý tập trung toàn bộ danh sách tài khoản nhân viên của phòng khám MyPetClinic. Store hỗ trợ lọc nhanh theo vai trò (`Role`), trạng thái hoạt động (`Status`) và tìm kiếm văn bản tự do theo họ tên/email/SĐT nhân viên ở phía Client. Nó cũng thực hiện gọi API để thay đổi quyền hoặc khóa tài khoản của nhân viên và cập nhật lại giao diện người dùng tức thì (Optimistic UI updates) sau khi nhận phản hồi từ Backend.

---

## 2. Mã nguồn TypeScript hoàn chỉnh cho Pinia Store

Dưới đây là mã nguồn chi tiết của file `useAdminStaffStore.ts` triển khai trong thư mục `frontend/src/stores/useAdminStaffStore.ts`:

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';

// Định nghĩa cấu trúc nhân viên trả về từ API
export interface StaffMember {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: 'admin' | 'doctor' | 'receptionist' | 'cashier';
  status: 'PendingActivation' | 'Active' | 'Suspended';
  requirePasswordChange: boolean;
  createdAt: string;
}

interface AdminStaffState {
  staffList: StaffMember[];
  searchQuery: string;
  roleFilter: 'All' | 'admin' | 'doctor' | 'receptionist' | 'cashier';
  statusFilter: 'All' | 'PendingActivation' | 'Active' | 'Suspended';
  isLoading: boolean;
  isSubmitting: boolean;
  error: string | null;
}

export const useAdminStaffStore = defineStore('adminStaff', {
  state: (): AdminStaffState => ({
    staffList: [],
    searchQuery: '',
    roleFilter: 'All',
    statusFilter: 'All',
    isLoading: false,
    isSubmitting: false,
    error: null
  }),

  getters: {
    // Bộ lọc danh sách nhân viên linh hoạt
    filteredStaff(state): StaffMember[] {
      return state.staffList.filter((staff) => {
        const query = state.searchQuery.toLowerCase();
        const matchesSearch =
          staff.fullName.toLowerCase().includes(query) ||
          staff.email.toLowerCase().includes(query) ||
          staff.phoneNumber.includes(query);

        const matchesRole =
          state.roleFilter === 'All' || staff.role === state.roleFilter;

        const matchesStatus =
          state.statusFilter === 'All' || staff.status === state.statusFilter;

        return matchesSearch && matchesRole && matchesStatus;
      });
    },

    // Thống kê nhanh số lượng nhân viên theo vai trò phục vụ Admin Dashboard widgets
    staffCountSummary(state) {
      return {
        total: state.staffList.length,
        admins: state.staffList.filter(s => s.role === 'admin').length,
        doctors: state.staffList.filter(s => s.role === 'doctor').length,
        receptionists: state.staffList.filter(s => s.role === 'receptionist').length,
        cashiers: state.staffList.filter(s => s.role === 'cashier').length,
        active: state.staffList.filter(s => s.status === 'Active').length,
        suspended: state.staffList.filter(s => s.status === 'Suspended').length
      };
    }
  },

  actions: {
    // 1. Tải danh sách toàn bộ tài khoản nhân sự từ Backend
    async fetchStaffList() {
      this.isLoading = true;
      this.error = null;
      try {
        const response = await axios.get<StaffMember[]>('/api/admin/staff');
        this.staffList = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải danh sách nhân sự.';
        console.error('Error fetching staff list:', err);
      } finally {
        this.isLoading = false;
      }
    },

    // 2. Tạo tài khoản nhân viên mới
    async createStaffAccount(data: { fullName: string; email: string; phoneNumber: string; role: string }) {
      this.isSubmitting = true;
      this.error = null;
      try {
        const response = await axios.post<StaffMember>('/api/admin/staff', data);
        // Thêm vào đầu mảng danh sách cục bộ
        this.staffList.unshift(response.data);
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Khởi tạo tài khoản nhân viên thất bại.';
        throw err;
      } finally {
        this.isSubmitting = false;
      }
    },

    // 3. Đổi vai trò chuyên môn của nhân viên
    async changeStaffRole(staffId: string, newRole: 'admin' | 'doctor' | 'receptionist' | 'cashier') {
      this.isSubmitting = true;
      this.error = null;
      try {
        const response = await axios.put<StaffMember>(`/api/admin/staff/${staffId}/role`, { newRole });
        // Cập nhật lại đối tượng trong mảng danh sách cục bộ
        const index = this.staffList.findIndex(s => s.id === staffId);
        if (index !== -1) {
          this.staffList[index] = response.data;
        }
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể thay đổi vai trò nhân viên.';
        throw err;
      } finally {
        this.isSubmitting = false;
      }
    },

    // 4. Bật/Tắt trạng thái tài khoản (Khóa / Kích hoạt lại)
    async toggleStaffStatus(staffId: string) {
      this.isSubmitting = true;
      this.error = null;
      try {
        const response = await axios.put<StaffMember>(`/api/admin/staff/${staffId}/toggle-status`);
        // Cập nhật lại đối tượng trong mảng danh sách cục bộ
        const index = this.staffList.findIndex(s => s.id === staffId);
        if (index !== -1) {
          this.staffList[index] = response.data;
        }
        return response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Thay đổi trạng thái tài khoản thất bại.';
        throw err;
      } finally {
        this.isSubmitting = false;
      }
    }
  }
});
```
