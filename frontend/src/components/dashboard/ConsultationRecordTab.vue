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
                              <select v-model="pres.medicineId" class="form-select form-select-sm fw-bold mb-2 w-75" @change="onMedicineChange(idx, pres.medicineId)">
                                <option :value="null" disabled>— Chọn thuốc —</option>
                                <option v-for="med in medicineOptions" :key="med.id" :value="med.id">{{ med.name }} ({{ med.unit }})</option>
                              </select>
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

          <!-- Bottom Row: Followup -->
          <div class="row g-4 mb-4">
            <div class="col-md-6">
              <label class="form-label small fw-bold text-secondary"><i class="bi bi-calendar-event text-warning me-1"></i> Ngày hẹn tái khám / Tiêm nhắc lại</label>
              <input type="date" v-model="form.plan.followUpDate" class="form-control rounded-pill shadow-sm border px-3">
            </div>
            <div class="col-md-6 d-flex align-items-end">
              <div class="alert alert-info py-2 px-3 mb-0 rounded-pill small w-100 border-0 shadow-sm d-flex align-items-center">
                <i class="bi bi-info-circle-fill me-2"></i> Bệnh án SOAP sẽ được lưu nguyên trạng vào hệ thống.
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

            <div v-else class="medical-timeline pe-2 overflow-auto" style="max-height: 550px;">
              <div v-for="record in medicalHistory" :key="record.recordId" class="timeline-item position-relative ps-4 pb-4">
                <div class="timeline-line"></div>
                <div class="timeline-circle bg-warning shadow-sm"></div>
                
                <div class="timeline-content p-4 bg-light rounded-4 shadow-sm border border-white">
                  <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap">
                    <span class="text-dark fw-bold fs-6"><i class="bi bi-calendar-check text-warning me-2"></i>{{ formatDate(record.visitDate) }}</span>
                    <span class="badge bg-white border text-dark shadow-sm px-3 py-1 rounded-pill"><i class="bi bi-person-badge text-muted me-1"></i> Bác sĩ: {{ record.doctorName }}</span>
                  </div>
                  
                  <div class="row g-3 small">
                    <div v-if="record.symptoms" class="col-12 border-bottom pb-2">
                      <div class="text-muted mb-1 fw-bold">Triệu chứng lúc khám:</div>
                      <div class="text-dark">{{ record.symptoms }}</div>
                    </div>
                    <div class="col-md-6 border-end">
                      <div class="text-muted mb-1 fw-bold text-danger">Chẩn đoán:</div>
                      <div class="fw-bold text-dark">{{ record.diagnosis }}</div>
                    </div>
                    <div class="col-md-6">
                      <div class="text-muted mb-1 fw-bold text-primary">Phương pháp điều trị:</div>
                      <div class="text-dark">{{ record.treatment }}</div>
                    </div>
                    <div v-if="record.note" class="col-12 mt-2">
                      <div class="p-3 bg-white rounded-3 fst-italic text-muted border border-warning border-opacity-50 border-start-3">"{{ record.note }}"</div>
                    </div>
                  </div>
                  
                  <!-- Prescribed medicines list -->
                  <div v-if="record.prescribedMedicines && record.prescribedMedicines.length > 0" class="mt-3 bg-white p-3 rounded-4 border shadow-sm">
                    <span class="fw-bold text-success d-block small mb-2"><i class="bi bi-capsule-pill me-1"></i>Thuốc đã kê đơn:</span>
                    <ul class="list-unstyled mb-0 ps-2">
                      <li v-for="(medStr, mIdx) in record.prescribedMedicines" :key="mIdx" class="text-dark small mb-2 d-flex align-items-start">
                        <i class="bi bi-check-circle-fill text-success me-2 mt-1" style="font-size: 0.7rem;"></i> {{ medStr }}
                      </li>
                    </ul>
                  </div>
                </div>
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
const activePatient = ref({
  appointmentId: '',
  petId: '',
  petName: '',
  customerName: ''
});

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
    prescriptions: [] as Array<{
      medicineId: number | null;
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

const loadingHistory = ref(false);
const submitting = ref(false);
const errorMessage = ref('');

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
    
    // Load Medical history of the pet
    fetchPetHistory(parseInt(petId, 10));
  }

  // Load list of medicines for prescription form
  fetchMedicines();
});

const fetchMedicines = async () => {
  try {
    const res = await api.get('/medicines');
    medicineOptions.value = res.data || [];
  } catch (err) {
    console.error('Lỗi tải danh mục thuốc:', err);
  }
};

const fetchPetHistory = async (petId: number) => {
  loadingHistory.value = true;
  try {
    const res = await api.get(`/medical-records/pet/${petId}`);
    medicalHistory.value = res.data || [];
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

  submitting.value = true;
  errorMessage.value = '';

  try {
    const payload = {
      appointmentId: form.value.appointmentId,
      petId: form.value.petId,
      subjective: form.value.subjective,
      objective: form.value.objective,
      assessment: form.value.assessment,
      plan: {
        treatmentDirections: form.value.plan.treatmentDirections,
        careInstructions: form.value.plan.careInstructions,
        followUpDate: form.value.plan.followUpDate ? new Date(form.value.plan.followUpDate).toISOString() : null,
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
  left: 9px;
  bottom: 0;
  width: 2px;
  background-color: #e2e8f0;
}
.timeline-item:last-child .timeline-line {
  display: none;
}
.timeline-circle {
  position: absolute;
  top: 12px;
  left: 4px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid white;
  z-index: 1;
}
</style>
