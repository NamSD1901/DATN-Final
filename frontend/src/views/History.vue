<template>
  <div class="history-wrapper">
    <!-- Background elements -->
    <div class="bg-glow bg-glow-1"></div>
    <div class="bg-glow bg-glow-2"></div>

    <div class="history-container">
      <!-- Back button -->
      <div class="action-bar">
        <router-link to="/" class="btn-back">
          <ArrowLeft class="icon-btn" />
          <span>Quay lại Trang chủ</span>
        </router-link>
      </div>

      <!-- Header Section -->
      <div class="history-header">
        <span class="section-tag">Chặng đường phát triển</span>
        <h1 class="gradient-text">Hành Trình Kiến Tạo Niềm Tin</h1>
        <p class="history-subtitle">
          MyPetClinic khởi đầu từ tình yêu thương vô bờ bến dành cho động vật. Trải qua một thập kỷ xây dựng và trưởng thành, chúng tôi tự hào là điểm tựa sức khỏe vững chắc cho hàng ngàn thú cưng.
        </p>
      </div>

      <!-- Timeline Section -->
      <div class="timeline">
        <div 
          v-for="(milestone, idx) in milestones" 
          :key="milestone.year" 
          class="timeline-item"
          :class="{ 'align-right': idx % 2 !== 0 }"
        >
          <div class="timeline-dot"></div>
          <div class="timeline-content">
            <span class="milestone-year">{{ milestone.year }}</span>
            <h3 class="milestone-title">{{ milestone.title }}</h3>
            <p class="milestone-desc">{{ milestone.desc }}</p>
            <div class="milestone-badge" :style="{ background: milestone.color }">
              {{ milestone.badge }}
            </div>
          </div>
        </div>
      </div>

      <!-- Vision & Values Section -->
      <div class="vision-values">
        <div class="info-card">
          <div class="icon-box icon-vision">
            <Eye class="card-icon" />
          </div>
          <h3>Tầm Nhìn</h3>
          <p>Trở thành hệ thống bệnh viện thú y tiêu chuẩn quốc tế đi đầu về chẩn đoán hình ảnh và phẫu thuật nội soi tại Việt Nam, mang đến chất lượng dịch vụ tốt nhất cho thú cưng.</p>
        </div>

        <div class="info-card">
          <div class="icon-box icon-mission">
            <Sparkles class="card-icon" />
          </div>
          <h3>Sứ Mệnh</h3>
          <p>Đồng hành cùng cộng đồng yêu thú cưng để bảo vệ sức khỏe và kéo dài tuổi thọ cho vật nuôi bằng các phương pháp y khoa tiên tiến, an toàn nhất.</p>
        </div>

        <div class="info-card">
          <div class="icon-box icon-values">
            <HeartHandshake class="card-icon" />
          </div>
          <h3>Giá Trị Cốt Lõi</h3>
          <p>Yêu thương làm gốc - Chuyên nghiệp làm chuẩn - Tận tâm làm thước đo. Tất cả các y bác sĩ tại phòng khám luôn hành động bằng lòng yêu thương và trách nhiệm cao nhất.</p>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ArrowLeft, Eye, Sparkles, HeartHandshake } from '@lucide/vue';

const milestones = ref([
  {
    year: '2016',
    title: 'Khởi đầu từ Đam mê',
    desc: 'Phòng khám thú y MyPetClinic đầu tiên được thành lập với quy mô nhỏ gồm 2 bác sĩ trực ca và dịch vụ khám lâm sàng cơ bản.',
    badge: 'Thành lập',
    color: '#0d9488'
  },
  {
    year: '2019',
    title: 'Mở rộng quy mô & Dịch vụ Spa',
    desc: 'Chính thức khai trương cơ sở 2 tại trung tâm thành phố Đà Nẵng, tích hợp thêm hệ thống Spa & Grooming làm đẹp cao cấp cho thú cưng.',
    badge: 'Mở rộng',
    color: '#6366f1'
  },
  {
    year: '2022',
    title: 'Đột phá Y khoa & Thiết bị hiện đại',
    desc: 'Nhập khẩu hệ thống máy siêu âm 4D, máy chụp X-quang kỹ thuật số và phòng xét nghiệm máu tự động cho kết quả chính xác tuyệt đối sau 15 phút.',
    badge: 'Nâng cấp công nghệ',
    color: '#f59e0b'
  },
  {
    year: '2026',
    title: 'Bệnh viện Thú y hàng đầu',
    desc: 'Phục vụ trên 5,000 khách hàng trung thành, phát triển đội ngũ lên hơn 15 bác sĩ thú y chuyên nghiệp hỗ trợ chẩn đoán và phẫu thuật 24/7.',
    badge: 'Dẫn đầu',
    color: '#ec4899'
  }
]);
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.history-wrapper {
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

.history-container {
  position: relative;
  z-index: 1;
  max-width: 900px;
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

.history-header {
  text-align: center;
  margin-bottom: 4.5rem;
}

.section-tag {
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

.gradient-text {
  font-size: 2.8rem;
  font-weight: 800;
  letter-spacing: -1px;
  margin-bottom: 1.2rem;
  background: linear-gradient(135deg, #14b8a6, #6366f1);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.history-subtitle {
  color: #94a3b8;
  font-size: 1.05rem;
  line-height: 1.7;
  max-width: 700px;
  margin: 0 auto;
}

/* Timeline Layout */
.timeline {
  position: relative;
  max-width: 700px;
  margin: 0 auto 5rem auto;
}

.timeline::after {
  content: '';
  position: absolute;
  width: 2px;
  background: rgba(255, 255, 255, 0.08);
  top: 0;
  bottom: 0;
  left: 50%;
  margin-left: -1px;
}

.timeline-item {
  padding: 1.5rem 0;
  position: relative;
  width: 50%;
  clear: both;
}

.timeline-item::after {
  content: "";
  display: table;
  clear: both;
}

.timeline-dot {
  width: 14px;
  height: 14px;
  background: #14b8a6;
  border: 3px solid #0f172a;
  border-radius: 50%;
  position: absolute;
  top: 2.2rem;
  left: 100%;
  margin-left: -7px;
  z-index: 1;
}

.timeline-content {
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 16px;
  padding: 1.8rem;
  margin-right: 2.5rem;
  position: relative;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  transition: transform 0.3s ease, border-color 0.3s ease;
}

.timeline-content:hover {
  transform: translateY(-4px);
  border-color: rgba(20, 184, 166, 0.3);
}

.milestone-year {
  font-size: 1.8rem;
  font-weight: 700;
  color: #14b8a6;
  display: block;
  margin-bottom: 0.5rem;
}

.milestone-title {
  font-size: 1.15rem;
  color: white;
  font-weight: 600;
  margin-bottom: 0.6rem;
}

.milestone-desc {
  font-size: 0.9rem;
  color: #94a3b8;
  line-height: 1.5;
  margin-bottom: 1.2rem;
}

.milestone-badge {
  display: inline-block;
  font-size: 0.75rem;
  font-weight: 600;
  color: white;
  padding: 0.25rem 0.75rem;
  border-radius: 10px;
}

/* Align Right Layout overrides */
.timeline-item.align-right {
  float: right;
  left: 50%;
}

.timeline-item.align-right .timeline-dot {
  left: 0;
  margin-left: -7px;
}

.timeline-item.align-right .timeline-content {
  margin-right: 0;
  margin-left: 2.5rem;
}

@media (max-width: 768px) {
  .timeline::after {
    left: 20px;
  }
  .timeline-item {
    width: 100%;
    left: 0 !important;
    float: none;
    padding-left: 45px;
    padding-right: 0;
  }
  .timeline-dot {
    left: 20px !important;
  }
  .timeline-content {
    margin-left: 0 !important;
    margin-right: 0 !important;
  }
}

/* Vision and values card section */
.vision-values {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
  margin-bottom: 4rem;
}

@media (max-width: 768px) {
  .vision-values {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
}

.info-card {
  background: rgba(30, 41, 59, 0.3);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 20px;
  padding: 2rem;
  text-align: center;
  transition: all 0.3s;
}

.info-card:hover {
  background: rgba(30, 41, 59, 0.45);
  border-color: rgba(20, 184, 166, 0.15);
  transform: translateY(-4px);
}

.icon-box {
  width: 50px;
  height: 50px;
  border-radius: 14px;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0 auto 1.2rem auto;
}

.icon-vision { background: rgba(13, 148, 136, 0.15); color: #14b8a6; }
.icon-mission { background: rgba(99, 102, 241, 0.15); color: #6366f1; }
.icon-values { background: rgba(245, 158, 11, 0.15); color: #f59e0b; }

.card-icon {
  width: 22px;
  height: 22px;
}

.info-card h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: white;
  margin-bottom: 0.8rem;
}

.info-card p {
  font-size: 0.9rem;
  color: #94a3b8;
  line-height: 1.6;
}
</style>
