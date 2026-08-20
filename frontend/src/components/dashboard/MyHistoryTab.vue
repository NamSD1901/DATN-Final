<template>
  <div class="myhistory-tab">
    
    <!-- Header -->
    <div class="history-hero mb-4">
      <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h3 class="fw-bold text-dark mb-1">
            <i class="bi bi-journal-medical me-2" style="color: var(--primary-gold);"></i>
            {{ $t('history.title') }}
          </h3>
          <p class="text-muted mb-0 small">{{ $t('history.subtitle') }}</p>
        </div>
      </div>
    </div>

    <!-- Pet Selection Pills -->
    <div class="pet-selector-container mb-4">
      <h5 class="fw-semibold text-dark mb-3">
        <i class="bi bi-tag-fill me-2 text-warning"></i>{{ $t('history.yourPets') }}
      </h5>
      <div class="d-flex gap-3 flex-wrap">
        <button
          v-for="pet in myPets"
          :key="pet.id"
          class="pet-pill-btn d-flex align-items-center gap-2"
          :class="{ active: selectedPetId === pet.id }"
          @click="selectPet(pet.id)"
        >
          <span class="pet-emoji">{{ getSpeciesEmoji(pet.species) }}</span>
          <span class="pet-name">{{ pet.name }}</span>
        </button>
      </div>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">{{ $t('history.syncing') }}</p>
    </div>

    <!-- Error state -->
    <div v-else-if="errorMsg" class="alert alert-danger rounded-4 border-0 shadow-sm py-3 px-4">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- No Pet Selected -->
    <div v-else-if="selectedPetId === null" class="empty-history text-center py-5">
      <div class="empty-icon">🐶</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">{{ $t('history.noPetSelectedTitle') }}</h5>
      <p class="text-muted small mb-0">{{ $t('history.noPetSelectedDesc') }}</p>
    </div>

    <!-- Empty history for selected pet -->
    <div v-else-if="records.length === 0" class="empty-history text-center py-5">
      <div class="empty-icon">📁</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">{{ $t('history.noHistoryTitle') }}</h5>
      <p class="text-muted small mb-0">{{ $t('history.noHistoryDesc') }}</p>
    </div>

    <div v-else>
      <!-- Vitals Chart Section -->
      <div class="chart-container mb-5" v-if="chartData.labels.length > 1">
        <h5 class="fw-semibold mb-3"><i class="bi bi-graph-up-arrow me-2 text-primary"></i>Biểu đồ Sinh hiệu</h5>
        <div class="chart-wrapper p-3 bg-white rounded-4 shadow-sm border">
          <Line :data="chartData" :options="chartOptions" style="max-height: 250px;" />
        </div>
      </div>

      <!-- Timeline records -->
      <h5 class="fw-semibold mb-4"><i class="bi bi-clock-history me-2 text-success"></i>Dòng thời gian y khoa</h5>
      <div class="medical-history-timeline mt-2" id="medical-records-export-area">
        <div class="timeline-container" id="historyTimeline">
          <div v-for="(record, index) in records" :key="record.recordId" class="timeline-item">
            <!-- Timeline dot -->
            <div class="timeline-badge" :class="record.recordType === 'Vaccination' ? 'bg-success' : 'bg-primary'">
              <div class="badge-inner">
                <i v-if="record.recordType === 'Vaccination'" class="bi bi-shield-plus text-white"></i>
                <i v-else class="bi bi-heart-pulse-fill text-white"></i>
              </div>
            </div>
            
            <!-- Timeline card - click to open modal -->
            <div
              class="timeline-card card border-0 bg-transparent mb-3 shadow-sm"
              style="cursor: pointer;"
              @click="openRecordModal(record)"
            >
              <div class="card-body bg-white rounded-4 p-3 border timeline-card-hover">
                <div class="d-flex flex-column w-100">
                  <div class="d-flex justify-content-between align-items-center w-100 mb-1">
                    <span class="visit-date fw-bold text-dark">
                      {{ formatDateFull(record.visitDate) }}
                    </span>
                    <span class="badge rounded-pill" :class="record.recordType === 'Vaccination' ? 'bg-success' : 'bg-primary'">
                      {{ record.serviceName || (record.recordType === 'Vaccination' ? 'Tiêm phòng' : 'Khám bệnh') }}
                    </span>
                  </div>
                  <div class="d-flex justify-content-between align-items-center w-100">
                    <span class="doctor-badge mb-0 text-muted small">
                      <i class="bi bi-person-badge me-1"></i>BS: <span class="fw-semibold text-dark">{{ record.doctorName || 'Chưa rõ' }}</span>
                    </span>
                    <span v-if="record.diagnosis" class="text-truncate text-muted small ms-2" style="max-width: 220px;">
                      {{ record.diagnosis }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Chi Tiết Lịch Sử Y Tế -->
    <div class="modal fade" id="medicalRecordModal" tabindex="-1" aria-labelledby="medicalRecordModalLabel" aria-hidden="true" ref="medicalRecordModalRef">
      <div class="modal-dialog modal-dialog-centered modal-xl modal-dialog-scrollable">
        <div class="modal-content border-0 shadow-lg rounded-4" v-if="selectedRecord">
          <div class="modal-header bg-light rounded-top-4 pb-2" style="border-bottom: 1px solid #e2e8f0;">
            <h5 class="modal-title fw-bold text-dark" id="medicalRecordModalLabel">
              <i class="bi bi-journal-text text-primary me-2"></i>Chi Tiết Hồ Sơ Y Tế
            </h5>
            <button type="button" class="btn-close shadow-none" @click="closeModal" aria-label="Close"></button>
          </div>
          <div class="modal-body p-4 bg-white" :id="'record-content-' + selectedRecord.recordId">

            <!-- Action Bar -->
            <div v-if="selectedRecord.invoiceId" class="d-flex justify-content-end mb-3 action-bar hide-on-print">
              <div class="badge bg-light text-dark border d-flex align-items-center px-3 py-2 rounded-3">
                <i class="bi bi-receipt me-2 text-secondary"></i>
                <span class="me-2">Hóa đơn: <strong>#INV-{{ selectedRecord.invoiceId }}</strong></span>
                <span v-if="selectedRecord.invoiceStatus === 'paid'" class="badge bg-success">Đã thanh toán</span>
                <span v-else class="badge bg-warning text-dark">Chờ thanh toán</span>
              </div>
            </div>

            <!-- PDF Header (Only visible in PDF) -->
            <div class="pdf-header d-none mb-4 text-center">
              <h2 class="text-primary fw-bold mb-1">MYPET CLINIC</h2>
              <p class="mb-0">Hồ Sơ Bệnh Án Điện Tử</p>
              <hr>
            </div>
            
            <!-- Patient Info Summary -->
            <div class="d-flex justify-content-between align-items-center mb-4 pb-3" style="border-bottom: 1px solid #f1f5f9;">
              <div>
                <h5 class="fw-bold mb-1 text-primary">{{ myPets.find(p => p.id === selectedPetId)?.name || 'Thú cưng' }}</h5>
                <div class="text-muted small">Ngày khám: {{ formatDateFull(selectedRecord.visitDate) }} | BS: {{ selectedRecord.doctorName || 'Chưa rõ' }}</div>
              </div>
              <div>
                <span class="badge rounded-pill px-3 py-2" :class="selectedRecord.recordType === 'Vaccination' ? 'bg-success' : 'bg-primary'">
                  {{ selectedRecord.serviceName || (selectedRecord.recordType === 'Vaccination' ? 'Tiêm phòng' : 'Khám bệnh') }}
                </span>
              </div>
            </div>

            <!-- Vitals Row -->
            <div class="vitals-grid mb-4">
              <div class="vital-card shadow-sm">
                <span class="vital-icon">⚖️</span>
                <span class="vital-label">Cân nặng</span>
                <span class="vital-val">{{ selectedRecord.weight ? selectedRecord.weight + ' kg' : '—' }}</span>
              </div>
              <div class="vital-card shadow-sm">
                <span class="vital-icon">🌡️</span>
                <span class="vital-label">Nhiệt độ</span>
                <span class="vital-val">{{ selectedRecord.temperature ? selectedRecord.temperature + ' °C' : '—' }}</span>
              </div>
              <template v-if="selectedRecord.recordType === 'Vaccination' && selectedVaccinationRecord">
                <div class="vital-card shadow-sm" v-if="selectedVaccinationRecord.heartRate">
                  <span class="vital-icon">💓</span>
                  <span class="vital-label">Nhịp tim</span>
                  <span class="vital-val">{{ selectedVaccinationRecord.heartRate }} l/p</span>
                </div>
                <div class="vital-card shadow-sm" v-if="selectedVaccinationRecord.respiratoryRate">
                  <span class="vital-icon">😮‍💨</span>
                  <span class="vital-label">Nhịp thở</span>
                  <span class="vital-val">{{ selectedVaccinationRecord.respiratoryRate }} l/p</span>
                </div>
              </template>
            </div>

            <div v-if="loadingModal" class="text-center py-5">
              <div class="spinner-border text-success" role="status"></div>
              <p class="text-muted mt-2">Đang tải chi tiết tiêm chủng...</p>
            </div>

            <!-- SOAP Details (Bệnh án thường) -->
            <div v-if="!loadingModal && selectedRecord.recordType !== 'Vaccination'" class="medical-details-premium mb-4">
              
              <!-- Subjective (S) -->
              <div v-if="selectedRecord.medicalHistory" class="soap-block mb-4">
                <div class="soap-header text-primary mb-2">
                  <i class="bi bi-file-earmark-medical-fill me-2"></i>Tiền sử &amp; Lý do khám (S)
                </div>
                <div class="soap-body bg-primary bg-opacity-10 border-start border-primary border-4 p-3 rounded-end-3">
                  <div class="d-flex flex-wrap gap-2">
                    <div v-for="(item, idx) in parseSoapField(selectedRecord.medicalHistory)" :key="idx"
                         class="soap-badge" v-html="formatSoapItem(item)">
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- Objective (O) -->
              <div v-if="selectedRecord.clinicalSigns" class="soap-block mb-4">
                <div class="soap-header text-success mb-2">
                  <i class="bi bi-heart-pulse-fill me-2"></i>Khám lâm sàng (O)
                </div>
                <div class="soap-body bg-success bg-opacity-10 border-start border-success border-4 p-3 rounded-end-3">
                  <div class="d-flex flex-wrap gap-2">
                    <div v-for="(item, idx) in parseSoapField(selectedRecord.clinicalSigns)" :key="idx"
                         class="soap-badge" v-html="formatSoapItem(item)">
                    </div>
                  </div>
                </div>
              </div>

              <!-- Assessment (A) -->
              <div class="soap-block mb-4">
                <div class="soap-header text-danger mb-2">
                  <i class="bi bi-exclamation-triangle-fill me-2"></i>Chẩn đoán của bác sĩ (A)
                </div>
                <div class="soap-body bg-danger bg-opacity-10 border-start border-danger border-4 p-3 rounded-end-3">
                  <div class="d-flex flex-column gap-2">
                    <div v-for="(item, idx) in parseSoapField(selectedRecord.diagnosis || 'Chưa có chẩn đoán')" :key="idx"
                         class="soap-badge fs-6 py-2 px-3" v-html="formatSoapItem(item)" style="border-left: 3px solid #ef4444;">
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- Plan (P) -->
              <div v-if="selectedRecord.treatmentPlan" class="soap-block mb-4">
                <div class="soap-header mb-2" style="color: #0891b2 !important;">
                  <i class="bi bi-capsule me-2"></i>Kế hoạch điều trị (P)
                </div>
                <div class="soap-body bg-info bg-opacity-10 border-start border-info border-4 p-3 rounded-end-3">
                  <div class="d-flex flex-wrap gap-2">
                    <div v-for="(item, idx) in parseSoapField(selectedRecord.treatmentPlan)" :key="idx"
                         class="soap-badge" v-html="formatSoapItem(item)">
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- Attachments if any -->
              <div v-if="selectedRecord.attachments && selectedRecord.attachments.length > 0" class="soap-block mb-4">
                <div class="soap-header mb-2" style="color: #6366f1 !important;">
                  <i class="bi bi-images me-2"></i>Hình ảnh đính kèm (Cận lâm sàng)
                </div>
                <div class="d-flex flex-wrap gap-3 mt-2">
                  <div v-for="(img, idx) in selectedRecord.attachments" :key="idx" class="position-relative">
                    <img :src="getImageUrl(img)" class="rounded-4 border shadow-sm" style="width: 120px; height: 120px; object-fit: cover; cursor: zoom-in;" alt="Attachment" @click.stop="openLightbox(selectedRecord.attachments.map((u: string) => getImageUrl(u)), idx)" />
                  </div>
                </div>
              </div>

              <!-- Care Instructions -->
              <div v-if="selectedRecord.careInstructions || selectedRecord.doctorNotes" class="soap-block mt-4">
                <div class="soap-header text-warning-emphasis mb-2">
                  <i class="bi bi-info-circle-fill me-2"></i>Lời dặn dò chăm sóc
                </div>
                <div class="soap-body bg-warning bg-opacity-10 border-start border-warning border-4 p-3 rounded-end-3">
                  <div class="text-dark fw-medium" style="line-height: 1.6;">{{ selectedRecord.careInstructions || selectedRecord.doctorNotes }}</div>
                </div>
              </div>
            </div>

            <!-- SOAP Details (Tiêm chủng) -->
            <div v-if="!loadingModal && selectedRecord.recordType === 'Vaccination' && selectedVaccinationRecord" class="medical-details-premium mb-4">
              
              <!-- Subjective (S) -->
              <div class="soap-block mb-4">
                <div class="soap-header text-primary mb-2">
                  <i class="bi bi-file-earmark-medical-fill me-2"></i>Tiền sử & Lý do khám (S)
                </div>
                <div class="soap-body bg-primary bg-opacity-10 border-start border-primary border-4 p-3 rounded-end-3">
                  <div class="d-flex flex-wrap gap-2">
                    <div class="soap-badge"><span class="text-muted fw-bold me-1">Lý do:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.reasonForVisit || 'Tiêm cơ bản' }}</span></div>
                    <div class="soap-badge"><span class="text-muted fw-bold me-1">Ăn uống:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.eatingStatus || 'Bình thường' }}</span></div>
                    <div v-if="selectedVaccinationRecord.hasVomitingOrDiarrhea" class="soap-badge bg-danger bg-opacity-10 border-danger"><span class="text-danger fw-bold">Nôn mửa / Tiêu chảy</span></div>
                    <div v-if="selectedVaccinationRecord.hasCoughOrSneeze" class="soap-badge bg-warning bg-opacity-10 border-warning"><span class="text-warning-emphasis fw-bold">Ho / Hắt hơi</span></div>
                    <div v-if="selectedVaccinationRecord.isAllergic" class="soap-badge bg-danger bg-opacity-10 border-danger"><span class="text-danger fw-bold me-1">Dị ứng:</span><span class="text-danger">{{ selectedVaccinationRecord.allergyDetails }}</span></div>
                    <div v-if="selectedVaccinationRecord.previousVaccineHistory" class="soap-badge"><span class="text-muted fw-bold me-1">Tiền sử vắc-xin:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.previousVaccineHistory }}</span></div>
                  </div>
                </div>
              </div>
              
              <!-- Objective (O) -->
              <div class="soap-block mb-4">
                <div class="soap-header text-info mb-2">
                  <i class="bi bi-heart-pulse-fill me-2"></i>Khám lâm sàng (O)
                </div>
                <div class="soap-body bg-info bg-opacity-10 border-start border-info border-4 p-3 rounded-end-3">
                  <div class="d-flex flex-wrap gap-2">
                    <div class="soap-badge"><span class="text-muted fw-bold me-1">Tinh thần:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.mentalStatus || 'Linh hoạt' }}</span></div>
                    <div class="soap-badge"><span class="text-muted fw-bold me-1">Niêm mạc:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.mucosaStatus || 'Hồng hào' }}</span></div>
                    <div class="soap-badge" v-if="selectedVaccinationRecord.dehydrationPercent"><span class="text-muted fw-bold me-1">Mất nước:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.dehydrationPercent }}%</span></div>
                  </div>
                </div>
              </div>

              <!-- Assessment (A) -->
              <div class="soap-block mb-4">
                <div class="soap-header text-success mb-2">
                  <i class="bi bi-shield-check me-2"></i>Đánh giá (A)
                </div>
                <div class="soap-body bg-success bg-opacity-10 border-start border-success border-4 p-3 rounded-end-3">
                  <div class="d-flex flex-wrap gap-2">
                    <div class="soap-badge" :class="{'bg-success bg-opacity-25 border-success': selectedVaccinationRecord.clinicalAssessment === 'Đủ điều kiện', 'bg-danger bg-opacity-25 border-danger': selectedVaccinationRecord.clinicalAssessment !== 'Đủ điều kiện'}">
                      <span class="fw-bold me-1" :class="selectedVaccinationRecord.clinicalAssessment === 'Đủ điều kiện' ? 'text-success' : 'text-danger'">Kết luận:</span>
                      <span class="text-dark fw-bold">{{ selectedVaccinationRecord.clinicalAssessment }}</span>
                    </div>
                    <div class="soap-badge" v-if="selectedVaccinationRecord.doctorRemarks"><span class="text-muted fw-bold me-1">Bác sĩ ghi chú:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.doctorRemarks }}</span></div>
                  </div>
                </div>
              </div>
              
              <!-- Plan (P) -->
              <div class="soap-block mb-4" v-if="selectedVaccinationRecord.vaccineName">
                <div class="soap-header mb-2" style="color: #8b5cf6 !important;">
                  <i class="bi bi-capsule me-2"></i>Kế hoạch tiêm (P)
                </div>
                <div class="soap-body bg-opacity-10 border-start border-4 p-3 rounded-end-3" style="background-color: rgba(139, 92, 246, 0.1); border-color: #8b5cf6 !important;">
                  <div class="d-flex flex-column gap-2">
                    <div class="soap-badge fs-6 py-2 px-3" style="border-left: 3px solid #8b5cf6;">
                      <span class="fw-bold" style="color: #8b5cf6">Vắc-xin:</span> <span class="text-dark fw-bold ms-1">{{ selectedVaccinationRecord.vaccineName }}</span>
                    </div>
                    <div class="d-flex flex-wrap gap-2">
                      <div class="soap-badge"><span class="text-muted fw-bold me-1">Lô:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.batchNumber || '—' }}</span></div>
                      <div class="soap-badge"><span class="text-muted fw-bold me-1">Liều lượng:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.dose || 1 }} ml</span></div>
                      <div class="soap-badge"><span class="text-muted fw-bold me-1">Đường tiêm:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.route || 'Dưới da (SC)' }}</span></div>
                      <div class="soap-badge" v-if="selectedVaccinationRecord.injectionSite"><span class="text-muted fw-bold me-1">Vị trí:</span><span class="text-dark fw-medium">{{ selectedVaccinationRecord.injectionSite }}</span></div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Attachments if any -->
              <div v-if="selectedVaccinationRecord.attachments && selectedVaccinationRecord.attachments.length > 0" class="soap-block mt-4">
                <div class="soap-header mb-2" style="color: #6366f1 !important;">
                  <i class="bi bi-images me-2"></i>Hình ảnh đính kèm
                </div>
                <div class="d-flex flex-wrap gap-3 mt-2">
                  <div v-for="(img, idx) in selectedVaccinationRecord.attachments" :key="idx" class="position-relative">
                    <img :src="getImageUrl(img)" class="rounded-4 border shadow-sm" style="width: 120px; height: 120px; object-fit: cover; cursor: zoom-in;" alt="Attachment" @click.stop="openLightbox(selectedVaccinationRecord.attachments.map((u: string) => getImageUrl(u)), idx)" />
                  </div>
                </div>
              </div>

            </div>


            <!-- Prescribed Medicines -->
            <div v-if="selectedRecord.prescribedMedicines && selectedRecord.prescribedMedicines.length > 0" class="medicine-section mb-4">
              <span class="detail-label mb-2"><i class="bi bi-capsule text-primary me-1"></i>Đơn thuốc chỉ định:</span>
              <div class="table-responsive mt-2">
                <table class="table table-sm table-bordered medicine-table">
                  <thead class="table-light">
                    <tr>
                      <th>Tên thuốc</th>
                      <th>Số lượng</th>
                      <th>Liều dùng</th>
                      <th>Cách dùng</th>
                      <th>Lời dặn</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(med, mIndex) in selectedRecord.prescribedMedicines" :key="mIndex">
                      <td class="fw-semibold text-primary">{{ med.medicineName }}</td>
                      <td>{{ med.quantity || '-' }}</td>
                      <td>{{ med.dosage || '-' }}</td>
                      <td>{{ med.frequency || '-' }} ({{ med.durationDays ? med.durationDays + ' ngày' : '-' }})</td>
                      <td class="text-muted small">{{ med.instruction || '-' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Doctor Notes -->
            <div v-if="selectedRecord.doctorNotes" class="detail-block note-block mb-3">
              <span class="detail-label text-muted"><i class="bi bi-journal-text me-1"></i>Ghi chú của bác sĩ:</span>
              <p class="detail-content text-muted mb-0 small">{{ selectedRecord.doctorNotes }}</p>
            </div>

            <!-- Follow Up -->
            <div v-if="selectedRecord.followUpDate" class="detail-block bg-warning bg-opacity-10 border-warning mt-3">
              <span class="detail-label" style="color: #92400e;"><i class="bi bi-calendar-event me-1"></i>Ngày hẹn tái khám:</span>
              <p class="detail-content text-dark fw-bold mb-0 small">{{ formatDateFull(selectedRecord.followUpDate) }}</p>
            </div>

          </div>
        </div>
      </div>
    </div>

  </div>

  <!-- ===== LIGHTBOX MODAL ===== -->
  <Teleport to="body">
    <Transition name="lightbox-fade">
      <div v-if="lightbox.show" class="lightbox-overlay" @click.self="closeLightbox">
        <button class="btn-close-lightbox" @click="closeLightbox"><i class="bi bi-x-lg"></i></button>
        
        <button class="btn-nav-lightbox prev" v-if="lightbox.images.length > 1" @click.stop="prevImage">
          <i class="bi bi-chevron-left"></i>
        </button>
        
        <div class="lightbox-content-wrapper" @click.self="closeLightbox">
          <img :src="lightbox.images[lightbox.currentIndex]" class="lightbox-image" />
          <div class="lightbox-counter" v-if="lightbox.images.length > 1">
            {{ lightbox.currentIndex + 1 }} / {{ lightbox.images.length }}
          </div>
        </div>

        <button class="btn-nav-lightbox next" v-if="lightbox.images.length > 1" @click.stop="nextImage">
          <i class="bi bi-chevron-right"></i>
        </button>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, nextTick } from 'vue';
import { Modal } from 'bootstrap';
import api from '../../services/api';
// @ts-ignore
import html2pdf from 'html2pdf.js';

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js'
import { Line } from 'vue-chartjs'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

// ===== Types =====
interface Pet {
  id: number;
  name: string;
  species: string;
  breed?: string;
  weight?: number;
}

interface PrescribedMedicine {
  medicineName: string;
  dosage?: string;
  frequency?: string;
  durationDays?: number;
  quantity?: number;
  instruction?: string;
}

interface MedicalRecord {
  recordId: number;
  appointmentId: number;
  serviceName: string;
  recordType: string;
  visitDate: string;
  doctorName: string;
  weight?: number;
  temperature?: number;
  medicalHistory?: string;
  clinicalSigns?: string;
  diagnosis: string;
  treatmentPlan?: string;
  careInstructions: string;
  doctorNotes?: string;
  followUpDate?: string;
  prescribedMedicines: PrescribedMedicine[];
  invoiceId?: number;
  invoiceStatus?: string;
  invoiceTotalAmount?: number;
  attachments?: string[];
}

// ===== State =====
const myPets = ref<Pet[]>([]);
const selectedPetId = ref<number | null>(null);
const records = ref<MedicalRecord[]>([]);
const loading = ref(false);
const errorMsg = ref('');

// Modal state
const medicalRecordModalRef = ref<HTMLElement | null>(null);
const selectedRecord = ref<MedicalRecord | null>(null);
const selectedVaccinationRecord = ref<any>(null);
const loadingModal = ref(false);
let modalInstance: Modal | null = null;

const openRecordModal = async (record: MedicalRecord) => {
  // Guard: only open if record has meaningful content
  if (!record || (!record.medicalHistory && !record.clinicalSigns && !record.diagnosis && !record.treatmentPlan && !record.prescribedMedicines?.length)) {
    return;
  }
  selectedRecord.value = record;
  selectedVaccinationRecord.value = null;

  if (record.recordType === 'Vaccination') {
    loadingModal.value = true;
    try {
      const res = await api.get(`/vaccinations/appointments/${record.appointmentId}`);
      selectedVaccinationRecord.value = res.data;
    } catch (e) {
      console.error('Failed to fetch vaccination details', e);
    } finally {
      loadingModal.value = false;
    }
  }

  await nextTick();
  if (!modalInstance && medicalRecordModalRef.value) {
    modalInstance = new Modal(medicalRecordModalRef.value, { backdrop: true });
  }
  modalInstance?.show();
};

const closeModal = () => {
  modalInstance?.hide();
};

// ===== Lightbox Logic =====
const lightbox = ref({
  show: false,
  images: [] as string[],
  currentIndex: 0
});

const openLightbox = (images: string[], index: number) => {
  lightbox.value.images = images;
  lightbox.value.currentIndex = index;
  lightbox.value.show = true;
  document.body.style.overflow = 'hidden';
};

const closeLightbox = () => {
  lightbox.value.show = false;
  document.body.style.overflow = '';
};

const prevImage = () => {
  if (lightbox.value.currentIndex > 0) {
    lightbox.value.currentIndex--;
  } else {
    lightbox.value.currentIndex = lightbox.value.images.length - 1;
  }
};

const nextImage = () => {
  if (lightbox.value.currentIndex < lightbox.value.images.length - 1) {
    lightbox.value.currentIndex++;
  } else {
    lightbox.value.currentIndex = 0;
  }
};

const getImageUrl = (url: string) => {
  if (!url) return '';
  if (url.startsWith('http')) return url;
  let baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7284';
  baseUrl = baseUrl.replace(/\/api$/, '');
  return `${baseUrl}${url}`;
};

const parseSoapField = (text: string | undefined) => {
  if (!text) return [];
  if (text.includes('|')) {
    return text.split('|').map(item => item.trim()).filter(item => item.length > 0);
  }
  
  if (text.includes(', ') && (text.match(/:/g) || []).length > 1) {
    const parts = text.split(', ');
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
  
  return [text.trim()];
};

const formatSoapItem = (item: string) => {
  const colonIndex = item.indexOf(':');
  if (colonIndex > -1) {
    const key = item.substring(0, colonIndex);
    const value = item.substring(colonIndex + 1);
    return `<span class="text-muted fw-bold me-1">${key}:</span><span class="text-dark fw-medium">${value}</span>`;
  }
  return `<span class="text-dark fw-medium">${item}</span>`;
};

// ===== API =====
const fetchPets = async () => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get('/mypets');
    myPets.value = res.data;
    if (myPets.value.length > 0) {
      selectedPetId.value = myPets.value[0].id;
      await fetchMedicalHistory(myPets.value[0].id);
    }
  } catch (err: any) {
    errorMsg.value = 'Không thể tải danh sách thú cưng. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
};

const fetchMedicalHistory = async (petId: number) => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get(`/my-appointments/pets/${petId}/medical-history`);
    records.value = res.data;
  } catch (err: any) {
    errorMsg.value = 'Không thể tải lịch sử bệnh án. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
};

const selectPet = async (petId: number) => {
  selectedPetId.value = petId;
  await fetchMedicalHistory(petId);
};

// ===== Chart Logic =====
const chartData = computed(() => {
  const sorted = [...records.value].reverse();
  const labels = sorted.map(r => new Date(r.visitDate).toLocaleDateString('vi-VN'));
  const weights = sorted.map(r => r.weight || null);
  const temps = sorted.map(r => r.temperature || null);

  return {
    labels,
    datasets: [
      {
        label: 'Cân nặng (kg)',
        backgroundColor: '#3b82f6',
        borderColor: '#3b82f6',
        data: weights,
        yAxisID: 'y'
      },
      {
        label: 'Nhiệt độ (°C)',
        backgroundColor: '#ef4444',
        borderColor: '#ef4444',
        data: temps,
        yAxisID: 'y1'
      }
    ]
  };
});

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: {
    mode: 'index' as const,
    intersect: false,
  },
  plugins: {
    legend: { position: 'top' as const }
  },
  scales: {
    y: {
      type: 'linear' as const,
      display: true,
      position: 'left' as const,
      title: { display: true, text: 'Cân nặng (kg)' }
    },
    y1: {
      type: 'linear' as const,
      display: true,
      position: 'right' as const,
      grid: { drawOnChartArea: false },
      title: { display: true, text: 'Nhiệt độ (°C)' }
    }
  }
};

// ===== PDF Export =====
const downloadPDF = (recordId: number) => {
  const element = document.getElementById(`record-content-${recordId}`);
  if (!element) return;
  
  const clone = element.cloneNode(true) as HTMLElement;
  
  const pdfHeader = clone.querySelector('.pdf-header');
  if (pdfHeader) pdfHeader.classList.remove('d-none');
  
  const actionBar = clone.querySelector('.hide-on-print');
  if (actionBar) actionBar.remove();

  const tempDiv = document.createElement('div');
  tempDiv.appendChild(clone);
  tempDiv.style.padding = '20px';
  tempDiv.style.backgroundColor = 'white';
  tempDiv.style.color = 'black';

  const opt = {
    margin:       0.5,
    filename:     `BenhAn_MyPetClinic_${recordId}.pdf`,
    image:        { type: 'jpeg', quality: 0.98 },
    html2canvas:  { scale: 2 },
    jsPDF:        { unit: 'in', format: 'letter', orientation: 'portrait' }
  };

  html2pdf().set(opt).from(tempDiv).save();
};

// ===== Helpers =====
const getSpeciesEmoji = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': '🐕', 'Mèo': '🐈', 'Thỏ': '🐇', 'Chim': '🦜', 'Cá': '🐟', 'Bò sát': '🦎',
  };
  return map[species ?? ''] || '🐾';
};

const formatDateFull = (dateStr: string): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

// ===== Lifecycle =====
onMounted(() => {
  fetchPets();
  if (medicalRecordModalRef.value) {
    modalInstance = new Modal(medicalRecordModalRef.value, { backdrop: 'static' });
  }
});
</script>

<style scoped>
/* ===== Layout ===== */
.myhistory-tab {
  padding: 0;
  color: #1f2937;
}

/* ===== Hero ===== */
.history-hero {
  background: linear-gradient(135deg, rgba(251, 191, 36, 0.08) 0%, rgba(16, 185, 129, 0.08) 100%);
  backdrop-filter: blur(12px);
  border-radius: 20px;
  padding: 1.75rem 2rem;
  border: 1px solid rgba(251, 191, 36, 0.15);
  box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.03);
}

/* ===== Pet Selector ===== */
.pet-pill-btn {
  background: rgba(255, 255, 255, 0.65);
  backdrop-filter: blur(8px);
  border: 1.5px solid #e5e7eb;
  padding: 0.6rem 1.2rem;
  border-radius: 30px;
  font-size: 0.92rem;
  font-weight: 600;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.02);
}

.pet-pill-btn:hover {
  transform: translateY(-2px);
  background: white;
  border-color: #fbbf24;
  box-shadow: 0 4px 12px rgba(251, 191, 36, 0.15);
}

.pet-pill-btn.active {
  background: linear-gradient(135deg, #fbbf24, #d97706);
  border-color: transparent;
  color: white;
  box-shadow: 0 8px 20px rgba(217, 119, 6, 0.3);
}

.pet-emoji {
  font-size: 1.2rem;
}

/* ===== Empty State ===== */
.empty-history {
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(12px);
  border-radius: 24px;
  padding: 4.5rem 2rem;
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.04);
  border: 2px dashed rgba(229, 231, 235, 0.8);
}

.empty-icon {
  font-size: 3.5rem;
  animation: float 3s ease-in-out infinite;
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-8px); }
}

/* ===== Timeline ===== */
.medical-history-timeline {
  position: relative;
  padding-left: 2rem;
}

.timeline-container {
  position: relative;
}

.timeline-container::before {
  content: '';
  position: absolute;
  top: 0;
  bottom: 0;
  left: -1.25rem;
  width: 3px;
  background: linear-gradient(180deg, rgba(59, 130, 246, 0.4) 0%, rgba(16, 185, 129, 0.4) 100%);
  border-radius: 2px;
}

.timeline-item {
  position: relative;
  margin-bottom: 1.5rem;
}

.timeline-badge {
  position: absolute;
  left: -2.1rem;
  top: 0.8rem;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 0 0 4px #f3f4f6;
  z-index: 2;
}

.badge-inner {
  font-size: 0.8rem;
}

.timeline-card-hover {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.timeline-card-hover:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.08) !important;
}

/* ===== Vitals ===== */
.vitals-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
  gap: 12px;
}

.vital-card {
  background: rgba(243, 244, 246, 0.6);
  border: 1px solid rgba(229, 231, 235, 0.5);
  border-radius: 12px;
  padding: 0.6rem 0.8rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.vital-icon {
  font-size: 1.1rem;
  margin-bottom: 2px;
}

.vital-label {
  font-size: 0.75rem;
  color: #6b7280;
  font-weight: 500;
}

.vital-val {
  font-size: 0.95rem;
  font-weight: 700;
  color: #1f2937;
  margin-top: 2px;
}

/* ===== Detail blocks ===== */
.detail-block {
  background: rgba(249, 250, 251, 0.8);
  padding: 0.85rem 1rem;
  border-radius: 12px;
  border: 1px solid rgba(229, 231, 235, 0.5);
}

.detail-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: #4b5563;
  margin-bottom: 4px;
}

.detail-content {
  font-size: 0.92rem;
  color: #1f2937;
  line-height: 1.5;
  margin-bottom: 0;
}

.note-block {
  background: rgba(107, 114, 128, 0.03);
  border-left: 3px solid #6b7280;
}

/* ===== Medicine Table ===== */
.medicine-table {
  font-size: 0.85rem;
  border-radius: 8px;
  overflow: hidden;
}
.medicine-table th {
  background-color: #f8fafc;
  color: #475569;
  font-weight: 600;
  border-bottom-width: 2px;
}
.medicine-table td {
  vertical-align: middle;
}

/* ===== SOAP UI ===== */
.soap-header {
  font-size: 0.85rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.soap-badge {
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(4px);
  border: 1px solid rgba(0, 0, 0, 0.05);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
  border-radius: 8px;
  padding: 0.4rem 0.8rem;
  font-size: 0.92rem;
  display: inline-block;
  transition: transform 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.soap-badge:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.05);
}
/* ===== Lightbox ===== */
.lightbox-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.9);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(5px);
}
.lightbox-content-wrapper {
  position: relative;
  max-width: 90vw;
  max-height: 90vh;
  display: flex;
  align-items: center;
  justify-content: center;
}
.lightbox-image {
  max-width: 100%;
  max-height: 90vh;
  object-fit: contain;
  border-radius: 8px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.5);
}
.btn-close-lightbox {
  position: absolute;
  top: 20px;
  right: 30px;
  background: rgba(255, 255, 255, 0.2);
  border: none;
  color: white;
  font-size: 1.5rem;
  width: 45px;
  height: 45px;
  border-radius: 50%;
  cursor: pointer;
  z-index: 10000;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.btn-close-lightbox:hover {
  background: rgba(255, 255, 255, 0.4);
  transform: scale(1.1);
}
.btn-nav-lightbox {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  background: rgba(255, 255, 255, 0.1);
  border: none;
  color: white;
  font-size: 2rem;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  cursor: pointer;
  z-index: 10000;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}
.btn-nav-lightbox:hover {
  background: rgba(255, 255, 255, 0.3);
}
.btn-nav-lightbox.prev {
  left: 20px;
}
.btn-nav-lightbox.next {
  right: 20px;
}
.lightbox-counter {
  position: absolute;
  bottom: -40px;
  left: 50%;
  transform: translateX(-50%);
  color: white;
  font-size: 1rem;
  background: rgba(255,255,255,0.2);
  padding: 4px 12px;
  border-radius: 20px;
}
.lightbox-fade-enter-active,
.lightbox-fade-leave-active {
  transition: opacity 0.3s ease;
}
.lightbox-fade-enter-from,
.lightbox-fade-leave-to {
  opacity: 0;
}
</style>
