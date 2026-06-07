<template>
  <div class="service-detail-wrapper">
    <!-- Background elements -->
    <div class="bg-glow bg-glow-1"></div>
    <div class="bg-glow bg-glow-2"></div>

    <div class="service-detail-container">
      <!-- Back button -->
      <div class="action-bar">
        <router-link to="/" class="btn-back">
          <ArrowLeft class="icon-btn" />
          <span>Quay lại Trang chủ</span>
        </router-link>
      </div>

      <!-- Header Section -->
      <div class="service-header">
        <div class="icon-wrapper">
          <Activity class="service-icon" />
        </div>
        <span class="service-tag">Dịch vụ y khoa</span>
        <h1>Khám & Điều Trị Chuyên Sâu</h1>
        <p class="service-subtitle">
          Chẩn đoán chính xác bằng hình ảnh công nghệ cao và hệ thống xét nghiệm máu tự động hiện đại giúp đưa ra phác đồ điều trị tối ưu nhất cho thú cưng.
        </p>
      </div>

      <!-- Feature Grid -->
      <div class="feature-grid">
        <div class="feature-card">
          <Stethoscope class="feat-icon" />
          <h3>Khám Lâm Sàng Toàn Diện</h3>
          <p>Kiểm tra nhiệt độ, tai, mắt, răng miệng, hạch bạch huyết và nghe tim phổi để phát hiện các triệu chứng bệnh lý bất thường.</p>
        </div>

        <div class="feature-card">
          <Activity class="feat-icon" />
          <h3>Chẩn Đoán Hình Ảnh</h3>
          <p>Sử dụng máy siêu âm 4D thế hệ mới và máy chụp X-quang kỹ thuật số giúp chẩn đoán nhanh các bệnh lý nội tạng và chấn thương xương khớp.</p>
        </div>

        <div class="feature-card">
          <FlaskConical class="feat-icon" />
          <h3>Xét Nghiệm Sinh Hóa</h3>
          <p>Hệ thống phòng xét nghiệm máu tự động cho kết quả các chỉ số gan, thận, hồng cầu, bạch cầu chỉ sau 15 phút, giúp bác sĩ lên phác đồ kịp thời.</p>
        </div>
      </div>

      <!-- Price Table Section -->
      <div class="price-section">
        <h2>Bảng Giá Dịch Vụ Khám & Điều Trị</h2>
        <p class="price-desc">Chi phí có thể thay đổi tùy thuộc vào thể trạng thực tế và kích thước của thú cưng.</p>
        
        <div class="table-container">
          <table class="price-table">
            <thead>
              <tr>
                <th>Hạng Mục Khám & Chẩn Đoán</th>
                <th>Thú cưng &lt; 5kg</th>
                <th>Thú cưng từ 5 - 15kg</th>
                <th>Thú cưng &gt; 15kg</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Khám lâm sàng ban đầu</td>
                <td>100.000đ</td>
                <td>100.000đ</td>
                <td>100.000đ</td>
              </tr>
              <tr>
                <td>Siêu âm chẩn đoán ổ bụng</td>
                <td>150.000đ</td>
                <td>180.000đ</td>
                <td>220.000đ</td>
              </tr>
              <tr>
                <td>Chụp X-quang kỹ thuật số (1 phim)</td>
                <td>150.000đ</td>
                <td>150.000đ</td>
                <td>180.000đ</td>
              </tr>
              <tr>
                <td>Xét nghiệm tổng phân tích máu (CBC)</td>
                <td>200.000đ</td>
                <td>200.000đ</td>
                <td>200.000đ</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Call To Action -->
      <div class="cta-card">
        <h3>Thú cưng của bạn đang có biểu hiện mệt mỏi hoặc bỏ ăn?</h3>
        <p>Đăng ký lịch khám ngay hôm nay để bác sĩ trực ca kiểm tra và hỗ trợ kịp thời.</p>
        <router-link to="/contact" class="btn-cta">Đặt lịch khám ngay</router-link>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ArrowLeft, Activity, Stethoscope, FlaskConical } from '@lucide/vue';
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.service-detail-wrapper {
  position: relative;
  min-height: 100vh;
  background: radial-gradient(circle at top right, #1e293b, #0f172a, #0b0f19);
  font-family: 'Outfit', sans-serif;
  color: #f8fafc;
  overflow: hidden;
  padding: 2.5rem 1.5rem;
}

.bg-glow {
  position: absolute;
  border-radius: 50%;
  filter: blur(100px);
  opacity: 0.12;
  z-index: 0;
  pointer-events: none;
}

.bg-glow-1 {
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, #0d9488, transparent);
  top: -100px;
  right: -100px;
}

.bg-glow-2 {
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, #6366f1, transparent);
  bottom: -50px;
  left: -50px;
}

.service-detail-container {
  position: relative;
  z-index: 1;
  max-width: 950px;
  margin: 0 auto;
}

.action-bar {
  margin-bottom: 2.5rem;
}

.btn-back {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  padding: 0.6rem 1.2rem;
  border-radius: 10px;
  color: #cbd5e1;
  font-weight: 500;
  cursor: pointer;
  text-decoration: none;
  transition: all 0.3s;
}

.btn-back:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
  transform: translateX(-4px);
}

.icon-btn {
  width: 18px;
  height: 18px;
}

.service-header {
  text-align: center;
  margin-bottom: 4rem;
}

.icon-wrapper {
  width: 70px;
  height: 70px;
  border-radius: 20px;
  background: rgba(13, 148, 136, 0.15);
  color: #14b8a6;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0 auto 1.5rem auto;
  box-shadow: 0 8px 20px rgba(13, 148, 136, 0.2);
}

.service-icon {
  width: 32px;
  height: 32px;
}

.service-tag {
  color: #14b8a6;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 2px;
  background: rgba(20, 184, 166, 0.1);
  padding: 0.4rem 1rem;
  border-radius: 20px;
  margin-bottom: 1rem;
  display: inline-block;
}

h1 {
  font-size: 2.8rem;
  font-weight: 800;
  letter-spacing: -1px;
  margin-bottom: 1.2rem;
  background: linear-gradient(135deg, #14b8a6, #6366f1);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.service-subtitle {
  color: #94a3b8;
  font-size: 1.05rem;
  line-height: 1.7;
  max-width: 750px;
  margin: 0 auto;
}

/* Feature grid layout */
.feature-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.8rem;
  margin-bottom: 4.5rem;
}

@media (max-width: 768px) {
  .feature-grid {
    grid-template-columns: 1fr;
  }
}

.feature-card {
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 20px;
  padding: 2.2rem 1.8rem;
  text-align: center;
  transition: all 0.3s;
}

.feature-card:hover {
  transform: translateY(-5px);
  border-color: rgba(20, 184, 166, 0.25);
}

.feat-icon {
  width: 32px;
  height: 32px;
  color: #14b8a6;
  margin-bottom: 1.2rem;
}

.feature-card h3 {
  font-size: 1.2rem;
  font-weight: 600;
  color: white;
  margin-bottom: 0.8rem;
}

.feature-card p {
  font-size: 0.9rem;
  color: #94a3b8;
  line-height: 1.6;
}

/* Price Section styling */
.price-section {
  margin-bottom: 4.5rem;
}

.price-section h2 {
  font-size: 1.6rem;
  font-weight: 700;
  color: white;
  margin-bottom: 0.4rem;
}

.price-desc {
  font-size: 0.9rem;
  color: #64748b;
  margin-bottom: 2rem;
}

.table-container {
  overflow-x: auto;
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.price-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  background: rgba(30, 41, 59, 0.25);
  font-size: 0.95rem;
}

.price-table th, .price-table td {
  padding: 1.2rem 1.5rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.price-table th {
  background: rgba(15, 23, 42, 0.6);
  color: #cbd5e1;
  font-weight: 600;
}

.price-table td {
  color: #94a3b8;
}

.price-table tbody tr:hover {
  background: rgba(255, 255, 255, 0.02);
}

.price-table tbody tr:last-child td {
  border-bottom: none;
}

/* CTA Card */
.cta-card {
  background: linear-gradient(135deg, rgba(20, 184, 166, 0.15) 0%, rgba(99, 102, 241, 0.15) 100%);
  border: 1px solid rgba(20, 184, 166, 0.2);
  border-radius: 24px;
  padding: 3rem 2rem;
  text-align: center;
  margin-bottom: 4rem;
  backdrop-filter: blur(20px);
}

.cta-card h3 {
  font-size: 1.5rem;
  color: white;
  margin-bottom: 0.8rem;
  font-weight: 700;
}

.cta-card p {
  color: #cbd5e1;
  font-size: 0.95rem;
  margin-bottom: 1.8rem;
}

.btn-cta {
  display: inline-block;
  background: linear-gradient(135deg, #14b8a6, #0d9488);
  color: white;
  border: none;
  padding: 0.85rem 2.2rem;
  border-radius: 12px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.3s;
  box-shadow: 0 4px 15px rgba(20, 184, 166, 0.3);
}

.btn-cta:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(20, 184, 166, 0.4);
}
</style>
