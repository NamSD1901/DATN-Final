<template>
  <div class="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
    <div class="table-responsive">
      <table class="table table-hover align-middle mb-0">
        <thead class="bg-light">
          <tr>
            <th scope="col" class="border-0 text-muted small py-3 ps-4 text-uppercase fw-bold">Tên ngày nghỉ</th>
            <th scope="col" class="border-0 text-muted small py-3 text-uppercase fw-bold">Từ ngày</th>
            <th scope="col" class="border-0 text-muted small py-3 text-uppercase fw-bold">Đến ngày</th>
            <th scope="col" class="border-0 text-muted small py-3 text-uppercase fw-bold">Trạng thái</th>
            <th scope="col" class="border-0 text-muted small py-3 pe-4 text-end text-uppercase fw-bold">Thao tác</th>
          </tr>
        </thead>
        <tbody class="border-top-0">
          <tr v-for="holiday in holidays" :key="holiday.id">
            <td class="py-3 ps-4 fw-bold text-dark">
              {{ holiday.name }}
            </td>
            <td class="py-3 text-muted">
              {{ formatDate(holiday.startDate) }}
            </td>
            <td class="py-3 text-muted">
              {{ formatDate(holiday.endDate) }}
            </td>
            <td class="py-3">
              <span class="badge rounded-pill px-3 py-2 fw-bold shadow-sm" :class="holiday.isActive ? 'bg-success bg-opacity-10 text-success' : 'bg-danger bg-opacity-10 text-danger'">
                <i class="bi me-1" :class="holiday.isActive ? 'bi-check-circle-fill' : 'bi-x-circle-fill'"></i>
                {{ holiday.isActive ? 'Đang áp dụng' : 'Đã vô hiệu hóa' }}
              </span>
            </td>
            <td class="py-3 pe-4 text-end">
              <button @click="$emit('edit', holiday)" class="btn btn-sm btn-light rounded-circle shadow-sm me-2 hover-btn-warning" title="Chỉnh sửa">
                <i class="bi bi-pencil-fill text-warning"></i>
              </button>
              <button @click="$emit('delete', holiday.id)" class="btn btn-sm btn-light rounded-circle shadow-sm hover-btn-danger" title="Xóa">
                <i class="bi bi-trash-fill text-danger"></i>
              </button>
            </td>
          </tr>
          <tr v-if="holidays.length === 0">
            <td colspan="5" class="text-center py-5 text-muted">
              <i class="bi bi-calendar-x fs-1 d-block mb-3 opacity-50"></i>
              Chưa có cấu hình ngày nghỉ nào được thiết lập.
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">

defineProps({
  holidays: {
    type: Array as () => any[],
    required: true
  }
});

defineEmits(['edit', 'delete']);

const formatDate = (dateString: string) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('vi-VN');
};
</script>

<style scoped>
.hover-btn-warning:hover {
  background-color: #ffc107 !important;
}
.hover-btn-warning:hover i {
  color: white !important;
}

.hover-btn-danger:hover {
  background-color: #dc3545 !important;
}
.hover-btn-danger:hover i {
  color: white !important;
}
</style>
