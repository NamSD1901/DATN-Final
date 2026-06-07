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
          <Heart class="service-icon" />
        </div>
        <span class="service-tag">Dịch vụ tư vấn</span>
        <h1>Tư Vấn Sức Khỏe & Dinh Dưỡng</h1>
        <p class="service-subtitle">
          Chăm sóc chủ động bằng cách tối ưu hóa dinh dưỡng và thói quen sinh hoạt khoa học dưới sự hướng dẫn của bác sĩ chuyên khoa dinh dưỡng thú y.
        </p>
      </div>

      <!-- Feature Grid -->
      <div class="feature-grid">
        <div class="feature-card">
          <Apple class="feat-icon" />
          <h3>Cá Nhân Hóa Thực Đơn</h3>
          <p>Thiết lập chế độ ăn phù hợp cho từng giai đoạn phát triển, kiểm soát cân nặng béo phì hoặc hỗ trợ điều trị bệnh lý (thận, gan, tiết niệu).</p>
        </div>

        <div class="feature-card">
          <Activity class="feat-icon" />
          <h3>Kiểm Tra Thể Trạng Định Kỳ</h3>
          <p>Đánh giá chỉ số cơ thể BCS, kiểm tra chất lượng cơ bắp, độ ẩm của da và tư vấn bài tập thể lực nâng cao đề kháng.</p>
        </div>

        <div class="feature-card">
          <ShieldAlert class="feat-icon" />
          <h3>Tư Vấn Phòng Ngừa Bệnh</h3>
          <p>Giải đáp thắc mắc về phòng chống ký sinh trùng (ve, rận, giun tim), chăm sóc răng miệng định kỳ và huấn luyện hành vi cơ bản.</p>
        </div>
      </div>

      <!-- Detail Info Section -->
      <div class="detail-section">
        <h2>Tại Sao Cần Tư Vấn Sức Khỏe Cho Pet?</h2>
        <p class="section-desc">Phòng bệnh luôn tốt hơn chữa bệnh. Hơn 80% bệnh lý thường gặp ở chó mèo (như sỏi thận, rụng lông, viêm da, béo phì) đều bắt nguồn từ thói quen ăn uống và chăm sóc sai cách của chủ nuôi.</p>

        <div class="bullets-container">
          <div class="bullet-item">
            <CheckCircle2 class="bullet-icon" />
            <span>Được tham vấn bởi các chuyên gia dinh dưỡng thú y hàng đầu tốt nghiệp các trường Nông Lâm danh tiếng.</span>
          </div>
          <div class="bullet-item">
            <CheckCircle2 class="bullet-icon" />
            <span>Tặng kèm cẩm nang hướng dẫn tự chuẩn bị thức ăn sạch (Raw Food/Barf) an toàn tại nhà.</span>
          </div>
          <div class="bullet-item">
            <CheckCircle2 class="bullet-icon" />
            <span>Theo dõi sức khỏe trọn đời thông qua hồ sơ điện tử lưu trữ tại Clinic.</span>
          </div>
        </div>
      </div>

      <!-- Call To Action -->
      <div class="cta-card">
        <h3>Bạn đang lo lắng về chế độ ăn uống hoặc cân nặng của thú cưng?</h3>
        <p>Đặt lịch hẹn tư vấn miễn phí ngay hôm nay khi kèm theo gói khám sức khỏe tổng quát.</p>
        <router-link to="/contact" class="btn-cta">Đăng ký tư vấn ngay</router-link>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ArrowLeft, Heart, Apple, Activity, ShieldAlert, CheckCircle2 } from '@lucide/vue';
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
  background: radial-gradient(circle, #ec4899, transparent);
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
  background: rgba(236, 72, 153, 0.15);
  color: #ec4899;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0 auto 1.5rem auto;
  box-shadow: 0 8px 20px rgba(236, 72, 153, 0.2);
}

.service-icon {
  width: 32px;
  height: 32px;
}

.service-tag {
  color: #ec4899;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 2px;
  background: rgba(236, 72, 153, 0.1);
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
  background: linear-gradient(135deg, #ec4899, #0d9488);
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
  border-color: rgba(236, 72, 153, 0.25);
}

.feat-icon {
  width: 32px;
  height: 32px;
  color: #ec4899;
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

/* Detail section styling */
.detail-section {
  background: rgba(30, 41, 59, 0.25);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 20px;
  padding: 2.5rem;
  margin-bottom: 4.5rem;
}

.detail-section h2 {
  font-size: 1.6rem;
  font-weight: 700;
  color: white;
  margin-bottom: 0.8rem;
}

.section-desc {
  font-size: 0.95rem;
  color: #94a3b8;
  line-height: 1.7;
  margin-bottom: 2rem;
}

.bullets-container {
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
}

.bullet-item {
  display: flex;
  gap: 12px;
  align-items: flex-start;
}

.bullet-icon {
  width: 20px;
  height: 20px;
  color: #ec4899;
  flex-shrink: 0;
  margin-top: 2px;
}

.bullet-item span {
  font-size: 0.95rem;
  color: #cbd5e1;
  line-height: 1.5;
}

/* CTA Card */
.cta-card {
  background: linear-gradient(135deg, rgba(236, 72, 153, 0.15) 0%, rgba(20, 184, 166, 0.15) 100%);
  border: 1px solid rgba(236, 72, 153, 0.2);
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
  background: linear-gradient(135deg, #ec4899, #be185d);
  color: white;
  border: none;
  padding: 0.85rem 2.2rem;
  border-radius: 12px;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.3s;
  box-shadow: 0 4px 15px rgba(236, 72, 153, 0.3);
}

.btn-cta:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(236, 72, 153, 0.4);
}
</style>
