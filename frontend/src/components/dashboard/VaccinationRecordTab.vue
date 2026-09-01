<template>
  <div class="medical-records-tab h-100 d-flex flex-column">
    <!-- Inner Tabs Navigation -->
    <ul class="nav nav-pills custom-pills mb-4">
      <li class="nav-item">
        <a class="nav-link fw-bold" :class="{ 'active': internalTab === 'treatment' }" href="#" @click.prevent="internalTab = 'treatment'">
          <i class="bi bi-shield-plus me-1"></i> Bệnh Án Tiêm Chủng
        </a>
      </li>
      <li class="nav-item">
        <a class="nav-link fw-bold" :class="{ 'active': internalTab === 'pet-history' }" href="#" @click.prevent="internalTab = 'pet-history'">
          <i class="bi bi-folder2-open me-1"></i> Lịch Sử Tiêm Phòng
        </a>
      </li>
    </ul>

    <!-- Tab 1: Phiếu Tiêm Chủng SOAP -->
    <div v-if="internalTab === 'treatment'" class="flex-grow-1 overflow-auto pe-2 pb-5">
      <!-- Header -->
      <div class="d-flex flex-column mb-3">
        <h4 class="fw-bold text-dark mb-0">Bệnh Án Tiêm Chủng (S.O.A.P)</h4>
        <span class="text-muted small">Hôm nay: {{ new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: '2-digit', day: '2-digit' }) }}</span>
      </div>

      <div class="card border-0 shadow-sm rounded-4 p-4 bg-glass bg-white border-top border-4 border-success">
        <div class="d-flex justify-content-between align-items-center mb-4 border-bottom pb-3">
          <div>
            <a href="#" class="text-muted text-decoration-none fw-bold" @click.prevent="cancelTreatment">
              <i class="bi bi-arrow-left me-1"></i> Trở về hàng khám
            </a>
            <span class="mx-2 text-muted">/</span>
            <span class="fw-bold text-dark fs-5">{{ activePatient.petName }}</span>
          </div>
          <div class="badge bg-success bg-opacity-10 text-success rounded-pill px-4 py-2 fs-6 shadow-sm border border-success border-opacity-25">
            <i class="bi bi-shield-check me-1"></i> S.O.A.P
          </div>
        </div>

        <div v-if="!activePatient.appointmentId" class="alert alert-info rounded-4 border-0 p-4 text-center">
          <i class="bi bi-exclamation-triangle-fill text-info fs-1 d-block mb-2"></i>
          <h6 class="fw-bold text-dark">Chưa chọn ca khám hoạt động</h6>
        </div>

        <form v-else @submit.prevent="submitForm">
          <!-- Quick Info Row -->
          <div class="row mb-4 bg-light rounded-4 p-3 mx-0">
            <div class="col-md-3 border-end">
              <div class="small text-muted mb-1">Chủ nuôi</div>
              <div class="fw-bold text-dark">{{ activePatient.customerName }}</div>
            </div>
            <div class="col-md-3 border-end">
              <div class="small text-muted mb-1">Thú cưng</div>
              <div class="fw-bold text-dark">{{ activePatient.petName }} <span class="small fw-normal text-muted">(ID: {{ activePatient.petId }})</span></div>
            </div>
            <div class="col-md-3 border-end">
              <div class="small text-muted mb-1">Cân nặng (kg) <span class="text-danger">*</span></div>
              <input type="number" step="0.1" v-model="form.weight" class="form-control form-control-sm border-warning rounded-3 bg-white" placeholder="VD: 5.2" required>
            </div>
            <div class="col-md-3">
              <div class="small text-muted mb-1">Nhiệt độ (°C)</div>
              <input type="number" step="0.1" v-model="form.temperature" class="form-control form-control-sm border-warning rounded-3 bg-white" placeholder="VD: 38.5">
            </div>
          </div>

          <!-- Split-pane Layout -->
          <div class="row g-4">
            
            <!-- CỘT TRÁI: S (Subjective) & O (Objective) -->
            <div class="col-lg-6 d-flex flex-column gap-4">
              
              <!-- S - Subjective -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-primary fs-5" style="width:35px;height:35px;line-height:22px">S</span>
                <h6 class="fw-bold text-primary mb-3 ms-2 border-bottom pb-2">Thông tin chủ quan (Khách hàng)</h6>
                
                <div class="mb-3">
                  <label class="form-label small fw-bold text-secondary">Lý do tiêm <span class="text-danger">*</span></label>
                  <select v-model="form.reasonForVisit" class="form-select rounded-3 shadow-sm" required>
                    <option value="" disabled>-- Chọn lý do --</option>
                    <option value="Tiêm cơ bản">Tiêm cơ bản mũi mới</option>
                    <option value="Tiêm nhắc lại">Tiêm nhắc lại theo lịch</option>
                    <option value="Tư vấn tiêm chủng">Tư vấn tiêm chủng</option>
                  </select>
                </div>
                
                <div class="row g-3 mb-3">
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Tình trạng ăn uống <span class="text-danger">*</span></label>
                    <select v-model="form.eatingStatus" class="form-select rounded-3 shadow-sm" required>
                      <option value="Bình thường">Bình thường</option>
                      <option value="Biếng ăn">Biếng ăn</option>
                      <option value="Bỏ ăn">Bỏ ăn</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Tiền sử vắc-xin</label>
                    <input type="text" v-model="form.previousVaccineHistory" class="form-control rounded-3 shadow-sm" placeholder="VD: Đã tiêm dại cách đây 1 năm">
                  </div>
                </div>

                <div class="row g-2 mb-3">
                  <div class="col-6">
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="form.hasVomitingOrDiarrhea" id="vomit">
                      <label class="form-check-label small" for="vomit">Nôn mửa / Tiêu chảy</label>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="form.hasCoughOrSneeze" id="cough">
                      <label class="form-check-label small" for="cough">Ho / Hắt hơi</label>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="form-check form-switch mb-2">
                      <input class="form-check-input" type="checkbox" v-model="form.isAllergic" id="allergy">
                      <label class="form-check-label small text-danger fw-bold" for="allergy">Dị ứng</label>
                    </div>
                    <div v-if="form.isAllergic">
                      <input type="text" v-model="form.allergyDetails" class="form-control form-control-sm border-danger text-danger bg-danger bg-opacity-10" placeholder="Chi tiết tình trạng dị ứng..." required>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="form-check form-switch mb-2">
                      <input class="form-check-input" type="checkbox" v-model="form.hasPreviousReaction" id="reaction">
                      <label class="form-check-label small text-danger fw-bold" for="reaction">Sốc/Phản ứng tiêm cũ</label>
                    </div>
                    <div v-if="form.hasPreviousReaction">
                      <input type="text" v-model="form.previousReactionDetails" class="form-control form-control-sm border-danger text-danger bg-danger bg-opacity-10" placeholder="Triệu chứng phản vệ lần trước..." required>
                    </div>
                  </div>
                </div>
              </div>

              <!-- O - Objective -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-info fs-5" style="width:35px;height:35px;line-height:22px">O</span>
                <h6 class="fw-bold text-info mb-3 ms-2 border-bottom pb-2 text-dark">Khám lâm sàng (Bác sĩ)</h6>
                
                <div class="row g-3 mb-3">
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Tinh thần <span class="text-danger">*</span></label>
                    <select v-model="form.mentalStatus" class="form-select rounded-3 shadow-sm" required>
                      <option value="Linh hoạt">Linh hoạt</option>
                      <option value="Lờ đờ">Lờ đờ</option>
                      <option value="Hưng phấn">Hưng phấn</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Niêm mạc <span class="text-danger">*</span></label>
                    <select v-model="form.mucosaStatus" class="form-select rounded-3 shadow-sm" required>
                      <option value="Hồng hào">Hồng hào</option>
                      <option value="Nhợt nhạt">Nhợt nhạt</option>
                      <option value="Vàng">Vàng</option>
                    </select>
                  </div>
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Nhịp tim (lần/p)</label>
                    <input type="number" v-model="form.heartRate" class="form-control rounded-3 shadow-sm">
                  </div>
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Nhịp thở (lần/p)</label>
                    <input type="number" v-model="form.respiratoryRate" class="form-control rounded-3 shadow-sm">
                  </div>
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Mất nước (%)</label>
                    <input type="number" v-model="form.dehydrationPercent" class="form-control rounded-3 shadow-sm" placeholder="< 5%">
                  </div>
                </div>
              </div>
            </div>

            <!-- CỘT PHẢI: A (Assessment) & P (Plan) -->
            <div class="col-lg-6 d-flex flex-column gap-4">
              
              <!-- A - Assessment -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-success fs-5" style="width:35px;height:35px;line-height:22px">A</span>
                <h6 class="fw-bold text-success mb-3 ms-2 border-bottom pb-2">Đánh giá & Vắc-xin</h6>

                <div class="mb-4">
                  <label class="form-label small fw-bold text-secondary">Kết luận lâm sàng <span class="text-danger">*</span></label>
                  <div class="d-flex gap-3">
                    <div class="form-check form-check-inline border p-2 rounded-3 bg-white" :class="{'border-success bg-success bg-opacity-10': form.clinicalAssessment === 'Đủ điều kiện'}">
                      <input class="form-check-input" type="radio" v-model="form.clinicalAssessment" value="Đủ điều kiện" id="assess1" required>
                      <label class="form-check-label fw-bold text-success" for="assess1">Đủ điều kiện tiêm</label>
                    </div>
                    <div class="form-check form-check-inline border p-2 rounded-3 bg-white" :class="{'border-danger bg-danger bg-opacity-10': form.clinicalAssessment === 'Hoãn tiêm'}">
                      <input class="form-check-input" type="radio" v-model="form.clinicalAssessment" value="Hoãn tiêm" id="assess2" required>
                      <label class="form-check-label fw-bold text-danger" for="assess2">Hoãn tiêm</label>
                    </div>
                  </div>
                </div>

                <div v-if="form.clinicalAssessment === 'Hoãn tiêm'" class="mb-3">
                  <label class="form-label small fw-bold text-danger">Lý do hoãn tiêm (Chỉ tạo Phí Khám) <span class="text-danger">*</span></label>
                  <textarea v-model="form.doctorRemarks" class="form-control rounded-3 border-danger shadow-sm" rows="2" placeholder="Sốt cao, cần theo dõi thêm..." required></textarea>
                </div>

                <div v-if="form.clinicalAssessment === 'Đủ điều kiện'" class="vaccine-selector border border-success border-opacity-50 p-3 rounded-4 bg-white mb-3">
                  <div class="mb-3">
                    <label class="form-label small fw-bold text-secondary">Chọn Vắc-xin <span class="text-danger">*</span></label>
                    <select v-model="form.vaccineId" @change="onVaccineChange" class="form-select border-success rounded-3 shadow-sm fw-bold" required>
                      <option :value="null" disabled>-- Danh mục Vắc-xin --</option>
                      <option v-for="v in availableVaccines" :key="v.id" :value="v.id">
                        {{ v.name }} ({{ v.targetSpecies }})
                      </option>
                    </select>
                  </div>

                  <div v-if="form.vaccineId" class="mb-0">
                    <label class="form-label small fw-bold text-secondary">Chọn Lô & Hạn sử dụng <span class="text-danger">*</span></label>
                    <select v-model="form.vaccineBatchId" class="form-select rounded-3 shadow-sm" required>
                      <option :value="null" disabled>-- Chọn Lô --</option>
                      <option v-for="b in selectedVaccineBatches" :key="b.id" :value="b.id">
                        Lô: {{ b.batchNumber }} - HSD: {{ formatDate(b.expirationDate) }} (Tồn: {{ b.stockQuantity }}) - Giá: {{ b.sellingPrice.toLocaleString() }}đ
                      </option>
                    </select>
                    <div v-if="selectedVaccineBatches.length === 0" class="text-danger small mt-1">
                      <i class="bi bi-x-circle"></i> Vắc-xin này hiện đang hết hàng / Hết hạn sử dụng lô.
                    </div>
                  </div>
                </div>

                <div class="row g-3" v-if="form.clinicalAssessment === 'Đủ điều kiện'">
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Đường tiêm</label>
                    <select v-model="form.route" class="form-select rounded-3 shadow-sm">
                      <option value="Dưới da (SC)">Dưới da (SC)</option>
                      <option value="Tiêm bắp (IM)">Tiêm bắp (IM)</option>
                      <option value="Nhỏ mũi">Nhỏ mũi</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Vị trí tiêm</label>
                    <input type="text" v-model="form.injectionSite" class="form-control rounded-3 shadow-sm" placeholder="VD: Đùi phải">
                  </div>
                </div>
              </div>

              <!-- Hình Ảnh Cận Lâm Sàng -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle" style="width:35px;height:35px;line-height:22px;background:linear-gradient(135deg,#6366f1,#8b5cf6);border-radius:50%;display:flex;align-items:center;justify-content:center">
                  <i class="bi bi-camera-fill text-white" style="font-size:0.9rem"></i>
                </span>
                <h6 class="fw-bold mb-3 ms-2 border-bottom pb-2" style="color:#6366f1">Hình Ảnh Cận Lâm Sàng</h6>
                <span class="badge mb-3 d-inline-block" style="background:rgba(99,102,241,0.1);color:#6366f1;font-size:0.7rem">X-Quang · Siêu âm · Ảnh lâm sàng</span>

                <!-- Drag & Drop Zone -->
                <div
                  class="upload-dropzone rounded-4 text-center p-4 position-relative"
                  :class="{ 'dragging': isDraggingVacc }"
                  @dragover.prevent="isDraggingVacc = true"
                  @dragleave.prevent="isDraggingVacc = false"
                  @drop.prevent="onVaccFileDrop"
                  @click="triggerVaccFileInput"
                >
                  <input
                    ref="vaccFileInputRef"
                    type="file"
                    multiple
                    accept="image/jpeg,image/png,image/gif"
                    class="d-none"
                    @change="onVaccFileSelected"
                  />
                  <div v-if="vaccSelectedFiles.length === 0" class="py-2">
                    <i class="bi bi-cloud-arrow-up-fill opacity-50" style="font-size:2.5rem;color:#6366f1"></i>
                    <p class="fw-bold text-secondary mb-1 mt-2">Kéo &amp; thả ảnh vào đây</p>
                    <p class="small text-muted mb-0">hoặc <span class="fw-bold" style="color:#6366f1">bấm để chọn ảnh</span> · JPG, PNG, GIF · Tối đa 5MB/ảnh</p>
                  </div>
                  <div v-else class="d-flex align-items-center gap-2 flex-wrap justify-content-center">
                    <span class="text-success fw-bold"><i class="bi bi-check-circle-fill me-1"></i>{{ vaccSelectedFiles.length }} ảnh đã chọn</span>
                    <span class="text-muted small">· Bấm để thêm ảnh khác</span>
                  </div>
                </div>

                <!-- Preview Grid -->
                <div v-if="vaccPreviewUrls.length > 0" class="mt-3">
                  <div class="d-flex flex-wrap gap-2">
                    <div
                      v-for="(url, idx) in vaccPreviewUrls"
                      :key="idx"
                      class="img-preview-wrapper position-relative"
                    >
                      <img :src="url" class="img-preview rounded-3 shadow-sm" @click.stop="openVaccLightbox(vaccPreviewUrls, idx)" title="Bấm để xem ảnh to" />
                      <button
                        type="button"
                        class="btn-remove-img position-absolute top-0 end-0"
                        @click.stop="removeVaccPreviewImage(idx)"
                      >
                        <i class="bi bi-x-lg"></i>
                      </button>
                    </div>
                  </div>
                  <p class="text-muted small mt-2 mb-0"><i class="bi bi-info-circle me-1"></i>Ảnh sẽ được tải lên khi bạn nhấn "Lưu Bệnh Án SOAP"</p>
                </div>
              </div>

              <!-- P - Plan -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-warning text-dark fs-5" style="width:35px;height:35px;line-height:22px">P</span>
                <h6 class="fw-bold text-warning mb-3 ms-2 border-bottom pb-2">Kế hoạch & Dặn dò</h6>
                
                <div class="mb-3">
                  <label class="form-label small fw-bold text-secondary"><i class="bi bi-calendar-event text-warning me-1"></i> Ngày nhắc lại dự kiến</label>
                  <input type="date" v-model="form.nextDueDate" class="form-control rounded-pill shadow-sm border px-3">
                </div>
                
                <div class="mb-0">
                  <label class="form-label small fw-bold text-secondary">Lời dặn dò về nhà (In lên phiếu) <span class="text-danger">*</span></label>
                  <textarea v-model="form.followUpInstructions" class="form-control rounded-3 shadow-sm" rows="2" placeholder="Kiêng tắm 7 ngày, theo dõi nếu sốt..." required></textarea>
                </div>
              </div>

            </div>
          </div>

          <!-- Error Banner -->
          <div v-if="errorMessage" class="alert alert-danger rounded-4 p-3 small my-4 shadow-sm border-0 d-flex align-items-center">
            <i class="bi bi-exclamation-circle-fill fs-5 me-2"></i> {{ errorMessage }}
          </div>

          <!-- Actions -->
          <div class="d-flex justify-content-end align-items-center border-top pt-4 mt-4">
            <button type="button" class="btn btn-light rounded-pill px-4 fw-bold text-dark me-3 shadow-sm border" @click="cancelTreatment">
              Hủy bỏ
            </button>
            <button type="submit" class="btn btn-success rounded-pill px-5 fw-bold text-white shadow-lg btn-save" :disabled="submitting || isSubmitDisabled">
              <span v-if="submitting" class="spinner-border spinner-border-sm me-1"></span>
              <i class="bi bi-check2-all me-1"></i> Lưu Bệnh Án SOAP
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Tab 2: Lịch Sử Tiêm Phòng -->
    <div v-if="internalTab === 'pet-history'" class="flex-grow-1 overflow-auto">
      <div class="card border-0 shadow-sm rounded-4 p-4 bg-white h-100 border">
        <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 pb-3 border-bottom">
          <h6 class="fw-bold mb-0 text-dark"><i class="bi bi-shield-check text-success me-2"></i>Lịch sử tiêm chủng của bé</h6>
          
          <!-- Filters -->
          <div class="d-flex flex-wrap gap-3 mt-3 mt-sm-0 align-items-center" v-if="vaccinationHistory.length > 0">
            <!-- Bộ lọc Ngày tiêm -->
            <div class="position-relative filter-wrapper">
              <label class="position-absolute text-muted small px-2 bg-white" style="top: -8px; left: 16px; font-size: 0.7rem; z-index: 5; border-radius: 10px;">Lọc theo ngày</label>
              <div class="input-group shadow-sm rounded-pill overflow-hidden transition-all filter-group" style="width: 220px; height: 42px;">
                <span class="input-group-text border-0 bg-transparent text-success ps-3 pe-2"><i class="bi bi-calendar2-check-fill"></i></span>
                <input type="date" v-model="vaccDateFilter" class="form-control border-0 bg-transparent text-dark fw-medium custom-date-input px-1" style="outline: none; box-shadow: none;">
                <button v-if="vaccDateFilter" class="btn btn-link border-0 text-danger text-decoration-none px-3 d-flex align-items-center justify-content-center" @click="vaccDateFilter = ''" title="Xóa lọc">
                  <i class="bi bi-x-circle-fill"></i>
                </button>
              </div>
            </div>

            <!-- Bộ lọc Trạng thái -->
            <div class="position-relative filter-wrapper">
              <label class="position-absolute text-muted small px-2 bg-white" style="top: -8px; left: 16px; font-size: 0.7rem; z-index: 5; border-radius: 10px;">Đánh giá</label>
              <div class="input-group shadow-sm rounded-pill overflow-hidden transition-all filter-group" style="width: 170px; height: 42px;">
                <span class="input-group-text border-0 bg-transparent text-primary ps-3 pe-2"><i class="bi bi-funnel-fill"></i></span>
                <select v-model="vaccStatusFilter" class="form-select border-0 bg-transparent text-dark fw-medium custom-select px-1" style="outline: none; box-shadow: none; cursor: pointer;">
                  <option value="ALL">Tất cả</option>
                  <option value="Đủ điều kiện">Đủ điều kiện</option>
                  <option value="Hoãn tiêm">Hoãn tiêm</option>
                </select>
              </div>
            </div>
          </div>
        </div>
        
        <div v-if="loadingHistory" class="text-center py-5">
          <div class="spinner-border text-success" role="status"></div>
          <p class="text-muted small mt-3">Đang tải lịch sử tiêm...</p>
        </div>

        <div v-else-if="filteredVaccHistory.length === 0" class="text-center py-5 text-muted small">
          <i class="bi bi-shield-x fs-1 d-block mb-3 text-black-50 opacity-25"></i>
          Không tìm thấy lịch sử tiêm chủng phù hợp.
        </div>

        <div v-else class="table-responsive">
          <table class="table table-hover align-middle">
            <thead class="bg-light text-secondary small">
              <tr>
                <th>Ngày Tiêm</th>
                <th>Vắc-xin & Lô</th>
                <th>Bác sĩ</th>
                <th>Đánh giá</th>
                <th class="text-center">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="record in paginatedVaccHistory" :key="record.id">
                <td class="fw-bold text-dark">{{ formatDate(record.injectionDate) }}</td>
                <td>
                  <span v-if="record.vaccineName" class="badge bg-primary rounded-pill px-3 py-1 mb-1">{{ record.vaccineName }}</span><br>
                  <span v-if="record.batchNumber" class="small text-muted">Lô: {{ record.batchNumber }}</span>
                </td>
                <td>Bs. {{ record.doctorName }}</td>
                <td>
                  <span class="badge" :class="record.clinicalAssessment === 'Đủ điều kiện' ? 'bg-success' : 'bg-danger'">{{ record.clinicalAssessment }}</span>
                </td>
                <td class="text-center">
                  <button class="btn btn-sm btn-success bg-gradient bg-opacity-75 rounded-pill px-3 shadow-sm border-0 fw-bold" style="font-size: 0.75rem;" @click="openSoapDetail(record)">
                    <i class="bi bi-file-earmark-medical-fill me-1"></i> Bệnh án
                  </button>
                </td>
              </tr>
            </tbody>
          </table>

          <!-- Pagination UI -->
          <div class="d-flex justify-content-between align-items-center mt-3" v-if="vaccTotalPages > 1">
            <span class="small text-muted">Đang xem <strong>{{ (vaccCurrentPage - 1) * vaccPageSize + 1 }}</strong> - <strong>{{ Math.min(vaccCurrentPage * vaccPageSize, filteredVaccHistory.length) }}</strong> trong tổng số <strong>{{ filteredVaccHistory.length }}</strong> kết quả</span>
            <ul class="pagination pagination-sm mb-0 shadow-sm overflow-hidden">
              <li class="page-item" :class="{ disabled: vaccCurrentPage === 1 }">
                <a class="page-link text-success fw-bold" href="#" @click.prevent="vaccCurrentPage--"><i class="bi bi-chevron-left"></i></a>
              </li>
              <li class="page-item" v-for="p in vaccTotalPages" :key="p" :class="{ active: p === vaccCurrentPage }">
                <a class="page-link bg-success border-success text-white fw-bold" v-if="p === vaccCurrentPage" href="#">{{ p }}</a>
                <a class="page-link text-secondary fw-bold" v-else href="#" @click.prevent="vaccCurrentPage = p">{{ p }}</a>
              </li>
              <li class="page-item" :class="{ disabled: vaccCurrentPage === vaccTotalPages }">
                <a class="page-link text-success fw-bold" href="#" @click.prevent="vaccCurrentPage++"><i class="bi bi-chevron-right"></i></a>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- ===== SOAP DETAIL MODAL ===== -->
  <div v-if="showSoapModal" class="zalo-modal-overlay" style="z-index: 1055;" @click.self="closeSoapModal">
    <div class="zalo-modal-card modal-lg max-w-700 bg-light border-0 shadow-lg" style="animation: fadeUp 0.3s ease-out; margin-top: 5vh; margin-bottom: 5vh; max-height: 90vh; display: flex; flex-direction: column;">
      <div class="zalo-modal-header bg-white border-bottom border-success border-opacity-25 p-4 flex-shrink-0 d-flex justify-content-between align-items-center" style="background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px);">
        <h5 class="modal-title fw-bold text-success mb-0 d-flex align-items-center">
          <div class="bg-success bg-opacity-10 p-2 rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;"><i class="bi bi-shield-check fs-4"></i></div>
          Hồ Sơ Bệnh Án Tiêm Chủng (S.O.A.P)
        </h5>
        <button class="btn-close shadow-none" @click="closeSoapModal"></button>
      </div>
      <div class="zalo-modal-body text-start p-4 bg-light overflow-auto flex-grow-1" v-if="selectedSoapRecord">
        <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 p-3 bg-white rounded-4 shadow-sm border border-light">
          <div class="d-flex align-items-center gap-3">
            <div class="bg-primary bg-opacity-10 text-primary p-3 rounded-circle fs-3"><i class="bi bi-calendar2-check-fill"></i></div>
            <div>
              <h6 class="fw-bold text-dark mb-1 fs-5">{{ formatDate(selectedSoapRecord.injectionDate) }}</h6>
              <div class="small text-muted"><i class="bi bi-person-badge-fill me-1 text-secondary"></i> Bác sĩ: <span class="fw-bold text-dark">Bs. {{ selectedSoapRecord.doctorName }}</span></div>
            </div>
          </div>
          <span class="badge rounded-pill px-4 py-2 fs-6 shadow-sm mt-3 mt-sm-0" :class="selectedSoapRecord.clinicalAssessment === 'Đủ điều kiện' ? 'bg-success bg-opacity-10 text-success border border-success border-opacity-25' : 'bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25'">
            {{ selectedSoapRecord.clinicalAssessment }}
          </span>
        </div>

        <div class="row g-4">
          <!-- CỘT TRÁI -->
          <div class="col-md-6 d-flex flex-column gap-4">
            <!-- S - Subjective -->
            <div class="p-4 bg-white rounded-4 shadow-sm border border-primary border-opacity-10 h-100 position-relative transition-all hover-lift">
              <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-primary shadow-sm" style="width: 32px; height: 32px; line-height: 22px; font-size: 1rem;">S</span>
              <h6 class="fw-bold text-primary mb-3 ms-2 border-bottom border-light pb-2"><i class="bi bi-person-lines-fill me-1"></i> Triệu Chứng Chủ Quan</h6>
              <ul class="list-unstyled small mb-0 d-flex flex-column gap-3 text-dark mt-3">
                <li class="d-flex justify-content-between border-bottom border-light pb-2"><strong class="text-secondary fw-semibold">Lý do tiêm:</strong> <span class="text-end fw-medium">{{ selectedSoapRecord.reasonForVisit || '—' }}</span></li>
                <li class="d-flex justify-content-between border-bottom border-light pb-2"><strong class="text-secondary fw-semibold">Ăn uống:</strong> <span class="text-end fw-medium">{{ selectedSoapRecord.eatingStatus || '—' }}</span></li>
                <li class="d-flex justify-content-between border-bottom border-light pb-2">
                  <strong class="text-secondary fw-semibold">Nôn/Tiêu chảy:</strong> 
                  <span class="text-end fw-bold" :class="selectedSoapRecord.hasVomitingOrDiarrhea ? 'text-danger' : 'text-success'"><i class="bi" :class="selectedSoapRecord.hasVomitingOrDiarrhea ? 'bi-exclamation-circle-fill' : 'bi-check-circle-fill'"></i> {{ selectedSoapRecord.hasVomitingOrDiarrhea ? 'Có' : 'Không' }}</span>
                </li>
                <li class="d-flex justify-content-between border-bottom border-light pb-2">
                  <strong class="text-secondary fw-semibold">Ho/Hắt hơi:</strong> 
                  <span class="text-end fw-bold" :class="selectedSoapRecord.hasCoughOrSneeze ? 'text-danger' : 'text-success'"><i class="bi" :class="selectedSoapRecord.hasCoughOrSneeze ? 'bi-exclamation-circle-fill' : 'bi-check-circle-fill'"></i> {{ selectedSoapRecord.hasCoughOrSneeze ? 'Có' : 'Không' }}</span>
                </li>
                <li class="d-flex justify-content-between align-items-start border-bottom border-light pb-2">
                  <strong class="text-secondary fw-semibold">Dị ứng:</strong> 
                  <span v-if="selectedSoapRecord.isAllergic" class="text-end text-danger fw-bold bg-danger bg-opacity-10 px-2 py-1 rounded">{{ selectedSoapRecord.allergyDetails }}</span>
                  <span v-else class="text-end fw-bold text-success"><i class="bi bi-check-circle-fill"></i> Không</span>
                </li>
                <li v-if="selectedSoapRecord.previousVaccineHistory" class="d-flex flex-column bg-light p-2 rounded">
                  <strong class="text-secondary fw-semibold mb-1">Tiền sử vắc-xin:</strong> 
                  <span class="fst-italic">{{ selectedSoapRecord.previousVaccineHistory }}</span>
                </li>
              </ul>
            </div>
            
            <!-- O - Objective -->
            <div class="p-4 bg-white rounded-4 shadow-sm border border-info border-opacity-25 h-100 position-relative transition-all hover-lift">
              <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-info text-white shadow-sm" style="width: 32px; height: 32px; line-height: 22px; font-size: 1rem;">O</span>
              <h6 class="fw-bold text-info mb-3 ms-2 border-bottom border-light pb-2"><i class="bi bi-heart-pulse-fill me-1"></i> Khám Lâm Sàng</h6>
              <div class="row g-3 small text-dark mt-1">
                <div class="col-6 d-flex flex-column"><strong class="text-secondary fw-semibold mb-1">Cân nặng</strong> <span class="fs-6 fw-bold text-dark">{{ selectedSoapRecord.weight }} kg</span></div>
                <div class="col-6 d-flex flex-column"><strong class="text-secondary fw-semibold mb-1">Nhiệt độ</strong> <span class="fs-6 fw-bold text-dark" :class="selectedSoapRecord.temperature > 39 ? 'text-danger' : ''">{{ selectedSoapRecord.temperature ? selectedSoapRecord.temperature + ' °C' : '—' }}</span></div>
                <div class="col-6 d-flex flex-column"><strong class="text-secondary fw-semibold mb-1">Nhịp tim</strong> <span class="fs-6 fw-bold text-dark">{{ selectedSoapRecord.heartRate || '—' }} <small class="text-muted">lần/p</small></span></div>
                <div class="col-6 d-flex flex-column"><strong class="text-secondary fw-semibold mb-1">Nhịp thở</strong> <span class="fs-6 fw-bold text-dark">{{ selectedSoapRecord.respiratoryRate || '—' }} <small class="text-muted">lần/p</small></span></div>
                
                <div class="col-12 border-top border-light pt-2 mt-2"></div>
                
                <div class="col-6 d-flex flex-column"><strong class="text-secondary fw-semibold mb-1">Tinh thần</strong> <span class="fw-medium badge bg-light text-dark border p-2">{{ selectedSoapRecord.mentalStatus || '—' }}</span></div>
                <div class="col-6 d-flex flex-column"><strong class="text-secondary fw-semibold mb-1">Niêm mạc</strong> <span class="fw-medium badge bg-light text-dark border p-2">{{ selectedSoapRecord.mucosaStatus || '—' }}</span></div>
                <div class="col-12 d-flex flex-column" v-if="selectedSoapRecord.dehydrationPercent"><strong class="text-secondary fw-semibold mb-1">Mất nước</strong> <span class="fw-medium badge bg-light text-dark border p-2">{{ selectedSoapRecord.dehydrationPercent }}%</span></div>
              </div>
            </div>
          </div>

          <!-- CỘT PHẢI -->
          <div class="col-md-6 d-flex flex-column gap-4">
            <!-- A - Assessment -->
            <div class="p-4 bg-white rounded-4 shadow-sm border border-success border-opacity-25 h-100 position-relative transition-all hover-lift">
              <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-success shadow-sm" style="width: 32px; height: 32px; line-height: 22px; font-size: 1rem;">A</span>
              <h6 class="fw-bold text-success mb-3 ms-2 border-bottom border-light pb-2"><i class="bi bi-clipboard-pulse me-1"></i> Đánh Giá & Vắc-xin</h6>
              <div class="small text-dark d-flex flex-column gap-3 mt-3">
                <div v-if="selectedSoapRecord.vaccineName" class="bg-success bg-opacity-10 p-3 rounded-4 border border-success border-opacity-25">
                  <div class="fw-bold text-dark fs-6 text-success mb-2 d-flex align-items-center"><i class="bi bi-syringe fs-4 me-2 text-success"></i> {{ selectedSoapRecord.vaccineName }}</div>
                  <div class="d-flex justify-content-between border-bottom border-success border-opacity-25 pb-2 mb-2">
                    <span class="text-secondary fw-semibold">Lô Vắc-xin:</span> <span class="fw-bold">{{ selectedSoapRecord.batchNumber || '—' }}</span>
                  </div>
                  <div class="d-flex justify-content-between border-bottom border-success border-opacity-25 pb-2 mb-2">
                    <span class="text-secondary fw-semibold">Đường tiêm:</span> <span class="fw-medium">{{ selectedSoapRecord.route || '—' }}</span>
                  </div>
                  <div class="d-flex justify-content-between" v-if="selectedSoapRecord.injectionSite">
                    <span class="text-secondary fw-semibold">Vị trí:</span> <span class="fw-medium">{{ selectedSoapRecord.injectionSite }}</span>
                  </div>
                </div>
                <div v-else-if="selectedSoapRecord.clinicalAssessment === 'Hoãn tiêm'" class="bg-danger bg-opacity-10 p-3 rounded-4 border border-danger border-opacity-25">
                  <div class="fw-bold text-danger mb-1"><i class="bi bi-x-octagon-fill me-1"></i> Lý do hoãn tiêm:</div>
                  <div class="text-dark fst-italic">{{ selectedSoapRecord.doctorRemarks }}</div>
                </div>
                <div v-else class="text-muted fst-italic text-center p-3 border rounded bg-light">
                  <i class="bi bi-slash-circle me-1"></i> Không ghi nhận tiêm vắc-xin.
                </div>
              </div>
            </div>

            <!-- P - Plan -->
            <div class="p-4 bg-white rounded-4 shadow-sm border border-warning border-opacity-50 h-100 position-relative transition-all hover-lift">
              <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-warning text-dark shadow-sm" style="width: 32px; height: 32px; line-height: 22px; font-size: 1rem;">P</span>
              <h6 class="fw-bold text-warning text-dark mb-3 ms-2 border-bottom border-light pb-2"><i class="bi bi-clipboard-check me-1"></i> Kế Hoạch & Dặn Dò</h6>
              <div class="small text-dark mt-3">
                <div class="d-flex align-items-center mb-3 p-3 bg-warning bg-opacity-10 rounded-4 border border-warning border-opacity-25">
                  <div class="bg-white p-2 rounded-circle me-3 text-warning shadow-sm"><i class="bi bi-calendar-check-fill fs-5"></i></div>
                  <div>
                    <div class="text-secondary fw-semibold mb-1">Ngày nhắc lại vắc-xin:</div>
                    <div class="fw-bold fs-6 text-dark" :class="!selectedSoapRecord.nextDueDate ? 'text-muted' : ''">
                      {{ selectedSoapRecord.nextDueDate ? formatDate(selectedSoapRecord.nextDueDate) : 'Không có hẹn nhắc lại' }}
                    </div>
                  </div>
                </div>
                <div class="mb-2">
                  <strong class="text-secondary fw-semibold d-block mb-2"><i class="bi bi-chat-square-text-fill text-warning me-1"></i> Lời dặn dò bác sĩ:</strong>
                  <div class="bg-light p-3 rounded-4 text-dark border-start border-4 border-warning border-opacity-50 fst-italic d-flex align-items-start gap-2">
                    <i class="bi bi-quote text-warning opacity-50 fs-3" style="line-height: 1;"></i>
                    <span class="pt-1">{{ selectedSoapRecord.followUpInstructions || 'Bác sĩ không có dặn dò thêm.' }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- CẬN LÂM SÀNG (ATTACHMENTS) -->
        <div v-if="selectedSoapRecord.attachments && selectedSoapRecord.attachments.length > 0" class="mt-4 p-4 bg-white rounded-4 shadow-sm border border-secondary border-opacity-10 position-relative transition-all hover-lift">
          <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-secondary shadow-sm d-flex align-items-center justify-content-center" style="width: 32px; height: 32px;"><i class="bi bi-camera-fill"></i></span>
          <h6 class="fw-bold text-dark mb-3 ms-2 border-bottom border-light pb-2"><i class="bi bi-images me-1"></i> Hình Ảnh Cận Lâm Sàng</h6>
          <div class="d-flex gap-3 flex-wrap mt-3">
            <img
              v-for="(imgUrl, imgIdx) in selectedSoapRecord.attachments"
              :key="imgIdx"
              :src="getImageUrl(imgUrl)"
              class="rounded-3 shadow-sm border border-light"
              style="width: 120px; height: 120px; object-fit: cover; cursor: zoom-in; transition: transform 0.2s;"
              onmouseover="this.style.transform='scale(1.05)'"
              onmouseout="this.style.transform='scale(1)'"
              @click.stop="openVaccLightbox(selectedSoapRecord.attachments.map((u: string) => getImageUrl(u)), imgIdx)"
              title="Bấm để xem phóng to"
              alt="Ảnh cận lâm sàng"
            />
          </div>
        </div>

      </div>
      <div class="zalo-modal-footer bg-white border-top flex-shrink-0 p-3 d-flex justify-content-end rounded-bottom-4" style="background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px);">
        <button class="btn btn-light text-muted rounded-pill px-5 py-2 fw-bold shadow-sm border" @click="closeSoapModal">Đóng Lại</button>
      </div>
    </div>
  </div>
  <!-- ===== END SOAP DETAIL MODAL ===== -->

  <!-- ===== LIGHTBOX MODAL (VACCINATION) ===== -->
  <Teleport to="body">
    <Transition name="lightbox-fade">
      <div
        v-if="vaccLightbox.visible"
        class="lightbox-overlay"
        @click.self="closeVaccLightbox"
        @keydown.esc="closeVaccLightbox"
        tabindex="0"
        ref="vaccLightboxRef"
      >
        <button class="lightbox-close" @click="closeVaccLightbox" title="Đóng (Esc)">
          <i class="bi bi-x-lg"></i>
        </button>
        <button
          v-if="vaccLightbox.urls.length > 1"
          class="lightbox-nav lightbox-prev"
          @click.stop="vaccLightboxNav(-1)"
          title="Ảnh trước"
        >
          <i class="bi bi-chevron-left"></i>
        </button>
        <div class="lightbox-img-wrapper">
          <img
            :src="vaccLightbox.urls[vaccLightbox.index]"
            class="lightbox-img"
            alt="Ảnh cận lâm sàng tiêm chủng"
          />
          <div class="lightbox-counter" v-if="vaccLightbox.urls.length > 1">
            {{ vaccLightbox.index + 1 }} / {{ vaccLightbox.urls.length }}
          </div>
        </div>
        <button
          v-if="vaccLightbox.urls.length > 1"
          class="lightbox-nav lightbox-next"
          @click.stop="vaccLightboxNav(1)"
          title="Ảnh sau"
        >
          <i class="bi bi-chevron-right"></i>
        </button>
      </div>
    </Transition>
  </Teleport>
  <!-- ===== END LIGHTBOX MODAL ===== -->

</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue';
import api from '../../services/api';

const emit = defineEmits<{
  (e: 'switch-tab', tab: string): void;
}>();

const internalTab = ref('treatment');
const activePatient = ref({
  appointmentId: '',
  petId: '',
  petName: '',
  customerName: ''
});

// SOAP Form
const form = ref({
  appointmentId: 0,
  petId: 0,
  weight: null as number | null,
  temperature: null as number | null,
  reasonForVisit: '',
  previousVaccineHistory: '',
  isAllergic: false,
  allergyDetails: '',
  hasPreviousReaction: false,
  previousReactionDetails: '',
  isUnderTreatment: false,
  treatmentDetails: '',
  eatingStatus: 'Bình thường',
  hasVomitingOrDiarrhea: false,
  hasCoughOrSneeze: false,
  ownerNotes: '',
  heartRate: null as number | null,
  respiratoryRate: null as number | null,
  mentalStatus: 'Linh hoạt',
  mucosaStatus: 'Hồng hào',
  eyeNoseEarStatus: '',
  lymphNodeStatus: '',
  dehydrationPercent: null as number | null,
  vaccineId: null as number | null,
  vaccineBatchId: null as number | null,
  dose: 1,
  route: 'Dưới da (SC)',
  injectionSite: '',
  clinicalAssessment: 'Đủ điều kiện',
  doctorRemarks: '',
  nextDueDate: '',
  followUpInstructions: '',
  reactionNote: '',
  attachments: [] as string[]
});

const availableVaccines = ref<any[]>([]);
const vaccinationHistory = ref<any[]>([]);

// ===== Pagination & Filters =====
const vaccDateFilter = ref('');
const vaccStatusFilter = ref('ALL');
const vaccCurrentPage = ref(1);
const vaccPageSize = ref(5);

const filteredVaccHistory = computed(() => {
  let list = vaccinationHistory.value;
  if (vaccStatusFilter.value !== 'ALL') {
    list = list.filter((r: any) => r.clinicalAssessment === vaccStatusFilter.value);
  }
  if (vaccDateFilter.value) {
    const selectedDate = vaccDateFilter.value;
    list = list.filter((r: any) => {
      if (!r.injectionDate) return false;
      const recordDate = new Date(r.injectionDate).toISOString().split('T')[0];
      return recordDate === selectedDate;
    });
  }
  return list;
});

const vaccTotalPages = computed(() => Math.max(1, Math.ceil(filteredVaccHistory.value.length / vaccPageSize.value)));

const paginatedVaccHistory = computed(() => {
  const start = (vaccCurrentPage.value - 1) * vaccPageSize.value;
  return filteredVaccHistory.value.slice(start, start + vaccPageSize.value);
});

watch([vaccDateFilter, vaccStatusFilter], () => {
  vaccCurrentPage.value = 1;
});

const loadingHistory = ref(false);
const submitting = ref(false);
const errorMessage = ref('');

// ===== Image Upload =====
const vaccFileInputRef = ref<HTMLInputElement | null>(null);
const vaccSelectedFiles = ref<File[]>([]);
const vaccPreviewUrls = ref<string[]>([]);
const isDraggingVacc = ref(false);

// ===== Lightbox =====
const vaccLightboxRef = ref<HTMLElement | null>(null);
const vaccLightbox = ref<{ visible: boolean; urls: string[]; index: number }>({
  visible: false,
  urls: [],
  index: 0
});

// ===== SOAP Modal State =====
const showSoapModal = ref(false);
const selectedSoapRecord = ref<any>(null);

const openSoapDetail = (record: any) => {
  selectedSoapRecord.value = record;
  showSoapModal.value = true;
};

const closeSoapModal = () => {
  showSoapModal.value = false;
  selectedSoapRecord.value = null;
};

const openVaccLightbox = (urls: string[], index: number) => {
  vaccLightbox.value = { visible: true, urls, index };
  nextTick(() => vaccLightboxRef.value?.focus());
};

const closeVaccLightbox = () => {
  vaccLightbox.value.visible = false;
};

const vaccLightboxNav = (dir: number) => {
  const len = vaccLightbox.value.urls.length;
  vaccLightbox.value.index = (vaccLightbox.value.index + dir + len) % len;
};

const triggerVaccFileInput = () => vaccFileInputRef.value?.click();

const addVaccFiles = (files: FileList) => {
  for (const file of Array.from(files)) {
    if (file.size > 5 * 1024 * 1024) { alert(`File ${file.name} vượt quá 5MB.`); continue; }
    vaccSelectedFiles.value.push(file);
    vaccPreviewUrls.value.push(URL.createObjectURL(file));
  }
};

const onVaccFileSelected = (e: Event) => {
  const input = e.target as HTMLInputElement;
  if (input.files) addVaccFiles(input.files);
};

const onVaccFileDrop = (e: DragEvent) => {
  isDraggingVacc.value = false;
  if (e.dataTransfer?.files) addVaccFiles(e.dataTransfer.files);
};

const removeVaccPreviewImage = (idx: number) => {
  URL.revokeObjectURL(vaccPreviewUrls.value[idx]);
  vaccPreviewUrls.value.splice(idx, 1);
  vaccSelectedFiles.value.splice(idx, 1);
};

const selectedVaccineBatches = computed(() => {
  if (!form.value.vaccineId) return [];
  const vaccine = availableVaccines.value.find(v => v.id === form.value.vaccineId);
  return vaccine ? vaccine.batches : [];
});

const isSubmitDisabled = computed(() => {
  if (form.value.clinicalAssessment === 'Đủ điều kiện') {
    return !form.value.vaccineId || !form.value.vaccineBatchId;
  }
  return false;
});

onMounted(async () => {
  const appointmentId = localStorage.getItem('active_treatment_appointment_id');
  const petId = localStorage.getItem('active_treatment_pet_id');
  
  if (appointmentId && petId) {
    activePatient.value = {
      appointmentId,
      petId,
      petName: localStorage.getItem('active_treatment_pet_name') || 'Bệnh nhi',
      customerName: localStorage.getItem('active_treatment_customer_name') || 'Khách vãng lai'
    };
    form.value.appointmentId = parseInt(appointmentId, 10);
    form.value.petId = parseInt(petId, 10);
    
    fetchPetVaccinationHistory(parseInt(petId, 10));
  }

  fetchVaccines();
});

const fetchVaccines = async () => {
  try {
    const res = await api.get('/vaccinations/vaccines');
    if (res.data.success) {
      availableVaccines.value = res.data.data || [];
    }
  } catch (err) {
    console.error('Lỗi tải danh mục vắc-xin:', err);
  }
};

const fetchPetVaccinationHistory = async (petId: number) => {
  loadingHistory.value = true;
  try {
    const res = await api.get(`/vaccinations/pet/${petId}`);
    vaccinationHistory.value = res.data || res.data?.data || [];
  } catch (err) {
    console.error('Lỗi tải lịch sử tiêm chủng:', err);
  } finally {
    loadingHistory.value = false;
  }
};

const onVaccineChange = () => {
  form.value.vaccineBatchId = null; // reset batch when vaccine changes
  
  // Auto calculate nextDueDate based on vaccine intervalDays
  const vaccine = availableVaccines.value.find(v => v.id === form.value.vaccineId);
  if (vaccine && vaccine.intervalDays) {
    const nextDate = new Date();
    nextDate.setDate(nextDate.getDate() + vaccine.intervalDays);
    form.value.nextDueDate = nextDate.toISOString().split('T')[0];
  } else {
    form.value.nextDueDate = '';
  }
};

const submitForm = async () => {
  submitting.value = true;
  errorMessage.value = '';

  try {
    // Upload ảnh trước nếu có
    let uploadedUrls: string[] = [];
    if (vaccSelectedFiles.value.length > 0) {
      const formData = new FormData();
      vaccSelectedFiles.value.forEach(f => formData.append('files', f));
      const uploadRes = await api.post('/upload/medical-images', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      if (uploadRes.data.success) uploadedUrls = uploadRes.data.urls || [];
    }

    // Xử lý logic VR03, VR04, VR06 trước khi gửi
    const payload = { ...form.value, attachments: uploadedUrls };
    
    if (payload.clinicalAssessment === 'Hoãn tiêm') {
      payload.vaccineId = null;
      payload.vaccineBatchId = null;
    }

    if (payload.nextDueDate) {
      payload.nextDueDate = new Date(payload.nextDueDate).toISOString();
    } else {
      payload.nextDueDate = undefined as any;
    }

    const res = await api.post(`/vaccinations/appointments/${payload.appointmentId}`, payload);
    if (res.data.success) {
      // Tự động chuyển sang trạng thái chờ thanh toán
      try {
        await api.put(`/receptionist/queue/${payload.appointmentId}/status`, { status: 'ready_to_pay' });
      } catch (e) {
        console.warn('Không thể tự chuyển trạng thái ready_to_pay:', e);
      }

      localStorage.removeItem('active_treatment_appointment_id');
      localStorage.removeItem('active_treatment_pet_id');
      localStorage.removeItem('active_treatment_pet_name');
      localStorage.removeItem('active_treatment_customer_name');
      localStorage.removeItem('active_treatment_service_type');
      
      emit('switch-tab', 'doctor-cases');
    }
  } catch (err: any) {
    // Map backend FluentValidation/DataAnnotations errors if available
    if (err.response?.data?.errors) {
      const errors = err.response.data.errors;
      const firstError = Object.values(errors)[0] as string[];
      errorMessage.value = firstError[0];
    } else {
      errorMessage.value = err.response?.data?.message || 'Không thể lưu bệnh án. Vui lòng kiểm tra lại dữ liệu.';
    }
  } finally {
    submitting.value = false;
  }
};

const cancelTreatment = () => {
  emit('switch-tab', 'doctor-cases');
};

const getImageUrl = (url: string) => {
  if (!url) return '';
  if (url.startsWith('http')) return url;
  
  // Lấy baseUrl từ env, nếu có đuôi /api thì cắt đi để ghép đúng path file tĩnh
  let baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7284';
  baseUrl = baseUrl.replace(/\/api$/, '');
  
  return `${baseUrl}${url}`;
};

const formatDate = (dateStr: string | null): string => {
  if (!dateStr) return '';
  return new Date(dateStr).toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' });
};
</script>

<style scoped>
/* ===== Premium Filters ===== */
.filter-group {
  border: 1.5px solid #e5e7eb;
  background-color: #f9fafb;
}
.filter-wrapper:hover .filter-group {
  border-color: #10b981;
  background-color: #ffffff;
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.15) !important;
}
.custom-date-input::-webkit-calendar-picker-indicator {
  cursor: pointer;
  opacity: 0.6;
  transition: opacity 0.2s ease;
  padding: 5px;
}
.custom-date-input::-webkit-calendar-picker-indicator:hover {
  opacity: 1;
}

.medical-records-tab {
  padding: 0;
}

.hover-lift {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}
.hover-lift:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 20px rgba(0,0,0,0.08) !important;
}

.bg-glass {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
}

.custom-pills .nav-link {
  color: #6b7280;
  border-radius: 50px;
  padding: 10px 24px;
  margin-right: 10px;
  background-color: #f3f4f6;
  transition: all 0.3s ease;
}

.custom-pills .nav-link:hover {
  background-color: #e5e7eb;
}

.custom-pills .nav-link.active {
  background: linear-gradient(135deg, #198754, #20c997);
  color: white;
  box-shadow: 0 4px 10px rgba(32, 201, 151, 0.3);
}

.btn-save {
  transition: all 0.3s;
}
.btn-save:hover {
  transform: translateY(-1px);
}
.btn-save:disabled {
  cursor: not-allowed;
  transform: none;
}

/* ===== Image Upload Zone ===== */
.upload-dropzone {
  border: 2px dashed #c7d2fe;
  background: rgba(99, 102, 241, 0.03);
  cursor: pointer;
  transition: all 0.25s ease;
  min-height: 90px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.upload-dropzone:hover, .upload-dropzone.dragging {
  border-color: #6366f1;
  background: rgba(99, 102, 241, 0.08);
  box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.1);
}

/* ===== Preview Images ===== */
.img-preview-wrapper {
  width: 90px;
  height: 90px;
  overflow: visible;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  flex-shrink: 0;
}
.img-preview {
  width: 90px;
  height: 90px;
  object-fit: cover;
  cursor: zoom-in;
  border-radius: 10px;
  display: block;
  transition: transform 0.2s ease;
}
.img-preview:hover { transform: scale(1.05); }
.btn-remove-img {
  background: rgba(220, 38, 38, 0.85);
  border: none;
  border-radius: 50%;
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: white;
  font-size: 0.6rem;
  transition: background 0.2s;
  transform: translate(30%, -30%);
  z-index: 2;
}
.btn-remove-img:hover { background: #dc2626; }

/* ===== Lightbox ===== */
.lightbox-overlay {
  position: fixed;
  inset: 0;
  background: rgba(5, 5, 15, 0.92);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(6px);
}
.lightbox-img-wrapper {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}
.lightbox-img {
  max-width: 88vw;
  max-height: 88vh;
  object-fit: contain;
  border-radius: 12px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.6);
}
.lightbox-close {
  position: fixed;
  top: 22px;
  right: 28px;
  background: rgba(255,255,255,0.12);
  border: 1px solid rgba(255,255,255,0.2);
  color: white;
  font-size: 1.2rem;
  width: 42px;
  height: 42px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.2s;
  z-index: 10000;
}
.lightbox-close:hover { background: rgba(255,255,255,0.25); }
.lightbox-nav {
  position: fixed;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(255,255,255,0.12);
  border: 1px solid rgba(255,255,255,0.2);
  color: white;
  font-size: 1.4rem;
  width: 52px;
  height: 52px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.2s;
  z-index: 10000;
}
.lightbox-nav:hover { background: rgba(255,255,255,0.28); }
.lightbox-prev { left: 24px; }
.lightbox-next { right: 24px; }
.lightbox-counter {
  position: absolute;
  bottom: -34px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(255,255,255,0.15);
  color: white;
  font-size: 0.8rem;
  padding: 3px 14px;
  border-radius: 20px;
  backdrop-filter: blur(4px);
}

/* Lightbox Transition */
.lightbox-fade-enter-active, .lightbox-fade-leave-active { transition: opacity 0.25s ease; }
.lightbox-fade-enter-from, .lightbox-fade-leave-to { opacity: 0; }

/* History table image thumbs */
.history-thumb {
  width: 46px;
  height: 46px;
  object-fit: cover;
  border-radius: 8px;
  cursor: zoom-in;
  border: 2px solid #e0e7ff;
  transition: transform 0.2s, border-color 0.2s;
  box-shadow: 0 1px 4px rgba(0,0,0,0.12);
}
.history-thumb:hover {
  transform: scale(1.15);
  border-color: #6366f1;
}
</style>
