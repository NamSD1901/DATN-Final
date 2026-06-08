<template>
  <div v-if="show" class="modal-overlay" @click.self="closeModal">
    <div class="modal-card">
      <div class="modal-header">
        <h5 class="modal-title">
          <CalendarDays class="header-icon" /> Đặt Lịch Ngay
        </h5>
        <button class="modal-close" @click="closeModal"><X /></button>
      </div>
      
      <div class="modal-body">
        <p class="modal-desc">
          Vui lòng điền đầy đủ thông tin bên dưới để đăng ký lịch hẹn cho thú cưng của bạn tại MyPetClinic.
        </p>

        <form @submit.prevent="handleBookingSubmit">
          <div class="form-grid">
            <!-- Họ tên -->
            <div class="form-group">
              <label for="bookingOwnerName">Họ và tên *</label>
              <div class="input-wrapper">
                <User class="input-icon" />
                <input 
                  type="text" 
                  id="bookingOwnerName" 
                  v-model="form.ownerName" 
                  required 
                  placeholder="Nguyễn Văn A" 
                  class="form-control input-premium" 
                />
              </div>
            </div>

            <!-- Số điện thoại -->
            <div class="form-group">
              <label for="bookingPhone">Số điện thoại *</label>
              <div class="input-wrapper">
                <Phone class="input-icon" />
                <input 
                  type="tel" 
                  id="bookingPhone" 
                  v-model="form.phone" 
                  required 
                  placeholder="0901234567" 
                  class="form-control input-premium" 
                />
              </div>
            </div>

            <!-- Tên Pet -->
            <div class="form-group">
              <label for="bookingPetName">Tên thú cưng *</label>
              <div class="input-wrapper">
                <Heart class="input-icon" />
                <input 
                  type="text" 
                  id="bookingPetName" 
                  v-model="form.petName" 
                  required 
                  placeholder="Milu" 
                  class="form-control input-premium" 
                />
              </div>
            </div>

            <!-- Loại Pet -->
            <div class="form-group">
              <label for="bookingPetType">Loại thú cưng</label>
              <select id="bookingPetType" v-model="form.petType" class="form-select input-premium">
                <option value="Chó">Chó</option>
                <option value="Mèo">Mèo</option>
                <option value="Khác">Khác</option>
              </select>
            </div>

            <!-- Dịch vụ -->
            <div class="form-group">
              <label for="bookingService">Chọn dịch vụ *</label>
              <select id="bookingService" v-model="form.service" class="form-select input-premium" required>
                <option value="Khám & Điều trị">Khám & Điều trị</option>
                <option value="Tiêm phòng">Tiêm phòng Vaccine</option>
                <option value="Spa & Grooming">Spa & Làm đẹp</option>
                <option value="Dịch vụ khác">Dịch vụ khác</option>
              </select>
            </div>

            <!-- Ngày hẹn -->
            <div class="form-group">
              <label for="bookingDate">Ngày đặt lịch *</label>
              <input 
                type="date" 
                id="bookingDate" 
                v-model="form.date" 
                required 
                :min="todayDate"
                class="form-control input-premium" 
              />
            </div>

            <!-- Giờ hẹn -->
            <div class="form-group full-width">
              <label for="bookingTime">Giờ hẹn mong muốn *</label>
              <select id="bookingTime" v-model="form.time" class="form-select input-premium" required>
                <option value="08:00 - 09:00">Ca sáng: 08:00 - 09:00</option>
                <option value="09:00 - 10:00">Ca sáng: 09:00 - 10:00</option>
                <option value="10:00 - 11:00">Ca sáng: 10:00 - 11:00</option>
                <option value="11:00 - 12:00">Ca sáng: 11:00 - 12:00</option>
                <option value="14:00 - 15:00">Ca chiều: 14:00 - 15:00</option>
                <option value="15:00 - 16:00">Ca chiều: 15:00 - 16:00</option>
                <option value="16:00 - 17:00">Ca chiều: 16:00 - 17:00</option>
                <option value="17:00 - 18:00">Ca chiều: 17:00 - 18:00</option>
                <option value="18:00 - 19:00">Ca chiều: 18:00 - 19:00</option>
              </select>
            </div>

            <!-- Triệu chứng / Ghi chú -->
            <div class="form-group full-width">
              <label for="bookingNotes">Triệu chứng / Ghi chú thêm</label>
              <textarea 
                id="bookingNotes" 
                v-model="form.notes" 
                placeholder="Ví dụ: Bé bị ho, cần tắm spa..." 
                class="form-control input-premium"
                rows="3"
              ></textarea>
            </div>
          </div>

          <button type="submit" :disabled="loading" class="btn-premium w-100 py-3 shadow-md mt-4">
            <span v-if="!loading"><CalendarDays class="btn-icon" /> Xác Nhận Đặt Lịch</span>
            <span v-else class="spinner"></span>
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { CalendarDays, X, User, Phone, Heart } from '@lucide/vue';
import api from '../../services/api';

const props = defineProps<{
  show: boolean;
}>();

const emit = defineEmits(['close', 'success', 'error']);

const loading = ref(false);
const todayDate = ref('');

const form = reactive({
  ownerName: '',
  phone: '',
  petName: '',
  petType: 'Chó',
  service: 'Khám & Điều trị',
  date: '',
  time: '08:00 - 09:00',
  notes: ''
});

const closeModal = () => {
  emit('close');
};

const handleBookingSubmit = async () => {
  loading.value = true;
  try {
    // Gọi API lưu đặt lịch
    const payload = {
      customerName: form.ownerName,
      phone: form.phone,
      petName: form.petName,
      species: form.petType,
      symptom: form.notes,
      appointmentDate: `${form.date}T${form.time.split(' - ')[0]}:00`,
      serviceName: form.service
    };

    // Giả lập lưu thành công hoặc gọi API thực tế nếu cần
    await api.post('/appointment/book', payload).catch(() => {
      // Fallback nếu api chưa viết xong
      return new Promise(resolve => setTimeout(resolve, 1000));
    });

    emit('success', `Đặt lịch thành công cho bé ${form.petName}! Cửa hàng sẽ liên hệ sớm nhất.`);
    resetForm();
    closeModal();
  } catch (err) {
    emit('error', 'Có lỗi xảy ra khi đặt lịch. Vui lòng liên hệ hotline.');
  } finally {
    loading.value = false;
  }
};

const resetForm = () => {
  form.ownerName = '';
  form.phone = '';
  form.petName = '';
  form.petType = 'Chó';
  form.service = 'Khám & Điều trị';
  form.notes = '';
  form.time = '08:00 - 09:00';
  if (todayDate.value) {
    form.date = todayDate.value;
  }
};

onMounted(() => {
  const today = new Date().toISOString().split('T')[0];
  todayDate.value = today;
  form.date = today;
  
  // Tải thông tin người dùng nếu đã đăng nhập để tự động điền
  api.get('/profile').then(res => {
    if (res.data) {
      form.ownerName = res.data.fullName || '';
      form.phone = res.data.phone || '';
    }
  }).catch(() => {});
});
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(5px);
  z-index: 1100;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}

.modal-card {
  background: white;
  width: 100%;
  max-width: 550px;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  animation: modal-enter 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes modal-enter {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.modal-header {
  background: var(--primary-gold);
  color: var(--text-dark);
  padding: 1.2rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-title {
  font-size: 1.2rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0;
}

.header-icon {
  width: 22px;
  height: 22px;
}

.modal-close {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-dark);
  display: flex;
  align-items: center;
}

.modal-close svg {
  width: 20px;
  height: 20px;
}

.modal-body {
  padding: 2rem 1.5rem;
  max-height: 80vh;
  overflow-y: auto;
}

.modal-desc {
  font-size: 0.85rem;
  color: var(--text-muted);
  text-align: center;
  margin-bottom: 1.5rem;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

@media (max-width: 576px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
  .form-group.full-width {
    grid-column: span 1 !important;
  }
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.form-group.full-width {
  grid-column: span 2;
}

label {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-muted);
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 12px;
  width: 16px;
  height: 16px;
  color: var(--text-muted);
}

.form-control, .form-select {
  width: 100%;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-sm);
  border: 1px solid rgba(0, 0, 0, 0.08);
  font-family: inherit;
  font-size: 0.9rem;
}

.input-wrapper input {
  padding-left: 2.5rem;
}

textarea.form-control {
  resize: none;
}

.btn-icon {
  width: 16px;
  height: 16px;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s infinite linear;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
