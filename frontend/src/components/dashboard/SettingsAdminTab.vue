<template>
  <div class="settings-admin-tab container-fluid p-0">
    <div class="card border-0 shadow-sm rounded-4 p-4 p-md-5 bg-white">
      <div class="mb-4 flex-md-row flex-column d-flex justify-content-between align-items-md-center">
        <div>
          <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-calendar-range text-warning me-2"></i>Cấu hình Khung giờ Hoạt động</h4>
          <p class="text-muted small mb-0">Quản lý giờ mở cửa, ca làm việc và các ngày nghỉ lễ của phòng khám.</p>
        </div>
      </div>

      <!-- Tabs -->
      <div class="mb-4">
        <ul class="nav nav-tabs nav-tabs-premium" role="tablist">
          <li class="nav-item" role="presentation">
            <button class="nav-link fw-bold px-4" :class="{ active: activeTab === 'weekly' }" @click="activeTab = 'weekly'">
              <i class="bi bi-calendar-week me-2"></i> Lịch Hàng Tuần
            </button>
          </li>
          <li class="nav-item" role="presentation">
            <button class="nav-link fw-bold px-4" :class="{ active: activeTab === 'holidays' }" @click="activeTab = 'holidays'">
              <i class="bi bi-calendar-event me-2"></i> Ngày Nghỉ / Lễ
            </button>
          </li>
        </ul>
      </div>

      <div v-if="store.error" class="alert alert-danger alert-dismissible fade show rounded-4" role="alert">
        <i class="bi bi-exclamation-triangle-fill me-2"></i> {{ store.error }}
        <button type="button" class="btn-close" @click="store.error = null" aria-label="Close"></button>
      </div>

      <!-- Tab Content -->
      <div v-if="activeTab === 'weekly'" class="tab-pane-content">
        <div v-if="store.isLoading" class="text-center py-5">
          <div class="spinner-border text-warning" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <p class="text-muted mt-2 small">Đang tải cấu hình...</p>
        </div>
        <div v-else>
          <div class="alert alert-info border-0 bg-info bg-opacity-10 text-dark rounded-4 mb-4 d-flex">
            <i class="bi bi-info-circle-fill text-info fs-5 me-3 mt-1"></i>
            <div>
              <strong>Lưu ý quan trọng:</strong>
              <ul class="mb-0 mt-1 small ps-3">
                <li>Tối đa 3 ca mỗi ngày. Mỗi ca tối thiểu 1 tiếng.</li>
                <li>Thời gian các ca không được chồng lấp lên nhau.</li>
                <li>Bạn không thể đóng cửa vào khung giờ đã có khách hàng đặt lịch hẹn.</li>
              </ul>
            </div>
          </div>

          <div class="border rounded-4 overflow-hidden mb-4">
            <WeeklyDayRow 
              v-for="(_, index) in localWeeklyHours" 
              :key="index" 
              v-model="localWeeklyHours[index]" 
            />
          </div>

          <div class="d-flex justify-content-end border-top pt-4">
            <button @click="saveWeeklyHours" class="btn btn-premium rounded-pill px-5 fw-bold" :disabled="isSaving">
              <span v-if="isSaving" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
              <i v-else class="bi bi-save-fill me-2"></i>
              {{ isSaving ? 'Đang lưu...' : 'Lưu Cấu Hình Hàng Tuần' }}
            </button>
          </div>
        </div>
      </div>

      <div v-if="activeTab === 'holidays'" class="tab-pane-content">
        <div class="d-flex justify-content-end mb-4">
          <button @click="openHolidayModal()" class="btn btn-premium rounded-pill px-4 fw-bold">
            <i class="bi bi-plus-circle-fill me-2"></i> Thêm Ngày Nghỉ
          </button>
        </div>

        <div v-if="store.isLoading" class="text-center py-5">
          <div class="spinner-border text-warning" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
        <HolidayTable v-else :holidays="store.holidays" @edit="openHolidayModal" @delete="confirmDeleteHoliday" />
      </div>

      <!-- Holiday Modal (Bootstrap Style) -->
      <div class="modal fade show" tabindex="-1" style="display: block; background: rgba(0,0,0,0.5); backdrop-filter: blur(4px);" v-if="showModal" aria-modal="true" role="dialog">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content border-0 shadow-lg rounded-4 overflow-hidden">
            <div class="modal-header bg-light border-0 px-4 py-3">
              <h5 class="modal-title fw-bold text-dark">
                <i class="bi" :class="editingHoliday ? 'bi-pencil-square text-warning' : 'bi-plus-circle-fill text-warning'"></i>
                {{ editingHoliday ? 'Cập nhật Ngày nghỉ' : 'Thêm Ngày nghỉ mới' }}
              </h5>
              <button type="button" class="btn-close" @click="closeModal" aria-label="Close"></button>
            </div>
            <div class="modal-body p-4">
              <form @submit.prevent="saveHoliday">
                <div class="mb-3">
                  <label class="form-label fw-bold text-muted small">Tên ngày nghỉ *</label>
                  <input v-model="holidayForm.name" type="text" class="form-control form-control-lg input-premium fs-6" required placeholder="VD: Nghỉ Tết Nguyên Đán">
                </div>
                <div class="row g-3 mb-3">
                  <div class="col-md-6">
                    <label class="form-label fw-bold text-muted small">Từ ngày *</label>
                    <input v-model="holidayForm.startDate" type="date" class="form-control input-premium" required>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-bold text-muted small">Đến ngày *</label>
                    <input v-model="holidayForm.endDate" type="date" class="form-control input-premium" required>
                  </div>
                </div>
                <div class="mb-4" v-if="editingHoliday">
                  <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" role="switch" id="holidayActiveSwitch" v-model="holidayForm.isActive">
                    <label class="form-check-label fw-bold ms-2" for="holidayActiveSwitch" :class="holidayForm.isActive ? 'text-success' : 'text-muted'">
                      {{ holidayForm.isActive ? 'Đang áp dụng' : 'Đã vô hiệu hóa' }}
                    </label>
                  </div>
                </div>
                <div class="d-flex justify-content-end gap-2 mt-4 pt-3 border-top">
                  <button type="button" class="btn btn-light rounded-pill px-4 fw-bold" @click="closeModal">Hủy</button>
                  <button type="submit" class="btn btn-premium rounded-pill px-4 fw-bold" :disabled="isSaving">
                    <span v-if="isSaving" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    {{ isSaving ? 'Đang lưu...' : 'Lưu lại' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useClinicConfigStore } from '../../stores/clinicConfig.store';
import WeeklyDayRow from '../Admin/OperatingHours/WeeklyDayRow.vue';
import HolidayTable from '../Admin/OperatingHours/HolidayTable.vue';

const store = useClinicConfigStore();
const activeTab = ref('weekly');
const isSaving = ref(false);

const localWeeklyHours = ref<any[]>([]);

onMounted(async () => {
  await store.fetchWeeklyHours();
  localWeeklyHours.value = JSON.parse(JSON.stringify(store.weeklyHours));
  await store.fetchHolidays();
});

const saveWeeklyHours = async () => {
  isSaving.value = true;
  try {
    await store.saveWeeklyHours({ days: localWeeklyHours.value });
    alert('Cập nhật khung giờ hoạt động thành công!');
    await store.fetchWeeklyHours();
    localWeeklyHours.value = JSON.parse(JSON.stringify(store.weeklyHours));
  } catch (error) {
    // Error is handled in store and displayed via store.error
  } finally {
    isSaving.value = false;
  }
};

// Holiday Modal Logic
const showModal = ref(false);
const editingHoliday = ref<any>(null);
const holidayForm = ref({
  name: '',
  startDate: '',
  endDate: '',
  isActive: true
});

const openHolidayModal = (holiday: any = null) => {
  editingHoliday.value = holiday;
  if (holiday) {
    holidayForm.value = {
      name: holiday.name,
      startDate: holiday.startDate.substring(0, 10),
      endDate: holiday.endDate.substring(0, 10),
      isActive: holiday.isActive
    };
  } else {
    holidayForm.value = {
      name: '',
      startDate: new Date().toISOString().substring(0, 10),
      endDate: new Date().toISOString().substring(0, 10),
      isActive: true
    };
  }
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  editingHoliday.value = null;
};

const saveHoliday = async () => {
  isSaving.value = true;
  try {
    if (editingHoliday.value) {
      await store.editHoliday(editingHoliday.value.id, holidayForm.value);
      alert('Cập nhật ngày nghỉ thành công!');
    } else {
      await store.addHoliday(holidayForm.value);
      alert('Thêm ngày nghỉ mới thành công!');
    }
    closeModal();
  } catch (error) {
    // Error is handled in store
  } finally {
    isSaving.value = false;
  }
};

const confirmDeleteHoliday = async (id: number) => {
  if (confirm('Bạn có chắc chắn muốn xóa ngày nghỉ này không?')) {
    try {
      await store.removeHoliday(id);
      alert('Xóa ngày nghỉ thành công!');
    } catch (error) {
      // Error handled in store
    }
  }
};
</script>

<style scoped>
.btn-premium {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: white;
  border: none;
  transition: all 0.3s ease;
}
.btn-premium:hover {
  background: linear-gradient(135deg, #d97706 0%, #b45309 100%);
  transform: translateY(-1px);
  box-shadow: 0 4px 6px rgba(245, 158, 11, 0.2);
  color: white;
}
.btn-premium:disabled {
  opacity: 0.7;
  transform: none;
}

.input-premium {
  border: 1px solid #e2e8f0;
  transition: all 0.3s ease;
  background-color: #f8fafc;
}
.input-premium:focus {
  border-color: #f59e0b;
  box-shadow: 0 0 0 0.25rem rgba(245, 158, 11, 0.15);
  background-color: #fff;
}

.nav-tabs-premium {
  border-bottom: 2px solid #f1f5f9;
}
.nav-tabs-premium .nav-link {
  color: #64748b;
  border: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  transition: all 0.3s ease;
}
.nav-tabs-premium .nav-link:hover {
  color: #f59e0b;
  border-color: transparent;
}
.nav-tabs-premium .nav-link.active {
  color: #d97706;
  background: transparent;
  border-color: #f59e0b;
}

.tab-pane-content {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
