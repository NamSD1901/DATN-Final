<template>
  <div class="pet-profile-page">
    <!-- Back Button -->
    <div class="profile-topbar">
      <button class="back-btn" @click="goBack">
        <i class="bi bi-arrow-left me-2"></i>Quay lại
      </button>
      <div class="breadcrumb-trail">
        <span>Hồ sơ</span>
        <i class="bi bi-chevron-right mx-2"></i>
        <span>{{ pet?.species || 'Thú cưng' }}</span>
        <i class="bi bi-chevron-right mx-2"></i>
        <span class="active">{{ pet?.name || '...' }}</span>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading-screen">
      <div class="spinner-border text-primary" style="width: 3rem; height: 3rem;"></div>
      <p class="mt-3 text-muted">Đang tải hồ sơ thú cưng...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="error-state">
      <div style="font-size: 4rem;">😿</div>
      <h5>Không thể tải hồ sơ</h5>
      <p class="text-muted">{{ error }}</p>
      <button class="btn btn-primary rounded-pill px-4" @click="fetchAll">Thử lại</button>
    </div>

    <template v-else-if="pet">
      <!-- MAIN CONTAINER: Dashboard Layout -->
      <div class="dashboard-container">
        
        <!-- TOP SECTION: Avatar & Cards Row -->
        <div class="top-cards-row">
          
          <!-- Avatar Card -->
          <div class="avatar-card">
            <div class="avatar-wrapper">
              <img v-if="pet.avatar" :src="getAvatarUrl(pet.avatar)" alt="avatar" class="avatar-img" />
              <div v-else class="avatar-emoji">{{ getSpeciesEmoji(pet.species) }}</div>
              <div v-if="pet.sterilized" class="badge-sterilized" title="Đã triệt sản">
                <i class="bi bi-shield-check-fill"></i>
              </div>
            </div>
            <div class="avatar-info">
              <h1 class="pet-name-lg">{{ pet.name }}</h1>
              <p class="pet-breed-lg">{{ pet.breed || pet.species }} · {{ calculateAge(pet.birthDate) }}</p>
            </div>
          </div>

          <!-- Info Cards Group -->
          <div class="info-cards-group">
            
            <!-- Chủ sở hữu -->
            <div class="info-card">
              <div class="card-label">Chủ sở hữu</div>
              <div class="card-value">{{ ownerProfile?.fullName || 'Người dùng' }}</div>
              <div class="card-sub">{{ ownerProfile?.phoneNumber || '—' }}</div>
            </div>

            <!-- Lần khám gần nhất -->
            <div class="info-card">
              <div class="card-label"><i class="bi bi-calendar2-week me-1"></i>Lần khám gần nhất</div>
              <div class="card-value">{{ latestMedicalRecord ? formatDate(latestMedicalRecord.examinationDate || latestMedicalRecord.createdAt) : '—' }}</div>
              <div class="card-sub">{{ latestMedicalRecord?.serviceName || latestMedicalRecord?.diagnosis || 'Chưa có' }}</div>
            </div>

            <!-- Dị ứng -->
            <div class="info-card allergy-card-top">
              <div class="card-label text-danger"><i class="bi bi-exclamation-triangle-fill me-1"></i>Dị ứng</div>
              <div class="card-value text-danger">{{ pet.allergyNote ? 'Có lưu ý' : 'Không có' }}</div>
              <div class="card-sub text-danger">{{ pet.allergyNote || 'Bình thường' }}</div>
            </div>

          </div>

          <!-- Lịch hẹn tiếp theo (Right Side) -->
          <div class="next-appt-card">
            <div class="nac-header">
              <i class="bi bi-calendar-event me-2"></i>Lịch hẹn tiếp theo
            </div>
            <div class="nac-body" v-if="nextAppointment">
              <div class="nac-date">{{ formatDateTimeFull(nextAppointment.appointmentDate) }}</div>
              <div class="nac-time">{{ formatTimeOnly(nextAppointment.appointmentDate) }}</div>
              <div class="nac-service"><i class="bi bi-stethoscope me-2"></i>{{ nextAppointment.serviceName }}</div>
              <div class="nac-footer">
                <div class="nac-doctor" v-if="nextAppointment.doctorName">
                  <i class="bi bi-person-circle me-1"></i>{{ nextAppointment.doctorName }}
                </div>
                <button class="btn-link" @click="activeTab = 'appointments'">Chi tiết</button>
              </div>
            </div>
            <div class="nac-body empty" v-else>
              <p class="text-muted mb-0">Chưa có lịch hẹn nào sắp tới.</p>
              <button class="btn btn-outline-primary btn-sm mt-3 w-100" @click="$router.push('/dashboard')">Đặt lịch ngay</button>
            </div>
          </div>

        </div>

        <!-- TABS NAV (Underline Style) -->
        <div class="clean-tabs-wrap">
          <button
            v-for="tab in tabs"
            :key="tab.key"
            class="clean-tab-btn"
            :class="{ active: activeTab === tab.key }"
            @click="activeTab = tab.key"
          >
            <i :class="tab.icon"></i>
            {{ tab.label }}
            <span v-if="tab.key === 'vaccines' && isVaccineDueSoon()" class="red-dot"></span>
          </button>
        </div>

        <!-- TABS CONTENT -->
        <div class="tab-content-container">

          <!-- ===== TAB: TỔNG QUAN ===== -->
          <div v-show="activeTab === 'overview'" class="tab-pane">
            
            <!-- Mini stats row -->
            <div class="stats-row">
              <div class="stat-card">
                <div class="sc-icon text-primary bg-primary-light"><i class="bi bi-speedometer2"></i></div>
                <div class="sc-info">
                  <div class="sc-label">Cân nặng</div>
                  <div class="sc-value">
                    {{ pet.weight ? pet.weight + ' kg' : '—' }}
                    <span v-if="weightDiff" class="weight-diff" :class="weightDiff > 0 ? 'text-success' : 'text-danger'">
                      <i :class="weightDiff > 0 ? 'bi bi-arrow-up' : 'bi bi-arrow-down'"></i>{{ Math.abs(weightDiff) }}kg
                    </span>
                  </div>
                  <div class="sc-sub">Đo lần cuối: {{ latestMedicalRecord ? formatDateShort(latestMedicalRecord.createdAt) : '—' }}</div>
                </div>
              </div>

              <div class="stat-card">
                <div class="sc-icon text-success bg-success-light"><i class="bi bi-file-medical"></i></div>
                <div class="sc-info">
                  <div class="sc-label">Lần khám gần nhất</div>
                  <div class="sc-value">{{ latestMedicalRecord ? formatDate(latestMedicalRecord.createdAt) : '—' }}</div>
                  <div class="sc-sub">{{ latestMedicalRecord?.serviceName || 'Khám tổng quát' }}</div>
                </div>
              </div>

              <div class="stat-card">
                <div class="sc-icon text-danger bg-danger-light"><i class="bi bi-exclamation-triangle"></i></div>
                <div class="sc-info">
                  <div class="sc-label">Dị ứng & Lưu ý</div>
                  <div class="sc-value text-danger">{{ pet.allergyNote ? 'Có lưu ý đặc biệt' : 'Không ghi nhận' }}</div>
                  <div class="sc-sub text-danger text-truncate" :title="pet.allergyNote">{{ pet.allergyNote || 'Sức khỏe bình thường' }}</div>
                </div>
              </div>
            </div>

            <!-- Main Layout 70-30 -->
            <div class="overview-main-layout">
              <!-- Left Col: Chart -->
              <div class="overview-left">
                <div class="dashboard-panel">
                  <div class="panel-header">
                    <h6 class="panel-title">Biểu đồ cân nặng</h6>
                    <select class="form-select form-select-sm" style="width: auto;">
                      <option>Tất cả thời gian</option>
                      <option>6 tháng qua</option>
                    </select>
                  </div>
                  <div class="panel-body">
                    <div class="chart-container" style="height: 300px; position: relative;">
                      <canvas ref="chartCanvas" id="weightChart"></canvas>
                      <div v-if="!hasWeightData" class="chart-empty-state">
                        <p class="text-muted">Chưa có đủ dữ liệu cân nặng để vẽ biểu đồ.</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Right Col: Details -->
              <div class="overview-right">
                
                <div class="dashboard-panel">
                  <div class="panel-header border-bottom-0 pb-0">
                    <h6 class="panel-title"><i class="bi bi-file-text text-primary me-2"></i>Lần khám gần nhất</h6>
                  </div>
                  <div class="panel-body pt-2" v-if="latestMedicalRecord">
                    <div class="lkg-date">{{ formatDateFull(latestMedicalRecord.examinationDate || latestMedicalRecord.createdAt) }} - {{ latestMedicalRecord.serviceName || 'Khám bệnh' }}</div>
                    <div class="lkg-doctor text-muted mb-3" v-if="latestMedicalRecord.doctorName">
                      Bác sĩ phụ trách: <span class="text-dark fw-medium">Bs. {{ latestMedicalRecord.doctorName }}</span>
                    </div>
                    
                    <div class="lkg-notes">
                      Ghi chú lâm sàng:
                      <div class="notes-content mt-1">
                        <em>"{{ latestMedicalRecord.notes || latestMedicalRecord.diagnosis || 'Không có ghi chú đặc biệt.' }}"</em>
                      </div>
                    </div>

                    <button class="btn btn-outline-secondary w-100 mt-3" @click="activeTab = 'history'">Xem toàn bộ bệnh án</button>
                  </div>
                  <div class="panel-body" v-else>
                    <p class="text-muted text-center py-4">Chưa có bệnh án nào được ghi nhận.</p>
                  </div>
                </div>

                <div class="dashboard-panel mt-4" v-if="pet.allergyNote">
                  <div class="panel-header border-bottom-0 pb-0">
                    <h6 class="panel-title"><i class="bi bi-exclamation-triangle-fill text-warning me-2"></i>Lưu ý & Dị ứng</h6>
                  </div>
                  <div class="panel-body pt-2">
                    <div class="allergy-tag-large">
                      <i class="bi bi-x-circle-fill me-2"></i>{{ pet.allergyNote }}
                    </div>
                  </div>
                </div>

              </div>
            </div>

          </div>

          <!-- ===== OTHER TABS (Placeholder/Basic UI to keep logic) ===== -->
          
          <div v-show="activeTab === 'history'" class="tab-pane">
            <div class="row g-4">
              <div class="col-lg-8">
                <div class="d-flex justify-content-between align-items-center mb-4">
                  <h4 class="fw-bold text-dark mb-0">Medical History Timeline</h4>
                  <button class="btn btn-light btn-sm text-muted fw-medium border rounded-3 px-3">
                    <i class="bi bi-funnel me-1"></i> Filter by: All <i class="bi bi-chevron-down ms-1"></i>
                  </button>
                </div>
                
                <div v-if="medicalRecords.length === 0" class="text-center py-5 text-muted bg-white rounded-4 shadow-sm border">
                  Chưa có lịch sử khám bệnh.
                </div>
                <div v-else class="timeline-container position-relative ps-4 ms-2 mt-4">
                  <!-- Vertical Line -->
                  <div class="position-absolute h-100" style="left: 0; top: 0; width: 2px; background-color: #e2e8f0;"></div>

                  <div v-for="(rec, index) in medicalRecords" :key="rec.id" class="position-relative mb-4">
                    <!-- Dot -->
                    <div class="position-absolute rounded-circle" :style="`width: 12px; height: 12px; left: -29px; top: 24px; background-color: ${rec.followUpDate ? '#f97316' : '#1e3a8a'}; border: 2px solid white; box-shadow: 0 0 0 1px ${rec.followUpDate ? '#f97316' : '#1e3a8a'};`"></div>
                    
                    <!-- Card -->
                    <div class="card border-0 shadow-sm rounded-4 overflow-hidden" :style="`border-left: 4px solid ${rec.followUpDate ? '#f97316' : '#1e3a8a'} !important;`">
                      <div class="card-body p-4">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                           <div class="d-flex align-items-center flex-wrap gap-3">
                             <h5 class="fw-bold mb-0 text-dark">{{ rec.serviceName || rec.diagnosis || 'Kiểm tra định kỳ' }}</h5>
                             <span class="badge rounded-pill px-3 py-1" :class="rec.followUpDate ? 'bg-warning-subtle text-warning' : 'bg-success-subtle text-success'" style="font-size: 0.65rem; font-weight: 800; letter-spacing: 0.5px;">
                               {{ rec.followUpDate ? 'FOLLOW-UP REQUIRED' : 'COMPLETED' }}
                             </span>
                           </div>
                           <button class="btn btn-link text-muted p-0"><i class="bi bi-three-dots-vertical"></i></button>
                        </div>
                        
                        <div class="d-flex gap-4 small text-muted mb-4 fw-medium">
                          <div class="d-flex align-items-center"><i class="bi bi-calendar3 me-1"></i> {{ formatDate(rec.createdAt) }}</div>
                          <div class="d-flex align-items-center" v-if="rec.doctorName"><i class="bi bi-person-badge me-1"></i> {{ rec.doctorName }}</div>
                        </div>

                        <p class="text-secondary mb-4" style="line-height: 1.6; font-size: 0.95rem;">
                          {{ rec.note || rec.treatmentPlan || rec.diagnosis || 'Không có ghi chú thêm.' }}
                        </p>

                        <div class="d-flex gap-2">
                          <button v-if="rec.followUpDate" class="btn btn-primary btn-sm rounded-3 px-4 py-2 fw-bold shadow-sm d-flex align-items-center">
                            <i class="bi bi-calendar-plus me-2"></i> Book Follow-up
                          </button>
                          <button class="btn btn-light btn-sm rounded-3 px-4 py-2 fw-bold text-dark border d-flex align-items-center" style="background: #f8fafc;">
                            <i class="bi bi-eye me-2 text-muted"></i> View Details
                          </button>
                          <button v-if="!rec.followUpDate" class="btn btn-light btn-sm rounded-3 px-4 py-2 fw-bold text-dark border d-flex align-items-center" style="background: #f8fafc;">
                            <i class="bi bi-download me-2 text-muted"></i> Report
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- Right Column -->
              <div class="col-lg-4">
                <!-- Quick Summary -->
                <div class="card border-0 shadow-sm rounded-4 mb-4 mt-2">
                  <div class="card-body p-4">
                    <h5 class="fw-bold text-dark mb-4">Quick Summary</h5>
                    
                    <div class="mb-4">
                      <div class="text-muted small fw-bold mb-2" style="font-size: 0.7rem; letter-spacing: 1px;">LAST VISIT</div>
                      <div class="d-flex align-items-center gap-3 p-3 rounded-4" style="border: 1px solid #f1f5f9; background: #f8fafc;">
                        <div class="rounded-circle d-flex align-items-center justify-content-center" style="width: 48px; height: 48px; background: #e0f2fe; color: #0284c7;">
                          <i class="bi bi-calendar-check fs-5"></i>
                        </div>
                        <div>
                          <div class="fw-bold text-dark mb-1" style="font-size: 0.95rem;">{{ medicalRecords.length > 0 ? formatDate(medicalRecords[0].createdAt) : 'Chưa có' }}</div>
                          <div class="small text-muted fw-medium">{{ medicalRecords.length > 0 ? (medicalRecords[0].serviceName || 'Kiểm tra định kỳ') : '-' }}</div>
                        </div>
                      </div>
                    </div>

                    <div>
                      <div class="text-muted small fw-bold mb-2" style="font-size: 0.7rem; letter-spacing: 1px;">NEXT DUE</div>
                      <div class="d-flex align-items-center gap-3 p-3 rounded-4" style="border: 1px solid #f1f5f9; background: #f8fafc;">
                        <div class="rounded-circle d-flex align-items-center justify-content-center" style="width: 48px; height: 48px; background: #ffedd5; color: #ea580c;">
                          <i class="bi bi-capsule fs-5"></i>
                        </div>
                        <div>
                          <div class="fw-bold text-dark mb-1" style="font-size: 0.95rem;">{{ nextAppointment ? formatDate(nextAppointment.appointmentDate) : 'Chưa có lịch' }}</div>
                          <div class="small text-muted fw-medium">{{ nextAppointment ? nextAppointment.serviceName : 'Vui lòng đặt lịch khám' }}</div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Medical Alerts -->
                <div class="card border-0 rounded-4" style="box-shadow: 0 4px 20px rgba(220, 38, 38, 0.08); border: 2px solid #fecaca !important;">
                  <div class="card-body p-4">
                    <h5 class="fw-bold text-danger mb-4 d-flex align-items-center gap-2">
                      <i class="bi bi-exclamation-triangle-fill"></i> Medical Alerts
                    </h5>
                    
                    <div class="mb-4 p-3 rounded-4" style="background-color: #fef2f2;">
                      <div class="text-danger small fw-bold mb-2" style="font-size: 0.7rem; letter-spacing: 1px;">KNOWN ALLERGIES</div>
                      <div class="fw-bold text-dark d-flex align-items-center gap-2 mb-2" style="font-size: 0.95rem;">
                        <div class="rounded-circle bg-danger" style="width: 6px; height: 6px;"></div> {{ pet?.allergyNote ? 'Có lưu ý dị ứng' : 'Không phát hiện dị ứng' }}
                      </div>
                      <div class="small text-muted fw-medium" style="line-height: 1.5;">{{ pet?.allergyNote || 'Chưa ghi nhận phản ứng phụ với thành phần nào.' }}</div>
                    </div>

                    <div class="p-3 rounded-4" style="border: 1px solid #f1f5f9; background: #f8fafc;">
                      <div class="text-muted small fw-bold mb-2" style="font-size: 0.7rem; letter-spacing: 1px;">DIETARY NOTES</div>
                      <div class="small text-dark fw-medium" style="line-height: 1.6;">
                        {{ pet?.currentDiet || 'Không có yêu cầu đặc biệt về khẩu phần ăn. Dùng thức ăn tiêu chuẩn.' }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-show="activeTab === 'appointments'" class="tab-pane">
            <div class="row g-4">
              <!-- Cột Lịch sắp tới -->
              <div class="col-lg-6">
                <h5 class="fw-bold text-dark mb-4 d-flex align-items-center gap-2">
                  <i class="bi bi-calendar-event text-primary"></i> Sắp tới
                </h5>
                <div v-if="upcomingAppointments.length === 0" class="text-center py-5 text-muted bg-white rounded-4 shadow-sm border">
                  Không có lịch hẹn sắp tới.
                </div>
                <div v-else class="d-flex flex-column gap-3">
                  <div v-for="appt in upcomingAppointments" :key="appt.id" class="card border-0 shadow-sm rounded-4 overflow-hidden" style="transition: transform 0.2s; cursor: pointer;" onmouseover="this.style.transform='translateY(-2px)'" onmouseout="this.style.transform='none'">
                    <div class="card-body p-4 d-flex gap-4 align-items-center">
                      <!-- Mini Calendar -->
                      <div class="text-center rounded-3 bg-light border border-light-subtle shadow-sm overflow-hidden" style="width: 70px; flex-shrink: 0;">
                        <div class="bg-primary text-white small fw-bold py-1" style="font-size: 0.75rem; letter-spacing: 1px;">
                          T{{ new Date(appt.appointmentDate).getMonth() + 1 }}
                        </div>
                        <div class="fw-bold text-dark py-2 fs-4" style="line-height: 1;">
                          {{ new Date(appt.appointmentDate).getDate() }}
                        </div>
                      </div>
                      
                      <!-- Details -->
                      <div class="flex-grow-1">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                          <h6 class="fw-bold text-dark mb-0 fs-6">{{ appt.serviceName }}</h6>
                          <span class="badge rounded-pill px-3 py-1" :class="getStatusClass(appt.status)" style="font-weight: 700; font-size: 0.7rem;">
                            {{ getStatusLabel(appt.status) }}
                          </span>
                        </div>
                        <div class="text-muted small fw-medium mb-1 d-flex align-items-center gap-2">
                          <i class="bi bi-clock"></i> {{ formatTimeOnly(appt.appointmentDate) }}
                        </div>
                        <div class="text-muted small fw-medium d-flex align-items-center gap-2">
                          <i class="bi bi-person-badge"></i> {{ appt.doctorName || 'Hệ thống tự động' }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Cột Lịch đã qua -->
              <div class="col-lg-6">
                <h5 class="fw-bold text-dark mb-4 d-flex align-items-center gap-2">
                  <i class="bi bi-clock-history text-secondary"></i> Đã qua
                </h5>
                <div v-if="pastAppointments.length === 0" class="text-center py-5 text-muted bg-white rounded-4 shadow-sm border">
                  Chưa có lịch sử.
                </div>
                <div v-else class="d-flex flex-column gap-3">
                  <div v-for="appt in pastAppointments" :key="appt.id" class="card border-0 shadow-sm rounded-4 overflow-hidden" style="opacity: 0.85;">
                    <div class="card-body p-4 d-flex gap-4 align-items-center">
                      <div class="text-center rounded-3 bg-light border border-light-subtle shadow-sm overflow-hidden" style="width: 70px; flex-shrink: 0;">
                        <div class="bg-secondary text-white small fw-bold py-1" style="font-size: 0.75rem; letter-spacing: 1px;">
                          T{{ new Date(appt.appointmentDate).getMonth() + 1 }}
                        </div>
                        <div class="fw-bold text-secondary py-2 fs-4" style="line-height: 1;">
                          {{ new Date(appt.appointmentDate).getDate() }}
                        </div>
                      </div>
                      
                      <div class="flex-grow-1">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                          <h6 class="fw-bold text-secondary mb-0 fs-6">{{ appt.serviceName }}</h6>
                          <span class="badge rounded-pill px-3 py-1" :class="getStatusClass(appt.status)" style="font-weight: 700; font-size: 0.7rem;">
                            {{ getStatusLabel(appt.status) }}
                          </span>
                        </div>
                        <div class="text-muted small fw-medium mb-1 d-flex align-items-center gap-2">
                          <i class="bi bi-clock"></i> {{ formatTimeOnly(appt.appointmentDate) }}
                        </div>
                        <div class="text-muted small fw-medium d-flex align-items-center gap-2">
                          <i class="bi bi-person-badge"></i> {{ appt.doctorName || 'Hệ thống tự động' }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-show="activeTab === 'vaccines'" class="tab-pane">
            <div class="row g-4">
              <!-- Cột Trái: Nhắc lịch sắp tới -->
              <div class="col-lg-5">
                <div class="d-flex align-items-center mb-3">
                  <i class="bi bi-bell-fill text-warning fs-5 me-2"></i>
                  <h6 class="fw-bold mb-0 text-dark">Nhắc lịch sắp tới</h6>
                </div>
                
                <div v-if="upcomingVaccines.length === 0" class="p-4 text-center rounded-4 border bg-white shadow-sm">
                  <i class="bi bi-check-circle text-success fs-1 mb-2 d-block opacity-50"></i>
                  <p class="text-muted mb-0">Không có lịch tiêm chủng nào sắp đến hạn.</p>
                </div>
                
                <div v-else class="d-flex flex-column gap-3">
                  <div v-for="vac in upcomingVaccines" :key="'up-' + vac.id" class="vaccine-card upcoming-card p-4 rounded-4 bg-white border">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                      <span class="badge bg-warning-subtle text-warning border border-warning-subtle rounded-pill px-3 py-2 fw-bold">Nhắc lại</span>
                      <span class="small fw-bold" :class="isVaccineOverdue(vac.nextDueDate) ? 'text-danger' : 'text-muted'">
                        {{ isVaccineOverdue(vac.nextDueDate) ? 'Đã quá hạn' : 'Sắp đến hạn' }}
                      </span>
                    </div>
                    <h5 class="fw-bold text-dark mb-2">{{ vac.vaccineName }}</h5>
                    <div class="d-flex align-items-center text-dark mb-4">
                      <i class="bi bi-calendar3 me-2 text-muted"></i>
                      <span class="fw-medium">Đến hạn: {{ formatDate(vac.nextDueDate) }}</span>
                    </div>
                    <button class="btn btn-outline-warning w-100 rounded-pill fw-bold" style="border-width: 2px;">Đặt lịch tiêm</button>
                  </div>
                </div>
              </div>

              <!-- Cột Phải: Lịch sử tiêm chủng -->
              <div class="col-lg-7">
                <div class="d-flex justify-content-between align-items-center mb-3">
                  <div class="d-flex align-items-center">
                    <i class="bi bi-clock-history text-success fs-5 me-2"></i>
                    <h6 class="fw-bold mb-0 text-dark">Lịch sử tiêm chủng</h6>
                  </div>
                  <button class="btn btn-sm btn-light text-primary fw-bold rounded-pill px-3" style="background: transparent;">
                    <i class="bi bi-filter me-1"></i>Lọc
                  </button>
                </div>

                <div v-if="historyVaccines.length === 0" class="p-5 text-center rounded-4 border bg-white shadow-sm">
                  <p class="text-muted mb-0">Chưa có dữ liệu tiêm phòng.</p>
                </div>
                
                <div v-else class="d-flex flex-column gap-3">
                  <div v-for="vac in historyVaccines" :key="'hist-' + vac.id" class="vaccine-card history-card p-4 rounded-4 bg-white border d-flex gap-4">
                    <div class="vac-icon-wrap">
                      <div class="vac-icon bg-success-subtle text-success d-flex align-items-center justify-content-center rounded-circle" style="width: 48px; height: 48px;">
                        <i class="bi bi-syringe fs-4"></i>
                      </div>
                    </div>
                    <div class="flex-grow-1">
                      <div class="d-flex justify-content-between align-items-start mb-2">
                        <div>
                          <h5 class="fw-bold text-dark mb-1">{{ vac.vaccineName }}</h5>
                          <div class="small text-muted mb-3">{{ vac.note || 'Mũi tiêm định kỳ' }}</div>
                        </div>
                        <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-3 py-2 fw-bold">
                          <i class="bi bi-check-circle-fill me-1"></i>Đã tiêm
                        </span>
                      </div>
                      
                      <div class="p-3 bg-light rounded-3 d-flex gap-4">
                        <div class="flex-1">
                          <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Ngày tiêm</div>
                          <div class="fw-medium text-dark">{{ formatDate(vac.administeredAt) }}</div>
                        </div>
                        <div class="flex-1">
                          <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Bác sĩ phụ trách</div>
                          <div class="fw-medium text-dark">{{ vac.doctorName || 'Bs. Thú y' }}</div>
                        </div>
                      </div>
                      <div class="mt-3" v-if="vac.notes">
                         <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Ghi chú</div>
                         <p class="small text-dark mb-0">{{ vac.notes }}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-show="activeTab === 'prescriptions'" class="tab-pane">
            <div class="row g-4">
              <!-- Cột Trái: Lọc đơn thuốc -->
              <div class="col-lg-3">
                <div class="dashboard-panel p-4 border-0 shadow-sm rounded-4">
                  <h6 class="fw-bold mb-4 text-dark fs-5">Lọc đơn thuốc</h6>
                  
                  <div class="mb-4">
                    <label class="form-label small fw-bold text-muted mb-2">Trạng thái</label>
                    <select v-model="filterPrescriptionStatus" class="form-select rounded-3 border-light shadow-none" style="background-color: #f8fafc; font-weight: 500;">
                      <option value="all">Tất cả</option>
                      <option value="active">Đang dùng</option>
                      <option value="completed">Đã hoàn thành</option>
                    </select>
                  </div>
                  
                  <div>
                    <label class="form-label small fw-bold text-muted mb-2">Thời gian</label>
                    <select v-model="filterPrescriptionTime" class="form-select rounded-3 border-light shadow-none" style="background-color: #f8fafc; font-weight: 500;">
                      <option value="6m">6 tháng gần đây</option>
                      <option value="1y">1 năm gần đây</option>
                      <option value="all">Tất cả</option>
                    </select>
                  </div>
                </div>
              </div>

              <!-- Cột Phải: Danh sách Đơn thuốc -->
              <div class="col-lg-9">
                <div class="d-flex flex-column gap-4">
                  <div v-for="rx in filteredPrescriptions" :key="rx.id" class="prescription-card bg-white rounded-4 shadow-sm border overflow-hidden" :class="rx.status === 'active' ? 'border-left-success' : 'border-left-secondary'">
                    
                    <!-- Card Header -->
                    <div class="p-4 border-bottom">
                      <div class="d-flex justify-content-between align-items-start mb-2">
                        <div class="d-flex align-items-center flex-wrap gap-2">
                          <h5 class="fw-bold text-dark mb-0">{{ rx.diagnosis }}</h5>
                          <span class="badge rounded-pill px-3 py-1 fw-bold" :class="rx.status === 'active' ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary'">
                            {{ rx.status === 'active' ? 'Đang dùng' : 'Đã hoàn thành' }}
                          </span>
                        </div>

                      </div>
                      
                      <div class="d-flex gap-4 small text-muted">
                        <div class="d-flex align-items-center"><i class="bi bi-calendar3 me-1"></i> Kê đơn: {{ formatDate(rx.date) }}</div>
                        <div class="d-flex align-items-center"><i class="bi bi-person-badge me-1"></i> {{ rx.doctor }}</div>
                        <div class="d-flex align-items-center fw-medium text-dark"><i class="bi bi-hash text-muted"></i>{{ rx.id }}</div>
                      </div>
                    </div>

                    <!-- Card Body: Medicines Table -->
                    <div class="p-4 pb-2">
                      <div class="table-responsive">
                        <table class="table table-borderless align-middle mb-0 rx-table">
                          <thead class="border-bottom">
                            <tr>
                              <th class="text-muted fw-semibold pb-3" style="font-size: 0.85rem;">Tên thuốc</th>
                              <th class="text-muted fw-semibold pb-3" style="font-size: 0.85rem;">Liều lượng</th>
                              <th class="text-muted fw-semibold pb-3" style="font-size: 0.85rem;">Cách dùng</th>
                              <th class="text-muted fw-semibold pb-3 text-end" style="font-size: 0.85rem; width: 90px;">Số lượng</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr v-for="(med, idx) in rx.medicines" :key="idx" class="border-bottom">
                              <td class="py-3">
                                <div class="fw-bold text-primary">{{ med.name }}</div>
                                <div class="small text-muted" style="font-size: 0.75rem;">{{ med.activeIngredient }}</div>
                              </td>
                              <td class="py-3 fw-medium text-dark" style="font-size: 0.9rem;">{{ med.dosage }}</td>
                              <td class="py-3 text-dark" style="font-size: 0.9rem;">{{ med.usage }}</td>
                              <td class="py-3 text-end fw-bold text-dark">{{ med.quantity }} {{ med.unit }}</td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                    </div>

                    <!-- Doctor Notes & Footer -->
                    <div class="p-4 pt-2 d-flex justify-content-between align-items-end">
                      <div class="flex-grow-1 pe-4">
                        <div v-if="rx.notes" class="p-3 rounded-3 mt-2" style="background-color: #fffbeb;">
                          <div class="small fw-bold mb-1" style="color: #d97706;">Ghi chú của bác sĩ:</div>
                          <div class="small fw-medium" style="color: #f59e0b;">{{ rx.notes }}</div>
                        </div>
                      </div>
                      

                    </div>

                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-show="activeTab === 'attachments'" class="tab-pane">
            <div class="dashboard-panel">
              <div class="panel-body text-center py-5 text-muted">
                <i class="bi bi-paperclip" style="font-size: 3rem;"></i>
                <p class="mt-3 mb-0">Không có tệp đính kèm nào (X-quang, Xét nghiệm...).</p>
              </div>
            </div>
          </div>

        </div> <!-- End Tab Content -->
      </div> <!-- End Dashboard Container -->
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick, shallowRef, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import Chart from 'chart.js/auto';
import api from '../services/api';

const route = useRoute();
const router = useRouter();
const petId = computed(() => route.params.id as string);

const backendUrl = import.meta.env.VITE_API_URL || 'http://localhost:5150';

// ===== State =====
const pet = ref<any>(null);
const loading = ref(true);
const error = ref('');

const ownerProfile = ref<any>(null);
const medicalRecords = ref<any[]>([]);
const vaccinations = ref<any[]>([]);
const appointments = ref<any[]>([]);

const prescriptions = ref<any[]>([]);

const filterPrescriptionStatus = ref('all');
const filterPrescriptionTime = ref('6m');

const filteredPrescriptions = computed(() => {
  let result = prescriptions.value;
  
  if (filterPrescriptionStatus.value !== 'all') {
    result = result.filter(rx => rx.status === filterPrescriptionStatus.value);
  }
  
  if (filterPrescriptionTime.value !== 'all') {
    const now = new Date();
    result = result.filter(rx => {
      const rxDate = new Date(rx.date);
      if (filterPrescriptionTime.value === '6m') {
        return (now.getTime() - rxDate.getTime()) <= 6 * 30 * 24 * 60 * 60 * 1000;
      } else if (filterPrescriptionTime.value === '1y') {
        return (now.getTime() - rxDate.getTime()) <= 365 * 24 * 60 * 60 * 1000;
      }
      return true;
    });
  }
  
  return result;
});

const activeTab = ref('overview');
const chartCanvas = ref<HTMLCanvasElement | null>(null);
const chartInstance = shallowRef<Chart | null>(null);

// ===== Tabs config =====
const tabs = computed(() => [
  { key: 'overview', label: 'Tổng quan', icon: 'bi bi-grid-1x2-fill' },
  { key: 'history', label: 'Lịch sử khám', icon: 'bi bi-file-medical-fill' },
  { key: 'appointments', label: 'Lịch hẹn', icon: 'bi bi-calendar-check-fill' },
  { key: 'vaccines', label: 'Vaccine', icon: 'bi bi-syringe' },
  { key: 'prescriptions', label: 'Đơn thuốc', icon: 'bi bi-capsule' },
  { key: 'attachments', label: 'Tệp đính kèm', icon: 'bi bi-paperclip' }
]);

// ===== Computed =====
const petAppointments = computed(() => {
  return appointments.value.filter((a: any) => a.petId === pet.value?.id || a.petName === pet.value?.name);
});

const upcomingAppointments = computed(() => {
  return petAppointments.value.filter(a => ['waiting', 'pending', 'confirmed', 'in_progress', 'checked_in'].includes(a.status?.toLowerCase()))
    .sort((a, b) => new Date(a.appointmentDate).getTime() - new Date(b.appointmentDate).getTime());
});

const pastAppointments = computed(() => {
  return petAppointments.value.filter(a => ['completed', 'cancelled'].includes(a.status?.toLowerCase()))
    .sort((a, b) => new Date(b.appointmentDate).getTime() - new Date(a.appointmentDate).getTime());
});

const upcomingVaccines = computed(() => {
  return vaccinations.value.filter((v: any) => v.nextDueDate && (isVaccineOverdue(v.nextDueDate) || isSoonDue(v.nextDueDate)));
});

const historyVaccines = computed(() => {
  return [...vaccinations.value].sort((a: any, b: any) => new Date(b.administeredAt).getTime() - new Date(a.administeredAt).getTime());
});

const nextAppointment = computed(() => {
  const now = new Date();
  return petAppointments.value
    .filter(a => new Date(a.appointmentDate) > now && a.status !== 'cancelled')
    .sort((a, b) => new Date(a.appointmentDate).getTime() - new Date(b.appointmentDate).getTime())[0];
});

const latestMedicalRecord = computed(() => medicalRecords.value[0] || null);

const hasWeightData = computed(() => {
  return medicalRecords.value.filter(r => r.weight != null).length >= 1;
});

const weightDiff = computed(() => {
  const records = medicalRecords.value.filter(r => r.weight != null);
  if (records.length < 2) return 0;
  const current = records[0].weight;
  const previous = records[1].weight;
  return parseFloat((current - previous).toFixed(1));
});

// ===== API Calls =====
const fetchAll = async () => {
  loading.value = true;
  error.value = '';
  try {
    const res = await api.get(`/mypets/${petId.value}`);
    pet.value = res.data;
    await Promise.all([
      fetchMedicalRecords(), 
      fetchVaccinations(), 
      fetchAppointments(),
      fetchPrescriptions(),
      fetchOwnerProfile()
    ]);
  } catch (e: any) {
    error.value = e?.response?.data?.message || 'Không thể tải thông tin thú cưng.';
  } finally {
    loading.value = false;
    
    // Render chart initially if on overview tab
    if (activeTab.value === 'overview') {
      nextTick(() => renderChart());
    }
  }
};

const fetchOwnerProfile = async () => {
  try {
    const res = await api.get('/profile');
    ownerProfile.value = res.data;
  } catch {
    ownerProfile.value = null;
  }
};

const fetchMedicalRecords = async () => {
  try {
    const res = await api.get(`/mypets/${petId.value}/medical-records`);
    medicalRecords.value = Array.isArray(res.data) ? res.data : (res.data.items || []);
  } catch {
    medicalRecords.value = [];
  }
};

const fetchVaccinations = async () => {
  try {
    const res = await api.get(`/mypets/${petId.value}/vaccinations`);
    vaccinations.value = Array.isArray(res.data) ? res.data : (res.data.items || []);
  } catch {
    vaccinations.value = [];
  }
};

const fetchAppointments = async () => {
  try {
    const res = await api.get(`/my-appointments?pageSize=100`);
    const all = Array.isArray(res.data) ? res.data : (res.data.items || []);
    appointments.value = all.filter((a: any) => String(a.petId) === String(petId.value));
  } catch {
    appointments.value = [];
  }
};

const fetchPrescriptions = async () => {
  try {
    const res = await api.get(`/mypets/${petId.value}/prescriptions`);
    prescriptions.value = Array.isArray(res.data) ? res.data : (res.data.items || []);
  } catch {
    prescriptions.value = [];
  }
};

// ===== Chart Logic =====
const renderChart = () => {
  if (!chartCanvas.value) return;
  
  if (chartInstance.value) {
    chartInstance.value.destroy();
  }

  const recordsWithWeight = [...medicalRecords.value]
    .filter(r => r.weight != null)
    .sort((a, b) => new Date(a.examinationDate || a.createdAt).getTime() - new Date(b.examinationDate || b.createdAt).getTime());

  if (recordsWithWeight.length === 0) return;

  const labels = recordsWithWeight.map(r => formatDateShort(r.examinationDate || r.createdAt));
  const data = recordsWithWeight.map(r => r.weight);

  const ctx = chartCanvas.value.getContext('2d');
  if (!ctx) return;

  // Create gradient
  const gradient = ctx.createLinearGradient(0, 0, 0, 300);
  gradient.addColorStop(0, 'rgba(2, 132, 199, 0.25)'); // Tailwind sky-600 with opacity
  gradient.addColorStop(1, 'rgba(2, 132, 199, 0.01)');

  chartInstance.value = new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: 'Cân nặng (kg)',
        data,
        borderColor: '#0284c7', 
        backgroundColor: gradient,
        borderWidth: 3,
        tension: 0.4,
        fill: true,
        pointBackgroundColor: '#ffffff',
        pointBorderColor: '#0284c7',
        pointBorderWidth: 2,
        pointRadius: 5,
        pointHoverRadius: 7
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: '#1e293b',
          titleFont: { family: 'Inter', size: 13 },
          bodyFont: { family: 'Inter', size: 14, weight: 'bold' },
          padding: 12,
          cornerRadius: 8,
          displayColors: false,
        }
      },
      scales: {
        y: {
          beginAtZero: false,
          grid: { color: '#f1f5f9', tickLength: 0 },
          border: { display: false },
          ticks: { font: { family: 'Inter' }, color: '#64748b', padding: 10 }
        },
        x: {
          grid: { display: false },
          border: { display: false },
          ticks: { font: { family: 'Inter' }, color: '#64748b' }
        }
      }
    }
  });
};

watch(() => activeTab.value, (newTab) => {
  if (newTab === 'overview') {
    nextTick(() => {
      renderChart();
    });
  }
});

// ===== Helpers =====
const goBack = () => {
  if (window.history.length > 1) router.back();
  else router.push('/dashboard');
};

const getAvatarUrl = (path: string) => {
  if (!path) return '';
  if (path.startsWith('http')) return path;
  return `${backendUrl}${path}`;
};

const getSpeciesEmoji = (species: string): string => {
  const map: Record<string, string> = {
    'Chó': '🐕', 'Mèo': '🐈', 'Thỏ': '🐇', 'Chim': '🦜', 'Cá': '🐟', 'Bò sát': '🦎',
  };
  return map[species] || '🐾';
};

const calculateAge = (birthDate: string | null): string => {
  if (!birthDate) return 'Chưa rõ tuổi';
  const birth = new Date(birthDate);
  const now = new Date();
  const diffMs = now.getTime() - birth.getTime();
  const totalMonths = Math.floor(diffMs / (1000 * 60 * 60 * 24 * 30.44));
  const years = Math.floor(totalMonths / 12);
  const months = totalMonths % 12;
  if (years === 0) return months === 0 ? 'Sơ sinh' : `${months} tháng`;
  if (months === 0) return `${years} tuổi`;
  return `${years} tuổi, ${months} tháng`;
};

const formatDate = (dateStr: string | null | undefined): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatDateShort = (dateStr: string | null | undefined): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
};

const formatDateFull = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatDateTime = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

const formatDateTimeFull = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('vi-VN', {
    weekday: 'short', day: '2-digit', month: '2-digit', year: 'numeric'
  });
};

const formatTimeOnly = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};

const getStatusLabel = (status: string): string => {
  const map: Record<string, string> = {
    waiting: 'Chờ xác nhận',
    pending: 'Chờ xác nhận',
    confirmed: 'Đã xác nhận',
    in_progress: 'Đang tiến hành',
    completed: 'Hoàn thành',
    cancelled: 'Đã hủy',
    checked_in: 'Đã check-in',
  };
  return map[status?.toLowerCase()] || status;
};

const getStatusClass = (status: string): string => {
  const s = status?.toLowerCase();
  if (s === 'completed') return 'bg-success-subtle text-success border border-success border-opacity-25';
  if (s === 'confirmed' || s === 'checked_in') return 'bg-info-subtle text-info border border-info border-opacity-25';
  if (s === 'in_progress') return 'bg-primary-subtle text-primary border border-primary border-opacity-25';
  if (s === 'cancelled') return 'bg-danger-subtle text-danger border border-danger border-opacity-25';
  return 'bg-warning-subtle text-warning border border-warning border-opacity-25';
};

const isVaccineOverdue = (nextDueDate: string): boolean => {
  return new Date(nextDueDate) < new Date();
};

const isSoonDue = (nextDueDate: string): boolean => {
  const due = new Date(nextDueDate);
  const soon = new Date();
  soon.setDate(soon.getDate() + 30);
  return due >= new Date() && due <= soon;
};

const isVaccineDueSoon = (): boolean => {
  return vaccinations.value.some(v => v.nextDueDate && (isVaccineOverdue(v.nextDueDate) || isSoonDue(v.nextDueDate)));
};

// ===== Lifecycle =====
onMounted(fetchAll);
</script>

<style scoped>
/* Google Font applied if not globally */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.pet-profile-page {
  min-height: 100vh;
  background: #f8fafc; /* light gray bg like mockup */
  font-family: 'Inter', sans-serif;
  padding-bottom: 3rem;
}

/* TOPBAR */
.profile-topbar {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 2rem;
  background: white;
  border-bottom: 1px solid #e2e8f0;
  position: sticky;
  top: 0;
  z-index: 100;
}

.back-btn {
  display: flex;
  align-items: center;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 0.4rem 0.8rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  color: #475569;
  transition: all 0.2s;
}

.back-btn:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.breadcrumb-trail {
  font-size: 0.875rem;
  color: #64748b;
  display: flex;
  align-items: center;
}

.breadcrumb-trail .active { color: #0f172a; font-weight: 500; }

/* MAIN CONTAINER */
.dashboard-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

/* TOP ROW */
.top-cards-row {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 2rem;
  align-items: stretch;
}

/* AVATAR CARD */
.avatar-card {
  background: white;
  border-radius: 16px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  display: flex;
  align-items: center;
  gap: 1.25rem;
  border: 1px solid #f1f5f9;
  flex: 1;
  min-width: 320px;
}

.avatar-wrapper {
  position: relative;
  width: 90px;
  height: 90px;
  border-radius: 14px;
  overflow: visible;
}

.avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 14px;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);
}

.avatar-emoji {
  width: 100%;
  height: 100%;
  border-radius: 14px;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
}

.badge-sterilized {
  position: absolute;
  bottom: -6px;
  right: -6px;
  width: 24px;
  height: 24px;
  background: #10b981;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 12px;
  border: 2px solid white;
}

.pet-name-lg {
  font-size: 1.75rem;
  font-weight: 800;
  color: #0f172a;
  margin: 0 0 0.25rem 0;
  letter-spacing: -0.5px;
}

.pet-breed-lg {
  font-size: 0.9rem;
  color: #64748b;
  margin: 0;
}

/* INFO CARDS GROUP */
.info-cards-group {
  display: flex;
  gap: 1rem;
  flex: 2;
}

.info-card {
  background: white;
  border-radius: 16px;
  padding: 1.25rem;
  flex: 1;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  border: 1px solid #f1f5f9;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.allergy-card-top {
  background: #fef2f2;
  border: 1px solid #fee2e2;
}

.card-label {
  font-size: 0.75rem;
  text-transform: uppercase;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 0.5rem;
  letter-spacing: 0.5px;
}

.card-value {
  font-size: 1.1rem;
  font-weight: 700;
  color: #0f172a;
  margin-bottom: 0.25rem;
}

.card-sub {
  font-size: 0.8rem;
  color: #94a3b8;
}

/* NEXT APPT CARD */
.next-appt-card {
  background: white;
  border-radius: 16px;
  padding: 1.25rem;
  width: 280px;
  box-shadow: 0 4px 20px rgba(245, 158, 11, 0.08);
  border: 1px solid #ffedd5;
  border-top: 4px solid #f59e0b; /* Orange top border */
  display: flex;
  flex-direction: column;
}

.nac-header {
  font-size: 0.9rem;
  font-weight: 700;
  color: #0f172a;
  margin-bottom: 0.75rem;
}

.nac-date {
  font-size: 0.95rem;
  font-weight: 700;
  color: #c2410c;
  margin-bottom: 0.25rem;
}

.nac-time {
  font-size: 1.25rem;
  font-weight: 800;
  color: #0f172a;
  margin-bottom: 0.5rem;
}

.nac-service {
  font-size: 0.85rem;
  color: #475569;
  margin-bottom: 1rem;
}

.nac-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: auto;
  padding-top: 0.75rem;
  border-top: 1px solid #f1f5f9;
}

.nac-doctor {
  font-size: 0.8rem;
  color: #64748b;
}

.btn-link {
  background: none;
  border: none;
  color: #0284c7;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0;
}

/* TABS NAV */
.clean-tabs-wrap {
  display: flex;
  gap: 2rem;
  border-bottom: 1px solid #e2e8f0;
  margin-bottom: 2rem;
  overflow-x: auto;
}

.clean-tab-btn {
  background: none;
  border: none;
  padding: 1rem 0;
  font-size: 0.95rem;
  font-weight: 600;
  color: #64748b;
  cursor: pointer;
  border-bottom: 3px solid transparent;
  transition: all 0.2s;
  position: relative;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  white-space: nowrap;
}

.clean-tab-btn:hover {
  color: #0f172a;
}

.clean-tab-btn.active {
  color: #0284c7; /* Sky blue active */
  border-bottom-color: #0284c7;
}

.red-dot {
  width: 6px;
  height: 6px;
  background: #ef4444;
  border-radius: 50%;
  position: absolute;
  top: 10px;
  right: -8px;
}

/* OVERVIEW MAIN */
.stats-row {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  flex: 1;
  background: white;
  border-radius: 12px;
  padding: 1.25rem;
  box-shadow: 0 1px 2px rgba(0,0,0,0.03);
  border: 1px solid #f1f5f9;
  display: flex;
  align-items: flex-start;
  gap: 1rem;
}

.sc-icon {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
}

/* VACCINE CARDS */
.vaccine-card {
  transition: all 0.2s;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
}
.vaccine-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(0,0,0,0.05);
}
.upcoming-card {
  border-left: 4px solid #f59e0b !important;
}

/* PRESCRIPTION CARDS */
.prescription-card {
  transition: transform 0.2s, box-shadow 0.2s;
}
.prescription-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 24px -8px rgba(0,0,0,0.08) !important;
}
.border-left-success {
  border-left: 4px solid #22c55e !important;
}
.border-left-secondary {
  border-left: 4px solid #94a3b8 !important;
}
.rx-table td {
  padding-left: 0.5rem;
  padding-right: 0.5rem;
}
.rx-table th {
  padding-left: 0.5rem;
  padding-right: 0.5rem;
}
.rx-table tbody tr:last-child {
  border-bottom: none !important;
}

@media (max-width: 992px) {
  .top-cards-row {
    flex-direction: column;
  }
  .info-cards-group {
    flex-wrap: wrap;
  }
  .overview-main-layout {
    flex-direction: column;
  }
}

.bg-primary-light { background: #f0f9ff; }
.bg-success-light { background: #f0fdf4; }
.bg-danger-light { background: #fef2f2; }

.sc-info { flex: 1; overflow: hidden; }

.sc-label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 0.25rem;
}

.sc-value {
  font-size: 1.5rem;
  font-weight: 800;
  color: #0f172a;
  margin-bottom: 0.25rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.weight-diff {
  font-size: 0.8rem;
  font-weight: 600;
  display: flex;
  align-items: center;
}

.sc-sub {
  font-size: 0.8rem;
  color: #94a3b8;
}

.overview-main-layout {
  display: flex;
  gap: 1.5rem;
}

.overview-left { flex: 7; }
.overview-right { flex: 3; min-width: 300px; }

/* PANELS */
.dashboard-panel {
  background: white;
  border-radius: 16px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
  overflow: hidden;
}

.panel-header {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.panel-title {
  font-size: 1.05rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.panel-body {
  padding: 1.5rem;
}

.chart-empty-state {
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255,255,255,0.8);
}

/* LKG DETAILS */
.lkg-date {
  font-size: 0.95rem;
  font-weight: 700;
  color: #0f172a;
  margin-bottom: 0.25rem;
}

.lkg-doctor {
  font-size: 0.85rem;
}

.lkg-notes {
  font-size: 0.85rem;
  color: #64748b;
  background: #f8fafc;
  padding: 1rem;
  border-radius: 8px;
  border-left: 3px solid #0284c7;
}

.notes-content {
  color: #334155;
  line-height: 1.5;
}

.allergy-tag-large {
  background: #fef2f2;
  color: #dc2626;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  border: 1px solid #fee2e2;
}

/* RESPONSIVE */
@media (max-width: 1024px) {
  .top-cards-row { flex-wrap: wrap; }
  .next-appt-card { width: 100%; flex: auto; }
  .overview-main-layout { flex-direction: column; }
}

@media (max-width: 768px) {
  .info-cards-group { flex-direction: column; }
  .stats-row { flex-direction: column; }
}
</style>
