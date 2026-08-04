<template>
  <div class="staff-tab container-fluid p-0">
    <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
      <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
        <div>
          <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-person-badge-fill text-warning me-2"></i>Quản lý Nhân sự</h4>
          <p class="text-muted small mb-0">Quản lý vai trò, quyền hạn và danh sách nhân viên phòng khám</p>
        </div>
        <button class="btn btn-warning fw-bold px-4 rounded-pill shadow-sm text-dark" @click="openAddModal">
          <i class="bi bi-plus-lg me-1"></i> Thêm Nhân Viên
        </button>
      </div>

      <!-- Filters & Table -->
      <div class="row g-2 mb-3 align-items-center">
        <div class="col-md-5">
          <div class="input-group">
            <span class="input-group-text bg-white border-end-0 rounded-start-pill"><i class="bi bi-search text-muted"></i></span>
            <input 
              type="text" 
              v-model="searchKeyword" 
              class="form-control border-start-0 input-premium rounded-end-pill" 
              placeholder="Tìm kiếm theo tên, email hoặc CCCD..."
            />
          </div>
        </div>
        <div class="col-md-3">
          <select v-model="selectedRole" class="form-select input-premium rounded-pill text-muted">
            <option value="">Tất cả vai trò</option>
            <option value="admin">Quản trị viên</option>
            <option value="clinical_doctor">BS. Khám Bệnh</option>
            <option value="vaccination_doctor">BS. Tiêm Chủng</option>
            <option value="receptionist">Lễ tân</option>
          </select>
        </div>
        <div class="col-md-4 text-md-end text-muted small">
          Tổng số: <strong class="text-dark">{{ filteredStaff.length }}</strong> nhân sự
        </div>
      </div>

      <!-- Staff Table -->
      <div class="table-responsive rounded-4 border overflow-hidden mt-3">
        <table class="table table-hover align-middle mb-0">
          <thead class="bg-light-gold">
            <tr>
              <th class="ps-4">Nhân viên</th>
              <th>CCCD & SĐT</th>
              <th>Vai trò</th>
              <th>Trạng thái</th>
              <th class="text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="employeeStore.isLoading && employeeStore.employees.length === 0" class="text-center">
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
                  <div class="avatar-circle-gold" style="width: 42px; height: 42px; font-size: 1rem;">
                    {{ getAvatarLetters(member.fullName) }}
                  </div>
                  <div>
                    <div class="fw-bold text-dark">{{ member.fullName }}</div>
                    <div class="small text-muted">{{ member.email }}</div>
                  </div>
                </div>
              </td>
              <td>
                <div class="text-dark small"><i class="bi bi-person-vcard text-muted me-1"></i> {{ member.identityCard }}</div>
                <div class="text-dark small mt-1"><i class="bi bi-telephone text-muted me-1"></i> {{ member.phone }}</div>
              </td>
              <td>
                <span class="badge bg-secondary bg-opacity-10 text-dark border">
                  {{ formatRole(member.roleName) }}
                </span>
              </td>
              <td>
                <div v-if="member.isResigned">
                  <span class="badge rounded-pill px-3 py-1.5 fw-bold bg-danger bg-opacity-10 text-danger border border-danger-subtle">
                    <i class="bi bi-x-circle me-1"></i> Đã nghỉ việc
                  </span>
                </div>
                <div v-else>
                  <span v-if="member.isActive" class="badge rounded-pill px-3 py-1.5 fw-bold bg-success bg-opacity-10 text-success border border-success-subtle">
                    <i class="bi bi-check-circle me-1"></i> Đã Kích hoạt
                  </span>
                  <span v-else class="badge rounded-pill px-3 py-1.5 fw-bold bg-warning bg-opacity-10 text-warning border border-warning-subtle">
                    <i class="bi bi-clock me-1"></i> Chờ kích hoạt
                  </span>
                </div>
              </td>
              <td class="text-center">
                <button 
                  class="btn btn-sm btn-light border text-warning hover-bg-warning rounded-circle me-2" 
                  title="Chỉnh sửa"
                  @click="openEditModal(member)"
                >
                  <i class="bi bi-pencil-square"></i>
                </button>
                <button 
                  v-if="!member.isActive && !member.isResigned"
                  class="btn btn-sm btn-light border text-primary hover-bg-primary rounded-circle" 
                  title="Gửi lại link kích hoạt"
                  @click="resendActivation(member.id)"
                >
                  <i class="bi bi-envelope-paper"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal Component -->
    <EmployeeModal 
      :show="showModal" 
      :edit-data="selectedEmployee"
      @close="closeModal"
      @success="handleSuccess"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useEmployeeStore } from '../../stores/employee.store';
import EmployeeModal from './EmployeeModal.vue';
import type { EmployeeDto } from '../../services/employee.service';

const employeeStore = useEmployeeStore();
const searchKeyword = ref('');
const selectedRole = ref('');
const showModal = ref(false);
const selectedEmployee = ref<EmployeeDto | null>(null);

const filteredStaff = computed(() => {
  const keyword = searchKeyword.value.toLowerCase().trim();
  const roleFilter = selectedRole.value;

  return employeeStore.employees.filter(member => {
    const matchesKeyword = !keyword || 
      member.fullName.toLowerCase().includes(keyword) || 
      member.email.toLowerCase().includes(keyword) ||
      member.identityCard.includes(keyword);
      
    const matchesRole = !roleFilter || member.roleName === roleFilter;
    
    return matchesKeyword && matchesRole;
  });
});

const loadStaff = async () => {
  await employeeStore.fetchEmployees();
};

const openAddModal = () => {
  selectedEmployee.value = null;
  showModal.value = true;
};

const openEditModal = (employee: EmployeeDto) => {
  selectedEmployee.value = employee;
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  selectedEmployee.value = null;
};

const handleSuccess = () => {
  closeModal();
};

const resendActivation = async (id: string) => {
  if (confirm('Bạn có chắc chắn muốn gửi lại email kích hoạt cho nhân viên này?')) {
    try {
      const res = await employeeStore.resendActivation(id);
      alert(res.message);
    } catch (e: any) {
      alert(employeeStore.error || 'Có lỗi xảy ra');
    }
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

const formatRole = (role: string) => {
  switch (role) {
    case 'admin': return 'Quản trị viên';
    case 'clinical_doctor': return 'BS. Khám Bệnh';
    case 'vaccination_doctor': return 'BS. Tiêm Chủng';
    case 'receptionist': return 'Lễ tân';
    default: return role;
  }
};

onMounted(() => {
  loadStaff();
});
</script>

<style scoped>
.bg-light-gold {
  background-color: #fdfaf0;
}
.input-premium {
  border: 1px solid #ffeed1;
  transition: all 0.3s ease;
}
.input-premium:focus {
  border-color: #f59e0b;
  box-shadow: none;
}
.avatar-circle-gold {
  border-radius: 50%;
  background: #fdfaf0;
  color: #f59e0b;
  border: 1px solid #ffeed1;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
}
.hover-bg-warning:hover {
  background-color: #fff3cd !important;
  color: #856404 !important;
}
.hover-bg-primary:hover {
  background-color: #cce5ff !important;
  color: #004085 !important;
}
</style>
