<template>
  <div class="mypets-tab glass-container">

    <!-- Header Hero Section with Glassmorphic design -->
    <div class="pets-hero glass-panel mb-4 p-4">
      <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h3 class="fw-bold gradient-text-gold mb-1">
            <i class="bi bi-heptagon-fill me-2 pulse-gold-icon"></i>
            {{ $t('pets.title') }}
          </h3>
          <p class="text-secondary-muted mb-0 small">{{ $t('pets.subtitle') }}</p>
        </div>
        <button class="btn btn-premium-neon" @click="openAddModal">
          <i class="bi bi-plus-circle-fill me-2"></i> {{ $t('pets.addNew') }}
        </button>
      </div>
    </div>

    <!-- Loading State with Pulsing Skeleton -->
    <div v-if="loading" class="row g-4">
      <div v-for="n in 3" :key="n" class="col-lg-4 col-md-6">
        <div class="pet-card-skeleton glass-panel">
          <div class="skeleton-banner"></div>
          <div class="skeleton-avatar mx-auto"></div>
          <div class="skeleton-body p-4">
            <div class="skeleton-line title mb-3"></div>
            <div class="skeleton-line subtitle mb-4"></div>
            <div class="row g-2 mb-3">
              <div class="col-6" v-for="x in 4" :key="x"><div class="skeleton-line stat"></div></div>
            </div>
            <div class="skeleton-button"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="errorMsg" class="alert alert-danger-glass glass-panel py-3 px-4 mb-4">
      <i class="bi bi-exclamation-triangle-fill me-2 text-danger"></i> {{ errorMsg }}
    </div>

    <!-- Empty State -->
    <div v-else-if="pets.length === 0" class="empty-state-glass glass-panel p-5 text-center">
      <div class="empty-state-icon">🐶</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">{{ $t('pets.noPetTitle') }}</h5>
      <p class="text-secondary-muted small mb-4">{{ $t('pets.noPetDesc') }}</p>
      <button class="btn btn-premium-neon" @click="openAddModal">
        <i class="bi bi-plus-circle-fill me-2"></i> {{ $t('pets.addNow') }}
      </button>
    </div>

    <!-- Pet Cards Grid -->
    <div v-else class="pets-grid">
      <TransitionGroup name="pet-card" tag="div" class="row g-4">
        <div v-for="pet in pets" :key="pet.id" class="col-lg-4 col-md-6">
          <div class="pet-card-glass glass-panel" :class="getSpeciesClass(pet.species)">
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
              <div v-if="pet.avatar" class="pet-avatar-img-wrapper">
                <img :src="getPetAvatarUrl(pet.avatar)" alt="Pet Avatar" class="pet-avatar-img" />
              </div>
              <div v-else class="pet-avatar-img-wrapper" style="background: transparent; border: 2px solid rgba(245, 158, 11, 0.2);">
                <img :src="getSpeciesImageUrl(pet.species)" alt="Pet Avatar" class="pet-avatar-img" />
              </div>
              <div v-if="pet.sterilized" class="sterilized-badge" title="Đã triệt sản">
                <i class="bi bi-shield-check-fill"></i>
              </div>
            </div>

            <!-- Pet Info -->
            <div class="pet-card-body p-4">
              <h5 class="pet-name text-dark text-center fw-bold mb-1">{{ pet.name }}</h5>
              <p class="pet-breed text-secondary-muted text-center small mb-3">{{ pet.breed || 'Chưa xác định giống' }}</p>

              <div class="pet-stats-grid">
                <div class="pet-stat-item">
                  <span class="stat-label">Giới tính</span>
                  <span class="stat-value text-dark">{{ pet.gender === 1 ? '♂ Đực' : pet.gender === 2 ? '♀ Cái' : 'Chưa rõ' }}</span>
                </div>
                <div class="pet-stat-item">
                  <span class="stat-label">Tuổi</span>
                  <span class="stat-value text-dark">{{ calculateAge(pet.birthDate) }}</span>
                </div>
                <div class="pet-stat-item">
                  <span class="stat-label">Cân nặng</span>
                  <span class="stat-value text-dark">{{ pet.weight ? `${pet.weight} kg` : 'Chưa rõ' }}</span>
                </div>
                <div class="pet-stat-item">
                  <span class="stat-label">Màu lông</span>
                  <span class="stat-value text-dark truncate-text">{{ pet.color || '—' }}</span>
                </div>
              </div>

              <!-- Allergy Alert -->
              <div v-if="pet.allergyNote" class="allergy-note-glass mt-3">
                <i class="bi bi-exclamation-triangle-fill me-2 text-warning pulse-icon"></i>
                <span class="truncate-text">{{ pet.allergyNote }}</span>
              </div>



              <!-- View Details Button -->
              <button class="btn-view-detail-glass w-100 mt-3" @click="$emit('view-pet', pet.id)">
                Xem hồ sơ đầy đủ <i class="bi bi-arrow-right ms-2 transition-arrow"></i>
              </button>
            </div>
          </div>
        </div>
      </TransitionGroup>
    </div>

    <!-- ===== ADD / EDIT PET MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showFormModal" class="pet-modal-overlay-glass" @click.self="closeModal">
          <div class="pet-modal-card-glass glass-panel" :class="{ 'shake-animation': formValidationError }">
            <div class="pet-modal-header-glass border-bottom-glass p-3 d-flex justify-content-between align-items-center">
              <h5 class="fw-bold mb-0 text-dark d-flex align-items-center">
                <i class="bi bi-stars me-2 text-warning fs-4"></i>
                {{ isEditing ? 'Chỉnh sửa hồ sơ thú cưng' : 'Thêm thú cưng mới' }}
              </h5>
              <button class="modal-close-btn-glass" @click="closeModal" title="Đóng">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="pet-modal-body-glass p-4 px-sm-5">
              <!-- Alert messages -->
              <div v-if="formSuccess" class="alert alert-success-glass glass-panel py-2 px-3 mb-4 d-flex align-items-center">
                <i class="bi bi-check-circle-fill me-2 text-emerald fs-5"></i><span class="ms-1">{{ formSuccess }}</span>
              </div>
              <div v-if="formError" class="alert alert-danger-glass glass-panel py-2 px-3 mb-4 d-flex align-items-center">
                <i class="bi bi-exclamation-triangle-fill me-2 text-danger fs-5"></i><span class="ms-1">{{ formError }}</span>
              </div>

              <form @submit.prevent="submitForm" class="pet-form">
                <!-- Pet Avatar Uploader -->
                <div class="pet-upload-section mb-4 text-center p-4 rounded-4" style="background: rgba(245, 158, 11, 0.04); border: 1px dashed rgba(245, 158, 11, 0.3);">
                  <div class="pet-upload-container mx-auto mb-3 shadow-sm" style="width: 110px; height: 110px;">
                    <img v-if="form.avatar" :src="getPetAvatarUrl(form.avatar)" alt="Pet Preview" class="pet-upload-preview" />
                    <div v-else class="pet-upload-placeholder overflow-hidden" style="padding: 0; border-radius: 50%;">
                      <img :src="getSpeciesImageUrl(form.species)" class="w-100 h-100" style="object-fit: cover;" />
                    </div>
                    <label for="pet-avatar-upload" class="pet-upload-label" :class="{ uploading: avatarUploading }">
                      <span v-if="avatarUploading" class="spinner-border spinner-border-sm text-white" role="status"></span>
                      <i v-else class="bi bi-camera-fill"></i>
                      <input type="file" id="pet-avatar-upload" class="d-none" accept="image/*" @change="handlePetAvatarUpload" />
                    </label>
                  </div>
                  <h6 class="fw-semibold text-dark mb-1">Ảnh đại diện thú cưng</h6>
                  <p class="text-secondary-muted small mb-0 opacity-75">Hỗ trợ JPG, PNG. Tối đa 2MB.</p>
                </div>

                <div class="row g-4">
                  <!-- Name -->
                  <div class="col-sm-6">
                    <label class="form-label-glass text-secondary">Tên gọi của bé <span class="text-danger">*</span></label>
                    <div class="input-group-custom">
                      <span class="input-icon"><i class="bi bi-tag"></i></span>
                      <input v-model="form.name" type="text" class="form-control-glass with-icon" placeholder="VD: Mochi, Buddy..." required />
                    </div>
                  </div>

                  <!-- Species -->
                  <div class="col-sm-6">
                    <label class="form-label-glass text-secondary">Giống loài <span class="text-danger">*</span></label>
                    <div class="input-group-custom">
                      <span class="input-icon"><i class="bi bi-emoji-smile"></i></span>
                      <select v-model="form.species" class="form-control-glass with-icon" required>
                        <option value="">-- Chọn loài --</option>
                        <option value="Chó">🐕 Chó</option>
                        <option value="Mèo">🐈 Mèo</option>
                      </select>
                    </div>
                  </div>

                  <!-- Breed -->
                  <div class="col-sm-6">
                    <label class="form-label-glass text-secondary">Giống / Dòng</label>
                    <div class="input-group-custom">
                      <span class="input-icon"><i class="bi bi-info-circle"></i></span>
                      <input v-model="form.breed" type="text" class="form-control-glass with-icon" placeholder="VD: Poodle, British Shorthair..." />
                    </div>
                  </div>

                  <!-- Gender -->
                  <div class="col-sm-6">
                    <label class="form-label-glass text-secondary">Giới tính</label>
                    <div class="input-group-custom">
                      <span class="input-icon"><i class="bi bi-gender-ambiguous"></i></span>
                      <select v-model="form.gender" class="form-control-glass with-icon">
                        <option :value="null">-- Chưa xác định --</option>
                        <option :value="1">♂ Đực (Male)</option>
                        <option :value="2">♀ Cái (Female)</option>
                      </select>
                    </div>
                  </div>

                  <!-- Birth Date -->
                  <div class="col-sm-6">
                    <label class="form-label-glass text-secondary">Ngày sinh (Dự kiến)</label>
                    <div class="input-group-custom">
                      <span class="input-icon"><i class="bi bi-calendar2-heart"></i></span>
                      <input v-model="form.birthDate" type="date" class="form-control-glass with-icon" :max="todayStr" />
                    </div>
                  </div>

                  <!-- Weight -->
                  <div class="col-sm-6">
                    <label class="form-label-glass text-secondary">Cân nặng (kg)</label>
                    <div class="input-group-custom">
                      <span class="input-icon"><i class="bi bi-speedometer2"></i></span>
                      <input v-model="form.weight" type="number" step="0.1" min="0" max="999" class="form-control-glass with-icon" placeholder="VD: 5.5" />
                    </div>
                  </div>

                  <!-- Color -->
                  <div class="col-sm-6">
                    <label class="form-label-glass text-secondary">Màu lông đặc trưng</label>
                    <div class="input-group-custom">
                      <span class="input-icon"><i class="bi bi-palette"></i></span>
                      <input v-model="form.color" type="text" class="form-control-glass with-icon" placeholder="VD: Vàng kem, Đen trắng..." />
                    </div>
                  </div>



                  <!-- Allergy Note -->
                  <div class="col-12">
                    <label class="form-label-glass text-secondary">Ghi chú sức khoẻ / Dị ứng</label>
                    <div class="input-group-custom textarea-custom align-items-start">
                      <span class="input-icon mt-2 pt-1"><i class="bi bi-heart-pulse"></i></span>
                      <textarea v-model="form.allergyNote" class="form-control-glass with-icon" rows="3" placeholder="VD: Dị ứng với thuốc penicillin, hoặc không ăn được thịt gà..."></textarea>
                    </div>
                  </div>
                </div>

                <div class="pet-modal-footer-glass border-top-glass mt-4 pt-4 d-flex justify-content-end gap-3">
                  <button type="button" class="btn btn-outline-glass px-4 py-2 rounded-pill fw-semibold" @click="closeModal" :disabled="formLoading">Huỷ bỏ</button>
                  <button type="submit" class="btn btn-premium-neon px-5 py-2 rounded-pill shadow-sm d-flex align-items-center" :disabled="formLoading">
                    <span v-if="formLoading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                    <i v-else class="bi bi-check2-circle me-2 fs-5"></i>
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
        <div v-if="showDetailModal && selectedPet" class="pet-modal-overlay-glass" @click.self="showDetailModal = false">
          <div class="pet-modal-card-glass detail-modal glass-panel">
            <div class="pet-modal-header-glass detail-header" :style="{ background: getPetAvatarColor(selectedPet.species) }">
              <div class="text-center w-100 py-3">
                <div v-if="selectedPet.avatar" class="detail-avatar-img-wrapper mb-2 mx-auto">
                  <img :src="getPetAvatarUrl(selectedPet.avatar)" alt="Pet Avatar" class="detail-avatar-img" />
                </div>
                <div v-else class="detail-avatar-img-wrapper mb-2 mx-auto">
                  <img :src="getSpeciesImageUrl(selectedPet.species)" alt="Pet Avatar" class="detail-avatar-img" />
                </div>
                <h4 class="fw-bold text-white mt-2 mb-0">{{ selectedPet.name }}</h4>
                <p class="text-white opacity-75 small mb-0">{{ selectedPet.species }} · {{ selectedPet.breed || 'Chưa xác định' }}</p>
              </div>
              <button class="modal-close-btn-glass text-white position-absolute top-0 end-0 m-3" @click="showDetailModal = false">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="pet-modal-body-glass p-4">
              <div class="row g-3">
                <div class="col-6">
                  <div class="detail-info-item-glass">
                    <span class="detail-label">Giới tính</span>
                    <span class="detail-value text-dark">{{ selectedPet.gender === 1 ? '♂ Đực' : selectedPet.gender === 2 ? '♀ Cái' : 'Không rõ' }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item-glass">
                    <span class="detail-label">Tuổi</span>
                    <span class="detail-value text-dark">{{ calculateAge(selectedPet.birthDate) }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item-glass">
                    <span class="detail-label">Ngày sinh</span>
                    <span class="detail-value text-dark">{{ formatDate(selectedPet.birthDate) }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item-glass">
                    <span class="detail-label">Cân nặng</span>
                    <span class="detail-value text-dark">{{ selectedPet.weight ? `${selectedPet.weight} kg` : 'Chưa cập nhật' }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item-glass">
                    <span class="detail-label">Màu lông</span>
                    <span class="detail-value text-dark">{{ selectedPet.color || '—' }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item-glass">
                    <span class="detail-label">Nhóm máu</span>
                    <span class="detail-value text-dark">{{ selectedPet.bloodType || '—' }}</span>
                  </div>
                </div>
                <div class="col-6">
                  <div class="detail-info-item-glass">
                    <span class="detail-label">Triệt sản</span>
                    <span class="detail-value">
                      <span v-if="selectedPet.sterilized" class="badge bg-success-glass">✓ Đã triệt sản</span>
                      <span v-else class="badge bg-secondary-glass">Chưa triệt sản</span>
                    </span>
                  </div>
                </div>

                <div v-if="selectedPet.allergyNote" class="col-12">
                  <div class="detail-info-item-glass allergy-detail-glass">
                    <span class="detail-label text-warning"><i class="bi bi-exclamation-triangle-fill me-1"></i>Dị ứng / Ghi chú đặc biệt</span>
                    <span class="detail-value text-dark">{{ selectedPet.allergyNote }}</span>
                  </div>
                </div>
                <div class="col-12">
                  <div class="detail-info-item-glass">
                    <span class="detail-label">Ngày đăng ký</span>
                    <span class="detail-value text-secondary-muted">{{ formatDate(selectedPet.createdAt) }}</span>
                  </div>
                </div>
              </div>

              <div class="d-flex gap-2 mt-4 pt-3 border-top-glass">
                <button class="btn btn-premium-neon flex-fill" @click="openEditFromDetail(selectedPet)">
                  <i class="bi bi-pencil-fill me-2"></i> Chỉnh sửa
                </button>
                <button class="btn btn-outline-glass px-4" @click="showDetailModal = false">Đóng</button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ===== DELETE CONFIRM MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDeleteModal && petToDelete" class="pet-modal-overlay-glass" @click.self="showDeleteModal = false">
          <div class="pet-modal-card-glass glass-panel" style="max-width: 420px;">
            <div class="pet-modal-header-glass bg-danger-glass border-bottom-glass p-3">
              <h5 class="fw-bold mb-0 text-dark"><i class="bi bi-exclamation-triangle-fill me-2 text-danger"></i>Xác nhận xoá</h5>
              <button class="modal-close-btn-glass text-dark" @click="showDeleteModal = false"><i class="bi bi-x-lg"></i></button>
            </div>
            <div class="pet-modal-body-glass p-4 text-center">
              <div class="mx-auto mb-3 pulse-icon" style="width: 80px; height: 80px; border-radius: 50%; overflow: hidden; border: 3px solid #fecaca;">
                 <img :src="getSpeciesImageUrl(petToDelete.species)" class="w-100 h-100" style="object-fit: cover;" />
              </div>
              <p class="text-dark fw-bold mb-1 fs-5">{{ petToDelete.name }}</p>
              <p class="text-secondary-muted small mb-4">Bạn có chắc muốn xoá hồ sơ của <strong>{{ petToDelete.name }}</strong>? Hành động này không thể hoàn tác.</p>
              <div class="d-flex gap-2 justify-content-center">
                <button class="btn btn-outline-glass px-4" @click="showDeleteModal = false" :disabled="deleteLoading">Huỷ bỏ</button>
                <button class="btn btn-danger-glass px-4 fw-bold" @click="deletePet" :disabled="deleteLoading">
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
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import api from '../../services/api';
import { translateApiError } from '../../utils/errorTranslator';

const backendUrl = import.meta.env.VITE_API_URL || 'http://localhost:5150';
const router = useRouter();
const { t } = useI18n();
const emit = defineEmits(['switch-tab', 'view-pet']);

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
  avatar: string | null;
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
  avatar: string | null;
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
const formValidationError = ref(false);
const avatarUploading = ref(false);

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
  avatar: null,
});

const form = ref<PetForm>(defaultForm());

const todayStr = computed(() => {
  return new Date().toISOString().split('T')[0];
});

// ===== Helper for Avatar URL =====
const getPetAvatarUrl = (avatarPath: string | null) => {
  if (!avatarPath) return '';
  if (avatarPath.startsWith('http')) return avatarPath;
  return `${backendUrl}${avatarPath}`;
};

// ===== Handle Avatar Upload =====
const handlePetAvatarUpload = async (event: Event) => {
  const fileInput = event.target as HTMLInputElement;
  if (!fileInput.files || fileInput.files.length === 0) return;

  const file = fileInput.files[0];
  
  // Client-side image validation (NFR & UX)
  const allowedExtensions = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
  if (!allowedExtensions.includes(file.type)) {
    formError.value = 'Chỉ chấp nhận ảnh định dạng: JPG, JPEG, PNG, GIF';
    triggerValidationError();
    return;
  }

  if (file.size > 2 * 1024 * 1024) {
    formError.value = 'Kích thước ảnh đại diện không được vượt quá 2MB';
    triggerValidationError();
    return;
  }

  const formData = new FormData();
  formData.append('avatarFile', file);

  avatarUploading.value = true;
  formError.value = '';
  try {
    const response = await api.post('/mypets/upload-avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    if (response.data.success) {
      form.value.avatar = response.data.avatarUrl;
    }
  } catch (error: any) {
    formError.value = error.response?.data?.message || 'Không thể tải ảnh thú cưng lên.';
  } finally {
    avatarUploading.value = false;
  }
};

const triggerValidationError = () => {
  formValidationError.value = true;
  setTimeout(() => {
    formValidationError.value = false;
  }, 500);
};

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
  if (!form.value.name || !form.value.species) {
    formError.value = 'Vui lòng điền đầy đủ các thông tin bắt buộc.';
    triggerValidationError();
    return;
  }

  formLoading.value = true;
  formError.value = '';
  formSuccess.value = '';
  try {
    if (isEditing.value && form.value.id) {
      await api.put(`/mypets/${form.value.id}`, form.value);
      formSuccess.value = `Đã cập nhật hồ sơ ${form.value.name} thành công!`;
    } else {
      await api.post('/mypets', form.value);
      formSuccess.value = `Đã thêm ${form.value.name} thành công!`;
    }
    await fetchPets();
    setTimeout(() => closeModal(), 1200);
  } catch (err: any) {
    formError.value = translateApiError(err, t, 'DEFAULT');
    triggerValidationError();
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
    alert(translateApiError(err, t, 'DEFAULT'));
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
    avatar: pet.avatar ?? null,
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

const getSpeciesImageUrl = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': 'https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=300&h=300&fit=crop',
    'Mèo': 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=300&h=300&fit=crop',
    'Thỏ': 'https://images.unsplash.com/photo-1585110396000-c9fd45c265fc?w=300&h=300&fit=crop',
    'Chim': 'https://images.unsplash.com/photo-1522926193341-e9eb1b369405?w=300&h=300&fit=crop',
    'Cá': 'https://images.unsplash.com/photo-1524704796725-9fc3044a58b2?w=300&h=300&fit=crop',
    'Bò sát': 'https://images.unsplash.com/photo-1504450758481-7338eba7524a?w=300&h=300&fit=crop',
  };
  return map[species ?? ''] || 'https://images.unsplash.com/photo-1548767797-d8c844163c4c?w=300&h=300&fit=crop';
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
    'Chó': 'linear-gradient(135deg, hsl(35, 95%, 60%), hsl(35, 95%, 45%))',
    'Mèo': 'linear-gradient(135deg, hsl(265, 85%, 65%), hsl(265, 85%, 50%))',
    'Thỏ': 'linear-gradient(135deg, hsl(330, 85%, 65%), hsl(330, 85%, 50%))',
    'Chim': 'linear-gradient(135deg, hsl(190, 90%, 55%), hsl(190, 90%, 40%))',
    'Cá': 'linear-gradient(135deg, hsl(220, 90%, 60%), hsl(220, 90%, 45%))',
    'Bò sát': 'linear-gradient(135deg, hsl(150, 80%, 50%), hsl(150, 80%, 35%))',
  };
  return map[species ?? ''] || 'linear-gradient(135deg, hsl(210, 10%, 50%), hsl(210, 10%, 35%))';
};

// ===== Lifecycle =====
onMounted(fetchPets);
</script>

<style scoped>
/* Glassmorphism Design System Stylesheet */
.glass-container {
  min-height: 100%;
}

.glass-panel {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(245, 158, 11, 0.12);
  border-radius: 20px;
  box-shadow: 0 8px 32px 0 rgba(217, 119, 6, 0.04);
}

.text-secondary-muted {
  color: var(--text-muted) !important;
}

.border-bottom-glass {
  border-bottom: 1px solid rgba(217, 119, 6, 0.08);
}

.border-top-glass {
  border-top: 1px solid rgba(217, 119, 6, 0.08);
}

/* Hero Section */
.pets-hero {
  background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%) !important;
  border-left: 5px solid var(--primary-gold);
  border-radius: 20px;
}

/* Premium Neon Button */
.btn-premium-neon {
  background: linear-gradient(135deg, var(--primary-gold), var(--primary-dark));
  color: white;
  border: none;
  padding: 0.65rem 1.6rem;
  border-radius: 50px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 15px rgba(245, 158, 11, 0.3);
  display: inline-flex;
  align-items: center;
}

.btn-premium-neon:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.45);
  filter: brightness(1.05);
}

.btn-outline-glass {
  background: rgba(255, 255, 255, 0.6);
  color: var(--text-dark);
  border: 1px solid rgba(217, 119, 6, 0.2);
  padding: 0.65rem 1.6rem;
  border-radius: 50px;
  font-weight: 600;
  font-size: 0.9rem;
  transition: all 0.3s ease;
}

.btn-outline-glass:hover {
  background: rgba(255, 255, 255, 0.9);
  border-color: var(--primary-gold);
  color: var(--primary-dark);
}

/* Glassmorphic Pet Card */
.pet-card-glass {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  overflow: hidden;
  position: relative;
  background: rgba(255, 255, 255, 0.85);
  border: 1px solid rgba(217, 119, 6, 0.15);
  box-shadow: 0 10px 30px -10px rgba(217, 119, 6, 0.08);
}

.pet-card-glass:hover {
  transform: translateY(-8px);
  box-shadow: 0 20px 40px rgba(217, 119, 6, 0.12);
  border-color: rgba(245, 158, 11, 0.4);
}

.pet-card-banner {
  height: 80px;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 0.9rem 1.2rem;
}

.species-dog .pet-card-banner { background: linear-gradient(180deg, rgba(245, 158, 11, 0.08), transparent); }
.species-cat .pet-card-banner { background: linear-gradient(180deg, rgba(139, 92, 246, 0.08), transparent); }
.species-rabbit .pet-card-banner { background: linear-gradient(180deg, rgba(236, 72, 153, 0.08), transparent); }
.species-bird .pet-card-banner { background: linear-gradient(180deg, rgba(6, 182, 212, 0.08), transparent); }
.species-fish .pet-card-banner { background: linear-gradient(180deg, rgba(59, 130, 246, 0.08), transparent); }
.species-reptile .pet-card-banner { background: linear-gradient(180deg, rgba(16, 185, 129, 0.08), transparent); }
.species-other .pet-card-banner { background: linear-gradient(180deg, rgba(217, 119, 6, 0.05), transparent); }

.pet-species-badge {
  background: rgba(245, 158, 11, 0.1);
  padding: 5px 14px;
  border-radius: 20px;
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--primary-dark);
  border: 1px solid rgba(245, 158, 11, 0.15);
}

.species-cat .pet-species-badge {
  background: rgba(139, 92, 246, 0.1);
  color: #6d28d9;
  border-color: rgba(139, 92, 246, 0.2);
}
.species-rabbit .pet-species-badge {
  background: rgba(236, 72, 153, 0.1);
  color: #be185d;
  border-color: rgba(236, 72, 153, 0.2);
}
.species-bird .pet-species-badge {
  background: rgba(6, 182, 212, 0.1);
  color: #0e7490;
  border-color: rgba(6, 182, 212, 0.2);
}
.species-fish .pet-species-badge {
  background: rgba(59, 130, 246, 0.1);
  color: #1d4ed8;
  border-color: rgba(59, 130, 246, 0.2);
}
.species-reptile .pet-species-badge {
  background: rgba(16, 185, 129, 0.1);
  color: #047857;
  border-color: rgba(16, 185, 129, 0.2);
}

.pet-card-actions {
  display: flex;
  gap: 8px;
  opacity: 0;
  transform: translateY(-5px);
  transition: all 0.3s ease;
}

.pet-card-glass:hover .pet-card-actions {
  opacity: 1;
  transform: translateY(0);
}

.action-btn {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 1px solid rgba(217, 119, 6, 0.15);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  background: white;
  transition: all 0.25s ease;
}

.action-btn.edit {
  background: #fffbeb;
  color: #f59e0b;
}

.action-btn.edit:hover {
  background: #f59e0b;
  color: white;
  box-shadow: 0 0 10px rgba(245, 158, 11, 0.3);
}

.action-btn.delete {
  background: #fef2f2;
  color: #ef4444;
}

.action-btn.delete:hover {
  background: #ef4444;
  color: white;
  box-shadow: 0 0 10px rgba(239, 68, 68, 0.3);
}

/* Avatar layout */
.pet-avatar-wrapper {
  display: flex;
  justify-content: center;
  margin-top: -30px;
  position: relative;
  margin-bottom: 0.8rem;
}

.pet-avatar-img-wrapper {
  width: 76px;
  height: 76px;
  border-radius: 50%;
  padding: 3px;
  background: linear-gradient(135deg, #f59e0b, #d97706);
  box-shadow: 0 4px 10px rgba(217, 119, 6, 0.25);
}

.pet-avatar-img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid white;
}

.pet-avatar {
  width: 76px;
  height: 76px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  border: 3px solid white;
  box-shadow: 0 4px 10px rgba(217, 119, 6, 0.2);
}

.sterilized-badge {
  position: absolute;
  bottom: 0;
  right: calc(50% - 38px);
  background: #10b981;
  color: white;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
  border: 2px solid white;
  box-shadow: 0 2px 8px rgba(16, 185, 129, 0.4);
}

/* Stats grid inside card */
.pet-stats-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 10px;
  background: rgba(245, 158, 11, 0.03);
  border-radius: 12px;
  padding: 12px;
  border: 1px solid rgba(245, 158, 11, 0.08);
}

.pet-stat-item {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 0.72rem;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-value {
  font-size: 0.85rem;
  font-weight: 600;
}

/* Badges and Alerts */
.allergy-note-glass {
  background: rgba(239, 68, 68, 0.05);
  border: 1px solid rgba(239, 68, 68, 0.15);
  border-radius: 10px;
  padding: 8px 12px;
  font-size: 0.8rem;
  color: #dc2626;
  display: flex;
  align-items: center;
}

.microchip-badge-glass {
  background: rgba(6, 182, 212, 0.05);
  border: 1px solid rgba(6, 182, 212, 0.15);
  border-radius: 10px;
  padding: 6px 12px;
  font-size: 0.8rem;
  color: #0891b2;
  display: flex;
  align-items: center;
  font-family: monospace;
}

.btn-view-detail-glass {
  background: white;
  color: var(--primary-dark);
  border: 1px solid rgba(245, 158, 11, 0.25);
  border-radius: 12px;
  padding: 8px;
  font-weight: 600;
  font-size: 0.82rem;
  transition: all 0.3s ease;
  cursor: pointer;
}

.btn-view-detail-glass:hover {
  background: var(--primary-cream);
  border-color: var(--primary-gold);
  color: var(--primary-dark);
}

.btn-view-detail-glass:hover .transition-arrow {
  transform: translateX(4px);
}

.transition-arrow {
  display: inline-block;
  transition: transform 0.25s ease;
}

/* Modals glass */
.pet-modal-overlay-glass {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(41, 37, 36, 0.4);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1050;
}

.pet-modal-card-glass {
  width: 90%;
  max-width: 650px;
  max-height: 90vh;
  overflow-y: auto;
  background: white !important;
  border: 1px solid rgba(245, 158, 11, 0.2) !important;
  border-radius: 20px;
  box-shadow: 0 25px 50px -12px rgba(217, 119, 6, 0.15);
}

.modal-close-btn-glass {
  background: transparent;
  border: none;
  color: var(--text-muted);
  font-size: 1.2rem;
  cursor: pointer;
  transition: color 0.25s ease;
}

.modal-close-btn-glass:hover {
  color: var(--text-dark);
}

/* Form inputs Glassmorphic */
.form-label-glass {
  color: var(--text-dark);
  font-size: 0.85rem;
  font-weight: 600;
  margin-bottom: 5px;
}

.form-control-glass {
  background: white !important;
  border: 1px solid rgba(217, 119, 6, 0.2) !important;
  border-radius: 12px !important;
  color: var(--text-dark) !important;
  padding: 10px 14px !important;
  width: 100%;
  font-size: 0.9rem;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.form-control-glass:focus {
  background: white !important;
  border-color: var(--primary-gold) !important;
  box-shadow: 0 0 0 4px rgba(245, 158, 11, 0.15) !important;
  outline: none;
}

select.form-control-glass option {
  background: white;
  color: var(--text-dark);
}

/* Custom Input Groups */
.input-group-custom {
  position: relative;
  display: flex;
  align-items: center;
  width: 100%;
}

.input-icon {
  position: absolute;
  left: 14px;
  color: var(--text-muted);
  opacity: 0.7;
  z-index: 5;
  pointer-events: none;
  transition: all 0.3s ease;
}

.form-control-glass.with-icon {
  padding-left: 40px !important;
}

.input-group-custom:focus-within .input-icon {
  color: var(--primary-gold);
  opacity: 1;
}

/* Switch Custom Glass */
.form-check-input {
  cursor: pointer;
}

.form-switch-glass .form-check-input {
  background-color: rgba(217, 119, 6, 0.1);
  border-color: rgba(217, 119, 6, 0.25);
  width: 2.5em;
  height: 1.3em;
}

.form-switch-glass .form-check-input:checked {
  background-color: #10b981;
  border-color: #10b981;
}

/* Upload profile picture */
.pet-upload-container {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  position: relative;
  background: var(--primary-cream);
  border: 2px dashed rgba(245, 158, 11, 0.3);
}

.pet-upload-preview {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
}

.pet-upload-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.pet-upload-label {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--primary-gold);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 4px 10px rgba(217, 119, 6, 0.2);
  transition: transform 0.25s ease;
}

.pet-upload-label:hover {
  transform: scale(1.1);
}

/* Details Modal spec */
.detail-modal {
  max-width: 520px;
}

.detail-header {
  border-top-left-radius: 20px;
  border-top-right-radius: 20px;
  position: relative;
}

.detail-avatar-img-wrapper {
  width: 90px;
  height: 90px;
  border-radius: 50%;
  border: 3px solid white;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  overflow: hidden;
}

.detail-avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.detail-avatar {
  width: 90px;
  height: 90px;
  border-radius: 50%;
  background: rgba(255,255,255,0.25);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 3rem;
  border: 3px solid white;
}

.detail-info-item-glass {
  background: rgba(245, 158, 11, 0.02);
  border: 1px solid rgba(245, 158, 11, 0.08);
  border-radius: 12px;
  padding: 10px 14px;
  display: flex;
  flex-direction: column;
}

.detail-label {
  font-size: 0.75rem;
  color: var(--text-muted);
}

.detail-value {
  font-size: 0.95rem;
  font-weight: 600;
}

.bg-success-glass {
  background: rgba(16, 185, 129, 0.1) !important;
  color: #10b981 !important;
  border: 1px solid rgba(16, 185, 129, 0.2);
}

.bg-secondary-glass {
  background: rgba(217, 119, 6, 0.05) !important;
  color: var(--text-muted) !important;
}

.allergy-detail-glass {
  background: rgba(239, 68, 68, 0.03);
  border-color: rgba(239, 68, 68, 0.1);
}

/* Skeleton loader for cards */
.pet-card-skeleton {
  height: 380px;
  overflow: hidden;
  background: white;
}

.skeleton-banner {
  height: 80px;
  background: rgba(217, 119, 6, 0.04);
}

.skeleton-avatar {
  width: 76px;
  height: 76px;
  border-radius: 50%;
  background: rgba(217, 119, 6, 0.04);
  margin-top: -38px;
}

.skeleton-line {
  background: rgba(217, 119, 6, 0.04);
  border-radius: 4px;
}

.skeleton-line.title {
  width: 50%;
  height: 20px;
  margin: 0 auto;
}

.skeleton-line.subtitle {
  width: 30%;
  height: 14px;
  margin: 0 auto;
}

.skeleton-line.stat {
  height: 40px;
  border-radius: 12px;
}

.skeleton-button {
  height: 38px;
  border-radius: 12px;
  background: rgba(217, 119, 6, 0.02);
}

.skeleton-banner, .skeleton-avatar, .skeleton-line, .skeleton-button {
  animation: pulse 1.8s infinite ease-in-out;
}

@keyframes pulse {
  0% { opacity: 0.5; }
  50% { opacity: 1; }
  100% { opacity: 0.5; }
}

/* Animations */
.pulse-icon {
  animation: pulse-glow 2s infinite ease-in-out;
}

.pulse-gold-icon {
  color: #f59e0b;
  animation: pulse-gold 2.5s infinite ease-in-out;
}

@keyframes pulse-glow {
  0% { transform: scale(1); filter: drop-shadow(0 0 2px rgba(245, 158, 11, 0.2)); }
  50% { transform: scale(1.05); filter: drop-shadow(0 0 8px rgba(245, 158, 11, 0.6)); }
  100% { transform: scale(1); filter: drop-shadow(0 0 2px rgba(245, 158, 11, 0.2)); }
}

@keyframes pulse-gold {
  0% { filter: drop-shadow(0 0 1px rgba(245, 158, 11, 0.3)); }
  50% { filter: drop-shadow(0 0 6px rgba(245, 158, 11, 0.8)); }
  100% { filter: drop-shadow(0 0 1px rgba(245, 158, 11, 0.3)); }
}

.shake-animation {
  animation: shake 0.4s ease-in-out;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-8px); }
  75% { transform: translateX(8px); }
}

.pet-card-enter-active,
.pet-card-leave-active {
  transition: all 0.5s ease;
}
.pet-card-enter-from,
.pet-card-leave-to {
  opacity: 0;
  transform: translateY(30px) scale(0.9);
}

.truncate-text {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
