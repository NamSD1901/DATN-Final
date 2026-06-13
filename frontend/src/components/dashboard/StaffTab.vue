<template>
  <div class="staff-tab container-fluid p-0">
    <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
      <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
        <div>
          <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-person-badge-fill text-warning me-2"></i>Quản lý Nhân sự</h4>
          <p class="text-muted small mb-0">Quản lý vai trò, quyền hạn và trạng thái hoạt động của nhân viên phòng khám</p>
        </div>
      </div>

      <!-- Filters & Table -->
      <div class="row g-2 mb-3 align-items-center">
        <div class="col-md-6">
          <div class="input-group">
            <span class="input-group-text bg-white border-end-0"><i class="bi bi-search text-muted"></i></span>
            <input 
              type="text" 
              v-model="searchKeyword" 
              class="form-control border-start-0 input-premium" 
              placeholder="Tìm kiếm theo tên hoặc email..."
            />
          </div>
        </div>
        <div class="col-md-6 text-md-end text-muted small">
          Tổng số: <strong class="text-dark">{{ filteredStaff.length }}</strong> nhân viên
        </div>
      </div>

      <!-- Staff Table -->
      <div class="table-responsive rounded-4 border overflow-hidden mt-3">
        <table class="table table-hover align-middle mb-0">
          <thead class="bg-light-gold">
            <tr>
              <th class="ps-4">Nhân sự</th>
              <th>Email</th>
              <th>Vai trò</th>
              <th>Trạng thái</th>
              <th class="text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center">
              <td colspan="5" class="py-5">
                <div class="spinner-border text-warning spinner-border-sm me-2"></div>
                <span class="text-muted">Đang tải danh sách nhân sự...</span>
              </td>
            </tr>
            <tr v-else-if="filteredStaff.length === 0" class="text-center">
              <td colspan="5" class="py-5 text-muted">
                <i class="bi bi-people fs-2 mb-2 d-block"></i>
                Không tìm thấy nhân viên nào.
              </td>
            </tr>
            <tr v-for="member in filteredStaff" :key="member.id" v-else>
              <td class="ps-4">
                <div class="d-flex align-items-center gap-3">
                  <div class="avatar-circle-gold" style="width: 38px; height: 38px; font-size: 0.95rem;">
                    {{ getAvatarLetters(member.fullName) }}
                  </div>
                  <div>
                    <div class="fw-bold text-dark">{{ member.fullName }}</div>
                  </div>
                </div>
              </td>
              <td>{{ member.email }}</td>
              <td>
                <select 
                  :value="member.role" 
                  @change="handleRoleChange(member.id, ($event.target as HTMLSelectElement).value)"
                  class="form-select form-select-sm border-warning rounded-pill px-3 py-1 fw-bold"
                  style="width: 140px;"
                >
                  <option value="admin">Admin</option>
                  <option value="doctor">Bác sĩ</option>
                  <option value="receptionist">Lễ tân</option>
                  <option value="customer">Khách hàng</option>
                </select>
              </td>
              <td>
                <span :class="['badge rounded-pill px-3 py-1.5 fw-bold', member.isActive ? 'bg-success bg-opacity-10 text-success' : 'bg-danger bg-opacity-10 text-danger']">
                  {{ member.isActive ? 'Đang hoạt động' : 'Tạm khóa' }}
                </span>
              </td>
              <td class="text-center">
                <button 
                  :class="['btn btn-sm rounded-pill px-3 fw-bold', member.isActive ? 'btn-outline-danger' : 'btn-outline-success']"
                  @click="handleToggleStatus(member.id, member.isActive)"
                >
                  <i :class="['bi me-1', member.isActive ? 'bi-lock-fill' : 'bi-unlock-fill']"></i>
                  {{ member.isActive ? 'Khóa' : 'Kích hoạt' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import api from '../../services/api';

const loading = ref(false);
const staffList = ref<any[]>([]);
const searchKeyword = ref('');

const filteredStaff = computed(() => {
  const keyword = searchKeyword.value.toLowerCase().trim();
  return staffList.value.filter(member => 
    member.role !== 'customer' && (
      member.fullName.toLowerCase().includes(keyword) || 
      member.email.toLowerCase().includes(keyword)
    )
  );
});

const loadStaff = async () => {
  loading.value = true;
  try {
    const res = await api.get('/admin/users');
    staffList.value = res.data || [];
  } catch (err) {
    console.error('Lỗi tải danh sách nhân sự:', err);
  } finally {
    loading.value = false;
  }
};

const handleRoleChange = async (userId: string, newRole: string) => {
  try {
    const res = await api.put(`/admin/users/${userId}/role`, { newRole });
    alert(res.data.message || 'Cập nhật vai trò thành công!');
    await loadStaff();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi cập nhật vai trò.');
    await loadStaff();
  }
};

const handleToggleStatus = async (userId: string, currentStatus: boolean) => {
  const actionText = currentStatus ? 'khóa' : 'kích hoạt';
  if (!confirm(`Bạn có chắc chắn muốn ${actionText} tài khoản này không?`)) {
    return;
  }
  try {
    const res = await api.put(`/admin/users/${userId}/status`, { isActive: !currentStatus });
    alert(res.data.message || 'Cập nhật trạng thái thành công!');
    await loadStaff();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi cập nhật trạng thái.');
  }
};

const getAvatarLetters = (name: string) => {
  if (!name) return '?';
  const parts = name.trim().split(/\s+/);
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
  return name.slice(0, 1).toUpperCase();
};

onMounted(() => {
  loadStaff();
});
</script>

<style scoped>
.bg-light-gold {
  background-color: #fdfaf0;
}
.text-dark-gold {
  color: #b25e00;
}
.input-premium {
  border: 1px solid #ffeed1;
  transition: all 0.3s ease;
}
.input-premium:focus {
  border-color: #f59e0b;
  box-shadow: 0 0 0 0.25rem rgba(245, 158, 11, 0.15);
}
.avatar-circle-gold {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background: #fdfaf0;
  color: #f59e0b;
  border: 1px solid #ffeed1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
}
</style>
