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
          <span class="text-muted small fw-bold">Trạng thái:</span>
          <select v-model="selectedStatus" class="form-select border-warning rounded-pill px-3 py-1.5 shadow-sm" style="width: 160px;">
            <option value="ALL">Tất cả</option>
            <option value="pending">Chờ xác nhận</option>
            <option value="confirmed">Đã xác nhận</option>
            <option value="waiting">Chờ khám</option>
            <option value="in_progress">Đang khám</option>
            <option value="ready_to_pay">Chờ thanh toán</option>
            <option value="completed">Hoàn thành</option>
            <option value="cancelled">Đã hủy</option>
          </select>
          <span class="text-muted small fw-bold">Bác sĩ:</span>
          <select v-model="selectedDoctor" @change="loadEvents" class="form-select border-warning rounded-pill px-3 py-1.5 shadow-sm" style="width: 180px;">
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
    <ul class="nav nav-tabs border-bottom-0 mb-4 bg-white p-2 rounded-4 shadow-sm" style="font-family: 'Be Vietnam Pro', sans-serif;">
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
              <div v-else class="table-responsive rounded-4 border" style="min-height: 350px; overflow-y: visible; padding-bottom: 150px;">
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
                      v-for="evt in paginatedEventsList" 
                      :key="evt.id" 
                      class="cursor-pointer" 
                      @click="handleRowClick($event, evt.id)"
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
                          <button class="btn btn-sm btn-light border rounded-pill" type="button" @click.stop="toggleDropdown(evt.id)">
                            <i class="bi bi-three-dots-vertical"></i>
                          </button>
                          <ul class="dropdown-menu dropdown-menu-end shadow-sm border-0" :class="{ 'show': activeDropdownId === evt.id }" style="position: absolute; right: 0; z-index: 1000; margin-top: 5px;">
                            <li><a class="dropdown-item" href="#" @click.prevent.stop="openDetailModal(evt.id)"><i class="bi bi-eye text-primary me-2"></i>Xem chi tiết</a></li>
                            
                            <!-- Change Doctor -->
                            <li v-if="['pending', 'confirmed', 'waiting'].includes(evt.status)"><a class="dropdown-item" href="#" @click.prevent.stop="openChangeDoctorModal(evt)"><i class="bi bi-person-hearts text-info me-2"></i>Điều phối bác sĩ</a></li>
                            
                            <!-- Reschedule -->
                            <li v-if="['pending', 'confirmed', 'waiting'].includes(evt.status)"><a class="dropdown-item" href="#" @click.prevent.stop="openRescheduleModal(evt)"><i class="bi bi-calendar-range text-warning me-2"></i>Dời lịch khám</a></li>
                            
                            <!-- No show -->
                            <li v-if="['pending', 'confirmed', 'waiting'].includes(evt.status) && isPastDue(evt)"><a class="dropdown-item" href="#" @click.prevent.stop="markNoShow(evt.id)"><i class="bi bi-person-x text-secondary me-2"></i>Khách vắng mặt</a></li>
                            
                            <!-- Cancel -->
                            <li v-if="['pending', 'confirmed', 'waiting'].includes(evt.status)"><hr class="dropdown-divider"></li>
                            <li v-if="['pending', 'confirmed', 'waiting'].includes(evt.status)"><a class="dropdown-item text-danger" href="#" @click.prevent.stop="openCancelModal(evt)"><i class="bi bi-x-circle me-2"></i>Hủy lịch</a></li>
                          </ul>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <!-- Pagination Control -->
              <div class="d-flex justify-content-between align-items-center mt-3" v-if="totalPages > 1">
                <span class="text-muted small">Hiển thị {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, filteredEventsList.length) }} trong tổng số {{ filteredEventsList.length }} ca khám</span>
                <nav>
                  <ul class="pagination pagination-sm mb-0">
                    <li class="page-item" :class="{ disabled: currentPage === 1 }">
                      <button class="page-link text-warning" @click="currentPage--">Trước</button>
                    </li>
                    <li class="page-item" v-for="page in totalPages" :key="page" :class="{ active: currentPage === page }">
                      <button class="page-link" :class="currentPage === page ? 'bg-warning border-warning text-dark' : 'text-dark'" @click="currentPage = page">{{ page }}</button>
                    </li>
                    <li class="page-item" :class="{ disabled: currentPage === totalPages }">
                      <button class="page-link text-warning" @click="currentPage++">Sau</button>
                    </li>
                  </ul>
                </nav>
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
                    <button class="btn btn-sm btn-success rounded-pill px-3 fw-bold" @click="openConfirmApprove(item)">
                      <i class="bi bi-check2"></i> Duyệt
                    </button>
                    <button class="btn btn-sm btn-outline-danger rounded-pill px-3" @click="openConfirmReject(item)">
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


    </div>

    <!-- Modal 1: Create Appointment Modal -->
    <div v-if="showCreateModal" class="zalo-modal-overlay" @click.self="showCreateModal = false">
      <div class="zalo-modal-card modal-lg max-w-700">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold"><i class="bi bi-calendar-plus me-2"></i> Tạo Lịch Hẹn Khám Mới</h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showCreateModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <!-- Tab pills for booking type -->
          <ul class="nav nav-pills nav-fill mb-4 gap-2 border p-1.5 rounded-pill bg-light" role="tablist">
            <li class="nav-item">
              <button class="nav-link rounded-pill fw-bold border-0" :class="{ 'active': activeBookingTab === 'prebooked' }" @click="activeBookingTab = 'prebooked'">
                <i class="bi bi-calendar-check me-2"></i>Đặt Lịch Khám Trước
              </button>
            </li>
            <li class="nav-item">
              <button class="nav-link rounded-pill fw-bold border-0" :class="{ 'active': activeBookingTab === 'walkin', 'bg-success text-white': activeBookingTab === 'walkin' }" @click="activeBookingTab = 'walkin'">
                <i class="bi bi-person-walking me-2"></i>Khách Vãng Lai (Walk-in)
              </button>
            </li>
          </ul>

          <form @submit.prevent="activeBookingTab === 'prebooked' ? submitPrebookedAppointment() : submitWalkInAppointment()">
            <!-- Part 1: Select Owner (Shared for both tabs) -->
            <div class="mb-4">
              <div class="bg-light p-3 rounded-4 border border-primary border-opacity-20 mb-3">
                <label class="form-label fw-bold small text-muted">Tìm kiếm SĐT khách hàng *</label>
                <div class="input-group">
                  <span class="input-group-text bg-white border-end-0"><i class="bi bi-search text-muted"></i></span>
                  <input type="text" v-model="searchQueryPhone" class="form-control border-start-0" placeholder="Nhập SĐT để tìm hoặc đăng ký mới..." @keyup.enter="handleSearchCustomer" />
                  <button type="button" class="btn btn-primary fw-bold" @click="handleSearchCustomer">Tìm / Đăng ký</button>
                </div>
                
                <div v-if="searchCustomerStatus === 'idle'" class="mt-2 text-start">
                  <button type="button" class="btn btn-link text-decoration-none small p-0 text-primary fw-bold" @click="searchCustomerStatus = 'not_found'; customerForm.customerPhone = searchQueryPhone">
                    <i class="bi bi-person-plus-fill me-1"></i> Bỏ qua tìm kiếm, đăng ký khách mới ngay
                  </button>
                </div>

                <small v-if="searchCustomerStatus === 'not_found'" class="text-warning mt-2 d-block fw-bold"><i class="bi bi-info-circle me-1"></i>Vui lòng điền thông tin khách mới bên dưới.</small>
              </div>

              <!-- Found Customer -->
              <div v-if="searchCustomerStatus === 'found'" class="bg-success bg-opacity-10 p-3 rounded-4 border border-success border-opacity-25 animate-fade-in">
                <div class="row align-items-start">
                  <div class="col-md-6 mb-3 mb-md-0">
                    <label class="form-label fw-bold small text-success">Thông tin khách hàng</label>
                    <div class="d-flex align-items-center bg-white p-2.5 rounded-3 border border-success border-opacity-25">
                      <div class="me-3 bg-success bg-opacity-25 p-2 rounded-circle text-success"><i class="bi bi-person-fill"></i></div>
                      <div>
                        <div class="fw-bold text-dark">{{ selectedCustomer?.fullName }}</div>
                        <div class="small text-muted">{{ selectedCustomer?.phone }}</div>
                      </div>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-bold small text-success d-flex justify-content-between align-items-center">
                      <span>Chọn thú cưng *</span>
                      <button type="button" class="btn btn-sm btn-outline-success rounded-pill py-0 px-2" style="font-size: 0.75rem" @click="isAddingNewPet = !isAddingNewPet">
                        <i class="bi" :class="isAddingNewPet ? 'bi-x' : 'bi-plus-lg'"></i> {{ isAddingNewPet ? 'Hủy thêm' : 'Thêm bé mới' }}
                      </button>
                    </label>
                    <select v-if="!isAddingNewPet" v-model="formPayload.petId" class="form-select border-success" :required="!isAddingNewPet">
                      <option value="">-- Chọn thú cưng --</option>
                      <option v-for="pet in customerPets" :key="pet.id" :value="pet.id">{{ pet.name }} ({{ pet.species }})</option>
                    </select>

                    <!-- Add new pet for old customer -->
                    <div v-if="isAddingNewPet" class="bg-white p-3 rounded-3 border border-success border-opacity-25 mt-2 animate-fade-in">
                      <div class="row g-2">
                        <div class="col-12">
                          <input type="text" v-model="petForm.petName" class="form-control form-control-sm" placeholder="Tên thú cưng *" :required="isAddingNewPet" />
                        </div>
                        <div class="col-6">
                          <select v-model="petForm.species" class="form-select form-select-sm">
                            <option value="Chó">Chó</option>
                            <option value="Mèo">Mèo</option>
                            <option value="Khác">Khác</option>
                          </select>
                        </div>
                        <div class="col-6">
                          <input type="number" step="0.1" v-model="petForm.petWeight" class="form-control form-control-sm" placeholder="Cân nặng (kg)" />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- New Customer / Not Found -->
              <div v-if="searchCustomerStatus === 'not_found'" class="bg-warning bg-opacity-10 p-3 rounded-4 border border-warning border-opacity-35 animate-fade-in">
                <div class="row g-3">
                  <div class="col-md-6">
                    <label class="form-label fw-bold small text-dark-gold">Số điện thoại *</label>
                    <input type="text" v-model="customerForm.customerPhone" class="form-control border-warning border-opacity-50" required placeholder="VD: 0901234567" />
                  </div>
                  <div class="col-md-6">
                    <label class="form-label fw-bold small text-dark-gold">Tên chủ nuôi *</label>
                    <input type="text" v-model="customerForm.customerName" class="form-control border-warning border-opacity-50" required placeholder="VD: Nguyễn Văn A" />
                  </div>
                  <div class="col-md-4">
                    <label class="form-label fw-bold small text-dark-gold">Tên thú cưng *</label>
                    <input type="text" v-model="petForm.petName" class="form-control border-warning border-opacity-50" required placeholder="VD: Milo" />
                  </div>
                  <div class="col-md-4">
                    <label class="form-label fw-bold small text-dark-gold">Giống loài *</label>
                    <select v-model="petForm.species" class="form-select border-warning border-opacity-50">
                      <option value="Chó">Chó</option>
                      <option value="Mèo">Mèo</option>
                      <option value="Khác">Khác</option>
                    </select>
                  </div>
                  <div class="col-md-4">
                    <label class="form-label fw-bold small text-dark-gold">Cân nặng (kg)</label>
                    <input type="number" step="0.1" v-model="petForm.petWeight" class="form-control border-warning border-opacity-50" placeholder="VD: 5.5" />
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
                <label class="form-label fw-bold text-dark">Bác sĩ phụ trách</label>
                <div class="form-control border-primary bg-light text-muted fw-bold d-flex align-items-center">
                  <i class="bi bi-robot me-2 text-primary"></i> Hệ thống tự động phân công
                </div>
              </div>
            </div>

            <!-- Part 3: Time Slot Selector (ONLY FOR PRE-BOOKED) -->
            <div v-if="activeBookingTab === 'prebooked'" class="card border-primary border-opacity-25 shadow-sm rounded-4 mb-4 animate-fade-in">
              <div class="card-body">
                <h6 class="fw-bold text-primary mb-3"><i class="bi bi-clock me-2"></i>Chọn thời gian đặt hẹn</h6>
                <div class="row g-3">
                  <div class="col-md-4 border-end pe-md-3">
                    <label class="form-label fw-bold small text-muted">Ngày đặt lịch *</label>
                    <input type="date" v-model="formPayload.dateOnly" class="form-control border-primary" required />
                  </div>
                  <div class="col-md-8 ps-md-3">
                    <label class="form-label fw-bold small text-muted">Khung giờ làm việc còn trống *</label>
                    <div v-if="fetchingSlots" class="d-flex align-items-center gap-2 text-muted small mt-2">
                      <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
                      Đang tải khung giờ trống...
                    </div>
                    <div v-else-if="slotFetchError" class="text-danger small mt-2">{{ slotFetchError }}</div>
                    <div v-else-if="availableTimeSlots.length === 0" class="text-warning small fw-bold mt-2 d-flex align-items-center gap-1">
                      <i class="bi bi-exclamation-circle"></i> Hiện không có khung giờ làm việc nào còn trống. Bạn vui lòng chọn ngày khác!
                    </div>
                    <div v-else class="d-flex flex-column gap-3 mt-2">
                      <!-- Morning Slots -->
                      <div>
                        <h6 class="text-muted fw-bold mb-2 small" style="letter-spacing: 1px;"><i class="bi bi-brightness-alt-high me-1"></i> BUỔI SÁNG</h6>
                        <div class="d-flex flex-wrap gap-2">
                          <button
                            v-for="slot in displayMorningSlots"
                            :key="slot.time"
                            type="button"
                            class="time-slot-btn"
                            :class="{
                              'slot-selected': formPayload.timeOnly === slot.time,
                              'slot-past': slot.isPast,
                              'slot-too-soon': slot.isTooSoon,
                              'slot-booked': slot.isBooked,
                              'slot-available': slot.isAvailable
                            }"
                            :disabled="!slot.isAvailable"
                            :title="slot.isPast ? 'Giờ đã qua' : (slot.isTooSoon ? 'Cần đặt trước ít nhất 1 tiếng' : (slot.isBooked ? 'Khung giờ này đã được đặt' : ''))"
                            @click="slot.isAvailable && (formPayload.timeOnly = slot.time)"
                          >
                            <span class="slot-time-text">{{ slot.time }}</span>
                            <i v-if="formPayload.timeOnly === slot.time" class="bi bi-check-circle-fill ms-1"></i>
                            <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                            <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                            <span v-else-if="slot.isBooked" class="slot-badge-label">Đã đặt</span>
                          </button>
                        </div>
                      </div>
                      
                      <!-- Afternoon Slots -->
                      <div>
                        <h6 class="text-muted fw-bold mb-2 small" style="letter-spacing: 1px;"><i class="bi bi-brightness-alt-low me-1"></i> BUỔI CHIỀU</h6>
                        <div class="d-flex flex-wrap gap-2">
                          <button
                            v-for="slot in displayAfternoonSlots"
                            :key="slot.time"
                            type="button"
                            class="time-slot-btn"
                            :class="{
                              'slot-selected': formPayload.timeOnly === slot.time,
                              'slot-past': slot.isPast,
                              'slot-too-soon': slot.isTooSoon,
                              'slot-booked': slot.isBooked,
                              'slot-available': slot.isAvailable
                            }"
                            :disabled="!slot.isAvailable"
                            :title="slot.isPast ? 'Giờ đã qua' : (slot.isTooSoon ? 'Cần đặt trước ít nhất 1 tiếng' : (slot.isBooked ? 'Khung giờ này đã được đặt' : ''))"
                            @click="slot.isAvailable && (formPayload.timeOnly = slot.time)"
                          >
                            <span class="slot-time-text">{{ slot.time }}</span>
                            <i v-if="formPayload.timeOnly === slot.time" class="bi bi-check-circle-fill ms-1"></i>
                            <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                            <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                            <span v-else-if="slot.isBooked" class="slot-badge-label">Đã đặt</span>
                          </button>
                        </div>
                      </div>

                      <div class="mt-3 pt-2 border-top">
                        <p class="small text-muted mb-2 fw-semibold"><i class="bi bi-info-circle me-1"></i> Chú giải màu khung giờ:</p>
                        <div class="d-flex flex-wrap gap-2">
                          <span class="d-flex align-items-center gap-1 small"><span style="width:12px;height:12px;border-radius:3px;background:#fff;border:1.5px solid #dee2e6;display:inline-block"></span> <span class="text-muted">Trống</span></span>
                          <span class="d-flex align-items-center gap-1 small"><span style="width:12px;height:12px;border-radius:3px;background:#f8f9fa;border:1.5px solid #e9ecef;display:inline-block"></span> <span class="text-muted">Đã qua</span></span>
                          <span class="d-flex align-items-center gap-1 small"><span style="width:12px;height:12px;border-radius:3px;background:#fff8ec;border:1.5px solid #ffc107;display:inline-block"></span> <span class="text-muted">Quá gần</span></span>
                          <span class="d-flex align-items-center gap-1 small"><span style="width:12px;height:12px;border-radius:3px;background:#fff5f5;border:1.5px solid #fca5a5;display:inline-block"></span> <span class="text-muted">Đã đặt</span></span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="mb-4">
              <div class="form-floating">
                <textarea v-model="formPayload.symptom" class="form-control" style="height: 80px" placeholder="Lý do khám..." required></textarea>
                <label class="text-muted">Lý do khám bệnh / Triệu chứng *</label>
              </div>
            </div>

            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showCreateModal = false">Hủy</button>
              <button v-if="activeBookingTab === 'prebooked'" type="submit" class="btn btn-premium rounded-pill px-5 fw-bold">Xác Nhận Đặt Lịch</button>
              <button v-else type="submit" class="btn btn-success rounded-pill px-5 fw-bold shadow-sm"><i class="bi bi-play-fill me-1"></i>Đưa Vào Hàng Đợi Ngay</button>
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
                  <small class="text-muted d-block">Lý do khám bệnh</small>
                  <div class="p-2.5 bg-warning bg-opacity-10 border border-warning border-opacity-25 rounded text-dark small" style="min-height: 50px;">
                    {{ selectedDetail.symptom || 'Không có lý do khám bệnh ghi nhận' }}
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

            <!-- Confirmed: no actions in detail modal -->
            <template v-if="selectedDetail.status === 'confirmed'">
              <span class="badge bg-success fs-6 px-3 py-2.5 rounded-pill"><i class="bi bi-calendar-check"></i> Đã duyệt hẹn</span>
              <button class="btn btn-danger text-white rounded-pill px-3 fw-bold shadow-sm" @click="updateStatus(selectedDetail.id, 'in_progress')">
                <i class="bi bi-rocket-takeoff me-1"></i> Chuyển phòng khám (Bypass)
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
                <label class="form-label fw-bold small text-muted">Chọn bác sĩ thay thế <span v-if="fetchingSuitableDocs" class="spinner-border spinner-border-sm text-info ms-2" role="status"></span></label>
                <select v-model="selectedNewDoctorId" class="form-select rounded-3" :disabled="fetchingSuitableDocs">
                  <option value="" disabled>-- Chọn bác sĩ --</option>
                  <option v-for="doc in suitableDoctorsList.filter(d => d.id !== changeDoctorTarget?.extendedProps?.doctorId && d.id !== changeDoctorTarget?.doctorId)" :key="doc.id" :value="doc.id">
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
    <div v-if="showRescheduleModal" class="zalo-modal-overlay" @click.self="showRescheduleModal = false">
      <div class="zalo-modal-card modal-lg max-w-700">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold"><i class="bi bi-calendar-range me-2"></i>Dời Lịch Khám</h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showRescheduleModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <div class="alert alert-warning border-0 bg-warning bg-opacity-10 text-dark rounded-4 mb-4 d-flex align-items-center">
            <div class="my-1 rounded-circle bg-white d-flex align-items-center justify-content-center me-3" style="width: 45px; height: 45px;">
              <span class="fs-3">{{ getAnimalEmoji(rescheduleTarget?.species) }}</span>
            </div>
            <div>
              <div class="fw-bold fs-6">Bé {{ rescheduleTarget?.petName }}</div>
              <div class="small text-muted">Chủ nuôi: {{ rescheduleTarget?.customerName }}</div>
            </div>
          </div>
          
          <div class="card border-primary border-opacity-25 shadow-sm rounded-4 mb-4">
            <div class="card-body">
              <div class="row g-3">
                <div class="col-md-4 border-end pe-md-3">
                  <label class="form-label fw-bold small text-muted">Ngày khám mới *</label>
                  <input type="date" class="form-control border-warning" v-model="rescheduleDate" @change="fetchRescheduleSlots">
                </div>
                
                <div class="col-md-8 ps-md-3">
                  <label class="form-label fw-bold small text-muted">Khung giờ làm việc còn trống *</label>
                  <div v-if="loadingSlots" class="d-flex align-items-center gap-2 text-muted small mt-2">
                    <div class="spinner-border spinner-border-sm text-warning" role="status"></div>
                    Đang tải khung giờ trống...
                  </div>
                  <div v-else-if="rescheduleDate && availableSlots.length === 0" class="text-danger small fw-bold mt-2 d-flex align-items-center gap-1">
                    <i class="bi bi-exclamation-circle"></i> Bác sĩ không có giờ rảnh trong ngày này.
                  </div>
                  <div v-else class="d-flex flex-column gap-3 mt-2">
                    <!-- Morning Slots -->
                    <div v-if="displayRescheduleMorningSlots.length > 0">
                      <h6 class="text-muted fw-bold mb-2 small" style="letter-spacing: 1px;"><i class="bi bi-brightness-alt-high me-1"></i> BUỔI SÁNG</h6>
                      <div class="d-flex flex-wrap gap-2">
                        <button
                          v-for="slot in displayRescheduleMorningSlots"
                          :key="slot.time"
                          type="button"
                          class="time-slot-btn"
                          :class="{
                            'slot-selected': rescheduleTime === slot.time,
                            'slot-past': slot.isPast,
                            'slot-too-soon': slot.isTooSoon,
                            'slot-booked': slot.isBooked,
                            'slot-available': slot.isAvailable
                          }"
                          :disabled="!slot.isAvailable"
                          :title="slot.isPast ? 'Giờ đã qua' : (slot.isTooSoon ? 'Cần dời trước ít nhất 15 phút' : (slot.isBooked ? 'Khung giờ này đã được đặt hoặc ngoài giờ' : ''))"
                          @click="slot.isAvailable && (rescheduleTime = slot.time)"
                        >
                          <span class="slot-time-text">{{ slot.time }}</span>
                          <i v-if="rescheduleTime === slot.time" class="bi bi-check-circle-fill ms-1"></i>
                          <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                          <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                          <span v-else-if="slot.isBooked" class="slot-badge-label">Đã đặt</span>
                        </button>
                      </div>
                    </div>
                    
                    <!-- Afternoon Slots -->
                    <div v-if="displayRescheduleAfternoonSlots.length > 0">
                      <h6 class="text-muted fw-bold mb-2 small" style="letter-spacing: 1px;"><i class="bi bi-brightness-alt-low me-1"></i> BUỔI CHIỀU</h6>
                      <div class="d-flex flex-wrap gap-2">
                        <button
                          v-for="slot in displayRescheduleAfternoonSlots"
                          :key="slot.time"
                          type="button"
                          class="time-slot-btn"
                          :class="{
                            'slot-selected': rescheduleTime === slot.time,
                            'slot-past': slot.isPast,
                            'slot-too-soon': slot.isTooSoon,
                            'slot-booked': slot.isBooked,
                            'slot-available': slot.isAvailable
                          }"
                          :disabled="!slot.isAvailable"
                          :title="slot.isPast ? 'Giờ đã qua' : (slot.isTooSoon ? 'Cần dời trước ít nhất 15 phút' : (slot.isBooked ? 'Khung giờ này đã được đặt hoặc ngoài giờ' : ''))"
                          @click="slot.isAvailable && (rescheduleTime = slot.time)"
                        >
                          <span class="slot-time-text">{{ slot.time }}</span>
                          <i v-if="rescheduleTime === slot.time" class="bi bi-check-circle-fill ms-1"></i>
                          <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                          <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                          <span v-else-if="slot.isBooked" class="slot-badge-label">Đã đặt</span>
                        </button>
                      </div>
                    </div>

                    <div class="mt-2 pt-2 border-top" v-if="displayRescheduleMorningSlots.length > 0 || displayRescheduleAfternoonSlots.length > 0">
                      <p class="small text-muted mb-2 fw-semibold"><i class="bi bi-info-circle me-1"></i> Chú giải màu khung giờ:</p>
                      <div class="d-flex flex-wrap gap-2">
                        <span class="d-flex align-items-center gap-1 small"><span style="width:12px;height:12px;border-radius:3px;background:#fff;border:1.5px solid #dee2e6;display:inline-block"></span> <span class="text-muted">Trống</span></span>
                        <span class="d-flex align-items-center gap-1 small"><span style="width:12px;height:12px;border-radius:3px;background:#f8f9fa;border:1.5px solid #e9ecef;display:inline-block"></span> <span class="text-muted">Đã qua</span></span>
                        <span class="d-flex align-items-center gap-1 small"><span style="width:12px;height:12px;border-radius:3px;background:#fff8ec;border:1.5px solid #ffc107;display:inline-block"></span> <span class="text-muted">Quá gần</span></span>
                        <span class="d-flex align-items-center gap-1 small"><span style="width:12px;height:12px;border-radius:3px;background:#fff5f5;border:1.5px solid #fca5a5;display:inline-block"></span> <span class="text-muted">Đã đặt/Nghỉ</span></span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <div class="mt-4 pt-3 border-top text-end">
            <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showRescheduleModal = false">Hủy</button>
            <button type="button" class="btn btn-premium rounded-pill px-5 fw-bold" @click="confirmReschedule" :disabled="!rescheduleDate || !rescheduleTime">Xác nhận Dời</button>
          </div>
        </div>
      </div>
    </div>

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
                  <span class="fw-bold text-dark">{{ previewAppointment.startTime ? previewAppointment.startTime.substring(0, 5) : formatTimeOnly(previewAppointment.appointmentDate) }} - {{ formatDate(previewAppointment.appointmentDate) }}</span>
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

    <!-- Approve Confirm Modal -->
    <div v-if="showApproveModal" class="zalo-modal-overlay" @click.self="showApproveModal = false">
      <div class="zalo-modal-card border-0 shadow-lg" style="max-width: 450px; width: 100%; margin: 0 auto; border-radius: 16px; overflow: hidden;">
        <div class="zalo-modal-header bg-success text-white">
          <h5 class="modal-title fw-bold"><i class="bi bi-check-circle me-2"></i> Xác nhận duyệt lịch</h5>
          <button class="modal-close text-white border-0 bg-transparent" @click="showApproveModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body">
          <p class="mb-4">Bạn có chắc chắn muốn duyệt lịch hẹn này cho thú cưng <strong>{{ approveTarget?.petName }}</strong> không?</p>
          <div class="d-flex justify-content-end gap-2 mt-4">
            <button class="btn btn-outline-secondary rounded-pill px-4" @click="showApproveModal = false">Hủy</button>
            <button class="btn btn-success rounded-pill px-4" @click="confirmApproveAction">Duyệt lịch</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Reject Confirm Modal -->
    <div v-if="showRejectModal" class="zalo-modal-overlay" @click.self="showRejectModal = false">
      <div class="zalo-modal-card border-0 shadow-lg" style="max-width: 450px; width: 100%; margin: 0 auto; border-radius: 16px; overflow: hidden;">
        <div class="zalo-modal-header bg-danger text-white">
          <h5 class="modal-title fw-bold"><i class="bi bi-x-circle me-2"></i> Từ chối lịch hẹn</h5>
          <button class="modal-close text-white border-0 bg-transparent" @click="showRejectModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <p class="mb-3">Vui lòng nhập lý do từ chối để khách hàng có thể biết:</p>
          <div class="form-floating mb-4">
            <textarea v-model="rejectReason" class="form-control" style="height: 100px" placeholder="Lý do..."></textarea>
            <label class="text-muted">Lý do từ chối (có thể để trống)</label>
          </div>
          <div class="d-flex justify-content-end gap-2 mt-2">
            <button class="btn btn-outline-secondary rounded-pill px-4" @click="showRejectModal = false">Hủy</button>
            <button class="btn btn-danger rounded-pill px-4" @click="confirmRejectAction">Từ chối lịch</button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick, computed, watch } from 'vue';
import api from '../../services/api';
import { Html5Qrcode } from 'html5-qrcode';

// Tab states
const activeSubTab = ref<'calendar' | 'pending'>('calendar');
const selectedDate = ref(new Date().toISOString().slice(0, 10));
const selectedDoctor = ref('ALL');
const selectedStatus = ref('ALL');
const activeDropdownId = ref<number | null>(null);

const toggleDropdown = (id: number) => {
  activeDropdownId.value = activeDropdownId.value === id ? null : id;
};

// Data
const stats = ref({ total: 0, pending: 0, confirmed: 0, waiting: 0, completed: 0 });
const doctorList = ref<any[]>([]);
const serviceList = ref<any[]>([]);
const eventsList = ref<any[]>([]);
const filteredEventsList = computed(() => {
  if (selectedStatus.value === 'ALL') return eventsList.value;
  return eventsList.value.filter((evt: any) => evt.status === selectedStatus.value);
});

// Pagination
const currentPage = ref(1);
const pageSize = ref(10);
const totalPages = computed(() => Math.max(1, Math.ceil(filteredEventsList.value.length / pageSize.value)));
const paginatedEventsList = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  return filteredEventsList.value.slice(start, start + pageSize.value);
});

watch([selectedDate, selectedStatus, selectedDoctor], () => {
  currentPage.value = 1;
});

const pendingList = ref<any[]>([]);


// Loading
const loadingEvents = ref(false);

// Modals
const showDetailModal = ref(false);
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

// Approve Modal
const showApproveModal = ref(false);
const approveTarget = ref<any>(null);

const openConfirmApprove = (item: any) => {
  approveTarget.value = item;
  showApproveModal.value = true;
};

const confirmApproveAction = async () => {
  if (!approveTarget.value) return;
  await updateStatus(approveTarget.value.id, 'confirmed');
  showApproveModal.value = false;
  approveTarget.value = null;
};

// Reject Modal
const showRejectModal = ref(false);
const rejectTarget = ref<any>(null);
const rejectReason = ref('');

const openConfirmReject = (item: any) => {
  rejectTarget.value = item;
  rejectReason.value = '';
  showRejectModal.value = true;
};

const confirmRejectAction = async () => {
  if (!rejectTarget.value) return;
  await updateStatus(rejectTarget.value.id, 'cancelled', rejectReason.value);
  showRejectModal.value = false;
  rejectTarget.value = null;
};

// Change Doctor Modal
const showChangeDoctorModal = ref(false);
const changeDoctorTarget = ref<any>(null);
const selectedNewDoctorId = ref('');
const forceChangeDoctor = ref(false);
const suitableDoctorsList = ref<any[]>([]);
const fetchingSuitableDocs = ref(false);

// Reschedule Modal
const showRescheduleModal = ref(false);
const rescheduleTarget = ref<any>(null);
const rescheduleDate = ref('');
const rescheduleTime = ref('');
const forceReschedule = ref(false);
const availableSlots = ref<string[]>([]);
const loadingSlots = ref(false);

const buildRescheduleSlots = (times: string[]): SlotDisplay[] => {
  if (!rescheduleDate.value) return [];
  const now = Date.now();
  const cutoff = now + BOOKING_BUFFER_MS;
  const [year, month, day] = rescheduleDate.value.split('-');
  
  // Find operating hours for this day
  const slotDateObj = new Date(parseInt(year), parseInt(month) - 1, parseInt(day));
  const dayOfWeek = slotDateObj.getDay();
  const operatingDay = clinicStore.weeklyHours.find((d: any) => d.dayOfWeek === dayOfWeek);
  
  // Check holidays
  const isHoliday = clinicStore.holidays.some((h: any) => {
    if (!h.isActive) return false;
    const start = new Date(h.startDate);
    const end = new Date(h.endDate);
    start.setHours(0,0,0,0);
    end.setHours(23,59,59,999);
    return slotDateObj >= start && slotDateObj <= end;
  });

  return times.map(time => {
    const slotStr = `${rescheduleDate.value}T${time}:00`;
    const [hour, minute] = time.split(':');
    const slotDate = new Date(parseInt(year), parseInt(month) - 1, parseInt(day), parseInt(hour), parseInt(minute), 0);
    const slotMs = slotDate.getTime();
    
    let isClosed = isHoliday || !operatingDay || !operatingDay.isOpen;
    if (!isClosed && operatingDay) {
      const inShift = operatingDay.shifts.some((shift: any) => {
        const sTime = shift.startTime.substring(0, 5);
        const eTime = shift.endTime.substring(0, 5);
        return time >= sTime && time < eTime;
      });
      if (!inShift) isClosed = true;
    }

    const isPast = slotMs < now;
    const isTooSoon = !isPast && slotMs < cutoff;
    const isAvailableFromApi = availableSlots.value.includes(time);
    
    if (forceReschedule.value) {
      return { 
        time, slotStr, 
        isAvailable: isAvailableFromApi, 
        isPast: false, 
        isBooked: !isAvailableFromApi && !isClosed, 
        isTooSoon: false,
        isClosed
      };
    }
    
    const isBooked = !isPast && !isTooSoon && !isAvailableFromApi && !isClosed;
    const isAvailable = !isPast && !isTooSoon && isAvailableFromApi && !isClosed;
    return { time, slotStr, isAvailable, isPast, isBooked, isTooSoon, isClosed };
  }).filter(s => !s.isClosed);
};

const displayRescheduleMorningSlots = computed<SlotDisplay[]>(() => buildRescheduleSlots(masterMorningTimes));
const displayRescheduleAfternoonSlots = computed<SlotDisplay[]>(() => buildRescheduleSlots(masterAfternoonTimes));

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

// Forms (New - Phase 1)
const activeBookingTab = ref<'prebooked' | 'walkin'>('prebooked');

const searchQueryPhone = ref('');
const searchCustomerStatus = ref<'idle' | 'found' | 'not_found'>('idle');
const selectedCustomer = ref<any>(null);
const customerPets = ref<any[]>([]);

const resetCustomerSearch = () => {
  searchQueryPhone.value = '';
  searchCustomerStatus.value = 'idle';
  selectedCustomer.value = null;
  customerPets.value = [];
  isAddingNewPet.value = false;
  customerForm.value = { customerName: '', customerPhone: '' };
  petForm.value = { petName: '', species: 'Chó', breed: '', petWeight: null };
  formPayload.value.petId = '';
};

watch(activeBookingTab, () => {
  resetCustomerSearch();
});

const isAddingNewPet = ref(false);

const customerForm = ref({
  customerName: '',
  customerPhone: '',
});

const petForm = ref({
  petName: '',
  species: 'Chó',
  breed: '',
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
  "14:00", "14:30", "15:00", "15:30", "16:00", "16:30", "17:00", "17:30", "18:00", "18:30", "19:00", "19:30"
];

const fetchingSlots = ref(false);
const slotFetchError = ref('');
const availableTimeSlots = ref<string[]>([]);

const fetchAvailableSlots = async () => {
  if (!formPayload.value.dateOnly) {
    availableTimeSlots.value = [];
    return;
  }
  
  fetchingSlots.value = true;
  slotFetchError.value = '';
  try {
    const res = await api.get('/Appointment/available-slots', {
      params: { 
        date: formPayload.value.dateOnly,
        serviceId: formPayload.value.serviceId || null
      }
    });
    const allSlots: string[] = [];
    res.data.forEach((doc: any) => {
      doc.availableSlots.forEach((slot: string) => {
        if (!allSlots.includes(slot)) {
          allSlots.push(slot);
        }
      });
    });
    availableTimeSlots.value = allSlots.sort((a, b) => a.localeCompare(b));
  } catch (err: any) {
    slotFetchError.value = err?.response?.data?.message || 'Lỗi tải khung giờ trống';
    availableTimeSlots.value = [];
  } finally {
    fetchingSlots.value = false;
  }
};

const isSlotAvailable = (time: string) => {
  if (!formPayload.value.dateOnly) return false;
  const now = new Date();
  const cutoff = now.getTime() + 2 * 60 * 60 * 1000; // 2 hours buffer
  const [year, month, day] = formPayload.value.dateOnly.split('-');
  const [hour, minute] = time.split(':');
  const slotDate = new Date(parseInt(year), parseInt(month) - 1, parseInt(day), parseInt(hour), parseInt(minute), 0);
  const slotMs = slotDate.getTime();
  const isPastOrTooSoon = slotMs < cutoff;
  
  return !isPastOrTooSoon && availableTimeSlots.value.includes(time);
};

watch([() => formPayload.value.dateOnly, () => formPayload.value.serviceId], () => {
  formPayload.value.timeOnly = '';
  fetchAvailableSlots();
}, { immediate: true });

interface SlotDisplay {
  time: string;
  slotStr: string;
  isAvailable: boolean;
  isPast: boolean;
  isBooked: boolean;
  isTooSoon: boolean;
  isClosed?: boolean;
}

const masterMorningTimes = ['08:00', '08:30', '09:00', '09:30', '10:00', '10:30', '11:00', '11:30'];
const masterAfternoonTimes = ['13:30', '14:00', '14:30', '15:00', '15:30', '16:00', '16:30', '17:00', '17:30', '18:00', '18:30', '19:00', '19:30'];

const BOOKING_BUFFER_MS = 15 * 60 * 1000; // 15 phút

import { useClinicConfigStore } from '../../stores/clinicConfig.store';
const clinicStore = useClinicConfigStore();

const buildSlots = (times: string[]): SlotDisplay[] => {
  if (!formPayload.value.dateOnly) return [];
  const now = Date.now();
  const cutoff = now + BOOKING_BUFFER_MS;
  const [year, month, day] = formPayload.value.dateOnly.split('-');
  
  // Find operating hours for this day
  const slotDateObj = new Date(parseInt(year), parseInt(month) - 1, parseInt(day));
  const dayOfWeek = slotDateObj.getDay();
  const operatingDay = clinicStore.weeklyHours.find((d: any) => d.dayOfWeek === dayOfWeek);
  
  // Check holidays
  const isHoliday = clinicStore.holidays.some((h: any) => {
    if (!h.isActive) return false;
    const start = new Date(h.startDate);
    const end = new Date(h.endDate);
    start.setHours(0,0,0,0);
    end.setHours(23,59,59,999);
    return slotDateObj >= start && slotDateObj <= end;
  });

  return times.map(time => {
    const slotStr = `${formPayload.value.dateOnly}T${time}:00`;
    const [hour, minute] = time.split(':');
    const slotDate = new Date(parseInt(year), parseInt(month) - 1, parseInt(day), parseInt(hour), parseInt(minute), 0);
    const slotMs = slotDate.getTime();
    
    let isClosed = isHoliday || !operatingDay || !operatingDay.isOpen;
    if (!isClosed && operatingDay) {
      // Check if time is within any shift
      // Shift time format from backend is usually "HH:MM:SS" or "HH:MM"
      const inShift = operatingDay.shifts.some((shift: any) => {
        const sTime = shift.startTime.substring(0, 5);
        const eTime = shift.endTime.substring(0, 5);
        return time >= sTime && time < eTime;
      });
      if (!inShift) isClosed = true;
    }

    const isPast = slotMs < now;
    const isTooSoon = !isPast && slotMs < cutoff;
    const isAvailableFromApi = availableTimeSlots.value.includes(time);
    const isBooked = !isPast && !isTooSoon && !isAvailableFromApi && !isClosed;
    const isAvailable = !isPast && !isTooSoon && isAvailableFromApi && !isClosed;
    
    return { time, slotStr, isAvailable, isPast, isBooked, isTooSoon, isClosed };
  }).filter(s => !s.isClosed);
};

const displayMorningSlots = computed<SlotDisplay[]>(() => buildSlots(masterMorningTimes));
const displayAfternoonSlots = computed<SlotDisplay[]>(() => buildSlots(masterAfternoonTimes));




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
    const khamSvc = res.data.find((s: any) => s.name.toLowerCase().includes('khám lâm sàng tổng quát'));
    const tiemSvc = res.data.find((s: any) => s.name.toLowerCase().includes('tiêm vaccine tổng hợp (chó)'));
    const mappedServices = [];
    if (khamSvc) {
      mappedServices.push({
        ...khamSvc,
        name: 'Khám bệnh',
        description: 'Kiểm tra sức khỏe tổng quát, chẩn đoán và tư vấn điều trị cho thú cưng của bạn.'
      });
    }
    if (tiemSvc) {
      mappedServices.push({
        ...tiemSvc,
        name: 'Tiêm phòng',
        description: 'Tiêm các loại vaccine cần thiết định kỳ để phòng ngừa bệnh truyền nhiễm cho thú cưng.'
      });
    }
    serviceList.value = mappedServices.length > 0 ? mappedServices : res.data;
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



const loadAllData = async () => {
  await Promise.all([
    loadStats(),
    loadEvents(),
    loadPending(),

  ]);
};

// Form methods (New - Phase 1)
const handleSearchCustomer = async () => {
  const phone = searchQueryPhone.value.trim();
  if (!phone) return;
  
  searchCustomerStatus.value = 'idle';
  selectedCustomer.value = null;
  customerPets.value = [];
  isAddingNewPet.value = false;
  
  try {
    const res = await api.get(`/receptionist/customer-by-phone?phone=${encodeURIComponent(phone)}`);
    if (res.data.success) {
      searchCustomerStatus.value = 'found';
      selectedCustomer.value = res.data.customer;
      customerPets.value = res.data.pets || [];
      if (customerPets.value.length > 0) {
        formPayload.value.petId = customerPets.value[0].id;
      } else {
        formPayload.value.petId = '';
      }
    } else {
      searchCustomerStatus.value = 'not_found';
      customerForm.value.customerPhone = phone;
      customerForm.value.customerName = '';
      petForm.value = { petName: '', species: 'Chó', breed: '', petWeight: null };
    }
  } catch (err) {
    console.error(err);
    searchCustomerStatus.value = 'not_found';
    customerForm.value.customerPhone = phone;
  }
};

const submitPrebookedAppointment = async () => {
  if (activeBookingTab.value !== 'prebooked') return;
  
  if (!formPayload.value.timeOnly) {
    showToast('Vui lòng chọn khung giờ hẹn khám!', 'warning');
    return;
  }
  
  if (searchCustomerStatus.value === 'idle') {
    showToast('Vui lòng tìm kiếm thông tin khách hàng trước!', 'warning');
    return;
  }

  const fullDateTime = `${formPayload.value.dateOnly}T${formPayload.value.timeOnly}:00`;

  try {
    if (searchCustomerStatus.value === 'found') {
      let finalPetId = formPayload.value.petId;
      
      if (isAddingNewPet.value) {
        if (!petForm.value.petName) {
          showToast('Vui lòng nhập tên thú cưng mới!', 'warning');
          return;
        }
        const petRes = await api.post(`/receptionist/customers/${selectedCustomer.value.id}/pets`, {
          name: petForm.value.petName,
          species: petForm.value.species,
          breed: petForm.value.breed,
          weight: petForm.value.petWeight
        });
        if (petRes.data.success) {
          finalPetId = petRes.data.petId;
        } else {
          showToast('Lỗi khi thêm thú cưng mới!', 'danger');
          return;
        }
      }

      const payload: any = {
        customerId: selectedCustomer.value.id,
        petId: finalPetId,
        serviceId: formPayload.value.serviceId,
        appointmentDate: fullDateTime,
        symptom: formPayload.value.symptom,
        note: formPayload.value.note
      };
      if (formPayload.value.doctorId) payload.doctorId = formPayload.value.doctorId;
      
      const res = await api.post('/appointment', payload);
      if (res.data.success) {
        showToast('Tạo lịch hẹn thành công!', 'success');
        showCreateModal.value = false;
        await loadAllData();
      }
    } else if (searchCustomerStatus.value === 'not_found') {
      const payload: any = {
        customerName: customerForm.value.customerName,
        customerPhone: customerForm.value.customerPhone,
        petName: petForm.value.petName,
        species: petForm.value.species,
        petWeight: petForm.value.petWeight,
        serviceId: formPayload.value.serviceId,
        appointmentDate: fullDateTime,
        symptom: formPayload.value.symptom,
        note: formPayload.value.note
      };
      if (formPayload.value.doctorId) payload.doctorId = formPayload.value.doctorId;
      
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

const submitWalkInAppointment = async () => {
  if (activeBookingTab.value !== 'walkin') return;
  
  if (searchCustomerStatus.value === 'idle') {
    showToast('Vui lòng tìm kiếm thông tin khách hàng trước!', 'warning');
    return;
  }

  try {
    let payload: any = {
      serviceId: formPayload.value.serviceId,
      symptom: formPayload.value.symptom,
      isEmergency: false
    };
    if (formPayload.value.doctorId) payload.doctorId = formPayload.value.doctorId;

    if (!formPayload.value.serviceId) {
      showToast('Vui lòng chọn dịch vụ khám bệnh!', 'warning');
      return;
    }

    if (searchCustomerStatus.value === 'found') {
      payload.phone = selectedCustomer.value.phone || customerForm.value.customerPhone;
      payload.fullName = selectedCustomer.value.fullName;
      
      if (isAddingNewPet.value) {
        if (!petForm.value.petName) {
          showToast('Vui lòng nhập tên thú cưng!', 'warning');
          return;
        }
        payload.petName = petForm.value.petName;
        payload.species = petForm.value.species;
        payload.weight = petForm.value.petWeight;
      } else {
        if (!formPayload.value.petId) {
          showToast('Vui lòng chọn thú cưng!', 'warning');
          return;
        }
        const pet = customerPets.value.find(p => p.id === formPayload.value.petId);
        if (pet) {
            payload.petName = pet.name;
            payload.species = pet.species;
            payload.weight = pet.weight;
        } else {
            showToast('Không tìm thấy thông tin thú cưng!', 'warning');
            return;
        }
      }
    } else if (searchCustomerStatus.value === 'not_found') {
      if (!customerForm.value.customerName || !customerForm.value.customerPhone || !petForm.value.petName) {
        showToast('Vui lòng điền đầy đủ thông tin bắt buộc!', 'warning');
        return;
      }
      payload.phone = customerForm.value.customerPhone;
      payload.fullName = customerForm.value.customerName;
      payload.petName = petForm.value.petName;
      payload.species = petForm.value.species;
      payload.weight = petForm.value.petWeight;
    }

    const res = await api.post('/receptionist/walk-in', payload);
    if (res.data.success) {
      showToast('Đã đưa vào hàng đợi thành công!', 'success');
      showCreateModal.value = false;
      await loadAllData();
    }
  } catch (err: any) {
    let msg = err.response?.data?.message;
    if (!msg && err.response?.data?.errors) {
      msg = Object.values(err.response.data.errors).flat().join(', ');
    }
    showToast(msg || 'Lỗi khi tạo lịch hẹn vãng lai.', 'danger');
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
  // Reset new states
  activeBookingTab.value = 'prebooked';
  resetCustomerSearch();
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



// Detail modal
const handleRowClick = (event: MouseEvent, apptId: number) => {
  const target = event.target as HTMLElement;
  if (target.closest('.dropdown') || target.closest('button')) return;
  openDetailModal(apptId);
};

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

const updateStatus = async (apptId: number, status: string, reason: string = '') => {
  try {
    const res = await api.put(`/appointment/${apptId}/status`, { status, reason });
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

const openChangeDoctorModal = async (evt: any) => {
  changeDoctorTarget.value = evt;
  selectedNewDoctorId.value = '';
  forceChangeDoctor.value = false;
  showChangeDoctorModal.value = true;
  
  fetchingSuitableDocs.value = true;
  try {
    const res = await api.get(`/appointment/${evt.id}/suitable-doctors`);
    suitableDoctorsList.value = res.data;
  } catch (err: any) {
    showToast('Lỗi khi tải danh sách bác sĩ chuyên môn', 'danger');
    suitableDoctorsList.value = [];
  } finally {
    fetchingSuitableDocs.value = false;
  }
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

onMounted(async () => {
  await clinicStore.fetchWeeklyHours();
  await clinicStore.fetchHolidays();

  document.addEventListener('click', (e) => {
    const target = e.target as HTMLElement;
    if (!target.closest('.dropdown')) {
      activeDropdownId.value = null;
    }
  });
  loadDoctors();
  loadServices();
  loadAllData();
});

defineExpose({
  openCreateModal
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

/* ===== Time Slot Buttons ===== */
.time-slot-btn {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  flex: 1 1 calc(25% - 0.5rem);
  padding: 8px 10px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.88rem;
  border: 1.5px solid transparent;
  cursor: pointer;
  transition: all 0.18s ease;
  gap: 2px;
  min-width: 72px;
  line-height: 1.2;
}

/* Available slot */
.time-slot-btn.slot-available {
  background: #fff;
  border-color: #dee2e6;
  color: #495057;
}
.time-slot-btn.slot-available:hover {
  border-color: #0d6efd;
  color: #0d6efd;
  background: #f0f6ff;
  box-shadow: 0 2px 8px rgba(13,110,253,0.12);
}

/* Selected slot */
.time-slot-btn.slot-selected {
  background: #0d6efd;
  border-color: #0d6efd;
  color: white;
  box-shadow: 0 4px 12px rgba(13,110,253,0.3);
}

/* Past slot — grey, italic, strikethrough */
.time-slot-btn.slot-past {
  background: #f8f9fa;
  border-color: #e9ecef;
  color: #adb5bd;
  cursor: not-allowed;
  opacity: 0.65;
}
.time-slot-btn.slot-past .slot-time-text {
  text-decoration: line-through;
  font-style: italic;
}

/* Too soon — amber/orange warning */
.time-slot-btn.slot-too-soon {
  background: #fff8ec;
  border-color: #ffc107;
  color: #b45309;
  cursor: not-allowed;
  opacity: 0.8;
}

/* Booked by someone else — red/rose */
.time-slot-btn.slot-booked {
  background: #fff5f5;
  border-color: #fca5a5;
  color: #dc3545;
  cursor: not-allowed;
  opacity: 0.8;
}

/* Small label badge below the time text */
.slot-badge-label {
  font-size: 0.65rem;
  font-weight: 700;
  letter-spacing: 0.3px;
  text-transform: uppercase;
  line-height: 1;
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
