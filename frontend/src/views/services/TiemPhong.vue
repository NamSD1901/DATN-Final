<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <ShieldCheck class="icon-shield" /> Y Học Dự Phòng
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">TIÊM PHÒNG BỆNH ĐỊNH KỲ</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Cách đơn giản, hiệu quả và kinh tế nhất để bảo vệ thú cưng khỏi các căn bệnh truyền nhiễm nguy hiểm gây tử vong cao.
        </p>
      </div>
    </section>

    <!-- Services Body -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <!-- Importance Section -->
        <div class="importance-row mb-5">
          <div class="importance-content">
            <h3 class="fw-bold mb-3">Tại sao cần tiêm vaccine đúng lịch?</h3>
            <p class="text-muted mb-4">
              Vaccine huấn luyện hệ miễn dịch của thú cưng nhận biết và tiêu diệt nhanh chóng virus, vi khuẩn gây hại khi chúng xâm nhập. Bỏ lỡ lịch tiêm phòng hoặc tiêm không đủ liều có thể khiến bé cưng mất đi lớp bảo vệ trước các dịch bệnh chết người như Parvo, Care ở chó hay giảm bạch cầu ở mèo.
            </p>
            <div class="benefit-check-item">
              <div class="benefit-check-icon">
                <Check class="check-icon" />
              </div>
              <div>
                <h6 class="fw-bold mb-1">Bảo vệ cá thể thú cưng</h6>
                <p class="small text-muted mb-0">Ngăn ngừa các bệnh lý nguy hại đến tính mạng.</p>
              </div>
            </div>
            <div class="benefit-check-item">
              <div class="benefit-check-icon">
                <Check class="check-icon" />
              </div>
              <div>
                <h6 class="fw-bold mb-1">Tránh lây sang người</h6>
                <p class="small text-muted mb-0">Các bệnh nguy hiểm như dại hay xoắn khuẩn Leptospira có thể lây sang người nuôi.</p>
              </div>
            </div>
          </div>
          <div class="importance-visual">
            <img src="https://images.unsplash.com/photo-1576201836106-db1758fd1c97?q=80&w=600&auto=format&fit=crop" class="img-fluid rounded-4 shadow-lg" alt="Thú y tiêm phòng cho thú cưng" />
          </div>
        </div>

        <!-- Vaccination Schedule Tables -->
        <h3 class="fw-bold text-center mb-5 section-title-center">Lịch Tiêm Vaccine Tham Khảo</h3>
        <div class="schedules-grid mb-5">
          <!-- Dog Schedule -->
          <div class="schedule-card-wrapper">
            <div class="card border-0 glass-card p-4 h-100">
              <h4 class="fw-bold text-warning mb-3 text-center d-flex align-items-center justify-content-center gap-2">
                <PawPrint class="schedule-title-icon" /> Lịch Tiêm Cho Chó
              </h4>
              <div class="table-container">
                <table class="table-premium">
                  <thead>
                    <tr>
                      <th>Độ tuổi</th>
                      <th>Mũi tiêm</th>
                      <th>Phòng bệnh</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td class="fw-semibold text-dark">6 - 8 tuần tuổi</td>
                      <td>Mũi 1 (5-in-1 hoặc 7-in-1)</td>
                      <td>Care, Parvo, Viêm gan, Ho cũi chó...</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">10 - 12 tuần tuổi</td>
                      <td>Mũi 2 (7-in-1)</td>
                      <td>Nhắc lại và bổ sung xoắn khuẩn Leptospira.</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">14 - 16 tuần tuổi</td>
                      <td>Mũi 3 (7-in-1) + Tiêm Dại</td>
                      <td>Mũi nhắc lại cuối cùng của năm đầu + phòng bệnh Dại.</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">Hàng năm</td>
                      <td>Nhắc lại 1 mũi 7-in-1 + Dại</td>
                      <td>Duy trì kháng thể bảo vệ suốt đời.</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          <!-- Cat Schedule -->
          <div class="schedule-card-wrapper">
            <div class="card border-0 glass-card p-4 h-100">
              <h4 class="fw-bold text-warning mb-3 text-center d-flex align-items-center justify-content-center gap-2">
                <Cat class="schedule-title-icon" /> Lịch Tiêm Cho Mèo
              </h4>
              <div class="table-container">
                <table class="table-premium">
                  <thead>
                    <tr>
                      <th>Độ tuổi</th>
                      <th>Mũi tiêm</th>
                      <th>Phòng bệnh</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td class="fw-semibold text-dark">8 - 10 tuần tuổi</td>
                      <td>Mũi 1 (4-in-1)</td>
                      <td>Giảm bạch cầu, Viêm mũi - khí quản, Calicivirus...</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">12 - 14 tuần tuổi</td>
                      <td>Mũi 2 (4-in-1)</td>
                      <td>Nhắc lại tạo kháng thể chủ động.</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">16 - 18 tuần tuổi</td>
                      <td>Mũi 3 (4-in-1) + Tiêm Dại</td>
                      <td>Nhắc lại lần 3 bảo đảm an toàn + phòng bệnh Dại.</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">Hàng năm</td>
                      <td>Nhắc lại 1 mũi 4-in-1 + Dại</td>
                      <td>Duy trì miễn dịch chống lại virus giảm bạch cầu nguy hiểm.</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>

        <!-- Warning & Care Alert Box -->
        <div class="alert-box-warning p-4">
          <h5 class="fw-bold text-warning mb-3 d-flex align-items-center gap-2">
            <AlertTriangle class="alert-icon-title" /> Lưu Ý Quan Trọng Cho Chủ Nuôi
          </h5>
          <div class="alert-grid small text-muted">
            <div class="alert-col">
              <h6 class="fw-bold text-dark mb-1">Trước khi tiêm:</h6>
              <ul class="ps-3 mb-0">
                <li>Thú cưng phải hoàn toàn khỏe mạnh, không bị sốt, bỏ ăn hay tiêu chảy.</li>
                <li>Nên tẩy giun trước khi tiêm vaccine khoảng 1 tuần để vaccine đạt hiệu quả cao nhất.</li>
              </ul>
            </div>
            <div class="alert-col">
              <h6 class="fw-bold text-dark mb-1">Sau khi tiêm:</h6>
              <ul class="ps-3 mb-0">
                <li>Kiêng tắm cho thú cưng ít nhất 7 ngày sau tiêm để tránh sốc nhiệt hoặc nhiễm trùng.</li>
                <li>Theo dõi các biểu hiện sốc phản vệ (sưng mặt, nôn mửa liên tục) để đưa ngay đến bệnh viện xử lý kịp thời.</li>
              </ul>
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
import Header from '../../components/layout/Header.vue';
import Footer from '../../components/layout/Footer.vue';
import BookingModal from '../../components/shared/BookingModal.vue';
import { ShieldCheck, Check, PawPrint, Cat, AlertTriangle } from '@lucide/vue';

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

.icon-shield {
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

.section-title-center {
  font-size: 2rem;
  font-weight: 800;
  margin-bottom: 2rem;
  text-align: center;
}

/* Importance section row */
.importance-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 3rem;
  align-items: center;
}

@media (max-width: 991px) {
  .importance-row {
    grid-template-columns: 1fr;
  }
}

.benefit-check-item {
  display: flex;
  align-items: start;
  gap: 12px;
  margin-bottom: 1rem;
}

.benefit-check-icon {
  background: var(--primary-cream);
  color: var(--primary-dark);
  padding: 0.4rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.check-icon {
  width: 16px;
  height: 16px;
}

.importance-visual img {
  width: 100%;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
}

/* Schedules layouts */
.schedules-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

@media (max-width: 991px) {
  .schedules-grid {
    grid-template-columns: 1fr;
  }
}

.schedule-title-icon {
  width: 20px;
  height: 20px;
  color: var(--primary-gold);
}

.table-container {
  overflow-x: auto;
  border: 1px solid var(--border-color);
  border-radius: 12px;
  margin-top: 1rem;
}

.table-premium {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

.table-premium th, .table-premium td {
  padding: 0.8rem 1rem;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.table-premium th {
  background-color: var(--primary-cream);
  color: var(--primary-dark);
  font-weight: 700;
}

.table-premium td {
  color: var(--text-muted);
}

.table-premium tbody tr:hover {
  background-color: rgba(254, 243, 199, 0.2);
}

/* Warning alert box */
.alert-box-warning {
  border-radius: var(--radius-lg);
  background-color: rgba(245, 158, 11, 0.08);
  border: 1px solid var(--primary-gold);
}

.alert-icon-title {
  width: 20px;
  height: 20px;
}

.alert-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

@media (max-width: 768px) {
  .alert-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
}

.alert-col ul {
  line-height: 1.6;
}
</style>
