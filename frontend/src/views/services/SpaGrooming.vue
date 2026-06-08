<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <Scissors class="icon-scissors" /> Chăm Sóc Sắc Đẹp
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">SPA & THẨM MỸ CẮT TỈA</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Hô biến diện mạo lộng lẫy cho bé cưng với bàn tay khéo léo của các chuyên gia Grooming hàng đầu cùng thiết bị tắm sấy cao cấp.
        </p>
      </div>
    </section>

    <!-- Services Body -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <!-- 7-Step Grooming Process -->
        <h3 class="fw-bold text-center mb-5 section-title-center">Quy Trình Spa 7 Bước Chuyên Nghiệp</h3>
        <div class="steps-grid mb-5">
          <!-- Step 1 -->
          <div class="step-card glass-card">
            <span class="step-number">01</span>
            <h6 class="step-title">Kiểm tra da & lông</h6>
            <p class="step-desc">Đánh giá tình trạng gàu, nấm hoặc ký sinh trùng để chọn loại dầu tắm phù hợp.</p>
          </div>
          <!-- Step 2 -->
          <div class="step-card glass-card">
            <span class="step-number">02</span>
            <h6 class="step-title">Vệ sinh tai & Mài móng</h6>
            <p class="step-desc">Cắt tỉa móng chân gọn gàng, mài mịn móng tránh cào xước. Vệ sinh sạch tai bằng dung dịch chuyên dụng.</p>
          </div>
          <!-- Step 3 -->
          <div class="step-card glass-card">
            <span class="step-number">03</span>
            <h6 class="step-title">Vắt tuyến hôi</h6>
            <p class="step-desc">Giúp giải phóng dịch ứ đọng ở hậu môn, loại bỏ mùi hôi đặc trưng và tránh viêm nhiễm tuyến hậu môn.</p>
          </div>
          <!-- Step 4 -->
          <div class="step-card glass-card">
            <span class="step-number">04</span>
            <h6 class="step-title">Tắm massage 2 lần</h6>
            <p class="step-desc">Lần 1 làm sạch bụi bẩn, lần 2 dùng dầu dưỡng mềm mượt và lưu hương dài lâu.</p>
          </div>
          <!-- Step 5 -->
          <div class="step-card glass-card">
            <span class="step-number">05</span>
            <h6 class="step-title">Sấy & Đánh tơi lông</h6>
            <p class="step-desc">Sấy khô kết hợp chải tơi lông bằng máy sấy chuyên dụng không ồn để thú cưng không hoảng sợ.</p>
          </div>
          <!-- Step 6 -->
          <div class="step-card glass-card">
            <span class="step-number">06</span>
            <h6 class="step-title">Cắt tỉa tạo kiểu</h6>
            <p class="step-desc">Groomer tay nghề cao thiết kế kiểu lông (Teddy bear, tròn, phẳng...) theo yêu cầu của chủ nuôi.</p>
          </div>
          <!-- Step 7 -->
          <div class="step-card glass-card">
            <span class="step-number">07</span>
            <h6 class="step-title">Xịt dưỡng hoa hồng</h6>
            <p class="step-desc">Thoa dầu dưỡng bảo vệ lông chống xơ rối và tạo mùi hương thơm dịu nhẹ quý phái.</p>
          </div>
        </div>

        <!-- Pricing Grid -->
        <h3 class="fw-bold text-center mb-3 section-title-center">Bảng Giá Dịch Vụ Spa Trọn Gói</h3>
        <p class="text-muted text-center max-w-2xl mx-auto mb-5">
          Bảng giá được tính toán dựa trên khối lượng cân nặng thực tế và độ phức tạp của bộ lông thú cưng.
        </p>
        <div class="table-wrapper-center mb-5">
          <div class="card border-0 glass-card table-card">
            <div class="table-container">
              <table class="table-premium">
                <thead>
                  <tr>
                    <th scope="col" class="ps-4">Cân nặng Thú cưng</th>
                    <th scope="col">Gói Spa Tắm Dưỡng</th>
                    <th scope="col" class="pe-4">Gói Spa Cắt Tỉa Tạo Kiểu</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td class="ps-4"><strong>Dưới 5kg</strong></td>
                    <td>150.000 VNĐ</td>
                    <td class="pe-4">300.000 VNĐ</td>
                  </tr>
                  <tr>
                    <td class="ps-4"><strong>Từ 5kg - dưới 10kg</strong></td>
                    <td>200.000 VNĐ</td>
                    <td class="pe-4">400.000 VNĐ</td>
                  </tr>
                  <tr>
                    <td class="ps-4"><strong>Từ 10kg - dưới 20kg</strong></td>
                    <td>300.000 VNĐ</td>
                    <td class="pe-4">550.000 VNĐ</td>
                  </tr>
                  <tr>
                    <td class="ps-4"><strong>Trên 20kg</strong></td>
                    <td>Liên hệ trực tiếp</td>
                    <td class="pe-4">Liên hệ trực tiếp</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- Beautiful Visual Display -->
        <div class="images-grid mt-5">
          <img src="https://images.unsplash.com/photo-1516734212186-a967f81ad0d7?q=80&w=600&auto=format&fit=crop" class="img-fluid rounded-4 shadow" alt="Dog getting bathed" />
          <img src="https://images.unsplash.com/photo-1527526029430-319f10814151?q=80&w=600&auto=format&fit=crop" class="img-fluid rounded-4 shadow" alt="Dog getting haircut" />
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
import { Scissors } from '@lucide/vue';

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

.icon-scissors {
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

/* Steps grid layout */
.steps-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.5rem;
}

.step-card {
  padding: 1.5rem;
  text-align: center;
  background-color: white !important;
}

.step-number {
  font-size: 2rem;
  font-weight: 800;
  color: var(--primary-gold);
  display: block;
  margin-bottom: 8px;
}

.step-title {
  font-size: 1rem;
  font-weight: 700;
  margin-bottom: 8px;
  color: var(--text-dark);
}

.step-desc {
  font-size: 0.8rem;
  color: var(--text-muted);
  line-height: 1.5;
}

/* Table layout */
.table-wrapper-center {
  display: flex;
  justify-content: center;
}

.table-card {
  width: 100%;
  max-width: 800px;
}

.table-container {
  overflow-x: auto;
}

.table-premium {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.table-premium th, .table-premium td {
  padding: 1rem 1.25rem;
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

/* Images Grid */
.images-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 768px) {
  .images-grid {
    grid-template-columns: 1fr;
  }
}

.images-grid img {
  width: 100%;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
}
</style>
