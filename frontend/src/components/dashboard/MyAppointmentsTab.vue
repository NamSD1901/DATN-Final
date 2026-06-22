<template>
  <div class="myappts-tab">

    <!-- Header -->
    <div class="appts-hero mb-4">
      <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h3 class="fw-bold text-dark mb-1">
            <i class="bi bi-calendar-check-fill me-2" style="color: var(--primary-gold);"></i>
            Lịch hẹn của tôi
          </h3>
          <p class="text-muted mb-0 small">Theo dõi và quản lý các buổi hẹn khám bệnh cho thú cưng.</p>
        </div>
        <div class="d-flex gap-2 mt-3 mt-sm-0">
          <button class="btn btn-outline-success fw-bold rounded-pill px-3" style="border-width: 2px; border-color: #10b981; color: #10b981;" @click="openQrModal">
            <i class="bi bi-qr-code-scan me-2"></i>Mã QR Check-in
          </button>
          <button class="btn btn-premium-appt" @click="openBookModal">
            <i class="bi bi-plus-circle-fill me-2"></i> Đặt lịch mới
          </button>
        </div>
      </div>
    </div>

    <!-- Filter Tabs -->
    <div class="filter-tabs mb-4">
      <button
        v-for="tab in filterOptions"
        :key="tab.value"
        class="filter-tab-btn"
        :class="{ active: activeFilter === tab.value }"
        @click="changeFilter(tab.value)"
      >
        <i :class="tab.icon" class="me-1"></i> {{ tab.label }}
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">Đang tải lịch hẹn...</p>
    </div>

    <!-- Error -->
    <div v-else-if="errorMsg" class="alert alert-danger rounded-4 border-0 shadow-sm py-3 px-4">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- Empty -->
    <div v-else-if="filteredAppointments.length === 0" class="empty-state-appt">
      <div class="empty-icon">📅</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">
        {{ activeFilter === 'all' ? 'Chưa có lịch hẹn nào' : `Không có lịch hẹn ${getStatusLabel(activeFilter)}` }}
      </h5>
      <p class="text-muted small mb-4">Đặt lịch ngay để được phục vụ nhanh hơn, không cần chờ đợi.</p>
      <button class="btn btn-premium-appt" @click="openBookModal">
        <i class="bi bi-plus-circle-fill me-2"></i> Đặt lịch ngay
      </button>
    </div>

    <!-- Appointment Cards -->
    <div v-else>
      <TransitionGroup name="appt-card" tag="div" class="appts-list">
        <div
          v-for="appt in filteredAppointments"
          :key="appt.id"
          class="appt-card"
          :class="`status-${appt.status}`"
        >
          <!-- Top status border handled via CSS per status class -->

          <div class="appt-card-inner">
            <!-- LEFT: Date Pill Block -->
            <div class="appt-date-pill">
              <div class="appt-date-pill-day">{{ formatDay(appt.appointmentDate) }}</div>
              <div class="appt-date-pill-month">{{ formatMonthYear(appt.appointmentDate) }}</div>
              <div class="appt-date-pill-time">
                <i class="bi bi-clock me-1"></i>{{ formatTime(appt.appointmentDate) }}
              </div>
            </div>

            <!-- DIVIDER -->
            <div class="appt-divider"></div>

            <!-- RIGHT: Info + Actions -->
            <div class="appt-right">
              <!-- Top: status badge -->
              <div class="appt-right-top">
                <span class="appt-status-badge" :class="`badge-${appt.status}`">
                  <i :class="getStatusIcon(appt.status)" class="me-1"
                    :style="appt.status === 'in_progress' ? 'animation: pulse-dot 1.2s infinite;' : ''"
                  ></i>
                  {{ getStatusLabel(appt.status) }}
                </span>
                <span v-if="appt.invoiceStatus" class="invoice-badge ms-2">
                  <i class="bi bi-receipt me-1"></i>
                  {{ getInvoiceStatusLabel(appt.invoiceStatus) }}
                </span>
              </div>

              <!-- Middle: 3-col fixed info grid -->
              <div class="appt-info-grid">
                <div class="appt-info-item">
                  <span class="appt-info-label">Thú cưng</span>
                  <span class="appt-info-value">
                    <span class="pet-inline-badge">{{ appt.petName || '—' }}</span>
                    <small class="text-muted ms-1">{{ appt.species }}</small>
                  </span>
                </div>
                <div class="appt-info-item">
                  <span class="appt-info-label">Dịch vụ</span>
                  <span class="appt-info-value">{{ appt.serviceName }}</span>
                </div>
                <div class="appt-info-item">
                  <span class="appt-info-label">Bác sĩ phụ trách</span>
                  <span class="appt-info-value">{{ appt.doctorName || 'Chưa phân công' }}</span>
                </div>
              </div>

              <!-- Note -->
              <div v-if="appt.note || appt.symptom" class="appt-note">
                <i class="bi bi-chat-left-text-fill me-1 text-muted"></i>
                <span>{{ appt.symptom || appt.note }}</span>
              </div>

              <!-- Actions -->
              <div class="appt-actions">
                <button class="btn-appt-detail" @click="openDetailModal(appt)">
                  <i class="bi bi-eye me-1"></i> Xem chi tiết
                </button>
                <button
                  v-if="canCancel(appt.status)"
                  class="btn-appt-cancel"
                  @click="confirmCancel(appt)"
                >
                  <i class="bi bi-x-circle me-1"></i> Huỷ lịch
                </button>
                <div v-if="appt.invoiceTotalAmount && appt.invoiceTotalAmount > 0" class="appt-price-tag">
                  {{ formatCurrency(appt.invoiceTotalAmount) }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </TransitionGroup>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="d-flex justify-content-center align-items-center gap-2 mt-4 pb-2">
        <button 
          class="btn btn-outline-secondary btn-sm rounded-pill px-3" 
          :disabled="currentPage === 1" 
          @click="prevPage"
        >
          <i class="bi bi-chevron-left me-1"></i> Trang trước
        </button>
        <span class="text-muted small mx-2" style="font-size: 0.8rem; font-weight: 600;">Trang {{ currentPage }} / {{ totalPages }} (Tổng số: {{ totalCount }} lịch hẹn)</span>
        <button 
          class="btn btn-outline-secondary btn-sm rounded-pill px-3" 
          :disabled="currentPage === totalPages" 
          @click="nextPage"
        >
          Trang sau <i class="bi bi-chevron-right ms-1"></i>
        </button>
      </div>
    </div>

    
    <!-- ===== BOOKING MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showBookModal" class="appt-modal-overlay wizard-overlay" @click.self="closeBookModal">
          <div class="appt-modal-card wizard-modal" style="max-width: 1000px; height: 85vh; display: flex; flex-direction: column;">
            <div class="appt-modal-header border-0 pb-0">
              <button class="modal-close-btn ms-auto" @click="closeBookModal">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="appt-modal-body wizard-body p-0 d-flex flex-column" style="flex-grow: 1; overflow: hidden;">
              <!-- Progress steps -->
              <div class="stepper-container px-5 pt-3 mb-4">
                <div class="progress-container position-relative">
                  <div class="progress-line position-absolute top-50 start-0 end-0 translate-middle-y" style="height: 3px; background: #e9ecef; z-index: 1;">
                    <div class="progress-fill" :style="{ width: `${(currentStep / (maxSteps)) * 100}%`, background: '#0d6efd', height: '100%', transition: 'width 0.4s ease' }"></div>
                  </div>
                  <div class="step-items d-flex justify-content-between position-relative" style="z-index: 2;">
                    <div v-for="(step, idx) in bookingSteps" :key="idx" class="step-item d-flex flex-column align-items-center gap-2" :class="{ 'active': currentStep >= idx, 'current': currentStep === idx }">
                      <div class="step-circle d-flex align-items-center justify-content-center" 
                           :style="currentStep >= idx ? 'width: 32px; height: 32px; border-radius: 50%; background: #0d6efd; color: white; border: 2px solid #0d6efd; font-weight: bold; transition: all 0.3s ease;' : 'width: 32px; height: 32px; border-radius: 50%; background: white; color: #adb5bd; border: 2px solid #dee2e6; font-weight: bold; transition: all 0.3s ease;'">
                        <i v-if="currentStep > idx" class="bi bi-check-lg"></i>
                        <span v-else>{{ idx + 1 }}</span>
                      </div>
                      <span class="step-label" :style="currentStep >= idx ? 'font-size: 0.8rem; font-weight: 600; color: #212529;' : 'font-size: 0.8rem; font-weight: 600; color: #adb5bd;'">{{ step }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="step-content-wrapper px-5 pb-4" style="flex-grow: 1; overflow-y: auto;">
                <div v-if="bookingSuccess" class="text-center py-5">
                  <div style="font-size: 5rem;">🎉</div>
                  <h4 class="fw-bold text-success mt-4 mb-2">Đặt lịch thành công!</h4>
                  <p class="text-muted">Chúng tôi sẽ xác nhận lịch hẹn của bạn sớm nhất có thể.</p>
                </div>

                <form v-else @submit.prevent="submitBooking" class="h-100">
                  <!-- Step 0: Choose Pet -->
                  <div v-if="currentStep === 0" class="step-content animate-fade-in">
                    <h3 class="fw-bold text-dark mb-1">Chọn thú cưng</h3>
                    <p class="text-muted mb-4">Chọn thú cưng cho lần khám này.</p>

                    <div class="d-flex justify-content-end mb-4">
                      <button class="btn btn-outline-primary rounded-pill fw-bold" @click.prevent="$emit('switch-tab', 'my-pets')">
                        <i class="bi bi-plus-circle me-1"></i> Thêm thú cưng mới
                      </button>
                    </div>

                    <div v-if="myPets.length === 0" class="text-center py-5 text-muted">
                      Bạn chưa có thú cưng nào. Hãy thêm mới để tiếp tục.
                    </div>
                    <div v-else class="row g-4">
                      <div v-for="pet in myPets" :key="pet.id" class="col-md-6">
                        <div
                          class="pet-select-card position-relative"
                          :class="{
                            'pet-selected': bookForm.petId === pet.id,
                            'pet-has-warning': petActiveAppointments.has(pet.id)
                          }"
                          @click="selectPet(pet.id)"
                        >
                          <!-- Warning ribbon if pet has active appt -->
                          <div v-if="petActiveAppointments.has(pet.id)" class="pet-warning-ribbon">
                            <i class="bi bi-exclamation-triangle-fill me-1"></i>Có lịch hẹn
                          </div>

                          <div class="d-flex gap-3 mb-3">
                            <div class="pet-avatar-wrapper" style="width: 70px; height: 70px; border-radius: 12px; background: #f8f9fa; display: flex; align-items: center; justify-content: center; font-size: 2.5rem;">
                              {{ getSpeciesEmoji(pet.species) }}
                            </div>
                            <div class="flex-grow-1">
                              <h5 class="fw-bold mb-1">{{ pet.name }}</h5>
                              <p class="small text-muted mb-0">{{ pet.species }}</p>
                              <!-- Mini badge count -->
                              <span v-if="petActiveAppointments.has(pet.id)" class="badge bg-warning text-dark mt-1" style="font-size:0.7rem;">
                                {{ petActiveAppointments.get(pet.id)!.length }} lịch đang hoạt động
                              </span>
                            </div>
                          </div>
                          <div class="border-top pt-3 mt-auto d-flex justify-content-between align-items-center small">
                            <span class="text-muted">Nhấn để chọn</span>
                            <i class="bi bi-check-circle-fill" :class="bookForm.petId === pet.id ? 'text-primary' : 'text-light'"></i>
                          </div>
                        </div>
                      </div>
                    </div>

                    <!-- Duplicate appointment warning banner -->
                    <Transition name="fade-slide">
                      <div
                        v-if="selectedPetActiveAppts.length > 0 && !petWarningDismissed"
                        class="pet-dup-warning mt-4"
                      >
                        <div class="d-flex align-items-start gap-3">
                          <div class="flex-shrink-0" style="font-size: 1.8rem;">⚠️</div>
                          <div class="flex-grow-1">
                            <p class="fw-bold mb-1 text-warning-emphasis">
                              {{ getSelectedPetName() }} đang có {{ selectedPetActiveAppts.length }} lịch hẹn chưa hoàn tất!
                            </p>
                            <ul class="mb-2 ps-3 small text-muted">
                              <li v-for="a in selectedPetActiveAppts" :key="a.id">
                                <strong>{{ formatDateFull(a.appointmentDate) }}</strong>
                                — {{ a.serviceName }} 
                                <span class="badge rounded-pill ms-1" :class="getStatusBadgeClass(a.status ?? '')">{{ getStatusLabel(a.status ?? '') }}</span>
                              </li>
                            </ul>
                            <div class="d-flex gap-2 mt-2 flex-wrap">
                              <button type="button" class="btn btn-sm btn-outline-warning rounded-pill fw-bold" @click="petWarningDismissed = true">
                                <i class="bi bi-arrow-right-circle me-1"></i>Vẫn tiếp tục đặt lịch mới
                              </button>
                              <button type="button" class="btn btn-sm btn-outline-secondary rounded-pill" @click="closeBookModal">
                                <i class="bi bi-x-circle me-1"></i>Đóng lại
                              </button>
                            </div>
                          </div>
                        </div>
                      </div>
                    </Transition>
                  </div>

                  <!-- Step 1: Choose Service -->
                  <div v-else-if="currentStep === 1" class="step-content animate-fade-in">
                    <h3 class="fw-bold text-dark mb-1">Chọn dịch vụ</h3>
                    <p class="text-muted mb-4">Chọn dịch vụ khám chữa bệnh cho thú cưng.</p>

                    <div class="row g-4">
                      <div class="col-md-6" v-for="svc in services" :key="svc.id">
                        <div class="service-card h-100" :style="bookForm.serviceId === svc.id ? 'background: #f8fbff; border-radius: 16px; border: 2px solid #0d6efd; padding: 25px; cursor: pointer; position: relative;' : 'background: white; border-radius: 16px; border: 2px solid transparent; box-shadow: 0 4px 15px rgba(0,0,0,0.03); padding: 25px; cursor: pointer; position: relative;'" @click="bookForm.serviceId = svc.id">
                          <div class="service-radio" :style="bookForm.serviceId === svc.id ? 'position: absolute; top: 25px; right: 25px; width: 22px; height: 22px; border-radius: 50%; border: 2px solid #0d6efd; display: flex; align-items: center; justify-content: center;' : 'position: absolute; top: 25px; right: 25px; width: 22px; height: 22px; border-radius: 50%; border: 2px solid #dee2e6;'">
                            <div v-if="bookForm.serviceId === svc.id" style="width: 12px; height: 12px; border-radius: 50%; background: #0d6efd;"></div>
                          </div>
                          <div class="service-icon mb-3" style="width: 50px; height: 50px; border-radius: 12px; display: flex; align-items: center; justify-content: center; background: rgba(13,110,253,0.1); color: #0d6efd; font-size: 1.5rem;">
                            <i class="bi" :class="svc.name.toLowerCase().includes('vaccine') || svc.name.toLowerCase().includes('tiêm') ? 'bi-bandaid' : (svc.name.toLowerCase().includes('spa') || svc.name.toLowerCase().includes('cắt tỉa') ? 'bi-scissors' : 'bi-heart-pulse')"></i>
                          </div>
                          <h5 class="fw-bold mb-2 pe-4">{{ svc.name }}</h5>
                          <p class="text-muted small mb-4">{{ (svc as any).description || 'Dịch vụ chăm sóc sức khoẻ tốt nhất cho thú cưng.' }}</p>
                          <div class="d-flex justify-content-between align-items-start mt-auto">
                            <span class="badge bg-light text-muted px-3 py-2 rounded-pill mt-1"><i class="bi bi-clock me-1"></i> {{ (svc as any).durationMinutes || 30 }} phút</span>
                            <div class="text-end">
                              <h5 class="fw-bold text-primary mb-0">
                                {{ svc.name.toLowerCase().includes('tiêm') ? 'Theo giá Vắc-xin' : (svc.price ? formatCurrency(svc.price) : 'Miễn phí') }}
                              </h5>
                              <div v-if="svc.name.toLowerCase().includes('tiêm')" style="font-size: 0.75rem; color: #198754;" class="mt-1 fw-medium"><i class="bi bi-gift me-1"></i>Miễn phí công tiêm</div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  
                  <!-- Step 2: Choose Time -->
                  <div v-else-if="currentStep === 2" class="step-content animate-fade-in d-flex flex-column h-100">
                    <div class="row g-4 h-100 overflow-hidden">
                      <!-- Calendar Area -->
                      <div class="col-md-5 border-end pe-4 h-100 overflow-auto">
                        
                        <div class="d-flex justify-content-between align-items-center mb-4">
                          <h5 class="fw-bold text-dark mb-0">Tháng {{ currentMonth + 1 }}, {{ currentYear }}</h5>
                          <div class="d-flex gap-2">
                            <button type="button" class="btn btn-sm btn-light rounded-circle" style="width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;" @click.prevent="prevMonth">
                              <i class="bi bi-chevron-left"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-light rounded-circle" style="width: 32px; height: 32px; display: flex; align-items: center; justify-content: center;" @click.prevent="nextMonth">
                              <i class="bi bi-chevron-right"></i>
                            </button>
                          </div>
                        </div>

                        <div class="calendar-grid mb-4">
                          <div class="d-grid" style="grid-template-columns: repeat(7, 1fr); text-align: center; font-weight: 600; font-size: 0.8rem; color: #6c757d; margin-bottom: 10px;">
                            <div>CN</div><div>T2</div><div>T3</div><div>T4</div><div>T5</div><div>T6</div><div>T7</div>
                          </div>
                          <div class="d-grid gap-1" style="grid-template-columns: repeat(7, 1fr); text-align: center;">
                            <div v-for="(day, idx) in calendarDays" :key="idx" 
                                 class="calendar-day rounded-circle d-flex align-items-center justify-content-center mx-auto"
                                 :class="{
                                   'text-muted opacity-50': !day.isCurrentMonth || day.isPast,
                                   'fw-bold': day.isCurrentMonth && !day.isPast,
                                   'bg-primary text-white': selectedBookingDate === day.dateStr,
                                   'border border-primary': day.isToday && selectedBookingDate !== day.dateStr
                                 }"
                                 :style="`width: 36px; height: 36px; cursor: ${day.isPast ? 'not-allowed' : 'pointer'}; ${selectedBookingDate === day.dateStr ? 'box-shadow: 0 4px 10px rgba(13,110,253,0.3);' : ''} ${!day.isPast && selectedBookingDate !== day.dateStr && day.isCurrentMonth ? 'background: #f8f9fa;' : ''}`"
                                 @click="!day.isPast && onCalendarDateSelect(day.dateStr)">
                              {{ day.dayNum }}
                              <div v-if="day.hasAvailability && !day.isPast" class="availability-dot" style="width: 4px; height: 4px; background: #10b981; border-radius: 50%; position: absolute; bottom: 4px;"></div>
                            </div>
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

                      <!-- Slots Area -->
                      <div class="col-md-7 ps-4 d-flex flex-column h-100">
                        <div class="d-flex align-items-center mb-4 p-3 rounded-3" style="background: #f8fbff; border: 1px solid rgba(13,110,253,0.1);">
                          <i class="bi bi-calendar-event text-primary fs-4 me-3"></i>
                          <div class="flex-grow-1">
                            <h6 class="fw-bold mb-0 text-dark">{{ selectedBookingDate ? formatDateOnly(selectedBookingDate) : 'Chưa chọn ngày' }}</h6>
                            <small class="text-muted">{{ getSelectedServiceName() }}</small>
                          </div>
                          <div>
                            <span class="badge bg-primary bg-opacity-10 text-primary px-3 py-2" style="font-size: 0.85rem;">
                              ✨ Tự động phân công
                            </span>
                          </div>
                        </div>

                        <div v-if="fetchingSlots" class="text-center py-5">
                          <div class="spinner-border text-primary"></div>
                          <div class="text-muted mt-2">Đang tải lịch trống...</div>
                        </div>
                        <div v-else-if="!selectedBookingDate" class="text-center py-5 text-muted">
                          Vui lòng chọn ngày để xem giờ trống.
                        </div>
                        <!-- The no-slots fallback has been removed because we always render the greyed out slots -->
                        <div v-else class="d-flex flex-column flex-grow-1 overflow-hidden">
                          <div class="slots-scroll-area flex-grow-1 overflow-auto" style="padding-right: 10px; margin-right: -10px;">
                            <!-- Morning Slots -->
                            <div class="mb-4">
                              <h6 class="text-muted fw-bold mb-3 small" style="letter-spacing: 1px;"><i class="bi bi-brightness-alt-high me-1"></i> BUỔI SÁNG</h6>
                              <div class="d-flex flex-wrap gap-2">
                                <button
                                  v-for="slot in displayMorningSlots"
                                  :key="slot.time"
                                  type="button"
                                  class="time-slot-btn"
                                  :class="{
                                    'slot-selected': bookForm.appointmentDate === slot.slotStr,
                                    'slot-past': slot.isPast,
                                    'slot-too-soon': slot.isTooSoon,
                                    'slot-booked': slot.isBooked,
                                    'slot-available': slot.isAvailable
                                  }"
                                  :disabled="!slot.isAvailable"
                                  :title="slot.isPast ? 'Giờ đã qua' : (slot.isTooSoon ? 'Cần đặt trước ít nhất 1 tiếng' : (slot.isBooked ? 'Khung giờ này đã được đặt' : ''))"
                                  @click="slot.isAvailable && selectTimeSlot(selectedDoctorFilter === 'auto' ? null : selectedDoctorFilter, slot.slotStr)"
                                >
                                  <span class="slot-time-text">{{ slot.time }}</span>
                                  <i v-if="bookForm.appointmentDate === slot.slotStr" class="bi bi-check-circle-fill ms-1"></i>
                                  <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                                  <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                                  <span v-else-if="slot.isBooked" class="slot-badge-label">Đã đặt</span>
                                </button>
                              </div>
                            </div>

                            <!-- Afternoon Slots -->
                            <div class="mb-4">
                              <h6 class="text-muted fw-bold mb-3 small" style="letter-spacing: 1px;"><i class="bi bi-brightness-alt-low me-1"></i> BUỔI CHIỀU</h6>
                              <div class="d-flex flex-wrap gap-2">
                                <button
                                  v-for="slot in displayAfternoonSlots"
                                  :key="slot.time"
                                  type="button"
                                  class="time-slot-btn"
                                  :class="{
                                    'slot-selected': bookForm.appointmentDate === slot.slotStr,
                                    'slot-past': slot.isPast,
                                    'slot-too-soon': slot.isTooSoon,
                                    'slot-booked': slot.isBooked,
                                    'slot-available': slot.isAvailable
                                  }"
                                  :disabled="!slot.isAvailable"
                                  :title="slot.isPast ? 'Giờ đã qua' : (slot.isTooSoon ? 'Cần đặt trước ít nhất 1 tiếng' : (slot.isBooked ? 'Khung giờ này đã được đặt' : ''))"
                                  @click="slot.isAvailable && selectTimeSlot(selectedDoctorFilter === 'auto' ? null : selectedDoctorFilter, slot.slotStr)"
                                >
                                  <span class="slot-time-text">{{ slot.time }}</span>
                                  <i v-if="bookForm.appointmentDate === slot.slotStr" class="bi bi-check-circle-fill ms-1"></i>
                                  <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                                  <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                                  <span v-else-if="slot.isBooked" class="slot-badge-label">Đã đặt</span>
                                </button>
                              </div>
                            </div>
                          </div>
                          

                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Step 3: Choose Vaccine (Optional) -->
                  <div v-else-if="isVaccinationService && currentStep === 3" class="step-content animate-fade-in">
                    <h3 class="fw-bold text-dark mb-1">Chọn Vaccine</h3>
                    <p class="text-muted mb-4">Vui lòng chọn loại vắc-xin cho bé <strong>{{ getSelectedPetName() }}</strong>.</p>
                    
                    <div class="row g-3">
                      <div class="col-md-6" v-for="vac in filteredVaccines" :key="vac.id">
                        <div class="service-card" :style="bookForm.vaccineId === vac.id ? 'background: #f8fbff; border-radius: 16px; border: 2px solid #0d6efd; padding: 20px; cursor: pointer;' : 'background: white; border-radius: 16px; border: 2px solid #dee2e6; padding: 20px; cursor: pointer;'" @click="selectVaccine(vac.id)">
                          <h6 class="fw-bold mb-1">{{ vac.name }}</h6>
                          <div class="small text-muted mb-2">{{ vac.description }}</div>
                          <span class="badge bg-success-subtle text-success">Còn: {{ vac.stockQuantity }} liều</span>
                        </div>
                      </div>
                    </div>


                  </div>

                  <!-- Step 4: Confirm -->
                  <div v-else-if="currentStep === (isVaccinationService ? 4 : 3)" class="step-content animate-fade-in d-flex flex-column" style="min-height: min-content;">
                    <div v-if="bookingError" class="alert alert-danger mb-2 border-0 rounded-3 small text-start py-2">
                      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ bookingError }}
                    </div>

                    <div class="confirm-layout flex-grow-1 pe-2 mt-1" style="overflow-y: visible;">
                      <div class="confirm-details">
                        <!-- Patient Info -->
                        <div class="booking-confirm-card">
                          <div class="card-header-flex border-bottom pb-2 mb-3">
                            <h6 class="mb-0 fw-bold" style="color: #1e293b;"><i class="bi bi-person-vcard text-primary me-2"></i> Thông tin bệnh nhân</h6>
                            <span class="edit-link text-primary fw-bold" style="font-size: 0.75rem; cursor: pointer;" @click="currentStep = 0">SỬA</span>
                          </div>
                          <div class="d-flex align-items-center">
                            <img v-if="getSelectedPetObj()?.imageUrl" :src="getSelectedPetObj()?.imageUrl" alt="Pet" class="rounded-circle" style="width: 65px; height: 65px; object-fit: cover;">
                            <div v-else class="rounded-circle bg-light d-flex align-items-center justify-content-center" style="width: 65px; height: 65px; font-size: 2.2rem; border: 1px solid #e2e8f0;">
                              {{ getSpeciesEmoji(selectedPetSpecies || 'Chó') }}
                            </div>
                            <div class="ms-3 flex-grow-1">
                              <div class="d-flex align-items-center mb-1">
                                <h5 class="fw-bold text-dark mb-0 me-2" style="font-size: 1.2rem;">{{ getSelectedPetName() }}</h5>
                                <span class="badge bg-primary bg-opacity-10 text-primary rounded-pill px-2 py-1" style="font-size: 0.65rem;">{{ selectedPetSpecies }}{{ getSelectedPetObj()?.breed ? ' (' + getSelectedPetObj()?.breed + ')' : '' }}</span>
                              </div>
                              <div class="row gx-2 mt-2">
                                <div class="col-6">
                                  <small class="text-muted d-block" style="font-size: 0.7rem;">Tuổi</small>
                                  <strong class="text-dark small" style="font-size: 0.85rem;">{{ getSelectedPetObj()?.birthDate ? calculateAge(getSelectedPetObj()?.birthDate || '') : '--' }}</strong>
                                </div>
                                <div class="col-6">
                                  <small class="text-muted d-block" style="font-size: 0.7rem;">Cân nặng</small>
                                  <strong class="text-dark small" style="font-size: 0.85rem;">{{ getSelectedPetObj()?.weight || '--' }} kg</strong>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>

                        <!-- Service Info -->
                        <div class="booking-confirm-card mt-3" style="border-left: 4px solid #198754; padding-top: 15px; padding-bottom: 15px;">
                          <div class="card-header-flex pb-2 mb-2">
                            <h6 class="mb-0 fw-bold" style="color: #1e293b;"><i class="bi bi-medical-mac text-success me-2"></i> Dịch vụ đăng ký</h6>
                            <span class="edit-link text-primary fw-bold" style="font-size: 0.75rem; cursor: pointer;" @click="currentStep = 1">SỬA</span>
                          </div>
                          <div class="border rounded-3 p-3 d-flex align-items-center" style="border-color: #e2e8f0 !important;">
                            <div class="bg-success bg-opacity-25 text-success rounded-3 d-flex align-items-center justify-content-center me-3" style="width: 48px; height: 48px; flex-shrink: 0;">
                              <i :class="(services.find(s => s.id === bookForm.serviceId) as any)?.icon || 'bi-bandaid'" style="font-size: 1.3rem;"></i>
                            </div>
                            <div class="flex-grow-1">
                              <h6 class="fw-bold text-dark mb-0" style="font-size: 0.95rem;">
                                {{ getSelectedServiceName() }}<template v-if="bookForm.vaccineId"> &amp; Tiêm phòng</template>
                              </h6>
                              <small class="text-muted d-block mt-1" style="font-size: 0.75rem;">
                                {{ bookForm.vaccineId ? getSelectedVaccineName() : ((services.find(s => s.id === bookForm.serviceId) as any)?.description || 'Gói khám dịch vụ') }}
                              </small>
                            </div>
                            <div class="fw-bold text-dark ms-2 text-end" style="font-size: 0.95rem;">
                              {{ getSelectedServiceName().toLowerCase().includes('tiêm') ? 'Theo giá Vắc-xin' : (getSelectedServicePrice() ? formatCurrency(getSelectedServicePrice()) : 'Liên hệ') }}
                              <div v-if="getSelectedServiceName().toLowerCase().includes('tiêm')" class="text-success fw-normal mt-1" style="font-size: 0.7rem;">(Miễn phí công tiêm)</div>
                            </div>
                          </div>
                        </div>

                        <!-- Notes -->
                        <div class="booking-confirm-card mt-3">
                          <div class="card-header-flex border-0 pb-0 mb-2">
                            <h6 class="mb-0 fw-bold" style="color: #1e293b;"><i class="bi bi-justify-left text-muted me-2"></i> Lý do khám bệnh</h6>
                          </div>
                          <div class="p-0">
                            <textarea 
                              v-model="bookForm.symptom" 
                              class="form-control bg-light border-0 rounded-3" 
                              rows="2" 
                              style="min-height: 50px; font-size: 0.85rem;" 
                              placeholder="Nhập các triệu chứng, thói quen đặc biệt hoặc yêu cầu khác..."
                            ></textarea>
                          </div>
                        </div>
                      </div>

                      <div class="confirm-sidebar">
                        <div class="receipt-card">
                          <div class="receipt-header">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                              <span class="text-uppercase fw-bold text-muted small tracking-wide">LỊCH HẸN</span>
                              <span class="edit-link text-white opacity-75" @click="currentStep = 2">Thay đổi</span>
                            </div>
                            <h3 class="fw-bold text-white mb-1">
                              {{ formatTimeOnly(bookForm.appointmentDate) }}
                            </h3>
                            <div class="text-white d-flex align-items-center gap-2 opacity-90 small mt-2">
                              <i class="bi bi-calendar-event"></i> {{ formatDateFull(bookForm.appointmentDate) }}
                            </div>
                          </div>
                          
                          <div class="receipt-doctor p-2 pb-1">
                            <div class="d-flex align-items-center gap-3">
                              <div class="bg-primary text-white rounded-circle d-flex align-items-center justify-content-center" style="width: 36px; height: 36px; font-size: 1rem;">
                                <i class="bi bi-person-fill"></i>
                              </div>
                              <div>
                                <small class="text-muted d-block" style="font-size: 0.7rem;">Bác sĩ phụ trách</small>
                                <strong class="text-dark small">{{ selectedDoctorFilter !== 'auto' && doctorAvailableSlots.find(d => d.doctorId === selectedDoctorFilter) ? doctorAvailableSlots.find(d => d.doctorId === selectedDoctorFilter)?.doctorName : 'Hệ thống tự phân công' }}</strong>
                              </div>
                            </div>
                          </div>

                          <div class="receipt-body p-3 pt-2">
                            <h6 class="text-muted small fw-bold mb-1 tracking-wide" style="font-size: 0.75rem;">CHI TIẾT DỊCH VỤ</h6>
                            <div class="d-flex justify-content-between mb-2">
                              <span class="text-dark fw-medium" style="font-size: 0.85rem;">{{ getSelectedServiceName() }}<template v-if="bookForm.vaccineId"><br/><small class="text-muted">+ {{ getSelectedVaccineName() }}</small></template></span>
                              <span class="edit-link" @click="currentStep = 1">SỬA</span>
                            </div>

                            <h6 class="text-muted small fw-bold mb-1 tracking-wide" style="font-size: 0.75rem;">CHI TIẾT CHI PHÍ</h6>
                            <div class="d-flex justify-content-between mb-1">
                              <span class="text-muted" style="font-size: 0.8rem;">{{ getSelectedServiceName().toLowerCase().includes('tiêm') ? 'Tiền công tiêm' : 'Phí khám dịch vụ' }}</span>
                              <strong class="text-success" style="font-size: 0.85rem;">{{ getSelectedServiceName().toLowerCase().includes('tiêm') ? 'Miễn phí' : (getSelectedServicePrice() ? formatCurrency(getSelectedServicePrice()) : '0 ₫') }}</strong>
                            </div>
                            <div v-if="getSelectedServiceName().toLowerCase().includes('tiêm')" class="d-flex justify-content-between mb-1">
                              <span class="text-muted" style="font-size: 0.8rem;">Giá Vắc-xin</span>
                              <strong class="text-dark" style="font-size: 0.85rem;">Tính theo thực tế</strong>
                            </div>
                            <div class="d-flex justify-content-between mb-2 border-bottom pb-2">
                              <span class="text-muted" style="font-size: 0.8rem;">Phí mở hồ sơ mới</span>
                              <strong class="text-dark" style="font-size: 0.85rem;">0 ₫</strong>
                            </div>
                            <div class="d-flex justify-content-between align-items-center mb-1">
                              <span class="fw-bold text-dark" style="font-size: 0.9rem;">Tổng cộng</span>
                              <strong class="text-primary fs-5">{{ getSelectedServicePrice() ? formatCurrency(getSelectedServicePrice()) : '0 ₫' }}</strong>
                            </div>
                            <p class="text-center text-muted mb-0" style="font-size: 0.65rem;">Thanh toán tại phòng khám</p>
                          </div>
                        </div>


                      </div>
                    </div>
                  </div>

                </form>
              </div>


              <!-- Footer Buttons -->
              <div v-if="!bookingSuccess" class="border-top bg-white p-4 d-flex justify-content-between align-items-center" style="z-index: 10;">
                <button v-if="currentStep > 0" type="button" class="btn btn-light px-4 py-2 rounded-pill fw-bold" @click="currentStep--" :disabled="bookingLoading">
                  <i class="bi bi-arrow-left me-2"></i> Quay lại
                </button>
                <button v-else type="button" class="btn btn-light px-4 py-2 rounded-pill fw-bold" @click="closeBookModal">Hủy</button>

                <button v-if="currentStep < maxSteps" type="button" class="btn btn-primary px-4 py-2 rounded-pill fw-bold" @click="nextStep" :disabled="!canProceed">
                  Tiếp theo <i class="bi bi-arrow-right ms-2"></i>
                </button>
                <button v-else type="button" class="btn btn-primary px-5 py-2 rounded-pill fw-bold d-flex align-items-center" @click="submitBooking" :disabled="bookingLoading">
                  <span v-if="bookingLoading" class="spinner-border spinner-border-sm me-2"></span>
                  <span v-else><i class="bi bi-check2-circle me-2"></i>Xác nhận đặt lịch</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

<!-- ===== DETAIL MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDetailModal && detailAppt" class="appt-modal-overlay" @click.self="showDetailModal = false">
          <div class="appt-modal-card">
            <div class="appt-modal-header" :class="`detail-header-${detailAppt.status}`">
              <div>
                <h5 class="fw-bold mb-1">
                  <i class="bi bi-calendar-event-fill me-2"></i>Chi tiết lịch hẹn #{{ detailAppt.id }}
                </h5>
                <span class="appt-status-badge" :class="`badge-${detailAppt.status}`">
                  <i :class="getStatusIcon(detailAppt.status)" class="me-1"></i>
                  {{ getStatusLabel(detailAppt.status) }}
                </span>
              </div>
              <button class="modal-close-btn" @click="showDetailModal = false">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="appt-modal-body">
              <div class="row g-3">
                <div class="col-sm-6">
                  <div class="detail-item">
                    <div class="detail-label">Thú cưng</div>
                    <div class="detail-value">{{ detailAppt.petName }} <small class="text-muted">({{ detailAppt.species }})</small></div>
                  </div>
                </div>
                <div class="col-sm-6">
                  <div class="detail-item">
                    <div class="detail-label">Bác sĩ phụ trách</div>
                    <div class="detail-value">{{ detailAppt.doctorName || 'Chưa phân công' }}</div>
                  </div>
                </div>
                <div class="col-sm-6">
                  <div class="detail-item">
                    <div class="detail-label">Dịch vụ</div>
                    <div class="detail-value">{{ detailAppt.serviceName }}</div>
                  </div>
                </div>
                <div class="col-sm-6">
                  <div class="detail-item">
                    <div class="detail-label">Thời gian</div>
                    <div class="detail-value">{{ formatDateFull(detailAppt.appointmentDate) }}</div>
                  </div>
                </div>
                <div v-if="detailAppt.symptom" class="col-12">
                  <div class="detail-item">
                    <div class="detail-label">Triệu chứng / Lý do khám</div>
                    <div class="detail-value">{{ detailAppt.symptom }}</div>
                  </div>
                </div>
                <div v-if="detailAppt.note" class="col-12">
                  <div class="detail-item">
                    <div class="detail-label">Ghi chú</div>
                    <div class="detail-value">{{ detailAppt.note }}</div>
                  </div>
                </div>
                <div v-if="detailAppt.invoiceId" class="col-12">
                  <div class="detail-item" :class="detailAppt.invoiceStatus === 'paid' ? 'item-success' : 'item-warning'">
                    <div class="detail-label"><i class="bi bi-receipt me-1"></i>Hóa đơn</div>
                    <div class="detail-value d-flex justify-content-between align-items-center">
                      <span>{{ getInvoiceStatusLabel(detailAppt.invoiceStatus) }}</span>
                      <strong v-if="detailAppt.invoiceTotalAmount" class="text-dark">{{ formatCurrency(detailAppt.invoiceTotalAmount) }}</strong>
                    </div>
                  </div>
                </div>
                <div v-if="detailAppt.status === 'pending'" class="col-12">
                  <div class="detail-item text-center p-3" style="background: #fafafa; border: 1px solid #e2e8f0; border-radius: 12px;">
                    <div class="detail-label"><i class="bi bi-info-circle me-1 text-primary"></i>Mã QR Check-in</div>
                    <div class="small text-muted mt-2">Vui lòng chờ phòng khám xác nhận lịch hẹn. Mã QR sẽ hiển thị tại đây sau khi lịch được xác nhận.</div>
                  </div>
                </div>
                <div v-else-if="detailAppt.status === 'confirmed' && detailAppt.qrToken" class="col-12">
                  <div id="appointment-detail-qr-box" class="detail-item text-center p-3" style="background: #fafafa; border: 1.5px dashed #10b981; border-radius: 12px;">
                    <div class="detail-label"><i class="bi bi-qr-code me-1"></i>Mã QR Check-in</div>
                    <div class="d-flex justify-content-center my-3 position-relative">
                      <qrcode-vue :value="detailAppt.qrToken" :size="150" level="M" :margin="3" />
                      <div class="position-absolute w-100 h-100 d-flex align-items-center justify-content-center" style="background: rgba(255,255,255,0.8); cursor: pointer; transition: all 0.2s;" @click="$router.push(`/qr-checkin/${detailAppt.id}`)" onmouseover="this.style.background='rgba(255,255,255,0.5)'" onmouseout="this.style.background='rgba(255,255,255,0.8)'">
                        <span class="btn btn-primary btn-sm rounded-pill shadow-sm fw-bold"><i class="bi bi-arrows-fullscreen me-1"></i>Mở thẻ</span>
                      </div>
                    </div>
                    <div class="fs-5 fw-bold text-dark font-monospace mb-2">{{ detailAppt.qrToken }}</div>
                    <button class="btn btn-outline-success btn-sm rounded-pill w-100 fw-bold" @click="$router.push(`/qr-checkin/${detailAppt.id}`)">
                      <i class="bi bi-box-arrow-up-right me-1"></i> Xem thẻ Check-in lớn
                    </button>
                  </div>
                </div>
                <div v-else-if="detailAppt.status && ['waiting', 'in_progress', 'ready_to_pay', 'completed'].includes(detailAppt.status)" class="col-12">
                  <div class="detail-item text-center p-3" style="background: #f0fdf4; border: 1px solid #10b981; border-radius: 12px;">
                    <div class="detail-label text-success"><i class="bi bi-check-circle-fill me-1"></i>Trạng thái Check-in</div>
                    <div class="small text-success fw-bold mt-2">Bạn đã check-in thành công.</div>
                  </div>
                </div>
              </div>

              <div class="d-flex gap-2 mt-4">
                <button
                  v-if="canCancel(detailAppt.status)"
                  class="btn btn-outline-danger rounded-pill fw-semibold flex-fill"
                  @click="confirmCancelFromDetail(detailAppt)"
                >
                  <i class="bi bi-x-circle me-2"></i>Huỷ lịch hẹn này
                </button>
                <button class="btn btn-outline-secondary rounded-pill px-4" @click="showDetailModal = false">Đóng</button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ===== CANCEL CONFIRM MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showCancelModal && apptToCancel" class="appt-modal-overlay" @click.self="showCancelModal = false">
          <div class="appt-modal-card" style="max-width: 420px;">
            <div class="appt-modal-header">
              <h5 class="fw-bold mb-0"><i class="bi bi-info-circle-fill text-warning me-2"></i>Không thể tự huỷ lịch</h5>
              <button class="modal-close-btn" @click="showCancelModal = false"><i class="bi bi-x-lg"></i></button>
            </div>
            <div class="appt-modal-body text-center">
              <div style="font-size: 3.5rem; margin-bottom: 1rem;">📞</div>
              <p class="text-muted mb-4">Chức năng tự huỷ lịch trên hệ thống đã được tắt để đảm bảo công tác điều phối phòng khám.<br><br>
              Để huỷ lịch hẹn <strong>{{ formatDateFull(apptToCancel.appointmentDate) }}</strong>, quý khách vui lòng liên hệ trực tiếp với phòng khám qua Hotline: <strong>1900 1234</strong>.</p>
              <div class="d-flex gap-2 justify-content-center">
                <button class="btn btn-warning text-dark rounded-pill px-4 fw-bold" @click="showCancelModal = false">Đã hiểu</button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
    <!-- ===== QR LIST MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showQrListModal" class="appt-modal-overlay" @click.self="showQrListModal = false">
          <div class="appt-modal-card">
            <div class="appt-modal-header border-bottom mb-3 pb-3">
              <h5 class="fw-bold mb-0">
                <i class="bi bi-qr-code-scan me-2 text-success"></i>Mã QR Check-in của tôi
              </h5>
              <button class="modal-close-btn" @click="showQrListModal = false">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="appt-modal-body text-center">
              <div v-if="confirmedAppointments.length === 0" class="py-4">
                <i class="bi bi-box-seam text-muted" style="font-size: 3rem;"></i>
                <h6 class="mt-3 text-dark fw-bold">Chưa có mã QR nào</h6>
                <p class="text-muted small mb-0">Bạn hiện không có lịch hẹn nào đang chờ check-in. Mã QR chỉ hiển thị khi phòng khám đã duyệt lịch hẹn của bạn.</p>
              </div>

              <div v-else class="qr-single-view text-start">
                
                <!-- Appointment Selector -->
                <div class="mb-4">
                  <label class="form-label text-muted small fw-bold text-uppercase mb-2">CHỌN LỊCH HẸN</label>
                  <select v-model="selectedQrApptId" class="form-select form-select-lg shadow-sm fw-bold text-dark" style="border: 2px solid #0f766e; border-radius: 12px; cursor: pointer;">
                    <option v-for="appt in confirmedAppointments" :key="appt.id" :value="appt.id">
                      #{{ appt.id }} · {{ formatTime(appt.appointmentDate) }} {{ formatDateOnly(appt.appointmentDate) }} — {{ appt.petName }}
                    </option>
                  </select>
                </div>

                <div v-if="selectedQrAppt" class="p-4 rounded-4" style="background-color: #f8fafc; border: 1px solid #f1f5f9;">
                  
                  <!-- Info Grid -->
                  <div class="row g-3 mb-4">
                    <div class="col-6">
                      <div class="text-muted small mb-1">Ngày khám</div>
                      <div class="fw-bold text-dark">{{ formatDateOnly(selectedQrAppt.appointmentDate) }}</div>
                    </div>
                    <div class="col-6">
                      <div class="text-muted small mb-1">Giờ</div>
                      <div class="fw-bold text-dark">{{ formatTime(selectedQrAppt.appointmentDate) }}</div>
                    </div>
                    <div class="col-12 mt-2">
                      <div class="text-muted small mb-1">Bác sĩ / Dịch vụ</div>
                      <div class="text-dark fw-medium">{{ selectedQrAppt.doctorName || 'Chưa phân công' }} <span class="text-muted px-1">•</span> {{ selectedQrAppt.serviceName }}</div>
                    </div>
                  </div>

                  <!-- QR Display -->
                  <div class="bg-white p-3 rounded-4 shadow-sm border text-center mb-4">
                    <qrcode-vue :value="selectedQrAppt.qrToken || ''" :size="160" level="M" :margin="3" />
                    <div class="fs-5 fw-bolder text-primary font-monospace mt-2 mb-1">{{ selectedQrAppt.qrToken }}</div>
                  </div>
                  
                  <button class="btn w-100 rounded-pill fw-bold py-2" style="background-color: #0f766e; color: white;" @click="$router.push(`/qr-checkin/${selectedQrAppt.id}`)">
                    <i class="bi bi-box-arrow-up-right me-2"></i>Mở thẻ Check-in
                  </button>
                  
                </div>

              </div>

              <div class="mt-2 text-center">
                <button class="btn btn-outline-secondary rounded-pill px-4" @click="showQrListModal = false">Đóng</button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import api from '../../services/api';
import QrcodeVue from 'qrcode.vue';

// ===== Emits =====
const emit = defineEmits<{
  (e: 'switch-tab', tab: string): void;
}>();

// ===== Types =====
interface AppointmentDetail {
  id: number;
  petId: number;
  petName: string | null;
  species: string | null;
  breed: string | null;
  weight: number | null;
  isAggressive: boolean;
  customerId: string;
  customerName: string | null;
  customerPhone: string | null;
  serviceId: number;
  serviceName: string | null;
  servicePrice: number | null;
  doctorId: string | null;
  doctorName: string | null;
  appointmentDate: string;
  symptom: string | null;
  note: string | null;
  status: string | null;
  qrToken: string | null;
  invoiceId: number | null;
  invoiceStatus: string | null;
  invoiceTotalAmount: number | null;
}

interface Pet {
  id: number;
  name: string;
  species: string;
}

interface Service {
  id: number;
  name: string;
  price: number | null;
}

// ===== State =====
const appointments = ref<AppointmentDetail[]>([]);
const myPets = ref<Pet[]>([]);
const services = ref<Service[]>([]);
const loading = ref(false);
const errorMsg = ref('');

const activeFilter = ref('all');
const currentPage = ref(1);
const pageSize = ref(5);
const totalCount = ref(0);
const totalPages = ref(1);

// Detail modal
const showDetailModal = ref(false);
const detailAppt = ref<AppointmentDetail | null>(null);

// Cancel modal
const showCancelModal = ref(false);
const apptToCancel = ref<AppointmentDetail | null>(null);
const cancelLoading = ref(false);

// QR List modal
const showQrListModal = ref(false);
const selectedQrApptId = ref<number | null>(null);

const confirmedAppointments = computed(() => {
  return appointments.value.filter(a => a.status === 'confirmed' && a.qrToken);
});

const selectedQrAppt = computed(() => {
  return confirmedAppointments.value.find(a => a.id === selectedQrApptId.value);
});

const openQrModal = () => {
  if (confirmedAppointments.value.length > 0) {
    selectedQrApptId.value = confirmedAppointments.value[0].id;
  }
  showQrListModal.value = true;
};

// Book modal
const showBookModal = ref(false);
const currentStep = ref(0);
const bookingLoading = ref(false);
const bookingError = ref('');
const bookingSuccess = ref(false);

const bookForm = ref({
  petId: 0,
  serviceId: 0,
  appointmentDate: '',
  symptom: '',
  note: '',
  vaccineId: null as number | null,
  doctorId: null as string | null,
});

const selectedBookingDate = ref('');
const selectedDoctorFilter = ref<string>('auto');
const doctorAvailableSlots = ref<Array<{ doctorId: string; doctorName: string; availableSlots: string[] }>>([]);
const fetchingSlots = ref(false);
const slotFetchError = ref('');

const computedAvailableSlots = computed(() => {
  if (selectedDoctorFilter.value === 'auto') {
    const allSlots: string[] = [];
    doctorAvailableSlots.value.forEach(doc => {
      doc.availableSlots.forEach(slot => {
        if (!allSlots.includes(slot)) {
          allSlots.push(slot);
        }
      });
    });
    return allSlots.sort((a, b) => a.localeCompare(b));
  } else {
    const doc = doctorAvailableSlots.value.find(d => d.doctorId === selectedDoctorFilter.value);
    return doc ? doc.availableSlots : [];
  }
});

// Bỏ bước chọn Vaccine: luôn trả về false để wizard chỉ có 4 bước (không có bước Vaccine)
const isVaccinationService = computed(() => false);

const bookingSteps = computed(() => ['Thú cưng', 'Dịch vụ', 'Thời gian', 'Xác nhận']);

const maxSteps = computed(() => 3);

const vaccines = ref<Vaccine[]>([]);
const vaccineValidation = ref<ValidationResult | null>(null);
const checkingValidation = ref(false);
const lastBookedAppt = ref<AppointmentDetail | null>(null);


interface Vaccine {
  id: number;
  name: string;
  manufacturer: string | null;
  description: string | null;
  stockQuantity: number;
  targetSpecies: string | null;
  minAgeWeeks: number | null;
  intervalDays: number | null;
}

interface ValidationResult {
  isValid: boolean;
  warningMessage: string | null;
  requiresDoctorOverride: boolean;
  nextAvailableDate: string | null;
}

// ===== Filter Options =====
const filterOptions = [
  { value: 'all', label: 'Tất cả', icon: 'bi bi-list-ul' },
  { value: 'pending', label: 'Chờ xác nhận', icon: 'bi bi-hourglass-split' },
  { value: 'confirmed', label: 'Đã xác nhận', icon: 'bi bi-check-circle' },
  { value: 'completed', label: 'Hoàn thành', icon: 'bi bi-check2-all' },
  { value: 'cancelled', label: 'Đã huỷ', icon: 'bi bi-x-circle' },
];


// ===== Calendar State & Computed =====
const currentMonth = ref(new Date().getMonth());
const currentYear = ref(new Date().getFullYear());

const toLocalDateStr = (d: Date) => {
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
};

const calendarDays = computed(() => {
  const firstDay = new Date(currentYear.value, currentMonth.value, 1).getDay();
  const daysInMonth = new Date(currentYear.value, currentMonth.value + 1, 0).getDate();
  const daysInPrevMonth = new Date(currentYear.value, currentMonth.value, 0).getDate();
  
  const days = [];
  const today = new Date();
  today.setHours(0,0,0,0);
  
  for (let i = 0; i < firstDay; i++) {
    const d = new Date(currentYear.value, currentMonth.value - 1, daysInPrevMonth - firstDay + i + 1);
    days.push({
      dayNum: d.getDate(),
      dateStr: toLocalDateStr(d),
      isCurrentMonth: false,
      isPast: d < today,
      isToday: false,
      hasAvailability: false
    });
  }
  
  for (let i = 1; i <= daysInMonth; i++) {
    const d = new Date(currentYear.value, currentMonth.value, i);
    days.push({
      dayNum: i,
      dateStr: toLocalDateStr(d),
      isCurrentMonth: true,
      isPast: d < today,
      isToday: d.getTime() === today.getTime(),
      hasAvailability: ! (d < today)
    });
  }
  
  const remaining = 42 - days.length;
  for (let i = 1; i <= remaining; i++) {
    const d = new Date(currentYear.value, currentMonth.value + 1, i);
    days.push({
      dayNum: d.getDate(),
      dateStr: toLocalDateStr(d),
      isCurrentMonth: false,
      isPast: d < today,
      isToday: false,
      hasAvailability: false
    });
  }
  return days;
});

const prevMonth = () => {
  if (currentMonth.value === 0) {
    currentMonth.value = 11;
    currentYear.value--;
  } else {
    currentMonth.value--;
  }
};

const nextMonth = () => {
  if (currentMonth.value === 11) {
    currentMonth.value = 0;
    currentYear.value++;
  } else {
    currentMonth.value++;
  }
};

const onCalendarDateSelect = (dateStr: string) => {
  selectedBookingDate.value = dateStr;
  onBookingDateChange();
};


const masterMorningTimes = ['08:00', '08:30', '09:00', '09:30', '10:00', '10:30', '11:00', '11:30'];
const masterAfternoonTimes = ['13:30', '14:00', '14:30', '15:00', '15:30', '16:00', '16:30', '17:00', '17:30', '18:00', '18:30', '19:00', '19:30'];

interface SlotDisplay {
  time: string;
  slotStr: string;
  isAvailable: boolean;
  isPast: boolean;
  isBooked: boolean;
  isTooSoon: boolean;
}

// Buffer: slots must be at least 15 minutes from now to be bookable
const BOOKING_BUFFER_MS = 15 * 60 * 1000;

const buildSlots = (times: string[]): SlotDisplay[] => {
  if (!selectedBookingDate.value) return [];
  const now = Date.now();
  const cutoff = now + BOOKING_BUFFER_MS;
  const [year, month, day] = selectedBookingDate.value.split('-');
  return times.map(time => {
    const slotStr = `${selectedBookingDate.value}T${time}:00`;
    const [hour, minute] = time.split(':');
    const slotDate = new Date(parseInt(year), parseInt(month) - 1, parseInt(day), parseInt(hour), parseInt(minute), 0);
    const slotMs = slotDate.getTime();
    const isPast = slotMs < now;
    const isTooSoon = !isPast && slotMs < cutoff;
    const isAvailableFromApi = computedAvailableSlots.value.includes(time);
    const isBooked = !isPast && !isTooSoon && !isAvailableFromApi;
    const isAvailable = !isPast && !isTooSoon && isAvailableFromApi;
    return { time, slotStr, isAvailable, isPast, isBooked, isTooSoon };
  });
};

const displayMorningSlots = computed<SlotDisplay[]>(() => buildSlots(masterMorningTimes));
const displayAfternoonSlots = computed<SlotDisplay[]>(() => buildSlots(masterAfternoonTimes));

// ===== End Calendar State =====

// ===== Computed =====
const filteredAppointments = computed(() => {
  return appointments.value;
});

const minDateOnlyStr = computed(() => {
  const now = new Date();
  return now.toISOString().split('T')[0];
});


const canProceed = computed(() => {
  if (currentStep.value === 0) {
    if (bookForm.value.petId <= 0) return false;
    if (selectedPetActiveAppts.value.length > 0 && !petWarningDismissed.value) return false;
    return true;
  }
  if (currentStep.value === 1) return bookForm.value.serviceId > 0;
  if (currentStep.value === 2) return bookForm.value.appointmentDate !== '';
  return true;
});

// ===== Pet Duplicate Warning =====
const ACTIVE_STATUSES = ['pending', 'confirmed', 'waiting', 'in_progress'];

// Map petId → active appointments (pending/confirmed/waiting/in_progress)
const petActiveAppointments = computed(() => {
  const map = new Map<number, AppointmentDetail[]>();
  for (const appt of appointments.value) {
    if (appt.petId && ACTIVE_STATUSES.includes(appt.status ?? '')) {
      if (!map.has(appt.petId)) map.set(appt.petId, []);
      map.get(appt.petId)!.push(appt);
    }
  }
  return map;
});

// Active appointments for the currently selected pet in the booking wizard
const selectedPetActiveAppts = computed(() => {
  if (!bookForm.value.petId) return [];
  return petActiveAppointments.value.get(bookForm.value.petId) ?? [];
});

// Whether user has dismissed the warning (allow them to proceed anyway)
const petWarningDismissed = ref(false);

const selectPet = (petId: number) => {
  if (bookForm.value.petId === petId) return; // deselect logic: keep selected
  bookForm.value.petId = petId;
  petWarningDismissed.value = false; // reset dismissal when changing pet
};


// ===== API =====
const fetchAppointments = async () => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get('/my-appointments', {
      params: {
        page: currentPage.value,
        pageSize: pageSize.value,
        status: activeFilter.value === 'all' ? null : activeFilter.value
      }
    });
    appointments.value = res.data.items;
    totalCount.value = res.data.totalCount;
    totalPages.value = res.data.totalPages;
  } catch (err: any) {
    errorMsg.value = 'Không thể tải lịch hẹn. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
};

const changeFilter = (val: string) => {
  activeFilter.value = val;
  currentPage.value = 1;
  fetchAppointments();
};

const prevPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--;
    fetchAppointments();
  }
};

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
    fetchAppointments();
  }
};

const fetchPets = async () => {
  try {
    const res = await api.get('/mypets');
    myPets.value = res.data;
  } catch { /* silent */ }
};

const fetchServices = async () => {
  try {
    const res = await api.get('/my-appointments/services');
    services.value = res.data.map((s: any) => {
      // Add default descriptions for specific services if they lack one
      if (s.name.toLowerCase().includes('khám') && !s.description) {
        return { ...s, description: 'Kiểm tra sức khỏe tổng quát, chẩn đoán và tư vấn điều trị cho thú cưng của bạn.' };
      }
      if (s.name.toLowerCase().includes('tiêm') && !s.description) {
        return { ...s, description: 'Tiêm các loại vaccine cần thiết định kỳ để phòng ngừa bệnh truyền nhiễm cho thú cưng.' };
      }
      return s;
    });
  } catch { /* silent */ }
};

const submitBooking = async () => {
  bookingLoading.value = true;
  bookingError.value = '';
  try {
    const res = await api.post('/my-appointments', {
      petId: bookForm.value.petId,
      serviceId: bookForm.value.serviceId,
      appointmentDate: bookForm.value.appointmentDate,
      symptom: bookForm.value.symptom,
      note: bookForm.value.note,
      vaccineId: bookForm.value.vaccineId,
      doctorId: bookForm.value.doctorId,
    });
    const appointmentId = res.data.id;
    try {
      const detailRes = await api.get(`/my-appointments/${appointmentId}`);
      lastBookedAppt.value = detailRes.data;
    } catch {
      lastBookedAppt.value = null;
    }
    bookingSuccess.value = true;
    await fetchAppointments();
  } catch (err: any) {
    bookingError.value = err?.response?.data?.message || 'Đặt lịch thất bại. Vui lòng thử lại.';
  } finally {
    bookingLoading.value = false;
  }
};

const onBookingDateChange = async () => {
  bookForm.value.appointmentDate = '';
  bookForm.value.doctorId = null;
  selectedDoctorFilter.value = 'auto';
  if (!selectedBookingDate.value) {
    doctorAvailableSlots.value = [];
    return;
  }
  
  fetchingSlots.value = true;
  slotFetchError.value = '';
  try {
    const res = await api.get('/my-appointments/available-slots', {
      params: { 
        date: selectedBookingDate.value,
        serviceId: bookForm.value.serviceId
      }
    });
    doctorAvailableSlots.value = res.data;
  } catch (err: any) {
    slotFetchError.value = err?.response?.data?.message || 'Không thể tải danh sách khung giờ trống.';
    doctorAvailableSlots.value = [];
  } finally {
    fetchingSlots.value = false;
  }
};

const selectTimeSlot = (doctorId: string | null, slotStr: string) => {
  bookForm.value.appointmentDate = slotStr;
  bookForm.value.doctorId = selectedDoctorFilter.value === 'auto' ? null : doctorId;
};

const formatTimeOnly = (dateStr: string): string => {
  if (!dateStr) return '';
  return new Date(dateStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};

const cancelAppointment = async () => {
  if (!apptToCancel.value) return;
  cancelLoading.value = true;
  try {
    await api.put(`/my-appointments/${apptToCancel.value.id}/cancel`);
    await fetchAppointments();
    showCancelModal.value = false;
    showDetailModal.value = false;
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Huỷ lịch thất bại. Vui lòng thử lại.');
  } finally {
    cancelLoading.value = false;
  }
};

// ===== Modal controls =====
const openBookModal = async () => {
  bookForm.value = { petId: 0, serviceId: 0, appointmentDate: '', symptom: '', note: '', vaccineId: null, doctorId: null };
  selectedBookingDate.value = '';
  doctorAvailableSlots.value = [];
  currentStep.value = 0;
  bookingError.value = '';
  bookingSuccess.value = false;
  vaccineValidation.value = null;
  lastBookedAppt.value = null;
  petWarningDismissed.value = false;
  await fetchPets();
  await fetchServices();
  await fetchVaccines();
  showBookModal.value = true;
};


const closeBookModal = () => {
  showBookModal.value = false;
};

const nextStep = () => {
  console.log('nextStep called. currentStep:', currentStep.value, 'maxSteps:', maxSteps.value, 'canProceed:', canProceed.value);
  console.log('bookForm.serviceId:', bookForm.value.serviceId, 'type:', typeof bookForm.value.serviceId);
  console.log('isVaccinationService:', isVaccinationService.value);
  if (!canProceed.value) {
    console.log('Cannot proceed!');
    return;
  }
  if (currentStep.value < maxSteps.value) {
    console.log('Incrementing currentStep');
    currentStep.value++;
    if (currentStep.value === 2 && !selectedBookingDate.value) {
      console.log('Setting default date');
      selectedBookingDate.value = toLocalDateStr(new Date());
      currentMonth.value = new Date().getMonth();
      currentYear.value = new Date().getFullYear();
      onBookingDateChange();
    }
  } else {
    console.log('Already at max steps');
  }
};

const openDetailModal = (appt: AppointmentDetail) => {
  detailAppt.value = appt;
  showDetailModal.value = true;
};

const confirmCancel = (appt: AppointmentDetail) => {
  apptToCancel.value = appt;
  showCancelModal.value = true;
};

const confirmCancelFromDetail = (appt: AppointmentDetail) => {
  showDetailModal.value = false;
  apptToCancel.value = appt;
  showCancelModal.value = true;
};

// ===== Helpers =====
const canCancel = (status: string | null): boolean => {
  return status === 'pending' || status === 'confirmed';
};

const getStatusLabel = (status: string | null): string => {
  const map: Record<string, string> = {
    pending: 'Chờ xác nhận',
    pending_approval: 'Chờ duyệt đặc biệt',
    confirmed: 'Đã xác nhận',
    waiting: 'Chờ khám',
    in_progress: 'Đang khám',
    completed: 'Hoàn thành',
    cancelled: 'Đã huỷ',
  };
  return map[status ?? ''] || (status ?? 'Không rõ');
};

const getStatusBadgeClass = (status: string): string => {
  const map: Record<string, string> = {
    pending: 'bg-warning text-dark',
    pending_approval: 'bg-danger bg-opacity-75 text-white',
    confirmed: 'bg-info text-white',
    waiting: 'bg-primary text-white',
    in_progress: 'bg-warning text-dark',
    completed: 'bg-success text-white',
    cancelled: 'bg-danger text-white',
  };
  return map[status] ?? 'bg-secondary text-white';
};

const getStatusIcon = (status: string | null): string => {
  const map: Record<string, string> = {
    pending: 'bi bi-hourglass-split',
    pending_approval: 'bi bi-exclamation-octagon-fill',
    confirmed: 'bi bi-check-circle-fill',
    waiting: 'bi bi-person-lines-fill',
    in_progress: 'bi bi-activity',
    completed: 'bi bi-check2-all',
    cancelled: 'bi bi-x-circle-fill',
  };
  return map[status ?? ''] || 'bi bi-question-circle';
};

const getInvoiceStatusLabel = (status: string | null | undefined): string => {
  const map: Record<string, string> = {
    unpaid: 'Chưa thanh toán',
    paid: 'Đã thanh toán',
    cancelled: 'Đã huỷ HĐ',
  };
  return map[status ?? ''] || (status ?? '');
};

const getSpeciesEmoji = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': '🐕', 'Mèo': '🐈', 'Thỏ': '🐇', 'Chim': '🦜', 'Cá': '🐟', 'Bò sát': '🦎',
  };
  return map[species ?? ''] || '🐾';
};

const getSelectedPetName = (): string => {
  const pet = myPets.value.find(p => p.id === bookForm.value.petId);
  return pet ? `${getSpeciesEmoji(pet.species)} ${pet.name}` : '—';
};

const getSelectedServiceName = (): string => {
  const svc = services.value.find(s => s.id === bookForm.value.serviceId);
  return svc?.name ?? '—';
};

const getSelectedServicePrice = (): number => {
  const svc = services.value.find(s => s.id === bookForm.value.serviceId);
  return (svc as any)?.price || 0;
};

const getSelectedPetObj = (): any => {
  return myPets.value.find(p => p.id === bookForm.value.petId);
};

const calculateAge = (birthDate: string): string => {
  if (!birthDate) return '--';
  const birth = new Date(birthDate);
  const now = new Date();
  let years = now.getFullYear() - birth.getFullYear();
  let months = now.getMonth() - birth.getMonth();
  if (months < 0) {
    years--;
    months += 12;
  }
  if (years > 0) return `${years} tuổi${months > 0 ? ` ${months} tháng` : ''}`;
  return `${months} tháng`;
};

const getSelectedVaccineName = (): string => {
  const vac = vaccines.value.find(v => v.id === bookForm.value.vaccineId);
  return vac ? vac.name : '—';
};

const fetchVaccines = async () => {
  try {
    const res = await api.get('/my-appointments/vaccines');
    vaccines.value = res.data;
  } catch { /* silent */ }
};

const selectedPetSpecies = computed(() => {
  const pet = myPets.value.find(p => p.id === bookForm.value.petId);
  return pet?.species || '';
});

const filteredVaccines = computed(() => {
  if (!selectedPetSpecies.value) return vaccines.value;
  const petSpeciesLower = selectedPetSpecies.value.toLowerCase();
  return vaccines.value.filter(v => {
    if (!v.targetSpecies || v.targetSpecies.toLowerCase() === 'all') return true;
    const targetLower = v.targetSpecies.toLowerCase();
    return petSpeciesLower.includes(targetLower) || targetLower.includes(petSpeciesLower);
  });
});

const selectVaccine = async (id: number) => {
  bookForm.value.vaccineId = id;
  await validateVaccineChoice();
};

const validateVaccineChoice = async () => {
  if (!bookForm.value.petId || !bookForm.value.vaccineId || !bookForm.value.appointmentDate) {
    vaccineValidation.value = null;
    return;
  }
  checkingValidation.value = true;
  vaccineValidation.value = null;
  try {
    const res = await api.post('/my-appointments/validate-vaccine', {
      petId: bookForm.value.petId,
      vaccineId: bookForm.value.vaccineId,
      targetDate: bookForm.value.appointmentDate,
    });
    vaccineValidation.value = res.data;
  } catch (err: any) {
    vaccineValidation.value = {
      isValid: false,
      warningMessage: err?.response?.data?.message || 'Không thể kiểm tra phác đồ tiêm chủng.',
      requiresDoctorOverride: false,
      nextAvailableDate: null
    };
  } finally {
    checkingValidation.value = false;
  }
};

const formatDay = (dateStr: string): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).getDate().toString().padStart(2, '0');
};

const formatMonthYear = (dateStr: string): string => {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return `Th${d.getMonth() + 1}/${d.getFullYear()}`;
};

const formatTime = (dateStr: string): string => {
  if (!dateStr) return '';
  const time = new Date(dateStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  if (time === '00:00' || time === '24:00') return 'Chưa có thông tin giờ';
  return time;
};

const formatDateFull = (dateStr: string): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('vi-VN', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

const formatDatetimeLocal = (val: string): string => {
  if (!val) return '—';
  let date = new Date(val);
  if (isNaN(date.getTime())) {
    date = new Date(val.replace('T', ' '));
  }
  if (isNaN(date.getTime())) return val;
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

const formatDateOnly = (dateStr: string): string => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  if (isNaN(date.getTime())) return dateStr;
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  return `${day}/${month}/${year}`;
};

const formatCurrency = (amount: number | null | undefined): string => {
  if (!amount) return '';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

// ===== Lifecycle =====
onMounted(fetchAppointments);

defineExpose({
  openBookModal
});
</script>

<style scoped>
/* ===== Layout ===== */
.myappts-tab { padding: 0; }

/* ===== Hero ===== */
.appts-hero {
  background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
  border-radius: 16px;
  padding: 1.5rem 2rem;
  border: 1px solid #bbf7d0;
}

/* ===== Premium Button ===== */
.btn-premium-appt {
  background: linear-gradient(135deg, #10b981, #059669);
  color: white;
  border: none;
  padding: 0.6rem 1.4rem;
  border-radius: 50px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(16, 185, 129, 0.35);
  display: inline-flex;
  align-items: center;
}

.btn-premium-appt:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(16, 185, 129, 0.45);
}

/* ===== Filter Tabs ===== */
.filter-tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.filter-tab-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 6px 14px;
  border-radius: 20px;
  border: 1.5px solid #e5e7eb;
  background: white;
  font-size: 0.82rem;
  font-weight: 500;
  color: #6b7280;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
}

.filter-tab-btn:hover {
  border-color: #10b981;
  color: #059669;
}

.filter-tab-btn.active {
  background: #10b981;
  border-color: #10b981;
  color: white;
  font-weight: 700;
}

.tab-count {
  background: rgba(255,255,255,0.3);
  border-radius: 10px;
  padding: 1px 7px;
  font-size: 0.75rem;
  font-weight: 700;
  margin-left: 2px;
}

.filter-tab-btn:not(.active) .tab-count {
  background: #f3f4f6;
  color: #374151;
}

/* ===== Empty State ===== */
.empty-state-appt {
  background: white;
  border-radius: 20px;
  padding: 4rem 2rem;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0,0,0,0.06);
  border: 2px dashed #bbf7d0;
}
.empty-icon { font-size: 5rem; }

/* ===== Appointment Cards ===== */
.appts-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

/* ===== Appointment Card — Redesigned ===== */
.appt-card {
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.06);
  border: 1px solid #f1f5f9;
  overflow: hidden;
  transition: box-shadow 0.25s ease;
  border-top: 3px solid transparent;
}

.appt-card:hover {
  box-shadow: 0 4px 20px rgba(0,0,0,0.1);
}

/* Top border color by status */
.status-pending    { border-top-color: #f59e0b; }
.status-confirmed  { border-top-color: #3b82f6; }
.status-waiting    { border-top-color: #f59e0b; }
.status-in_progress { border-top-color: #8b5cf6; }
.status-completed  { border-top-color: #10b981; }
.status-cancelled  { border-top-color: #9ca3af; }
.status-ready_to_pay { border-top-color: #10b981; }

/* Inner flex layout */
.appt-card-inner {
  display: flex;
  align-items: stretch;
  padding: 1.1rem 1.25rem;
  gap: 1.25rem;
}

/* LEFT: Date Pill Block */
.appt-date-pill {
  flex-shrink: 0;
  width: 80px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  padding: 0.75rem 0.5rem;
  text-align: center;
  gap: 2px;
}

.status-pending    .appt-date-pill,
.status-waiting    .appt-date-pill { background: #fef3c7; }
.status-confirmed  .appt-date-pill { background: #dbeafe; }
.status-in_progress .appt-date-pill { background: #ede9fe; }
.status-completed  .appt-date-pill,
.status-ready_to_pay .appt-date-pill { background: #d1fae5; }
.status-cancelled  .appt-date-pill { background: #f3f4f6; }

.appt-date-pill-day {
  font-size: 2rem;
  font-weight: 800;
  line-height: 1;
  color: #1e293b;
}

.status-pending    .appt-date-pill-day,
.status-waiting    .appt-date-pill-day { color: #92400e; }
.status-confirmed  .appt-date-pill-day { color: #1e40af; }
.status-in_progress .appt-date-pill-day { color: #5b21b6; }
.status-completed  .appt-date-pill-day,
.status-ready_to_pay .appt-date-pill-day { color: #065f46; }
.status-cancelled  .appt-date-pill-day { color: #6b7280; }

.appt-date-pill-month {
  font-size: 0.7rem;
  font-weight: 600;
  opacity: 0.7;
  color: inherit;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.appt-date-pill-time {
  font-size: 0.75rem;
  font-weight: 700;
  margin-top: 6px;
  padding: 3px 8px;
  border-radius: 20px;
  background: rgba(255,255,255,0.6);
  color: #374151;
  white-space: nowrap;
}

/* Divider */
.appt-divider {
  width: 1px;
  background: #f1f5f9;
  flex-shrink: 0;
  align-self: stretch;
}

/* RIGHT side */
.appt-right {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.65rem;
  min-width: 0;
}

.appt-right-top {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 6px;
}

/* Status badge */
.appt-status-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.78rem;
  font-weight: 700;
  white-space: nowrap;
}

.badge-pending, .badge-waiting { background: #fef3c7; color: #92400e; }
.badge-confirmed               { background: #dbeafe; color: #1e40af; }
.badge-in_progress             { background: #ede9fe; color: #5b21b6; }
.badge-completed, .badge-ready_to_pay { background: #d1fae5; color: #065f46; }
.badge-cancelled               { background: #f3f4f6; color: #4b5563; }

.invoice-badge {
  display: inline-flex;
  align-items: center;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 0.72rem;
  font-weight: 600;
  background: #fffbeb;
  color: #92400e;
  border: 1px solid #fde68a;
}

/* Info grid — 3 fixed columns */
.appt-info-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.5rem 1rem;
}

.appt-info-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.appt-info-label {
  font-size: 0.68rem;
  color: #9ca3af;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.appt-info-value {
  font-size: 0.875rem;
  font-weight: 600;
  color: #1e293b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.pet-inline-badge {
  background: #fef3c7;
  color: #92400e;
  padding: 1px 8px;
  border-radius: 10px;
  font-size: 0.8rem;
  font-weight: 700;
}

.appt-symptom {
  font-weight: 400;
  color: #6b7280;
  font-size: 0.85rem;
}

/* Note */
.appt-note {
  font-size: 0.8rem;
  color: #6b7280;
  background: #f9fafb;
  border-radius: 8px;
  padding: 6px 10px;
  border-left: 3px solid #e5e7eb;
}

/* Actions */
.appt-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  margin-top: 2px;
}

.btn-appt-detail {
  background: transparent;
  border: 1.5px solid #e5e7eb;
  border-radius: 20px;
  padding: 5px 14px;
  font-size: 0.82rem;
  font-weight: 600;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-appt-detail:hover {
  border-color: #10b981;
  color: #059669;
  background: #f0fdf4;
}

.btn-appt-cancel {
  background: transparent;
  border: 1.5px solid #fee2e2;
  border-radius: 20px;
  padding: 5px 14px;
  font-size: 0.82rem;
  font-weight: 600;
  color: #dc2626;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-appt-cancel:hover {
  background: #fee2e2;
}

.appt-price-tag {
  margin-left: auto;
  font-size: 0.9rem;
  font-weight: 700;
  color: #059669;
  background: #d1fae5;
  padding: 4px 12px;
  border-radius: 20px;
}

@keyframes pulse-dot {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}

/* ===== Cards animation ===== */
.appt-card-enter-active, .appt-card-leave-active { transition: all 0.35s ease; }
.appt-card-enter-from { opacity: 0; transform: translateX(-20px); }
.appt-card-leave-to { opacity: 0; transform: translateX(20px); }

@media (max-width: 640px) {
  .appt-card-inner {
    flex-direction: column;
    gap: 0.75rem;
  }
  .appt-date-pill {
    width: 100%;
    flex-direction: row;
    justify-content: flex-start;
    gap: 0.75rem;
    padding: 0.6rem 0.75rem;
  }
  .appt-date-pill-day { font-size: 1.4rem; }
  .appt-divider { width: 100%; height: 1px; }
  .appt-info-grid { grid-template-columns: 1fr 1fr; }
}

/* ===== Modal ===== */
.appt-modal-overlay {
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

.appt-modal-card {
  background: white;
  border-radius: 20px;
  width: 100%;
  max-width: 640px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 25px 60px rgba(0,0,0,0.2);
}

.appt-modal-header {
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

.appt-modal-body { padding: 1.5rem; }

.modal-close-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 1.1rem;
  color: #6b7280;
  padding: 0.25rem 0.5rem;
  border-radius: 6px;
  transition: all 0.2s;
}

.modal-close-btn:hover {
  background: #f3f4f6;
  color: #111;
}

/* Booking Steps */
.booking-steps {
  display: flex;
  align-items: flex-start;
  justify-content: center;
  gap: 0;
}

.step-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
  flex: 1;
}

.step-circle {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: #f3f4f6;
  color: #9ca3af;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.85rem;
  transition: all 0.3s;
  z-index: 1;
  border: 2px solid #e5e7eb;
}

.step-item.active .step-circle {
  background: #10b981;
  color: white;
  border-color: #10b981;
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
}

.step-item.done .step-circle {
  background: #d1fae5;
  color: #059669;
  border-color: #10b981;
}

.step-label {
  font-size: 0.7rem;
  font-weight: 600;
  color: #9ca3af;
  margin-top: 6px;
  text-align: center;
  white-space: nowrap;
}

.step-item.active .step-label, .step-item.done .step-label {
  color: #059669;
}

.step-line {
  position: absolute;
  top: 18px;
  left: 50%;
  width: 100%;
  height: 2px;
  background: #e5e7eb;
  z-index: 0;
}

.step-item.done .step-line {
  background: #10b981;
}

/* Pet select grid */
.pet-select-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
  gap: 10px;
}

.pet-select-card {
  border: 2px solid #e5e7eb;
  border-radius: 14px;
  padding: 1rem 0.75rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  background: #fafafa;
}

.pet-select-card:hover {
  border-color: #10b981;
  background: #f0fdf4;
}

.pet-select-card.selected {
  border-color: #10b981;
  background: #f0fdf4;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
}

.pet-select-emoji { font-size: 2.5rem; margin-bottom: 4px; }
.pet-select-name { font-weight: 700; font-size: 0.88rem; color: #1a1a2e; }
.pet-select-species { font-size: 0.72rem; }
.pet-select-check {
  position: absolute;
  top: 6px;
  right: 6px;
  font-size: 1.1rem;
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
  transition: border-color 0.2s;
  background: #fafafa;
}

.form-control-custom:focus {
  border-color: #10b981;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
  background: white;
}

textarea.form-control-custom { resize: vertical; min-height: 80px; }

/* Booking confirm card */
.booking-confirm-card {
  background: white;
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.02);
  border: 1px solid #f1f3f5;
}

.confirm-layout {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 30px;
}

.card-header-flex {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 15px;
  margin-bottom: 15px;
  border-bottom: 1px solid #f1f3f5;
}

.edit-link {
  font-size: 0.8rem;
  font-weight: 800;
  color: #0d6efd;
  cursor: pointer;
  text-transform: uppercase;
}

.card-body-flex {
  display: flex;
  gap: 20px;
  align-items: center;
}

.confirm-avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  object-fit: cover;
}
.confirm-avatar.placeholder {
  background: #f8f9fa;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
}

.service-summary-box {
  background: #f8f9fa;
  border-radius: 12px;
  padding: 15px;
  display: flex;
  align-items: center;
  gap: 15px;
}
.service-icon-box {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.premium-textarea {
  background: #f8f9fa;
  border: 1px solid transparent;
  border-radius: 12px;
  padding: 15px;
  resize: none;
}
.premium-textarea:focus {
  background: white;
  border-color: #0d6efd;
  box-shadow: 0 0 0 4px rgba(13, 110, 253, 0.1);
}

.receipt-card {
  background: white;
  border-radius: 20px;
  box-shadow: 0 10px 40px rgba(0,0,0,0.05);
  overflow: hidden;
  border: 1px solid #f1f3f5;
}
.receipt-header {
  background: #e3f2fd;
  padding: 15px 20px;
  position: relative;
}
.receipt-header::after {
  content: '';
  position: absolute;
  bottom: -10px;
  left: 0;
  width: 100%;
  height: 20px;
  background: white;
  border-radius: 20px 20px 0 0;
}
.tracking-wide {
  letter-spacing: 1px;
}


.confirm-row {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.confirm-row:last-child { border-bottom: none; }

.confirm-label {
  font-size: 0.8rem;
  color: #9ca3af;
  font-weight: 600;
}

.confirm-value {
  font-size: 0.88rem;
  font-weight: 600;
  color: #374151;
  max-width: 60%;
  text-align: right;
}

/* Navigation buttons */
.booking-nav-btns {
  display: flex;
  align-items: center;
  gap: 10px;
  padding-top: 1rem;
  border-top: 1px solid #f0f0f0;
}

/* Detail modal items */
.detail-item {
  background: #f9fafb;
  border-radius: 10px;
  padding: 10px 14px;
  border: 1px solid #f0f0f0;
}

.detail-label {
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

.item-success { background: #f0fdf4; border-color: #bbf7d0; }
.item-warning { background: #fffbeb; border-color: #fde68a; }

/* Modal transition */
.modal-fade-enter-active, .modal-fade-leave-active { transition: all 0.3s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; transform: scale(0.95); }

/* Vaccine Selection Styles */
.vaccine-card-select {
  border: 1.5px solid #e5e7eb;
  border-radius: 12px;
  padding: 0.9rem 1.1rem;
  cursor: pointer;
  transition: all 0.2s;
  background: #fafafa;
  display: flex;
  align-items: center;
}

.vaccine-card-select:hover {
  border-color: #10b981;
  background: #f0fdf4;
}

.vaccine-card-select.selected {
  border-color: #10b981;
  background: #f0fdf4;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
}

/* Time slots selection styles */
.doctor-slots-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.doctor-slot-group {
  transition: all 0.25s ease;
}

.doctor-name-title {
  font-size: 0.95rem;
  color: #1e293b;
}

.avatar-mini-doctor {
  font-size: 1.25rem;
}

.time-slots-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.btn-time-pill {
  background: white;
  border: 1.5px solid #cbd5e1;
  color: #334155;
  padding: 6px 14px;
  border-radius: 50px;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-time-pill:hover {
  border-color: #10b981;
  color: #059669;
  background: #f0fdf4;
  transform: translateY(-1px);
}

.btn-time-pill.active {
  background: linear-gradient(135deg, #10b981, #059669);
  border-color: #10b981;
  color: white;
  box-shadow: 0 4px 10px rgba(16, 185, 129, 0.25);
  transform: scale(1.03);
}

<style scoped>
/* Injecting Wizard CSS into MyAppointmentsTab.vue */
.wizard-body {
  padding: 1.5rem 2rem;
  max-height: 75vh;
  overflow-y: auto;
  background: #f8fafc;
}

.stepper-container {
  background: transparent;
  padding: 0;
}

.stepper {
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: relative;
  max-width: 100%;
  margin: 0 auto;
}

.step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  z-index: 2;
  cursor: pointer;
  flex: 1;
}

.step-icon {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: #e2e8f0;
  color: #64748b;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 0.9rem;
  transition: all 0.3s ease;
}

.step.active .step-icon, .step.completed .step-icon {
  background: var(--bs-primary);
  color: white;
  box-shadow: 0 0 0 4px rgba(13, 110, 253, 0.15);
}

.step-label {
  font-size: 0.8rem;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  text-align: center;
}

.step.active .step-label {
  color: var(--bs-primary);
}

.animate-fade-in {
  animation: fadeIn 0.4s ease forwards;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateX(10px); }
  to { opacity: 1; transform: translateX(0); }
}

/* Pet Card Select */
.pet-select-card {
  background: white;
  border: 2px solid transparent;
  border-radius: 1rem;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
  position: relative;
}
.pet-select-card:hover { transform: translateY(-2px); box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05); }
.pet-select-card.selected { border-color: var(--bs-primary); background: rgba(13, 110, 253, 0.02); }
.pet-select-card .check-icon { position: absolute; top: 1rem; right: 1rem; font-size: 1.2rem; color: var(--bs-primary); opacity: 0; transform: scale(0.5); transition: all 0.2s ease; }
.pet-select-card.selected .check-icon { opacity: 1; transform: scale(1); }

.pet-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Service Card */
.service-card {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 1rem;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s ease;
  height: 100%;
  position: relative;
}
.service-card:hover { border-color: #cbd5e1; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.05); }
.service-card.selected { border-color: var(--bs-primary); box-shadow: 0 0 0 1px var(--bs-primary); background: #f8fbff; }
.check-circle-empty { position: absolute; top: 1rem; right: 1rem; color: #cbd5e1; font-size: 1.2rem; }
.check-circle-filled { position: absolute; top: 1rem; right: 1rem; font-size: 1.2rem; opacity: 0; transition: opacity 0.2s; }
.service-card.selected .check-circle-empty { opacity: 0; }
.service-card.selected .check-circle-filled { opacity: 1; }
.price-tag { font-weight: 700; color: var(--bs-primary); }

.slot-pill {
  padding: 0.5rem 1rem;
  border-radius: 0.5rem;
  border: 1px solid #e2e8f0;
  background: white;
  color: #334155;
  font-weight: 600;
  font-size: 0.95rem;
  cursor: pointer;
  transition: all 0.2s;
  min-width: 80px;
  text-align: center;
}
.slot-pill:hover { border-color: var(--bs-primary); color: var(--bs-primary); }
.slot-pill.selected { background: var(--bs-primary); color: white; border-color: var(--bs-primary); }

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
.date-picker-lg { padding: 0.75rem; border-radius: 0.5rem; }

.receipt-card { background: #e0f2fe; border-radius: 1rem; padding: 1.5rem; height: 100%; }
.receipt-header { background: #bae6fd; padding: 1rem; border-radius: 0.5rem; margin-bottom: 1rem; text-align: center; }
.receipt-doctor { background: white; padding: 1rem; border-radius: 0.5rem; margin-bottom: 1.5rem; box-shadow: 0 2px 4px rgba(0,0,0,0.02); }
.doctor-avatar { width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 1.2rem; }
.summary-card { background: white; border: 1px solid #e2e8f0; border-radius: 1rem; padding: 1.5rem; }
.summary-title { font-size: 0.9rem; font-weight: 700; color: #334155; margin-bottom: 1rem; }

/* ===== Pet Select Card (Step 0) ===== */
.pet-select-card {
  background: white;
  border-radius: 16px;
  border: 2px solid transparent;
  box-shadow: 0 4px 15px rgba(0,0,0,0.05);
  cursor: pointer;
  padding: 20px;
  display: flex;
  flex-direction: column;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  overflow: hidden;
}
.pet-select-card:hover {
  border-color: #cbd5e1;
  box-shadow: 0 8px 20px rgba(0,0,0,0.08);
}
.pet-select-card.pet-selected {
  border-color: #0d6efd;
  box-shadow: 0 8px 25px rgba(13,110,253,0.15);
}
.pet-select-card.pet-has-warning {
  border-color: #fbbf24;
}
.pet-select-card.pet-selected.pet-has-warning {
  border-color: #f59e0b;
  box-shadow: 0 8px 25px rgba(245,158,11,0.2);
}

/* Warning ribbon in top-right corner */
.pet-warning-ribbon {
  position: absolute;
  top: 0;
  right: 0;
  background: linear-gradient(135deg, #f59e0b, #fbbf24);
  color: white;
  font-size: 0.68rem;
  font-weight: 700;
  padding: 4px 10px 4px 14px;
  border-radius: 0 16px 0 12px;
  white-space: nowrap;
  letter-spacing: 0.3px;
}

/* Duplicate appointment warning banner */
.pet-dup-warning {
  background: linear-gradient(135deg, #fffbeb, #fef3c7);
  border: 1.5px solid #fcd34d;
  border-radius: 14px;
  padding: 1.1rem 1.3rem;
  animation: warning-pulse 2s ease-in-out infinite;
}

@keyframes warning-pulse {
  0%, 100% { box-shadow: 0 0 0 0 rgba(251,191,36,0.25); }
  50%       { box-shadow: 0 0 0 6px rgba(251,191,36,0); }
}

/* Fade-slide transition for the warning banner */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.3s ease;
}
.fade-slide-enter-from {
  opacity: 0;
  transform: translateY(-10px);
}
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

</style>
