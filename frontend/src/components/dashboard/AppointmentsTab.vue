<template>
  <div class="appointments-tab container-fluid p-0">
    <!-- Statistics Cards (KPI) -->
    <div class="row mb-4">
      <div class="col-md col-sm-6 mb-3">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-primary bg-opacity-10 text-primary h-100">
          <div class="d-flex align-items-center justify-content-between">
            <div>
              <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Hôm Nay</h6>
              <h3 class="fw-extrabold mb-0">{{ stats.total }}</h3>
            </div>
            <i class="bi bi-calendar3 fs-1 opacity-50"></i>
          </div>
        </div>
      </div>
      <div class="col-md col-sm-6 mb-3">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-warning bg-opacity-10 text-warning h-100">
          <div class="d-flex align-items-center justify-content-between">
            <div>
              <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Chờ duyệt</h6>
              <h3 class="fw-extrabold mb-0">{{ stats.pending }}</h3>
            </div>
            <i class="bi bi-clock-history fs-1 opacity-50"></i>
          </div>
        </div>
      </div>
      <div class="col-md col-sm-6 mb-3">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-info bg-opacity-10 text-info h-100">
          <div class="d-flex align-items-center justify-content-between">
            <div>
              <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Đã xác nhận</h6>
              <h3 class="fw-extrabold mb-0">{{ stats.confirmed }}</h3>
            </div>
            <i class="bi bi-check-circle fs-1 opacity-50"></i>
          </div>
        </div>
      </div>
      <div class="col-md col-sm-6 mb-3">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-primary bg-opacity-10 text-primary h-100" style="background-color: rgba(13, 110, 253, 0.1) !important; color: #0d6efd !important;">
          <div class="d-flex align-items-center justify-content-between">
            <div>
              <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Chờ khám</h6>
              <h3 class="fw-extrabold mb-0">{{ stats.waiting }}</h3>
            </div>
            <i class="bi bi-person-workspace fs-1 opacity-50"></i>
          </div>
        </div>
      </div>
      <div class="col-md col-sm-6 mb-3">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-success bg-opacity-10 text-success h-100">
          <div class="d-flex align-items-center justify-content-between">
            <div>
              <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Đã hoàn thành</h6>
              <h3 class="fw-extrabold mb-0">{{ stats.completed }}</h3>
            </div>
            <i class="bi bi-check-all fs-1 opacity-50"></i>
          </div>
        </div>
      </div>
    </div>

    <!-- Header Actions -->
    <div class="card border-0 shadow-sm rounded-4 p-3 mb-4 bg-white">
      <div class="d-flex flex-wrap justify-content-between align-items-center gap-3">
        <h5 class="fw-bold mb-0 text-dark">
          <i class="bi bi-calendar-check-fill me-2 text-warning"></i> Lịch Hẹn & Điều Phối
        </h5>
        <div class="d-flex align-items-center gap-3">
          <span class="text-muted small fw-bold">Bác sĩ:</span>
          <select v-model="selectedDoctor" @change="loadEvents" class="form-select border-warning rounded-pill px-3 py-1.5 shadow-sm" style="width: 200px;">
            <option value="ALL">Tất cả bác sĩ</option>
            <option v-for="doc in doctorList" :key="doc.id" :value="doc.id">Bs. {{ doc.fullName }}</option>
          </select>
          <button class="btn btn-premium rounded-pill px-4 fw-bold shadow-sm" @click="openCreateModal">
            <i class="bi bi-plus-lg me-1"></i> Tạo lịch hẹn mới
          </button>
        </div>
      </div>
    </div>

    <!-- Internal Navigation Tabs -->
    <ul class="nav nav-tabs border-bottom-0 mb-4 bg-white p-2 rounded-4 shadow-sm" style="font-family: 'Outfit', sans-serif;">
      <li class="nav-item">
        <button class="nav-link rounded-3 px-4 py-2.5 fw-bold border-0" :class="{ 'active': activeSubTab === 'calendar' }" @click="activeSubTab = 'calendar'">
          <i class="bi bi-calendar3 me-2"></i>Lịch Trình Chi Tiết
        </button>
      </li>
      <li class="nav-item">
        <button class="nav-link rounded-3 px-4 py-2.5 fw-bold border-0 position-relative" :class="{ 'active': activeSubTab === 'pending' }" @click="activeSubTab = 'pending'">
          <i class="bi bi-inbox me-2"></i>Yêu Cầu Chờ Duyệt
          <span v-if="pendingList.length > 0" class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger" style="font-size: 0.75rem;">
            {{ pendingList.length }}
          </span>
        </button>
      </li>
      <li class="nav-item">
        <button class="nav-link rounded-3 px-4 py-2.5 fw-bold border-0" :class="{ 'active': activeSubTab === 'flow' }" @click="activeSubTab = 'flow'">
          <i class="bi bi-kanban me-2"></i>Clinical Flowboard
        </button>
      </li>
    </ul>

    <!-- Tabs Content -->
    <div class="tab-content">
      <!-- Calendar Agenda View -->
      <div v-if="activeSubTab === 'calendar'" class="animate-fade-in">
        <div class="row g-4">
          <!-- Left Picker -->
          <div class="col-md-3">
            <div class="card border-0 shadow-sm rounded-4 p-3 bg-white h-100">
              <label class="form-label fw-bold text-dark mb-2"><i class="bi bi-calendar-day text-warning me-1"></i>Chọn ngày khám</label>
              <input type="date" v-model="selectedDate" @change="loadEvents" class="form-control border-warning rounded-pill mb-4 py-2 text-center fw-bold" />
              
              <div class="border-top pt-3 text-start small">
                <span class="fw-bold d-block text-muted mb-2">Trú thích trạng thái:</span>
                <div class="d-flex flex-column gap-2 fw-semibold">
                  <span class="text-warning"><i class="bi bi-circle-fill me-2"></i>Chờ duyệt (Pending)</span>
                  <span class="text-info"><i class="bi bi-circle-fill me-2"></i>Đã xác nhận</span>
                  <span class="text-primary"><i class="bi bi-circle-fill me-2"></i>Đang chờ (Waiting)</span>
                  <span class="text-orange"><i class="bi bi-circle-fill me-2" style="color: #fd7e14;"></i>Đang khám</span>
                  <span class="text-success"><i class="bi bi-circle-fill me-2"></i>Đã xong / Chờ TT</span>
                  <span class="text-danger"><i class="bi bi-circle-fill me-2"></i>Đã hủy</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Agenda Schedules List -->
          <div class="col-md-9">
            <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
              <h5 class="fw-bold mb-3 text-dark">Lịch hẹn trong ngày {{ formatDate(selectedDate) }}</h5>
              
              <div v-if="loadingEvents" class="text-center py-5 text-muted">
                <div class="spinner-border spinner-border-sm text-warning mb-2"></div>
                <div>Đang tải dữ liệu lịch trình...</div>
              </div>
              <div v-else-if="eventsList.length === 0" class="text-center py-5 text-muted">
                <i class="bi bi-calendar-x fs-2 d-block mb-2"></i>
                Không có lịch hẹn nào ghi nhận trong ngày này.
              </div>
              <div v-else class="table-responsive rounded-4 border overflow-hidden">
                <table class="table table-hover align-middle mb-0">
                  <thead class="table-light">
                    <tr>
                      <th class="ps-3">Giờ hẹn</th>
                      <th>Thú cưng</th>
                      <th>Chủ nuôi</th>
                      <th>Bác sĩ phụ trách</th>
                      <th>Dịch vụ</th>
                      <th class="text-center">Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr 
                      v-for="evt in eventsList" 
                      :key="evt.id" 
                      class="cursor-pointer" 
                      @click="openDetailModal(evt.id)"
                    >
                      <td class="ps-3 fw-bold text-dark">{{ formatTimeOnly(evt.appointmentTime || evt.start) }}</td>
                      <td>
                        <div class="d-flex align-items-center gap-2">
                          <span class="fs-4">{{ getAnimalEmoji(evt.species) }}</span>
                          <div>
                            <span class="fw-bold text-dark">{{ evt.petName }}</span>
                            <div class="small text-muted">{{ evt.species }}</div>
                          </div>
                        </div>
                      </td>
                      <td>
                        <div class="fw-semibold">{{ evt.customerName }}</div>
                        <small class="text-muted">{{ evt.customerPhone }}</small>
                      </td>
                      <td>Bs. {{ getLastWord(evt.doctorName) }}</td>
                      <td><span class="badge bg-success bg-opacity-10 text-success rounded px-2.5 py-1 fw-bold">{{ evt.serviceName }}</span></td>
                      <td class="text-center">
                        <span class="badge rounded-pill" :class="getStatusBadgeClass(evt.status)">
                          {{ getStatusLabel(evt.status) }}
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

      <!-- Pending Approval Queue -->
      <div v-else-if="activeSubTab === 'pending'" class="card border-0 shadow-sm rounded-4 p-4 bg-white animate-fade-in">
        <h5 class="fw-bold mb-3 text-dark"><i class="bi bi-clock-history text-warning me-2"></i>Yêu cầu lịch hẹn chờ duyệt</h5>
        
        <div v-if="pendingList.length === 0" class="text-center py-5 text-muted">
          <i class="bi bi-inbox fs-2 mb-2 d-block text-black-50"></i>
          Không có yêu cầu đặt lịch trực tuyến nào đang chờ duyệt.
        </div>
        <div v-else class="table-responsive rounded-4 border overflow-hidden">
          <table class="table table-hover align-middle mb-0">
            <thead class="table-light">
              <tr>
                <th class="ps-3">Thú cưng</th>
                <th>Chủ nuôi</th>
                <th>Dịch vụ khám</th>
                <th>Bác sĩ</th>
                <th>Giờ hẹn</th>
                <th>Triệu chứng</th>
                <th class="text-center" style="width: 260px;">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in pendingList" :key="item.id">
                <td class="ps-3">
                  <div class="d-flex align-items-center gap-2">
                    <span class="fs-4">{{ getAnimalEmoji(item.species) }}</span>
                    <div>
                      <span class="fw-bold text-dark">{{ item.petName }}</span>
                      <div class="small text-muted">{{ item.species }} | {{ item.breed || 'Chưa rõ giống' }}</div>
                    </div>
                  </div>
                </td>
                <td>
                  <div class="fw-bold">{{ item.customerName }}</div>
                  <div class="text-muted small">{{ item.customerPhone }}</div>
                </td>
                <td><span class="badge bg-success bg-opacity-10 text-success fw-bold">{{ item.serviceName }}</span></td>
                <td>Bs. {{ getLastWord(item.doctorName) }}</td>
                <td class="fw-bold text-dark">{{ formatDateFull(item.appointmentDate) }}</td>
                <td class="text-muted small text-truncate" style="max-width: 150px;">{{ item.symptom || '—' }}</td>
                <td class="text-center">
                  <div class="d-flex gap-2 justify-content-center">
                    <button class="btn btn-sm btn-success rounded-pill px-3 fw-bold" @click="updateStatus(item.id, 'confirmed')">
                      <i class="bi bi-check2"></i> Duyệt
                    </button>
                    <button class="btn btn-sm btn-outline-danger rounded-pill px-3" @click="updateStatus(item.id, 'cancelled')">
                      <i class="bi bi-x"></i> Từ chối
                    </button>
                    <button class="btn btn-sm btn-light border rounded-pill" @click="openDetailModal(item.id)">
                      <i class="bi bi-eye"></i>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Flowboard View -->
      <div v-else-if="activeSubTab === 'flow'" class="animate-fade-in">
        <div class="row g-4">
          <!-- Waiting column -->
          <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 p-3 bg-light" style="min-height: 500px;">
              <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="fw-bold text-secondary mb-0 text-uppercase"><i class="bi bi-hourglass-split me-2"></i>Đang chờ khám</h6>
                <span class="badge bg-secondary rounded-pill px-3">{{ flowWaiting.length }}</span>
              </div>
              <div class="flow-card-list">
                <div v-for="item in flowWaiting" :key="item.appointmentId" class="card flow-patient-card border-start-primary p-3 mb-2 shadow-sm rounded-3 cursor-pointer bg-white" @click="openDetailModal(item.appointmentId)">
                  <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="fw-bold text-dark mb-0">{{ getAnimalEmoji(item.species) }} {{ item.petName }}</h6>
                    <span class="badge bg-light text-secondary border rounded-pill">#{{ item.queueNumber }}</span>
                  </div>
                  <p class="text-muted small mb-2"><i class="bi bi-person me-1"></i>{{ item.customerName || 'Khách vãng lai' }}</p>
                  <p v-if="item.symptom" class="small text-muted mb-2 text-truncate"><strong>Lý do:</strong> {{ item.symptom }}</p>
                  <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
                    <span class="badge rounded-pill bg-light text-dark border">{{ getWaitingTimeText(item) }}</span>
                    <span class="small fw-bold text-warning">Bs. {{ getLastWord(item.doctorName) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- In progress column -->
          <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 p-3 bg-light" style="min-height: 500px;">
              <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="fw-bold text-orange mb-0 text-uppercase" style="color: #fd7e14;"><i class="bi bi-activity me-2"></i>Đang khám</h6>
                <span class="badge bg-orange text-white rounded-pill px-3" style="background-color: #fd7e14;">{{ flowInProgress.length }}</span>
              </div>
              <div class="flow-card-list">
                <div v-for="item in flowInProgress" :key="item.appointmentId" class="card flow-patient-card border-start-warning p-3 mb-2 shadow-sm rounded-3 cursor-pointer bg-white" @click="openDetailModal(item.appointmentId)">
                  <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="fw-bold text-dark mb-0">{{ getAnimalEmoji(item.species) }} {{ item.petName }}</h6>
                    <span class="badge bg-light text-secondary border rounded-pill">#{{ item.queueNumber }}</span>
                  </div>
                  <p class="text-muted small mb-2"><i class="bi bi-person me-1"></i>{{ item.customerName || 'Khách vãng lai' }}</p>
                  <p v-if="item.symptom" class="small text-muted mb-2 text-truncate"><strong>Lý do:</strong> {{ item.symptom }}</p>
                  <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
                    <span class="badge bg-warning text-dark rounded-pill">Đang khám</span>
                    <span class="small fw-bold text-warning">Bs. {{ getLastWord(item.doctorName) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Completed column -->
          <div class="col-md-4">
            <div class="card border-0 shadow-sm rounded-4 p-3 bg-light" style="min-height: 500px;">
              <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="fw-bold text-success mb-0 text-uppercase"><i class="bi bi-cash-coin me-2"></i>Chờ thanh toán / Xong</h6>
                <span class="badge bg-success rounded-pill px-3">{{ flowCompleted.length }}</span>
              </div>
              <div class="flow-card-list">
                <div v-for="item in flowCompleted" :key="item.appointmentId" class="card flow-patient-card border-start-success p-3 mb-2 shadow-sm rounded-3 cursor-pointer bg-white" @click="openDetailModal(item.appointmentId)">
                  <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="fw-bold text-dark mb-0">{{ getAnimalEmoji(item.species) }} {{ item.petName }}</h6>
                    <span class="badge bg-light text-secondary border rounded-pill">#{{ item.queueNumber }}</span>
                  </div>
                  <p class="text-muted small mb-2"><i class="bi bi-person me-1"></i>{{ item.customerName || 'Khách vãng lai' }}</p>
                  <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
                    <span class="badge rounded-pill bg-success text-white">Chờ thanh toán</span>
                    <span class="small text-muted">Bs. {{ getLastWord(item.doctorName) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal 1: Create Appointment Modal -->
    <div v-if="showCreateModal" class="zalo-modal-overlay" @click.self="showCreateModal = false">
      <div class="zalo-modal-card modal-lg max-w-700">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold"><i class="bi bi-calendar-plus me-2"></i> Tạo Lịch Hẹn Khám Mới</h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showCreateModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <!-- Tab pills for customers selector -->
          <ul class="nav nav-pills nav-fill mb-4 gap-2 border p-1.5 rounded-pill bg-light" role="tablist">
            <li class="nav-item">
              <button class="nav-link rounded-pill fw-bold border-0" :class="{ 'active': createAptType === 'old' }" @click="createAptType = 'old'">
                <i class="bi bi-person-check me-2"></i>Khách hàng đã có
              </button>
            </li>
            <li class="nav-item">
              <button class="nav-link rounded-pill fw-bold border-0" :class="{ 'active': createAptType === 'new' }" @click="createAptType = 'new'">
                <i class="bi bi-person-plus me-2"></i>Đăng ký khách mới
              </button>
            </li>
          </ul>

          <form @submit.prevent="submitCreateAppointment">
            <!-- Part 1: Select Owner -->
            <div class="mb-4">
              <!-- Old Customer Selector -->
              <div v-if="createAptType === 'old'" class="bg-light p-3 rounded-4 border border-warning border-opacity-20">
                <div class="row align-items-center">
                  <div class="col-md-6 mb-3 mb-md-0">
                    <label class="form-label fw-bold small text-muted">Tìm kiếm SĐT khách hàng *</label>
                    <div class="input-group">
                      <span class="input-group-text bg-white border-end-0"><i class="bi bi-search text-muted"></i></span>
                      <input type="text" v-model="oldCustPhone" class="form-control border-start-0" placeholder="Nhập SĐT..." />
                      <button type="button" class="btn btn-warning fw-bold text-dark" @click="searchOldCustomer">Tìm</button>
                    </div>
                    <small v-if="searchOldCustError" class="text-danger mt-1 d-block"><i class="bi bi-exclamation-circle me-1"></i>Không tìm thấy khách hàng!</small>
                  </div>
                  <div class="col-md-6" v-if="oldCustData">
                    <label class="form-label fw-bold small text-muted">Thông tin chủ & Thú cưng</label>
                    <input type="text" class="form-control bg-white mb-2" readonly :value="oldCustData.customer.fullName" />
                    
                    <select v-model="formPayload.petId" class="form-select border-warning" required>
                      <option value="">-- Chọn thú cưng --</option>
                      <option v-for="pet in oldCustPets" :key="pet.id" :value="pet.id">{{ pet.name }} ({{ pet.species }})</option>
                    </select>
                  </div>
                </div>
              </div>

              <!-- New Customer inputs -->
              <div v-else class="bg-warning bg-opacity-10 p-3 rounded-4 border border-warning border-opacity-35">
                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label fw-bold small text-dark-gold">Số điện thoại *</label>
                    <input type="text" v-model="newCustForm.customerPhone" class="form-control border-warning border-opacity-50" required placeholder="VD: 0901234567" />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-bold small text-dark-gold">Tên chủ nuôi *</label>
                    <input type="text" v-model="newCustForm.customerName" class="form-control border-warning border-opacity-50" required placeholder="VD: Nguyễn Văn A" />
                  </div>
                  <div class="col-md-4">
                    <label class="form-label fw-bold small text-dark-gold">Tên thú cưng *</label>
                    <input type="text" v-model="newCustForm.petName" class="form-control border-warning border-opacity-50" required placeholder="VD: Milo" />
                  </div>
                  <div class="col-md-4">
                    <label class="form-label fw-bold small text-dark-gold">Giống loài *</label>
                    <select v-model="newCustForm.species" class="form-select border-warning border-opacity-50">
                      <option value="Chó">Chó</option>
                      <option value="Mèo">Mèo</option>
                      <option value="Khác">Khác</option>
                    </select>
                  </div>
                  <div class="col-md-4">
                    <label class="form-label fw-bold small text-dark-gold">Cân nặng (kg)</label>
                    <input type="number" step="0.1" v-model="newCustForm.petWeight" class="form-control border-warning border-opacity-50" placeholder="VD: 5.5" />
                  </div>
                </div>
              </div>
            </div>

            <!-- Part 2: Service & Doctor -->
            <div class="row g-3 mb-4">
              <div class="col-md-6">
                <label class="form-label fw-bold text-dark">Dịch vụ khám bệnh *</label>
                <select v-model="formPayload.serviceId" class="form-select border-primary" required>
                  <option value="">-- Chọn dịch vụ khám --</option>
                  <option v-for="srv in serviceList" :key="srv.id" :value="srv.id">
                    {{ srv.name }} ({{ formatCurrency(srv.price) }})
                  </option>
                </select>
              </div>
              <div class="col-md-6">
                <label class="form-label fw-bold text-dark">Bác sĩ chỉ định *</label>
                <select v-model="formPayload.doctorId" class="form-select border-primary" required>
                  <option value="">-- Chọn Bác sĩ phụ trách --</option>
                  <option v-for="doc in doctorList" :key="doc.id" :value="doc.id">Bs. {{ doc.fullName }}</option>
                </select>
              </div>
            </div>

            <!-- Part 3: Time Slot Selector -->
            <div class="card border-primary border-opacity-25 shadow-sm rounded-4 mb-4">
              <div class="card-body">
                <h6 class="fw-bold text-primary mb-3"><i class="bi bi-clock me-2"></i>Chọn thời gian đặt hẹn</h6>
                <div class="row g-3">
                  <div class="col-md-4 border-end pe-md-3">
                    <label class="form-label fw-bold small text-muted">Ngày đặt lịch *</label>
                    <input type="date" v-model="formPayload.dateOnly" class="form-control border-primary" required />
                  </div>
                  <div class="col-md-8 ps-md-3">
                    <label class="form-label fw-bold small text-muted">Khung giờ làm việc còn trống *</label>
                    <div class="d-flex flex-wrap gap-2">
                      <button 
                        type="button" 
                        v-for="time in workingHours" 
                        :key="time"
                        class="btn btn-sm rounded-pill px-3 py-1.5 fw-bold time-slot-btn"
                        :class="formPayload.timeOnly === time ? 'btn-primary text-white' : 'btn-outline-primary bg-white'"
                        @click="formPayload.timeOnly = time"
                      >
                        {{ time }}
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Part 4: Details note -->
            <div class="mb-4">
              <div class="form-floating">
                <textarea v-model="formPayload.symptom" class="form-control" style="height: 80px" placeholder="Lý do khám..." required></textarea>
                <label class="text-muted">Lý do khám / Triệu chứng bệnh nhi *</label>
              </div>
            </div>
            <div class="mb-3">
              <div class="form-floating">
                <textarea v-model="formPayload.note" class="form-control" style="height: 60px" placeholder="Ghi chú thêm..."></textarea>
                <label class="text-muted">Ghi chú thêm (Nội bộ phòng khám)</label>
              </div>
            </div>

            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showCreateModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-5 fw-bold">Xác Nhận Đặt Lịch</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal 2: Appointment Detail Dialog -->
    <div v-if="showDetailModal" class="zalo-modal-overlay" @click.self="showDetailModal = false">
      <div class="zalo-modal-card modal-lg max-w-700">
        <div class="zalo-modal-header bg-primary text-white">
          <h5 class="modal-title fw-bold"><i class="bi bi-clipboard-pulse me-2"></i> Chi Tiết Ca Hẹn Khám Bệnh</h5>
          <button class="modal-close text-white border-0 bg-transparent" @click="showDetailModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        
        <div class="zalo-modal-body bg-light" v-if="detailLoading">
          <div class="text-center py-5">
            <div class="spinner-border text-primary"></div>
          </div>
        </div>
        <div class="zalo-modal-body bg-light text-start" v-else-if="selectedDetail">
          <div class="row g-3">
            <!-- Left Card: Patient Summary -->
            <div class="col-md-5">
              <div class="card border-0 shadow-sm rounded-4 h-100 text-center p-3 bg-white">
                <div class="my-3 rounded-circle bg-primary bg-opacity-10 d-flex align-items-center justify-content-center mx-auto" style="width: 80px; height: 80px;">
                  <span class="fs-1">{{ getAnimalEmoji(selectedDetail.species) }}</span>
                </div>
                <h5 class="fw-bold text-primary mb-1">{{ selectedDetail.petName }}</h5>
                <span class="badge bg-secondary bg-opacity-10 text-secondary rounded-pill px-3 py-1.5 mb-3">
                  {{ selectedDetail.species }} | {{ selectedDetail.breed || 'Chưa rõ giống' }}
                </span>

                <hr class="w-100 my-2">

                <div class="text-start mt-3">
                  <div class="mb-2">
                    <small class="text-muted d-block">Chủ nuôi (Khách hàng)</small>
                    <strong class="text-dark">{{ selectedDetail.customerName }}</strong>
                  </div>
                  <div class="mb-0">
                    <small class="text-muted d-block">Số điện thoại liên hệ</small>
                    <strong class="text-dark"><i class="bi bi-telephone-fill text-primary me-1"></i>{{ selectedDetail.customerPhone }}</strong>
                  </div>
                </div>
              </div>
            </div>

            <!-- Right Card: Appointment service & time -->
            <div class="col-md-7">
              <div class="card border-0 shadow-sm rounded-4 h-100 p-3 bg-white">
                <h6 class="fw-bold text-secondary mb-3 border-bottom pb-2"><i class="bi bi-info-circle me-1"></i>Thông tin đặt hẹn</h6>
                
                <div class="row mb-3">
                  <div class="col-6">
                    <small class="text-muted d-block">Dịch vụ yêu cầu</small>
                    <span class="badge bg-success bg-opacity-10 text-success fw-bold p-2 rounded">{{ selectedDetail.serviceName }}</span>
                  </div>
                  <div class="col-6">
                    <small class="text-muted d-block">Bác sĩ phụ trách</small>
                    <span class="badge bg-primary bg-opacity-10 text-primary fw-bold p-2 rounded">Bs. {{ getLastWord(selectedDetail.doctorName) }}</span>
                  </div>
                </div>

                <div class="mb-3">
                  <small class="text-muted d-block">Thời gian đặt hẹn</small>
                  <div class="p-2 bg-light rounded text-dark fw-bold">
                    <i class="bi bi-calendar-event text-primary me-2"></i>{{ formatDateFull(selectedDetail.appointmentDate || selectedDetail.appointmentTime) }}
                  </div>
                </div>

                <div class="mb-3">
                  <small class="text-muted d-block">Triệu chứng lâm sàng</small>
                  <div class="p-2.5 bg-warning bg-opacity-10 border border-warning border-opacity-25 rounded text-dark small" style="min-height: 50px;">
                    {{ selectedDetail.symptom || 'Không có triệu chứng ghi nhận' }}
                  </div>
                </div>

                <div class="mb-0">
                  <small class="text-muted d-block">Ghi chú nội bộ</small>
                  <div class="p-2 bg-light rounded text-dark small" style="min-height: 40px;">
                    {{ selectedDetail.note || '—' }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Dynamic status operation buttons -->
          <div class="d-flex flex-wrap gap-2 justify-content-center mt-4 pt-3 border-top">
            <!-- Pending actions -->
            <template v-if="selectedDetail.status === 'pending'">
              <button class="btn btn-success rounded-pill px-4 fw-bold shadow-sm" @click="updateStatus(selectedDetail.id, 'confirmed')">
                <i class="bi bi-check2-circle me-1"></i> Duyệt lịch hẹn
              </button>
              <button class="btn btn-outline-danger rounded-pill px-4" @click="updateStatus(selectedDetail.id, 'cancelled')">
                <i class="bi bi-x-circle me-1"></i> Từ chối lịch hẹn
              </button>
            </template>

            <!-- Confirmed actions -->
            <template v-if="selectedDetail.status === 'confirmed'">
              <button class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" @click="updateStatus(selectedDetail.id, 'waiting')">
                <i class="bi bi-person-workspace me-1"></i> Check-in hàng chờ khám
              </button>
              <button class="btn btn-outline-danger rounded-pill px-4" @click="updateStatus(selectedDetail.id, 'cancelled')">
                <i class="bi bi-x-circle me-1"></i> Hủy lịch hẹn
              </button>
            </template>

            <!-- Waiting actions -->
            <template v-if="selectedDetail.status === 'waiting'">
              <span class="badge bg-primary fs-6 px-3 py-2.5 rounded-pill"><i class="bi bi-hourglass-split"></i> Đang trong hàng khám</span>
              <button class="btn btn-warning text-dark rounded-pill px-3 fw-bold shadow-sm" @click="updateStatus(selectedDetail.id, 'in_progress')">
                <i class="bi bi-play-circle me-1"></i> Chuyển phòng khám (Bypass)
              </button>
            </template>

            <!-- In Progress actions -->
            <template v-if="selectedDetail.status === 'in_progress'">
              <span class="badge bg-warning text-dark fs-6 px-3 py-2.5 rounded-pill"><i class="bi bi-heart-pulse"></i> Đang tiến hành khám</span>
              <button class="btn btn-success text-white rounded-pill px-3 fw-bold shadow-sm" @click="updateStatus(selectedDetail.id, 'ready_to_pay')">
                <i class="bi bi-cash me-1"></i> Hoàn tất khám (Bypass)
              </button>
            </template>

            <!-- Cancelled -->
            <span v-if="selectedDetail.status === 'cancelled'" class="badge bg-danger fs-6 px-4 py-2.5 rounded-pill"><i class="bi bi-x-circle"></i> Đã hủy ca hẹn</span>

            <!-- Completed -->
            <span v-if="selectedDetail.status === 'completed' || selectedDetail.status === 'ready_to_pay'" class="badge bg-success fs-6 px-4 py-2.5 rounded-pill"><i class="bi bi-check-circle"></i> Đã hoàn thành khám</span>

            <button class="btn btn-outline-secondary rounded-pill px-4 ms-auto" @click="showDetailModal = false">Đóng</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Toast Notification -->
    <div class="position-fixed top-0 end-0 p-3 mt-5 pt-5" style="z-index: 9999;">
      <div class="toast align-items-center text-white border-0 shadow-lg" :class="[`bg-${toastInfo.type}`, { 'show': toastInfo.show, 'hide': !toastInfo.show }]" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
          <div class="toast-body fw-bold">
            <i class="bi me-2" :class="toastInfo.type === 'success' ? 'bi-check-circle-fill' : (toastInfo.type === 'danger' ? 'bi-exclamation-triangle-fill' : 'bi-info-circle-fill')"></i>
            {{ toastInfo.message }}
          </div>
          <button type="button" class="btn-close btn-close-white me-2 m-auto" @click="toastInfo.show = false" aria-label="Close"></button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../../services/api';

// Tab states
const activeSubTab = ref<'calendar' | 'pending' | 'flow'>('calendar');
const selectedDate = ref(new Date().toISOString().slice(0, 10));
const selectedDoctor = ref('ALL');

// Data
const stats = ref({ total: 0, pending: 0, confirmed: 0, waiting: 0, completed: 0 });
const doctorList = ref<any[]>([]);
const serviceList = ref<any[]>([]);
const eventsList = ref<any[]>([]);
const pendingList = ref<any[]>([]);
const flowList = ref<any[]>([]);

// Loading
const loadingEvents = ref(false);

// Modals
const showCreateModal = ref(false);
const showDetailModal = ref(false);
const detailLoading = ref(false);
const selectedDetail = ref<any>(null);

// Toast
const toastInfo = ref({
  show: false,
  message: '',
  type: 'success'
});

const showToast = (message: string, type: 'success' | 'danger' | 'warning' = 'success') => {
  toastInfo.value = { show: true, message, type };
  setTimeout(() => {
    toastInfo.value.show = false;
  }, 3000);
};

// Forms
const createAptType = ref<'old' | 'new'>('old');
const oldCustPhone = ref('');
const searchOldCustError = ref(false);
const oldCustData = ref<any>(null);
const oldCustPets = ref<any[]>([]);

const newCustForm = ref({
  customerName: '',
  customerPhone: '',
  petName: '',
  species: 'Chó',
  petWeight: null as number | null
});

const formPayload = ref({
  petId: '',
  doctorId: '',
  serviceId: '',
  dateOnly: new Date().toISOString().slice(0, 10),
  timeOnly: '',
  symptom: '',
  note: ''
});

const workingHours = [
  "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30",
  "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30", "18:00", "18:30"
];

// Flow board groupings
const flowWaiting = ref<any[]>([]);
const flowInProgress = ref<any[]>([]);
const flowCompleted = ref<any[]>([]);

// Fetch functions
const loadStats = async () => {
  try {
    const res = await api.get('/appointment/stats');
    stats.value = res.data;
  } catch (err) {
    console.error(err);
  }
};

const loadDoctors = async () => {
  try {
    const res = await api.get('/receptionist/doctors');
    doctorList.value = res.data;
  } catch (err) {
    console.error(err);
  }
};

const loadServices = async () => {
  try {
    const res = await api.get('/appointment/services');
    serviceList.value = res.data;
  } catch (err) {
    console.error(err);
  }
};

const loadEvents = async () => {
  loadingEvents.value = true;
  try {
    // start is start of day, end is end of day
    const startStr = `${selectedDate.value}T00:00:00`;
    const endStr = `${selectedDate.value}T23:59:59`;
    const docParam = selectedDoctor.value === 'ALL' ? '' : `&doctorId=${selectedDoctor.value}`;
    const res = await api.get(`/appointment/events?start=${startStr}&end=${endStr}${docParam}`);
    eventsList.value = res.data || [];
  } catch (err) {
    console.error(err);
  } finally {
    loadingEvents.value = false;
  }
};

const loadPending = async () => {
  try {
    const res = await api.get('/appointment/pending');
    pendingList.value = res.data || [];
  } catch (err) {
    console.error(err);
  }
};

const loadFlowBoard = async () => {
  try {
    const res = await api.get('/receptionist/queue');
    flowList.value = res.data || [];
    
    // Group them
    flowWaiting.value = flowList.value.filter(item => item.status === 'waiting');
    flowInProgress.value = flowList.value.filter(item => item.status === 'in_progress');
    flowCompleted.value = flowList.value.filter(item => item.status === 'ready_to_pay' || item.status === 'completed');
  } catch (err) {
    console.error(err);
  }
};

const loadAllData = async () => {
  await Promise.all([
    loadStats(),
    loadEvents(),
    loadPending(),
    loadFlowBoard()
  ]);
};

// Form methods
const searchOldCustomer = async () => {
  const phone = oldCustPhone.value.trim();
  if (!phone) return;
  searchOldCustError.value = false;
  oldCustData.value = null;
  oldCustPets.value = [];
  try {
    const res = await api.get(`/receptionist/customer-by-phone?phone=${encodeURIComponent(phone)}`);
    if (res.data.success) {
      oldCustData.value = res.data;
      oldCustPets.value = res.data.pets || [];
    } else {
      searchOldCustError.value = true;
    }
  } catch (err) {
    console.error(err);
    searchOldCustError.value = true;
  }
};

const openCreateModal = () => {
  createAptType.value = 'old';
  oldCustPhone.value = '';
  searchOldCustError.value = false;
  oldCustData.value = null;
  oldCustPets.value = [];
  newCustForm.value = {
    customerName: '',
    customerPhone: '',
    petName: '',
    species: 'Chó',
    petWeight: null
  };
  formPayload.value = {
    petId: '',
    doctorId: '',
    serviceId: '',
    dateOnly: new Date().toISOString().slice(0, 10),
    timeOnly: '',
    symptom: '',
    note: ''
  };
  showCreateModal.value = true;
};

const submitCreateAppointment = async () => {
  if (!formPayload.value.timeOnly) {
    showToast('Vui lòng chọn khung giờ hẹn khám!', 'warning');
    return;
  }

  const fullDateTime = `${formPayload.value.dateOnly}T${formPayload.value.timeOnly}:00`;

  try {
    if (createAptType.value === 'old') {
      const payload = {
        customerId: oldCustData.value.customer.id,
        petId: formPayload.value.petId,
        doctorId: formPayload.value.doctorId,
        serviceId: formPayload.value.serviceId,
        appointmentDate: fullDateTime,
        symptom: formPayload.value.symptom,
        note: formPayload.value.note
      };
      const res = await api.post('/appointment', payload);
      if (res.data.success) {
        showToast('Tạo lịch hẹn thành công!', 'success');
        showCreateModal.value = false;
        await loadAllData();
      }
    } else {
      const payload = {
        customerName: newCustForm.value.customerName,
        customerPhone: newCustForm.value.customerPhone,
        petName: newCustForm.value.petName,
        species: newCustForm.value.species,
        petWeight: newCustForm.value.petWeight,
        doctorId: formPayload.value.doctorId,
        serviceId: formPayload.value.serviceId,
        appointmentDate: fullDateTime,
        symptom: formPayload.value.symptom,
        note: formPayload.value.note
      };
      const res = await api.post('/appointment/with-new-customer', payload);
      if (res.data.success) {
        showToast('Đăng ký khách mới và tạo lịch hẹn thành công!', 'success');
        showCreateModal.value = false;
        await loadAllData();
      }
    }
  } catch (err: any) {
    showToast(err.response?.data?.message || 'Lỗi khi tạo lịch hẹn.', 'danger');
  }
};

// Detail modal
const openDetailModal = async (apptId: number) => {
  showDetailModal.value = true;
  detailLoading.value = true;
  selectedDetail.value = null;
  try {
    const res = await api.get(`/appointment/${apptId}`);
    selectedDetail.value = res.data;
  } catch (err) {
    console.error(err);
    showToast('Không thể tải chi tiết lịch hẹn.', 'danger');
    showDetailModal.value = false;
  } finally {
    detailLoading.value = false;
  }
};

const updateStatus = async (apptId: number, status: string) => {
  try {
    const res = await api.put(`/appointment/${apptId}/status`, { status });
    if (res.data.success) {
      showToast('Cập nhật trạng thái thành công!', 'success');
      if (showDetailModal.value) {
        showDetailModal.value = false;
      }
      await loadAllData();
    } else {
      showToast('Không thể cập nhật trạng thái.', 'danger');
    }
  } catch (err) {
    console.error(err);
    showToast('Đã xảy ra lỗi.', 'danger');
  }
};

// UI helpers
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

const formatDateFull = (dateStr: string) => {
  if (!dateStr) return '';
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

const getLastWord = (name: string) => {
  if (!name) return '—';
  const parts = name.trim().split(/\s+/);
  return parts[parts.length - 1];
};

const getAnimalEmoji = (species: string) => {
  const s = (species || '').toLowerCase();
  if (s.includes('chó') || s.includes('dog')) return '🐶';
  if (s.includes('mèo') || s.includes('cat')) return '🐱';
  return '🐾';
};

const getStatusBadgeClass = (status: string) => {
  const s = (status || '').toLowerCase();
  if (s === 'pending') return 'bg-warning text-dark';
  if (s === 'confirmed') return 'bg-info text-white';
  if (s === 'waiting') return 'bg-primary text-white';
  if (s === 'in_progress') return 'bg-warning text-dark';
  if (s === 'ready_to_pay' || s === 'completed') return 'bg-success text-white';
  if (s === 'cancelled') return 'bg-danger text-white';
  return 'bg-secondary text-white';
};

const getStatusLabel = (status: string) => {
  const s = (status || '').toLowerCase();
  if (s === 'pending') return 'Chờ duyệt';
  if (s === 'confirmed') return 'Xác nhận';
  if (s === 'waiting') return 'Chờ khám';
  if (s === 'in_progress') return 'Đang khám';
  if (s === 'ready_to_pay') return 'Chờ thanh toán';
  if (s === 'completed') return 'Hoàn thành';
  if (s === 'cancelled') return 'Đã hủy';
  return status;
};

const getWaitingTimeText = (card: any) => {
  if (!card.checkInTime) return '0 phút';
  const checkIn = new Date(card.checkInTime);
  const diffMins = Math.max(0, Math.floor((new Date().getTime() - checkIn.getTime()) / 60000));
  return `Chờ ${diffMins} phút`;
};

onMounted(() => {
  loadDoctors();
  loadServices();
  loadAllData();
});
</script>

<script lang="ts">
export default {
  name: 'AppointmentsTab'
}
</script>

<style scoped>
.flow-card-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.flow-patient-card {
  transition: var(--transition-smooth);
}
.flow-patient-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md) !important;
}

.border-start-primary { border-left: 5px solid #3b82f6 !important; }
.border-start-warning { border-left: 5px solid #ffb020 !important; }
.border-start-success { border-left: 5px solid #22c55e !important; }

.cursor-pointer {
  cursor: pointer;
}

.nav-tabs .nav-link {
  color: var(--text-muted);
}
.nav-tabs .nav-link.active {
  color: white !important;
  background-color: var(--primary-gold) !important;
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.25);
}

.time-slot-btn {
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
