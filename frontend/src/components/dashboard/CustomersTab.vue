<template>
  <div class="customers-tab container-fluid p-0">
    <!-- Main Panel: Lists and Search -->
    <CustomerList 
      v-if="!selectedCustomerDetail" 
      @view-detail="viewCustomerDetail"
      @open-create-customer="openCreateCustomerModal"
      @create-customer-with-phone="openCreateCustomerModalWithPhone"
      @add-pet="openAddPetModalDirect"
    />

    <!-- Customer Detailed Medical History Profile -->
    <CustomerDetail 
      v-else 
      ref="customerDetailRef"
      :customerId="selectedCustomerDetail"
      @back="selectedCustomerDetail = null"
      @add-pet="openAddPetModalDirect"
      @edit-pet="openEditPetModal"
      @view-pet-history="openPetHistoryModal"
    />

    <!-- Modal Comprehensive Medical Profile (PetDetail with Chart) -->
    <PetDetail 
      :isOpen="showPetHistoryModal" 
      :pet="selectedPetHistory" 
      @close="showPetHistoryModal = false" 
    />

    <!-- Modal 1: Create Customer & Pets -->
    <div v-if="showCreateCustomerModal" class="zalo-modal-overlay" @click.self="showCreateCustomerModal = false">
      <div class="zalo-modal-card modal-lg max-w-700">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold"><i class="bi bi-person-plus-fill me-2"></i> Đăng Ký Hồ Sơ Khách Hàng Mới</h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showCreateCustomerModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="submitCreateCustomer">
            <div class="row g-3">
              <!-- Customer details -->
              <div class="col-md-6 border-end pe-md-4">
                <h6 class="text-warning fw-bold mb-3 border-bottom pb-2">1. Thông tin Chủ nuôi</h6>
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Họ và tên *</label>
                  <input type="text" v-model="createForm.fullName" class="form-control input-premium" required placeholder="Nhập họ và tên..." />
                </div>
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Số điện thoại *</label>
                  <input type="text" v-model="createForm.phone" class="form-control input-premium" required placeholder="VD: 0901234567..." />
                </div>
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Địa chỉ Email</label>
                  <input type="email" v-model="createForm.email" class="form-control input-premium" placeholder="email@gmail.com..." />
                </div>
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Địa chỉ nhà</label>
                  <input type="text" v-model="createForm.address" class="form-control input-premium" placeholder="Số nhà, đường, quận..." />
                </div>
              </div>

              <!-- Initial pet details -->
              <div class="col-md-6 ps-md-4">
                <h6 class="text-warning fw-bold mb-3 border-bottom pb-2">2. Đăng ký bé Thú cưng đi kèm</h6>
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Tên thú cưng *</label>
                  <input type="text" v-model="createForm.petName" class="form-control input-premium" required placeholder="Tên bé..." />
                </div>
                <div class="row g-2 mb-3">
                  <div class="col-6">
                    <label class="form-label text-muted small fw-bold">Loài *</label>
                    <select v-model="createForm.species" class="form-select border-warning" required>
                      <option value="Chó">Chó</option>
                      <option value="Mèo">Mèo</option>
                      <option value="Khác">Khác</option>
                    </select>
                  </div>
                  <div class="col-6">
                    <label class="form-label text-muted small fw-bold">Giới tính</label>
                    <select v-model="createForm.gender" class="form-select">
                      <option :value="1">Đực</option>
                      <option :value="2">Cái</option>
                    </select>
                  </div>
                </div>
                <div class="row g-2 mb-3">
                  <div class="col-6">
                    <label class="form-label text-muted small fw-bold">Cân nặng (kg)</label>
                    <input type="number" step="0.1" v-model="createForm.weight" class="form-control" placeholder="Cân nặng..." />
                  </div>
                  <div class="col-6">
                    <label class="form-label text-muted small fw-bold">Giống (Breed)</label>
                    <input type="text" v-model="createForm.breed" class="form-control" placeholder="Poodle, Corgi..." />
                  </div>
                </div>
                <div class="form-check form-switch mt-3">
                  <input class="form-check-input" type="checkbox" role="switch" id="sterilizedCheck" v-model="createForm.sterilized">
                  <label class="form-check-label text-muted small" for="sterilizedCheck">Bé này đã triệt sản</label>
                </div>
              </div>
            </div>

            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showCreateCustomerModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-5">Tạo tài khoản</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal 2: Add Pet to existing customer -->
    <div v-if="showAddPetModal" class="zalo-modal-overlay" @click.self="showAddPetModal = false">
      <div class="zalo-modal-card max-w-450">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold"><i class="bi bi-plus-circle-fill me-2"></i> Thêm Thú Cưng Mới</h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showAddPetModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="submitAddPet">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Tên thú cưng *</label>
              <input type="text" v-model="petForm.name" class="form-control input-premium" required placeholder="Tên bé..." />
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Loài *</label>
                <select v-model="petForm.species" class="form-select" required>
                  <option value="Chó">Chó</option>
                  <option value="Mèo">Mèo</option>
                  <option value="Khác">Khác</option>
                </select>
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giới tính</label>
                <select v-model="petForm.gender" class="form-select">
                  <option :value="1">Đực</option>
                  <option :value="2">Cái</option>
                </select>
              </div>
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Cân nặng (kg)</label>
                <input type="number" step="0.1" v-model="petForm.weight" class="form-control" />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giống (Breed)</label>
                <input type="text" v-model="petForm.breed" class="form-control" />
              </div>
            </div>
            <div class="form-check form-switch mb-3">
              <input class="form-check-input" type="checkbox" id="addPetSterilized" v-model="petForm.sterilized">
              <label class="form-check-label text-muted small" for="addPetSterilized">Bé đã triệt sản</label>
            </div>

            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showAddPetModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-4">Lưu lại</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal 3: Edit Pet -->
    <div v-if="showEditPetModal" class="zalo-modal-overlay" @click.self="showEditPetModal = false">
      <div class="zalo-modal-card max-w-450">
        <div class="zalo-modal-header bg-primary text-white">
          <h5 class="modal-title fw-bold"><i class="bi bi-pencil-fill me-2"></i> Chỉnh Sửa Thông Tin Thú Cưng</h5>
          <button class="modal-close text-white border-0 bg-transparent" @click="showEditPetModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="submitEditPet">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Tên thú cưng *</label>
              <input type="text" v-model="editPetForm.name" class="form-control input-premium" required />
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Loài *</label>
                <select v-model="editPetForm.species" class="form-select" required>
                  <option value="Chó">Chó</option>
                  <option value="Mèo">Mèo</option>
                  <option value="Khác">Khác</option>
                </select>
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giới tính</label>
                <select v-model="editPetForm.gender" class="form-select">
                  <option :value="1">Đực</option>
                  <option :value="2">Cái</option>
                </select>
              </div>
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Cân nặng (kg)</label>
                <input type="number" step="0.1" v-model="editPetForm.weight" class="form-control" />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giống (Breed)</label>
                <input type="text" v-model="editPetForm.breed" class="form-control" />
              </div>
            </div>
            <div class="form-check form-switch mb-3">
              <input class="form-check-input" type="checkbox" id="editPetSterilized" v-model="editPetForm.sterilized">
              <label class="form-check-label text-muted small" for="editPetSterilized">Bé đã triệt sản</label>
            </div>

            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showEditPetModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-4">Cập nhật</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import api from '../../services/api';
import CustomerList from './customer/CustomerList.vue';
import CustomerDetail from './customer/CustomerDetail.vue';
import PetDetail from './customer/PetDetail.vue';

// State
const selectedCustomerDetail = ref<string | null>(null);
const customerDetailRef = ref<any>(null);

// Modals
const showCreateCustomerModal = ref(false);
const showAddPetModal = ref(false);
const showEditPetModal = ref(false);

// Comprehensive Medical Profile Modal
const showPetHistoryModal = ref(false);
const selectedPetHistory = ref<any>(null);

// Forms
const createForm = ref({
  fullName: '',
  phone: '',
  email: '',
  address: '',
  petName: '',
  species: 'Chó',
  gender: 1,
  weight: null as number | null,
  breed: '',
  sterilized: false
});

const petForm = ref({
  customerId: '',
  name: '',
  species: 'Chó',
  gender: 1,
  weight: null as number | null,
  breed: '',
  microchipCode: '',
  sterilized: false
});

const editPetForm = ref({
  id: 0,
  name: '',
  species: 'Chó',
  gender: 1,
  weight: null as number | null,
  breed: '',
  microchipCode: '',
  sterilized: false
});

// View Customer Details
const viewCustomerDetail = (customerId: string) => {
  selectedCustomerDetail.value = customerId;
};

// Comprehensive Medical Profile Actions
const openPetHistoryModal = (pet: any) => {
  selectedPetHistory.value = pet;
  showPetHistoryModal.value = true;
};

// Form Actions
const openCreateCustomerModal = () => {
  createForm.value = {
    fullName: '',
    phone: '',
    email: '',
    address: '',
    petName: '',
    species: 'Chó',
    gender: 1,
    weight: null,
    breed: '',
    sterilized: false
  };
  showCreateCustomerModal.value = true;
};

const openCreateCustomerModalWithPhone = (phone: string) => {
  openCreateCustomerModal();
  createForm.value.phone = phone;
};

const submitCreateCustomer = async () => {
  try {
    const payload = {
      fullName: createForm.value.fullName,
      phone: createForm.value.phone,
      email: createForm.value.email || null,
      address: createForm.value.address || null,
      pets: createForm.value.petName ? [
        {
          name: createForm.value.petName,
          species: createForm.value.species,
          gender: createForm.value.gender,
          weight: createForm.value.weight,
          breed: createForm.value.breed || null,
          sterilized: createForm.value.sterilized
        }
      ] : []
    };

    const res = await api.post('/receptionist/customers', payload);
    if (res.data.success) {
      alert(res.data.message || 'Tạo hồ sơ khách hàng thành công!');
      showCreateCustomerModal.value = false;
      // If we are showing list, we might want to refresh it. In this refactor, 
      // we can just force view details.
      if (res.data.customerId) {
        viewCustomerDetail(res.data.customerId);
      } else {
        // Simple reload trick
        selectedCustomerDetail.value = null;
      }
    }
  } catch (err: any) {
    const msg = err.response?.data?.message || err.response?.data || 'Đã xảy ra lỗi khi tạo hồ sơ. Vui lòng kiểm tra lại thông tin.';
    alert(typeof msg === 'string' ? msg : JSON.stringify(msg));
  }
};

// Add Pet actions
const openAddPetModal = () => {
  petForm.value = {
    customerId: selectedCustomerDetail.value || '',
    name: '',
    species: 'Chó',
    gender: 1,
    weight: null,
    breed: '',
    microchipCode: '',
    sterilized: false
  };
  showAddPetModal.value = true;
};

const openAddPetModalDirect = (customerId: string) => {
  selectedCustomerDetail.value = customerId;
  openAddPetModal();
};

const submitAddPet = async () => {
  try {
    const cid = petForm.value.customerId;
    const res = await api.post(`/receptionist/customers/${cid}/pets`, petForm.value);
    if (res.data.success) {
      alert(res.data.message || 'Thêm thú cưng thành công!');
      showAddPetModal.value = false;
      if (customerDetailRef.value) {
        customerDetailRef.value.refresh();
      }
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi thêm thú cưng.');
  }
};

// Edit Pet Actions
const openEditPetModal = (pet: any) => {
  editPetForm.value = {
    id: pet.id,
    name: pet.name,
    species: pet.species || 'Chó',
    gender: pet.gender || 1,
    weight: pet.weight || null,
    breed: pet.breed || '',
    microchipCode: pet.microchipCode || '',
    sterilized: pet.sterilized || false
  };
  showEditPetModal.value = true;
};

const submitEditPet = async () => {
  try {
    const pid = editPetForm.value.id;
    const res = await api.put(`/receptionist/pets/${pid}`, editPetForm.value);
    if (res.data.success) {
      alert(res.data.message || 'Cập nhật thành công!');
      showEditPetModal.value = false;
      if (customerDetailRef.value) {
        customerDetailRef.value.refresh();
      }
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra.');
  }
};
</script>

<script lang="ts">
export default {
  name: 'CustomersTab'
}
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
  background: linear-gradient(135deg, var(--primary-gold), #d97706);
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

/* Timeline */
.timeline-item {
  position: relative;
}
.timeline-line {
  position: absolute;
  top: 15px;
  left: 9px;
  bottom: 0;
  width: 2px;
  background-color: #e2e8f0;
}
.timeline-item:last-child .timeline-line {
  display: none;
}
.timeline-circle {
  position: absolute;
  top: 12px;
  left: 4px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid white;
  z-index: 1;
}

/* Modals overlays */
.zalo-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(5px);
  z-index: 1200;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}

.zalo-modal-card {
  background: white;
  width: 100%;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.max-w-450 { max-width: 450px; }
.max-w-700 { max-width: 700px; }

.zalo-modal-header {
  padding: 1.2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.zalo-modal-body {
  padding: 1.5rem;
  max-height: 80vh;
  overflow-y: auto;
}
</style>
