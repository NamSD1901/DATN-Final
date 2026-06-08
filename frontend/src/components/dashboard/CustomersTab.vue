<template>
  <div class="customers-tab container-fluid p-0">
    <!-- Main Panel: Lists and Search -->
    <div v-if="!selectedCustomerDetail" class="card border-0 shadow-sm rounded-4 p-4 bg-white">
      <!-- Header Actions -->
      <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
        <div>
          <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-people-fill text-warning me-2"></i>Quản lý Khách hàng</h4>
          <p class="text-muted small mb-0">Tra cứu nhanh SĐT, tạo mới hồ sơ chủ nuôi và các bé thú cưng</p>
        </div>
        <button class="btn btn-premium px-4 py-2.5 rounded-pill shadow-sm" @click="openCreateCustomerModal">
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
                  <i class="bi bi-phone me-1"></i>{{ quickSearchResult.phone }} | 
                  <i class="bi bi-envelope me-1"></i>{{ quickSearchResult.email || 'Không có email' }}
                </div>
              </div>
            </div>
            <div class="d-flex gap-2">
              <button class="btn btn-sm btn-outline-warning rounded-pill px-3 fw-bold" @click="viewCustomerDetail(quickSearchResult.customerId)">
                <i class="bi bi-eye me-1"></i>Xem Hồ Sơ
              </button>
              <button class="btn btn-sm btn-premium rounded-pill px-3" @click="openAddPetModalDirect(quickSearchResult.customerId)">
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
          <span>Không tìm thấy chủ nuôi. <a href="#" class="fw-bold text-warning" @click.prevent="openCreateCustomerModalWithPhone">Tạo hồ sơ mới với số điện thoại này?</a></span>
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
                  <i class="bi bi-phone me-1"></i>{{ cust.phone }}
                </span>
              </td>
              <td class="text-muted small text-truncate" style="max-width: 250px;">{{ cust.address || '—' }}</td>
              <td class="text-muted small">{{ formatDate(cust.createdAt) }}</td>
              <td class="text-center">
                <button class="btn btn-sm btn-outline-warning rounded-pill px-4 fw-bold shadow-sm" @click="viewCustomerDetail(cust.id)">
                  <i class="bi bi-eye me-1"></i>Xem Hồ Sơ
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Customer Detailed Medical History Profile -->
    <div v-else class="card border-0 shadow-sm rounded-4 p-4 bg-white animate-fade-in">
      <!-- Back Button -->
      <div class="d-flex align-items-center gap-2 mb-4">
        <button class="btn btn-light rounded-circle p-2 border shadow-sm" @click="selectedCustomerDetail = null">
          <i class="bi bi-arrow-left fs-5"></i>
        </button>
        <span class="fw-bold text-muted">Quay lại danh sách khách hàng</span>
      </div>

      <!-- Loading Details state -->
      <div v-if="loadingDetail" class="text-center py-5">
        <div class="spinner-border text-warning mb-3"></div>
        <div>Đang tải hồ sơ bệnh án khách hàng...</div>
      </div>

      <!-- Details Content -->
      <div v-else-if="detailData" class="row g-4">
        <!-- Customer Left Card -->
        <div class="col-lg-4">
          <div class="card border-0 shadow-sm bg-light rounded-4 p-4 text-center h-100">
            <div class="avatar-circle-gold fs-2 mx-auto mb-3" style="width: 100px; height: 100px;">
              {{ getAvatarLetters(detailData.customer.fullName) }}
            </div>
            <h4 class="fw-bold text-dark mb-1">{{ detailData.customer.fullName }}</h4>
            <span class="badge bg-warning text-dark px-3 py-1.5 rounded-pill fw-bold text-uppercase small shadow-sm mb-4">
              Khách hàng thân thiết
            </span>

            <div class="text-start mt-3 border-top pt-3">
              <div class="mb-3">
                <label class="text-muted small d-block mb-1">Số điện thoại</label>
                <strong class="text-dark"><i class="bi bi-telephone text-warning me-1"></i>{{ detailData.customer.phone }}</strong>
              </div>
              <div class="mb-3">
                <label class="text-muted small d-block mb-1">Địa chỉ Email</label>
                <strong class="text-dark"><i class="bi bi-envelope text-warning me-1"></i>{{ detailData.customer.email || 'Chưa cung cấp' }}</strong>
              </div>
              <div class="mb-3">
                <label class="text-muted small d-block mb-1">Địa chỉ liên hệ</label>
                <strong class="text-dark"><i class="bi bi-geo-alt text-warning me-1"></i>{{ detailData.customer.address || 'Chưa cung cấp' }}</strong>
              </div>
              <div class="mb-0">
                <label class="text-muted small d-block mb-1">Ngày tham gia hệ thống</label>
                <strong class="text-dark"><i class="bi bi-calendar-check text-warning me-1"></i>{{ formatDate(detailData.customer.createdAt) }}</strong>
              </div>
            </div>
          </div>
        </div>

        <!-- Customer Right panel (Stats + Pets + Medical Timeline) -->
        <div class="col-lg-8">
          <!-- KPI Widgets Grid -->
          <div class="row g-3 mb-4">
            <div class="col-sm-6 col-md-3">
              <div class="card border-0 shadow-sm rounded-4 p-3 bg-primary bg-opacity-10 text-primary h-100">
                <div class="d-flex justify-content-between align-items-center">
                  <div>
                    <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Số ca khám</h6>
                    <h4 class="fw-extrabold mb-0">{{ detailData.totalVisits }}</h4>
                  </div>
                  <i class="bi bi-stethoscope fs-2 opacity-50"></i>
                </div>
              </div>
            </div>
            <div class="col-sm-6 col-md-3">
              <div class="card border-0 shadow-sm rounded-4 p-3 bg-success bg-opacity-10 text-success h-100">
                <div class="d-flex justify-content-between align-items-center">
                  <div>
                    <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Đã chi tiêu</h6>
                    <h4 class="fw-extrabold mb-0">{{ formatCurrency(detailData.totalSpent) }}</h4>
                  </div>
                  <i class="bi bi-wallet2 fs-2 opacity-50"></i>
                </div>
              </div>
            </div>
            <div class="col-sm-6 col-md-3">
              <div class="card border-0 shadow-sm rounded-4 p-3 bg-danger bg-opacity-10 text-danger h-100">
                <div class="d-flex justify-content-between align-items-center">
                  <div>
                    <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Bỏ hẹn/Hủy</h6>
                    <h4 class="fw-extrabold mb-0">{{ detailData.noShowCount }}</h4>
                  </div>
                  <i class="bi bi-calendar-x fs-2 opacity-50"></i>
                </div>
              </div>
            </div>
            <div class="col-sm-6 col-md-3">
              <div class="card border-0 shadow-sm rounded-4 p-3 bg-warning bg-opacity-10 text-dark-gold h-100">
                <div class="d-flex justify-content-between align-items-center">
                  <div>
                    <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Còn nợ</h6>
                    <h4 class="fw-extrabold mb-0">{{ formatCurrency(detailData.unpaidBalance) }}</h4>
                  </div>
                  <i class="bi bi-cash-stack fs-2 opacity-50"></i>
                </div>
              </div>
            </div>
          </div>

          <!-- Pets Management block -->
          <div class="card border-0 shadow-sm rounded-4 p-4 mb-4 bg-light">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5 class="fw-bold text-dark mb-0"><i class="bi bi-heptagon-fill text-warning me-2"></i>Thú cưng đăng ký</h5>
              <button class="btn btn-sm btn-premium rounded-pill px-3" @click="openAddPetModal">
                <i class="bi bi-plus-lg me-1"></i> Đăng ký thêm bé
              </button>
            </div>

            <div class="row g-3">
              <div v-if="detailData.pets.length === 0" class="col-12 text-center text-muted py-4">
                Chưa có thú cưng nào được đăng ký cho chủ này.
              </div>
              <div v-for="pet in detailData.pets" :key="pet.id" class="col-md-6">
                <div class="card border-0 rounded-4 shadow-sm p-3 bg-white h-100 position-relative">
                  <div class="position-absolute top-0 end-0 p-3">
                    <button class="btn btn-sm btn-light rounded-circle shadow-sm text-primary" @click="openEditPetModal(pet)" title="Sửa thông tin">
                      <i class="bi bi-pencil-fill"></i>
                    </button>
                  </div>
                  <div class="d-flex align-items-center gap-3">
                    <span class="fs-1 bg-light p-2 rounded-circle d-inline-block">{{ getAnimalEmoji(pet.species) }}</span>
                    <div>
                      <h6 class="fw-bold text-dark mb-1">{{ pet.name }}</h6>
                      <p class="text-muted small mb-0">{{ pet.breed || pet.species }} | {{ getGenderText(pet.gender) }}</p>
                      <small class="text-muted">Cân nặng: {{ pet.weight ? pet.weight + ' kg' : '—' }}</small>
                    </div>
                  </div>
                  <div class="d-flex justify-content-between align-items-center mt-3 pt-2 border-top small text-muted">
                    <span>Mã Microchip: <strong>{{ pet.microchipCode || '—' }}</strong></span>
                    <span class="badge rounded-pill" :class="pet.sterilized ? 'bg-success' : 'bg-secondary'">
                      {{ pet.sterilized ? 'Đã triệt sản' : 'Chưa triệt sản' }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Appointments / Clinical records timeline -->
          <div class="card border-0 shadow-sm rounded-4 p-4 bg-light">
            <h5 class="fw-bold text-dark mb-3"><i class="bi bi-clock-history text-warning me-2"></i>Lịch sử khám & Điều trị</h5>
            <div class="table-responsive">
              <table class="table table-hover align-middle bg-white rounded-4 overflow-hidden mb-0">
                <thead class="table-light">
                  <tr>
                    <th>Thời gian</th>
                    <th>Thú cưng</th>
                    <th>Dịch vụ</th>
                    <th>Bác sĩ</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="detailData.appointments.length === 0" class="text-center text-muted">
                    <td colspan="5" class="py-4">Chưa có lịch sử khám bệnh nào ghi nhận.</td>
                  </tr>
                  <tr v-for="appt in detailData.appointments" :key="appt.id">
                    <td class="small text-muted">{{ formatDateFull(appt.appointmentTime) }}</td>
                    <td class="fw-bold text-dark">{{ appt.petName }}</td>
                    <td><span class="badge bg-success bg-opacity-10 text-success rounded px-2.5 py-1 fw-bold">{{ appt.serviceName }}</span></td>
                    <td class="small">{{ appt.doctorName }}</td>
                    <td>
                      <span class="badge rounded-pill" :class="getStatusBadgeClass(appt.status)">
                        {{ getStatusLabel(appt.status) }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>

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
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Mã Microchip</label>
              <input type="text" v-model="petForm.microchipCode" class="form-control" placeholder="Mã định danh chíp..." />
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
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Mã Microchip</label>
              <input type="text" v-model="editPetForm.microchipCode" class="form-control" />
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
import { ref, onMounted } from 'vue';
import api from '../../services/api';

// State
const loading = ref(false);
const customersList = ref<any[]>([]);
const totalCustomers = ref(0);
const searchKeyword = ref('');

// Quick search phone
const quickPhoneQuery = ref('');
const quickSearchResult = ref<any>(null);

// Customer Detail
const selectedCustomerDetail = ref<string | null>(null);
const loadingDetail = ref(false);
const detailData = ref<any>(null);

// Modals
const showCreateCustomerModal = ref(false);
const showAddPetModal = ref(false);
const showEditPetModal = ref(false);

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

// Load Customers
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

// Search Debounce
let searchTimer: any = null;
const debouncedSearch = () => {
  clearTimeout(searchTimer);
  searchTimer = setTimeout(() => {
    loadCustomers();
  }, 400);
};

// Quick phone search
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

// View Customer Details Medical History
const viewCustomerDetail = async (customerId: string) => {
  selectedCustomerDetail.value = customerId;
  loadingDetail.value = true;
  detailData.value = null;
  try {
    const res = await api.get(`/receptionist/customers/${customerId}`);
    detailData.value = res.data;
  } catch (err) {
    console.error(err);
    alert('Không thể tải hồ sơ chi tiết khách hàng.');
    selectedCustomerDetail.value = null;
  } finally {
    loadingDetail.value = false;
  }
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

const openCreateCustomerModalWithPhone = () => {
  openCreateCustomerModal();
  createForm.value.phone = quickPhoneQuery.value;
};

const submitCreateCustomer = async () => {
  try {
    const res = await api.post('/receptionist/customers', createForm.value);
    if (res.data.success) {
      alert(res.data.message || 'Tạo hồ sơ khách hàng thành công!');
      showCreateCustomerModal.value = false;
      await loadCustomers();
      // Auto open detailed medical history
      if (res.data.customerId) {
        await viewCustomerDetail(res.data.customerId);
      }
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi tạo hồ sơ chủ nuôi.');
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
      if (selectedCustomerDetail.value) {
        await viewCustomerDetail(selectedCustomerDetail.value);
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
      if (selectedCustomerDetail.value) {
        await viewCustomerDetail(selectedCustomerDetail.value);
      }
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra.');
  }
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

const getGenderText = (g: number) => {
  if (g === 1) return 'Đực';
  if (g === 2) return 'Cái';
  return 'Khác';
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatDateFull = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const formatCurrency = (val: number) => {
  if (val === undefined || val === null) return '0đ';
  return val.toLocaleString('vi-VN') + 'đ';
};

const getStatusBadgeClass = (status: string) => {
  const s = (status || '').toLowerCase();
  if (s === 'completed') return 'bg-success text-white';
  if (s === 'ready_to_pay') return 'bg-info text-dark';
  if (s === 'in_progress') return 'bg-warning text-dark';
  if (s === 'cancelled') return 'bg-danger text-white';
  return 'bg-secondary text-white';
};

const getStatusLabel = (status: string) => {
  const s = (status || '').toLowerCase();
  if (s === 'completed') return 'Hoàn thành';
  if (s === 'ready_to_pay') return 'Chờ thanh toán';
  if (s === 'in_progress') return 'Đang khám';
  if (s === 'cancelled') return 'Đã hủy';
  if (s === 'waiting') return 'Đang chờ';
  return status;
};

onMounted(() => {
  loadCustomers();
});
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
