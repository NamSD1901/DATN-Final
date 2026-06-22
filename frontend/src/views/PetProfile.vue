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
                <div v-else class="timeline-container position-relative ps-4 ms-2 mt-4">
                  <!-- Vertical Line -->
                  <div class="position-absolute h-100" style="left: 0; top: 0; width: 2px; background-color: rgba(245, 158, 11, 0.3);"></div>

                  <div v-for="(rec, index) in filteredMedicalRecords" :key="rec.id" class="position-relative mb-4">
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

                        <!-- SOAP NOTES -->
                        <div class="row g-3 mb-4">
                          <!-- Subjective & Objective -->
                          <div class="col-md-6">
                            <div class="p-3 rounded-3 h-100" style="background-color: rgba(255,255,255,0.5); border: 1px solid var(--border-color);">
                              <h6 class="fw-bold text-dark mb-2" style="font-size: 0.85rem;"><i class="bi bi-chat-left-text me-2 text-primary"></i>Lý do khám (S)</h6>
                              <p class="small text-muted mb-3">{{ rec.medicalHistory || 'Không có ghi nhận' }}</p>

                              <h6 class="fw-bold text-dark mb-2" style="font-size: 0.85rem;"><i class="bi bi-activity me-2 text-info"></i>Dấu hiệu lâm sàng (O)</h6>
                              <div class="d-flex gap-2 mb-2 flex-wrap">
                                <span class="badge bg-white text-dark border shadow-sm">Nhiệt độ: {{ rec.temperature ? rec.temperature + '°C' : '—' }}</span>
                                <span class="badge bg-white text-dark border shadow-sm">Cân nặng: {{ rec.weight ? rec.weight + ' kg' : '—' }}</span>
                              </div>
                              <p class="small text-muted mb-0">{{ rec.clinicalSigns || 'Bình thường' }}</p>
                            </div>
                          </div>

                          <!-- Assessment & Plan -->
                          <div class="col-md-6">
                            <div class="p-3 rounded-3 h-100" style="background-color: rgba(255,255,255,0.5); border: 1px solid var(--border-color);">
                              <h6 class="fw-bold text-dark mb-2" style="font-size: 0.85rem;"><i class="bi bi-clipboard2-pulse me-2 text-warning"></i>Chẩn đoán (A)</h6>
                              <p class="small text-dark fw-bold mb-3">{{ rec.diagnosis || 'Chưa chẩn đoán' }}</p>

                              <h6 class="fw-bold text-dark mb-2" style="font-size: 0.85rem;"><i class="bi bi-journal-medical me-2 text-success"></i>Kế hoạch điều trị (P)</h6>
                              <p class="small text-muted mb-2">{{ rec.treatmentPlan || 'Theo dõi thêm' }}</p>
                              
                              <div v-if="rec.doctorNotes || rec.notes" class="mt-2 p-2 rounded" style="background-color: var(--primary-cream); border-left: 3px solid var(--primary-gold);">
                                <span class="small font-italic text-dark"><strong>Ghi chú BS:</strong> "{{ rec.doctorNotes || rec.notes }}"</span>
                              </div>
                            </div>
                          </div>
                        </div>

                        <div class="d-flex gap-2 flex-wrap">
                          <button v-if="rec.followUpDate" class="btn-premium px-4 py-2 hover-arrow" style="font-size: 0.85rem;">
                            Đặt lịch tái khám <i class="bi bi-arrow-right"></i>
                          </button>
                          <button class="btn-premium-outline px-4 py-2" style="font-size: 0.85rem; padding: 0.5rem 1.5rem !important;">
                            <i class="bi bi-eye"></i> Xem chi tiết
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

          <div v-show="activeTab === 'appointments'" class="tab-pane">
            <div class="row justify-content-center">
              <div class="col-lg-10">
                
                <div v-if="upcomingAppointments.length === 0" class="text-center py-5 glass-card border d-flex flex-column align-items-center justify-content-center" style="min-height: 400px;">
                  <img src="https://cdn-icons-png.flaticon.com/512/7486/7486747.png" alt="No appointments" style="width: 120px; opacity: 0.7; margin-bottom: 20px;">
                  <h4 class="fw-bold text-dark mb-3">Lịch trình đang trống</h4>
                  <p class="text-muted mb-4" style="max-width: 400px;">Thú cưng của bạn chưa có lịch hẹn nào sắp tới. Hãy lên lịch kiểm tra sức khỏe định kỳ để đảm bảo bé luôn khỏe mạnh nhé!</p>
                  <button class="btn btn-premium px-4 py-2 fw-bold d-flex align-items-center gap-2">
                    <i class="bi bi-calendar-plus"></i> Tạo lịch hẹn mới ngay
                  </button>
                </div>
                
                <div v-else class="d-flex flex-column gap-5">
                  <!-- HERO TICKET (Next Appointment) -->
                  <div class="appointment-hero-ticket position-relative">
                    <div class="ticket-wrapper d-flex flex-column flex-md-row shadow-lg rounded-4 overflow-hidden" style="background: white; border: 1px solid var(--border-color);">
                      
                      <!-- Left Side: Main Info -->
                      <div class="ticket-main p-4 p-md-5 position-relative flex-grow-1" style="background: linear-gradient(145deg, #ffffff, #f8fafc);">
                        <!-- Decor -->
                        <div class="position-absolute top-0 end-0 p-3 opacity-10">
                          <i class="bi bi-calendar-heart" style="font-size: 8rem; color: var(--primary-gold); margin-top: -30px; margin-right: -20px;"></i>
                        </div>

                        <div class="d-flex justify-content-between align-items-start mb-4 position-relative z-1">
                          <div class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold d-flex align-items-center gap-2 shadow-sm" style="font-size: 0.85rem;">
                            <i class="bi bi-clock-history"></i> {{ getDaysUntil(upcomingAppointments[0].appointmentDate) }}
                          </div>
                          <span class="badge" :class="getStatusClass(upcomingAppointments[0].status)" style="font-size: 0.8rem; padding: 8px 16px;">{{ getStatusLabel(upcomingAppointments[0].status) }}</span>
                        </div>

                        <h2 class="fw-bold text-dark mb-2 position-relative z-1">{{ upcomingAppointments[0].serviceName || 'Lịch khám tổng quát' }}</h2>
                        <div class="d-flex align-items-center gap-2 text-muted fw-medium mb-4 position-relative z-1">
                          <i class="bi bi-person-badge text-primary fs-5"></i>
                          <span style="font-size: 1.1rem;">Bs. <strong class="text-dark">{{ upcomingAppointments[0].doctorName || 'Sẽ phân công sau' }}</strong></span>
                        </div>

                        <div class="row g-3 position-relative z-1">
                          <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-3 p-3 rounded-3" style="background: rgba(2, 132, 199, 0.05); border-left: 4px solid var(--primary-color);">
                              <i class="bi bi-calendar3 fs-3 text-primary"></i>
                              <div>
                                <div class="small text-muted fw-bold">NGÀY KHÁM</div>
                                <div class="fw-bold text-dark" style="font-size: 1.1rem;">{{ formatDate(upcomingAppointments[0].appointmentDate) }}</div>
                              </div>
                            </div>
                          </div>
                          <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-3 p-3 rounded-3" style="background: rgba(245, 158, 11, 0.05); border-left: 4px solid var(--primary-gold);">
                              <i class="bi bi-clock fs-3 text-warning"></i>
                              <div>
                                <div class="small text-muted fw-bold">GIỜ KHÁM</div>
                                <div class="fw-bold text-dark" style="font-size: 1.1rem;">{{ formatTimeOnly(upcomingAppointments[0].appointmentDate) }}</div>
                              </div>
                            </div>
                          </div>
                        </div>

                        <div class="mt-4 p-3 rounded-3 position-relative z-1" style="background: #fffbeb; border: 1px dashed #fcd34d;">
                          <div class="d-flex align-items-center gap-2 mb-2">
                            <i class="bi bi-info-circle-fill text-warning"></i>
                            <span class="fw-bold text-dark small">ĐỂ CHUẨN BỊ TỐT NHẤT:</span>
                          </div>
                          <p class="small text-muted mb-0 fw-medium" style="line-height: 1.6;">
                            {{ upcomingAppointments[0].notes || 'Vui lòng đến sớm 10 phút trước giờ hẹn. Nhớ mang theo sổ khám bệnh (nếu có) nhé!' }}
                          </p>
                        </div>
                      </div>

                      <!-- Right Side: Action & QR -->
                      <div class="ticket-stub d-flex flex-column align-items-center justify-content-center p-4 position-relative" style="background: var(--bs-primary, #0d6efd); min-width: 260px; border-left: 2px dashed rgba(255,255,255,0.3);">
                        <div class="ticket-cut top"></div>
                        <div class="ticket-cut bottom"></div>

                        <div class="text-center mb-4 w-100">
                          <div class="text-white opacity-75 small fw-bold mb-2" style="letter-spacing: 2px;">MÃ CHECK-IN TẠI QUẦY</div>
                          <div class="bg-white p-3 rounded-3 d-inline-block shadow-sm">
                            <i class="bi bi-qr-code text-dark" style="font-size: 4rem; line-height: 1;"></i>
                          </div>
                          <div class="text-white fw-bold mt-2" style="font-size: 1.2rem; letter-spacing: 3px;">
                            {{ upcomingAppointments[0].qrToken ? upcomingAppointments[0].qrToken.substring(0, 6).toUpperCase() : 'PET123' }}
                          </div>
                        </div>

                        <div class="d-flex flex-column gap-2 w-100 mt-auto">
                          <button class="btn btn-light fw-bold text-primary w-100 d-flex align-items-center justify-content-center gap-2">
                            <i class="bi bi-download"></i> Tải mã QR
                          </button>
                          <button class="btn btn-outline-light fw-bold w-100" style="border: 1px solid rgba(255,255,255,0.3);">
                            Hủy lịch hẹn
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- TIMELINE (Other Future Appointments) -->
                  <div class="future-appointments mt-2" v-if="upcomingAppointments.length > 1">
                    <h5 class="fw-bold text-dark mb-4 d-flex align-items-center gap-2">
                      <i class="bi bi-calendar-range text-secondary"></i> Các lịch hẹn tiếp theo
                    </h5>
                    
                    <div class="timeline-container position-relative ps-4 ms-2">
                      <!-- Vertical Line -->
                      <div class="position-absolute h-100" style="left: 0; top: 0; width: 2px; background-color: #e2e8f0;"></div>

                      <div v-for="(appt, index) in upcomingAppointments.slice(1)" :key="appt.id" class="position-relative mb-4">
                        <!-- Dot -->
                        <div class="position-absolute rounded-circle" style="width: 14px; height: 14px; left: -30px; top: 24px; background-color: var(--primary-color); border: 3px solid white; box-shadow: 0 0 0 1px var(--primary-color);"></div>
                        
                        <div class="glass-card p-3 p-md-4 d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-3 hover-glow">
                          <div class="d-flex gap-4 align-items-center">
                            <div class="text-center rounded-3 bg-light border shadow-sm overflow-hidden" style="width: 60px; flex-shrink: 0;">
                              <div class="bg-secondary text-white fw-bold py-1" style="font-size: 0.7rem;">T{{ new Date(appt.appointmentDate).getMonth() + 1 }}</div>
                              <div class="fw-bold text-dark py-2 fs-5" style="line-height: 1;">{{ new Date(appt.appointmentDate).getDate() }}</div>
                            </div>
                            
                            <div>
                              <div class="d-flex align-items-center gap-2 mb-1">
                                <h6 class="fw-bold text-dark mb-0">{{ appt.serviceName }}</h6>
                                <span class="badge rounded-pill bg-light text-secondary border" style="font-size: 0.65rem;">{{ getStatusLabel(appt.status) }}</span>
                              </div>
                              <div class="text-muted small fw-medium d-flex align-items-center gap-3">
                                <span><i class="bi bi-clock text-primary"></i> {{ formatTimeOnly(appt.appointmentDate) }}</span>
                                <span><i class="bi bi-person-badge"></i> {{ appt.doctorName || 'Chưa phân công' }}</span>
                              </div>
                            </div>
                          </div>
                          
                          <div>
                            <button class="btn btn-sm btn-outline-secondary rounded-pill fw-bold px-3">Chi tiết</button>
                          </div>
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

@keyframes pulse {
  0% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.1); opacity: 0.7; }
  100% { transform: scale(1); opacity: 1; }
}
</style>
