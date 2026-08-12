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
    <div v-if="loading" class="d-flex flex-column align-items-center justify-content-center min-vh-100" style="background: linear-gradient(135deg, #f0f4f8 0%, #e0eaf5 100%);">
      <div class="spinner-grow text-primary" style="width: 4rem; height: 4rem;" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <h5 class="mt-4 fw-bold text-primary">Đang tải hồ sơ...</h5>
      <p class="text-muted">Vui lòng đợi trong giây lát</p>
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
              <div class="card-sub" v-if="ownerProfile?.phoneNumber">{{ ownerProfile.phoneNumber }}</div>
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
              <div class="glass-card p-4 d-flex align-items-center gap-3 hover-glow w-100" style="flex: 1;">
                <div class="sc-icon" style="background: rgba(245, 158, 11, 0.1); color: var(--primary-dark);"><i class="bi bi-speedometer2"></i></div>
                <div class="sc-info">
                  <div class="sc-label" style="text-transform: uppercase; letter-spacing: 0.5px;">Cân nặng</div>
                  <div class="sc-value fs-4 fw-bold">
                    {{ pet.weight ? pet.weight + ' kg' : 'Chưa có' }}
                    <span v-if="weightDiff" class="weight-diff ms-2" :class="weightDiff > 0 ? 'text-success' : 'text-danger'">
                      <i :class="weightDiff > 0 ? 'bi bi-arrow-up' : 'bi bi-arrow-down'"></i>{{ Math.abs(weightDiff) }}kg
                    </span>
                  </div>
                  <div class="sc-sub" v-if="latestMedicalRecord">Đo lần cuối: {{ formatDateShort(latestMedicalRecord.visitDate || latestMedicalRecord.createdAt) }}</div>
                  <div class="sc-sub" v-else>Chưa có dữ liệu đo</div>
                </div>
              </div>

              <div class="glass-card p-4 d-flex align-items-center gap-3 hover-glow w-100" style="flex: 1;">
                <div class="sc-icon bg-success-light text-success"><i class="bi bi-file-medical"></i></div>
                <div class="sc-info">
                  <div class="sc-label" style="text-transform: uppercase; letter-spacing: 0.5px;">Lần khám gần nhất</div>
                  <template v-if="latestMedicalRecord">
                    <div class="sc-value fs-4 fw-bold">{{ formatDate(latestMedicalRecord.visitDate || latestMedicalRecord.createdAt) }}</div>
                    <div class="sc-sub">{{ latestMedicalRecord.serviceName || 'Khám bệnh' }}</div>
                  </template>
                  <template v-else>
                    <div class="sc-value fw-bold text-muted" style="font-size: 1.25rem;">Chưa có</div>
                    <div class="sc-sub">Chưa có lịch sử khám</div>
                  </template>
                </div>
              </div>

              <div class="glass-card p-4 d-flex align-items-center gap-3 hover-glow w-100" style="flex: 1;" :style="pet.allergyNote ? 'box-shadow: 0 0 15px rgba(239, 68, 68, 0.2); border-color: rgba(239, 68, 68, 0.3);' : ''">
                <div class="sc-icon bg-danger-light text-danger"><i class="bi bi-exclamation-triangle"></i></div>
                <div class="sc-info">
                  <div class="sc-label" style="text-transform: uppercase; letter-spacing: 0.5px;">Dị ứng & Lưu ý</div>
                  <div class="sc-value fs-4 fw-bold text-danger">{{ pet.allergyNote ? 'Có lưu ý' : 'Không ghi nhận' }}</div>
                  <div class="sc-sub text-danger text-truncate" :title="pet.allergyNote">{{ pet.allergyNote || 'Sức khỏe bình thường' }}</div>
                </div>
              </div>
            </div>

            <!-- Main Layout 70-30 -->
            <div class="overview-main-layout">
              <!-- Left Col: Chart -->
              <div class="overview-left">
                <div class="glass-card">
                  <div class="panel-header d-flex align-items-center justify-content-between">
                    <h5 class="panel-title fw-bold m-0 d-flex align-items-center gap-2">
                      <i class="bi bi-graph-up-arrow text-primary"></i> Biểu đồ cân nặng
                    </h5>
                    <select class="form-select input-premium form-select-sm w-auto rounded-3">
                      <option>Tất cả thời gian</option>
                      <option>6 tháng qua</option>
                    </select>
                  </div>
                  <div class="panel-body p-4">
                    <div class="chart-container" style="height: 300px; position: relative;">
                      <canvas ref="chartCanvas" id="weightChart"></canvas>
                      <div v-if="!hasWeightData" class="chart-empty-state rounded-4">
                        <p class="text-muted fw-medium">Chưa có đủ dữ liệu cân nặng để vẽ biểu đồ.</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Right Col: Details -->
              <div class="overview-right">
                
                <div class="glass-card mb-4" v-if="pet.allergyNote" style="background: #fef2f2 !important; border: 1px solid #fca5a5 !important;">
                  <div class="panel-body p-4 text-center">
                    <i class="bi bi-exclamation-triangle-fill text-danger fs-1 mb-2 d-inline-block" style="animation: pulse 2s infinite;"></i>
                    <h6 class="fw-bold text-danger mb-1">CẢNH BÁO DỊ ỨNG</h6>
                    <p class="text-danger fw-medium mb-0">{{ pet.allergyNote }}</p>
                  </div>
                </div>

                <div class="glass-card">
                  <div class="panel-header border-bottom-0 pb-2 pt-4 px-4">
                    <h6 class="panel-title fw-bold"><i class="bi bi-file-earmark-medical-fill text-primary me-2"></i>Lần khám gần nhất</h6>
                  </div>
                  <div class="panel-body pt-0 px-4 pb-4" v-if="latestMedicalRecord">
                    
                    <div class="position-relative ms-2 mt-3" style="border-left: 2px solid var(--border-color); padding-left: 1rem;">
                      <div class="position-absolute rounded-circle" style="width: 10px; height: 10px; background: var(--primary-gold); left: -6px; top: 6px;"></div>
                      
                      <div class="fw-bold text-dark mb-1">{{ formatDateFull(latestMedicalRecord.visitDate || latestMedicalRecord.createdAt) }}</div>
                      <div class="small text-muted fw-medium mb-3 d-flex align-items-center gap-1">
                        <i class="bi bi-person-badge"></i> {{ latestMedicalRecord.doctorName ? `Bs. ${latestMedicalRecord.doctorName}` : 'Chưa cập nhật bác sĩ' }}
                      </div>
                      
                      <div class="p-3 rounded-3" style="background-color: var(--primary-cream); border-left: 4px solid var(--primary-gold);">
                        <div class="small fw-bold mb-1" style="color: var(--primary-dark);">Ghi chú lâm sàng:</div>
                        <div class="small fw-medium font-italic" style="color: var(--text-dark);">
                          "{{ latestMedicalRecord.notes || latestMedicalRecord.diagnosis || 'Không có ghi chú đặc biệt.' }}"
                        </div>
                      </div>
                    </div>

                    <button class="btn-premium-outline w-100 mt-4 hover-arrow" @click="activeTab = 'history'">
                      Xem toàn bộ bệnh án <i class="bi bi-arrow-right"></i>
                    </button>
                  </div>
                  <div class="panel-body text-center" v-else>
                    <p class="text-muted py-4 mb-0">Chưa có bệnh án nào được ghi nhận.</p>
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
                  <h4 class="fw-bold text-dark mb-0">Lịch sử bệnh án</h4>
                  <div class="dropdown position-relative">
                    <button class="btn btn-light btn-sm text-muted fw-medium border rounded-3 px-3" type="button" @click="isHistoryFilterOpen = !isHistoryFilterOpen">
                      <i class="bi bi-funnel me-1"></i> Lọc: {{ historyFilterLabel }} <i class="bi bi-chevron-down ms-1"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end shadow-sm border-0 rounded-3" :class="{ 'show': isHistoryFilterOpen }" style="position: absolute; top: 100%; right: 0; z-index: 1000;" @click="isHistoryFilterOpen = false">
                      <li><a class="dropdown-item" href="#" @click.prevent="historyFilter = 'all'">Tất cả</a></li>
                      <li><a class="dropdown-item" href="#" @click.prevent="historyFilter = 'followUp'">Cần tái khám</a></li>
                      <li><a class="dropdown-item" href="#" @click.prevent="historyFilter = 'completed'">Đã hoàn thành</a></li>
                    </ul>
                  </div>
                </div>
                
                <div v-if="filteredMedicalRecords.length === 0" class="text-center py-5 text-muted glass-card border">
                  Không tìm thấy bệnh án nào.
                </div>
                <div v-else>
                  <div class="timeline-container position-relative ps-4 ms-2 mt-4 mb-4">
                    <!-- Vertical Line -->
                    <div class="position-absolute h-100" style="left: 0; top: 0; width: 2px; background-color: rgba(245, 158, 11, 0.3);"></div>

                    <div v-for="(rec, index) in paginatedMedicalRecords" :key="rec.id" class="position-relative mb-4">
                      <!-- Dot -->
                      <div class="position-absolute rounded-circle" :style="`width: 14px; height: 14px; left: -30px; top: 24px; background-color: ${rec.followUpDate ? 'var(--primary-gold)' : '#0284c7'}; border: 3px solid white; box-shadow: 0 0 0 1px ${rec.followUpDate ? 'var(--primary-gold)' : '#0284c7'};`"></div>
                      
                      <!-- Card -->
                      <div class="glass-card overflow-hidden" :style="`border-left: 4px solid ${rec.followUpDate ? 'var(--primary-gold)' : '#0284c7'} !important;`">
                        <div class="panel-body p-4">
                          <div class="d-flex justify-content-between align-items-start mb-2">
                             <div class="d-flex align-items-center flex-wrap gap-3">
                               <h5 class="fw-bold mb-0 text-dark">{{ rec.serviceName || 'Khám tổng quát' }}</h5>
                               <span class="badge rounded-pill px-3 py-1" :class="rec.followUpDate ? 'bg-warning-subtle text-warning' : 'bg-info-subtle text-info'" style="font-size: 0.65rem; font-weight: 800; letter-spacing: 0.5px;">
                                 {{ rec.followUpDate ? 'CẦN TÁI KHÁM' : 'ĐÃ HOÀN THÀNH' }}
                               </span>
                             </div>
                             <div class="fw-bold text-dark" style="font-size: 1.1rem;">{{ formatDateShort(rec.visitDate || rec.createdAt) }}</div>
                          </div>
                          
                          <div class="d-flex gap-4 small text-muted mb-4 fw-medium">
                            <div class="d-flex align-items-center"><i class="bi bi-calendar3 me-1"></i> Ngày khám: {{ formatDate(rec.visitDate || rec.createdAt) }}</div>
                            <div class="d-flex align-items-center" v-if="rec.doctorName"><i class="bi bi-person-badge me-1"></i> Bác sĩ: {{ rec.doctorName }}</div>
                          </div>

                          <!-- SUMMARY (Compact) -->
                          <div class="d-flex flex-column gap-2 mb-3">
                            <div class="d-flex align-items-center text-dark small">
                              <i class="bi bi-clipboard2-pulse text-warning me-2 fs-6"></i>
                              <span class="fw-bold me-1">Chẩn đoán:</span> 
                              <span class="text-truncate" style="max-width: 250px;" :title="rec.diagnosis">{{ rec.diagnosis || 'Chưa có' }}</span>
                            </div>
                            
                            <div class="d-flex align-items-center gap-3 small text-muted">
                              <span v-if="rec.prescribedMedicines && rec.prescribedMedicines.length > 0">
                                <i class="bi bi-capsule-pill text-success me-1"></i> Kê {{ rec.prescribedMedicines.length }} loại thuốc
                              </span>
                              <span v-else>
                                <i class="bi bi-capsule-pill text-secondary me-1"></i> Không thuốc
                              </span>
                              
                              <span v-if="rec.clinicalSigns">
                                <i class="bi bi-activity text-info me-1"></i> Dấu hiệu lâm sàng
                              </span>
                            </div>
                          </div>

                          <div class="d-flex gap-2 flex-wrap mt-2">
                            <button v-if="rec.followUpDate" class="btn-premium px-3 py-1 hover-arrow" style="font-size: 0.8rem;">
                              Tái khám <i class="bi bi-arrow-right"></i>
                            </button>
                            <button @click="openMedicalRecordModal(rec)" class="btn btn-sm btn-outline-secondary px-3 py-1 rounded-pill fw-bold" style="font-size: 0.8rem;">
                              <i class="bi bi-eye"></i> Chi tiết
                            </button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  
                  <!-- Pagination controls -->
                  <div class="d-flex justify-content-center align-items-center mt-3 gap-2" v-if="totalMedicalHistoryPages > 0">
                    <button class="btn btn-outline-primary btn-sm rounded-circle" style="width: 32px; height: 32px; padding: 0; display: flex; align-items: center; justify-content: center;" :disabled="medicalHistoryPage === 1" @click="medicalHistoryPage--">
                      <i class="bi bi-chevron-left"></i>
                    </button>
                    <span class="text-muted small fw-bold mx-2">Trang {{ medicalHistoryPage }} / {{ totalMedicalHistoryPages }}</span>
                    <button class="btn btn-outline-primary btn-sm rounded-circle" style="width: 32px; height: 32px; padding: 0; display: flex; align-items: center; justify-content: center;" :disabled="medicalHistoryPage === totalMedicalHistoryPages" @click="medicalHistoryPage++">
                      <i class="bi bi-chevron-right"></i>
                    </button>
                  </div>
                </div>
              </div>
              
              <!-- Right Column -->
              <div class="col-lg-4">
                <!-- Quick Summary -->
                <!-- Quick Summary -->
                <div class="glass-card mb-4 mt-2">
                  <div class="panel-body p-4">
                    <h5 class="fw-bold text-dark mb-4">Tóm tắt nhanh</h5>
                    
                    <div class="mb-4">
                      <div class="text-muted small fw-bold mb-2" style="font-size: 0.7rem; letter-spacing: 1px;">LẦN KHÁM CUỐI</div>
                      <div class="d-flex align-items-center gap-3 p-3 rounded-4" style="border: 1px solid #f1f5f9; background: rgba(255,255,255,0.6);">
                        <div class="rounded-circle d-flex align-items-center justify-content-center" style="width: 48px; height: 48px; background: #e0f2fe; color: #0284c7;">
                          <i class="bi bi-calendar-check fs-5"></i>
                        </div>
                        <div>
                          <div class="fw-bold text-dark mb-1" style="font-size: 0.95rem;">{{ medicalRecords.length > 0 ? formatDate(medicalRecords[0].visitDate || medicalRecords[0].createdAt) : 'Chưa có' }}</div>
                          <div class="small text-muted fw-medium">{{ medicalRecords.length > 0 ? (medicalRecords[0].serviceName || 'Kiểm tra định kỳ') : '-' }}</div>
                        </div>
                      </div>
                    </div>

                    <div>
                      <div class="text-muted small fw-bold mb-2" style="font-size: 0.7rem; letter-spacing: 1px;">LỊCH TỚI HẠN</div>
                      <div class="d-flex align-items-center gap-3 p-3 rounded-4" style="border: 1px solid #f1f5f9; background: rgba(255,255,255,0.6);">
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
                <div class="glass-card" style="background: #fef2f2 !important; border: 1px solid #fca5a5 !important;">
                  <div class="panel-body p-4">
                    <h5 class="fw-bold text-danger mb-4 d-flex align-items-center gap-2">
                      <i class="bi bi-exclamation-triangle-fill" style="animation: pulse 2s infinite;"></i> Cảnh báo y tế
                    </h5>
                    
                    <div class="mb-4">
                      <div class="text-danger small fw-bold mb-2" style="font-size: 0.7rem; letter-spacing: 1px;">DỊ ỨNG & LƯU Ý</div>
                      <div class="fw-bold text-dark d-flex align-items-center gap-2 mb-2" style="font-size: 0.95rem;">
                        <div class="rounded-circle bg-danger" style="width: 6px; height: 6px;"></div> {{ pet?.allergyNote ? 'Có lưu ý dị ứng' : 'Không phát hiện dị ứng' }}
                      </div>
                      <div class="small text-muted fw-medium" style="line-height: 1.5;">{{ pet?.allergyNote || 'Chưa ghi nhận phản ứng phụ với thành phần nào.' }}</div>
                    </div>

                    <div>
                      <div class="text-muted small fw-bold mb-2" style="font-size: 0.7rem; letter-spacing: 1px;">CHẾ ĐỘ ĂN</div>
                      <div class="small text-dark fw-medium" style="line-height: 1.6;">
                        {{ pet?.currentDiet || 'Không có yêu cầu đặc biệt về khẩu phần ăn. Dùng thức ăn tiêu chuẩn.' }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-show="activeTab === 'appointments'" class="tab-pane" @click="openMenuId = null">
            <div v-if="upcomingAppointments.length === 0" class="appt-empty-state">
              <div class="appt-empty-icon">📅</div>
              <h4 class="fw-bold text-dark mb-2">Lịch trình đang trống</h4>
              <p class="text-muted mb-4">Thú cưng của bạn chưa có lịch hẹn nào sắp tới.</p>
              <button class="btn btn-primary px-5 py-2 rounded-pill fw-bold" @click="$router.push('/dashboard')">
                <i class="bi bi-calendar-plus me-2"></i>Tạo lịch hẹn mới
              </button>
            </div>

            <div v-else class="appt-layout">

              <!-- ============ HERO CARD ============ -->
              <div class="appt-hero-card">
                <div class="appt-hero-accent"></div>
                <div class="appt-hero-body">
                  <div class="appt-hero-left">
                    <div class="d-flex align-items-center gap-2 mb-3">
                      <span class="appt-countdown-badge">
                        <i class="bi bi-clock-history me-1"></i>{{ getDaysUntil(upcomingAppointments[0].appointmentDate) }}
                      </span>
                      <span class="appt-status-badge" :class="'status-' + upcomingAppointments[0].status">{{ getStatusLabel(upcomingAppointments[0].status) }}</span>
                    </div>

                    <h2 class="appt-hero-title">{{ upcomingAppointments[0].serviceName || 'Lịch khám tổng quát' }}</h2>
                    <div class="appt-hero-doctor">
                      <span class="appt-doctor-avatar"><i class="bi bi-person-fill"></i></span>
                      Bs. <strong>{{ upcomingAppointments[0].doctorName || 'Sẽ phân công sau' }}</strong>
                    </div>

                    <div class="appt-hero-datetime">
                      <div class="appt-datetime-chip appt-chip-blue">
                        <div class="appt-chip-icon"><i class="bi bi-calendar-event"></i></div>
                        <div>
                          <div class="appt-chip-label">NGÀY KHÁM</div>
                          <div class="appt-chip-value">{{ formatDate(upcomingAppointments[0].appointmentDate) }}</div>
                        </div>
                      </div>
                      <div class="appt-datetime-chip appt-chip-amber">
                        <div class="appt-chip-icon appt-chip-icon-amber"><i class="bi bi-clock"></i></div>
                        <div>
                          <div class="appt-chip-label">GIỜ KHÁM</div>
                          <div class="appt-chip-value">{{ formatTimeOnly(upcomingAppointments[0].appointmentDate) }}</div>
                        </div>
                      </div>
                    </div>

                    <div class="appt-hero-note">
                      <i class="bi bi-info-circle-fill text-warning me-2 flex-shrink-0"></i>
                      <span>{{ upcomingAppointments[0].notes || 'Vui lòng đến sớm 10 phút trước giờ hẹn.' }}</span>
                    </div>
                  </div>

                  <div class="appt-hero-right">
                    <div class="appt-qr-preview">
                      <i class="bi bi-qr-code-scan"></i>
                    </div>
                    <div class="appt-qr-code-text">{{ upcomingAppointments[0].qrToken ? upcomingAppointments[0].qrToken.substring(0,6).toUpperCase() : 'CHECKIN' }}</div>
                    <div class="appt-qr-sublabel">MÃ CHECK-IN</div>

                    <button v-if="upcomingAppointments[0].qrToken" @click.stop="openQrModal(upcomingAppointments[0])" class="appt-btn-primary">
                      <i class="bi bi-qr-code me-2"></i>Hiện mã QR
                    </button>
                    <button v-else class="appt-btn-primary" style="opacity: 0.6; cursor: not-allowed; background: #94a3b8; box-shadow: none;" disabled title="Vui lòng chờ xác nhận để lấy mã QR">
                      <i class="bi bi-hourglass-split me-2"></i>Chờ xác nhận
                    </button>
                    <button @click.stop="confirmCancelAppointment(upcomingAppointments[0].id)" class="appt-btn-danger">
                      Hủy lịch hẹn
                    </button>
                  </div>
                </div>
              </div>

              <!-- ============ FUTURE APPOINTMENTS GRID ============ -->
              <div v-if="upcomingAppointments.length > 1" class="appt-grid-section">
                <div class="appt-section-title">
                  <i class="bi bi-calendar-week text-primary me-2"></i>Các lịch hẹn tiếp theo
                </div>
                <div class="appt-grid">
                  <div class="appt-grid-card" v-for="appt in upcomingAppointments.slice(1)" :key="appt.id">
                    <div class="appt-card-strip" :class="'strip-' + appt.status"></div>

                    <div class="appt-card-body">
                      <div class="appt-card-header">
                        <span class="appt-card-status-badge" :class="'status-' + appt.status">{{ getStatusLabel(appt.status) }}</span>
                        <div class="appt-card-menu" @click.stop>
                          <button class="appt-menu-trigger" @click="openMenuId = openMenuId === appt.id ? null : appt.id">
                            <i class="bi bi-three-dots-vertical"></i>
                          </button>
                          <div class="appt-menu-dropdown" v-if="openMenuId === appt.id">
                            <button v-if="appt.qrToken" class="appt-menu-item" @click="openQrModal(appt); openMenuId = null">
                              <i class="bi bi-qr-code me-2"></i>Xem mã QR
                            </button>
                            <button class="appt-menu-item appt-menu-item-danger" @click="confirmCancelAppointment(appt.id); openMenuId = null">
                              <i class="bi bi-x-circle me-2"></i>Hủy lịch hẹn
                            </button>
                          </div>
                        </div>
                      </div>

                      <h6 class="appt-card-title">{{ appt.serviceName }}</h6>
                      <div class="appt-card-meta">
                        <span><i class="bi bi-calendar3"></i>{{ formatDateShort(appt.appointmentDate) }}</span>
                        <span><i class="bi bi-clock"></i>{{ formatTimeOnly(appt.appointmentDate) }}</span>
                      </div>
                      <div class="appt-card-doctor">
                        <i class="bi bi-person-badge"></i>{{ appt.doctorName || 'Chưa phân công' }}
                      </div>
                    </div>

                    <div class="appt-card-footer">
                      <span class="appt-card-code">{{ appt.qrToken ? appt.qrToken.substring(0,6).toUpperCase() : '---' }}</span>
                      <button v-if="appt.qrToken" class="appt-card-qr-btn" @click.stop="openQrModal(appt)">
                        <i class="bi bi-qr-code me-1"></i>Mã QR
                      </button>
                      <button v-else class="appt-card-qr-btn text-muted" style="border-color: transparent; background: transparent; cursor: not-allowed;" disabled title="Chờ xác nhận">
                        <i class="bi bi-hourglass-split me-1"></i>Chờ duyệt
                      </button>
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
                  <div class="dropdown position-relative">
                    <button class="btn btn-sm btn-light text-primary fw-bold rounded-pill px-3" type="button" @click="isVaccineFilterOpen = !isVaccineFilterOpen">
                      <i class="bi bi-funnel me-1"></i> Lọc: {{ vaccineFilterLabel }} <i class="bi bi-chevron-down ms-1"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end shadow-sm border-0 rounded-3" :class="{ 'show': isVaccineFilterOpen }" style="position: absolute; top: 100%; right: 0; z-index: 1000;" @click="isVaccineFilterOpen = false">
                      <li><a class="dropdown-item" href="#" @click.prevent="vaccineFilter = 'all'">Tất cả</a></li>
                      <li><a class="dropdown-item" href="#" @click.prevent="vaccineFilter = 'hasDue'">Có hẹn tái chủng</a></li>
                    </ul>
                  </div>
                </div>

                <div v-if="historyVaccines.length === 0" class="p-5 text-center rounded-4 border bg-white shadow-sm">
                  <p class="text-muted mb-0">Chưa có dữ liệu tiêm phòng.</p>
                </div>
                
                <div v-else>
                  <div class="d-flex flex-column gap-3 mb-4">
                    <div v-for="vac in paginatedHistoryVaccines" :key="'hist-' + vac.id" class="vaccine-card history-card p-4 rounded-4 bg-white border d-flex gap-4">
                      <div class="vac-icon-wrap">
                        <div class="vac-icon bg-success-subtle text-success d-flex align-items-center justify-content-center rounded-circle" style="width: 48px; height: 48px;">
                          <i class="bi bi-bandaid fs-4"></i>
                        </div>
                      </div>
                      <div class="flex-grow-1">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                          <div>
                            <h5 class="fw-bold text-dark mb-1">{{ vac.vaccineName }}</h5>
                            <div class="small text-muted mb-3">{{ vac.reasonForVisit || vac.note || 'Mũi tiêm định kỳ' }}</div>
                          </div>
                          <span class="badge bg-success-subtle text-success border border-success-subtle rounded-pill px-3 py-2 fw-bold">
                            <i class="bi bi-check-circle-fill me-1"></i>Đã tiêm
                          </span>
                        </div>
                        
                        <!-- Lịch sử tiêm chủng - Thông tin chi tiết -->
                        <div class="p-3 bg-light rounded-3">
                          <div class="row g-3">
                            <div class="col-sm-6 col-md-4">
                              <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Ngày tiêm</div>
                              <div class="fw-medium text-dark">{{ formatDate(vac.injectionDate || vac.administeredAt || vac.createdAt) }}</div>
                            </div>
                            <div class="col-sm-6 col-md-4">
                              <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Bác sĩ phụ trách</div>
                              <div class="fw-medium text-dark">{{ vac.doctorName || 'Bs. Thú y' }}</div>
                            </div>
                            <div class="col-sm-6 col-md-4" v-if="vac.batchNumber">
                              <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Số lô</div>
                              <div class="fw-medium text-dark">{{ vac.batchNumber }}</div>
                            </div>
                            <div class="col-sm-6 col-md-4" v-if="vac.route">
                              <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Đường tiêm</div>
                              <div class="fw-medium text-dark">{{ vac.route }}</div>
                            </div>
                            <div class="col-sm-6 col-md-4" v-if="vac.injectionSite">
                              <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Vị trí tiêm</div>
                              <div class="fw-medium text-dark">{{ vac.injectionSite }}</div>
                            </div>
                            <div class="col-sm-6 col-md-4" v-if="vac.nextDueDate">
                              <div class="small text-muted fw-bold text-uppercase mb-1" style="font-size: 0.7rem;">Hẹn tái chủng</div>
                              <div class="fw-medium text-primary">{{ formatDate(vac.nextDueDate) }}</div>
                            </div>
                          </div>
                        </div>

                        <div class="mt-3 p-3 rounded-3" style="background-color: #fffbeb;" v-if="vac.clinicalAssessment || vac.doctorRemarks || vac.notes || vac.reactionNote">
                           <div class="row g-2">
                             <div class="col-12" v-if="vac.clinicalAssessment">
                               <span class="small fw-bold text-warning me-2">Kết luận lâm sàng:</span>
                               <span class="small text-dark">{{ vac.clinicalAssessment }}</span>
                             </div>
                             <div class="col-12" v-if="vac.doctorRemarks || vac.notes">
                               <span class="small fw-bold text-warning me-2">Ghi chú BS:</span>
                               <span class="small text-dark">{{ vac.doctorRemarks || vac.notes }}</span>
                             </div>
                             <div class="col-12" v-if="vac.reactionNote">
                               <span class="small fw-bold text-danger me-2">Lưu ý phản ứng:</span>
                               <span class="small text-dark">{{ vac.reactionNote }}</span>
                             </div>
                           </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  
                  <!-- Pagination controls -->
                  <div class="d-flex justify-content-center align-items-center mt-3 gap-2" v-if="totalVaccineHistoryPages > 0">
                    <button class="btn btn-outline-primary btn-sm rounded-circle" style="width: 32px; height: 32px; padding: 0; display: flex; align-items: center; justify-content: center;" :disabled="vaccineHistoryPage === 1" @click="vaccineHistoryPage--">
                      <i class="bi bi-chevron-left"></i>
                    </button>
                    <span class="text-muted small fw-bold mx-2">Trang {{ vaccineHistoryPage }} / {{ totalVaccineHistoryPages }}</span>
                    <button class="btn btn-outline-primary btn-sm rounded-circle" style="width: 32px; height: 32px; padding: 0; display: flex; align-items: center; justify-content: center;" :disabled="vaccineHistoryPage === totalVaccineHistoryPages" @click="vaccineHistoryPage++">
                      <i class="bi bi-chevron-right"></i>
                    </button>
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
                          <h5 class="fw-bold text-dark mb-0">{{ formatDiagnosisTitle(rx.diagnosis) }}</h5>
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


        </div> <!-- End Tab Content -->
      </div> <!-- End Dashboard Container -->

      <!-- Medical Record Detail Modal -->
      <div v-if="showMedicalRecordModal" class="custom-modal-overlay" @click.self="closeMedicalRecordModal">
        <div class="custom-modal-card max-w-800">
          <div class="custom-modal-header bg-light">
            <h5 class="modal-title fw-bold text-dark d-flex align-items-center gap-2">
              <i class="bi bi-file-medical text-primary"></i> Chi tiết bệnh án
            </h5>
            <button type="button" class="modal-close" @click="closeMedicalRecordModal">
              <i class="bi bi-x-lg"></i>
            </button>
          </div>
          <div class="custom-modal-body p-4" v-if="selectedMedicalRecord">
            <div class="d-flex justify-content-between align-items-center mb-4 pb-3 border-bottom">
              <div>
                <h4 class="fw-bold text-dark mb-1">{{ selectedMedicalRecord.serviceName || 'Khám tổng quát' }}</h4>
                <div class="text-muted small"><i class="bi bi-calendar-check me-1"></i> {{ formatDate(selectedMedicalRecord.visitDate || selectedMedicalRecord.createdAt) }}</div>
              </div>
              <div class="text-end">
                <div class="fw-bold text-dark"><i class="bi bi-person-badge text-primary me-1"></i> Bs. {{ selectedMedicalRecord.doctorName }}</div>
                <span class="badge bg-success-subtle text-success mt-1" v-if="!selectedMedicalRecord.followUpDate">Đã hoàn thành</span>
                <span class="badge bg-warning-subtle text-warning mt-1" v-else>Hẹn tái khám: {{ formatDateShort(selectedMedicalRecord.followUpDate) }}</span>
              </div>
            </div>

            <div class="row g-4">
              <!-- Cột S O -->
              <div class="col-md-6">
                <div class="bg-light p-3 rounded-3 h-100">
                  <h6 class="fw-bold text-primary mb-3"><i class="bi bi-chat-left-text me-2"></i>S.O (Chủ quan & Khách quan)</h6>
                  
                  <div class="mb-3">
                    <div class="small fw-bold text-muted mb-1">Bệnh sử & Lý do khám:</div>
                    <div v-if="safeParseJSON(selectedMedicalRecord.medicalHistory)" class="mt-2 p-2 bg-info bg-opacity-10 rounded-3 small">
                      <div class="row g-2">
                        <div class="col-12" v-if="safeParseJSON(selectedMedicalRecord.medicalHistory).chiefComplaint"><span class="text-muted fw-semibold">Lý do khám:</span> {{safeParseJSON(selectedMedicalRecord.medicalHistory).chiefComplaint}}</div>
                        <div class="col-6" v-if="safeParseJSON(selectedMedicalRecord.medicalHistory).appetite"><span class="text-muted">Ăn uống:</span> {{safeParseJSON(selectedMedicalRecord.medicalHistory).appetite}}</div>
                        <div class="col-6" v-if="safeParseJSON(selectedMedicalRecord.medicalHistory).urinationIssues"><span class="text-muted">Tiêu tiểu:</span> {{safeParseJSON(selectedMedicalRecord.medicalHistory).urinationIssues}}</div>
                        <div class="col-6" v-if="safeParseJSON(selectedMedicalRecord.medicalHistory).activityLevel"><span class="text-muted">Hoạt động:</span> {{safeParseJSON(selectedMedicalRecord.medicalHistory).activityLevel}}</div>
                      </div>
                    </div>
                    <div v-else class="text-dark">{{ selectedMedicalRecord.medicalHistory || 'Không ghi nhận' }}</div>
                  </div>
                  
                  <div>
                    <div class="small fw-bold text-muted mb-1">Dấu hiệu lâm sàng:</div>
                    <div class="d-flex gap-2 mb-2">
                      <span class="badge bg-white text-dark border">Nhiệt độ: {{ selectedMedicalRecord.temperature ? selectedMedicalRecord.temperature + '°C' : '--' }}</span>
                      <span class="badge bg-white text-dark border">Cân nặng: {{ selectedMedicalRecord.weight ? selectedMedicalRecord.weight + ' kg' : '--' }}</span>
                    </div>
                    <div v-if="safeParseJSON(selectedMedicalRecord.clinicalSigns)" class="mt-2 bg-warning bg-opacity-10 p-2 rounded-3 small">
                      <div class="row g-2">
                        <div class="col-6" v-if="safeParseJSON(selectedMedicalRecord.clinicalSigns).heartRate"><span class="text-muted">Nhịp tim:</span> {{safeParseJSON(selectedMedicalRecord.clinicalSigns).heartRate}} bpm</div>
                        <div class="col-6" v-if="safeParseJSON(selectedMedicalRecord.clinicalSigns).respiratoryRate"><span class="text-muted">Nhịp thở:</span> {{safeParseJSON(selectedMedicalRecord.clinicalSigns).respiratoryRate}} l/p</div>
                        <div class="col-6" v-if="safeParseJSON(selectedMedicalRecord.clinicalSigns).mentation"><span class="text-muted">Tinh thần:</span> {{safeParseJSON(selectedMedicalRecord.clinicalSigns).mentation}}</div>
                        <div class="col-6" v-if="safeParseJSON(selectedMedicalRecord.clinicalSigns).hydration"><span class="text-muted">Mất nước:</span> {{safeParseJSON(selectedMedicalRecord.clinicalSigns).hydration}}</div>
                        <div class="col-6" v-if="safeParseJSON(selectedMedicalRecord.clinicalSigns).bodyConditionScore"><span class="text-muted">BCS:</span> {{safeParseJSON(selectedMedicalRecord.clinicalSigns).bodyConditionScore}}/9</div>
                      </div>
                    </div>
                    <div v-else class="text-dark small">{{ selectedMedicalRecord.clinicalSigns || 'Bình thường' }}</div>
                  </div>
                </div>
              </div>

              <!-- Cột A P -->
              <div class="col-md-6">
                <div class="bg-light p-3 rounded-3 h-100">
                  <h6 class="fw-bold text-warning mb-3"><i class="bi bi-clipboard2-pulse me-2"></i>A.P (Đánh giá & Kế hoạch)</h6>
                  
                  <div class="mb-3">
                    <div class="small fw-bold text-muted mb-1">Chẩn đoán:</div>
                    <div v-if="safeParseJSON(selectedMedicalRecord.diagnosis)" class="mt-2 p-2 bg-danger bg-opacity-10 rounded-3 small">
                      <div v-if="safeParseJSON(selectedMedicalRecord.diagnosis).tentativeDiagnosis" class="mb-1"><span class="text-muted fw-semibold">CĐ sơ bộ:</span> {{safeParseJSON(selectedMedicalRecord.diagnosis).tentativeDiagnosis}}</div>
                      <div v-if="safeParseJSON(selectedMedicalRecord.diagnosis).definitiveDiagnosis" class="mb-1"><span class="text-muted fw-semibold">CĐ xác định:</span> <span class="fw-bold text-danger">{{safeParseJSON(selectedMedicalRecord.diagnosis).definitiveDiagnosis}}</span></div>
                      <div v-if="safeParseJSON(selectedMedicalRecord.diagnosis).differentialDiagnosis" class="mb-1"><span class="text-muted fw-semibold">CĐ phân biệt:</span> {{safeParseJSON(selectedMedicalRecord.diagnosis).differentialDiagnosis}}</div>
                      <div class="d-flex gap-3 mt-2">
                        <span v-if="safeParseJSON(selectedMedicalRecord.diagnosis).diseaseSeverity" class="badge bg-white text-dark border">Mức độ: {{safeParseJSON(selectedMedicalRecord.diagnosis).diseaseSeverity}}</span>
                        <span v-if="safeParseJSON(selectedMedicalRecord.diagnosis).prognosis" class="badge bg-white text-dark border">Tiên lượng: {{safeParseJSON(selectedMedicalRecord.diagnosis).prognosis}}</span>
                      </div>
                    </div>
                    <div v-else class="text-dark fw-bold">{{ selectedMedicalRecord.diagnosis || 'Chưa có chẩn đoán' }}</div>
                  </div>
                  
                  <div class="mb-3">
                    <div class="small fw-bold text-muted mb-1">Kế hoạch điều trị:</div>
                    <ul v-if="Array.isArray(safeParseJSON(selectedMedicalRecord.treatmentPlan)) && safeParseJSON(selectedMedicalRecord.treatmentPlan).length > 0" class="mt-2 mb-0 ps-3 bg-success bg-opacity-10 p-2 rounded-3">
                      <li v-for="(step, idx) in safeParseJSON(selectedMedicalRecord.treatmentPlan)" :key="idx" class="text-dark small mb-1">{{ step }}</li>
                    </ul>
                    <div v-else-if="!safeParseJSON(selectedMedicalRecord.treatmentPlan)" class="text-dark small">{{ selectedMedicalRecord.treatmentPlan || 'Theo dõi thêm' }}</div>
                    <div v-else class="text-dark small">Chưa ghi nhận</div>
                  </div>
                  
                  <div v-if="selectedMedicalRecord.doctorNotes || selectedMedicalRecord.notes" class="p-2 bg-white rounded border-start border-warning border-3">
                    <div class="small fw-bold text-muted mb-1">Ghi chú thêm:</div>
                    <div class="text-dark small fst-italic">{{ selectedMedicalRecord.doctorNotes || selectedMedicalRecord.notes }}</div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Đơn thuốc -->
            <div class="mt-4" v-if="selectedMedicalRecord.prescribedMedicines && selectedMedicalRecord.prescribedMedicines.length > 0">
              <h6 class="fw-bold text-success mb-3"><i class="bi bi-capsule-pill me-2"></i>Đơn thuốc đã kê</h6>
              <div class="table-responsive border rounded-3">
                <table class="table table-hover mb-0 align-middle">
                  <thead class="table-light">
                    <tr>
                      <th>Tên thuốc</th>
                      <th>Liều lượng</th>
                      <th>SL</th>
                      <th>Cách dùng</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(med, idx) in selectedMedicalRecord.prescribedMedicines" :key="idx">
                      <td class="fw-bold text-dark">{{ med.medicineName }}</td>
                      <td>{{ med.dosage }}</td>
                      <td class="fw-bold">{{ med.quantity }}</td>
                      <td class="small">{{ med.frequency }} <span v-if="med.instruction" class="fst-italic text-muted d-block">{{ med.instruction }}</span></td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
            
            <div v-else class="mt-4 p-3 bg-light rounded-3 text-center text-muted small">
              Không có đơn thuốc nào được kê trong lần khám này.
            </div>

          </div>
          <div class="custom-modal-footer bg-light">
            <button type="button" class="btn btn-secondary px-4 fw-bold rounded-pill" @click="closeMedicalRecordModal">Đóng</button>
          </div>
        </div>
      </div>
    </template>

    <!-- QR Code Modal (teleport → body, fixed full-screen) -->
    <teleport to="body">
      <transition name="qr-modal">
        <div v-if="isQrModalOpen" class="qr-modal-backdrop" @click.self="closeQrModal">
          <div class="qr-modal-card">
            <div class="qr-modal-header">
              <div class="d-flex align-items-center gap-3">
                <div class="qr-modal-icon-wrap"><i class="bi bi-qr-code text-primary fs-5"></i></div>
                <div>
                  <div class="fw-bold text-dark" style="font-size:1rem;">Mã Check-in tại quầy</div>
                  <div class="small text-muted">{{ selectedApptForQr?.serviceName }}</div>
                </div>
              </div>
              <button class="qr-modal-close" @click="closeQrModal"><i class="bi bi-x-lg"></i></button>
            </div>

            <div class="qr-modal-body">
              <p class="text-muted small text-center mb-4">Đưa mã này cho lễ tân tại phòng khám để check-in nhanh chóng</p>
              <div class="qr-canvas-wrap" id="qr-code-container">
                <qrcode-vue :value="qrTokenToDisplay" :size="220" level="H" foreground="#0f172a" background="#ffffff" />
              </div>
              <div class="qr-code-number">{{ qrTokenToDisplay.substring(0,6).toUpperCase() }}</div>
              <div class="qr-code-date">{{ selectedApptForQr ? formatDate(selectedApptForQr.appointmentDate) + ' lúc ' + formatTimeOnly(selectedApptForQr.appointmentDate) : '' }}</div>
            </div>

            <div class="qr-modal-footer">
              <button class="qr-btn-secondary" @click="closeQrModal">Đóng</button>
              <button class="qr-btn-primary" @click="downloadQR">
                <i class="bi bi-download me-2"></i>Tải ảnh QR
              </button>
            </div>
          </div>
        </div>
      </transition>
    </teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick, shallowRef, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import Chart from 'chart.js/auto';
import QrcodeVue from 'qrcode.vue';
import Swal from 'sweetalert2';
import api from '../services/api';

const props = defineProps<{ petId?: string | number }>();
const emit = defineEmits(['go-back']);

const route = useRoute();
const router = useRouter();
const petId = computed(() => props.petId?.toString() || route.params.id as string);

const safeParseJSON = (jsonStr: string | undefined | null): any => {
  if (!jsonStr) return null;
  try {
    return JSON.parse(jsonStr);
  } catch (e) {
    return null;
  }
};

const formatDiagnosisTitle = (diagnosisStr: string | undefined | null): string => {
  if (!diagnosisStr) return 'Đơn thuốc';
  if (diagnosisStr.trim().startsWith('{')) {
    const parsed = safeParseJSON(diagnosisStr);
    if (parsed) {
      if (parsed.definitiveDiagnosis) return parsed.definitiveDiagnosis;
      if (parsed.tentativeDiagnosis) return `Sơ bộ: ${parsed.tentativeDiagnosis}`;
    }
  }
  return diagnosisStr;
};

const goBack = () => {
  if (props.petId) {
    emit('go-back');
  } else {
    router.push('/dashboard');
  }
};

const backendUrl = import.meta.env.VITE_API_URL || 'http://localhost:5150';

// ===== State =====
const pet = ref<any>(null);
const loading = ref(true);
const error = ref('');

const ownerProfile = ref<any>(null);
const medicalRecords = ref<any[]>([]);
const vaccinations = ref<any[]>([]);
const appointments = ref<any[]>([]);

// QR Code Modal State
const isQrModalOpen = ref(false);
const qrTokenToDisplay = ref('');
const selectedApptForQr = ref<any>(null);

const openQrModal = (appt: any) => {
  if (!appt.qrToken) return;
  selectedApptForQr.value = appt;
  qrTokenToDisplay.value = appt.qrToken;
  isQrModalOpen.value = true;
};

const closeQrModal = () => {
  isQrModalOpen.value = false;
  selectedApptForQr.value = null;
  qrTokenToDisplay.value = '';
};

const downloadQR = () => {
  const canvas = document.querySelector('#qr-code-container canvas') as HTMLCanvasElement;
  if (canvas) {
    const url = canvas.toDataURL('image/png');
    const link = document.createElement('a');
    link.download = `QR_Checkin_${qrTokenToDisplay.value.substring(0,6)}.png`;
    link.href = url;
    link.click();
  }
};

const confirmCancelAppointment = async (apptId: string | number) => {
  const result = await Swal.fire({
    title: 'Hủy lịch hẹn?',
    text: "Bạn có chắc chắn muốn hủy lịch hẹn này không?",
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#ef4444',
    cancelButtonColor: '#64748b',
    confirmButtonText: 'Đồng ý hủy',
    cancelButtonText: 'Không'
  });

  if (result.isConfirmed) {
    try {
      await api.delete(`/appointments/${apptId}`);
      Swal.fire('Thành công', 'Đã hủy lịch hẹn.', 'success');
      fetchAppointments();
    } catch (err: any) {
      Swal.fire('Lỗi', err.response?.data?.message || 'Không thể hủy lịch hẹn.', 'error');
    }
  }
};

const prescriptions = ref<any[]>([]);

const historyFilter = ref<string>('all');
const isHistoryFilterOpen = ref<boolean>(false);
const historyFilterLabel = computed(() => {
  switch (historyFilter.value) {
    case 'followUp': return 'Cần tái khám';
    case 'completed': return 'Đã hoàn thành';
    default: return 'Tất cả';
  }
});
const filteredMedicalRecords = computed(() => {
  if (historyFilter.value === 'followUp') {
    return medicalRecords.value.filter(r => r.followUpDate);
  }
  if (historyFilter.value === 'completed') {
    return medicalRecords.value.filter(r => !r.followUpDate);
  }
  return medicalRecords.value;
});

// Pagination for Medical History
const medicalHistoryPage = ref(1);
const medicalHistoryPerPage = ref(5);

watch(historyFilter, () => {
  medicalHistoryPage.value = 1; // Reset to page 1 when filter changes
});

const paginatedMedicalRecords = computed(() => {
  const start = (medicalHistoryPage.value - 1) * medicalHistoryPerPage.value;
  const end = start + medicalHistoryPerPage.value;
  return filteredMedicalRecords.value.slice(start, end);
});
const totalMedicalHistoryPages = computed(() => {
  return Math.ceil(filteredMedicalRecords.value.length / medicalHistoryPerPage.value) || 1;
});

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
  { key: 'vaccines', label: 'Vaccine', icon: 'bi bi-bandaid' },
  { key: 'prescriptions', label: 'Đơn thuốc', icon: 'bi bi-capsule' }
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

const vaccineFilter = ref<string>('all');
const isVaccineFilterOpen = ref<boolean>(false);
const vaccineFilterLabel = computed(() => {
  switch (vaccineFilter.value) {
    case 'hasDue': return 'Có hẹn tái chủng';
    default: return 'Tất cả';
  }
});

const historyVaccines = computed(() => {
  let filtered = [...vaccinations.value];
  if (vaccineFilter.value === 'hasDue') {
    filtered = filtered.filter(v => v.nextDueDate);
  }
  return filtered.sort((a: any, b: any) => new Date(b.administeredAt || b.injectionDate || b.createdAt).getTime() - new Date(a.administeredAt || a.injectionDate || a.createdAt).getTime());
});

// Pagination for Vaccines
const vaccineHistoryPage = ref(1);
const vaccineHistoryPerPage = ref(5);

watch(vaccineFilter, () => {
  vaccineHistoryPage.value = 1;
});
const paginatedHistoryVaccines = computed(() => {
  const start = (vaccineHistoryPage.value - 1) * vaccineHistoryPerPage.value;
  const end = start + vaccineHistoryPerPage.value;
  return historyVaccines.value.slice(start, end);
});
const totalVaccineHistoryPages = computed(() => {
  return Math.ceil(historyVaccines.value.length / vaccineHistoryPerPage.value) || 1;
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

  const recordsWithWeight = medicalRecords.value.filter(r => r.weight);
  recordsWithWeight.sort((a, b) => new Date(a.visitDate || a.createdAt).getTime() - new Date(b.visitDate || b.createdAt).getTime());

  const labels = recordsWithWeight.map(r => formatDateShort(r.visitDate || r.createdAt));
  const data = recordsWithWeight.map(r => r.weight);

  const ctx = chartCanvas.value.getContext('2d');
  if (!ctx) return;

  // Create gradient
  const gradient = ctx.createLinearGradient(0, 0, 0, 400);
  gradient.addColorStop(0, 'rgba(245, 158, 11, 0.4)'); // Vàng nhạt từ --primary-gold
  gradient.addColorStop(1, 'rgba(245, 158, 11, 0.0)');

  chartInstance.value = new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{
        label: 'Cân nặng (kg)',
        data,
        borderColor: '#f59e0b', 
        backgroundColor: gradient,
        borderWidth: 3,
        tension: 0.4,
        fill: true,
        pointBackgroundColor: '#ffffff',
        pointBorderColor: '#d97706',
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
const selectedMedicalRecord = ref<any>(null);
const showMedicalRecordModal = ref(false);

const openMedicalRecordModal = (record: any) => {
  selectedMedicalRecord.value = record;
  showMedicalRecordModal.value = true;
};

const closeMedicalRecordModal = () => {
  showMedicalRecordModal.value = false;
  // Giữ lại selectedMedicalRecord để animation đóng modal mượt hơn, hoặc xoá null.
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

const getDaysUntil = (dateStr: string | null) => {
  if (!dateStr) return '';
  const apptDate = new Date(dateStr);
  if (apptDate.getFullYear() < 2000) return 'Không xác định';

  const now = new Date();
  apptDate.setHours(0, 0, 0, 0);
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());

  const diffTime = apptDate.getTime() - today.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

  if (diffDays < 0) return 'Đã qua';
  if (diffDays === 0) return 'Hôm nay';
  if (diffDays === 1) return 'Ngày mai';
  return `Còn ${diffDays} ngày nữa`;
};

const formatDate = (dateStr: string | null | undefined): string => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  if (d.getFullYear() < 2000) return '—';
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  return `${day}/${month}/${d.getFullYear()}`;
};

const formatDateShort = (dateStr: string | null | undefined): string => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  if (d.getFullYear() < 2000) return '—';
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  return `${day}/${month}`;
};

const formatDateFull = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  if (d.getFullYear() < 2000) return '—';
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  return `${day}/${month}/${d.getFullYear()}`;
};

const formatDateTime = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  if (d.getFullYear() < 2000) return '—';
  return d.toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

const formatDateTimeFull = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  if (d.getFullYear() < 2000) return '—';
  return d.toLocaleString('vi-VN', {
    weekday: 'short', day: '2-digit', month: '2-digit', year: 'numeric'
  });
};

const formatTimeOnly = (dateStr: string | null): string => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  if (d.getFullYear() < 2000) return '—';
  if (d.getHours() === 0 && d.getMinutes() === 0) return 'Chưa có giờ';
  return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
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
watch(() => petId.value, (newId) => {
  if (newId) {
    fetchAll();
  }
});

onMounted(() => {
  if (petId.value) {
    fetchAll();
  }
});
</script>

<style scoped>
/* Google Font applied if not globally */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.pet-profile-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #f0f4f8 0%, #e0eaf5 100%);
  font-family: 'Inter', sans-serif;
  padding-bottom: 3rem;
}

/* TOPBAR */
.profile-topbar {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem 2rem 0.5rem;
  max-width: 1280px;
  margin: 0 auto;
  background: transparent;
  border-bottom: none;
  position: relative;
  z-index: 100;
  box-shadow: none;
  backdrop-filter: none;
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
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  padding: 1.75rem;
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.05);
  display: flex;
  align-items: center;
  gap: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.8);
  flex: 1;
  min-width: 320px;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.avatar-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(31, 38, 135, 0.08);
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
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  padding: 1.5rem;
  flex: 1;
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.8);
  display: flex;
  flex-direction: column;
  justify-content: center;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.info-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(31, 38, 135, 0.08);
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
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  padding: 1.5rem;
  width: 280px;
  box-shadow: 0 8px 32px rgba(245, 158, 11, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.8);
  border-top: 4px solid #f59e0b;
  display: flex;
  flex-direction: column;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.next-appt-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(245, 158, 11, 0.12);
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
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  border: 1px solid rgba(255, 255, 255, 0.8);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.05);
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

/* Ticket Style */
.appointment-hero-ticket .ticket-cut {
  position: absolute;
  width: 30px;
  height: 30px;
  background: #f8fafc;
  border-radius: 50%;
  left: -15px;
  z-index: 10;
}
.appointment-hero-ticket .ticket-cut.top {
  top: -15px;
  box-shadow: inset -3px -3px 5px rgba(0,0,0,0.05);
}
.appointment-hero-ticket .ticket-cut.bottom {
  bottom: -15px;
  box-shadow: inset -3px 3px 5px rgba(0,0,0,0.05);
}
@media (max-width: 768px) {
  .appointment-hero-ticket .ticket-stub {
    border-left: none !important;
    border-top: 2px dashed rgba(255,255,255,0.3);
  }
  .appointment-hero-ticket .ticket-cut {
    left: 50%;
    transform: translateX(-50%);
  }
  .appointment-hero-ticket .ticket-cut.top {
    top: -15px;
    left: 50%;
  }
  .appointment-hero-ticket .ticket-cut.bottom {
    display: none;
  }
}

/* Custom Modal Styles */
.custom-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(4px);
  z-index: 1050;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: fadeIn 0.3s ease;
}

.custom-modal-card {
  background: #ffffff;
  border-radius: 20px;
  width: 90%;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: slideUp 0.3s ease;
  overflow: hidden;
}

.max-w-800 {
  max-width: 800px;
}

.custom-modal-header {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-close {
  background: none;
  border: none;
  color: #64748b;
  font-size: 1.25rem;
  cursor: pointer;
  transition: color 0.2s;
  padding: 0.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-close:hover {
  color: #0f172a;
}

.custom-modal-body {
  padding: 1.5rem;
  overflow-y: auto;
  flex-grow: 1;
}

.custom-modal-footer {
  padding: 1.25rem 1.5rem;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: flex-end;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes pulse {
  0% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.1); opacity: 0.7; }
  100% { transform: scale(1); opacity: 1; }
}
/* =========================================================
   APPOINTMENT TAB UI STYLES (PREMIUM GLASSMORPHISM)
   ========================================================= */
.appt-layout {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

/* --- Hero Card --- */
.appt-hero-card {
  position: relative;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 24px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  display: flex;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.appt-hero-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 50px rgba(0, 0, 0, 0.08);
}

.appt-hero-accent {
  position: absolute;
  top: 0;
  left: 0;
  bottom: 0;
  width: 8px;
  background: linear-gradient(180deg, var(--primary-gold) 0%, #f59e0b 100%);
}

.appt-hero-body {
  display: flex;
  flex: 1;
  padding: 0;
}

.appt-hero-left {
  flex: 1;
  padding: 2.5rem;
  display: flex;
  flex-direction: column;
}

.appt-hero-right {
  width: 280px;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border-left: 1px solid rgba(0,0,0,0.05);
  padding: 2.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
}

/* Badges */
.appt-countdown-badge {
  display: inline-flex;
  align-items: center;
  background: rgba(245, 158, 11, 0.1);
  color: #d97706;
  font-weight: 700;
  font-size: 0.85rem;
  padding: 0.5rem 1rem;
  border-radius: 50px;
  border: 1px solid rgba(245, 158, 11, 0.2);
}

.appt-status-badge {
  display: inline-flex;
  align-items: center;
  font-weight: 700;
  font-size: 0.75rem;
  padding: 0.4rem 0.8rem;
  border-radius: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.appt-status-badge.status-pending { background: #fffbeb; color: #d97706; border: 1px solid #fef3c7; }
.appt-status-badge.status-confirmed { background: #eff6ff; color: #2563eb; border: 1px solid #dbeafe; }
.appt-status-badge.status-completed { background: #f0fdf4; color: #16a34a; border: 1px solid #dcfce7; }
.appt-status-badge.status-cancelled { background: #fef2f2; color: #dc2626; border: 1px solid #fee2e2; }

/* Text Elements */
.appt-hero-title {
  font-size: 2rem;
  font-weight: 800;
  color: var(--text-dark);
  margin-bottom: 0.5rem;
  letter-spacing: -0.5px;
}

.appt-hero-doctor {
  display: flex;
  align-items: center;
  font-size: 1.1rem;
  color: #475569;
  margin-bottom: 2rem;
}

.appt-doctor-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: #e2e8f0;
  color: #64748b;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 0.75rem;
}

/* Date & Time Chips */
.appt-hero-datetime {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.appt-datetime-chip {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 1.5rem;
  border-radius: 16px;
  flex: 1;
  min-width: 200px;
}
.appt-chip-blue {
  background: #f0f9ff;
  border: 1px solid #e0f2fe;
}
.appt-chip-amber {
  background: #fffbeb;
  border: 1px solid #fef3c7;
}

.appt-chip-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  background: #bae6fd;
  color: #0369a1;
}
.appt-chip-icon-amber {
  background: #fde68a;
  color: #b45309;
}

.appt-chip-label {
  font-size: 0.75rem;
  font-weight: 700;
  color: #64748b;
  letter-spacing: 1px;
  margin-bottom: 0.25rem;
}

.appt-chip-value {
  font-size: 1.1rem;
  font-weight: 800;
  color: var(--text-dark);
}

.appt-hero-note {
  display: flex;
  align-items: flex-start;
  font-size: 0.95rem;
  color: #64748b;
  background: #f8fafc;
  padding: 1rem;
  border-radius: 12px;
  line-height: 1.5;
}

/* Right Panel (QR & Actions) */
.appt-qr-preview {
  width: 80px;
  height: 80px;
  background: white;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
  color: var(--primary-gold);
  box-shadow: 0 4px 15px rgba(0,0,0,0.05);
  margin-bottom: 1rem;
}

.appt-qr-code-text {
  font-family: monospace;
  font-size: 1.5rem;
  font-weight: 800;
  letter-spacing: 2px;
  color: var(--text-dark);
}

.appt-qr-sublabel {
  font-size: 0.75rem;
  font-weight: 700;
  color: #94a3b8;
  letter-spacing: 1px;
  margin-bottom: 2rem;
}

.appt-btn-primary {
  width: 100%;
  padding: 0.8rem;
  background: linear-gradient(135deg, var(--primary-gold) 0%, #f59e0b 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1rem;
  margin-bottom: 0.75rem;
  cursor: pointer;
  box-shadow: 0 4px 15px rgba(245, 158, 11, 0.3);
  transition: all 0.2s ease;
}
.appt-btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.4);
}

.appt-btn-danger {
  width: 100%;
  padding: 0.8rem;
  background: white;
  color: #ef4444;
  border: 1px solid #fca5a5;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.2s ease;
}
.appt-btn-danger:hover {
  background: #fef2f2;
}


/* --- Grid Section --- */
.appt-grid-section {
  margin-top: 1rem;
}

.appt-section-title {
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--text-dark);
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
}

.appt-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1.5rem;
}

.appt-grid-card {
  position: relative;
  background: rgba(255,255,255,0.7);
  border: 1px solid rgba(0,0,0,0.05);
  border-radius: 20px;
  overflow: visible; /* to allow dropdowns */
  display: flex;
  flex-direction: column;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}
.appt-grid-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 25px rgba(0,0,0,0.05);
  background: rgba(255,255,255,0.95);
}

.appt-card-strip {
  height: 6px;
  width: 100%;
  border-top-left-radius: 20px;
  border-top-right-radius: 20px;
}
.appt-card-strip.strip-pending { background: #fcd34d; }
.appt-card-strip.strip-confirmed { background: #60a5fa; }
.appt-card-strip.strip-completed { background: #4ade80; }
.appt-card-strip.strip-cancelled { background: #f87171; }

.appt-card-body {
  padding: 1.5rem;
  flex: 1;
}

.appt-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.appt-card-status-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.3rem 0.6rem;
  border-radius: 6px;
  text-transform: uppercase;
}
.appt-card-status-badge.status-pending { background: #fffbeb; color: #d97706; }
.appt-card-status-badge.status-confirmed { background: #eff6ff; color: #2563eb; }
.appt-card-status-badge.status-completed { background: #f0fdf4; color: #16a34a; }
.appt-card-status-badge.status-cancelled { background: #fef2f2; color: #dc2626; }

.appt-card-menu {
  position: relative;
}
.appt-menu-trigger {
  background: none;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  padding: 0.25rem 0.5rem;
  border-radius: 8px;
  transition: background 0.2s;
}
.appt-menu-trigger:hover { background: #f1f5f9; color: #475569; }

.appt-menu-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0,0,0,0.1);
  border: 1px solid rgba(0,0,0,0.05);
  padding: 0.5rem;
  min-width: 150px;
  z-index: 10;
}
.appt-menu-item {
  width: 100%;
  text-align: left;
  background: none;
  border: none;
  padding: 0.5rem 1rem;
  font-size: 0.9rem;
  font-weight: 600;
  color: #475569;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  display: flex;
  align-items: center;
}
.appt-menu-item:hover { background: #f8fafc; color: var(--text-dark); }
.appt-menu-item-danger { color: #ef4444; }
.appt-menu-item-danger:hover { background: #fef2f2; color: #dc2626; }

.appt-card-title {
  font-size: 1.1rem;
  font-weight: 800;
  color: var(--text-dark);
  margin-bottom: 1rem;
}

.appt-card-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.85rem;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 0.75rem;
}
.appt-card-meta i { color: #cbd5e1; margin-right: 0.4rem; }

.appt-card-doctor {
  font-size: 0.9rem;
  color: #475569;
  display: flex;
  align-items: center;
}
.appt-card-doctor i { margin-right: 0.4rem; color: #94a3b8; }

.appt-card-footer {
  padding: 1rem 1.5rem;
  background: #f8fafc;
  border-top: 1px solid rgba(0,0,0,0.03);
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom-left-radius: 20px;
  border-bottom-right-radius: 20px;
}

.appt-card-code {
  font-family: monospace;
  font-weight: 700;
  font-size: 1.1rem;
  color: var(--text-dark);
}

.appt-card-qr-btn {
  background: white;
  border: 1px solid #e2e8f0;
  color: var(--text-dark);
  font-weight: 600;
  font-size: 0.85rem;
  padding: 0.4rem 0.8rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}
.appt-card-qr-btn:hover {
  border-color: var(--primary-gold);
  color: var(--primary-gold);
  background: #fffbeb;
}

/* Empty State */
.appt-empty-state {
  text-align: center;
  padding: 4rem 2rem;
  background: rgba(255,255,255,0.6);
  border-radius: 24px;
  border: 1px dashed rgba(0,0,0,0.1);
}
.appt-empty-icon {
  font-size: 4rem;
  margin-bottom: 1.5rem;
  opacity: 0.8;
}

/* Responsive adjustments */
@media (max-width: 992px) {
  .appt-hero-body {
    flex-direction: column;
  }
  .appt-hero-right {
    width: 100%;
    border-left: none;
    border-top: 1px solid rgba(0,0,0,0.05);
  }
}
/* =========================================================
   QR MODAL STYLES
   ========================================================= */
.qr-modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}

.qr-modal-card {
  background: #ffffff;
  border-radius: 24px;
  width: 90%;
  max-width: 400px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.qr-modal-header {
  padding: 1.5rem;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.qr-modal-icon-wrap {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: #eff6ff;
  display: flex;
  align-items: center;
  justify-content: center;
}

.qr-modal-close {
  background: transparent;
  border: none;
  font-size: 1.25rem;
  color: #94a3b8;
  cursor: pointer;
  transition: color 0.2s;
  padding: 0.5rem;
}
.qr-modal-close:hover {
  color: #ef4444;
}

.qr-modal-body {
  padding: 2rem 1.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.qr-canvas-wrap {
  background: white;
  padding: 1.5rem;
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
  margin-bottom: 1.5rem;
  border: 1px solid #e2e8f0;
}

.qr-code-number {
  font-family: monospace;
  font-size: 1.75rem;
  font-weight: 800;
  letter-spacing: 4px;
  color: var(--primary-dark, #0f172a);
  margin-bottom: 0.5rem;
}

.qr-code-date {
  font-size: 0.9rem;
  font-weight: 600;
  color: #64748b;
}

.qr-modal-footer {
  padding: 1.5rem;
  background: #f8fafc;
  display: flex;
  gap: 1rem;
}

.qr-btn-secondary,
.qr-btn-primary {
  flex: 1;
  padding: 0.8rem;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.qr-btn-secondary {
  background: white;
  color: #475569;
  border: 1px solid #cbd5e1;
}
.qr-btn-secondary:hover {
  background: #f1f5f9;
}

.qr-btn-primary {
  background: linear-gradient(135deg, var(--primary-gold, #d97706) 0%, #f59e0b 100%);
  color: white;
  border: none;
  box-shadow: 0 4px 15px rgba(245, 158, 11, 0.3);
}
.qr-btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.4);
}

/* Transitions */
.qr-modal-enter-active,
.qr-modal-leave-active {
  transition: opacity 0.3s ease;
}
.qr-modal-enter-from,
.qr-modal-leave-to {
  opacity: 0;
}
.qr-modal-enter-active .qr-modal-card,
.qr-modal-leave-active .qr-modal-card {
  transition: transform 0.3s ease;
}
.qr-modal-enter-from .qr-modal-card,
.qr-modal-leave-to .qr-modal-card {
  transform: scale(0.9) translateY(20px);
}

.glass-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  border: 1px solid rgba(255, 255, 255, 0.8);
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.05);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.hover-glow:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 40px rgba(31, 38, 135, 0.08);
}
</style>
