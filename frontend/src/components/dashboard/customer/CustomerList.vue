<template>
  <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
    <!-- Header Actions -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
      <div>
        <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-people-fill text-warning me-2"></i>Quản lý Khách hàng</h4>
        <p class="text-muted small mb-0">Tra cứu nhanh SĐT, tạo mới hồ sơ chủ nuôi và các bé thú cưng</p>
      </div>
      <button class="btn btn-premium px-4 py-2.5 rounded-pill shadow-sm" @click="$emit('open-create-customer')">
        <i class="bi bi-person-plus-fill me-2"></i> Thêm Khách Hàng Mới
      </button>
    </div>

    <!-- Quick Phone Search -->
    <div class="card bg-gold-gradient border-warning border-opacity-50 rounded-4 p-3 mb-4">
      <div class="row align-items-center g-3">
        <div class="col-auto">
          <span class="fw-bold text-dark"><i class="bi bi-search me-1 text-warning"></i>Tìm nhanh SĐT:</span>
        </div>
        <div class="col-md-5">
          <div class="input-group">
            <span class="input-group-text bg-white border-end-0 rounded-start-pill"><i class="bi bi-phone"></i></span>
            <input 
              type="text" 
              v-model="quickPhoneQuery" 
              @input="handleQuickPhoneSearch"
              class="form-control border-start-0 rounded-end-pill input-premium" 
              placeholder="Nhập số điện thoại để tra cứu nhanh..."
            />
          </div>
        </div>
        <div class="col-auto text-muted small">
          ↳ Tra cứu nhanh bệnh lịch cũ khi khách vừa đến quầy tiếp đón
        </div>
      </div>

      <!-- Quick search result card -->
      <div v-if="quickSearchResult" class="mt-3 bg-white p-3 rounded-4 shadow-sm border border-warning border-opacity-25 animate-fade-in">
        <div class="d-flex justify-content-between align-items-start flex-wrap gap-3">
          <div class="d-flex align-items-center gap-3">
            <div class="avatar-circle-gold fs-5">{{ getAvatarLetters(quickSearchResult.fullName) }}</div>
            <div>
              <h6 class="fw-bold text-dark mb-1">{{ quickSearchResult.fullName }}</h6>
              <div class="text-muted small">
                <i class="bi bi-phone me-1"></i>{{ quickSearchResult.phone || 'Chưa cung cấp' }} | 
                <i class="bi bi-envelope me-1"></i>{{ quickSearchResult.email || 'Không có email' }}
              </div>
            </div>
          </div>
          <div class="d-flex gap-2">
            <button class="btn btn-sm btn-outline-warning rounded-pill px-3 fw-bold" @click="$emit('view-detail', quickSearchResult.customerId)">
              <i class="bi bi-eye me-1"></i>Xem Hồ Sơ
            </button>
            <button class="btn btn-sm btn-premium rounded-pill px-3" @click="$emit('add-pet', quickSearchResult.customerId)">
              <i class="bi bi-plus-circle me-1"></i>Thêm Thú Cưng
            </button>
          </div>
        </div>
        
        <!-- Quick Pets view -->
        <div class="mt-3 border-top pt-2" v-if="quickSearchResult.pets && quickSearchResult.pets.length > 0">
          <span class="small text-muted fw-bold d-block mb-1">Thú cưng đã đăng ký:</span>
          <div class="d-flex flex-wrap gap-2">
            <span 
              v-for="pet in quickSearchResult.pets" 
              :key="pet.id" 
              class="badge bg-light text-dark border px-3 py-1.5 rounded-pill"
            >
              {{ getAnimalEmoji(pet.species) }} {{ pet.name }} ({{ pet.breed || pet.species }})
            </span>
          </div>
        </div>
      </div>

      <div v-if="quickPhoneQuery && quickSearchResult === null" class="mt-3 alert alert-warning rounded-4 mb-0 d-flex align-items-center gap-2">
        <i class="bi bi-exclamation-triangle-fill fs-5"></i>
        <span>Không tìm thấy chủ nuôi. <a href="#" class="fw-bold text-warning" @click.prevent="$emit('create-customer-with-phone', quickPhoneQuery)">Tạo hồ sơ mới với số điện thoại này?</a></span>
      </div>
    </div>

    <!-- Advanced Filters & Table -->
    <div class="row g-2 mb-3 align-items-center">
      <div class="col-md-6">
        <div class="input-group">
          <span class="input-group-text bg-white border-end-0"><i class="bi bi-search text-muted"></i></span>
          <input 
            type="text" 
            v-model="searchKeyword" 
            @input="debouncedSearch"
            class="form-control border-start-0 input-premium" 
            placeholder="Tìm kiếm theo tên hoặc email..."
          />
        </div>
      </div>
      <div class="col-md-6 text-md-end text-muted small">
        Tổng số: <strong class="text-dark">{{ totalCustomers }}</strong> khách hàng
      </div>
    </div>

    <!-- Customers Table -->
    <div class="table-responsive rounded-4 border overflow-hidden mt-3">
      <table class="table table-hover align-middle mb-0">
        <thead class="bg-light-gold">
          <tr>
            <th class="ps-4">Khách hàng</th>
            <th>Số điện thoại</th>
            <th>Địa chỉ</th>
            <th>Ngày tham gia</th>
            <th class="text-center">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading" class="text-center">
            <td colspan="5" class="py-5">
              <div class="spinner-border text-warning spinner-border-sm me-2"></div>
              <span class="text-muted">Đang tải danh sách khách hàng...</span>
            </td>
          </tr>
          <tr v-else-if="customersList.length === 0" class="text-center">
            <td colspan="5" class="py-5 text-muted">
              <i class="bi bi-people fs-2 mb-2 d-block"></i>
              Không tìm thấy khách hàng nào.
            </td>
          </tr>
          <tr v-for="cust in customersList" :key="cust.id" v-else>
            <td class="ps-4">
              <div class="d-flex align-items-center gap-3">
                <div class="avatar-circle-gold" style="width: 38px; height: 38px; font-size: 0.95rem;">
                  {{ getAvatarLetters(cust.fullName) }}
                </div>
                <div>
                  <div class="fw-bold text-dark">{{ cust.fullName }}</div>
                  <small class="text-muted">{{ cust.email }}</small>
                </div>
              </div>
            </td>
            <td>
              <span class="badge bg-warning bg-opacity-10 text-dark-gold border border-warning border-opacity-20 px-3 py-1.5 rounded-pill">
                <i class="bi bi-phone me-1"></i>{{ cust.phone || 'Chưa cung cấp' }}
              </span>
            </td>
            <td class="text-muted small text-truncate" style="max-width: 250px;">{{ cust.address || 'Chưa cung cấp' }}</td>
            <td class="text-muted small">{{ formatDate(cust.createdAt) }}</td>
            <td class="text-center">
              <button class="btn btn-sm btn-outline-warning rounded-pill px-4 fw-bold shadow-sm" @click="$emit('view-detail', cust.id)">
                <i class="bi bi-eye me-1"></i>Xem Hồ Sơ
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../../../services/api';

defineEmits(['view-detail', 'open-create-customer', 'create-customer-with-phone', 'add-pet']);

const loading = ref(false);
const customersList = ref<any[]>([]);
const totalCustomers = ref(0);
const searchKeyword = ref('');

const quickPhoneQuery = ref('');
const quickSearchResult = ref<any>(null);

const loadCustomers = async () => {
  loading.value = true;
  try {
    const res = await api.get(`/receptionist/customers?search=${encodeURIComponent(searchKeyword.value)}`);
    customersList.value = res.data || [];
    totalCustomers.value = customersList.value.length;
  } catch (err) {
    console.error('Lỗi tải danh sách khách hàng:', err);
  } finally {
    loading.value = false;
  }
};

let searchTimer: any = null;
const debouncedSearch = () => {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(() => {
    loadCustomers();
  }, 400);
};

let phoneTimer: any = null;
const handleQuickPhoneSearch = () => {
  clearTimeout(phoneTimer);
  phoneTimer = setTimeout(async () => {
    const phone = quickPhoneQuery.value.trim();
    if (!phone) {
      quickSearchResult.value = null;
      return;
    }
    try {
      const res = await api.get(`/receptionist/quick-search?phone=${encodeURIComponent(phone)}`);
      if (res.data && res.data.found) {
        quickSearchResult.value = res.data;
      } else {
        quickSearchResult.value = null;
      }
    } catch (err) {
      console.error(err);
      quickSearchResult.value = null;
    }
  }, 400);
};

// UI Helpers
const getAvatarLetters = (name: string) => {
  if (!name) return '?';
  const parts = name.trim().split(/\s+/);
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
  return name.slice(0, 1).toUpperCase();
};

const getAnimalEmoji = (species: string) => {
  const s = (species || '').toLowerCase();
  if (s.includes('chó') || s.includes('dog')) return '🐶';
  if (s.includes('mèo') || s.includes('cat')) return '🐱';
  return '🐾';
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

onMounted(() => {
  loadCustomers();
});
</script>

<style scoped>
.bg-light-gold {
  background-color: #fdfaf0;
}
.text-dark-gold {
  color: #d97706;
}
.avatar-circle-gold {
  width: 45px;
  height: 45px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--primary-gold, #f59e0b), #d97706);
  color: white;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}

.animate-fade-in {
  animation: fadeIn 0.35s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
