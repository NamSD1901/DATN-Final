<template>
  <div class="mypets-tab">

    <!-- Header Hero Section -->
    <div class="pets-hero mb-4">
      <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h3 class="fw-bold text-dark mb-1">
            <i class="bi bi-heptagon-fill me-2" style="color: var(--primary-gold);"></i>
            Thú cưng của tôi
          </h3>
          <p class="text-muted mb-0 small">Quản lý hồ sơ sức khoẻ cho các thành viên nhỏ của gia đình bạn.</p>
        </div>
        <button class="btn btn-premium-add" @click="openAddModal">
          <i class="bi bi-plus-circle-fill me-2"></i> Thêm thú cưng mới
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">Đang tải hồ sơ thú cưng...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="errorMsg" class="alert alert-danger rounded-4 shadow-sm border-0 py-3 px-4">
      <i class="bi bi-exclamation-triangle-fill me-2"></i> {{ errorMsg }}
    </div>

    <!-- Empty State -->
    <div v-else-if="pets.length === 0" class="empty-state-card">
      <div class="empty-state-icon">🐾</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">Chưa có thú cưng nào</h5>
      <p class="text-muted small mb-4">Hãy thêm thú cưng đầu tiên của bạn để bắt đầu theo dõi sức khoẻ của bé nhé!</p>
      <button class="btn btn-premium-add" @click="openAddModal">
        <i class="bi bi-plus-circle-fill me-2"></i> Thêm ngay
      </button>
    </div>

    <!-- Pet Cards Grid -->
    <div v-else class="pets-grid">
      <TransitionGroup name="pet-card" tag="div" class="row g-4">
        <div v-for="pet in pets" :key="pet.id" class="col-lg-4 col-md-6">
          <div class="pet-card" :class="getSpeciesClass(pet.species)">
            <!-- Species Banner -->
            <div class="pet-card-banner">
              <span class="pet-species-badge">{{ getSpeciesEmoji(pet.species) }} {{ pet.species }}</span>
              <div class="pet-card-actions">
                <button class="action-btn edit" @click="openEditModal(pet)" title="Chỉnh sửa">
                  <i class="bi bi-pencil-fill"></i>
                </button>
                <button class="action-btn delete" @click="confirmDelete(pet)" title="Xoá">
                  <i class="bi bi-trash3-fill"></i>
                </button>
              </div>
            </div>

            <!-- Pet Avatar -->
            <div class="pet-avatar-wrapper">
              <div class="pet-avatar" :style="{ background: getPetAvatarColor(pet.species) }">
                <span>{{ getSpeciesEmoji(pet.species) }}</span>
              </div>
              <div v-if="pet.sterilized" class="sterilized-badge" title="Đã triệt sản">
                <i class="bi bi-shield-check-fill"></i>
              </div>
            </div>

            <!-- Pet Info -->
            <div class="pet-card-body">
              <h5 class="pet-name">{{ pet.name }}</h5>
              <p class="pet-breed text-muted">{{ pet.breed || 'Chưa xác định giống' }}</p>

              <div class="pet-stats">
                <div class="pet-stat">
                  <span class="stat-label">Giới tính</span>
                  <span class="stat-value">{{ pet.gender === 1 ? '♂ Đực' : pet.gender === 2 ? '♀ Cái' : 'Không rõ' }}</span>
                </div>
                <div class="pet-stat">
                  <span class="stat-label">Tuổi</span>
                  <span class="stat-value">{{ calculateAge(pet.birthDate) }}</span>
                </div>
                <div class="pet-stat">
                  <span class="stat-label">Cân nặng</span>
                  <span class="stat-value">{{ pet.weight ? `${pet.weight} kg` : 'Chưa cập nhật' }}</span>
                </div>
                <div class="pet-stat">
                  <span class="stat-label">Màu lông</span>
                  <span class="stat-value">{{ pet.color || '—' }}</span>
                </div>
              </div>

              <!-- Allergy Alert -->
              <div v-if="pet.allergyNote" class="allergy-note">
                <i class="bi bi-exclamation-triangle-fill me-1 text-warning"></i>
                <span>{{ pet.allergyNote }}</span>
              </div>

              <!-- Microchip -->
              <div v-if="pet.microchipCode" class="microchip-badge">
                <i class="bi bi-cpu me-1"></i> {{ pet.microchipCode }}
              </div>

              <!-- View Details Button -->
              <button class="btn-view-detail w-100 mt-3" @click="openDetailModal(pet)">
                Xem hồ sơ đầy đủ <i class="bi bi-arrow-right ms-1"></i>
              </button>
            </div>
          </div>
        </div>
      </TransitionGroup>
    </div>

    <!-- ===== ADD / EDIT PET MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showFormModal" class="pet-modal-overlay" @click.self="closeModal">
          <div class="pet-modal-card">
            <div class="pet-modal-header">
              <h5 class="fw-bold mb-0">
                <i class="bi bi-heptagon-fill me-2 text-warning"></i>
                {{ isEditing ? 'Chỉnh sửa hồ sơ thú cưng' : 'Thêm thú cưng mới' }}
              </h5>
              <button class="modal-close-btn" @click="closeModal">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="pet-modal-body">
              <!-- Alert messages -->
              <div v-if="formSuccess" class="alert alert-success border-0 rounded-3 py-2 px-3 mb-3">
                <i class="bi bi-check-circle-fill me-2"></i>{{ formSuccess }}
              </div>
              <div v-if="formError" class="alert alert-danger border-0 rounded-3 py-2 px-3 mb-3">
                <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ formError }}
              </div>

              <form @submit.prevent="submitForm" class="pet-form">
                <div class="row g-3">
                  <!-- Name -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Tên thú cưng <span class="text-danger">*</span></label>
                    <input v-model="form.name" type="text" class="form-control-custom" placeholder="VD: Mochi, Buddy..." required />
                  </div>

                  <!-- Species -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Loài <span class="text-danger">*</span></label>
                    <select v-model="form.species" class="form-control-custom" required>
                      <option value="">-- Chọn loài --</option>
                      <option value="Chó">🐕 Chó</option>
                      <option value="Mèo">🐈 Mèo</option>
                      <option value="Thỏ">🐇 Thỏ</option>
                      <option value="Chim">🦜 Chim</option>
                      <option value="Cá">🐟 Cá</option>
                      <option value="Bò sát">🦎 Bò sát</option>
                      <option value="Khác">🐾 Khác</option>
                    </select>
                  </div>

                  <!-- Breed -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Giống</label>
                    <input v-model="form.breed" type="text" class="form-control-custom" placeholder="VD: Golden Retriever..." />
                  </div>

                  <!-- Gender -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Giới tính</label>
                    <select v-model="form.gender" class="form-control-custom">
                      <option :value="null">-- Chưa xác định --</option>
                      <option :value="1">♂ Đực</option>
                      <option :value="2">♀ Cái</option>
                    </select>
                  </div>

                  <!-- Birth Date -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Ngày sinh</label>
                    <input v-model="form.birthDate" type="date" class="form-control-custom" :max="todayStr" />
                  </div>

                  <!-- Weight -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Cân nặng (kg)</label>
                    <input v-model="form.weight" type="number" step="0.1" min="0" max="999" class="form-control-custom" placeholder="VD: 5.5" />
                  </div>

                  <!-- Color -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Màu lông</label>
                    <input v-model="form.color" type="text" class="form-control-custom" placeholder="VD: Vàng kem, Đen trắng..." />
                  </div>

                  <!-- Blood Type -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Nhóm máu</label>
                    <input v-model="form.bloodType" type="text" class="form-control-custom" placeholder="VD: DEA 1.1+..." />
                  </div>

                  <!-- Microchip -->
                  <div class="col-sm-6">
                    <label class="form-label-custom">Mã microchip</label>
                    <input v-model="form.microchipCode" type="text" class="form-control-custom" placeholder="VD: 900006000000000..." />
                  </div>

                  <!-- Sterilized -->
                  <div class="col-sm-6 d-flex align-items-center gap-3 pt-3">
                    <div class="form-check form-switch">
                      <input v-model="form.sterilized" class="form-check-input" type="checkbox" id="sterilizedCheck" style="width: 2.5em; height: 1.3em; cursor: pointer;" />
                      <label class="form-check-label fw-semibold ms-2" for="sterilizedCheck">Đã triệt sản</label>
                    </div>
                  </div>

                  <!-- Allergy Note -->
                  <div class="col-12">
                    <label class="form-label-custom">Ghi chú dị ứng / đặc biệt</label>
                    <textarea v-model="form.allergyNote" class="form-control-custom" rows="3" placeholder="VD: Dị ứng với penicillin, không ăn được gà..."></textarea>
                  </div>
                </div>

                <div class="pet-modal-footer mt-4">
                  <button type="button" class="btn btn-outline-secondary rounded-pill px-4" @click="closeModal" :disabled="formLoading">Huỷ</button>
                  <button type="submit" class="btn btn-premium-add px-4" :disabled="formLoading">
                    <span v-if="formLoading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                    <i v-else class="bi bi-check2-circle me-2"></i>
                    {{ isEditing ? 'Lưu thay đổi' : 'Thêm thú cưng' }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ===== PET DETAIL MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDetailModal && selectedPet" class="pet-modal-overlay" @click.self="showDetailModal = false">
          <div class="pet-modal-card detail-modal">
            <div class="pet-modal-header detail-header" :style="{ background: getPetAvatarColor(selectedPet.species) }">
              <div class="text-center w-100">
                <div class="detail-avatar">{{ getSpeciesEmoji(selectedPet.species) }}</div>
                <h4 class="fw-bold text-white mt-2 mb-0">{{ selectedPet.name }}</h4>
                <p class="text-white opacity-75 small mb-0">{{ selectedPet.species }} · {{ selectedPet.breed || 'Chưa xác định' }}</p>
              </div>
              <button class="modal-close-btn text-white" @click="showDetailModal = false">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="pet-modal-body">
              <div class="row g-3">
                <div class="col-6">
                  <div class="detail-info-item">
                    <span class="detail-label">Giới tính</span>
                    <span class="detail-value">{{ selectedPet.gender === 1 ? '♂ Đực' : selectedPet.gender === 2 ? '♀ Cái' : 'Không rõ' }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item">
                    <span class="detail-label">Tuổi</span>
                    <span class="detail-value">{{ calculateAge(selectedPet.birthDate) }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item">
                    <span class="detail-label">Ngày sinh</span>
                    <span class="detail-value">{{ formatDate(selectedPet.birthDate) }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item">
                    <span class="detail-label">Cân nặng</span>
                    <span class="detail-value">{{ selectedPet.weight ? `${selectedPet.weight} kg` : 'Chưa cập nhật' }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item">
                    <span class="detail-label">Màu lông</span>
                    <span class="detail-value">{{ selectedPet.color || '—' }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item">
                    <span class="detail-label">Nhóm máu</span>
                    <span class="detail-value">{{ selectedPet.bloodType || '—' }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item">
                    <span class="detail-label">Triệt sản</span>
                    <span class="detail-value">
                      <span v-if="selectedPet.sterilized" class="badge bg-success rounded-pill">✓ Đã triệt sản</span>
                      <span v-else class="badge bg-secondary rounded-pill">Chưa triệt sản</span>
                    </span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item">
                    <span class="detail-label">Microchip</span>
                    <span class="detail-value detail-chip">{{ selectedPet.microchipCode || 'Chưa gắn chip' }}</span>
                  </div>
                </div>
                <div v-if="selectedPet.allergyNote" class="col-12">
                  <div class="detail-info-item allergy-detail">
                    <span class="detail-label"><i class="bi bi-exclamation-triangle-fill text-warning me-1"></i>Dị ứng / Ghi chú đặc biệt</span>
                    <span class="detail-value">{{ selectedPet.allergyNote }}</span>
                  </div>
                </div>
                <div class="col-12">
                  <div class="detail-info-item">
                    <span class="detail-label">Ngày đăng ký</span>
                    <span class="detail-value">{{ formatDate(selectedPet.createdAt) }}</span>
                  </div>
                </div>
              </div>

              <div class="d-flex gap-2 mt-4">
                <button class="btn btn-outline-warning fw-semibold rounded-pill flex-fill" @click="openEditFromDetail(selectedPet)">
                  <i class="bi bi-pencil-fill me-2"></i> Chỉnh sửa
                </button>
                <button class="btn btn-outline-secondary rounded-pill px-4" @click="showDetailModal = false">Đóng</button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ===== DELETE CONFIRM MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDeleteModal && petToDelete" class="pet-modal-overlay" @click.self="showDeleteModal = false">
          <div class="pet-modal-card" style="max-width: 420px;">
            <div class="pet-modal-header bg-danger text-white">
              <h5 class="fw-bold mb-0"><i class="bi bi-exclamation-triangle-fill me-2"></i>Xác nhận xoá</h5>
              <button class="modal-close-btn text-white" @click="showDeleteModal = false"><i class="bi bi-x-lg"></i></button>
            </div>
            <div class="pet-modal-body text-center">
              <div style="font-size: 4rem; margin-bottom: 0.5rem;">{{ getSpeciesEmoji(petToDelete.species) }}</div>
              <p class="text-dark fw-bold mb-1 fs-5">{{ petToDelete.name }}</p>
              <p class="text-muted small mb-4">Bạn có chắc muốn xoá hồ sơ của <strong>{{ petToDelete.name }}</strong>? Hành động này không thể hoàn tác.</p>
              <div class="d-flex gap-2 justify-content-center">
                <button class="btn btn-outline-secondary rounded-pill px-4" @click="showDeleteModal = false" :disabled="deleteLoading">Huỷ bỏ</button>
                <button class="btn btn-danger rounded-pill px-4 fw-bold" @click="deletePet" :disabled="deleteLoading">
                  <span v-if="deleteLoading" class="spinner-border spinner-border-sm me-2"></span>
                  <i v-else class="bi bi-trash3-fill me-2"></i>Xoá vĩnh viễn
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import api from '../../services/api';

// ===== Types =====
interface Pet {
  id: number;
  ownerId: string;
  name: string;
  species: string;
  breed: string | null;
  gender: number | null;
  birthDate: string | null;
  weight: number | null;
  color: string | null;
  bloodType: string | null;
  sterilized: boolean;
  microchipCode: string | null;
  allergyNote: string | null;
  createdAt: string | null;
}

interface PetForm {
  id?: number;
  name: string;
  species: string;
  breed: string;
  gender: number | null;
  birthDate: string;
  weight: number | null;
  color: string;
  bloodType: string;
  sterilized: boolean;
  microchipCode: string;
  allergyNote: string;
}

// ===== State =====
const pets = ref<Pet[]>([]);
const loading = ref(false);
const errorMsg = ref('');

// Modal state
const showFormModal = ref(false);
const showDetailModal = ref(false);
const showDeleteModal = ref(false);
const isEditing = ref(false);

const selectedPet = ref<Pet | null>(null);
const petToDelete = ref<Pet | null>(null);

const formLoading = ref(false);
const deleteLoading = ref(false);
const formSuccess = ref('');
const formError = ref('');

const defaultForm = (): PetForm => ({
  name: '',
  species: '',
  breed: '',
  gender: null,
  birthDate: '',
  weight: null,
  color: '',
  bloodType: '',
  sterilized: false,
  microchipCode: '',
  allergyNote: '',
});

const form = ref<PetForm>(defaultForm());

const todayStr = computed(() => {
  return new Date().toISOString().split('T')[0];
});

// ===== API calls =====
const fetchPets = async () => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get('/mypets');
    pets.value = res.data;
  } catch (err: any) {
    errorMsg.value = 'Không thể tải dữ liệu thú cưng. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
};

const submitForm = async () => {
  formLoading.value = true;
  formError.value = '';
  formSuccess.value = '';
  try {
    if (isEditing.value && form.value.id) {
      await api.put(`/mypets/${form.value.id}`, form.value);
      formSuccess.value = `Đã cập nhật hồ sơ ${form.value.name} thành công!`;
    } else {
      await api.post('/mypets', form.value);
      formSuccess.value = `Đã thêm ${form.value.name} vào danh sách thành công!`;
    }
    await fetchPets();
    setTimeout(() => closeModal(), 1500);
  } catch (err: any) {
    formError.value = err?.response?.data?.message || 'Có lỗi xảy ra, vui lòng thử lại.';
  } finally {
    formLoading.value = false;
  }
};

const deletePet = async () => {
  if (!petToDelete.value) return;
  deleteLoading.value = true;
  try {
    await api.delete(`/mypets/${petToDelete.value.id}`);
    await fetchPets();
    showDeleteModal.value = false;
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Xoá thất bại. Vui lòng thử lại.');
  } finally {
    deleteLoading.value = false;
  }
};

// ===== Modal controls =====
const openAddModal = () => {
  isEditing.value = false;
  form.value = defaultForm();
  formSuccess.value = '';
  formError.value = '';
  showFormModal.value = true;
};

const openEditModal = (pet: Pet) => {
  isEditing.value = true;
  form.value = {
    id: pet.id,
    name: pet.name ?? '',
    species: pet.species ?? '',
    breed: pet.breed ?? '',
    gender: pet.gender ?? null,
    birthDate: pet.birthDate ? pet.birthDate.split('T')[0] : '',
    weight: pet.weight ?? null,
    color: pet.color ?? '',
    bloodType: pet.bloodType ?? '',
    sterilized: pet.sterilized ?? false,
    microchipCode: pet.microchipCode ?? '',
    allergyNote: pet.allergyNote ?? '',
  };
  formSuccess.value = '';
  formError.value = '';
  showFormModal.value = true;
};

const openDetailModal = (pet: Pet) => {
  selectedPet.value = pet;
  showDetailModal.value = true;
};

const openEditFromDetail = (pet: Pet) => {
  showDetailModal.value = false;
  openEditModal(pet);
};

const confirmDelete = (pet: Pet) => {
  petToDelete.value = pet;
  showDeleteModal.value = true;
};

const closeModal = () => {
  showFormModal.value = false;
  form.value = defaultForm();
};

// ===== Helpers =====
const calculateAge = (birthDate: string | null): string => {
  if (!birthDate) return 'Chưa rõ';
  const birth = new Date(birthDate);
  const now = new Date();
  const diffMs = now.getTime() - birth.getTime();
  const totalMonths = Math.floor(diffMs / (1000 * 60 * 60 * 24 * 30.44));
  const years = Math.floor(totalMonths / 12);
  const months = totalMonths % 12;
  if (years === 0) return months === 0 ? 'Sơ sinh' : `${months} tháng`;
  if (months === 0) return `${years} tuổi`;
  return `${years} tuổi ${months} tháng`;
};

const formatDate = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const getSpeciesEmoji = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': '🐕', 'Mèo': '🐈', 'Thỏ': '🐇', 'Chim': '🦜', 'Cá': '🐟', 'Bò sát': '🦎',
  };
  return map[species ?? ''] || '🐾';
};

const getSpeciesClass = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': 'species-dog', 'Mèo': 'species-cat', 'Thỏ': 'species-rabbit', 'Chim': 'species-bird',
    'Cá': 'species-fish', 'Bò sát': 'species-reptile',
  };
  return map[species ?? ''] || 'species-other';
};

const getPetAvatarColor = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': 'linear-gradient(135deg, #f59e0b, #d97706)',
    'Mèo': 'linear-gradient(135deg, #8b5cf6, #6d28d9)',
    'Thỏ': 'linear-gradient(135deg, #ec4899, #be185d)',
    'Chim': 'linear-gradient(135deg, #06b6d4, #0891b2)',
    'Cá': 'linear-gradient(135deg, #3b82f6, #1d4ed8)',
    'Bò sát': 'linear-gradient(135deg, #10b981, #059669)',
  };
  return map[species ?? ''] || 'linear-gradient(135deg, #6b7280, #4b5563)';
};

// ===== Lifecycle =====
onMounted(fetchPets);
</script>

<style scoped>
/* ===== Layout ===== */
.mypets-tab {
  padding: 0;
}

/* ===== Hero ===== */
.pets-hero {
  background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);
  border-radius: 16px;
  padding: 1.5rem 2rem;
  border: 1px solid #fde68a;
}

/* ===== Premium Add Button ===== */
.btn-premium-add {
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: white;
  border: none;
  padding: 0.6rem 1.4rem;
  border-radius: 50px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(245, 158, 11, 0.4);
  display: inline-flex;
  align-items: center;
}

.btn-premium-add:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.5);
  filter: brightness(1.05);
}

/* ===== Empty State ===== */
.empty-state-card {
  background: white;
  border-radius: 20px;
  padding: 4rem 2rem;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0,0,0,0.06);
  border: 2px dashed #fde68a;
}

.empty-state-icon {
  font-size: 5rem;
  filter: drop-shadow(0 4px 8px rgba(0,0,0,0.1));
}

/* ===== Pet Card ===== */
.pet-card {
  background: white;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(0,0,0,0.07);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid #f0f0f0;
}

.pet-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 12px 35px rgba(0,0,0,0.13);
}

/* Species colored top banner */
.pet-card-banner {
  height: 80px;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 0.75rem 1rem;
}

.species-dog .pet-card-banner { background: linear-gradient(135deg, #fef3c7, #fde68a); }
.species-cat .pet-card-banner { background: linear-gradient(135deg, #ede9fe, #ddd6fe); }
.species-rabbit .pet-card-banner { background: linear-gradient(135deg, #fce7f3, #fbcfe8); }
.species-bird .pet-card-banner { background: linear-gradient(135deg, #cffafe, #a5f3fc); }
.species-fish .pet-card-banner { background: linear-gradient(135deg, #dbeafe, #bfdbfe); }
.species-reptile .pet-card-banner { background: linear-gradient(135deg, #d1fae5, #a7f3d0); }
.species-other .pet-card-banner { background: linear-gradient(135deg, #f3f4f6, #e5e7eb); }

.pet-species-badge {
  background: rgba(255,255,255,0.8);
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.78rem;
  font-weight: 600;
  color: #374151;
  backdrop-filter: blur(4px);
}

.pet-card-actions {
  display: flex;
  gap: 6px;
  opacity: 0;
  transition: opacity 0.2s;
}

.pet-card:hover .pet-card-actions {
  opacity: 1;
}

.action-btn {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  transition: all 0.2s;
}

.action-btn.edit {
  background: white;
  color: #f59e0b;
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
}

.action-btn.edit:hover {
  background: #f59e0b;
  color: white;
}

.action-btn.delete {
  background: white;
  color: #ef4444;
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
}

.action-btn.delete:hover {
  background: #ef4444;
  color: white;
}

/* Avatar */
.pet-avatar-wrapper {
  display: flex;
  justify-content: center;
  margin-top: -30px;
  position: relative;
  margin-bottom: 0.5rem;
}

.pet-avatar {
  width: 65px;
  height: 65px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  border: 4px solid white;
  box-shadow: 0 4px 15px rgba(0,0,0,0.15);
}

.sterilized-badge {
  position: absolute;
  bottom: 0;
  right: calc(50% - 45px);
  background: #10b981;
  color: white;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.65rem;
  border: 2px solid white;
}

/* Card body */
.pet-card-body {
  padding: 0 1.25rem 1.25rem;
  text-align: center;
}

.pet-name {
  font-size: 1.15rem;
  font-weight: 700;
  color: #1a1a2e;
  margin-bottom: 2px;
}

.pet-breed {
  font-size: 0.82rem;
  margin-bottom: 0.75rem;
}

/* Stats Grid */
.pet-stats {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px;
  text-align: left;
  margin-bottom: 0.75rem;
  background: #f9fafb;
  border-radius: 12px;
  padding: 10px;
}

.pet-stat {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 0.7rem;
  color: #9ca3af;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.stat-value {
  font-size: 0.85rem;
  font-weight: 600;
  color: #374151;
}

/* Allergy Note */
.allergy-note {
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 8px;
  padding: 6px 10px;
  font-size: 0.78rem;
  color: #92400e;
  text-align: left;
  margin-bottom: 6px;
}

/* Microchip */
.microchip-badge {
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  border-radius: 20px;
  padding: 3px 10px;
  font-size: 0.72rem;
  color: #166534;
  display: inline-flex;
  align-items: center;
  margin-bottom: 4px;
}

/* View Detail Button */
.btn-view-detail {
  background: transparent;
  border: 1.5px solid #e5e7eb;
  border-radius: 12px;
  padding: 8px 16px;
  font-size: 0.85rem;
  font-weight: 600;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-view-detail:hover {
  border-color: #f59e0b;
  color: #d97706;
  background: #fffbeb;
}

/* ===== Cards Grid Animation ===== */
.pet-card-enter-active, .pet-card-leave-active {
  transition: all 0.4s ease;
}
.pet-card-enter-from {
  opacity: 0;
  transform: scale(0.8);
}
.pet-card-leave-to {
  opacity: 0;
  transform: scale(0.8);
}

/* ===== Modal Styles ===== */
.pet-modal-overlay {
  position: fixed;
  top: 0; left: 0;
  width: 100vw; height: 100vh;
  background: rgba(0,0,0,0.45);
  backdrop-filter: blur(6px);
  z-index: 2000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.pet-modal-card {
  background: white;
  border-radius: 20px;
  width: 100%;
  max-width: 640px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 25px 60px rgba(0,0,0,0.2);
}

.detail-modal {
  max-width: 520px;
}

.pet-modal-header {
  padding: 1.25rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #f0f0f0;
  position: sticky;
  top: 0;
  background: white;
  z-index: 1;
  border-radius: 20px 20px 0 0;
}

.detail-header {
  border: none;
  padding: 2rem 1.5rem;
  border-radius: 20px 20px 0 0;
  position: relative;
}

.detail-avatar {
  font-size: 4rem;
  filter: drop-shadow(0 4px 8px rgba(0,0,0,0.2));
}

.modal-close-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 1.1rem;
  color: #6b7280;
  padding: 0.25rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.modal-close-btn:hover {
  background: rgba(0,0,0,0.08);
  color: #111;
}

.pet-modal-body {
  padding: 1.5rem;
}

.pet-modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding-top: 1rem;
  border-top: 1px solid #f0f0f0;
}

/* Form controls */
.form-label-custom {
  font-size: 0.82rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 5px;
  display: block;
}

.form-control-custom {
  width: 100%;
  padding: 0.55rem 0.9rem;
  border: 1.5px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.9rem;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
  background: #fafafa;
}

.form-control-custom:focus {
  border-color: #f59e0b;
  box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.15);
  background: white;
}

textarea.form-control-custom {
  resize: vertical;
  min-height: 80px;
}

/* Detail info items */
.detail-info-item {
  background: #f9fafb;
  border-radius: 10px;
  padding: 10px 14px;
}

.detail-label {
  display: block;
  font-size: 0.72rem;
  font-weight: 600;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  margin-bottom: 3px;
}

.detail-value {
  font-size: 0.9rem;
  font-weight: 600;
  color: #1f2937;
}

.detail-chip {
  font-family: monospace;
  font-size: 0.8rem;
  word-break: break-all;
}

.allergy-detail {
  background: #fffbeb;
  border: 1px solid #fde68a;
}

/* Modal transition */
.modal-fade-enter-active, .modal-fade-leave-active {
  transition: all 0.3s ease;
}
.modal-fade-enter-from, .modal-fade-leave-to {
  opacity: 0;
  transform: scale(0.95);
}
</style>
