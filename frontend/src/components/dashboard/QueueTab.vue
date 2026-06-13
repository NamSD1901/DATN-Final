<template>
  <div class="queue-tab container-fluid p-0">
    <!-- Action Bar & Omni Search -->
    <div class="row mb-4">
      <div class="col-12">
        <div class="card border-0 shadow-sm rounded-4 p-4 bg-gold-gradient position-relative overflow-hidden">
          <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 position-relative z-index-1">
            <div class="flex-grow-1" style="min-width: 300px; max-width: 600px;">
              <label class="form-label fw-bold text-dark mb-2"><i class="bi bi-search me-1 text-warning"></i>Tìm kiếm nhanh chủ nuôi, thú cưng hoặc mã số</label>
              <div class="input-group">
                <span class="input-group-text bg-white border-end-0 rounded-start-pill"><i class="bi bi-search text-muted"></i></span>
                <input 
                  type="text" 
                  v-model="searchQuery" 
                  @input="handleOmniSearch"
                  class="form-control border-start-0 rounded-end-pill input-premium" 
                  placeholder="Nhập tên khách hàng, SĐT, loài, tên thú cưng..." 
                />
              </div>
              <!-- Search Autocomplete Results -->
              <div v-if="searchResult.length > 0" class="position-absolute bg-white shadow-lg border rounded-4 mt-2 p-2 w-100 search-dropdown" style="z-index: 1050; max-height: 300px; overflow-y: auto;">
                <div 
                  v-for="item in searchResult" 
                  :key="item.id" 
                  class="p-2 border-bottom hover-bg-light cursor-pointer rounded-3 d-flex justify-content-between align-items-center"
                  @click="selectSearchResult(item)"
                >
                  <div>
                    <span class="fw-bold text-dark">{{ item.fullName }}</span> 
                    <span class="text-muted small ms-2">({{ item.phone }})</span>
                    <div class="text-muted small" v-if="item.pets && item.pets.length > 0">
                      Thú cưng: {{ item.pets.map((p: any) => p.name).join(', ') }}
                    </div>
                  </div>
                  <button class="btn btn-sm btn-outline-warning rounded-pill px-3 py-1">Chọn</button>
                </div>
              </div>
            </div>

            <div class="d-flex flex-wrap gap-2">
              <button class="btn btn-premium px-4 py-2 rounded-pill shadow-sm" @click="openQrScanModal">
                <i class="bi bi-qr-code-scan"></i> Quét QR
              </button>
              <button class="btn btn-danger px-4 py-2 rounded-pill shadow-sm fw-bold" @click="openEmergencyModal">
                <i class="bi bi-exclamation-triangle-fill"></i> CẤP CỨU
              </button>
              <button class="btn btn-warning text-dark px-4 py-2 rounded-pill shadow-sm fw-bold" @click="openWalkInModal">
                <i class="bi bi-person-walking"></i> Khách Vãng Lai
              </button>
              <router-link to="/tv-board" target="_blank" class="btn btn-outline-dark px-4 py-2 rounded-pill shadow-sm fw-bold" style="background: white; border: 1.5px solid #1e293b;">
                <i class="bi bi-display-fill text-warning"></i> Mở TV Board 🖥️
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Filter & Digital Whiteboard Header -->
    <div class="card border-0 shadow-sm rounded-4 p-3 mb-4 bg-white">
      <div class="d-flex flex-wrap justify-content-between align-items-center gap-3">
        <h5 class="fw-bold mb-0 text-dark">
          <i class="bi bi-display me-2 text-warning"></i> Digital Whiteboard - Hàng Khám
        </h5>
        <div class="d-flex align-items-center gap-3">
          <span class="text-muted small fw-bold">Bộ lọc Bác sĩ:</span>
          <select v-model="selectedDoctor" @change="loadQueue" class="form-select border-warning rounded-pill px-3 py-1.5 shadow-sm" style="width: 220px;">
            <option value="ALL">Tất cả Bác sĩ</option>
            <option v-for="doc in doctorList" :key="doc.id" :value="doc.id">
              Bs. {{ doc.fullName }}
            </option>
          </select>
          <button class="btn btn-light border rounded-circle p-2 shadow-sm" @click="loadQueue" title="Làm mới">
            <i class="bi bi-arrow-clockwise"></i>
          </button>
        </div>
      </div>
    </div>

    <!-- Kanban Columns -->
    <div class="row g-4 kanban-container">
      <!-- Waiting Column -->
      <div class="col-lg-4">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-light h-100 column-waiting">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h6 class="fw-bold text-secondary mb-0 text-uppercase tracking-wider">
              <i class="bi bi-hourglass-split me-2"></i> Đang chờ
            </h6>
            <span class="badge bg-secondary rounded-pill px-3 shadow-sm">{{ filteredWaiting.length }}</span>
          </div>

          <div class="kanban-list flex-grow-1" style="min-height: 450px;">
            <div v-if="filteredWaiting.length === 0" class="text-center py-5 text-muted">
              <i class="bi bi-emoji-smile fs-2 text-muted mb-2 d-block"></i>
              <span class="small">Không có bệnh nhi nào đang chờ.</span>
            </div>
            
            <div 
              v-for="card in filteredWaiting" 
              :key="card.appointmentId"
              class="card kanban-card shadow-sm p-3 mb-3 border-0 rounded-4 bg-white"
              :class="getSlaClass(card)"
            >
              <div class="d-flex justify-content-between align-items-start mb-2">
                <h6 class="fw-bold text-dark mb-0">
                  {{ getAnimalEmoji(card.species) }} {{ card.petName }}
                  <span v-if="card.isEmergency" class="badge bg-danger ms-1 text-white small" style="font-size: 0.65rem;">
                    <i class="bi bi-exclamation-triangle-fill"></i> CẤP CỨU
                  </span>
                </h6>
                <span class="badge bg-light text-secondary border rounded-pill">{{ formatQueueNumber(card.queueNumber) }}</span>
              </div>
              <div class="text-muted small mb-2"><i class="bi bi-person-circle text-warning me-1"></i>Chủ nuôi: {{ card.customerName || 'Khách vãng lai' }}</div>
              <div v-if="card.symptom" class="bg-light p-2 rounded text-muted small mb-2">{{ card.symptom }}</div>
              
              <!-- SLA Timer Indicator -->
              <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
                <span class="small fw-bold" :class="getSlaTextClass(card)">
                  <i class="bi bi-clock me-1"></i>{{ getWaitingTimeText(card) }}
                </span>
                <span class="small text-muted">Bs. {{ getLastWord(card.doctorName) }}</span>
              </div>

              <!-- Quick Actions -->
              <div class="d-flex gap-2 mt-3 pt-2 border-top">
                <button v-if="isAnonymousEmergency(card)" class="btn btn-sm btn-outline-danger w-100 rounded-pill py-1 fw-bold" @click="openLinkCustomerModal(card)">
                  <i class="bi bi-link-45deg"></i> Ghép Hồ Sơ
                </button>
                <div class="dropdown w-100">
                  <button class="btn btn-sm btn-outline-warning w-100 rounded-pill py-1 dropdown-toggle fw-bold" type="button" data-bs-toggle="dropdown">
                    Thao tác
                  </button>
                  <ul class="dropdown-menu shadow border-0">
                    <li><a class="dropdown-menu-item text-dark p-2 d-block text-decoration-none cursor-pointer" @click="updateStatus(card.appointmentId, 'in_progress')"><i class="bi bi-activity text-warning me-2"></i>Chuyển khám</a></li>
                    <li><a class="dropdown-menu-item text-dark p-2 d-block text-decoration-none cursor-pointer" @click="updateStatus(card.appointmentId, 'ready_to_pay')"><i class="bi bi-cash text-success me-2"></i>Thanh toán</a></li>
                    <li><a class="dropdown-menu-item text-dark p-2 d-block text-decoration-none cursor-pointer" @click="updateStatus(card.appointmentId, 'cancelled')"><i class="bi bi-trash text-danger me-2"></i>Hủy ca</a></li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- In Progress Column -->
      <div class="col-lg-4">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-light h-100 column-in-progress">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h6 class="fw-bold text-warning mb-0 text-uppercase tracking-wider">
              <i class="bi bi-activity me-2"></i> Đang khám
            </h6>
            <span class="badge bg-warning text-dark rounded-pill px-3 shadow-sm">{{ filteredInProgress.length }}</span>
          </div>

          <div class="kanban-list flex-grow-1" style="min-height: 450px;">
            <div v-if="filteredInProgress.length === 0" class="text-center py-5 text-muted">
              <i class="bi bi-stethoscope fs-2 text-muted mb-2 d-block"></i>
              <span class="small">Chưa có ca nào đang thực hiện khám.</span>
            </div>
            
            <div 
              v-for="card in filteredInProgress" 
              :key="card.appointmentId"
              class="card kanban-card shadow-sm p-3 mb-3 border-0 rounded-4 bg-white"
            >
              <div class="d-flex justify-content-between align-items-start mb-2">
                <h6 class="fw-bold text-dark mb-0">
                  {{ getAnimalEmoji(card.species) }} {{ card.petName }}
                  <span v-if="card.isEmergency" class="badge bg-danger ms-1 text-white small" style="font-size: 0.65rem;">
                    <i class="bi bi-exclamation-triangle-fill"></i> CẤP CỨU
                  </span>
                </h6>
                <span class="badge bg-light text-secondary border rounded-pill">{{ formatQueueNumber(card.queueNumber) }}</span>
              </div>
              <div class="text-muted small mb-2"><i class="bi bi-person-circle text-warning me-1"></i>Chủ nuôi: {{ card.customerName || 'Khách vãng lai' }}</div>
              <div v-if="card.symptom" class="bg-light p-2 rounded text-muted small mb-2">{{ card.symptom }}</div>
              
              <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
                <span class="small text-muted"><i class="bi bi-clock me-1"></i>Đang trong phòng khám</span>
                <span class="small fw-bold text-warning">Bs. {{ getLastWord(card.doctorName) }}</span>
              </div>

              <!-- Quick Actions -->
              <div class="d-flex gap-2 mt-3 pt-2 border-top">
                <button v-if="isAnonymousEmergency(card)" class="btn btn-sm btn-outline-danger w-100 rounded-pill py-1 fw-bold" @click="openLinkCustomerModal(card)">
                  <i class="bi bi-link-45deg"></i> Ghép Hồ Sơ
                </button>
                <div class="dropdown w-100">
                  <button class="btn btn-sm btn-outline-warning w-100 rounded-pill py-1 dropdown-toggle fw-bold" type="button" data-bs-toggle="dropdown">
                    Thao tác
                  </button>
                  <ul class="dropdown-menu shadow border-0">
                    <li><a class="dropdown-menu-item text-dark p-2 d-block text-decoration-none cursor-pointer" @click="updateStatus(card.appointmentId, 'waiting')"><i class="bi bi-hourglass-split text-secondary me-2"></i>Trả lại hàng chờ</a></li>
                    <li><a class="dropdown-menu-item text-dark p-2 d-block text-decoration-none cursor-pointer" @click="updateStatus(card.appointmentId, 'ready_to_pay')"><i class="bi bi-cash text-success me-2"></i>Thanh toán</a></li>
                    <li><a class="dropdown-menu-item text-dark p-2 d-block text-decoration-none cursor-pointer" @click="updateStatus(card.appointmentId, 'cancelled')"><i class="bi bi-trash text-danger me-2"></i>Hủy ca</a></li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Ready To Pay Column -->
      <div class="col-lg-4">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-light h-100 column-ready-to-pay">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h6 class="fw-bold text-success mb-0 text-uppercase tracking-wider">
              <i class="bi bi-cash-coin me-2"></i> Chờ thanh toán
            </h6>
            <span class="badge bg-success text-white rounded-pill px-3 shadow-sm">{{ filteredReadyToPay.length }}</span>
          </div>

          <div class="kanban-list flex-grow-1" style="min-height: 450px;">
            <div v-if="filteredReadyToPay.length === 0" class="text-center py-5 text-muted">
              <i class="bi bi-check-circle fs-2 text-muted mb-2 d-block"></i>
              <span class="small">Không có ca khám chờ thanh toán.</span>
            </div>
            
            <div 
              v-for="card in filteredReadyToPay" 
              :key="card.appointmentId"
              class="card kanban-card shadow-sm p-3 mb-3 border-0 rounded-4 bg-white border-top-success"
            >
              <div class="d-flex justify-content-between align-items-start mb-2">
                <h6 class="fw-bold text-dark mb-0">
                  {{ getAnimalEmoji(card.species) }} {{ card.petName }}
                  <span v-if="card.isEmergency" class="badge bg-danger ms-1 text-white small" style="font-size: 0.65rem;">
                    <i class="bi bi-exclamation-triangle-fill"></i> CẤP CỨU
                  </span>
                </h6>
                <span class="badge bg-light text-secondary border rounded-pill">{{ formatQueueNumber(card.queueNumber) }}</span>
              </div>
              <div class="text-muted small mb-2"><i class="bi bi-person-circle text-warning me-1"></i>Chủ nuôi: {{ card.customerName || 'Khách vãng lai' }}</div>
              <div v-if="card.symptom" class="bg-light p-2 rounded text-muted small mb-2">{{ card.symptom }}</div>
              
              <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
                <span class="small text-success fw-bold"><i class="bi bi-currency-dollar me-1"></i>Chờ thu ngân</span>
                <span class="small text-muted">Bs. {{ getLastWord(card.doctorName) }}</span>
              </div>

              <!-- Quick Actions -->
              <div class="d-flex gap-2 mt-3 pt-2 border-top">
                <button v-if="isAnonymousEmergency(card)" class="btn btn-sm btn-outline-danger w-100 rounded-pill py-1 fw-bold" @click="openLinkCustomerModal(card)">
                  <i class="bi bi-link-45deg"></i> Ghép Hồ Sơ
                </button>
                <button class="btn btn-sm btn-premium w-100 rounded-pill py-1 fw-bold" @click="goToInvoiceTab(card.appointmentId)">
                  <i class="bi bi-cash-stack"></i> Thu Tiền
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 1. Walk-in Modal -->
    <div v-if="showWalkInModal" class="zalo-modal-overlay" @click.self="showWalkInModal = false">
      <div class="zalo-modal-card modal-lg max-w-700">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold"><i class="bi bi-person-walking me-2"></i> Tạo Ca Khách Vãng Lai</h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showWalkInModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="submitWalkIn">
            <div class="row g-3">
              <!-- Owner Info -->
              <div class="col-md-6 border-end pe-md-4">
                <h6 class="text-warning fw-bold mb-3 border-bottom pb-2">1. Thông tin Chủ nuôi</h6>
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Số điện thoại *</label>
                  <div class="input-group">
                    <span class="input-group-text bg-light"><i class="bi bi-telephone"></i></span>
                    <input 
                      type="text" 
                      v-model="walkInForm.phone" 
                      @input="searchWalkInCustomer"
                      class="form-control" 
                      required 
                      placeholder="Nhập SĐT khách hàng..."
                    />
                  </div>
                </div>
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Họ và tên *</label>
                  <input type="text" v-model="walkInForm.fullName" class="form-control" required :disabled="isOldCustomerFound" />
                  <div v-if="isOldCustomerFound" class="form-text text-success"><i class="bi bi-check-circle-fill"></i> Nhận dạng khách hàng đã có trong hệ thống</div>
                </div>
              </div>

              <!-- Pet & Doctor info -->
              <div class="col-md-6 ps-md-4">
                <h6 class="text-warning fw-bold mb-3 border-bottom pb-2">2. Bệnh nhi & Dịch vụ</h6>
                
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Chọn thú cưng *</label>
                  <select v-if="isOldCustomerFound && existingPets.length > 0 && !wantsNewPet" v-model="walkInForm.petId" class="form-select border-warning" required>
                    <option value="">-- Chọn thú cưng --</option>
                    <option v-for="pet in existingPets" :key="pet.id" :value="pet.id">{{ pet.name }} ({{ pet.species }})</option>
                  </select>
                  
                  <input v-else type="text" v-model="walkInForm.petName" class="form-control" required placeholder="Nhập tên bé..." />
                  
                  <div v-if="isOldCustomerFound && existingPets.length > 0" class="mt-2">
                    <a href="#" class="small text-warning text-decoration-none fw-bold" @click.prevent="wantsNewPet = !wantsNewPet">
                      {{ wantsNewPet ? '← Chọn thú cưng cũ' : '+ Đăng ký thú cưng mới cho khách này' }}
                    </a>
                  </div>
                </div>

                <div class="row g-2 mb-3" v-if="!isOldCustomerFound || wantsNewPet">
                  <div class="col-6">
                    <label class="form-label text-muted small fw-bold">Giống loài</label>
                    <select v-model="walkInForm.species" class="form-select">
                      <option value="Chó">Chó</option>
                      <option value="Mèo">Mèo</option>
                      <option value="Khác">Khác</option>
                    </select>
                  </div>
                  <div class="col-6">
                    <label class="form-label text-muted small fw-bold">Giới tính</label>
                    <select v-model="walkInForm.gender" class="form-select">
                      <option :value="1">Đực</option>
                      <option :value="2">Cái</option>
                    </select>
                  </div>
                </div>

                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Bác sĩ chỉ định *</label>
                  <select v-model="walkInForm.doctorId" class="form-select" required>
                    <option value="">-- Tự động / Chọn bác sĩ --</option>
                    <option v-for="doc in doctorList" :key="doc.id" :value="doc.id">Bs. {{ doc.fullName }}</option>
                  </select>
                </div>

                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Triệu chứng ban đầu</label>
                  <textarea v-model="walkInForm.symptom" class="form-control" rows="2" placeholder="VD: Sốt nhẹ, bỏ ăn..."></textarea>
                </div>
              </div>
            </div>

            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showWalkInModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-5">Tạo ca khám</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- 2. Triage Emergency Modal -->
    <div v-if="showEmergencyModal" class="zalo-modal-overlay" @click.self="showEmergencyModal = false">
      <div class="zalo-modal-card modal-lg max-w-650">
        <div class="zalo-modal-header bg-danger text-white">
          <h5 class="modal-title fw-bold"><i class="bi bi-heart-pulse-fill text-warning me-2"></i> Bảng Đánh Giá Tri-Age (Cấp Cứu Nhanh)</h5>
          <button class="modal-close text-white border-0 bg-transparent" @click="showEmergencyModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start bg-light">
          <!-- Severity 1 - Critical -->
          <div class="mb-4">
            <h6 class="fw-bold text-danger mb-3"><span class="badge bg-danger me-2">MỨC 1 - NGUY KỊCH</span> Đe dọa tính mạng (Đẩy vào phòng khám ngay)</h6>
            <div class="d-flex flex-wrap gap-2">
              <button 
                type="button" 
                v-for="symptom in level1Symptoms" 
                :key="symptom"
                class="btn rounded-pill fw-bold border-danger py-2 px-3"
                :class="emergencyForm.symptom === symptom ? 'btn-danger text-white' : 'btn-outline-danger bg-white'"
                @click="selectTriage(1, symptom)"
              >
                {{ symptom }}
              </button>
            </div>
          </div>

          <!-- Severity 2 - Urgent -->
          <div class="mb-4">
            <h6 class="fw-bold text-warning-emphasis mb-3"><span class="badge bg-warning text-dark me-2">MỨC 2 - CẤP BÁCH</span> Cần can thiệp sớm (Yêu cầu SĐT tạm thời)</h6>
            <div class="d-flex flex-wrap gap-2">
              <button 
                type="button" 
                v-for="symptom in level2Symptoms" 
                :key="symptom"
                class="btn rounded-pill fw-bold border-warning py-2 px-3"
                :class="emergencyForm.symptom === symptom ? 'btn-warning text-dark' : 'btn-outline-warning text-dark bg-white'"
                @click="selectTriage(2, symptom)"
              >
                {{ symptom }}
              </button>
            </div>
          </div>

          <!-- Quick inputs based on choice -->
          <div v-if="emergencyForm.level > 0" class="card border-0 shadow-sm rounded-4 p-3 bg-white">
            <div class="row g-2">
              <div class="col-md-6" v-if="emergencyForm.level === 2">
                <label class="form-label text-muted small fw-bold">Số điện thoại liên hệ nhanh *</label>
                <input type="text" v-model="emergencyForm.phone" class="form-control border-warning" required placeholder="Nhập SĐT để gọi trả thú cưng..." />
              </div>
              <div class="col-md-6">
                <label class="form-label text-muted small fw-bold">Bác sĩ phụ trách</label>
                <select v-model="emergencyForm.doctorId" class="form-select border-primary">
                  <option value="">-- Tự động phân công bác sĩ --</option>
                  <option v-for="doc in doctorList" :key="doc.id" :value="doc.id">Bs. {{ doc.fullName }}</option>
                </select>
              </div>
            </div>
          </div>

          <div class="mt-4 pt-3 border-top text-end d-flex justify-content-between">
            <button type="button" class="btn btn-light rounded-pill px-4" @click="showEmergencyModal = false">Hủy</button>
            <button 
              type="button" 
              class="btn rounded-pill px-5 fw-bold shadow-sm"
              :class="emergencyForm.level > 0 ? 'btn-danger text-white' : 'btn-secondary text-white'"
              :disabled="emergencyForm.level === 0"
              @click="submitEmergency"
            >
              {{ emergencyForm.level > 0 ? 'Tiếp Nhận Cấp Cứu' : 'Chọn Triệu Chứng để Tiếp Nhận' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 3. Link Customer (Ghép hồ sơ) Modal -->
    <div v-if="showLinkCustomerModal" class="zalo-modal-overlay" @click.self="showLinkCustomerModal = false">
      <div class="zalo-modal-card max-w-450">
        <div class="zalo-modal-header bg-primary text-white">
          <h5 class="modal-title fw-bold"><i class="bi bi-person-plus-fill me-2"></i> Ghép Nối Hồ Sơ Chủ Nuôi</h5>
          <button class="modal-close text-white border-0 bg-transparent" @click="showLinkCustomerModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <div class="mb-3">
            <label class="form-label text-muted small fw-bold">Số điện thoại chủ nuôi *</label>
            <div class="input-group">
              <span class="input-group-text bg-light"><i class="bi bi-telephone"></i></span>
              <input 
                type="text" 
                v-model="linkForm.phone" 
                @input="searchLinkCustomer"
                class="form-control" 
                placeholder="Nhập SĐT để tìm hồ sơ..."
              />
            </div>
          </div>

          <div v-if="linkCustomerFound" class="bg-light p-3 rounded-4 mb-3 border">
            <div class="mb-2">
              <label class="form-label text-muted small d-block mb-1">Tên chủ nuôi:</label>
              <strong>{{ linkCustomerData.fullName }}</strong>
            </div>
            <div class="mb-0">
              <label class="form-label text-muted small d-block mb-1">Chọn thú cưng ghép nối *</label>
              <select v-model="linkForm.petId" class="form-select border-primary" required>
                <option value="">-- Chọn thú cưng --</option>
                <option v-for="pet in linkCustomerPets" :key="pet.id" :value="pet.id">{{ pet.name }} ({{ pet.species }})</option>
              </select>
            </div>
          </div>

          <div v-if="linkForm.phone && !linkCustomerFound" class="alert alert-warning small">
            Không tìm thấy SĐT trong hệ thống. Vui lòng tạo hồ sơ cho khách trước khi ghép nối!
          </div>

          <div class="mt-4 pt-3 border-top text-end">
            <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showLinkCustomerModal = false">Hủy</button>
            <button type="button" class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" :disabled="!linkForm.petId" @click="submitLinkCustomer">
              Cập nhật hồ sơ
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 4. QR Checkin Dialog -->
    <div v-if="showQrModal" class="zalo-modal-overlay" @click.self="showQrModal = false">
      <div class="zalo-modal-card max-w-400">
        <div class="zalo-modal-header bg-success text-white">
          <h5 class="modal-title fw-bold"><i class="bi bi-qr-code-scan me-2"></i> Quét Mã Check-in</h5>
          <button class="modal-close text-white border-0 bg-transparent" @click="showQrModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-center">
          <p class="text-muted small mb-3">Nhập mã check-in / UserId của khách hàng hoặc đưa mã QR của khách vào trước camera để quét tự động.</p>
          
          <div class="mb-3">
            <input 
              type="text" 
              v-model="qrManualCode" 
              class="form-control text-center fw-bold fs-5 border-success rounded-pill" 
              placeholder="Nhập mã check-in..."
              @keyup.enter="submitCheckInByCode"
            />
          </div>

          <button class="btn btn-success w-100 rounded-pill py-2.5 fw-bold shadow-sm" @click="submitCheckInByCode">
            Xác Nhận Check-in
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import api from '../../services/api';

const emit = defineEmits(['switch-tab', 'select-invoice']);

// State
const searchQuery = ref('');
const searchResult = ref<any[]>([]);
const selectedDoctor = ref('ALL');
const doctorList = ref<any[]>([]);
const queueList = ref<any[]>([]);
const intervals = ref<any[]>([]);

// Modals state
const showWalkInModal = ref(false);
const showEmergencyModal = ref(false);
const showLinkCustomerModal = ref(false);
const showQrModal = ref(false);

// Forms
const walkInForm = ref({
  phone: '',
  fullName: '',
  petName: '',
  petId: '',
  species: 'Chó',
  gender: 1,
  doctorId: '',
  symptom: '',
});
const isOldCustomerFound = ref(false);
const wantsNewPet = ref(false);
const existingPets = ref<any[]>([]);

const level1Symptoms = ['Khó thở / Tím tái', 'Co giật liên tục', 'Mất máu ồ ạt', 'Sốc / Bất tỉnh'];
const level2Symptoms = ['Gãy xương / Đa chấn thương', 'Nôn mửa / Tiêu chảy cấp', 'Nuốt dị vật', 'Ngộ độc'];

const emergencyForm = ref({
  level: 0,
  symptom: '',
  phone: '',
  doctorId: ''
});

const linkForm = ref({
  appointmentId: 0,
  phone: '',
  petId: ''
});
const linkCustomerFound = ref(false);
const linkCustomerData = ref<any>({});
const linkCustomerPets = ref<any[]>([]);

const qrManualCode = ref('');

// Computed Lists
const filteredQueue = computed(() => {
  if (selectedDoctor.value === 'ALL') return queueList.value;
  return queueList.value.filter(item => item.doctorId === selectedDoctor.value);
});

const filteredWaiting = computed(() => {
  return filteredQueue.value.filter(item => item.status === 'waiting');
});

const filteredInProgress = computed(() => {
  return filteredQueue.value.filter(item => item.status === 'in_progress');
});

const filteredReadyToPay = computed(() => {
  return filteredQueue.value.filter(item => item.status === 'ready_to_pay');
});

// Timers / Time helper
const nowRef = ref(new Date());
let timeUpdater: any = null;

const getWaitingTimeText = (card: any) => {
  if (!card.createdAt) return '0 phút';
  const createdTime = new Date(card.createdAt);
  const diffMs = nowRef.value.getTime() - createdTime.getTime();
  const diffMins = Math.max(0, Math.floor(diffMs / 60000));
  return `${diffMins} phút`;
};

const getSlaClass = (card: any) => {
  if (card.status !== 'waiting') return '';
  if (!card.createdAt) return '';
  const createdTime = new Date(card.createdAt);
  const diffMins = Math.floor((nowRef.value.getTime() - createdTime.getTime()) / 60000);
  if (diffMins >= 30) return 'sla-danger';
  if (diffMins >= 15) return 'sla-warning';
  return '';
};

const getSlaTextClass = (card: any) => {
  if (card.status !== 'waiting') return 'text-muted';
  if (!card.createdAt) return 'text-muted';
  const createdTime = new Date(card.createdAt);
  const diffMins = Math.floor((nowRef.value.getTime() - createdTime.getTime()) / 60000);
  if (diffMins >= 30) return 'text-danger';
  if (diffMins >= 15) return 'text-warning';
  return 'text-success';
};

// Animal Emojis
const getAnimalEmoji = (species: string) => {
  const s = (species || '').toLowerCase();
  if (s.includes('chó') || s.includes('dog')) return '🐶';
  if (s.includes('mèo') || s.includes('cat')) return '🐱';
  return '🐾';
};

const formatQueueNumber = (num: number | string) => {
  if (!num) return 'Q-000';
  const n = parseInt(num.toString(), 10);
  if (isNaN(n)) return num;
  return `Q-${String(n).padStart(3, '0')}`;
};

const getLastWord = (name: string) => {
  if (!name) return '—';
  const parts = name.trim().split(/\s+/);
  return parts[parts.length - 1];
};

const isAnonymousEmergency = (card: any) => {
  return card.isEmergency && (!card.customerId || card.customerName?.includes('ẩn danh'));
};

// API calls
const loadQueue = async () => {
  try {
    const res = await api.get('/receptionist/queue');
    queueList.value = res.data;
  } catch (err) {
    console.error('Lỗi tải danh sách hàng khám:', err);
  }
};

const loadDoctors = async () => {
  try {
    const res = await api.get('/receptionist/doctors');
    doctorList.value = res.data;
  } catch (err) {
    console.error('Lỗi tải danh sách bác sĩ:', err);
  }
};

const updateStatus = async (appointmentId: number, status: string) => {
  try {
    const res = await api.put(`/receptionist/queue/${appointmentId}/status`, { status });
    if (res.data.success) {
      await loadQueue();
    }
  } catch (err) {
    alert('Không thể cập nhật trạng thái');
    console.error(err);
  }
};

// Search actions
const handleOmniSearch = async () => {
  const q = searchQuery.value.trim();
  if (q.length < 2) {
    searchResult.value = [];
    return;
  }
  try {
    const res = await api.get(`/receptionist/omni-search?q=${encodeURIComponent(q)}`);
    searchResult.value = res.data || [];
  } catch (err) {
    console.error(err);
  }
};

const selectSearchResult = (item: any) => {
  searchQuery.value = '';
  searchResult.value = [];
  
  // Prefill check-in or quick actions
  if (item.customerId) {
    openWalkInModal();
    walkInForm.value.phone = item.phone || '';
    searchWalkInCustomer();
  }
};

// Customer phone search in Walk-in
let debounceTimer: any = null;
const searchWalkInCustomer = () => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(async () => {
    const phone = walkInForm.value.phone.trim();
    if (!phone) return;
    try {
      const res = await api.get(`/receptionist/customer-by-phone?phone=${encodeURIComponent(phone)}`);
      if (res.data.success) {
        isOldCustomerFound.value = true;
        walkInForm.value.fullName = res.data.customer.fullName;
        existingPets.value = res.data.pets || [];
        wantsNewPet.value = false;
      } else {
        isOldCustomerFound.value = false;
        existingPets.value = [];
      }
    } catch (err) {
      console.error(err);
    }
  }, 400);
};

// Link customer to anonymous emergency case
let linkDebounce: any = null;
const searchLinkCustomer = () => {
  clearTimeout(linkDebounce);
  linkDebounce = setTimeout(async () => {
    const phone = linkForm.value.phone.trim();
    if (!phone) return;
    try {
      const res = await api.get(`/receptionist/customer-by-phone?phone=${encodeURIComponent(phone)}`);
      if (res.data.success) {
        linkCustomerFound.value = true;
        linkCustomerData.value = res.data.customer;
        linkCustomerPets.value = res.data.pets || [];
      } else {
        linkCustomerFound.value = false;
        linkCustomerPets.value = [];
      }
    } catch (err) {
      console.error(err);
    }
  }, 400);
};

// Submit Walk-in
const submitWalkIn = async () => {
  try {
    const reqBody: any = {
      phone: walkInForm.value.phone,
      fullName: walkInForm.value.fullName,
      symptom: walkInForm.value.symptom,
      doctorId: walkInForm.value.doctorId || null,
      isEmergency: false,
      serviceId: 1 // default service
    };

    if (isOldCustomerFound.value && !wantsNewPet.value) {
      reqBody.petName = existingPets.value.find(p => p.id === walkInForm.value.petId)?.name || 'Chưa chọn';
    } else {
      reqBody.petName = walkInForm.value.petName;
      reqBody.species = walkInForm.value.species;
      reqBody.gender = walkInForm.value.gender;
    }

    const res = await api.post('/receptionist/walk-in', reqBody);
    if (res.data.success) {
      alert('Đã tạo ca tiếp nhận vãng lai thành công!');
      showWalkInModal.value = false;
      await loadQueue();
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi tạo ca khám.');
  }
};

// Triage actions
const selectTriage = (level: number, symptom: string) => {
  emergencyForm.value.level = level;
  emergencyForm.value.symptom = symptom;
  if (level === 1) {
    emergencyForm.value.phone = '0999999999'; // Admin emergency placeholder
  }
};

const submitEmergency = async () => {
  try {
    const reqBody = {
      phone: emergencyForm.value.phone || '0999999999',
      fullName: `Cấp cứu ẩn danh Mức ${emergencyForm.value.level}`,
      petName: 'Bệnh nhi cấp cứu',
      species: 'Khác',
      symptom: emergencyForm.value.symptom,
      doctorId: emergencyForm.value.doctorId || null,
      isEmergency: true,
      serviceId: 1
    };

    const res = await api.post('/receptionist/walk-in', reqBody);
    if (res.data.success) {
      alert('Tiếp nhận cấp cứu thành công. Bệnh nhi được xếp ưu tiên khám ngay.');
      showEmergencyModal.value = false;
      await loadQueue();
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra');
  }
};

// Link anonymous customer
const openLinkCustomerModal = (card: any) => {
  linkForm.value.appointmentId = card.appointmentId;
  linkForm.value.phone = '';
  linkForm.value.petId = '';
  linkCustomerFound.value = false;
  linkCustomerPets.value = [];
  showLinkCustomerModal.value = true;
};

const submitLinkCustomer = async () => {
  try {
    const res = await api.put(`/receptionist/emergency/${linkForm.value.appointmentId}/customer`, {
      customerId: linkCustomerData.value.id,
      petId: linkForm.value.petId
    });
    if (res.data.success) {
      alert('Ghép nối hồ sơ thành công!');
      showLinkCustomerModal.value = false;
      await loadQueue();
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi ghép nối');
  }
};

// QR actions
const submitCheckInByCode = async () => {
  const token = qrManualCode.value.trim();
  if (!token) return;
  try {
    const res = await api.post('/receptionist/check-in', {
      qrToken: token,
      isEmergency: false
    });
    if (res.data.success) {
      alert(res.data.message);
      showQrModal.value = false;
      qrManualCode.value = '';
      await loadQueue();
    } else {
      alert(res.data.message);
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Không thể check-in. Vui lòng kiểm tra lại mã.');
  }
};

// Nav Actions
const goToInvoiceTab = (appointmentId: number) => {
  emit('select-invoice', appointmentId);
  emit('switch-tab', 'invoices');
};

const openQrScanModal = () => {
  qrManualCode.value = '';
  showQrModal.value = true;
};

const openEmergencyModal = () => {
  emergencyForm.value.level = 0;
  emergencyForm.value.symptom = '';
  emergencyForm.value.phone = '';
  emergencyForm.value.doctorId = '';
  showEmergencyModal.value = true;
};

const openWalkInModal = () => {
  walkInForm.value = {
    phone: '',
    fullName: '',
    petName: '',
    petId: '',
    species: 'Chó',
    gender: 1,
    doctorId: '',
    symptom: '',
  };
  isOldCustomerFound.value = false;
  wantsNewPet.value = false;
  existingPets.value = [];
  showWalkInModal.value = true;
};

// Mount/Unmount hooks
onMounted(() => {
  loadQueue();
  loadDoctors();

  // Poll server for updates every 15 seconds
  const queueInterval = setInterval(loadQueue, 15000);
  intervals.value.push(queueInterval);

  // SLA clock ticks every 10 seconds
  timeUpdater = setInterval(() => {
    nowRef.value = new Date();
  }, 10000);
});

onUnmounted(() => {
  intervals.value.forEach(clearInterval);
  if (timeUpdater) clearInterval(timeUpdater);
});
</script>

<style scoped>
.search-dropdown {
  left: 0;
  right: 0;
  background-color: white;
  border-radius: 12px;
  border: 1px solid var(--border-color);
  box-shadow: var(--shadow-lg);
}
.hover-bg-light:hover {
  background-color: #fdfaf0;
}
.cursor-pointer {
  cursor: pointer;
}
.kanban-container {
  display: flex;
  align-items: stretch;
}
.kanban-list {
  background: transparent;
  border-radius: var(--radius-md);
  padding: 5px 0;
}
.kanban-card {
  transition: var(--transition-smooth);
  border: 1px solid rgba(0, 0, 0, 0.03) !important;
}
.kanban-card:hover {
  transform: translateY(-3px);
  box-shadow: var(--shadow-md) !important;
}

/* SLA Warning styles matching Razor site.css */
.sla-warning {
  border: 2px solid #f59e0b !important;
  animation: pulse-orange 2s infinite;
}
.sla-danger {
  border: 2px solid #ef4444 !important;
  animation: pulse-red 1.5s infinite;
}

@keyframes pulse-orange {
  0% { box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.2); }
  70% { box-shadow: 0 0 0 8px rgba(245, 158, 11, 0); }
  100% { box-shadow: 0 0 0 0 rgba(245, 158, 11, 0); }
}
@keyframes pulse-red {
  0% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.2); }
  70% { box-shadow: 0 0 0 8px rgba(239, 68, 68, 0); }
  100% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
}

.border-top-success {
  border-top: 4px solid #14b8a6 !important;
}

/* Triage selection classes */
.btn-triage {
  transition: all 0.2s;
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

.max-w-400 { max-width: 400px; }
.max-w-450 { max-width: 450px; }
.max-w-650 { max-width: 650px; }
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

.tracking-wider {
  letter-spacing: 0.05em;
}
</style>
