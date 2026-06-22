<template>
  <div v-if="show" class="zalo-modal-overlay" @click.self="close">
    <div class="zalo-modal-card import-modal-card shadow-lg">
      <div class="zalo-modal-header bg-success text-white">
        <h5 class="modal-title fw-bold">
          <i class="bi bi-box-arrow-in-down me-2"></i> Lập Phiếu Nhập Kho Dược Phẩm
        </h5>
        <button class="modal-close text-white border-0 bg-transparent" @click="close">
          <i class="bi bi-x-lg fs-5"></i>
        </button>
      </div>

      <div class="zalo-modal-body text-start p-4 bg-light">
        <div class="row g-3 mb-4 bg-white p-3 rounded-3 shadow-sm border">
          <div class="col-md-6">
            <label class="form-label small fw-bold text-muted">Mã chứng từ / Hoá đơn tham chiếu</label>
            <input type="text" v-model="referenceCode" class="form-control" placeholder="VD: HD-12345" />
          </div>
          <div class="col-md-6">
            <label class="form-label small fw-bold text-muted">Ghi chú phiếu nhập</label>
            <input type="text" v-model="notes" class="form-control" placeholder="Lý do nhập kho..." />
          </div>
        </div>

        <div class="bg-white p-3 rounded-3 shadow-sm border mb-3">
          <div class="d-flex justify-content-between align-items-center mb-3 border-bottom pb-2">
            <h6 class="fw-bold mb-0 text-dark">Chi tiết lô thuốc nhập</h6>
            <button class="btn btn-sm btn-outline-success rounded-pill fw-bold" @click="addRow">
              <i class="bi bi-plus-circle me-1"></i>Thêm dòng
            </button>
          </div>

          <div class="table-responsive">
            <table class="table table-bordered align-middle">
              <thead class="table-light text-muted small text-center">
                <tr>
                  <th style="width: 25%">Tên dược phẩm *</th>
                  <th style="width: 15%">Số Lô *</th>
                  <th style="width: 15%">Ngày sản xuất</th>
                  <th style="width: 15%">Hạn sử dụng *</th>
                  <th style="width: 15%">Số lượng *</th>
                  <th style="width: 10%">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="items.length === 0">
                  <td colspan="6" class="text-center py-4 text-muted">
                    Chưa có dược phẩm nào trong danh sách. Bấm "Thêm dòng" để bắt đầu.
                  </td>
                </tr>
                <tr v-for="(item, index) in items" :key="index">
                  <td>
                    <select v-model="item.medicineId" class="form-select form-select-sm" required>
                      <option value="0" disabled>-- Chọn thuốc --</option>
                      <option v-for="med in medicines" :key="med.id" :value="med.id">
                        {{ med.name }}
                      </option>
                    </select>
                  </td>
                  <td>
                    <input type="text" v-model="item.batchNumber" class="form-control form-control-sm" required placeholder="Số lô..." />
                  </td>
                  <td>
                    <input type="date" v-model="item.manufactureDate" class="form-control form-control-sm" required />
                  </td>
                  <td>
                    <input type="date" v-model="item.expiryDate" class="form-control form-control-sm" required />
                  </td>
                  <td>
                    <input type="number" v-model="item.quantity" class="form-control form-control-sm" required min="1" />
                  </td>
                  <td class="text-center">
                    <button class="btn btn-sm btn-outline-danger border-0" @click="removeRow(index)" title="Xoá dòng này">
                      <i class="bi bi-trash-fill"></i>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="mt-4 pt-3 border-top text-end">
          <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="close" :disabled="isSubmitting">Huỷ bỏ</button>
          <button type="button" class="btn btn-success rounded-pill px-5 fw-bold" @click="submitImport" :disabled="isSubmitting || items.length === 0">
            <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-2"></span>
            Hoàn tất Nhập Kho
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, defineProps, defineEmits, watch } from 'vue';
import api from '../../services/api';

const props = defineProps<{
  show: boolean;
}>();

const emit = defineEmits(['close', 'success']);

const isSubmitting = ref(false);
const medicines = ref<any[]>([]);

const referenceCode = ref('');
const notes = ref('');
const items = ref<any[]>([]);

const loadMedicines = async () => {
  try {
    const res = await api.get('/admin/medicines');
    medicines.value = res.data || [];
  } catch (err) {
    console.error('Lỗi tải danh mục thuốc:', err);
  }
};

watch(() => props.show, (newVal) => {
  if (newVal) {
    loadMedicines();
    items.value = [];
    referenceCode.value = '';
    notes.value = '';
    addRow();
  }
});

const addRow = () => {
  items.value.push({
    medicineId: 0,
    batchNumber: '',
    manufactureDate: '',
    expiryDate: '',
    quantity: 1
  });
};

const removeRow = (index: number) => {
  items.value.splice(index, 1);
};

const validateForm = () => {
  for (let i = 0; i < items.value.length; i++) {
    const item = items.value[i];
    if (!item.medicineId || item.medicineId === 0) return `Dòng ${i + 1}: Vui lòng chọn dược phẩm.`;
    if (!item.batchNumber) return `Dòng ${i + 1}: Vui lòng nhập số lô.`;
    if (!item.expiryDate) return `Dòng ${i + 1}: Vui lòng nhập hạn sử dụng.`;
    if (!item.manufactureDate) return `Dòng ${i + 1}: Vui lòng nhập ngày sản xuất.`;
    if (item.quantity <= 0) return `Dòng ${i + 1}: Số lượng phải lớn hơn 0.`;
    
    if (new Date(item.manufactureDate) > new Date()) return `Dòng ${i + 1}: Ngày sản xuất không được lớn hơn ngày hiện tại.`;
    if (new Date(item.expiryDate) <= new Date()) return `Dòng ${i + 1}: Hạn sử dụng phải lớn hơn ngày hiện tại.`;
  }
  return null;
};

const submitImport = async () => {
  const error = validateForm();
  if (error) {
    alert(error);
    return;
  }

  isSubmitting.value = true;
  let successCount = 0;
  try {
    for (const item of items.value) {
      await api.post('/medicines/import', {
        medicineId: item.medicineId,
        batchNumber: item.batchNumber,
        manufactureDate: item.manufactureDate,
        expiryDate: item.expiryDate,
        quantity: item.quantity,
        referenceCode: referenceCode.value,
        notes: notes.value
      });
      successCount++;
    }
    emit('success', `Đã nhập kho thành công ${successCount} lô thuốc!`);
    close();
  } catch (err: any) {
    console.error('Lỗi nhập kho:', err);
    alert(err.response?.data?.message || `Lỗi khi nhập kho sau ${successCount} lô thành công.`);
  } finally {
    isSubmitting.value = false;
  }
};

const close = () => {
  emit('close');
};
</script>

<style scoped>
.zalo-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(5px);
  z-index: 1200;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}
.import-modal-card {
  width: 100%;
  max-width: 900px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}
.zalo-modal-body {
  overflow-y: auto;
}
</style>
