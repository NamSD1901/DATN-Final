<template>
  <div v-if="show" class="modal-overlay glass-overlay" @click.self="$emit('close')">
    <div class="modal-card glass-card">
      <div class="modal-header border-bottom px-4 py-3 bg-white bg-opacity-75 d-flex justify-content-between align-items-center w-100">
        <h5 class="fw-bold text-dark mb-0">
          <i :class="isEdit ? 'bi-pencil-square text-warning' : 'bi-person-plus-fill text-warning'" class="bi me-2"></i>
          {{ isEdit ? 'Cập Nhật Nhân Viên' : 'Thêm Nhân Viên Mới' }}
        </h5>
        <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('close')"></button>
      </div>

      <div class="modal-body p-4 bg-white bg-opacity-50">
        <form @submit.prevent="handleSubmit">
          <div class="row g-3">
            <div class="col-md-12">
              <label class="form-label small fw-bold text-muted mb-1">Họ và Tên <span class="text-danger">*</span></label>
              <input type="text" v-model="form.fullName" class="form-control form-control-sm border-warning-subtle" required placeholder="Nguyễn Văn A" />
            </div>

            <div class="col-md-6">
              <label class="form-label small fw-bold text-muted mb-1">Email <span class="text-danger">*</span></label>
              <input type="email" v-model="form.email" class="form-control form-control-sm border-warning-subtle" :disabled="isEdit" required placeholder="email@example.com" />
              <small v-if="!isEdit" class="text-muted d-block mt-1" style="font-size: 0.7rem;">Hệ thống sẽ gửi link kích hoạt đến email này.</small>
            </div>

            <div class="col-md-6">
              <label class="form-label small fw-bold text-muted mb-1">Số điện thoại <span class="text-danger">*</span></label>
              <input type="text" v-model="form.phone" class="form-control form-control-sm border-warning-subtle" required placeholder="09xxxxxx" />
            </div>

            <div class="col-md-6">
              <label class="form-label small fw-bold text-muted mb-1">Căn Cước Công Dân <span class="text-danger">*</span></label>
              <input type="text" v-model="form.identityCard" class="form-control form-control-sm border-warning-subtle" :disabled="isEdit" required placeholder="12 số CCCD" />
            </div>

            <div class="col-md-6">
              <label class="form-label small fw-bold text-muted mb-1">Vai trò / Chức vụ <span class="text-danger">*</span></label>
              <select v-model="form.roleName" class="form-select form-select-sm border-warning-subtle" :disabled="isEdit" required>
                <option value="clinical_doctor">Bác sĩ khám bệnh</option>
                <option value="vaccination_doctor">Bác sĩ tiêm chủng</option>
                <option value="receptionist">Lễ tân</option>
                <option value="admin">Quản trị viên (Admin)</option>
              </select>
            </div>

            <div class="col-md-6">
              <label class="form-label small fw-bold text-muted mb-1">Ngày sinh <span class="text-danger">*</span></label>
              <input type="date" v-model="form.dateOfBirth" class="form-control form-control-sm border-warning-subtle" required />
            </div>

            <div class="col-md-6">
              <label class="form-label small fw-bold text-muted mb-1">Giới tính</label>
              <select v-model="form.gender" class="form-select form-select-sm border-warning-subtle">
                <option :value="1">Nam</option>
                <option :value="2">Nữ</option>
                <option :value="3">Khác</option>
              </select>
            </div>

            <div class="col-md-12">
              <label class="form-label small fw-bold text-muted mb-1">Địa chỉ liên hệ</label>
              <textarea v-model="form.address" class="form-control form-control-sm border-warning-subtle" rows="2" placeholder="Nhập địa chỉ..."></textarea>
            </div>

            <div class="col-md-12" v-if="isEdit">
               <div class="form-check form-switch mt-2">
                 <input class="form-check-input" type="checkbox" role="switch" id="resignSwitch" v-model="form.isResigned">
                 <label class="form-check-label text-danger fw-bold ms-1" for="resignSwitch">Đánh dấu Đã Nghỉ Việc (Khóa tài khoản)</label>
               </div>
            </div>
          </div>

          <div class="d-flex justify-content-end gap-2 mt-4">
            <button type="button" class="btn btn-sm btn-light fw-bold border" @click="$emit('close')">Hủy</button>
            <button type="submit" class="btn btn-sm btn-warning fw-bold px-4 shadow-sm" :disabled="loading">
              <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
              {{ isEdit ? 'Lưu Thay Đổi' : 'Thêm & Gửi Email' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import type { CreateEmployeeRequest, UpdateEmployeeRequest, EmployeeDto } from '../../services/employee.service';
import { useEmployeeStore } from '../../stores/employee.store';

const props = defineProps<{
  show: boolean;
  editData?: EmployeeDto | null;
}>();

const emit = defineEmits(['close', 'success']);
const employeeStore = useEmployeeStore();
const loading = ref(false);

const isEdit = computed(() => !!props.editData);

const form = ref<any>({
  email: '',
  fullName: '',
  phone: '',
  identityCard: '',
  roleName: 'clinical_doctor',
  gender: 1,
  dateOfBirth: '',
  address: '',
  isResigned: false
});

watch(() => props.show, (newVal) => {
  if (newVal) {
    if (props.editData) {
      form.value = {
        email: props.editData.email,
        fullName: props.editData.fullName,
        phone: props.editData.phone,
        identityCard: props.editData.identityCard,
        roleName: props.editData.roleName,
        gender: props.editData.gender || 1,
        dateOfBirth: props.editData.dateOfBirth ? new Date(props.editData.dateOfBirth).toISOString().split('T')[0] : '',
        address: props.editData.address || '',
        isResigned: props.editData.isResigned
      };
    } else {
      form.value = {
        email: '',
        fullName: '',
        phone: '',
        identityCard: '',
        roleName: 'clinical_doctor',
        gender: 1,
        dateOfBirth: '',
        address: '',
        isResigned: false
      };
    }
  }
});

const handleSubmit = async () => {
  loading.value = true;
  try {
    if (isEdit.value && props.editData) {
      const updateData: UpdateEmployeeRequest = {
        fullName: form.value.fullName,
        phone: form.value.phone,
        gender: form.value.gender,
        dateOfBirth: new Date(form.value.dateOfBirth).toISOString(),
        address: form.value.address,
        isResigned: form.value.isResigned
      };
      await employeeStore.updateEmployee(props.editData.id, updateData);
      alert('Cập nhật nhân viên thành công!');
    } else {
      const createData: CreateEmployeeRequest = {
        email: form.value.email,
        fullName: form.value.fullName,
        phone: form.value.phone,
        identityCard: form.value.identityCard,
        roleName: form.value.roleName,
        gender: form.value.gender,
        dateOfBirth: new Date(form.value.dateOfBirth).toISOString(),
        address: form.value.address
      };
      const res = await employeeStore.createEmployee(createData);
      alert(res.message);
    }
    emit('success');
  } catch (error: any) {
    alert(errorStoreMessage());
  } finally {
    loading.value = false;
  }
};

const errorStoreMessage = () => employeeStore.error || 'Có lỗi xảy ra';
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 1050;
  display: flex;
  justify-content: center;
  align-items: center;
}
.glass-overlay {
  background: rgba(0, 0, 0, 0.35);
  backdrop-filter: blur(8px);
}
.modal-card {
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  border-radius: 16px;
  box-shadow: 0 15px 35px rgba(0,0,0,0.2);
}
.modal-card.glass-card:hover {
  transform: none !important;
  box-shadow: 0 15px 35px rgba(0,0,0,0.2) !important;
  border-color: rgba(255, 255, 255, 0.5) !important;
}
.glass-card {
  background: rgba(255, 255, 255, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.5);
}
.border-warning-subtle {
  border-color: #ffda85;
}
.border-warning-subtle:focus {
  border-color: #f59e0b;
  box-shadow: 0 0 0 0.2rem rgba(245, 158, 11, 0.15);
}
</style>
