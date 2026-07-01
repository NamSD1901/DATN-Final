<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <Users class="icon-users" /> Bác Sĩ & Chuyên Gia
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">ĐỘI NGŨ NHÂN VIÊN</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Gặp gỡ đội ngũ y bác sĩ giàu lòng nhân ái, có trình độ chuyên môn cao và tận tụy hết lòng vì sức khỏe của thú cưng.
        </p>
      </div>
    </section>

    <!-- Team Content -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <!-- Message of Commitment -->
        <div class="text-center max-w-2xl mx-auto mb-5 section-commitment">
          <h3 class="fw-bold mb-3">Cam Kết Y Đức & Trách Nhiệm</h3>
          <p class="text-muted">
            Tại MyPetClinic, mỗi bác sĩ và nhân viên không chỉ là người chữa bệnh mà còn là những người bạn đáng tin cậy của thú cưng. Chúng tôi liên tục cập nhật các phác đồ y khoa hiện đại và học hỏi các kỹ thuật điều trị mới nhất trên thế giới.
          </p>
        </div>

        <!-- Doctors Grid -->
        <h4 class="fw-bold mb-4 text-warning border-bottom pb-2 title-category-team">
          <Award class="icon-category-team" /> Các Bác Sĩ Trụ Cột
        </h4>
        
        <div class="doctors-grid mb-5">
          <!-- Doctor 1 -->
          <div class="doctor-card-wrapper text-center">
            <div class="card border-0 glass-card h-100 p-4 doctor-card">
              <img src="../assets/bsi1.png" class="doctor-avatar" alt="Dr. Nguyen Van A" />
              <h5 class="fw-bold mb-1">ThS. BS. Nguyễn Văn An</h5>
              <p class="text-warning-highlight small fw-bold mb-2">Giám Đốc Y Khoa</p>
              <p class="text-muted small mb-0">
                Hơn 15 năm kinh nghiệm về phẫu thuật ngoại khoa và quản lý y tế thú y. Tốt nghiệp Đại học Thú y Queensland, Úc.
              </p>
            </div>
          </div>

          <!-- Doctor 2 -->
          <div class="doctor-card-wrapper text-center">
            <div class="card border-0 glass-card h-100 p-4 doctor-card">
              <img src="../assets/bsi2.png" class="doctor-avatar" alt="BS. Lê Thị Bình" />
              <h5 class="fw-bold mb-1">BS. Lê Thị Bình</h5>
              <p class="text-warning-highlight small fw-bold mb-2">Trưởng Khoa Nội Trực</p>
              <p class="text-muted small mb-0">
                Chuyên gia chẩn đoán các bệnh nội khoa truyền nhiễm và dinh dưỡng thú cưng. 10 năm kinh nghiệm lâm sàng thực tế.
              </p>
            </div>
          </div>

          <!-- Doctor 3 -->
          <div class="doctor-card-wrapper text-center">
            <div class="card border-0 glass-card h-100 p-4 doctor-card">
              <img src="../assets/bsi3.png" class="doctor-avatar" alt="Dr. Tran Hoang C" />
              <h5 class="fw-bold mb-1">BS. Trần Hoàng Cường</h5>
              <p class="text-warning-highlight small fw-bold mb-2">Chuyên Gia Chẩn Đoán Hình Ảnh</p>
              <p class="text-muted small mb-0">
                Chuyên sâu về siêu âm tim, Doppler mạch máu và đọc phim X-Quang, cắt lớp vi tính. Được đào tạo tại Nhật Bản.
              </p>
            </div>
          </div>

          <!-- Doctor 4 -->
          <div class="doctor-card-wrapper text-center">
            <div class="card border-0 glass-card h-100 p-4 doctor-card">
              <img src="../assets/bsi4.png" class="doctor-avatar" alt="Dr. Pham Minh D" />
              <h5 class="fw-bold mb-1">BS. Phạm Minh Duy</h5>
              <p class="text-warning-highlight small fw-bold mb-2">Chuyên Khoa Da Liễu</p>
              <p class="text-muted small mb-0">
                Chuyên trị các bệnh nấm da, ghẻ lở, viêm da dị ứng mãn tính ở chó mèo. Tư vấn chế độ chăm sóc da và lông chuẩn khoa học.
              </p>
            </div>
          </div>
        </div>

        <!-- Nursing & Care Team -->
        <h4 class="fw-bold mb-4 text-warning border-bottom pb-2 title-category-team">
          <HeartPulse class="icon-category-team" /> Đội Ngũ Điều Dưỡng & Chăm Sóc
        </h4>
        <p class="text-muted mb-4 section-desc-team">
          Đóng vai trò quan trọng trong việc hỗ trợ các bác sĩ và chăm sóc trực tiếp cho thú cưng khi điều trị nội trú hay phục hồi chức năng. Họ là những người túc trực ngày đêm để đảm bảo thú cưng của bạn cảm thấy ấm áp và an toàn nhất.
        </p>
        
        <div class="nurses-grid">
          <!-- Nurse 1 -->
          <div class="nurse-card-wrapper text-center">
            <div class="card border-0 glass-card p-4 nurse-card">
              <h6 class="fw-bold mb-1 text-dark">CN. Nguyễn Thảo Vy</h6>
              <p class="text-muted small mb-2">Trưởng Bộ Phận Điều Dưỡng</p>
              <span class="badge-exp">8 năm kinh nghiệm</span>
            </div>
          </div>
          <!-- Nurse 2 -->
          <div class="nurse-card-wrapper text-center">
            <div class="card border-0 glass-card p-4 nurse-card">
              <h6 class="fw-bold mb-1 text-dark">CN. Trần Quốc Bảo</h6>
              <p class="text-muted small mb-2">Điều Dưỡng Viên Nội Trú</p>
              <span class="badge-exp">5 năm kinh nghiệm</span>
            </div>
          </div>
          <!-- Nurse 3 -->
          <div class="nurse-card-wrapper text-center">
            <div class="card border-0 glass-card p-4 nurse-card">
              <h6 class="fw-bold mb-1 text-dark">CN. Lê Hoài Nam</h6>
              <p class="text-muted small mb-2">Kỹ Thuật Viên Phẫu Thuật</p>
              <span class="badge-exp">6 năm kinh nghiệm</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <Footer />

    <BookingModal 
      :show="showBookingModal" 
      @close="showBookingModal = false" 
      @success="handleBookingSuccess" 
      @error="handleBookingError" 
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import Header from '../components/layout/Header.vue';
import Footer from '../components/layout/Footer.vue';
import BookingModal from '../components/shared/BookingModal.vue';
import { Users, Award, HeartPulse } from '@lucide/vue';

const showBookingModal = ref(false);

const handleBookingSuccess = (msg: string) => {
  alert(msg);
};

const handleBookingError = (msg: string) => {
  alert(msg);
};
</script>

<style scoped>
.page-wrapper {
  background-color: var(--bg-light);
  color: var(--text-dark);
}

.hero-section {
  padding: 5rem 0;
  overflow: hidden;
}

.icon-users {
  width: 16px;
  height: 16px;
  display: inline-block;
  vertical-align: middle;
}

.hero-shape-1 {
  position: absolute;
  top: -20%;
  right: -10%;
  width: 600px;
  height: 600px;
  background: radial-gradient(circle, rgba(254, 243, 199, 0.7) 0%, rgba(254, 243, 199, 0) 70%);
  z-index: 1;
  pointer-events: none;
}

.section-commitment {
  margin-top: 1rem;
}

.title-category-team {
  font-size: 1.3rem;
  font-weight: 800;
  color: var(--primary-gold) !important;
  display: flex;
  align-items: center;
  gap: 8px;
}

.icon-category-team {
  width: 22px;
  height: 22px;
}

/* Doctors list grid */
.doctors-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 2rem;
  margin-top: 1.5rem;
}

.doctor-card {
  background-color: white !important;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.doctor-avatar {
  width: 130px;
  height: 130px;
  border-radius: 50%;
  border: 4px solid var(--primary-gold);
  object-fit: cover;
  box-shadow: var(--shadow-sm);
  margin-bottom: 1.5rem;
  transition: transform var(--transition-speed);
}

.doctor-card:hover .doctor-avatar {
  transform: scale(1.05);
}

.text-warning-highlight {
  color: var(--primary-dark) !important;
  font-weight: 700;
}

/* Nurses grid */
.nurses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 1.5rem;
  margin-top: 1.5rem;
}

.nurse-card {
  background-color: white !important;
  padding: 1.5rem;
}

.badge-exp {
  font-size: 0.75rem;
  color: var(--text-dark);
  border: 1px solid var(--border-color);
  padding: 0.3rem 1rem;
  border-radius: 20px;
  display: inline-block;
  margin-top: 0.5rem;
  background-color: var(--bg-light);
  font-weight: 600;
}

.section-desc-team {
  font-size: 0.95rem;
  line-height: 1.6;
}
</style>
