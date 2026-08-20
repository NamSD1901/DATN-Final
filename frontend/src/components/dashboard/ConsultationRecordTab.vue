<template>
  <div class="medical-records-tab h-100 d-flex flex-column">
    <!-- Inner Tabs Navigation -->
    <ul class="nav nav-pills custom-pills mb-4">
      <li class="nav-item">
        <a class="nav-link fw-bold" :class="{ 'active': internalTab === 'treatment' }" href="#" @click.prevent="internalTab = 'treatment'">
          <i class="bi bi-clipboard2-pulse-fill me-1"></i> Phiếu Điều Trị
        </a>
      </li>
      <li class="nav-item">
        <a class="nav-link fw-bold" :class="{ 'active': internalTab === 'pet-history' }" href="#" @click.prevent="internalTab = 'pet-history'">
          <i class="bi bi-folder2-open me-1"></i> Hồ Sơ & Bệnh Sử
        </a>
      </li>
    </ul>

    <!-- Tab 1: Phiếu Điều Trị -->
    <div v-if="internalTab === 'treatment'" class="flex-grow-1">
      <!-- Breadcrumb Header like Image 2 -->
      <div class="d-flex flex-column mb-3">
        <h4 class="fw-bold text-dark mb-0">Phiếu Điều Trị</h4>
        <span class="text-muted small">Hôm nay: {{ new Date().toLocaleDateString('en-GB', { weekday: 'long', year: 'numeric', month: '2-digit', day: '2-digit' }) }}</span>
      </div>

      <div class="card border-0 shadow-sm rounded-4 p-4 bg-glass bg-white mb-4 border-top border-4 border-primary">
        <!-- Back Navigation & Name -->
        <div class="d-flex justify-content-between align-items-center mb-4 border-bottom pb-3">
          <div>
            <a href="#" class="text-muted text-decoration-none fw-bold" @click.prevent="cancelTreatment">
              <i class="bi bi-arrow-left me-1"></i> Ca khám của tôi / <span class="text-dark">{{ activePatient.petName }}</span>
            </a>
          </div>
          <div class="badge bg-primary text-white rounded-pill px-4 py-2 fs-6 shadow-sm">
            <i class="bi bi-stethoscope me-1"></i> Khám Bệnh Tổng Quát
          </div>
        </div>

        <!-- Alert: Not selected patient -->
        <div v-if="!activePatient.appointmentId" class="alert alert-info rounded-4 border-0 p-4 mb-0 text-center">
          <i class="bi bi-exclamation-triangle-fill text-info fs-1 d-block mb-2"></i>
          <h6 class="fw-bold text-dark mb-2">Chưa chọn ca khám hoạt động</h6>
          <p class="text-muted small mb-3">Vui lòng quay lại tab "Ca khám của tôi" để chọn bệnh nhi bắt đầu khám.</p>
          <button class="btn btn-warning text-dark fw-bold rounded-pill px-4" @click="$emit('switch-tab', 'doctor-cases')">
            <i class="bi bi-arrow-left me-1"></i> Xem hàng khám
          </button>
        </div>

        <form v-else @submit.prevent="submitForm">
          <!-- Patient Quick Info -->
          <div class="row mb-4 bg-light rounded-4 p-3 mx-0">
            <div class="col-md-6 border-end">
              <div class="d-flex align-items-center gap-3">
                <div class="pet-avatar-large bg-white shadow-sm" style="width:50px;height:50px;font-size:1.8rem">🐾</div>
                <div>
                  <h6 class="fw-bold mb-0 text-dark">{{ activePatient.petName }}</h6>
                  <span class="small text-muted">ID: {{ activePatient.petId }}</span>
                </div>
              </div>
            </div>
            <div class="col-md-6">
              <div class="small text-muted mb-1">Chủ nuôi</div>
              <div class="fw-bold text-dark">{{ activePatient.customerName }}</div>
            </div>
          </div>

            <!-- Alerts Area (Dịch vụ) -->
          <div class="row g-3 mb-4">
            <div class="col-md-12" v-if="activePatient.allergies">
              <div class="alert bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25 rounded-4 d-flex align-items-center p-3 mb-0 shadow-sm">
                <i class="bi bi-exclamation-triangle-fill fs-4 me-3"></i>
                <div>
                  <h6 class="fw-bold mb-1 text-danger" style="letter-spacing: 0.5px;">CẢNH BÁO DỊ ỨNG</h6>
                  <p class="mb-0 small text-danger fw-semibold">{{ activePatient.allergies }}</p>
                </div>
              </div>
            </div>

            <div class="col-md-12">
              <div class="alert bg-primary bg-opacity-10 text-primary border-0 rounded-4 d-flex align-items-center p-3 mb-0">
                <i class="bi bi-info-circle-fill fs-4 me-3"></i>
                <div>
                  <h6 class="fw-bold mb-1 text-primary">DỊCH VỤ ĐẶT LỊCH</h6>
                  <p class="mb-0 small text-primary fw-semibold">Khám Bệnh Tổng Quát</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Main Two-Column Layout replaced by SOAP Tabs -->
          <div class="row g-4 mb-4">
            <div class="col-12">
              <div class="p-4 rounded-4 bg-light border shadow-sm">
                <!-- SOAP Nav Tabs -->
                <ul class="nav nav-tabs mb-4 border-bottom-0">
                  <li class="nav-item">
                    <a class="nav-link fw-bold px-4" :class="{ 'active bg-white border-bottom-0 text-primary': soapActiveTab === 'subjective', 'text-secondary': soapActiveTab !== 'subjective' }" href="#" @click.prevent="soapActiveTab = 'subjective'">
                      <i class="bi bi-person-lines-fill me-1"></i> Subjective (Chủ quan)
                    </a>
                  </li>
                  <li class="nav-item">
                    <a class="nav-link fw-bold px-4" :class="{ 'active bg-white border-bottom-0 text-primary': soapActiveTab === 'objective', 'text-secondary': soapActiveTab !== 'objective' }" href="#" @click.prevent="soapActiveTab = 'objective'">
                      <i class="bi bi-heart-pulse-fill me-1"></i> Objective (Khách quan)
                    </a>
                  </li>
                  <li class="nav-item">
                    <a class="nav-link fw-bold px-4" :class="{ 'active bg-white border-bottom-0 text-primary': soapActiveTab === 'assessment', 'text-secondary': soapActiveTab !== 'assessment' }" href="#" @click.prevent="soapActiveTab = 'assessment'">
                      <i class="bi bi-clipboard-check-fill me-1"></i> Assessment (Đánh giá)
                    </a>
                  </li>
                  <li class="nav-item">
                    <a class="nav-link fw-bold px-4" :class="{ 'active bg-white border-bottom-0 text-primary': soapActiveTab === 'plan', 'text-secondary': soapActiveTab !== 'plan' }" href="#" @click.prevent="soapActiveTab = 'plan'">
                      <i class="bi bi-journal-medical me-1"></i> Plan (Kế hoạch)
                    </a>
                  </li>
                </ul>

                <div class="tab-content bg-white p-4 rounded-4 shadow-sm border">
                  
                  <!-- SUBJECTIVE -->
                  <div v-show="soapActiveTab === 'subjective'">
                    <h6 class="fw-bold text-dark mb-4 pb-2 border-bottom"><i class="bi bi-person-lines-fill text-warning me-2"></i>Thông tin từ Chủ Nuôi (Subjective)</h6>
                    <div class="row g-3">
                      <div class="col-md-6">
                        <label class="small text-muted mb-1">Lý do đến khám (Chief Complaint)</label>
                        <input type="text" v-model="form.subjective.chiefComplaint" class="form-control" placeholder="VD: Bỏ ăn, Nôn mửa...">
                      </div>
                      <div class="col-md-6">
                        <label class="small text-muted mb-1">Thời gian phát bệnh</label>
                        <input type="text" v-model="form.subjective.onsetDuration" class="form-control" placeholder="VD: 2 ngày nay">
                      </div>
                      
                      <!-- Quick Checkboxes -->
                      <div class="col-12 mt-4"><h6 class="fw-bold small text-secondary">Tình trạng chung</h6></div>
                      <div class="col-md-3">
                        <label class="small text-muted mb-1">Ăn uống</label>
                        <select v-model="form.subjective.appetite" class="form-select form-select-sm"><option>Bình thường</option><option>Kém</option><option>Bỏ ăn</option></select>
                      </div>
                      <div class="col-md-3">
                        <label class="small text-muted mb-1">Uống nước</label>
                        <select v-model="form.subjective.thirst" class="form-select form-select-sm"><option>Bình thường</option><option>Uống nhiều</option><option>Ít uống</option></select>
                      </div>
                      <div class="col-md-3">
                        <label class="small text-muted mb-1">Mức độ hoạt động</label>
                        <select v-model="form.subjective.activityLevel" class="form-select form-select-sm"><option>Bình thường</option><option>Lừ đừ</option><option>Nằm liệt</option></select>
                      </div>
                      <div class="col-md-3">
                        <label class="small text-muted mb-1">Tiểu tiện</label>
                        <select v-model="form.subjective.urinationIssues" class="form-select form-select-sm"><option>Bình thường</option><option>Tiểu gắt</option><option>Tiểu ra máu</option><option>Bí tiểu</option></select>
                      </div>

                      <div class="col-12"><hr class="my-2 text-muted"></div>
                      
                      <div class="col-md-3">
                        <div class="form-check form-switch"><input class="form-check-input" type="checkbox" v-model="form.subjective.hasVomiting"><label class="form-check-label small fw-bold">Nôn ói</label></div>
                        <input v-if="form.subjective.hasVomiting" type="text" v-model="form.subjective.vomitingDetails" class="form-control form-control-sm mt-1" placeholder="Màu sắc, tần suất...">
                      </div>
                      <div class="col-md-3">
                        <div class="form-check form-switch"><input class="form-check-input" type="checkbox" v-model="form.subjective.hasDiarrhea"><label class="form-check-label small fw-bold">Tiêu chảy</label></div>
                        <input v-if="form.subjective.hasDiarrhea" type="text" v-model="form.subjective.diarrheaDetails" class="form-control form-control-sm mt-1" placeholder="Màu sắc, mùi...">
                      </div>
                      <div class="col-md-2">
                        <div class="form-check form-switch"><input class="form-check-input" type="checkbox" v-model="form.subjective.hasCoughing"><label class="form-check-label small fw-bold">Ho</label></div>
                        <div class="form-check form-switch mt-2"><input class="form-check-input" type="checkbox" v-model="form.subjective.hasSneezing"><label class="form-check-label small fw-bold">Hắt hơi</label></div>
                      </div>
                      <div class="col-md-2">
                        <div class="form-check form-switch"><input class="form-check-input" type="checkbox" v-model="form.subjective.hasBreathingDifficulty"><label class="form-check-label small fw-bold">Khó thở</label></div>
                        <div class="form-check form-switch mt-2"><input class="form-check-input" type="checkbox" v-model="form.subjective.hasItching"><label class="form-check-label small fw-bold">Ngứa ngáy</label></div>
                      </div>

                      <div class="col-12 mt-3">
                        <label class="small text-muted mb-1">Ghi chú thêm từ chủ nuôi</label>
                        <textarea v-model="form.subjective.petOwnerNotes" class="form-control" rows="2"></textarea>
                      </div>
                    </div>
                  </div>

                  <!-- OBJECTIVE -->
                  <div v-show="soapActiveTab === 'objective'">
                    <h6 class="fw-bold text-dark mb-4 pb-2 border-bottom"><i class="bi bi-heart-pulse-fill text-warning me-2"></i>Khám Lâm Sàng (Objective)</h6>
                    <div class="row g-3">
                      <div class="col-md-3">
                        <label class="small text-muted mb-1">Cân nặng (kg) <span class="text-danger">*</span></label>
                        <input type="number" step="0.1" v-model="form.objective.weight" class="form-control">
                      </div>
                      <div class="col-md-3">
                        <label class="small text-muted mb-1">Nhiệt độ (°C) <span class="text-danger">*</span></label>
                        <input type="number" step="0.1" v-model="form.objective.temperature" class="form-control">
                      </div>
                      <div class="col-md-3">
                        <label class="small text-muted mb-1">Nhịp tim (bpm)</label>
                        <input type="number" v-model="form.objective.heartRate" class="form-control">
                      </div>
                      <div class="col-md-3">
                        <label class="small text-muted mb-1">Nhịp thở (lần/phút)</label>
                        <input type="number" v-model="form.objective.respiratoryRate" class="form-control">
                      </div>
                      
                      <div class="col-md-4">
                        <label class="small text-muted mb-1">Tình trạng cơ thể (BCS 1-9)</label>
                        <input type="range" class="form-range" min="1" max="9" v-model="form.objective.bodyConditionScore">
                        <div class="text-center fw-bold text-primary">{{ form.objective.bodyConditionScore }} / 9</div>
                      </div>
                      <div class="col-md-4">
                        <label class="small text-muted mb-1">Tri giác (Mentation)</label>
                        <select v-model="form.objective.mentation" class="form-select"><option>Tỉnh táo</option><option>Lừ đừ</option><option>Hôn mê</option></select>
                      </div>
                      <div class="col-md-4">
                        <label class="small text-muted mb-1">Tình trạng mất nước</label>
                        <select v-model="form.objective.hydration" class="form-select"><option>&lt; 5% (Bình thường)</option><option>5-7% (Nhẹ)</option><option>8-10% (Vừa)</option><option>&gt; 10% (Nặng)</option></select>
                      </div>

                      <div class="col-12 mt-4"><h6 class="fw-bold small text-secondary">Khám Từng Hệ Cơ Quan</h6></div>
                      <div class="col-md-6" v-for="(exam, key) in { eyes: 'Mắt', ears: 'Tai', nose: 'Mũi', mouth: 'Miệng/Răng', skinCoat: 'Da lông', gastrointestinal: 'Tiêu hóa', respiratory: 'Hô hấp' }" :key="key">
                        <div class="d-flex justify-content-between align-items-center mb-1">
                          <span class="small fw-bold">{{ exam }}</span>
                          <div class="form-check form-switch"><input class="form-check-input" type="checkbox" v-model="(form.objective as any)[key].isNormal"><label class="form-check-label small" :class="{'text-success': (form.objective as any)[key].isNormal, 'text-danger': !(form.objective as any)[key].isNormal}">{{ (form.objective as any)[key].isNormal ? 'Bình thường' : 'Bất thường' }}</label></div>
                        </div>
                        <input v-if="!(form.objective as any)[key].isNormal" type="text" v-model="(form.objective as any)[key].note" class="form-control form-control-sm" placeholder="Ghi chú triệu chứng...">
                      </div>

                      <!-- ===== HÌNH ẢNH CẬN LÂM SÀNG ===== -->
                      <div class="col-12 mt-4">
                        <div class="d-flex align-items-center gap-2 mb-3">
                          <i class="bi bi-camera-fill text-primary fs-5"></i>
                          <h6 class="fw-bold text-dark mb-0">Hình Ảnh Cận Lâm Sàng</h6>
                          <span class="badge bg-primary bg-opacity-10 text-primary small ms-1">X-Quang · Siêu âm · Ảnh lâm sàng</span>
                        </div>

                        <!-- Drag & Drop Zone -->
                        <div
                          class="upload-dropzone rounded-4 text-center p-4 position-relative"
                          :class="{ 'dragging': isDragging }"
                          @dragover.prevent="isDragging = true"
                          @dragleave.prevent="isDragging = false"
                          @drop.prevent="onFileDrop"
                          @click="triggerFileInput"
                        >
                          <input
                            ref="fileInputRef"
                            type="file"
                            multiple
                            accept="image/jpeg,image/png,image/gif"
                            class="d-none"
                            @change="onFileSelected"
                          />
                          <div v-if="selectedFiles.length === 0" class="py-2">
                            <i class="bi bi-cloud-arrow-up-fill text-primary opacity-50" style="font-size:2.5rem"></i>
                            <p class="fw-bold text-secondary mb-1 mt-2">Kéo &amp; thả ảnh vào đây</p>
                            <p class="small text-muted mb-0">hoặc <span class="text-primary fw-bold">bấm để chọn ảnh</span> · JPG, PNG, GIF · Tối đa 5MB/ảnh</p>
                          </div>
                          <div v-else class="d-flex align-items-center gap-2 flex-wrap justify-content-center">
                            <span class="text-success fw-bold"><i class="bi bi-check-circle-fill me-1"></i>{{ selectedFiles.length }} ảnh đã chọn</span>
                            <span class="text-muted small">· Bấm để thêm ảnh khác</span>
                          </div>
                        </div>

                        <!-- Preview Grid -->
                        <div v-if="previewUrls.length > 0" class="mt-3">
                          <div class="d-flex flex-wrap gap-2">
                            <div
                              v-for="(url, idx) in previewUrls"
                              :key="idx"
                              class="img-preview-wrapper position-relative"
                            >
                              <img :src="url" class="img-preview rounded-3 shadow-sm" @click.stop="openLightbox(previewUrls, idx)" title="Bấm để xem ảnh to" />
                              <button
                                type="button"
                                class="btn-remove-img position-absolute top-0 end-0"
                                @click.stop="removePreviewImage(idx)"
                              >
                                <i class="bi bi-x-lg"></i>
                              </button>
                            </div>
                          </div>
                          <p class="text-muted small mt-2 mb-0"><i class="bi bi-info-circle me-1"></i>Ảnh sẽ được tải lên khi bạn nhấn "Hoàn thành &amp; Lưu bệnh án"</p>
                        </div>
                      </div>
                      <!-- ===== END HÌNH ẢNH CẬN LÂM SÀNG ===== -->

                    </div>
                  </div>

                  <!-- ASSESSMENT -->
                  <div v-show="soapActiveTab === 'assessment'">
                    <h6 class="fw-bold text-dark mb-4 pb-2 border-bottom"><i class="bi bi-clipboard-check-fill text-warning me-2"></i>Chẩn Đoán (Assessment)</h6>
                    <div class="row g-3">
                      <div class="col-md-6">
                        <label class="small text-muted mb-1">Chẩn đoán sơ bộ (Tentative) <span class="text-danger">*</span></label>
                        <input type="text" v-model="form.assessment.tentativeDiagnosis" class="form-control fw-bold text-primary">
                      </div>
                      <div class="col-md-6">
                        <label class="small text-muted mb-1">Chẩn đoán xác định (Definitive)</label>
                        <input type="text" v-model="form.assessment.definitiveDiagnosis" class="form-control fw-bold text-success">
                      </div>
                      <div class="col-12">
                        <label class="small text-muted mb-1">Chẩn đoán phân biệt (Differential)</label>
                        <textarea v-model="form.assessment.differentialDiagnosis" class="form-control" rows="2"></textarea>
                      </div>
                      <div class="col-md-6">
                        <label class="small text-muted mb-1">Mức độ bệnh</label>
                        <select v-model="form.assessment.diseaseSeverity" class="form-select"><option>Nhẹ</option><option>Trung bình</option><option>Nặng</option><option>Nguy kịch</option></select>
                      </div>
                      <div class="col-md-6">
                        <label class="small text-muted mb-1">Tiên lượng (Prognosis)</label>
                        <select v-model="form.assessment.prognosis" class="form-select"><option>Tốt</option><option>Dè dặt</option><option>Xấu</option></select>
                      </div>
                    </div>
                  </div>

                  <!-- PLAN -->
                  <div v-show="soapActiveTab === 'plan'">
                    <h6 class="fw-bold text-dark mb-4 pb-2 border-bottom"><i class="bi bi-journal-medical text-warning me-2"></i>Kế Hoạch Điều Trị (Plan)</h6>
                    <div class="row g-4">
                      <!-- Prescriptions -->
                      <div class="col-12">
                        <div class="d-flex justify-content-between align-items-center mb-3">
                          <h6 class="fw-bold text-dark mb-0"><i class="bi bi-capsule-pill text-success me-2"></i> Kê đơn thuốc</h6>
                          <button type="button" class="btn btn-sm btn-success rounded-pill px-3" @click="addPrescriptionLine"><i class="bi bi-plus-lg me-1"></i> Thêm thuốc</button>
                        </div>
                        <div v-if="form.plan.prescriptions.length === 0" class="text-center text-muted small py-3 bg-light rounded-4 border border-dashed">Chưa có thuốc được chỉ định</div>
                        <div v-else class="pe-2 overflow-auto" style="max-height: 450px; overflow-x: hidden;">
                          <div class="row g-3">
                            <div v-for="(pres, idx) in form.plan.prescriptions" :key="idx" class="col-md-6">
                              <div class="bg-light p-3 pt-4 rounded-4 border position-relative h-100">
                                <button type="button" class="btn-close position-absolute top-0 end-0 m-2 bg-white shadow-sm border" style="z-index: 10;" @click="removePrescriptionLine(idx)"></button>
                                <div class="position-relative mb-2 w-100">
                                  <div class="input-group input-group-sm">
                                    <span class="input-group-text bg-white"><i class="bi bi-search text-muted"></i></span>
                                    <input type="text" class="form-control fw-bold" placeholder="Tìm kiếm thuốc (chỉ hiển thị còn hàng)..." v-model="pres.searchQuery" @focus="pres.showDropdown = true" @blur="hideDropdown(pres)" @input="handleSearchInput(pres)" />
                                  </div>
                                  <div v-if="pres.showDropdown && filteredMedicines(pres.searchQuery).length > 0" class="position-absolute w-100 bg-white border rounded shadow-lg mt-1" style="max-height: 250px; overflow-y: auto; z-index: 1050;">
                                    <div v-for="med in filteredMedicines(pres.searchQuery)" :key="med.id" class="px-3 py-2 border-bottom autocomplete-item cursor-pointer" @mousedown.prevent="selectMedicine(idx, med)">
                                      <div class="d-flex justify-content-between align-items-center">
                                        <div class="fw-bold text-dark">{{ med.name }}</div>
                                        <span class="badge bg-success rounded-pill">Tồn: {{ med.stockQuantity }}</span>
                                      </div>
                                      <div class="small text-muted mt-1">{{ med.unit }} - {{ med.sellPrice ? med.sellPrice.toLocaleString() : 0 }}đ</div>
                                    </div>
                                  </div>
                                  <div v-if="pres.showDropdown && pres.searchQuery && filteredMedicines(pres.searchQuery).length === 0" class="position-absolute w-100 bg-white border rounded shadow-lg mt-1 px-3 py-3 small text-center text-muted" style="z-index: 1050;">
                                    <i class="bi bi-x-circle d-block fs-4 text-danger mb-2"></i>
                                    Không tìm thấy thuốc hoặc đã hết hàng.
                                  </div>
                                </div>
                                <div class="row g-2">
                                  <div class="col-4"><label class="small text-muted" style="font-size:0.7rem">Số lượng</label><input type="number" min="1" v-model.number="pres.quantity" class="form-control form-control-sm"></div>
                                  <div class="col-8"><label class="small text-muted" style="font-size:0.7rem">Liều lượng</label><input type="text" v-model="pres.dosage" class="form-control form-control-sm"></div>
                                  <div class="col-12"><label class="small text-muted" style="font-size:0.7rem">Cách dùng</label><input type="text" v-model="pres.frequency" class="form-control form-control-sm"></div>
                                </div>
                                <div v-if="pres.medicineId" class="mt-2 text-end">
                                  <span v-if="pres.stockQuantity === 0" class="text-danger small fw-bold"><i class="bi bi-x-circle"></i> Hết hàng</span>
                                  <span v-else-if="pres.quantity > pres.stockQuantity" class="text-danger small fw-bold"><i class="bi bi-exclamation-triangle"></i> Kho không đủ ({{ pres.stockQuantity }})</span>
                                  <span v-else class="text-success small fw-semibold"><i class="bi bi-check2-circle"></i> Tồn kho: {{ pres.stockQuantity }}</span>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                      
                      <div class="col-md-6">
                        <label class="small fw-bold text-secondary">Hướng xử lý (Treatment Directions)</label>
                        <div class="d-flex gap-2 mt-2 flex-wrap">
                          <label class="btn btn-sm rounded-pill" :class="form.plan.treatmentDirections.includes('Truyền dịch') ? 'btn-primary' : 'btn-outline-primary'">
                            <input type="checkbox" class="d-none" value="Truyền dịch" v-model="form.plan.treatmentDirections"> Truyền dịch
                          </label>
                          <label class="btn btn-sm rounded-pill" :class="form.plan.treatmentDirections.includes('Tiêm kháng sinh') ? 'btn-primary' : 'btn-outline-primary'">
                            <input type="checkbox" class="d-none" value="Tiêm kháng sinh" v-model="form.plan.treatmentDirections"> Tiêm kháng sinh
                          </label>
                          <label class="btn btn-sm rounded-pill" :class="form.plan.treatmentDirections.includes('Phẫu thuật') ? 'btn-primary' : 'btn-outline-primary'">
                            <input type="checkbox" class="d-none" value="Phẫu thuật" v-model="form.plan.treatmentDirections"> Phẫu thuật
                          </label>
                          <label class="btn btn-sm rounded-pill" :class="form.plan.treatmentDirections.includes('Xét nghiệm máu') ? 'btn-primary' : 'btn-outline-primary'">
                            <input type="checkbox" class="d-none" value="Xét nghiệm máu" v-model="form.plan.treatmentDirections"> Xét nghiệm máu
                          </label>
                        </div>
                      </div>
                      <div class="col-md-6">
                        <label class="small fw-bold text-secondary">Dặn dò chăm sóc (Care Instructions)</label>
                        <textarea v-model="form.plan.careInstructions" class="form-control mt-1" rows="2" placeholder="Chế độ ăn, vệ sinh..."></textarea>
                      </div>
                    </div>
                  </div>

                </div>
              </div>
            </div>
          </div>

          <!-- Bottom Row: Followup / Next Steps -->
          <div class="row g-4 mb-4">
            <div class="col-12">
              <div class="p-4 rounded-4 bg-primary bg-opacity-10 border border-primary border-opacity-25 shadow-sm">
                <div class="d-flex align-items-center mb-3">
                  <div class="form-check form-switch fs-5 me-3">
                    <input class="form-check-input cursor-pointer" type="checkbox" role="switch" id="switchFollowUp" v-model="form.plan.createFollowUpAppointment">
                  </div>
                  <label class="form-check-label fw-bold text-primary mb-0 cursor-pointer" for="switchFollowUp">
                    <i class="bi bi-calendar2-check-fill me-1"></i> Đặt lịch tự động cho lần khám tiếp theo
                  </label>
                </div>
                
                <div v-if="form.plan.createFollowUpAppointment" class="row g-3 bg-white p-3 rounded-4 shadow-sm mt-2">
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Loại lịch hẹn <span class="text-danger">*</span></label>
                    <select v-model="form.plan.followUpType" class="form-select rounded-pill border-primary" required>
                      <option value="FollowUp">Tái khám</option>
                      <option value="Revaccination">Tái tiêm</option>
                    </select>
                  </div>
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Ngày tái khám <span class="text-danger">*</span></label>
                    <input type="date" v-model="followUpDateOnly" @change="onFollowUpDateChange" :min="minDateOnlyStr" class="form-control rounded-pill border-primary" required>
                  </div>
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Ghi chú (Tùy chọn)</label>
                    <input type="text" v-model="form.plan.followUpNote" class="form-control rounded-pill border-primary" placeholder="VD: Nhớ mang theo sổ khám...">
                  </div>
                  
                  <div class="col-12 mt-2" v-if="followUpDateOnly">
                    <label class="form-label small fw-bold text-secondary mb-2">Chọn khung giờ trống <span class="text-danger">*</span></label>
                    
                    <div v-if="fetchingFollowUpSlots" class="text-primary small mb-2"><span class="spinner-border spinner-border-sm me-1"></span> Đang tải khung giờ...</div>
                    <div v-else-if="availableFollowUpSlots.length === 0" class="alert alert-warning small py-2 mb-0 d-flex align-items-center"><i class="bi bi-exclamation-triangle-fill me-2"></i> Không có khung giờ làm việc nào trống trong ngày này.</div>
                    <div v-else>
                        <div class="mb-3">
                          <span class="small fw-bold text-muted d-flex align-items-center mb-2"><i class="bi bi-brightness-alt-high me-1"></i> Buổi Sáng:</span>
                          <div class="d-flex flex-wrap gap-2">
                            <button type="button" v-for="slot in displayFollowUpMorningSlots" :key="slot.time" 
                               class="time-slot-btn"
                               :class="{
                                 'slot-selected': followUpTimeOnly === slot.time,
                                 'slot-past': slot.isPast,
                                 'slot-too-soon': slot.isTooSoon,
                                 'slot-booked': slot.isBooked,
                                 'slot-available': slot.isAvailable
                               }"
                               :disabled="!slot.isAvailable"
                               @click="slot.isAvailable && (followUpTimeOnly = slot.time)">
                              <span class="slot-time-text">{{ slot.time }}</span>
                              <i v-if="followUpTimeOnly === slot.time" class="bi bi-check-circle-fill text-white ms-1 position-absolute top-0 start-100 translate-middle" style="font-size: 1.1rem; background: #0d6efd; border-radius: 50%;"></i>
                              <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                              <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                              <span v-else-if="slot.isBooked" class="slot-badge-label">Đã hết</span>
                            </button>
                          </div>
                        </div>
                        <div class="mb-3">
                          <span class="small fw-bold text-muted d-flex align-items-center mb-2"><i class="bi bi-brightness-alt-low me-1"></i> Buổi Chiều:</span>
                          <div class="d-flex flex-wrap gap-2">
                            <button type="button" v-for="slot in displayFollowUpAfternoonSlots" :key="slot.time" 
                               class="time-slot-btn"
                               :class="{
                                 'slot-selected': followUpTimeOnly === slot.time,
                                 'slot-past': slot.isPast,
                                 'slot-too-soon': slot.isTooSoon,
                                 'slot-booked': slot.isBooked,
                                 'slot-available': slot.isAvailable
                               }"
                               :disabled="!slot.isAvailable"
                               @click="slot.isAvailable && (followUpTimeOnly = slot.time)">
                      <span class="slot-time-text">{{ slot.time }}</span>
                              <i v-if="followUpTimeOnly === slot.time" class="bi bi-check-circle-fill text-white ms-1 position-absolute top-0 start-100 translate-middle" style="font-size: 1.1rem; background: #0d6efd; border-radius: 50%;"></i>
                              <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                              <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                              <span v-else-if="slot.isBooked" class="slot-badge-label">Đã hết</span>
                              </button>
                            </div>
                          </div>
                          <div class="mb-3">
                            <span class="small fw-bold text-muted d-flex align-items-center mb-2"><i class="bi bi-moon-stars-fill me-1"></i> Buổi Tối:</span>
                            <div class="d-flex flex-wrap gap-2">
                              <button type="button" v-for="slot in displayFollowUpEveningSlots" :key="slot.time" 
                                 class="time-slot-btn"
                                 :class="{
                                   'slot-selected': followUpTimeOnly === slot.time,
                                   'slot-past': slot.isPast,
                                   'slot-too-soon': slot.isTooSoon,
                                   'slot-booked': slot.isBooked,
                                   'slot-available': slot.isAvailable
                                 }"
                                 :disabled="!slot.isAvailable"
                                 @click="slot.isAvailable && (followUpTimeOnly = slot.time)">
                                <span class="slot-time-text">{{ slot.time }}</span>
                                <i v-if="followUpTimeOnly === slot.time" class="bi bi-check-circle-fill text-white ms-1 position-absolute top-0 start-100 translate-middle" style="font-size: 1.1rem; background: #0d6efd; border-radius: 50%;"></i>
                                <span v-if="slot.isPast" class="slot-badge-label">Đã qua</span>
                                <span v-else-if="slot.isTooSoon" class="slot-badge-label">Quá gần</span>
                                <span v-else-if="slot.isBooked" class="slot-badge-label">Đã hết</span>
                              </button>
                            </div>
                          </div>
                        <div class="mt-3 pt-3 border-top">
                          <div class="d-flex flex-wrap gap-3 justify-content-center">
                            <span class="d-flex align-items-center gap-2 small fw-semibold"><span style="width:14px;height:14px;border-radius:4px;background:#fff;border:1.5px solid #dee2e6;display:inline-block"></span> <span class="text-muted">Trống</span></span>
                            <span class="d-flex align-items-center gap-2 small fw-semibold"><span style="width:14px;height:14px;border-radius:4px;background:#f8f9fa;border:1.5px solid #e9ecef;display:inline-block"></span> <span class="text-muted">Đã qua</span></span>
                            <span class="d-flex align-items-center gap-2 small fw-semibold"><span style="width:14px;height:14px;border-radius:4px;background:#fff8ec;border:1.5px solid #ffc107;display:inline-block"></span> <span class="text-muted">Quá gần</span></span>
                            <span class="d-flex align-items-center gap-2 small fw-semibold"><span style="width:14px;height:14px;border-radius:4px;background:#fff5f5;border:1.5px solid #fca5a5;display:inline-block"></span> <span class="text-muted">Đã hết</span></span>
                          </div>
                        </div>
                     </div>
                  </div>
                </div>
                <div v-else class="alert alert-light py-2 px-3 mb-0 rounded-pill small w-100 border-0 shadow-sm d-flex align-items-center">
                  <i class="bi bi-info-circle-fill text-muted me-2"></i> Bệnh án SOAP sẽ được lưu nguyên trạng vào hệ thống mà không sinh thêm lịch hẹn.
                </div>
              </div>
            </div>
          </div>

          <!-- Error Banner -->
          <div v-if="errorMessage" class="alert alert-danger rounded-4 p-3 small mb-4 shadow-sm border-0 d-flex align-items-center">
            <i class="bi bi-exclamation-circle-fill fs-5 me-2"></i> {{ errorMessage }}
          </div>

          <!-- Actions -->
          <div class="d-flex justify-content-end align-items-center border-top pt-4">
            <button type="submit" class="btn btn-premium rounded-pill px-5 fw-bold text-white shadow-lg btn-save" :disabled="submitting || hasStockDeficit">
              <span v-if="submitting" class="spinner-border spinner-border-sm me-1"></span>
              <i class="bi bi-check2-all me-1"></i> Hoàn Thành & Lưu Bệnh Án
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Tab 2: Hồ Sơ Thú Cưng -->
    <div v-if="internalTab === 'pet-history'" class="flex-grow-1">
      <div class="row g-4">
        <!-- Info Card -->
        <div class="col-lg-4">
          <div class="card border-0 shadow-sm rounded-4 p-4 bg-white mb-4 card-gradient-pet text-dark h-100 border">
            <h6 class="fw-bold mb-4 border-bottom pb-3 text-dark"><i class="bi bi-info-circle-fill text-warning me-2"></i>Thông tin tổng quan</h6>
            <div v-if="activePatient.petId" class="pet-info-grid">
              <div class="d-flex align-items-center gap-3 mb-4">
                <span class="pet-avatar-large shadow-sm">🐾</span>
                <div>
                  <h4 class="fw-bold text-dark mb-0">{{ activePatient.petName }}</h4>
                  <span class="badge bg-white text-secondary border small mt-1 shadow-sm">Mã Hồ Sơ: {{ activePatient.petId }}</span>
                </div>
              </div>
              <div class="bg-white rounded-4 p-3 shadow-sm border">
                <ul class="list-unstyled mb-0 small line-height-lg">
                  <li class="mb-2 d-flex justify-content-between border-bottom pb-2"><span class="text-muted">Chủ nuôi:</span> <strong class="text-dark">{{ activePatient.customerName }}</strong></li>
                  <li class="mb-2 d-flex justify-content-between border-bottom pb-2"><span class="text-muted">ID Cuộc hẹn:</span> <strong class="text-dark">#{{ activePatient.appointmentId }}</strong></li>
                  <li v-if="activePatient.species" class="mb-2 d-flex justify-content-between border-bottom pb-2"><span class="text-muted">Giống/Loài:</span> <strong class="text-dark">{{ activePatient.species }} <span v-if="activePatient.breed">({{ activePatient.breed }})</span></strong></li>
                  <li v-if="activePatient.gender" class="mb-2 d-flex justify-content-between border-bottom pb-2"><span class="text-muted">Giới tính:</span> <strong class="text-dark">{{ activePatient.gender }}</strong></li>
                  <li v-if="activePatient.age || activePatient.weight" class="mb-2 d-flex justify-content-between border-bottom pb-2">
                    <span class="text-muted">Tuổi / Cân nặng:</span> 
                    <strong class="text-dark">
                      <span v-if="activePatient.age">{{ activePatient.age }} tuổi</span>
                      <span v-if="activePatient.age && activePatient.weight"> - </span>
                      <span v-if="activePatient.weight">{{ activePatient.weight }} kg</span>
                    </strong>
                  </li>
                  <li class="d-flex justify-content-between"><span class="text-muted">Trạng thái:</span> <span class="badge bg-warning text-dark">Đang tiến hành khám</span></li>
                </ul>
              </div>
            </div>
            <div v-else class="text-center py-5 text-muted small">
              Chưa có thông tin thú cưng hoạt động.
            </div>
          </div>
        </div>

        <!-- History Timeline -->
        <div class="col-lg-8">
          <div class="card border-0 shadow-sm rounded-4 p-4 bg-white h-100 border">
            <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 pb-3 border-bottom">
              <h6 class="fw-bold mb-0 text-dark"><i class="bi bi-clock-history text-warning me-2"></i>Lịch sử khám & Điều trị (Timeline)</h6>
              
              <!-- Filters -->
              <div class="d-flex flex-wrap gap-3 mt-3 mt-sm-0 align-items-center" v-if="medicalHistory.length > 0">
                <!-- Date Filter -->
                <div class="position-relative filter-wrapper">
                  <label class="position-absolute text-muted small px-2 bg-white" style="top: -8px; left: 16px; font-size: 0.7rem; z-index: 5; border-radius: 10px;">Lọc theo ngày</label>
                  <div class="input-group shadow-sm rounded-pill overflow-hidden transition-all filter-group" style="width: 200px; height: 38px;">
                    <span class="input-group-text border-0 bg-transparent text-primary ps-3 pe-2"><i class="bi bi-calendar2-check-fill"></i></span>
                    <input type="date" v-model="historyDateFilter" class="form-control border-0 bg-transparent text-dark fw-medium custom-date-input px-1" style="outline: none; box-shadow: none;">
                    <button v-if="historyDateFilter" class="btn btn-link border-0 text-danger text-decoration-none px-2 d-flex align-items-center justify-content-center" @click="historyDateFilter = ''" title="Xóa lọc">
                      <i class="bi bi-x-circle-fill"></i>
                    </button>
                  </div>
                </div>

                <!-- Type Filter -->
                <div class="position-relative filter-wrapper">
                  <label class="position-absolute text-muted small px-2 bg-white" style="top: -8px; left: 16px; font-size: 0.7rem; z-index: 5; border-radius: 10px;">Loại phiếu</label>
                  <div class="input-group shadow-sm rounded-pill overflow-hidden transition-all filter-group" style="width: 150px; height: 38px;">
                    <span class="input-group-text border-0 bg-transparent text-primary ps-3 pe-2"><i class="bi bi-funnel-fill"></i></span>
                    <select v-model="historyTypeFilter" class="form-select border-0 bg-transparent text-dark fw-medium custom-select px-1" style="outline: none; box-shadow: none; cursor: pointer;">
                      <option value="ALL">Tất cả</option>
                      <option value="Consultation">Khám bệnh</option>
                      <option value="Vaccination">Tiêm phòng</option>
                    </select>
                  </div>
                </div>
              </div>
            </div>
            
            <div v-if="loadingHistory" class="text-center py-5">
              <div class="spinner-border text-warning" role="status"></div>
              <p class="text-muted small mt-3">Đang tải bệnh sử từ hệ thống...</p>
            </div>

            <div v-else-if="filteredMedicalHistory.length === 0" class="text-center py-5 text-muted small">
              <i class="bi bi-search fs-1 d-block mb-3 text-black-50 opacity-25"></i>
              Không tìm thấy hồ sơ nào phù hợp với bộ lọc.
            </div>

            <div v-else class="medical-history-timeline pe-2 overflow-auto mt-2" style="max-height: 650px;">
              <div class="timeline-container">
                <div v-for="record in paginatedHistory" :key="record.recordId" class="timeline-item position-relative ps-4 pb-4">
                  <!-- Timeline dot -->
                  <div class="timeline-line"></div>
                  <div class="timeline-circle shadow-sm" :class="record.recordType === 'Vaccination' ? 'bg-success border-success' : 'bg-primary border-primary'"></div>
                  
                  <!-- Premium Card -->
                  <div class="timeline-content bg-white border border-light rounded-4 shadow-sm p-4 transition-all hover-lift">
                    <div class="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
                       <span class="visit-date fw-bold text-dark fs-6 d-flex align-items-center">
                         <i class="bi bi-calendar-check text-warning me-2 fs-5"></i>{{ formatDate(record.visitDate) }}
                       </span>
                       <span class="badge rounded-pill px-3 py-1 fw-bold border" :class="record.recordType === 'Vaccination' ? 'bg-success bg-opacity-10 text-success border-success' : 'bg-primary bg-opacity-10 text-primary border-primary'">
                         {{ record.recordType === 'Vaccination' ? 'Tiêm phòng' : 'Khám bệnh' }}
                       </span>
                    </div>
                    
                    <div class="bg-light rounded-3 p-3 mb-3 d-flex flex-column gap-2">
                       <div class="text-muted small d-flex align-items-center">
                         <i class="bi bi-person-badge text-secondary me-2 fs-6"></i> Bác sĩ phụ trách: <strong class="text-dark ms-1">{{ record.doctorName || 'Chưa rõ' }}</strong>
                       </div>
                       <div v-if="record.diagnosis" class="text-muted small d-flex align-items-center">
                         <i class="bi bi-activity text-danger me-2 fs-6"></i> Chẩn đoán: <span class="text-dark ms-1 text-truncate" style="max-width: 300px;">{{ getShortDiagnosis(record.diagnosis) }}</span>
                       </div>
                    </div>
                    
                    <div class="d-flex justify-content-end">
                       <button class="btn btn-sm btn-outline-primary bg-gradient rounded-pill px-4 fw-bold shadow-sm d-flex align-items-center" @click="openSoapDetail(record)">
                         <i class="bi bi-file-earmark-medical-fill me-2"></i> Xem Bệnh Án
                       </button>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Pagination Controls -->
              <div class="d-flex justify-content-between align-items-center mt-3" v-if="totalPages > 1">
                <span class="small text-muted">Đang xem <strong>{{ (currentPage - 1) * recordsPerPage + 1 }}</strong> - <strong>{{ Math.min(currentPage * recordsPerPage, filteredMedicalHistory.length) }}</strong> trong số <strong>{{ filteredMedicalHistory.length }}</strong> kết quả</span>
                <ul class="pagination pagination-sm mb-0 shadow-sm overflow-hidden">
                  <li class="page-item" :class="{ disabled: currentPage === 1 }">
                    <a class="page-link text-primary fw-bold" href="#" @click.prevent="currentPage--"><i class="bi bi-chevron-left"></i></a>
                  </li>
                  <li class="page-item" v-for="p in totalPages" :key="p" :class="{ active: p === currentPage }">
                    <a class="page-link bg-primary border-primary text-white fw-bold" v-if="p === currentPage" href="#">{{ p }}</a>
                    <a class="page-link text-secondary fw-bold" v-else href="#" @click.prevent="currentPage = p">{{ p }}</a>
                  </li>
                  <li class="page-item" :class="{ disabled: currentPage === totalPages }">
                    <a class="page-link text-primary fw-bold" href="#" @click.prevent="currentPage++"><i class="bi bi-chevron-right"></i></a>
                  </li>
                </ul>
              </div>

            </div>

          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- ===== PREMIUM SOAP MODAL ===== -->
  <div v-if="showSoapModal && selectedSoapRecord" class="zalo-modal-overlay" style="z-index: 1055;" @click.self="closeSoapModal">
    <div class="zalo-modal-card modal-lg max-w-700 bg-light border-0 shadow-lg" style="animation: fadeUp 0.3s ease-out; margin-top: 5vh; margin-bottom: 5vh; max-height: 90vh; display: flex; flex-direction: column;">
      <div class="zalo-modal-header bg-white border-bottom border-primary border-opacity-25 p-4 flex-shrink-0 d-flex justify-content-between align-items-center" style="background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px);">
        <h5 class="modal-title fw-bold text-primary mb-0 d-flex align-items-center">
          <div class="bg-primary bg-opacity-10 p-2 rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 45px; height: 45px;"><i class="bi bi-file-earmark-medical-fill fs-4"></i></div>
          Hồ Sơ Bệnh Án {{ selectedSoapRecord.recordType === 'Vaccination' ? 'Tiêm Phòng' : 'Khám Bệnh' }}
        </h5>
        <button class="btn-close shadow-none" @click="closeSoapModal"></button>
      </div>
      
      <div class="zalo-modal-body p-4 flex-grow-1 overflow-auto bg-light">
        <!-- Vitals -->
        <div v-if="selectedSoapRecord.weight || selectedSoapRecord.temperature" class="row g-2 mb-4">
          <div v-if="selectedSoapRecord.weight" class="col-auto">
            <span class="badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25 px-3 py-2 rounded-pill">
              <i class="bi bi-clipboard2-pulse me-1"></i>Cân nặng: <strong class="fs-6">{{ selectedSoapRecord.weight }} kg</strong>
            </span>
          </div>
          <div v-if="selectedSoapRecord.temperature" class="col-auto">
            <span class="badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25 px-3 py-2 rounded-pill">
              <i class="bi bi-thermometer-half me-1"></i>Nhiệt độ: <strong class="fs-6">{{ selectedSoapRecord.temperature }} °C</strong>
            </span>
          </div>
        </div>

        <div class="row g-4">
          <!-- Medical History (Subjective) -->
          <div class="col-md-6">
            <div class="h-100 p-4 bg-white rounded-4 shadow-sm border border-secondary border-opacity-10 position-relative">
              <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-dark shadow-sm" style="width: 32px; height: 32px; line-height: 22px; font-size: 1.1rem;">S</span>
              <h6 class="fw-bold text-dark mb-3 ms-2 border-bottom pb-2"><i class="bi bi-person-lines-fill text-secondary me-1"></i> Bệnh sử (Subjective)</h6>
              <div v-if="safeParseJSON(selectedSoapRecord.medicalHistory)" class="small text-dark mt-2">
                <div class="mb-2" v-if="safeParseJSON(selectedSoapRecord.medicalHistory).chiefComplaint"><span class="text-muted fw-bold">Lý do khám:</span> <span class="text-primary fw-bold">{{safeParseJSON(selectedSoapRecord.medicalHistory).chiefComplaint}}</span></div>
                <div class="mb-1" v-if="safeParseJSON(selectedSoapRecord.medicalHistory).appetite"><span class="text-muted">Ăn uống:</span> {{safeParseJSON(selectedSoapRecord.medicalHistory).appetite}}</div>
                <div class="mb-1" v-if="safeParseJSON(selectedSoapRecord.medicalHistory).urinationIssues"><span class="text-muted">Tiêu tiểu:</span> {{safeParseJSON(selectedSoapRecord.medicalHistory).urinationIssues}}</div>
                <div class="mb-1" v-if="safeParseJSON(selectedSoapRecord.medicalHistory).activityLevel"><span class="text-muted">Hoạt động:</span> {{safeParseJSON(selectedSoapRecord.medicalHistory).activityLevel}}</div>
                <div class="mt-3 p-2 bg-light rounded-3" v-if="safeParseJSON(selectedSoapRecord.medicalHistory).petOwnerNotes"><span class="text-muted fw-bold d-block mb-1"><i class="bi bi-chat-left-text me-1"></i>Ghi chú:</span> <span class="fst-italic">{{safeParseJSON(selectedSoapRecord.medicalHistory).petOwnerNotes}}</span></div>
              </div>
              <div v-else class="small mt-3">
                <div v-if="!selectedSoapRecord.medicalHistory" class="text-muted fst-italic">Không ghi nhận</div>
                <div v-else v-for="(item, idx) in parseRawString(selectedSoapRecord.medicalHistory)" :key="idx" class="d-flex align-items-start mb-2 bg-light p-2 rounded-3 border">
                  <i class="bi bi-caret-right-fill text-secondary me-2 mt-1" style="font-size: 0.75rem;"></i>
                  <span class="text-dark lh-base" v-html="formatRawItem(item)"></span>
                </div>
              </div>
            </div>
          </div>

          <!-- Clinical Signs (Objective) -->
          <div class="col-md-6">
            <div class="h-100 p-4 bg-white rounded-4 shadow-sm border border-secondary border-opacity-10 position-relative">
              <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-info shadow-sm" style="width: 32px; height: 32px; line-height: 22px; font-size: 1.1rem;">O</span>
              <h6 class="fw-bold text-dark mb-3 ms-2 border-bottom pb-2"><i class="bi bi-heart-pulse-fill text-info me-1"></i> Khám lâm sàng (Objective)</h6>
              <div v-if="safeParseJSON(selectedSoapRecord.clinicalSigns)" class="small text-dark mt-2">
                <div class="d-flex flex-wrap gap-2 mb-2">
                  <span class="badge bg-light text-dark border" v-if="safeParseJSON(selectedSoapRecord.clinicalSigns).heartRate">Nhịp tim: <strong>{{safeParseJSON(selectedSoapRecord.clinicalSigns).heartRate}} bpm</strong></span>
                  <span class="badge bg-light text-dark border" v-if="safeParseJSON(selectedSoapRecord.clinicalSigns).respiratoryRate">Nhịp thở: <strong>{{safeParseJSON(selectedSoapRecord.clinicalSigns).respiratoryRate}} l/p</strong></span>
                  <span class="badge bg-light text-dark border" v-if="safeParseJSON(selectedSoapRecord.clinicalSigns).bodyConditionScore">BCS: <strong>{{safeParseJSON(selectedSoapRecord.clinicalSigns).bodyConditionScore}}/9</strong></span>
                </div>
                <div class="mb-1" v-if="safeParseJSON(selectedSoapRecord.clinicalSigns).mentation"><span class="text-muted">Tinh thần:</span> {{safeParseJSON(selectedSoapRecord.clinicalSigns).mentation}}</div>
                <div class="mb-1" v-if="safeParseJSON(selectedSoapRecord.clinicalSigns).hydration"><span class="text-muted">Mất nước:</span> {{safeParseJSON(selectedSoapRecord.clinicalSigns).hydration}}</div>
              </div>
              <div v-else class="small mt-3">
                <div v-if="!selectedSoapRecord.clinicalSigns" class="text-muted fst-italic">Không ghi nhận</div>
                <div v-else v-for="(item, idx) in parseRawString(selectedSoapRecord.clinicalSigns)" :key="idx" class="d-flex align-items-start mb-2 bg-light p-2 rounded-3 border">
                  <i class="bi bi-caret-right-fill text-info me-2 mt-1" style="font-size: 0.75rem;"></i>
                  <span class="text-dark lh-base" v-html="formatRawItem(item)"></span>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Diagnosis (Assessment) -->
          <div class="col-md-6">
            <div class="h-100 p-4 bg-white rounded-4 shadow-sm border border-secondary border-opacity-10 position-relative border-start border-4 border-danger">
              <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-danger shadow-sm" style="width: 32px; height: 32px; line-height: 22px; font-size: 1.1rem;">A</span>
              <h6 class="fw-bold text-dark mb-3 ms-2 border-bottom pb-2"><i class="bi bi-activity text-danger me-1"></i> Chẩn đoán (Assessment)</h6>
              <div v-if="safeParseJSON(selectedSoapRecord.diagnosis)" class="small text-dark mt-2">
                <div v-if="safeParseJSON(selectedSoapRecord.diagnosis).tentativeDiagnosis" class="mb-2"><span class="text-muted fw-bold d-block">Chẩn đoán sơ bộ:</span> {{safeParseJSON(selectedSoapRecord.diagnosis).tentativeDiagnosis}}</div>
                <div v-if="safeParseJSON(selectedSoapRecord.diagnosis).definitiveDiagnosis" class="mb-2"><span class="text-muted fw-bold d-block">Chẩn đoán xác định:</span> <span class="fw-bold text-danger">{{safeParseJSON(selectedSoapRecord.diagnosis).definitiveDiagnosis}}</span></div>
                <div v-if="safeParseJSON(selectedSoapRecord.diagnosis).differentialDiagnosis" class="mb-2"><span class="text-muted fw-bold d-block">Chẩn đoán phân biệt:</span> <span class="fst-italic">{{safeParseJSON(selectedSoapRecord.diagnosis).differentialDiagnosis}}</span></div>
                <div v-if="safeParseJSON(selectedSoapRecord.diagnosis).diseaseSeverity" class="mt-3"><span class="badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25">Mức độ: {{safeParseJSON(selectedSoapRecord.diagnosis).diseaseSeverity}}</span></div>
              </div>
              <div v-else class="small mt-3">
                <div v-if="!selectedSoapRecord.diagnosis" class="text-muted fst-italic">Chưa ghi nhận</div>
                <div v-else v-for="(item, idx) in parseRawString(selectedSoapRecord.diagnosis)" :key="idx" class="d-flex align-items-start mb-2 bg-danger bg-opacity-10 p-2 rounded-3 border border-danger border-opacity-25">
                  <i class="bi bi-caret-right-fill text-danger me-2 mt-1" style="font-size: 0.75rem;"></i>
                  <span class="text-danger fw-bold lh-base" v-html="formatRawItem(item)"></span>
                </div>
              </div>
            </div>
          </div>

          <!-- Treatment Plan (Plan) -->
          <div class="col-md-6">
            <div class="h-100 p-4 bg-white rounded-4 shadow-sm border border-secondary border-opacity-10 position-relative border-start border-4 border-primary">
              <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-primary shadow-sm" style="width: 32px; height: 32px; line-height: 22px; font-size: 1.1rem;">P</span>
              <h6 class="fw-bold text-dark mb-3 ms-2 border-bottom pb-2"><i class="bi bi-journal-medical text-primary me-1"></i> Phác đồ & Điều trị (Plan)</h6>
              <ul v-if="Array.isArray(safeParseJSON(selectedSoapRecord.treatmentPlan)) && safeParseJSON(selectedSoapRecord.treatmentPlan).length > 0" class="mt-2 mb-0 ps-3 text-dark small">
                <li v-for="(step, idx) in safeParseJSON(selectedSoapRecord.treatmentPlan)" :key="idx" class="mb-2 fw-semibold text-primary"><i class="bi bi-check2 text-success me-1"></i> {{ step }}</li>
              </ul>
              <div v-else-if="!safeParseJSON(selectedSoapRecord.treatmentPlan)" class="small mt-3">
                <div v-if="!selectedSoapRecord.treatmentPlan" class="text-muted fst-italic">Chưa ghi nhận</div>
                <div v-else v-for="(item, idx) in parseRawString(selectedSoapRecord.treatmentPlan)" :key="idx" class="d-flex align-items-start mb-2 bg-primary bg-opacity-10 p-2 rounded-3 border border-primary border-opacity-25">
                  <i class="bi bi-caret-right-fill text-primary me-2 mt-1" style="font-size: 0.75rem;"></i>
                  <span class="text-dark fw-medium lh-base" v-html="formatRawItem(item)"></span>
                </div>
              </div>
              <div v-else class="text-dark small">Chưa ghi nhận</div>
            </div>
          </div>
        </div>

        <!-- Prescribed Medicine Detailed -->
        <div v-if="selectedSoapRecord.prescribedMedicines && selectedSoapRecord.prescribedMedicines.length > 0" class="mt-4 bg-white p-4 rounded-4 shadow-sm border border-secondary border-opacity-10">
          <h6 class="fw-bold text-success mb-3 border-bottom pb-2"><i class="bi bi-capsule-pill me-1"></i> Thuốc đã kê đơn ({{ selectedSoapRecord.prescribedMedicines.length }} loại)</h6>
          <div class="row g-3">
            <div v-for="(med, mIdx) in selectedSoapRecord.prescribedMedicines" :key="mIdx" class="col-md-6">
              <div class="d-flex gap-3 p-3 bg-success bg-opacity-10 rounded-3 border border-success border-opacity-25 h-100 align-items-center">
                <div class="bg-white rounded-circle p-2 shadow-sm flex-shrink-0"><i class="bi bi-prescription2 text-success fs-4"></i></div>
                <div class="small w-100">
                  <div class="fw-bold text-dark fs-6">{{ med.medicineName }}</div>
                  <div class="d-flex justify-content-between border-bottom border-success border-opacity-25 pb-1 mb-1 mt-1">
                    <span v-if="med.quantity" class="text-success"><i class="bi bi-box me-1"></i>SL: <strong>{{ med.quantity }}</strong></span>
                    <span v-if="med.dosage" class="text-muted"><i class="bi bi-eyedropper me-1"></i>{{ med.dosage }}</span>
                  </div>
                  <div class="text-muted d-flex justify-content-between">
                    <span v-if="med.frequency"><i class="bi bi-clock me-1"></i>{{ med.frequency }}</span>
                    <span v-if="med.durationDays">{{ med.durationDays }} ngày</span>
                  </div>
                  <div v-if="med.instruction" class="text-muted fst-italic mt-2 p-2 bg-white rounded text-center w-100 border border-success border-opacity-25"><i class="bi bi-info-circle me-1"></i>{{ med.instruction }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Attachments -->
        <div v-if="selectedSoapRecord.attachments && selectedSoapRecord.attachments.length > 0" class="mt-4 p-4 bg-white rounded-4 shadow-sm border border-secondary border-opacity-10 position-relative">
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
              @click.stop="openLightbox(selectedSoapRecord.attachments.map((u: string) => getImageUrl(u)), imgIdx)"
              title="Bấm để xem phóng to"
              alt="Ảnh cận lâm sàng"
            />
          </div>
        </div>

        <!-- Note & Follow Up -->
        <div class="row g-4 mt-1">
          <div v-if="selectedSoapRecord.doctorNotes" class="col-md-6">
            <div class="h-100 p-4 bg-warning bg-opacity-10 rounded-4 shadow-sm border border-warning border-opacity-25 d-flex gap-3 align-items-start">
              <i class="bi bi-chat-quote-fill text-warning fs-3"></i>
              <div>
                <h6 class="fw-bold text-dark mb-2">Dặn dò chăm sóc:</h6>
                <div class="fst-italic text-muted small lh-lg">{{ selectedSoapRecord.doctorNotes }}</div>
              </div>
            </div>
          </div>
          <div v-if="selectedSoapRecord.followUpDate" class="col-md-6">
            <div class="h-100 p-4 bg-primary bg-opacity-10 rounded-4 shadow-sm border border-primary border-opacity-25 d-flex align-items-center gap-3">
              <i class="bi bi-calendar-plus-fill text-primary fs-1"></i>
              <div>
                <h6 class="fw-bold text-dark mb-1">Ngày hẹn tiếp theo:</h6>
                <span class="fs-5 text-primary fw-bold">{{ formatDate(selectedSoapRecord.followUpDate) }}</span>
                <div class="small text-muted mt-1"><i class="bi bi-info-circle me-1"></i>(Tái khám / Tiêm nhắc)</div>
              </div>
            </div>
          </div>
        </div>

      </div>
      <div class="zalo-modal-footer bg-white border-top flex-shrink-0 p-3 d-flex justify-content-end rounded-bottom-4" style="background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px);">
        <button class="btn btn-light text-muted rounded-pill px-5 py-2 fw-bold shadow-sm border" @click="closeSoapModal">Đóng Lại</button>
      </div>
    </div>
  </div>

    <!-- ===== LIGHTBOX MODAL ===== -->
    <Teleport to="body">
      <Transition name="lightbox-fade">
        <div
          v-if="lightbox.visible"
          class="lightbox-overlay"
          @click.self="closeLightbox"
          @keydown.esc="closeLightbox"
          tabindex="0"
          ref="lightboxRef"
        >
          <!-- Close button -->
          <button class="lightbox-close" @click="closeLightbox" title="Đóng (Esc)">
            <i class="bi bi-x-lg"></i>
          </button>

          <!-- Prev button -->
          <button
            v-if="lightbox.urls.length > 1"
            class="lightbox-nav lightbox-prev"
            @click.stop="lightboxNav(-1)"
            title="Ảnh trước"
          >
            <i class="bi bi-chevron-left"></i>
          </button>

          <!-- Image -->
          <div class="lightbox-img-wrapper">
            <img
              :src="lightbox.urls[lightbox.index]"
              class="lightbox-img"
              alt="Ảnh cận lâm sàng"
            />
            <div class="lightbox-counter" v-if="lightbox.urls.length > 1">
              {{ lightbox.index + 1 }} / {{ lightbox.urls.length }}
            </div>
          </div>

          <!-- Next button -->
          <button
            v-if="lightbox.urls.length > 1"
            class="lightbox-nav lightbox-next"
            @click.stop="lightboxNav(1)"
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
import { ref, computed, watch, onMounted } from 'vue';
import api from '../../services/api';

const emit = defineEmits<{
  (e: 'switch-tab', tab: string): void;
}>();

// Internal Tabs navigation
const internalTab = ref('treatment'); // 'treatment' or 'pet-history'

// Active patient details loaded from LocalStorage
const activePatient = ref<{
  appointmentId: string;
  petId: string;
  petName: string;
  customerName: string;
  species?: string;
  breed?: string;
  gender?: string;
  age?: number;
  weight?: number;
  allergies?: string;
}>({
  appointmentId: '',
  petId: '',
  petName: '',
  customerName: '',
  allergies: ''
});

const expandedRecords = ref<number[]>([]);
const toggleRecord = (recordId: number) => {
  if (expandedRecords.value.includes(recordId)) {
    expandedRecords.value = expandedRecords.value.filter(id => id !== recordId);
  } else {
    expandedRecords.value.push(recordId);
  }
};

const safeParseJSON = (str: string | null) => {
  if (!str) return null;
  try {
    return JSON.parse(str);
  } catch (e) {
    return null;
  }
};

const getShortDiagnosis = (diagnosis: string | null) => {
  if (!diagnosis) return 'Chưa chẩn đoán';
  const parsed = safeParseJSON(diagnosis);
  if (parsed && parsed.definitiveDiagnosis) return parsed.definitiveDiagnosis;
  if (parsed && parsed.tentativeDiagnosis) return parsed.tentativeDiagnosis;
  return diagnosis.length > 50 ? diagnosis.substring(0, 50) + '...' : diagnosis;
};

// Form states based on MedicalRecordSoapRequestDto
const form = ref({
  appointmentId: 0,
  petId: 0,
  subjective: {
    chiefComplaint: '',
    onsetDuration: '',
    appetite: 'Bình thường',
    thirst: 'Bình thường',
    hasVomiting: false,
    vomitingDetails: '',
    hasDiarrhea: false,
    diarrheaDetails: '',
    hasConstipation: false,
    urinationIssues: 'Bình thường',
    hasCoughing: false,
    hasSneezing: false,
    hasBreathingDifficulty: false,
    hasItching: false,
    hasHairLoss: false,
    activityLevel: 'Bình thường',
    currentMedications: 'Không có',
    petOwnerNotes: ''
  },
  objective: {
    weight: null as number | null,
    temperature: null as number | null,
    heartRate: null as number | null,
    respiratoryRate: null as number | null,
    bodyConditionScore: 5,
    mentation: 'Tỉnh táo',
    hydration: '< 5% (Bình thường)',
    eyes: { isNormal: true, note: '' },
    ears: { isNormal: true, note: '' },
    nose: { isNormal: true, note: '' },
    mouth: { isNormal: true, note: '' },
    skinCoat: { isNormal: true, note: '' },
    gastrointestinal: { isNormal: true, note: '' },
    respiratory: { isNormal: true, note: '' },
    attachments: [] as string[]
  },
  assessment: {
    tentativeDiagnosis: '',
    definitiveDiagnosis: '',
    differentialDiagnosis: '',
    diseaseSeverity: 'Nhẹ',
    prognosis: 'Tốt'
  },
  plan: {
    treatmentDirections: [] as string[],
    careInstructions: '',
    followUpDate: '',
    createFollowUpAppointment: false,
    followUpType: 'FollowUp',
    followUpNote: '',
    prescriptions: [] as Array<{
      medicineId: number | null;
      searchQuery: string;
      showDropdown: boolean;
      quantity: number;
      dosage: string;
      frequency: string;
      durationDays: number;
      instruction: string;
      stockQuantity: number;
      unit: string;
    }>
  }
});

const soapActiveTab = ref('subjective'); // subjective, objective, assessment, plan

const medicineOptions = ref<any[]>([]);
const medicalHistory = ref<any[]>([]);

// ===== Pagination & Filters =====
const historyDateFilter = ref('');
const historyTypeFilter = ref('ALL');
const currentPage = ref(1);
const recordsPerPage = 5;

const filteredMedicalHistory = computed(() => {
  let list = medicalHistory.value;
  if (historyTypeFilter.value !== 'ALL') {
    list = list.filter((r: any) => r.recordType === historyTypeFilter.value);
  }
  if (historyDateFilter.value) {
    const selectedDate = historyDateFilter.value;
    list = list.filter((r: any) => {
      if (!r.visitDate) return false;
      const recordDate = new Date(r.visitDate).toISOString().split('T')[0];
      return recordDate === selectedDate;
    });
  }
  return list;
});

const totalPages = computed(() => {
  return Math.max(1, Math.ceil(filteredMedicalHistory.value.length / recordsPerPage));
});

const paginatedHistory = computed(() => {
  const start = (currentPage.value - 1) * recordsPerPage;
  const end = start + recordsPerPage;
  return filteredMedicalHistory.value.slice(start, end);
});

watch([historyDateFilter, historyTypeFilter], () => {
  currentPage.value = 1;
});

// ===== SOAP Modal Logic =====
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

const getImageUrl = (url: string) => {
  if (!url) return '';
  if (url.startsWith('http')) return url;
  let baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7284';
  baseUrl = baseUrl.replace(/\/api$/, '');
  return `${baseUrl}${url}`;
};

const parseRawString = (str: string | null) => {
  if (!str) return [];
  
  if (str.includes('|')) {
    return str.split('|').map(s => s.trim()).filter(s => s.length > 0);
  }
  
  if (str.includes(', ') && (str.match(/:/g) || []).length > 1) {
    const parts = str.split(', ');
    const result: string[] = [];
    
    parts.forEach(part => {
      if (part.includes(':') && part.split(':')[0].length < 30) {
        result.push(part.trim());
      } else {
        if (result.length > 0) {
          result[result.length - 1] += ', ' + part.trim();
        } else {
          result.push(part.trim());
        }
      }
    });
    return result.filter(s => s.length > 0);
  }
  
  return [str.trim()];
};

const formatRawItem = (item: string) => {
  const parts = item.split(':');
  if (parts.length > 1) {
    const key = parts[0].trim();
    const val = parts.slice(1).join(':').trim();
    return `<span class="fw-bold text-muted">${key}:</span> <span class="fw-medium">${val}</span>`;
  }
  return `<span class="fw-medium">${item}</span>`;
};

const loadingHistory = ref(false);
const submitting = ref(false);
const errorMessage = ref('');

const followUpDateOnly = ref('');
const followUpTimeOnly = ref('');
const fetchingFollowUpSlots = ref(false);
const availableFollowUpSlots = ref<string[]>([]);
const currentDoctorId = ref<string>('');
const masterMorningTimes = ['08:00', '08:30', '09:00', '09:30', '10:00', '10:30', '11:00', '11:30'];
const masterAfternoonTimes = ['13:30', '14:00', '14:30', '15:00', '15:30', '16:00', '16:30', '17:00', '17:30'];
const masterEveningTimes = ['18:00', '18:30', '19:00', '19:30', '20:00', '20:30', '21:00', '21:30', '22:00', '22:30', '23:00', '23:30'];

const minDateOnlyStr = computed(() => {
  const now = new Date();
  return now.toISOString().split('T')[0];
});

const onFollowUpDateChange = async () => {
  followUpTimeOnly.value = '';
  if (!followUpDateOnly.value) {
    availableFollowUpSlots.value = [];
    return;
  }
  fetchingFollowUpSlots.value = true;
  try {
    const res = await api.get('/appointment/available-slots', {
      params: { date: followUpDateOnly.value }
    });
    const allSlots = new Set<string>();
    res.data.forEach((doc: any) => {
      if (currentDoctorId.value && doc.doctorId !== currentDoctorId.value) {
        return; // Filter to show only the current doctor's slots
      }
      if (doc.availableSlots) {
        doc.availableSlots.forEach((slot: string) => allSlots.add(slot));
      }
    });
    availableFollowUpSlots.value = Array.from(allSlots);
  } catch (err) {
    console.error('Lỗi khi tải danh sách giờ trống', err);
    availableFollowUpSlots.value = [];
  } finally {
    fetchingFollowUpSlots.value = false;
  }
};

const BOOKING_BUFFER_MS = 15 * 60 * 1000;

interface SlotDisplay {
  time: string;
  slotStr: string;
  isAvailable: boolean;
  isPast: boolean;
  isBooked: boolean;
  isTooSoon: boolean;
}

const buildFollowUpSlots = (times: string[]): SlotDisplay[] => {
  if (!followUpDateOnly.value) return [];
  const now = Date.now();
  const cutoff = now + BOOKING_BUFFER_MS;
  const [year, month, day] = followUpDateOnly.value.split('-');
  
  return times.map(time => {
    const slotStr = `${followUpDateOnly.value}T${time}:00`;
    const [hour, minute] = time.split(':');
    const slotDate = new Date(parseInt(year), parseInt(month) - 1, parseInt(day), parseInt(hour), parseInt(minute), 0);
    const slotMs = slotDate.getTime();
    
    const isPast = slotMs < now;
    const isTooSoon = !isPast && slotMs < cutoff;
    const isAvailableFromApi = availableFollowUpSlots.value.includes(time);
    
    const isBooked = !isPast && !isTooSoon && !isAvailableFromApi;
    const isAvailable = !isPast && !isTooSoon && isAvailableFromApi;
    
    return { time, slotStr, isAvailable, isPast, isBooked, isTooSoon };
  });
};

const displayFollowUpMorningSlots = computed<SlotDisplay[]>(() => buildFollowUpSlots(masterMorningTimes));
const displayFollowUpAfternoonSlots = computed<SlotDisplay[]>(() => buildFollowUpSlots(masterAfternoonTimes));
const displayFollowUpEveningSlots = computed<SlotDisplay[]>(() => buildFollowUpSlots(masterEveningTimes));

// Computed: Check if any medicine does not have enough stock
const hasStockDeficit = computed(() => {
  return form.value.plan.prescriptions.some(
    p => p.medicineId !== null && (p.stockQuantity === 0 || p.quantity > p.stockQuantity)
  );
});

// Load patient context and medicines list
onMounted(async () => {
  const appointmentId = localStorage.getItem('active_treatment_appointment_id');
  const petId = localStorage.getItem('active_treatment_pet_id');
  const petName = localStorage.getItem('active_treatment_pet_name');
  const customerName = localStorage.getItem('active_treatment_customer_name');

  if (appointmentId && petId) {
    activePatient.value = {
      appointmentId,
      petId,
      petName: petName || 'Bệnh nhi',
      customerName: customerName || 'Khách vãng lai'
    };
    form.value.appointmentId = parseInt(appointmentId, 10);
    form.value.petId = parseInt(petId, 10);
    
    // Load Medical history of the pet (only if petId is valid)
    const petIdNum = parseInt(petId, 10);
    if (petIdNum > 0) {
      fetchPetHistory(petIdNum);
      
      // We have petId, but we might still want to fetch extra pet info from appointment
      try {
        const res = await api.get(`/doctor/appointment/${appointmentId}`);
        const apptData = res.data;
        if (apptData && apptData.petName) {
          activePatient.value.species = apptData.petSpecies;
          activePatient.value.breed = apptData.petBreed;
          activePatient.value.allergies = apptData.petAllergies || apptData.allergies || 'Không ghi nhận';
        }
        if (apptData && apptData.doctorId) {
          currentDoctorId.value = apptData.doctorId;
        }
      } catch(err) {
        console.error('Không thể lấy thêm thông tin pet từ appointment:', err);
      }
    } else {
      // petId is invalid (0) or missing, try to resolve it from the appointment
      console.warn('[ConsultationRecordTab] petId is 0 or invalid. Trying to resolve from appointment...');
      try {
        const res = await api.get(`/doctor/appointment/${appointmentId}`);
        const apptData = res.data;
        if (apptData && apptData.petId && apptData.petId > 0) {
          const resolvedPetId = apptData.petId;
          localStorage.setItem('active_treatment_pet_id', resolvedPetId.toString());
          activePatient.value.petId = resolvedPetId.toString();
          form.value.petId = resolvedPetId;
          fetchPetHistory(resolvedPetId);
        }
        
        // Also fetch pet info if pet exists
        if (apptData && apptData.petName) {
           activePatient.value.petName = apptData.petName;
           activePatient.value.species = apptData.petSpecies;
           activePatient.value.breed = apptData.petBreed;
           activePatient.value.allergies = apptData.petAllergies || apptData.allergies || 'Không ghi nhận';
        }
        if (apptData && apptData.doctorId) {
          currentDoctorId.value = apptData.doctorId;
        }
      } catch (err) {
        console.error('Không thể resolve petId từ appointment:', err);
      }
    }
  }

  // Load list of medicines for prescription form
  fetchMedicines();

});

const fetchMedicines = async () => {
  try {
    const res = await api.get('/medicines');
    // Bác sĩ kê đơn: Chỉ lấy những thuốc còn hàng (Tồn > 0)
    medicineOptions.value = (res.data || []).filter((m: any) => m.stockQuantity > 0);
  } catch (err) {
    console.error('Lỗi tải danh mục thuốc:', err);
  }
};

const filteredMedicines = (query: string) => {
  if (!query) return medicineOptions.value;
  const lower = query.toLowerCase();
  return medicineOptions.value.filter(m => m.name.toLowerCase().includes(lower));
};

const handleSearchInput = (pres: any) => {
  pres.medicineId = null;
  pres.showDropdown = true;
};

const hideDropdown = (pres: any) => {
  // Delay slightly to allow mousedown on item to fire first
  setTimeout(() => {
    pres.showDropdown = false;
  }, 200);
};

const selectMedicine = (idx: number, med: any) => {
  const pres = form.value.plan.prescriptions[idx];
  pres.medicineId = med.id;
  pres.searchQuery = med.name;
  pres.showDropdown = false;
  onMedicineChange(idx, med.id);
};

const fetchPetHistory = async (petId: number) => {
  loadingHistory.value = true;
  try {
    const res = await api.get(`/medical-records/pet/${petId}`);
    medicalHistory.value = res.data || [];
    currentPage.value = 1;
  } catch (err) {
    console.error('Lỗi tải bệnh sử:', err);
  } finally {
    loadingHistory.value = false;
  }
};

// Prescription List Actions
const addPrescriptionLine = () => {
  form.value.plan.prescriptions.push({
    medicineId: null,
    searchQuery: '',
    showDropdown: false,
    quantity: 1,
    dosage: '',
    frequency: '',
    durationDays: 5,
    instruction: '',
    stockQuantity: 9999,
    unit: 'đơn vị'
  });
};

const removePrescriptionLine = (index: number) => {
  form.value.plan.prescriptions.splice(index, 1);
};

const onMedicineChange = (idx: number, medicineId: number | null) => {
  if (medicineId === null) return;
  const match = medicineOptions.value.find(m => m.id === medicineId);
  if (match) {
    form.value.plan.prescriptions[idx].stockQuantity = match.stockQuantity;
    form.value.plan.prescriptions[idx].unit = match.unit || 'đơn vị';
    form.value.plan.prescriptions[idx].dosage = '1 viên';
    form.value.plan.prescriptions[idx].frequency = '2 lần/ngày';
    form.value.plan.prescriptions[idx].instruction = 'Sau ăn';
  }
};

// ===== Image Upload State =====
const fileInputRef = ref<HTMLInputElement | null>(null);
const selectedFiles = ref<File[]>([]);
const previewUrls = ref<string[]>([]);
const isDragging = ref(false);

// ===== Lightbox =====
const lightboxRef = ref<HTMLElement | null>(null);
const lightbox = ref<{ visible: boolean; urls: string[]; index: number }>({
  visible: false,
  urls: [],
  index: 0
});

const openLightbox = (urls: string[], index: number) => {
  lightbox.value = { visible: true, urls, index };
  // Focus the overlay so Esc key works
  setTimeout(() => lightboxRef.value?.focus(), 50);
};

const closeLightbox = () => {
  lightbox.value.visible = false;
};

const lightboxNav = (dir: number) => {
  const len = lightbox.value.urls.length;
  lightbox.value.index = (lightbox.value.index + dir + len) % len;
};

const triggerFileInput = () => fileInputRef.value?.click();

const addFiles = (files: FileList) => {
  for (const file of Array.from(files)) {
    if (file.size > 5 * 1024 * 1024) { alert(`File ${file.name} vượt quá 5MB.`); continue; }
    selectedFiles.value.push(file);
    previewUrls.value.push(URL.createObjectURL(file));
  }
};

const onFileSelected = (e: Event) => {
  const input = e.target as HTMLInputElement;
  if (input.files) addFiles(input.files);
};

const onFileDrop = (e: DragEvent) => {
  isDragging.value = false;
  if (e.dataTransfer?.files) addFiles(e.dataTransfer.files);
};

const removePreviewImage = (idx: number) => {
  URL.revokeObjectURL(previewUrls.value[idx]);
  previewUrls.value.splice(idx, 1);
  selectedFiles.value.splice(idx, 1);
};

const uploadImages = async (): Promise<string[]> => {
  if (selectedFiles.value.length === 0) return [];
  const formData = new FormData();
  for (const file of selectedFiles.value) formData.append('files', file);
  const res = await api.post('/upload/medical-images', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
  return res.data.urls || [];
};

// Form submission & cancellation
const submitForm = async () => {
  if (!form.value.appointmentId) {
    errorMessage.value = 'Không tìm thấy ID cuộc hẹn hoạt động.';
    return;
  }
  if (!form.value.objective.weight || !form.value.objective.temperature) {
    errorMessage.value = 'Vui lòng điền cân nặng và nhiệt độ của bệnh nhi ở mục Khách quan (O).';
    soapActiveTab.value = 'objective';
    return;
  }
  if (!form.value.assessment.tentativeDiagnosis && !form.value.assessment.definitiveDiagnosis) {
    errorMessage.value = 'Vui lòng điền Chẩn đoán sơ bộ hoặc Chẩn đoán xác định.';
    soapActiveTab.value = 'assessment';
    return;
  }

  if (form.value.plan.createFollowUpAppointment) {
    if (!followUpDateOnly.value || !followUpTimeOnly.value) {
      errorMessage.value = 'Vui lòng chọn đầy đủ Ngày và Khung giờ trống cho lịch tái khám.';
      soapActiveTab.value = 'plan';
      return;
    }
  }

  submitting.value = true;
  errorMessage.value = '';

  try {
    // Step 1: Upload images first if any
    let uploadedUrls: string[] = [];
    if (selectedFiles.value.length > 0) {
      try {
        uploadedUrls = await uploadImages();
      } catch (uploadErr) {
        errorMessage.value = 'Lỗi tải ảnh lên. Vui lòng thử lại.';
        submitting.value = false;
        return;
      }
    }

    const finalFollowUpDate = (form.value.plan.createFollowUpAppointment && followUpDateOnly.value && followUpTimeOnly.value)
      ? `${followUpDateOnly.value}T${followUpTimeOnly.value}:00`
      : null;

    const payload = {
      appointmentId: form.value.appointmentId,
      petId: form.value.petId,
      subjective: form.value.subjective,
      objective: { ...form.value.objective, attachments: uploadedUrls },
      assessment: form.value.assessment,
      plan: {
        treatmentDirections: form.value.plan.treatmentDirections,
        careInstructions: form.value.plan.careInstructions,
        followUpDate: finalFollowUpDate,
        createFollowUpAppointment: form.value.plan.createFollowUpAppointment,
        followUpType: form.value.plan.followUpType,
        followUpNote: form.value.plan.followUpNote,
        prescriptions: form.value.plan.prescriptions
          .filter(p => p.medicineId !== null)
          .map(p => ({
            medicineId: p.medicineId,
            quantity: p.quantity,
            dosage: p.dosage || '1 viên',
            frequency: p.frequency || '1 lần/ngày',
            durationDays: p.durationDays || 5,
            instruction: p.instruction || ''
          }))
      }
    };

    const res = await api.post('/medical-records/soap', payload);
    if (res.data.success) {
      // Tự động chuyển sang trạng thái chờ thanh toán
      try {
        await api.put(`/receptionist/queue/${form.value.appointmentId}/status`, { status: 'ready_to_pay' });
      } catch (e) {
        console.warn('Không thể tự chuyển trạng thái ready_to_pay:', e);
      }

      localStorage.removeItem('active_treatment_appointment_id');
      localStorage.removeItem('active_treatment_pet_id');
      localStorage.removeItem('active_treatment_pet_name');
      localStorage.removeItem('active_treatment_customer_name');
      
      emit('switch-tab', 'doctor-cases');
    }
  } catch (err: any) {
    errorMessage.value = err.response?.data?.message || 'Không thể lưu bệnh án. Vui lòng kiểm tra lại.';
  } finally {
    submitting.value = false;
  }
};

const cancelTreatment = () => {
  emit('switch-tab', 'doctor-cases');
};

// Format Helpers
const formatDate = (dateStr: string): string => {
  if (!dateStr) return '';
  return new Date(dateStr).toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' });
};
</script>

<style scoped>
/* ===== Image Upload Widget ===== */
.upload-dropzone {
  border: 2px dashed #c7d2fe;
  background: linear-gradient(135deg, #f0f4ff, #fafbff);
  cursor: pointer;
  transition: all 0.25s ease;
}
.upload-dropzone:hover,
.upload-dropzone.dragging {
  border-color: #6366f1;
  background: linear-gradient(135deg, #eef2ff, #f5f3ff);
  box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.08);
  transform: scale(1.005);
}
/* ===== Premium Filters ===== */
.filter-group {
  border: 1.5px solid #e5e7eb;
  background-color: #f9fafb;
}
.filter-wrapper:hover .filter-group {
  border-color: #0d6efd;
  background-color: #ffffff;
  box-shadow: 0 4px 12px rgba(13, 110, 253, 0.15) !important;
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

.img-preview-wrapper {
  width: 100px;
  height: 100px;
}
.img-preview {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border: 2px solid #e5e7eb;
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: zoom-in;
}
.img-preview:hover {
  transform: scale(1.05);
  box-shadow: 0 4px 14px rgba(0,0,0,0.15);
}
.btn-remove-img {
  background: rgba(220, 38, 38, 0.9);
  border: none;
  color: white;
  border-radius: 50%;
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.65rem;
  cursor: pointer;
  transform: translate(6px, -6px);
  box-shadow: 0 2px 6px rgba(0,0,0,0.2);
  transition: background 0.2s;
}
.btn-remove-img:hover {
  background: rgba(185, 28, 28, 1);
}

/* ===== Lightbox ===== */
.lightbox-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: rgba(0, 0, 0, 0.88);
  backdrop-filter: blur(6px);
  display: flex;
  align-items: center;
  justify-content: center;
  outline: none;
}
.lightbox-img-wrapper {
  position: relative;
  max-width: 90vw;
  max-height: 88vh;
  display: flex;
  flex-direction: column;
  align-items: center;
}
.lightbox-img {
  max-width: 88vw;
  max-height: 82vh;
  object-fit: contain;
  border-radius: 12px;
  box-shadow: 0 24px 64px rgba(0,0,0,0.5);
  user-select: none;
}
.lightbox-counter {
  margin-top: 12px;
  color: rgba(255,255,255,0.75);
  font-size: 0.85rem;
  font-weight: 600;
  letter-spacing: 0.5px;
}
.lightbox-close {
  position: fixed;
  top: 20px;
  right: 24px;
  z-index: 10000;
  background: rgba(255,255,255,0.15);
  border: 1px solid rgba(255,255,255,0.25);
  color: white;
  border-radius: 50%;
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
  cursor: pointer;
  transition: background 0.2s, transform 0.2s;
  backdrop-filter: blur(4px);
}
.lightbox-close:hover {
  background: rgba(255,255,255,0.3);
  transform: scale(1.1);
}
.lightbox-nav {
  position: fixed;
  top: 50%;
  transform: translateY(-50%);
  z-index: 10000;
  background: rgba(255,255,255,0.12);
  border: 1px solid rgba(255,255,255,0.2);
  color: white;
  border-radius: 50%;
  width: 52px;
  height: 52px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.3rem;
  cursor: pointer;
  transition: background 0.2s, transform 0.2s;
  backdrop-filter: blur(4px);
}
.lightbox-nav:hover {
  background: rgba(255,255,255,0.28);
  transform: translateY(-50%) scale(1.1);
}
.lightbox-prev { left: 20px; }
.lightbox-next { right: 20px; }

/* Lightbox fade transition */
.lightbox-fade-enter-active,
.lightbox-fade-leave-active {
  transition: opacity 0.22s ease;
}
.lightbox-fade-enter-from,
.lightbox-fade-leave-to {
  opacity: 0;
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

.medical-records-tab {
  padding: 0;
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
  background: linear-gradient(135deg, #ffc107, #ff9800);
  color: white;
  box-shadow: 0 4px 10px rgba(255, 152, 0, 0.3);
}

.pet-avatar-large {
  font-size: 2.2rem;
  background-color: #fff9e6;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 3px solid #ffc107;
}

.autocomplete-item:hover {
  background-color: #f8f9fa;
  cursor: pointer;
}
.cursor-pointer {
  cursor: pointer;
}

.card-gradient-pet {
  background: linear-gradient(135deg, #fffbeb 0%, #fff9e6 100%);
  border-left: 5px solid #ffc107 !important;
}

.btn-premium {
  background: linear-gradient(135deg, #ffc107, #ff9800);
  border: none;
  transition: all 0.3s;
  box-shadow: 0 4px 12px rgba(255, 152, 0, 0.3);
}
.btn-premium:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 18px rgba(255, 152, 0, 0.4);
}
.btn-premium:disabled {
  background: #cbd5e1;
  box-shadow: none;
  cursor: not-allowed;
  transform: none;
}

.border-start-3 {
  border-left-width: 3px !important;
}

/* Timeline */
.timeline-item {
  position: relative;
}
.timeline-line {
  position: absolute;
  top: 15px;
  left: 31px;
  bottom: -15px;
  width: 2px;
  background-color: #e2e8f0;
}
.timeline-item:last-child .timeline-line {
  display: none;
}
.timeline-circle {
  position: absolute;
  top: 15px;
  left: 26px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid white;
  z-index: 1;
}

.timeline-content.accordion-item {
  margin-left: 20px;
}

.accordion-button:not(.collapsed) {
  background-color: #f8f9fa;
  box-shadow: inset 0 calc(-1 * var(--bs-accordion-border-width)) 0 var(--bs-accordion-border-color);
}
.accordion-button:focus {
  box-shadow: none;
}

</style>
