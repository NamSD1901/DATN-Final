<template>
  <div class="queue-tab container-fluid p-0">
    <!-- Smart Intake Flow has been removed -->

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
          <button class="btn btn-light border rounded-circle p-2 shadow-sm" @click="manualRefresh" title="Làm mới" :disabled="isRefreshing">
            <i v-if="!isRefreshing" class="bi bi-arrow-clockwise"></i>
            <span v-else class="spinner-border spinner-border-sm text-secondary"></span>
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

          <div class="kanban-list flex-grow-1" style="min-height: 450px;" v-auto-animate>
            <div v-if="filteredWaiting.length === 0" class="text-center py-5 text-muted">
              <i class="bi bi-emoji-smile fs-2 text-muted mb-2 d-block"></i>
              <span class="small">Không có bệnh nhi nào đang chờ.</span>
            </div>
            
            <div 
              v-for="(group, gIdx) in groupedWaiting" 
              :key="'waiting-' + gIdx"
              class="card shadow-sm mb-2 border-0 rounded-4 overflow-hidden kanban-group-card"
            >
              <div class="px-3 py-2 d-flex justify-content-between align-items-center" style="background: linear-gradient(to right, #f8fafc, #ffffff); border-bottom: 1px dashed #e2e8f0;">
                <div class="fw-bold text-dark d-flex align-items-center" style="font-size: 0.95rem;">
                   <div class="bg-secondary bg-opacity-10 text-secondary rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 28px; height: 28px;">
                     <i class="bi bi-person-fill" style="font-size: 0.85rem;"></i>
                   </div>
                   {{ group.customerName }}
                </div>
                <div class="d-flex align-items-center gap-2">
                  <span v-if="group.isEmergency" class="badge bg-danger text-white rounded-pill shadow-sm" style="font-size: 0.65rem; font-weight: 800; letter-spacing: 0.5px;">CẤP CỨU</span>
                  <span class="badge bg-secondary text-white rounded-pill px-2 shadow-sm" style="font-size: 0.75rem;">{{ group.items.length }} ca</span>
                </div>
              </div>
              <div class="p-2 bg-white">
                <div 
                  v-for="(card, idx) in group.items" 
                  :key="card.appointmentId"
                  class="position-relative rounded-3 p-2 mb-2"
                  :class="{'border-danger bg-danger bg-opacity-10': card.isEmergency, 'bg-light': !card.isEmergency}"
                  style="border: 1px solid rgba(0,0,0,0.05);"
                >
                  <div class="d-flex justify-content-between align-items-start mb-1">
                    <div class="fw-bold text-dark" style="font-size: 0.85rem;">
                      {{ getAnimalEmoji(card.species) }} {{ card.petName }}
                    </div>
                    <span class="badge bg-white text-secondary border rounded-pill" style="font-size: 0.7rem;">{{ formatQueueNumber(card.queueNumber) }}</span>
                  </div>
                  
                  <div v-if="card.symptom" class="text-muted text-truncate w-100 mb-1" style="font-size: 0.75rem;" :title="card.symptom">
                    <i class="bi bi-info-circle me-1"></i>{{ card.symptom }}
                  </div>
                  
                  <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="fw-bold" :class="getSlaTextClass(card)" style="font-size: 0.75rem;">
                      <i class="bi bi-clock me-1"></i>{{ getWaitingTimeText(card) }}
                    </span>
                    <span class="text-muted" style="font-size: 0.75rem;">Bs. {{ getLastWord(card.doctorName) }}</span>
                  </div>

                  <div class="d-flex gap-2">
                    <button v-if="isAnonymousEmergency(card)" class="btn btn-sm btn-outline-danger flex-grow-1 rounded py-1 fw-bold" style="font-size: 0.75rem;" @click="openLinkCustomerModal(card)">
                      <i class="bi bi-link-45deg"></i> Ghép
                    </button>
                    <button class="btn btn-sm btn-warning flex-grow-1 rounded py-1 fw-bold text-dark shadow-sm" style="font-size: 0.75rem;" @click.stop="updateStatus(card.appointmentId, 'in_progress')">
                      <i class="bi bi-megaphone-fill me-1"></i> Gọi khám
                    </button>
                  </div>
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

          <div class="kanban-list flex-grow-1" style="min-height: 450px;" v-auto-animate>
            <div v-if="filteredInProgress.length === 0" class="text-center py-5 text-muted">
              <i class="bi bi-stethoscope fs-2 text-muted mb-2 d-block"></i>
              <span class="small">Chưa có ca nào đang thực hiện khám.</span>
            </div>
            
            <div 
              v-for="(group, gIdx) in groupedInProgress" 
              :key="'progress-' + gIdx"
              class="card shadow-sm mb-2 border-0 rounded-4 overflow-hidden kanban-group-card"
            >
              <div class="px-3 py-2 d-flex justify-content-between align-items-center" style="background: linear-gradient(to right, #fffbeb, #ffffff); border-bottom: 1px dashed #fde68a;">
                <div class="fw-bold text-dark d-flex align-items-center" style="font-size: 0.95rem;">
                   <div class="bg-warning bg-opacity-25 text-warning-emphasis rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 28px; height: 28px;">
                     <i class="bi bi-person-fill" style="font-size: 0.85rem;"></i>
                   </div>
                   {{ group.customerName }}
                </div>
                <div class="d-flex align-items-center gap-2">
                  <span v-if="group.isEmergency" class="badge bg-danger text-white rounded-pill shadow-sm" style="font-size: 0.65rem; font-weight: 800; letter-spacing: 0.5px;">CẤP CỨU</span>
                  <span class="badge bg-warning text-dark rounded-pill px-2 shadow-sm" style="font-size: 0.75rem; font-weight: 700;">{{ group.items.length }} ca</span>
                </div>
              </div>
              <div class="p-2 bg-white">
                <div 
                  v-for="(card, idx) in group.items" 
                  :key="card.appointmentId"
                  class="position-relative rounded-3 p-2 mb-2"
                  :class="{'border-danger bg-danger bg-opacity-10': card.isEmergency, 'bg-light': !card.isEmergency}"
                  style="border: 1px solid rgba(0,0,0,0.05);"
                >
                  <div class="d-flex justify-content-between align-items-start mb-1">
                    <div class="fw-bold text-dark" style="font-size: 0.85rem;">
                      {{ getAnimalEmoji(card.species) }} {{ card.petName }}
                    </div>
                    <span class="badge bg-white text-secondary border rounded-pill" style="font-size: 0.7rem;">{{ formatQueueNumber(card.queueNumber) }}</span>
                  </div>
                  
                  <div v-if="card.symptom" class="text-muted text-truncate w-100 mb-1" style="font-size: 0.75rem;" :title="card.symptom">
                    <i class="bi bi-info-circle me-1"></i>{{ card.symptom }}
                  </div>
                  
                  <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="text-muted" style="font-size: 0.75rem;"><i class="bi bi-activity me-1 text-warning"></i>Đang khám</span>
                    <span class="text-muted fw-bold" style="font-size: 0.75rem;">Bs. {{ getLastWord(card.doctorName) }}</span>
                  </div>

                  <div v-if="isAnonymousEmergency(card)" class="mt-2">
                    <button class="btn btn-sm btn-outline-danger w-100 rounded py-1 fw-bold" style="font-size: 0.75rem;" @click="openLinkCustomerModal(card)">
                      <i class="bi bi-link-45deg"></i> Ghép
                    </button>
                  </div>
                </div>
                
                <div class="bg-success bg-opacity-10 text-success rounded-3 p-2 text-center border border-success border-opacity-25 shadow-sm mt-1 mb-1 mx-1" style="font-size: 0.72rem; cursor: default;">
                  <i class="bi bi-info-circle-fill me-1"></i> 
                  <span class="fw-bold">Tự động chuyển sang hàng thanh toán<br>sau khi bác sĩ hoàn tất khám</span>
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

          <div class="kanban-list flex-grow-1" style="min-height: 450px;" v-auto-animate>
            <div v-if="filteredReadyToPay.length === 0" class="text-center py-5 text-muted">
              <i class="bi bi-check-circle fs-2 text-muted mb-2 d-block"></i>
              <span class="small">Không có ca khám chờ thanh toán.</span>
            </div>
            
            <div 
              v-for="(group, gIdx) in groupedReadyToPay" 
              :key="'ready-' + gIdx"
              class="card shadow-sm mb-2 border-0 rounded-4 overflow-hidden kanban-group-card"
            >
              <div class="px-3 py-2 d-flex justify-content-between align-items-center" style="background: linear-gradient(to right, #f0fdf4, #ffffff); border-bottom: 1px dashed #bbf7d0;">
                <div class="fw-bold text-dark d-flex align-items-center" style="font-size: 0.95rem;">
                   <div class="bg-success bg-opacity-25 text-success-emphasis rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 28px; height: 28px;">
                     <i class="bi bi-person-fill" style="font-size: 0.85rem;"></i>
                   </div>
                   {{ group.customerName }}
                </div>
                <div class="d-flex align-items-center gap-2">
                  <span v-if="group.isEmergency" class="badge bg-danger text-white rounded-pill shadow-sm" style="font-size: 0.65rem; font-weight: 800; letter-spacing: 0.5px;">CẤP CỨU</span>
                  <span class="badge bg-success text-white rounded-pill px-2 shadow-sm" style="font-size: 0.75rem; font-weight: 700;">{{ group.items.length }} ca</span>
                </div>
              </div>
              <div class="p-2 bg-white">
                <div 
                  v-for="(card, idx) in group.items" 
                  :key="card.appointmentId"
                  class="position-relative rounded-3 p-2 mb-2"
                  :class="{'border-danger bg-danger bg-opacity-10': card.isEmergency, 'bg-light': !card.isEmergency}"
                  style="border: 1px solid rgba(0,0,0,0.05);"
                >
                  <div class="d-flex justify-content-between align-items-start mb-1">
                    <div class="fw-bold text-dark" style="font-size: 0.85rem;">
                      {{ getAnimalEmoji(card.species) }} {{ card.petName }}
                    </div>
                    <span class="badge bg-white text-secondary border rounded-pill" style="font-size: 0.7rem;">{{ formatQueueNumber(card.queueNumber) }}</span>
                  </div>
                  
                  <div v-if="card.symptom" class="text-muted text-truncate w-100 mb-1" style="font-size: 0.75rem;" :title="card.symptom">
                    <i class="bi bi-info-circle me-1"></i>{{ card.symptom }}
                  </div>
                  
                  <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="text-success fw-bold" style="font-size: 0.75rem;">Chờ thu ngân</span>
                    <span class="text-muted" style="font-size: 0.75rem;">Bs. {{ getLastWord(card.doctorName) }}</span>
                  </div>

                  <div class="d-flex gap-2">
                    <button v-if="isAnonymousEmergency(card)" class="btn btn-sm btn-outline-danger flex-grow-1 rounded py-1 fw-bold" style="font-size: 0.75rem;" @click="openLinkCustomerModal(card)">
                      <i class="bi bi-link-45deg"></i> Ghép
                    </button>
                    <button class="btn btn-sm btn-primary flex-grow-1 rounded py-1 fw-bold shadow-sm" style="font-size: 0.75rem;" @click.stop="goToInvoiceTab(card.appointmentId)">
                      Thanh toán
                    </button>
                  </div>
                </div>
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
                <input type="text" class="form-control border-primary bg-light text-muted" readonly value="-- Tự động phân công --" />
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
        <div class="zalo-modal-header bg-white border-bottom p-4 d-flex justify-content-between align-items-center">
          <h5 class="modal-title fw-bold text-dark mb-0">
            <div class="d-flex align-items-center bg-success bg-opacity-10 text-success rounded-pill px-3 py-2">
              <i class="bi bi-qr-code-scan me-2 fs-5"></i> Quét Mã Check-in
            </div>
          </h5>
          <button class="modal-close text-muted border-0 bg-light rounded-circle shadow-sm d-flex align-items-center justify-content-center hover-lift" style="width: 36px; height: 36px;" @click="closeQrModal"><i class="bi bi-x-lg fs-6"></i></button>
        </div>
        <div class="zalo-modal-body p-4 bg-white">
          
          <div v-if="previewAppointment">
            <!-- Preview Card -->
            <div class="text-center mb-4">
              <div :class="['rounded-circle d-inline-flex align-items-center justify-content-center mb-3', previewAppointment.hasGlobalError ? 'bg-danger bg-opacity-10' : 'bg-success bg-opacity-10']" style="width: 70px; height: 70px;">
                <i :class="['bi fs-1', previewAppointment.hasGlobalError ? 'bi-exclamation-triangle-fill text-danger' : 'bi-check-circle-fill text-success']"></i>
              </div>
              <h5 :class="['fw-bold mb-1', previewAppointment.hasGlobalError ? 'text-danger' : 'text-success']">
                {{ previewAppointment.hasGlobalError ? 'Lỗi Check-in!' : 'Quét Thành Công!' }}
              </h5>
              <p class="text-muted small">
                {{ previewAppointment.hasGlobalError ? 'Không thể check-in lúc này.' : 'Vui lòng xác nhận danh sách và nhập cân nặng (tùy chọn) trước khi đưa vào hàng đợi.' }}
              </p>
            </div>

            <div v-if="previewAppointment.hasGlobalError" class="alert alert-danger border-danger border-opacity-25 rounded-3 mb-4 text-start">
              <i class="bi bi-info-circle-fill me-2"></i> <strong>Lưu ý:</strong> {{ previewAppointment.globalErrorMessage }}
            </div>

            <div v-if="previewAppointment.appointments && previewAppointment.appointments.length > 0" class="mb-4 text-start">
              <div class="d-flex justify-content-between align-items-center mb-2 px-1">
                <span class="fw-bold text-dark fs-6"><i class="bi bi-person-badge text-primary me-2"></i>{{ previewAppointment.customerName }}</span>
                <span class="badge bg-primary rounded-pill">{{ previewAppointment.appointments.length }} Lịch Hẹn</span>
              </div>
              
              <div style="max-height: 350px; overflow-y: auto; overflow-x: hidden;" class="pe-2">
                <div v-for="appt in previewAppointment.appointments" :key="appt.appointmentId" class="card border border-opacity-25 bg-light rounded-4 mb-3" :class="appt.hasError ? 'border-danger' : 'border-success'">
                  <div class="card-body p-3">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                      <div class="fw-bold text-dark fs-6">{{ appt.petName }} <span v-if="appt.petSpecies" class="text-muted fw-normal" style="font-size: 0.8rem;">({{ appt.petSpecies }})</span></div>
                      <span v-if="appt.hasError" class="badge bg-danger rounded-pill"><i class="bi bi-x-circle me-1"></i>Lỗi</span>
                      <span v-else class="badge bg-success rounded-pill"><i class="bi bi-check-circle me-1"></i>Sẵn sàng</span>
                    </div>

                    <div v-if="appt.hasError" class="text-danger small mb-2"><i class="bi bi-exclamation-triangle-fill me-1"></i> {{ appt.errorMessage }}</div>
                    
                    <div class="d-flex justify-content-between mb-1">
                      <span class="text-muted small">Khung giờ:</span>
                      <span class="fw-bold text-dark small">{{ appt.startTime ? appt.startTime.substring(0, 5) : formatTimeOnly(appt.appointmentDate) }}</span>
                    </div>
                    <div class="d-flex justify-content-between mb-1">
                      <span class="text-muted small">Bác sĩ:</span>
                      <span class="fw-bold text-dark small">{{ appt.doctorName || 'Tự động xếp' }}</span>
                    </div>
                    <div class="d-flex justify-content-between mb-3">
                      <span class="text-muted small">Dịch vụ:</span>
                      <span class="fw-bold text-dark small text-truncate" style="max-width: 150px;" :title="appt.serviceName">{{ appt.serviceName || 'Khám bệnh' }}</span>
                    </div>

                    <!-- Input Cân Nặng -->
                    <div class="mt-2" v-if="!appt.hasError">
                      <label class="form-label small text-muted mb-1 fw-bold">Cân nặng hiện tại (kg)</label>
                      <div class="input-group input-group-sm">
                        <span class="input-group-text bg-white border-end-0 text-success"><i class="bi bi-speedometer2"></i></span>
                        <input type="number" v-model="appt.currentWeight" class="form-control border-start-0" placeholder="0.0" step="0.1" min="0">
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="d-flex gap-2">
              <button class="btn btn-light w-50 rounded-pill py-2.5 fw-bold" @click="cancelPreview">Hủy quét</button>
              <button v-if="!previewAppointment.hasGlobalError" class="btn btn-success w-50 rounded-pill py-2.5 fw-bold shadow-sm" @click="confirmCheckIn">Check-in Tất Cả <i class="bi bi-arrow-right ms-1"></i></button>
              <button v-else class="btn btn-danger w-50 rounded-pill py-2.5 fw-bold shadow-sm" @click="closeQrModal">Đóng</button>
            </div>
          </div>

          <div v-else class="px-2 py-1">
            <!-- Camera Scanner Area -->
            <div class="position-relative mb-4 rounded-4 overflow-hidden border shadow-sm mx-auto d-flex align-items-center justify-content-center" 
                 style="width: 100%; min-height: 260px; background: #f0f2f5;">
                 
              <!-- Actual QR Reader -->
              <div v-show="isCameraActive" id="qr-reader" class="w-100 h-100"></div>
              
              <!-- Placeholder when camera is off -->
              <div v-if="!isCameraActive" class="text-center p-4">
                 <div class="bg-white rounded-circle shadow-sm d-inline-flex align-items-center justify-content-center mb-3 text-success" style="width: 70px; height: 70px;">
                   <i class="bi bi-camera-video fs-1"></i>
                 </div>
                 <h6 class="fw-bold text-dark mb-1">Camera Đang Tắt</h6>
                 <p class="text-muted small mb-3">Bật camera để ứng dụng tự động nhận diện mã QR của khách hàng.</p>
                 <button class="btn rounded-pill px-4 py-2.5 fw-bold shadow-sm hover-lift d-flex align-items-center justify-content-center mx-auto btn-success text-white" 
                   @click="toggleCamera">
                   <i class="bi bi-camera-video-fill me-2 fs-5"></i> Kích Hoạt Camera
                 </button>
              </div>

              <!-- Floating Stop button when camera is on -->
              <button v-if="isCameraActive" class="btn btn-sm btn-danger rounded-pill shadow-lg position-absolute bottom-0 start-50 translate-middle-x mb-3 px-3 py-1.5 fw-bold border border-white border-opacity-50" style="z-index: 10;" @click="toggleCamera">
                 <i class="bi bi-camera-video-off-fill me-1"></i> Tắt Camera
              </button>
            </div>
            
            <div class="position-relative d-flex align-items-center justify-content-center mb-4 mt-2">
               <hr class="w-100 text-muted opacity-25 m-0">
               <span class="position-absolute bg-white px-3 text-muted fw-bold text-uppercase" style="font-size: 0.7rem; letter-spacing: 1.5px;">Hoặc Nhập Tay</span>
            </div>
            
            <div class="mb-4 text-start">
              <div class="input-group input-group-lg shadow-sm rounded-4 overflow-hidden border border-success border-opacity-25 bg-white">
                <span class="input-group-text bg-transparent border-0 text-success ps-4 pe-2"><i class="bi bi-keyboard fs-4"></i></span>
                <input 
                  type="text" 
                  v-model="qrManualCode" 
                  class="form-control border-0 fw-bold fs-5 shadow-none ps-2 bg-transparent" 
                  style="letter-spacing: 2px;"
                  placeholder="Nhập mã check-in..."
                  @keyup.enter="previewCheckIn"
                />
              </div>
            </div>

            <button class="btn btn-success w-100 rounded-pill py-3 fw-bold shadow-sm d-flex align-items-center justify-content-center gap-2" @click="previewCheckIn">
              <i class="bi bi-check2-circle fs-5"></i> Kiểm Tra Mã
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue';
import { vAutoAnimate } from '@formkit/auto-animate/vue';
import Swal from 'sweetalert2';
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
const activeDropdownId = ref<number | null>(null);
const toggleDropdown = (id: number) => {
  activeDropdownId.value = activeDropdownId.value === id ? null : id;
};

const doctorList = ref<any[]>([]);
const queueList = ref<any[]>([]);
const intervals = ref<any[]>([]);
const isRefreshing = ref(false);

// Modals state
const showEmergencyModal = ref(false);
const showLinkCustomerModal = ref(false);
const showQrModal = ref(false);
const previewAppointment = ref<any>(null);
const isCameraActive = ref(false);

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

const groupItemsByCustomer = (items: any[]) => {
  const groups = new Map();
  items.forEach(item => {
    const key = item.customerId && item.customerId !== '00000000-0000-0000-0000-000000000000' 
                ? item.customerId 
                : item.appointmentId.toString();
    if (!groups.has(key)) {
      groups.set(key, {
        customerId: item.customerId,
        customerName: item.customerName || 'Khách vãng lai',
        isEmergency: false,
        items: []
      });
    }
    const group = groups.get(key);
    group.items.push(item);
    if (item.isEmergency) group.isEmergency = true;
  });
  return Array.from(groups.values());
};

const groupedWaiting = computed(() => groupItemsByCustomer(filteredWaiting.value));
const groupedInProgress = computed(() => groupItemsByCustomer(filteredInProgress.value));
const groupedReadyToPay = computed(() => groupItemsByCustomer(filteredReadyToPay.value));

// Timers / Time helper
const nowRef = ref(new Date());
let timeUpdater: any = null;

const fixTimezone = (dateStr: string) => {
  if (!dateStr) return '';
  return dateStr.endsWith('Z') ? dateStr.slice(0, -1) : dateStr;
};

const getWaitingTimeText = (card: any) => {
  // Ưu tiên dùng checkInTime, fallback về appointmentDate
  const timeStr = card.checkInTime || card.appointmentDate;
  if (!timeStr) return '---';
  const refTime = new Date(fixTimezone(timeStr));
  const diffMs = nowRef.value.getTime() - refTime.getTime();
  const diffMins = Math.max(0, Math.floor(diffMs / 60000));
  return `${diffMins} phút`;
};

const getSlaClass = (card: any) => {
  if (card.status !== 'waiting') return '';
  const timeStr = card.checkInTime || card.appointmentDate;
  if (!timeStr) return '';
  const diffMins = Math.floor((nowRef.value.getTime() - new Date(fixTimezone(timeStr)).getTime()) / 60000);
  if (diffMins >= 30) return 'sla-danger';
  if (diffMins >= 15) return 'sla-warning';
  return '';
};

const getSlaTextClass = (card: any) => {
  if (card.status !== 'waiting') return 'text-muted';
  const timeStr = card.checkInTime || card.appointmentDate;
  if (!timeStr) return 'text-muted';
  const diffMins = Math.floor((nowRef.value.getTime() - new Date(fixTimezone(timeStr)).getTime()) / 60000);
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

const formatQueueNumber = (num: number | string | null | undefined) => {
  if (num == null || num === '') return '---';
  const n = parseInt(num.toString(), 10);
  if (isNaN(n) || n <= 0) return '---';
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

const manualRefresh = async () => {
  if (isRefreshing.value) return;
  isRefreshing.value = true;
  await loadQueue();
  Swal.fire({
    toast: true,
    position: 'top-end',
    icon: 'success',
    title: 'Đã cập nhật hàng khám',
    showConfirmButton: false,
    timer: 1500,
    timerProgressBar: true
  });
  setTimeout(() => {
    isRefreshing.value = false;
  }, 500); // Tạo độ trễ ảo để user kịp nhìn thấy hiệu ứng loading
};

const loadDoctors = async () => {
  try {
    const res = await api.get('/receptionist/doctors');
    const excludedDoctors = ['Nguyễn Xinh Trai', 'Bác sĩ.A', 'Bác Sĩ Test'];
    doctorList.value = res.data.filter((d: any) => !excludedDoctors.includes(d.fullName));
  } catch (err) {
    console.error('Lỗi tải danh sách bác sĩ:', err);
  }
};

const updateStatus = async (appointmentId: number, status: string) => {
  activeDropdownId.value = null;
  let title = 'Xác nhận thao tác';
  let text = 'Bạn có chắc chắn muốn chuyển trạng thái ca khám này?';
  let icon: any = 'question';
  let confirmButtonColor = '#f59e0b'; // warning (vàng)
  let confirmButtonText = 'Đồng ý';

  if (status === 'cancelled') {
    title = 'Xác nhận Hủy Ca';
    text = 'Sau khi hủy ca khám, bạn sẽ không thể hoàn tác. Bạn có chắc chắn?';
    icon = 'warning';
    confirmButtonColor = '#dc3545'; // danger (đỏ)
    confirmButtonText = 'Có, Hủy ngay!';
  } else if (status === 'ready_to_pay') {
    title = 'Chuyển sang Thu Ngân';
    text = 'Xác nhận ca khám đã hoàn tất và chuyển bệnh nhi sang quầy thanh toán?';
    icon = 'info';
    confirmButtonColor = '#198754'; // success (xanh lá)
    confirmButtonText = 'Đồng ý chuyển';
  } else if (status === 'in_progress') {
    title = 'Bắt đầu Khám';
    text = 'Chuyển bệnh nhi vào phòng khám ngay bây giờ?';
    confirmButtonText = 'Chuyển khám';
  }

  const result = await Swal.fire({
    title,
    text,
    icon,
    showCancelButton: true,
    confirmButtonColor,
    cancelButtonColor: '#6c757d',
    confirmButtonText,
    cancelButtonText: 'Đóng lại',
    customClass: {
      popup: 'rounded-4' // Thêm bo góc cho popup
    }
  });

  if (result.isConfirmed) {
    try {
      const res = await api.put(`/receptionist/queue/${appointmentId}/status`, { status });
      if (res.data.success) {
        Swal.fire({
          toast: true,
          position: 'top-end',
          icon: 'success',
          title: 'Đã cập nhật trạng thái thành công!',
          showConfirmButton: false,
          timer: 2000,
          timerProgressBar: true
        });
        await loadQueue();
      }
    } catch (err) {
      Swal.fire({
        icon: 'error',
        title: 'Thất bại',
        text: 'Đã xảy ra lỗi kết nối. Vui lòng thử lại!',
        customClass: { popup: 'rounded-4' }
      });
      console.error(err);
    }
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

const toggleCamera = async () => {
  if (isCameraActive.value) {
    isCameraActive.value = false;
    await stopScanner();
  } else {
    isCameraActive.value = true;
    await startScanner();
  }
};

const closeQrModal = () => {
  showQrModal.value = false;
  previewAppointment.value = null;
  isCameraActive.value = false;
  stopScanner();
};

const previewCheckIn = async () => {
  const token = qrManualCode.value.trim();
  if (!token) return;
  try {
    if (isCameraActive.value) await stopScanner();
    const res = await api.get(`/receptionist/appointment-preview?qrToken=${token}`);
    if (res.data.success) {
      previewAppointment.value = res.data.data;
      if (previewAppointment.value && previewAppointment.value.appointments) {
        previewAppointment.value.appointments.forEach((a: any) => {
          a.currentWeight = a.petWeight; // Initialize input with past weight
        });
      }
    } else {
      alert(res.data.message);
      if (isCameraActive.value) startScanner();
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Không thể kiểm tra mã. Vui lòng thử lại.');
    if (isCameraActive.value) startScanner();
  }
};

const cancelPreview = () => {
  previewAppointment.value = null;
  qrManualCode.value = '';
  if (isCameraActive.value) startScanner();
};

const confirmCheckIn = async () => {
  if (!previewAppointment.value) return;
  try {
    // Collect valid items
    const validItems = previewAppointment.value.appointments
      .filter((a: any) => !a.hasError)
      .map((a: any) => ({
        appointmentId: a.appointmentId,
        currentWeight: a.currentWeight || null
      }));

    if (validItems.length === 0) {
      alert('Không có thú cưng nào hợp lệ để check-in.');
      return;
    }

    const res = await api.post('/receptionist/check-in', {
      qrToken: previewAppointment.value.qrToken,
      items: validItems
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
  isCameraActive.value = false;
  showQrModal.value = true;
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
  loadDoctors();
  loadQueue();

  // Close dropdown on click outside
  document.addEventListener('click', (e) => {
    const target = e.target as HTMLElement;
    if (!target.closest('.dropdown')) {
      activeDropdownId.value = null;
    }
  });

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
