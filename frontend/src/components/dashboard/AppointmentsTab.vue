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
          <button class="btn btn-outline-warning text-dark rounded-pill px-4 fw-bold shadow-sm me-2" @click="openQrScanModal">
            <i class="bi bi-qr-code-scan me-1"></i> Quét mã QR
          </button>
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
                      <th class="text-center">Thao tác</th>
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
                      <td class="text-center">
                        <div class="dropdown">
                          <button class="btn btn-sm btn-light border rounded-pill" type="button" data-bs-toggle="dropdown" aria-expanded="false" @click.stop>
                            <i class="bi bi-three-dots-vertical"></i>
                          </button>
                          <ul class="dropdown-menu dropdown-menu-end shadow-sm border-0">
                            <li><a class="dropdown-item" href="#" @click.prevent.stop="openDetailModal(evt.id)"><i class="bi bi-eye text-primary me-2"></i>Xem chi tiết</a></li>
                            
                            <!-- Change Doctor -->
                            <li v-if="['pending', 'confirmed', 'checked_in'].includes(evt.status)"><a class="dropdown-item" href="#" @click.prevent.stop="openChangeDoctorModal(evt)"><i class="bi bi-person-hearts text-info me-2"></i>Điều phối bác sĩ</a></li>
                            
                            <!-- Reschedule -->
                            <li v-if="['pending', 'confirmed'].includes(evt.status)"><a class="dropdown-item" href="#" @click.prevent.stop="openRescheduleModal(evt)"><i class="bi bi-calendar-range text-warning me-2"></i>Dời lịch khám</a></li>
                            
                            <!-- No show -->
                            <li v-if="['pending', 'confirmed'].includes(evt.status) && isPastDue(evt)"><a class="dropdown-item" href="#" @click.prevent.stop="markNoShow(evt.id)"><i class="bi bi-person-x text-secondary me-2"></i>Khách vắng mặt</a></li>
                            
                            <!-- Cancel -->
                            <li v-if="['pending', 'confirmed'].includes(evt.status)"><hr class="dropdown-divider"></li>
                            <li v-if="['pending', 'confirmed'].includes(evt.status)"><a class="dropdown-item text-danger" href="#" @click.prevent.stop="openCancelModal(evt)"><i class="bi bi-x-circle me-2"></i>Hủy lịch</a></li>
                          </ul>
                        </div>
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
    <!-- Cancel Modal -->
    <Teleport to="body">
      <div v-if="showCancelModal" class="modal-backdrop fade show"></div>
      <div v-if="showCancelModal" class="modal fade show d-block" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content border-0 rounded-4 shadow-lg">
            <div class="modal-header border-0 pb-0">
              <h5 class="fw-bold"><i class="bi bi-exclamation-triangle text-danger me-2"></i>Hủy lịch hẹn</h5>
              <button type="button" class="btn-close" @click="showCancelModal = false"></button>
            </div>
            <div class="modal-body pt-3">
              <p class="text-muted mb-3">Bạn đang hủy lịch hẹn của khách hàng <strong>{{ cancelTarget?.customerName }}</strong> cho bé <strong>{{ cancelTarget?.petName }}</strong>.</p>
              <div class="mb-3">
                <label class="form-label fw-bold small text-muted">Lý do hủy (Bắt buộc)</label>
                <textarea v-model="cancelReason" class="form-control rounded-3" rows="3" placeholder="Nhập lý do khách hủy hoặc lý do phòng khám..."></textarea>
              </div>
            </div>
            <div class="modal-footer border-0 pt-0">
              <button type="button" class="btn btn-light rounded-pill px-4" @click="showCancelModal = false">Đóng</button>
              <button type="button" class="btn btn-danger rounded-pill px-4 fw-bold" @click="confirmCancel" :disabled="!cancelReason.trim()">Xác nhận Hủy</button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Change Doctor Modal -->
    <Teleport to="body">
      <div v-if="showChangeDoctorModal" class="modal-backdrop fade show"></div>
      <div v-if="showChangeDoctorModal" class="modal fade show d-block" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content border-0 rounded-4 shadow-lg">
            <div class="modal-header border-0 pb-0">
              <h5 class="fw-bold"><i class="bi bi-person-hearts text-info me-2"></i>Điều phối Bác sĩ</h5>
              <button type="button" class="btn-close" @click="showChangeDoctorModal = false"></button>
            </div>
            <div class="modal-body pt-3">
              <p class="text-muted mb-3">Ca khám: <strong>{{ formatTimeOnly(changeDoctorTarget?.appointmentTime || changeDoctorTarget?.start) }}</strong> - <strong>{{ changeDoctorTarget?.petName }}</strong></p>
              <p class="text-muted mb-3">Bác sĩ hiện tại: <strong>Bs. {{ getLastWord(changeDoctorTarget?.doctorName) }}</strong></p>
              
              <div class="mb-3">
                <label class="form-label fw-bold small text-muted">Chọn bác sĩ thay thế</label>
                <select v-model="selectedNewDoctorId" class="form-select rounded-3">
                  <option value="" disabled>-- Chọn bác sĩ --</option>
                  <option v-for="doc in doctorList.filter(d => d.id !== changeDoctorTarget?.extendedProps?.doctorId && d.id !== changeDoctorTarget?.doctorId)" :key="doc.id" :value="doc.id">
                    Bs. {{ doc.fullName }}
                  </option>
                </select>
              </div>

              <div class="form-check form-switch mt-3">
                <input class="form-check-input" type="checkbox" role="switch" id="forceChangeDoctor" v-model="forceChangeDoctor">
                <label class="form-check-label text-muted small" for="forceChangeDoctor">Ép buộc chuyển ca (Bypass trùng lịch - chỉ dùng khi khẩn cấp)</label>
              </div>

            </div>
            <div class="modal-footer border-0 pt-0">
              <button type="button" class="btn btn-light rounded-pill px-4" @click="showChangeDoctorModal = false">Đóng</button>
              <button type="button" class="btn btn-info text-white rounded-pill px-4 fw-bold" @click="confirmChangeDoctor" :disabled="!selectedNewDoctorId">Xác nhận chuyển</button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Reschedule Modal -->
    <Teleport to="body">
      <div v-if="showRescheduleModal" class="modal-backdrop fade show"></div>
      <div v-if="showRescheduleModal" class="modal fade show d-block" tabindex="-1">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content border-0 rounded-4 shadow-lg">
            <div class="modal-header border-0 pb-0">
              <h5 class="fw-bold"><i class="bi bi-calendar-range text-warning me-2"></i>Dời Lịch Khám</h5>
              <button type="button" class="btn-close" @click="showRescheduleModal = false"></button>
            </div>
            <div class="modal-body pt-3">
              <p class="text-muted mb-3">Đang dời lịch cho bé <strong>{{ rescheduleTarget?.petName }}</strong> - Khách hàng <strong>{{ rescheduleTarget?.customerName }}</strong></p>
              
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label fw-bold small text-muted">Ngày khám mới</label>
                  <input type="date" class="form-control" v-model="rescheduleDate" @change="fetchRescheduleSlots">
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-bold small text-muted">Giờ khám mới</label>
                  <select class="form-select" v-model="rescheduleTime" :disabled="!rescheduleDate || loadingSlots">
                    <option value="" disabled>-- Chọn giờ --</option>
                    <option v-for="slot in availableSlots" :key="slot" :value="slot">{{ slot }}</option>
                  </select>
                </div>
              </div>

              <div v-if="loadingSlots" class="mt-2 small text-muted text-center"><i class="spinner-border spinner-border-sm me-1"></i> Đang tải lịch trống...</div>
              <div v-else-if="rescheduleDate && availableSlots.length === 0" class="mt-2 small text-danger text-center"><i class="bi bi-exclamation-circle me-1"></i> Bác sĩ không có giờ trống trong ngày này.</div>

              <div class="form-check form-switch mt-4">
                <input class="form-check-input" type="checkbox" role="switch" id="forceReschedule" v-model="forceReschedule">
                <label class="form-check-label text-muted small" for="forceReschedule">Dời lùi lịch (Bypass chặn giờ quá khứ / sát giờ)</label>
              </div>
            </div>
            <div class="modal-footer border-0 pt-0">
              <button type="button" class="btn btn-light rounded-pill px-4" @click="showRescheduleModal = false">Đóng</button>
              <button type="button" class="btn btn-warning text-dark rounded-pill px-4 fw-bold" @click="confirmReschedule" :disabled="!rescheduleDate || !rescheduleTime">Xác nhận Dời</button>
            </div>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- QR Checkin Dialog -->
    <Teleport to="body">
      <div v-if="showQrModal" class="zalo-modal-overlay d-flex align-items-center justify-content-center" @click.self="closeQrModal">
        <div class="zalo-modal-card border-0 shadow-lg" style="max-width: 420px; width: 100%; margin: 0 auto; border-radius: 16px; overflow: hidden;">
          <div class="zalo-modal-header bg-success bg-gradient text-white p-3 border-0 d-flex justify-content-between align-items-center">
            <h5 class="modal-title fw-bold mb-0"><i class="bi bi-qr-code-scan me-2"></i> Quét Mã Check-in</h5>
            <button class="modal-close text-white border-0 bg-transparent" @click="closeQrModal"><i class="bi bi-x-lg fs-5"></i></button>
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
    </Teleport>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue';
import api from '../../services/api';
import { Html5Qrcode } from 'html5-qrcode';

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
const showCustomerModal = ref(false);
const showQrModal = ref(false);
const previewAppointment = ref<any>(null);
const qrManualCode = ref('');
const selectedDetail = ref<any>(null);

// Cancel Modal
const showCancelModal = ref(false);
const cancelTarget = ref<any>(null);
const cancelReason = ref('');

// Change Doctor Modal
const showChangeDoctorModal = ref(false);
const changeDoctorTarget = ref<any>(null);
const selectedNewDoctorId = ref('');
const forceChangeDoctor = ref(false);

// Reschedule Modal
const showRescheduleModal = ref(false);
const rescheduleTarget = ref<any>(null);
const rescheduleDate = ref('');
const rescheduleTime = ref('');
const forceReschedule = ref(false);
const availableSlots = ref<string[]>([]);
const loadingSlots = ref(false);

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
    const rawEvents = res.data || [];
    eventsList.value = rawEvents.map((e: any) => ({
      ...e,
      ...(e.extendedProps || {}),
      customerPhone: e.extendedProps?.phone
    }));
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

let html5QrCode: Html5Qrcode | null = null;

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

const openQrScanModal = () => {
  qrManualCode.value = '';
  previewAppointment.value = null;
  showQrModal.value = true;
  startScanner();
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
      showToast(res.data.message || 'Lỗi kiểm tra mã', 'danger');
      startScanner();
    }
  } catch (err: any) {
    showToast(err.response?.data?.message || 'Không thể kiểm tra mã. Vui lòng thử lại.', 'danger');
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
      showToast(res.data.message || 'Check-in thành công', 'success');
      showQrModal.value = false;
      previewAppointment.value = null;
      qrManualCode.value = '';
      await loadEvents();
    } else {
      showToast(res.data.message || 'Check-in thất bại', 'danger');
    }
  } catch (err: any) {
    showToast(err.response?.data?.message || 'Không thể check-in. Vui lòng kiểm tra lại mã.', 'danger');
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
  // detailLoading.value = true; // Placeholder for loading state if needed
  selectedDetail.value = null;
  try {
    const res = await api.get(`/appointment/${apptId}`);
    selectedDetail.value = res.data;
  } catch (err) {
    console.error(err);
    showToast('Không thể tải chi tiết lịch hẹn.', 'danger');
    showDetailModal.value = false;
  } finally {
    // detailLoading.value = false;
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

const isPastDue = (evt: any) => {
  if (!evt.start && !evt.appointmentTime) return false;
  const aptDate = new Date(evt.appointmentTime || evt.start);
  return aptDate.getTime() < new Date().getTime();
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
  switch (s) {
    case 'pending': return 'Chờ duyệt';
    case 'confirmed': return 'Xác nhận';
    case 'waiting': return 'Chờ khám';
    case 'in_progress': return 'Đang khám';
    case 'ready_to_pay': return 'Chờ thanh toán';
    case 'completed': return 'Đã xong';
    case 'cancelled': return 'Đã hủy';
    case 'no_show': return 'Vắng mặt';
    default: return status;
  }
};

const getWaitingTimeText = (card: any) => {
  if (!card.checkInTime) return '0 phút';
  const checkIn = new Date(card.checkInTime);
  const diffMins = Math.max(0, Math.floor((new Date().getTime() - checkIn.getTime()) / 60000));
  return `Chờ ${diffMins} phút`;
};

// Cancel, NoShow, Reschedule, ChangeDoctor
const openCancelModal = (evt: any) => {
  cancelTarget.value = evt;
  cancelReason.value = '';
  showCancelModal.value = true;
};

const confirmCancel = async () => {
  if (!cancelTarget.value || !cancelReason.value.trim()) return;
  try {
    const res = await api.put(`/appointment/${cancelTarget.value.id}/status`, {
      status: 'cancelled',
      reason: cancelReason.value.trim()
    });
    if (res.data.success) {
      showToast('Đã hủy lịch hẹn!', 'success');
      showCancelModal.value = false;
      loadEvents();
      loadPending();
    }
  } catch (err) {
    showToast('Lỗi khi hủy lịch', 'danger');
  }
};

const markNoShow = async (id: number) => {
  if (!confirm('Đánh dấu khách hàng này vắng mặt (No-show)?')) return;
  try {
    const res = await api.put(`/appointment/${id}/status`, { status: 'no_show' });
    if (res.data.success) {
      showToast('Đã đánh dấu vắng mặt', 'success');
      loadEvents();
      loadPending();
    }
  } catch (err) {
    showToast('Lỗi thao tác', 'danger');
  }
};

const openRescheduleModal = (evt: any) => {
  rescheduleTarget.value = evt;
  const oldDate = new Date(evt.appointmentTime || evt.start);
  rescheduleDate.value = oldDate.toISOString().slice(0, 10);
  rescheduleTime.value = '';
  forceReschedule.value = false;
  availableSlots.value = [];
  showRescheduleModal.value = true;
  fetchRescheduleSlots();
};

const fetchRescheduleSlots = async () => {
  if (!rescheduleDate.value || !rescheduleTarget.value) return;
  loadingSlots.value = true;
  try {
    const res = await api.get(`/appointment/available-slots?date=${rescheduleDate.value}`);
    const doctorId = rescheduleTarget.value.extendedProps?.doctorId || rescheduleTarget.value.doctorId;
    
    // Find slots for this specific doctor
    const docData = res.data.find((d: any) => d.doctorId === doctorId);
    if (docData && docData.availableSlots) {
      availableSlots.value = docData.availableSlots;
    } else {
      availableSlots.value = [];
    }
  } catch (err) {
    showToast('Lỗi khi tải lịch trống', 'danger');
    availableSlots.value = [];
  } finally {
    loadingSlots.value = false;
  }
};

const confirmReschedule = async () => {
  if (!rescheduleTarget.value || !rescheduleDate.value || !rescheduleTime.value) return;
  try {
    const newStart = `${rescheduleDate.value}T${rescheduleTime.value}:00`;
    const res = await api.put(`/appointment/${rescheduleTarget.value.id}/reschedule`, {
      newStart: newStart,
      force: forceReschedule.value
    });
    if (res.data.success) {
      showToast('Đã dời lịch khám thành công!', 'success');
      showRescheduleModal.value = false;
      loadEvents();
      loadPending();
    }
  } catch (err: any) {
    const msg = err.response?.data?.message || 'Lỗi khi dời lịch';
    showToast(msg, 'danger');
  }
};

const openChangeDoctorModal = (evt: any) => {
  changeDoctorTarget.value = evt;
  selectedNewDoctorId.value = '';
  forceChangeDoctor.value = false;
  showChangeDoctorModal.value = true;
};

const confirmChangeDoctor = async () => {
  if (!changeDoctorTarget.value || !selectedNewDoctorId.value) return;
  try {
    const res = await api.put(`/appointment/${changeDoctorTarget.value.id}/doctor`, {
      doctorId: selectedNewDoctorId.value,
      force: forceChangeDoctor.value
    });
    if (res.data.success) {
      showToast('Đã chuyển bác sĩ phụ trách!', 'success');
      showChangeDoctorModal.value = false;
      loadEvents();
      loadPending();
    }
  } catch (err: any) {
    const msg = err.response?.data?.message || 'Lỗi khi chuyển đổi bác sĩ';
    showToast(msg, 'danger');
  }
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
