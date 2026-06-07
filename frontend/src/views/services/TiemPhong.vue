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
          <ShieldCheck class="service-icon" />
        </div>
        <span class="service-tag">Dịch vụ phòng ngừa</span>
        <h1>Tiêm Phòng & Vaccine</h1>
        <p class="service-subtitle">
          Chủ động phòng ngừa các dịch bệnh nguy hiểm có tỷ lệ tử vong cao ở chó mèo bằng các loại vaccine nhập khẩu chính hãng, có tem nhãn kiểm định rõ ràng.
        </p>
      </div>

      <!-- Feature Grid -->
      <div class="feature-grid">
        <div class="feature-card">
          <ShieldAlert class="feat-icon" />
          <h3>Vaccine Nhập Khẩu Chính Hãng</h3>
          <p>Sử dụng các loại vaccine chất lượng cao từ các tập đoàn dược thú y hàng đầu thế giới (Zoetis - Mỹ, Boehringer Ingelheim - Đức, Virbac - Pháp).</p>
        </div>

        <div class="feature-card">
          <Stethoscope class="feat-icon" />
          <h3>Khám Sức Khỏe Trước Tiêm</h3>
          <p>100% thú cưng được kiểm tra nhiệt độ, nghe tim phổi và đánh giá thể trạng hoàn toàn miễn phí trước khi tiêm để đảm bảo an toàn tuyệt đối.</p>
        </div>

        <div class="feature-card">
          <Calendar class="feat-icon" />
          <h3>Nhắc Lịch Tự Động</h3>
          <p>Hệ thống tự động lưu trữ thông tin tiêm phòng và gửi tin nhắn SMS / Zalo nhắc lịch tiêm nhắc lại cho bạn khi đến hạn.</p>
        </div>
      </div>

      <!-- Price Table Section -->
      <div class="price-section">
        <h2>Bảng Giá Các Loại Vaccine Phổ Biến</h2>
        <p class="price-desc">Giá đã bao gồm chi phí khám sức khỏe ban đầu và sổ theo dõi tiêm phòng định kỳ.</p>
        
        <div class="table-container">
          <table class="price-table">
            <thead>
              <tr>
                <th>Loại Vaccine</th>
                <th>Đối Tượng</th>
                <th>Phòng Bệnh</th>
                <th>Đơn Giá</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Vaccine phòng dại (Rabisin)</td>
                <td>Chó & Mèo từ 3 tháng</td>
                <td>Bệnh dại nguy hiểm lây sang người</td>
                <td>120.000đ</td>
              </tr>
              <tr>
                <td>Vaccine 4 bệnh cho mèo (Purevax)</td>
                <td>Mèo từ 8 tuần tuổi</td>
                <td>Giảm bạch cầu, Viêm mũi, Calicivirus, Chlamydia</td>
                <td>350.000đ</td>
              </tr>
              <tr>
                <td>Vaccine 5 bệnh cho chó (Vanguard)</td>
                <td>Chó từ 6 tuần tuổi</td>
                <td>Care, Parvo, Viêm gan, Ho cũi chó, Cúm</td>
                <td>200.000đ</td>
              </tr>
              <tr>
                <td>Vaccine 7 bệnh cho chó (Vanguard Plus)</td>
                <td>Chó từ 6 tuần tuổi</td>
                <td>5 bệnh trên + 2 chủng Leptospira (bệnh xoắn khuẩn)</td>
                <td>250.000đ</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Call To Action -->
      <div class="cta-card">
        <h3>Đã đến lịch tiêm phòng định kỳ của pet cưng của bạn?</h3>
        <p>Đăng ký tiêm phòng ngay hôm nay để bảo vệ thú cưng khỏi các ổ dịch nguy hiểm.</p>
        <router-link to="/contact" class="btn-cta">Đặt lịch tiêm phòng</router-link>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ArrowLeft, ShieldCheck, ShieldAlert, Stethoscope, Calendar } from '@lucide/vue';
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
  background: radial-gradient(circle, #f59e0b, transparent);
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
  background: rgba(245, 158, 11, 0.15);
  color: #f59e0b;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0 auto 1.5rem auto;
  box-shadow: 0 8px 20px rgba(245, 158, 11, 0.2);
}

.service-icon {
  width: 32px;
  height: 32px;
}

.service-tag {
  color: #f59e0b;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 2px;
  background: rgba(245, 158, 11, 0.1);
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
  background: linear-gradient(135deg, #f59e0b, #0d9488);
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
  border-color: rgba(245, 158, 11, 0.25);
}

.feat-icon {
  width: 32px;
  height: 32px;
  color: #f59e0b;
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
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.15) 0%, rgba(20, 184, 166, 0.15) 100%);
  border: 1px solid rgba(245, 158, 11, 0.2);
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
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: white;
  border: none;
  padding: 0.85rem 2.2rem;
  border-radius: 12px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.3s;
  box-shadow: 0 4px 15px rgba(245, 158, 11, 0.3);
}

.btn-cta:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.4);
}
</style>
