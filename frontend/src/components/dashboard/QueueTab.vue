<template>
  <div class="queue-tab container-fluid p-0">
    <!-- Smart Intake Flow -->
    <div class="row mb-4">
      <div class="col-12">
        <div class="card border-0 shadow-sm rounded-4 p-4 bg-gold-gradient position-relative overflow-visible">
          <!-- Main Search Input -->
          <div class="d-flex flex-wrap align-items-center justify-content-between gap-3 position-relative z-index-1">
            <div class="flex-grow-1">
              <label class="form-label fw-bold text-dark mb-2"><i class="bi bi-telephone-fill me-1 text-warning"></i>Tiếp Nhận Thông Minh (Smart Intake)</label>
              <div class="input-group input-group-lg shadow-sm">
                <span class="input-group-text bg-white border-end-0 rounded-start-pill"><i class="bi bi-search text-muted"></i></span>
                <input 
                  type="text" 
                  v-model="intakePhone" 
                  @input="handleIntakeSearch"
                  class="form-control border-start-0 rounded-end-pill fs-5" 
                  placeholder="Nhập số điện thoại khách hàng (10 số)..." 
                  maxlength="15"
                />
              </div>
            </div>
            <div class="d-flex flex-wrap gap-2 mt-4">
              <button class="btn btn-premium px-4 py-2 rounded-pill shadow-sm" @click="openQrScanModal">
                <i class="bi bi-qr-code-scan"></i> Quét QR
              </button>
            </div>
          </div>

          <!-- Slide Down Panel: Searching -->
          <div v-if="intakeState === 'searching'" class="mt-3 p-3 bg-white rounded-4 shadow-sm text-center">
            <div class="spinner-border text-warning spinner-border-sm me-2" role="status"></div>
            <span class="text-muted fw-bold">Đang tra cứu dữ liệu...</span>
          </div>

          <!-- Slide Down Panel: Found Customer -->
          <div v-if="intakeState === 'found'" class="mt-4 p-4 bg-white rounded-4 shadow slide-down-animation border-top-premium">
            <div class="d-flex justify-content-between align-items-center mb-3 border-bottom pb-2">
              <h5 class="fw-bold text-dark mb-0"><i class="bi bi-person-check-fill text-success me-2"></i>Hồ sơ Khách Hàng Cũ</h5>
              <button class="btn btn-sm btn-light rounded-pill" @click="resetIntake"><i class="bi bi-x-lg"></i> Đóng</button>
            </div>
            <div class="row g-4">
              <!-- Cột 1: Thông tin khách -->
              <div class="col-md-4 border-end">
                <div class="d-flex align-items-center mb-3">
                  <div class="bg-light p-3 rounded-circle me-3">
                    <i class="bi bi-person fs-3 text-warning"></i>
                  </div>
                  <div>
                    <h6 class="fw-bold mb-1 fs-5">{{ intakeCustomer.fullName }}</h6>
                    <span class="text-muted"><i class="bi bi-telephone me-1"></i>{{ intakeCustomer.phone }}</span>
                  </div>
                </div>
                <div class="mt-3">
                  <button class="btn btn-outline-warning btn-sm rounded-pill fw-bold w-100 mb-2" @click="intakeWantsNewPet = !intakeWantsNewPet">
                    {{ intakeWantsNewPet ? 'Hủy thêm thú cưng' : '+ Thêm thú cưng mới' }}
                  </button>
                </div>
              </div>

              <!-- Cột 2: Chọn thú cưng & Triệu chứng -->
              <div class="col-md-8">
                <!-- List existing pets -->
                <div v-if="!intakeWantsNewPet" class="mb-3">
                  <label class="form-label text-muted small fw-bold d-block">Chọn thú cưng cần khám *</label>
                  <div class="d-flex flex-wrap gap-2">
                    <button 
                      v-for="pet in intakePets" :key="pet.id"
                      class="btn rounded-pill px-4 py-2 fw-bold"
                      :class="intakeForm.petId === pet.id ? 'btn-warning text-dark shadow-sm' : 'btn-outline-secondary'"
                      @click="intakeForm.petId = pet.id"
                    >
                      {{ getAnimalEmoji(pet.species) }} {{ pet.name }}
                    </button>
                  </div>
                  <div v-if="!intakeForm.petId" class="text-danger small mt-2"><i class="bi bi-exclamation-circle"></i> Vui lòng chọn một thú cưng.</div>
                </div>

                <!-- Add new pet form -->
                <div v-if="intakeWantsNewPet" class="mb-3 p-3 bg-light rounded-4 border border-warning">
                  <h6 class="fw-bold text-warning mb-2">Đăng ký Thú cưng mới</h6>
                  <div class="row g-2">
                    <div class="col-md-6">
                      <input type="text" v-model="intakeForm.petName" class="form-control" placeholder="Tên bé..." />
                    </div>
                    <div class="col-md-3">
                      <select v-model="intakeForm.species" class="form-select">
                        <option value="Chó">Chó</option>
                        <option value="Mèo">Mèo</option>
                        <option value="Khác">Khác</option>
                      </select>
                    </div>
                    <div class="col-md-3">
                      <select v-model="intakeForm.gender" class="form-select">
                        <option :value="1">Đực</option>
                        <option :value="2">Cái</option>
                      </select>
                    </div>
                  </div>
                </div>

                <!-- Info for appointment -->
                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label text-muted small fw-bold">Bác sĩ chỉ định (Tùy chọn)</label>
                    <select v-model="intakeForm.doctorId" class="form-select">
                      <option value="">-- Tự động phân bổ --</option>
                      <option v-for="doc in doctorList" :key="doc.id" :value="doc.id">Bs. {{ doc.fullName }}</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label text-muted small fw-bold">Triệu chứng (Ghi chú)</label>
                    <input type="text" v-model="intakeForm.symptom" class="form-control" placeholder="Sốt, bỏ ăn..." @keyup.enter="submitIntake" />
                  </div>
                </div>
                
                <div class="mt-4 text-end border-top pt-3">
                  <button class="btn btn-premium rounded-pill px-5 py-2 fw-bold fs-6" :disabled="!intakeWantsNewPet && !intakeForm.petId" @click="submitIntake">
                    <i class="bi bi-calendar-check me-2"></i>Tạo Ca Khám
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Slide Down Panel: Not Found (New Customer) -->
          <div v-if="intakeState === 'not_found'" class="mt-4 p-4 bg-white rounded-4 shadow slide-down-animation border-top-warning">
            <div class="d-flex justify-content-between align-items-center mb-3 border-bottom pb-2">
              <h5 class="fw-bold text-dark mb-0"><i class="bi bi-person-plus-fill text-warning me-2"></i>Đăng ký Khách Hàng Mới</h5>
              <button class="btn btn-sm btn-light rounded-pill" @click="resetIntake"><i class="bi bi-x-lg"></i> Đóng</button>
            </div>
            
            <div class="row g-4">
              <!-- Cột 1: Thông tin chủ -->
              <div class="col-md-5 border-end">
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Số điện thoại *</label>
                  <input type="text" :value="intakePhone" class="form-control bg-light" readonly />
                </div>
                <div class="mb-3">
                  <label class="form-label text-muted small fw-bold">Họ và tên Chủ Nuôi *</label>
                  <input type="text" v-model="intakeForm.fullName" class="form-control border-warning" placeholder="Nhập họ tên..." autofocus />
                </div>
              </div>

              <!-- Cột 2: Thông tin thú cưng & Khám -->
              <div class="col-md-7">
                <h6 class="fw-bold text-dark mb-3">Thông tin Bệnh nhi</h6>
                <div class="row g-2 mb-3">
                  <div class="col-md-6">
                    <input type="text" v-model="intakeForm.petName" class="form-control" placeholder="Tên bé..." />
                  </div>
                  <div class="col-md-3">
                    <select v-model="intakeForm.species" class="form-select">
                      <option value="Chó">Chó</option>
                      <option value="Mèo">Mèo</option>
                      <option value="Khác">Khác</option>
                    </select>
                  </div>
                  <div class="col-md-3">
                    <select v-model="intakeForm.gender" class="form-select">
                      <option :value="1">Đực</option>
                      <option :value="2">Cái</option>
                    </select>
                  </div>
                </div>

                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label text-muted small fw-bold">Bác sĩ</label>
                    <select v-model="intakeForm.doctorId" class="form-select">
                      <option value="">-- Tự động --</option>
                      <option v-for="doc in doctorList" :key="doc.id" :value="doc.id">Bs. {{ doc.fullName }}</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label text-muted small fw-bold">Triệu chứng</label>
                    <input type="text" v-model="intakeForm.symptom" class="form-control" placeholder="Sốt, bỏ ăn..." @keyup.enter="submitIntake" />
                  </div>
                </div>

                <div class="mt-4 text-end border-top pt-3">
                  <button class="btn btn-warning rounded-pill px-5 py-2 fw-bold text-dark fs-6 shadow-sm" :disabled="!intakeForm.fullName || !intakeForm.petName" @click="submitIntake">
                    <i class="bi bi-calendar-check me-2"></i>Lưu & Tiếp Nhận
                  </button>
                </div>
              </div>
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
    <div v-if="showQrModal" class="zalo-modal-overlay d-flex align-items-center justify-content-center" @click.self="closeQrModal">
      <div class="zalo-modal-card border-0 shadow-lg" style="max-width: 420px; width: 100%; margin: 0 auto; border-radius: 16px; overflow: hidden;">
        <div class="zalo-modal-header bg-success bg-gradient text-white p-3 border-0 d-flex justify-content-between align-items-center">
          <h5 class="modal-title fw-bold mb-0"><i class="bi bi-qr-code-scan me-2"></i> Quét Mã Check-in</h5>
          <button class="modal-close text-white border-0 bg-transparent ms-auto" @click="closeQrModal"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body p-4 bg-white">
          
          <div v-if="previewAppointment">
            <!-- Preview Card -->
            <div class="text-center mb-4">
              <div :class="['rounded-circle d-inline-flex align-items-center justify-content-center mb-3', previewAppointment.hasError ? 'bg-danger bg-opacity-10' : 'bg-success bg-opacity-10']" style="width: 70px; height: 70px;">
                <i :class="['bi fs-1', previewAppointment.hasError ? 'bi-exclamation-triangle-fill text-danger' : 'bi-check-circle-fill text-success']"></i>
              </div>
              <h5 :class="['fw-bold mb-1', previewAppointment.hasError ? 'text-danger' : 'text-success']">
                {{ previewAppointment.hasError ? 'Mã Không Hợp Lệ!' : 'Mã Hợp Lệ!' }}
              </h5>
              <p class="text-muted small">
                {{ previewAppointment.hasError ? 'Không thể check-in lúc này.' : 'Vui lòng xác nhận thông tin trước khi đưa vào hàng đợi' }}
              </p>
            </div>

            <div v-if="previewAppointment.hasError" class="alert alert-danger border-danger border-opacity-25 rounded-3 mb-4 text-start">
              <i class="bi bi-info-circle-fill me-2"></i> <strong>Lưu ý:</strong> {{ previewAppointment.errorMessage }}
            </div>

            <div v-if="previewAppointment.appointmentId !== 0" class="card border-0 bg-light rounded-4 mb-4">
              <div class="card-body p-3">
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted small">Thời gian hẹn:</span>
                  <span class="fw-bold text-dark">{{ formatTimeOnly(previewAppointment.appointmentDate) }} - {{ formatDate(previewAppointment.appointmentDate) }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted small">Khách hàng:</span>
                  <span class="fw-bold text-dark">{{ previewAppointment.customerName }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted small">Thú cưng:</span>
                  <span class="fw-bold text-dark">{{ previewAppointment.petName }} <span v-if="previewAppointment.petSpecies">({{ previewAppointment.petSpecies }})</span></span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span class="text-muted small">Bác sĩ:</span>
                  <span class="fw-bold text-dark">{{ previewAppointment.doctorName || 'Tự động xếp' }}</span>
                </div>
                <div class="d-flex justify-content-between">
                  <span class="text-muted small">Dịch vụ:</span>
                  <span class="fw-bold text-dark">{{ previewAppointment.serviceName || 'Khám bệnh' }}</span>
                </div>
              </div>
            </div>

            <div class="d-flex gap-2">
              <button class="btn btn-light w-50 rounded-pill py-2.5 fw-bold" @click="cancelPreview">Hủy quét</button>
              <button v-if="!previewAppointment.hasError" class="btn btn-success w-50 rounded-pill py-2.5 fw-bold shadow-sm" @click="confirmCheckIn">Vào Hàng Đợi <i class="bi bi-arrow-right ms-1"></i></button>
              <button v-else class="btn btn-danger w-50 rounded-pill py-2.5 fw-bold shadow-sm" @click="closeQrModal">Đóng</button>
            </div>
          </div>

          <div v-else class="text-center">
            <!-- Camera Scanner Area -->
            <div id="qr-reader" class="mb-3 rounded-4 overflow-hidden border border-success border-opacity-25" style="width: 100%; min-height: 250px; background: #f8f9fa;"></div>
            
            <p class="text-muted small mb-3">Đưa mã QR của khách vào khung hình để quét tự động, hoặc nhập tay mã check-in bên dưới.</p>
            
            <div class="mb-4">
              <input 
                type="text" 
                v-model="qrManualCode" 
                class="form-control text-center fw-bold fs-4 border-success border-2 rounded-3 py-2" 
                style="letter-spacing: 2px;"
                placeholder="Nhập mã..."
                @keyup.enter="previewCheckIn"
              />
            </div>

            <button class="btn btn-success w-100 rounded-pill py-2.5 fw-bold shadow-sm" @click="previewCheckIn">
              Kiểm Tra Mã
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue';
import api from '../../services/api';
import { Html5Qrcode } from 'html5-qrcode';

const emit = defineEmits(['switch-tab', 'select-invoice']);

// State
const intakePhone = ref('');
const intakeState = ref<'idle'|'searching'|'found'|'not_found'>('idle');
const intakeCustomer = ref<any>(null);
const intakePets = ref<any[]>([]);
const intakeWantsNewPet = ref(false);
const intakeForm = ref({
  fullName: '',
  petId: '',
  petName: '',
  species: 'Chó',
  gender: 1,
  doctorId: '',
  symptom: ''
});

const selectedDoctor = ref('ALL');
const doctorList = ref<any[]>([]);
const queueList = ref<any[]>([]);
const intervals = ref<any[]>([]);

// Modals state
const showEmergencyModal = ref(false);
const showLinkCustomerModal = ref(false);
const showQrModal = ref(false);
const previewAppointment = ref<any>(null);

let html5QrCode: Html5Qrcode | null = null;

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

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatTimeOnly = (dateStr: string) => {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
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

// Smart Intake logic
let intakeDebounce: any = null;
const handleIntakeSearch = () => {
  const phone = intakePhone.value.trim();
  if (phone.length < 10) {
    intakeState.value = 'idle';
    return;
  }
  
  intakeState.value = 'searching';
  clearTimeout(intakeDebounce);
  intakeDebounce = setTimeout(async () => {
    try {
      const res = await api.get(`/receptionist/customer-by-phone?phone=${encodeURIComponent(phone)}`);
      if (res.data.success) {
        intakeState.value = 'found';
        intakeCustomer.value = res.data.customer;
        intakePets.value = res.data.pets || [];
        intakeWantsNewPet.value = false;
        
        // Reset form
        intakeForm.value.fullName = res.data.customer.fullName;
        intakeForm.value.petId = '';
        intakeForm.value.petName = '';
        intakeForm.value.symptom = '';
        intakeForm.value.doctorId = '';
      } else {
        intakeState.value = 'not_found';
        // Reset form
        intakeForm.value.fullName = '';
        intakeForm.value.petId = '';
        intakeForm.value.petName = '';
        intakeForm.value.symptom = '';
        intakeForm.value.doctorId = '';
      }
    } catch (err) {
      console.error(err);
      intakeState.value = 'idle';
    }
  }, 500);
};

const resetIntake = () => {
  intakePhone.value = '';
  intakeState.value = 'idle';
};

const submitIntake = async () => {
  try {
    const reqBody: any = {
      phone: intakePhone.value,
      fullName: intakeForm.value.fullName,
      symptom: intakeForm.value.symptom,
      doctorId: intakeForm.value.doctorId || null,
      isEmergency: false,
      serviceId: 1
    };

    if (intakeState.value === 'found' && !intakeWantsNewPet.value) {
      if (!intakeForm.value.petId) {
        alert("Vui lòng chọn thú cưng!");
        return;
      }
      reqBody.petName = intakePets.value.find(p => p.id === intakeForm.value.petId)?.name || 'Chưa chọn';
    } else {
      if (!intakeForm.value.petName) {
        alert("Vui lòng nhập tên thú cưng!");
        return;
      }
      reqBody.petName = intakeForm.value.petName;
      reqBody.species = intakeForm.value.species;
      reqBody.gender = intakeForm.value.gender;
    }

    const res = await api.post('/receptionist/walk-in', reqBody);
    if (res.data.success) {
      // alert('Tiếp nhận ca khám thành công!');
      resetIntake();
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

// QR actions

const startScanner = async () => {
  try {
    await nextTick();
    html5QrCode = new Html5Qrcode("qr-reader");
    await html5QrCode.start(
      { facingMode: "environment" },
      { fps: 30, qrbox: { width: 300, height: 300 } },
      (decodedText) => {
        qrManualCode.value = decodedText;
        previewCheckIn();
      },
      (errorMessage) => {
        // parse error, ignore
      }
    );
  } catch (err) {
    console.error("Lỗi khởi động camera:", err);
  }
};

const stopScanner = async () => {
  if (html5QrCode) {
    try {
      if (html5QrCode.isScanning) {
        await html5QrCode.stop();
      }
      html5QrCode.clear();
      html5QrCode = null;
    } catch (e) {
      console.error("Lỗi dừng camera:", e);
    }
  }
};

const closeQrModal = () => {
  showQrModal.value = false;
  previewAppointment.value = null;
  stopScanner();
};

const previewCheckIn = async () => {
  const token = qrManualCode.value.trim();
  if (!token) return;
  try {
    await stopScanner();
    const res = await api.get(`/receptionist/appointment-preview?qrToken=${token}`);
    if (res.data.success) {
      previewAppointment.value = res.data.data;
    } else {
      alert(res.data.message);
      startScanner();
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Không thể kiểm tra mã. Vui lòng thử lại.');
    startScanner();
  }
};

const cancelPreview = () => {
  previewAppointment.value = null;
  qrManualCode.value = '';
  startScanner();
};

const confirmCheckIn = async () => {
  if (!previewAppointment.value) return;
  try {
    const res = await api.post('/receptionist/check-in', {
      qrToken: previewAppointment.value.qrToken,
      isEmergency: false
    });
    if (res.data.success) {
      alert(res.data.message);
      showQrModal.value = false;
      previewAppointment.value = null;
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
  previewAppointment.value = null;
  showQrModal.value = true;
  startScanner();
};

const openEmergencyModal = () => {
  emergencyForm.value.level = 0;
  emergencyForm.value.symptom = '';
  emergencyForm.value.phone = '';
  emergencyForm.value.doctorId = '';
  showEmergencyModal.value = true;
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

.slide-down-animation {
  animation: slideDown 0.3s ease-out forwards;
  transform-origin: top;
}
@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}
.border-top-premium { border-top: 4px solid var(--primary-color) !important; }
.border-top-warning { border-top: 4px solid #f59e0b !important; }

</style>
