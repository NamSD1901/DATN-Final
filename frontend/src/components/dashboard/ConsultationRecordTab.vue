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
            <div class="col-md-3 border-end">
              <div class="d-flex align-items-center gap-3">
                <div class="pet-avatar-large bg-white shadow-sm" style="width:50px;height:50px;font-size:1.8rem">🐾</div>
                <div>
                  <h6 class="fw-bold mb-0 text-dark">{{ activePatient.petName }}</h6>
                  <span class="small text-muted">ID: {{ activePatient.petId }}</span>
                </div>
              </div>
            </div>
            <div class="col-md-3 border-end">
              <div class="small text-muted mb-1">Chủ nuôi</div>
              <div class="fw-bold text-dark">{{ activePatient.customerName }}</div>
            </div>
            <div class="col-md-3 border-end">
              <div class="small text-muted mb-1">Cân nặng (kg) <span class="text-danger">*</span></div>
              <input type="number" step="0.1" v-model="form.weight" class="form-control form-control-sm border-warning rounded-3 bg-white" placeholder="VD: 5.2" required>
            </div>
            <div class="col-md-3">
              <div class="small text-muted mb-1">Nhiệt độ (°C) <span class="text-danger">*</span></div>
              <input type="number" step="0.1" v-model="form.temperature" class="form-control form-control-sm border-warning rounded-3 bg-white" placeholder="VD: 38.5" required>
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
                        <div v-else class="row g-3">
                          <div v-for="(pres, idx) in form.plan.prescriptions" :key="idx" class="col-md-6">
                            <div class="bg-light p-3 rounded-4 border position-relative h-100">
                              <button type="button" class="btn-close position-absolute top-0 end-0 m-2" @click="removePrescriptionLine(idx)"></button>
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
                      
                      <div class="col-md-6">
                        <label class="small fw-bold text-secondary">Hướng xử lý (Treatment Directions)</label>
                        <div class="d-flex gap-2 mt-2 flex-wrap">
                          <label class="btn btn-outline-primary btn-sm rounded-pill"><input type="checkbox" class="d-none" value="Truyền dịch" v-model="form.plan.treatmentDirections"> Truyền dịch</label>
                          <label class="btn btn-outline-primary btn-sm rounded-pill"><input type="checkbox" class="d-none" value="Tiêm kháng sinh" v-model="form.plan.treatmentDirections"> Tiêm kháng sinh</label>
                          <label class="btn btn-outline-primary btn-sm rounded-pill"><input type="checkbox" class="d-none" value="Phẫu thuật" v-model="form.plan.treatmentDirections"> Phẫu thuật</label>
                          <label class="btn btn-outline-primary btn-sm rounded-pill"><input type="checkbox" class="d-none" value="Xét nghiệm máu" v-model="form.plan.treatmentDirections"> Xét nghiệm máu</label>
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
            <h6 class="fw-bold mb-4 pb-3 text-dark border-bottom"><i class="bi bi-clock-history text-warning me-2"></i>Lịch sử khám & Điều trị (Timeline)</h6>
            
            <div v-if="loadingHistory" class="text-center py-5">
              <div class="spinner-border text-warning" role="status"></div>
              <p class="text-muted small mt-3">Đang tải bệnh sử từ hệ thống...</p>
            </div>

            <div v-else-if="medicalHistory.length === 0" class="text-center py-5 text-muted small">
              <i class="bi bi-folder-x fs-1 d-block mb-3 text-black-50 opacity-25"></i>
              Bé chưa có bất kỳ bệnh án lưu trữ nào trong hệ thống.
            </div>

            <div v-else class="medical-history-timeline pe-2 overflow-auto mt-2" style="max-height: 650px;">
              <div class="timeline-container accordion" id="doctorHistoryAccordion">
                <div v-for="record in paginatedHistory" :key="record.recordId" class="timeline-item position-relative ps-4 pb-4">
                  <!-- Timeline dot -->
                  <div class="timeline-line"></div>
                  <div class="timeline-circle shadow-sm" :class="record.recordType === 'Vaccination' ? 'bg-success border-success' : 'bg-primary border-primary'"></div>
                  
                  <!-- Timeline content card (Accordion) -->
                  <div class="timeline-content accordion-item border bg-transparent rounded-4 shadow-sm overflow-hidden">
                    <h2 class="accordion-header card-header-main" :id="'heading' + record.recordId">
                      <button 
                        class="accordion-button shadow-none bg-white py-3 px-4" 
                        :class="{ 'collapsed': !expandedRecords.includes(record.recordId) }"
                        type="button" 
                        @click="toggleRecord(record.recordId)"
                      >
                        <div class="d-flex flex-column w-100 pe-3">
                          <div class="d-flex justify-content-between align-items-center w-100 mb-1">
                            <span class="visit-date fw-bold text-dark fs-6">
                              <i class="bi bi-calendar-check text-warning me-2"></i>{{ formatDate(record.visitDate) }}
                            </span>
                            <span class="badge rounded-pill px-3 py-1" :class="record.recordType === 'Vaccination' ? 'bg-success bg-opacity-10 text-success border border-success border-opacity-25' : 'bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25'">
                              {{ record.recordType === 'Vaccination' ? 'Tiêm phòng' : 'Khám bệnh' }}
                            </span>
                          </div>
                          <div class="d-flex justify-content-between align-items-center w-100 mt-2">
                            <span class="doctor-badge mb-0 text-muted small">
                              <i class="bi bi-person-badge me-1"></i>BS: <span class="fw-semibold text-dark">{{ record.doctorName || 'Chưa rõ' }}</span>
                            </span>
                            <span v-if="record.diagnosis" class="text-truncate text-muted small ms-2" style="max-width: 200px;">
                              {{ getShortDiagnosis(record.diagnosis) }}
                            </span>
                          </div>
                        </div>
                      </button>
                    </h2>

                    <div 
                      :id="'collapse' + record.recordId" 
                      class="accordion-collapse collapse" 
                      :class="{ 'show': expandedRecords.includes(record.recordId) }"
                    >
                      <div class="accordion-body card-body-main bg-light border-top border-light rounded-bottom-4 p-4">
                        
                        <!-- Vitals -->
                        <div v-if="record.weight || record.temperature" class="row g-2 mb-4">
                          <div v-if="record.weight" class="col-auto">
                            <span class="badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25 px-3 py-2 rounded-pill">
                              <i class="bi bi-clipboard2-pulse me-1"></i>Cân nặng: <strong>{{ record.weight }} kg</strong>
                            </span>
                          </div>
                          <div v-if="record.temperature" class="col-auto">
                            <span class="badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25 px-3 py-2 rounded-pill">
                              <i class="bi bi-thermometer-half me-1"></i>Nhiệt độ: <strong>{{ record.temperature }} °C</strong>
                            </span>
                          </div>
                        </div>

                        <div class="row g-3 small">
                          <!-- Medical History (Subjective) -->
                          <div class="col-12">
                            <div class="p-3 bg-white rounded-3 border border-dashed">
                              <div class="text-muted mb-2 fw-bold"><i class="bi bi-person-lines-fill text-secondary me-1"></i>Bệnh sử & Lý do khám (S):</div>
                              <div v-if="safeParseJSON(record.medicalHistory)" class="mt-2">
                                <div class="row g-2 text-dark">
                                  <div class="col-12" v-if="safeParseJSON(record.medicalHistory).chiefComplaint"><span class="text-muted fw-semibold">Lý do khám:</span> {{safeParseJSON(record.medicalHistory).chiefComplaint}}</div>
                                  <div class="col-6" v-if="safeParseJSON(record.medicalHistory).appetite"><span class="text-muted">Ăn uống:</span> {{safeParseJSON(record.medicalHistory).appetite}}</div>
                                  <div class="col-6" v-if="safeParseJSON(record.medicalHistory).urinationIssues"><span class="text-muted">Tiêu tiểu:</span> {{safeParseJSON(record.medicalHistory).urinationIssues}}</div>
                                  <div class="col-6" v-if="safeParseJSON(record.medicalHistory).activityLevel"><span class="text-muted">Hoạt động:</span> {{safeParseJSON(record.medicalHistory).activityLevel}}</div>
                                  <div class="col-12 mt-1" v-if="safeParseJSON(record.medicalHistory).petOwnerNotes"><span class="text-muted">Ghi chú:</span> {{safeParseJSON(record.medicalHistory).petOwnerNotes}}</div>
                                </div>
                              </div>
                              <div v-else class="text-dark">{{ record.medicalHistory || 'Không ghi nhận' }}</div>
                            </div>
                          </div>

                          <!-- Clinical Signs (Objective) -->
                          <div class="col-12">
                            <div class="p-3 bg-white rounded-3 border">
                              <div class="text-muted mb-2 fw-bold"><i class="bi bi-heart-pulse-fill text-info me-1"></i>Khám lâm sàng (O):</div>
                              <div v-if="safeParseJSON(record.clinicalSigns)" class="mt-2 text-dark">
                                <div class="row g-2">
                                  <div class="col-6" v-if="safeParseJSON(record.clinicalSigns).heartRate"><span class="text-muted">Nhịp tim:</span> {{safeParseJSON(record.clinicalSigns).heartRate}} bpm</div>
                                  <div class="col-6" v-if="safeParseJSON(record.clinicalSigns).respiratoryRate"><span class="text-muted">Nhịp thở:</span> {{safeParseJSON(record.clinicalSigns).respiratoryRate}} l/p</div>
                                  <div class="col-6" v-if="safeParseJSON(record.clinicalSigns).mentation"><span class="text-muted">Tinh thần:</span> {{safeParseJSON(record.clinicalSigns).mentation}}</div>
                                  <div class="col-6" v-if="safeParseJSON(record.clinicalSigns).hydration"><span class="text-muted">Mất nước:</span> {{safeParseJSON(record.clinicalSigns).hydration}}</div>
                                  <div class="col-6" v-if="safeParseJSON(record.clinicalSigns).bodyConditionScore"><span class="text-muted">BCS:</span> {{safeParseJSON(record.clinicalSigns).bodyConditionScore}}/9</div>
                                </div>
                              </div>
                              <div v-else class="text-dark">{{ record.clinicalSigns || 'Không ghi nhận' }}</div>
                            </div>
                          </div>
                          
                          <!-- Diagnosis (Assessment) & Treatment Plan (Plan) -->
                          <div class="col-md-6">
                            <div class="p-3 bg-white rounded-3 border h-100">
                              <div class="text-muted mb-2 fw-bold text-danger"><i class="bi bi-activity me-1"></i>Chẩn đoán y khoa (A):</div>
                              <div v-if="safeParseJSON(record.diagnosis)" class="mt-2 text-dark">
                                <div v-if="safeParseJSON(record.diagnosis).tentativeDiagnosis" class="mb-1"><span class="text-muted fw-semibold">CĐ sơ bộ:</span> {{safeParseJSON(record.diagnosis).tentativeDiagnosis}}</div>
                                <div v-if="safeParseJSON(record.diagnosis).definitiveDiagnosis" class="mb-1"><span class="text-muted fw-semibold">CĐ xác định:</span> <span class="fw-bold text-danger">{{safeParseJSON(record.diagnosis).definitiveDiagnosis}}</span></div>
                                <div v-if="safeParseJSON(record.diagnosis).differentialDiagnosis" class="mb-1"><span class="text-muted fw-semibold">CĐ phân biệt:</span> {{safeParseJSON(record.diagnosis).differentialDiagnosis}}</div>
                                <div v-if="safeParseJSON(record.diagnosis).diseaseSeverity" class="mb-1 mt-2"><span class="badge bg-secondary">Mức độ: {{safeParseJSON(record.diagnosis).diseaseSeverity}}</span></div>
                              </div>
                              <div v-else class="fw-bold text-danger">{{ record.diagnosis || 'Chưa ghi nhận' }}</div>
                            </div>
                          </div>

                          <div class="col-md-6">
                            <div class="p-3 bg-white rounded-3 border h-100">
                              <div class="text-muted mb-2 fw-bold text-primary"><i class="bi bi-journal-medical me-1"></i>Phác đồ & Điều trị (P):</div>
                              <ul v-if="Array.isArray(safeParseJSON(record.treatmentPlan)) && safeParseJSON(record.treatmentPlan).length > 0" class="mt-2 mb-0 ps-3 text-dark">
                                <li v-for="(step, idx) in safeParseJSON(record.treatmentPlan)" :key="idx" class="mb-1">{{ step }}</li>
                              </ul>
                              <div v-else-if="!safeParseJSON(record.treatmentPlan)" class="text-dark">{{ record.treatmentPlan || 'Chưa ghi nhận' }}</div>
                              <div v-else class="text-dark">Chưa ghi nhận</div>
                            </div>
                          </div>
                        </div>

                        <!-- Prescribed Medicine Detailed -->
                        <div v-if="record.prescribedMedicines && record.prescribedMedicines.length > 0" class="mt-4 bg-white p-3 rounded-4 border shadow-sm">
                          <span class="fw-bold text-success d-block small mb-3"><i class="bi bi-capsule-pill me-1"></i>Thuốc đã kê đơn ({{ record.prescribedMedicines.length }} loại):</span>
                          <div class="row g-2">
                            <div v-for="(med, mIdx) in record.prescribedMedicines" :key="mIdx" class="col-md-6">
                              <div class="d-flex align-items-start gap-2 p-2 bg-success bg-opacity-10 rounded-3 border border-success border-opacity-25 h-100">
                                <i class="bi bi-check-circle-fill text-success mt-1 flex-shrink-0" style="font-size: 0.8rem;"></i>
                                <div class="small">
                                  <div class="fw-bold text-dark">{{ med.medicineName }}</div>
                                  <div class="text-muted mt-1">
                                    <span v-if="med.quantity" class="me-2"><i class="bi bi-box me-1"></i>SL: <strong>{{ med.quantity }}</strong></span>
                                    <span v-if="med.dosage" class="me-2"><i class="bi bi-eyedropper me-1"></i>{{ med.dosage }}</span>
                                    <span v-if="med.frequency"><i class="bi bi-clock me-1"></i>{{ med.frequency }}</span>
                                  </div>
                                  <div v-if="med.instruction" class="text-muted fst-italic mt-1"><i class="bi bi-info-circle me-1"></i>{{ med.instruction }}</div>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>

                        <!-- Note & Follow Up -->
                        <div v-if="record.doctorNotes" class="mt-4 p-3 bg-warning bg-opacity-10 rounded-3 border border-warning border-opacity-25 d-flex gap-2">
                          <i class="bi bi-chat-quote-fill text-warning mt-1 flex-shrink-0"></i>
                          <div>
                            <div class="fw-bold small text-dark mb-1">Dặn dò chăm sóc:</div>
                            <div class="fst-italic text-muted small">{{ record.doctorNotes }}</div>
                          </div>
                        </div>
                        <div v-if="record.followUpDate" class="mt-3 p-3 bg-success bg-opacity-10 rounded-3 border border-success border-opacity-25 d-flex align-items-center gap-2">
                          <i class="bi bi-calendar-plus-fill text-success fs-5"></i>
                          <span class="small text-dark fw-bold">Ngày tái khám / Tiêm nhắc: <span class="text-success">{{ formatDate(record.followUpDate) }}</span></span>
                        </div>

                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Pagination Controls -->
              <div v-if="totalPages > 1" class="d-flex justify-content-center align-items-center mt-4 gap-3 pb-3 pt-3 border-top border-light">
                <button class="btn btn-sm btn-light border rounded-pill px-3 shadow-sm" :class="{'disabled text-muted': currentPage === 1}" @click="prevPage">
                  <i class="bi bi-chevron-left me-1"></i> Trước
                </button>
                <div class="small fw-bold text-dark px-3 py-1 bg-light rounded-pill border">
                  Trang {{ currentPage }} / {{ totalPages }}
                </div>
                <button class="btn btn-sm btn-light border rounded-pill px-3 shadow-sm" :class="{'disabled text-muted': currentPage === totalPages}" @click="nextPage">
                  Tiếp <i class="bi bi-chevron-right ms-1"></i>
                </button>
              </div>

            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
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
    respiratory: { isNormal: true, note: '' }
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

const currentPage = ref(1);
const recordsPerPage = 5;

const totalPages = computed(() => {
  return Math.ceil(medicalHistory.value.length / recordsPerPage);
});

const paginatedHistory = computed(() => {
  const start = (currentPage.value - 1) * recordsPerPage;
  const end = start + recordsPerPage;
  return medicalHistory.value.slice(start, end);
});

const prevPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--;
  }
};

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
  }
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
const masterAfternoonTimes = ['13:30', '14:00', '14:30', '15:00', '15:30', '16:00', '16:30', '17:00', '17:30', '18:00', '18:30', '19:00', '19:30'];

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
    const finalFollowUpDate = (form.value.plan.createFollowUpAppointment && followUpDateOnly.value && followUpTimeOnly.value)
      ? `${followUpDateOnly.value}T${followUpTimeOnly.value}:00`
      : null;

    const payload = {
      appointmentId: form.value.appointmentId,
      petId: form.value.petId,
      subjective: form.value.subjective,
      objective: form.value.objective,
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
