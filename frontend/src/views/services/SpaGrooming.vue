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
          <Sparkles class="service-icon" />
        </div>
        <span class="service-tag">Dịch vụ làm đẹp</span>
        <h1>Spa & Grooming Làm Đẹp</h1>
        <p class="service-subtitle">
          Mang lại diện mạo xinh xắn, thơm tho và sạch sẽ nhất cho pet cưng bằng quy trình spa chuẩn 5 bước khép kín cùng đội ngũ kỹ thuật viên khéo tay, nhiều kinh nghiệm.
        </p>
      </div>

      <!-- Feature Grid -->
      <div class="feature-grid">
        <div class="feature-card">
          <Scissors class="feat-icon" />
          <h3>Cắt Tỉa & Tạo Kiểu Nghệ Thuật</h3>
          <p>Tạo kiểu lông chuyên nghiệp theo yêu cầu như kiểu Teddy, kiểu Hàn Quốc giúp pet cưng nổi bật, đáng yêu.</p>
        </div>

        <div class="feature-card">
          <Sparkles class="feat-icon" />
          <h3>Tắm Spa 5 Bước</h3>
          <p>Sử dụng sữa tắm cao cấp dưỡng lông, vắt tuyến hôi triệt để khử mùi, sấy đánh bông lông và xịt nước hoa dưỡng ẩm thơm lâu.</p>
        </div>

        <div class="feature-card">
          <Heart class="feat-icon" />
          <h3>Vệ Sinh Y Tế Cơ Bản</h3>
          <p>Cắt dũa móng an toàn tránh trầy xước, cạo lông bàn chân giữ vệ sinh, nhổ lông tai và vệ sinh tai sạch sẽ phòng ngừa viêm nhiễm.</p>
        </div>
      </div>

      <!-- Price Table Section -->
      <div class="price-section">
        <h2>Bảng Giá Dịch Vụ Spa & Grooming</h2>
        <p class="price-desc">Chi phí có thể thay đổi tùy thuộc vào độ dài lông, độ rối lông và tính khí thực tế của thú cưng.</p>
        
        <div class="table-container">
          <table class="price-table">
            <thead>
              <tr>
                <th>Gói Dịch Vụ Spa</th>
                <th>Thú cưng &lt; 5kg</th>
                <th>Thú cưng từ 5 - 15kg</th>
                <th>Thú cưng &gt; 15kg</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Combo Vệ sinh cơ bản (Cắt móng, tai, vắt tuyến hôi)</td>
                <td>150.000đ</td>
                <td>180.000đ</td>
                <td>220.000đ</td>
              </tr>
              <tr>
                <td>Gói Tắm Spa Dưỡng Lông (Khử mùi, sấy khô)</td>
                <td>200.000đ</td>
                <td>250.000đ</td>
                <td>300.000đ</td>
              </tr>
              <tr>
                <td>Gói Cắt Tỉa Tạo Kiểu (Bao gồm tắm spa trọn gói)</td>
                <td>350.000đ</td>
                <td>450.000đ</td>
                <td>550.000đ</td>
              </tr>
              <tr>
                <td>Gỡ rối lông (tính thêm theo giờ)</td>
                <td>80.000đ / giờ</td>
                <td>100.000đ / giờ</td>
                <td>120.000đ / giờ</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Call To Action -->
      <div class="cta-card">
        <h3>Bạn muốn pet cưng sở hữu diện mạo mới thật đáng yêu?</h3>
        <p>Đặt lịch hẹn spa ngay hôm nay để nhận ưu đãi giảm 10% cho lần trải nghiệm đầu tiên.</p>
        <router-link to="/contact" class="btn-cta">Đặt lịch hẹn làm đẹp</router-link>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ArrowLeft, Sparkles, Scissors, Heart } from '@lucide/vue';
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
  background: radial-gradient(circle, #6366f1, transparent);
  top: -100px;
  right: -100px;
}

.bg-glow-2 {
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, #0d9488, transparent);
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
  background: rgba(99, 102, 241, 0.15);
  color: #6366f1;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0 auto 1.5rem auto;
  box-shadow: 0 8px 20px rgba(99, 102, 241, 0.2);
}

.service-icon {
  width: 32px;
  height: 32px;
}

.service-tag {
  color: #6366f1;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 2px;
  background: rgba(99, 102, 241, 0.1);
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
  background: linear-gradient(135deg, #6366f1, #0d9488);
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
  border-color: rgba(99, 102, 241, 0.25);
}

.feat-icon {
  width: 32px;
  height: 32px;
  color: #6366f1;
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
  background: linear-gradient(135deg, rgba(99, 102, 241, 0.15) 0%, rgba(20, 184, 166, 0.15) 100%);
  border: 1px solid rgba(99, 102, 241, 0.2);
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
  background: linear-gradient(135deg, #6366f1, #4f46e5);
  color: white;
  border: none;
  padding: 0.85rem 2.2rem;
  border-radius: 12px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.3s;
  box-shadow: 0 4px 15px rgba(99, 102, 241, 0.3);
}

.btn-cta:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(99, 102, 241, 0.4);
}
</style>
