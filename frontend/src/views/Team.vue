<template>
  <div class="team-wrapper">
    <!-- Background elements -->
    <div class="bg-glow bg-glow-1"></div>
    <div class="bg-glow bg-glow-2"></div>

    <div class="team-container">
      <!-- Back button -->
      <div class="action-bar">
        <router-link to="/" class="btn-back">
          <ArrowLeft class="icon-btn" />
          <span>Quay lại Trang chủ</span>
        </router-link>
      </div>

      <!-- Header Section -->
      <div class="team-header">
        <span class="section-tag">Đội ngũ chuyên gia</span>
        <h1 class="gradient-text">Những Người Bảo Vệ Âm Thầm</h1>
        <p class="team-subtitle">
          MyPetClinic sở hữu đội ngũ y bác sĩ đầu ngành, tận tâm và giàu kinh nghiệm, luôn ứng dụng các kỹ thuật chẩn đoán và điều trị hiện đại nhất vì sức khỏe của thú cưng.
        </p>
      </div>

      <!-- Doctors Grid -->
      <div class="doctors-grid">
        <div 
          v-for="doctor in doctors" 
          :key="doctor.id" 
          class="doctor-card"
        >
          <div class="doctor-avatar-wrapper">
            <img :src="doctor.avatar" :alt="doctor.name" class="doctor-avatar" />
            <div class="doctor-experience-tag">
              {{ doctor.exp }} Năm kinh nghiệm
            </div>
          </div>
          
          <div class="doctor-info">
            <h3 class="doctor-name">{{ doctor.name }}</h3>
            <span class="doctor-role">{{ doctor.role }}</span>
            <p class="doctor-bio">{{ doctor.bio }}</p>
            
            <div class="doctor-degrees">
              <span v-for="deg in doctor.degrees" :key="deg" class="degree-tag">
                {{ deg }}
              </span>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { ArrowLeft } from '@lucide/vue';

const doctors = ref([
  {
    id: 1,
    name: 'Bác sĩ Nguyễn Văn Minh',
    role: 'Giám đốc chuyên môn - Ngoại khoa & Chấn thương chỉnh hình',
    avatar: 'https://images.unsplash.com/photo-1622253692010-333f2da6031d?auto=format&fit=crop&w=300&h=300',
    exp: 12,
    bio: 'Chuyên gia hàng đầu về phẫu thuật ngoại khoa, nội soi khớp và chấn thương chỉnh hình phức tạp cho vật nuôi nhỏ.',
    degrees: ['Thạc sĩ Thú y', 'Đại học Nông Lâm TPHCM', 'Chứng chỉ ngoại khoa nâng cao tại Thái Lan']
  },
  {
    id: 2,
    name: 'Bác sĩ Trần Thị Hồng',
    role: 'Trưởng khoa Nội - Da liễu & Nội tiết Thú y',
    avatar: 'https://images.unsplash.com/photo-1594824813573-246434de83fb?auto=format&fit=crop&w=300&h=300',
    exp: 8,
    bio: 'Chuyên điều trị các bệnh lý mãn tính, bệnh tự miễn, dị ứng da phức tạp và các hội chứng rối loạn nội tiết ở chó mèo.',
    degrees: ['Bác sĩ Thú y', 'Đại học Nông Lâm Huế', 'Chứng chỉ Da liễu Thú y Châu Á']
  },
  {
    id: 3,
    name: 'Bác sĩ Lê Hoàng Nam',
    role: 'Chuyên gia Siêu âm & Chẩn đoán hình ảnh',
    avatar: 'https://images.unsplash.com/photo-1537368910025-700350fe46c7?auto=format&fit=crop&w=300&h=300',
    exp: 6,
    bio: 'Đảm nhận công tác siêu âm tim mạch, ổ bụng chuyên sâu và đọc kết quả hình ảnh X-quang kỹ thuật số công nghệ cao.',
    degrees: ['Bác sĩ Thú y', 'Đại học Tây Nguyên', 'Chứng chỉ Chẩn đoán hình ảnh cao cấp']
  }
]);
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.team-wrapper {
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

.team-container {
  position: relative;
  z-index: 1;
  max-width: 1100px;
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

.team-header {
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

.team-subtitle {
  color: #94a3b8;
  font-size: 1.05rem;
  line-height: 1.7;
  max-width: 700px;
  margin: 0 auto;
}

/* Doctors list grid layout */
.doctors-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 2.5rem;
  margin-bottom: 4rem;
}

.doctor-card {
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 24px;
  overflow: hidden;
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.25);
  transition: transform 0.3s ease, border-color 0.3s ease;
}

.doctor-card:hover {
  transform: translateY(-6px);
  border-color: rgba(20, 184, 166, 0.3);
}

.doctor-avatar-wrapper {
  position: relative;
  height: 320px;
  overflow: hidden;
}

.doctor-avatar {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s ease;
}

.doctor-card:hover .doctor-avatar {
  transform: scale(1.03);
}

.doctor-experience-tag {
  position: absolute;
  bottom: 15px;
  right: 15px;
  background: rgba(15, 23, 42, 0.75);
  backdrop-filter: blur(8px);
  color: #14b8a6;
  font-size: 0.8rem;
  font-weight: 600;
  padding: 0.4rem 1rem;
  border-radius: 20px;
  border: 1px solid rgba(20, 184, 166, 0.3);
}

.doctor-info {
  padding: 2rem;
}

.doctor-name {
  font-size: 1.4rem;
  font-weight: 700;
  color: white;
  margin-bottom: 0.4rem;
}

.doctor-role {
  display: block;
  font-size: 0.9rem;
  font-weight: 600;
  color: #14b8a6;
  margin-bottom: 1rem;
  line-height: 1.4;
}

.doctor-bio {
  font-size: 0.9rem;
  color: #94a3b8;
  line-height: 1.6;
  margin-bottom: 1.5rem;
}

.doctor-degrees {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  padding-top: 1.2rem;
}

.degree-tag {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.08);
  color: #cbd5e1;
  font-size: 0.75rem;
  font-weight: 500;
  padding: 0.3rem 0.8rem;
  border-radius: 20px;
}
</style>
