<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <Clock class="icon-clock" /> Hành Trình Phát Triển
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">LỊCH SỬ PHÁT TRIỂN</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Hơn một thập kỷ đồng hành cùng hàng triệu gia đình Việt Nam, bảo vệ sức khỏe và mang lại niềm vui trọn vẹn cho các bé cưng.
        </p>
      </div>
    </section>

    <!-- History Content -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <!-- Vision & Mission Card Row -->
        <div class="mission-grid mb-5">
          <!-- Vision -->
          <div class="mission-card glass-card">
            <div class="mission-icon-wrapper">
              <Eye class="mission-icon" />
            </div>
            <h4 class="fw-bold mb-3">Tầm Nhìn</h4>
            <p class="text-muted small">
              Trở thành hệ thống bệnh viện thú y dẫn đầu cả nước về uy tín y khoa, chất lượng dịch vụ và ứng dụng công nghệ chẩn đoán tiên tiến nhất.
            </p>
          </div>
          <!-- Mission -->
          <div class="mission-card glass-card">
            <div class="mission-icon-wrapper">
              <Target class="mission-icon" />
            </div>
            <h4 class="fw-bold mb-3">Sứ Mệnh</h4>
            <p class="text-muted small">
              Mang lại cuộc sống khỏe mạnh, an toàn và hạnh phúc cho thú cưng bằng sự thấu hiểu sâu sắc, chuyên môn vượt trội và lòng trắc ẩn bao dung.
            </p>
          </div>
          <!-- Core values -->
          <div class="mission-card glass-card">
            <div class="mission-icon-wrapper">
              <Gem class="mission-icon" />
            </div>
            <h4 class="fw-bold mb-3">Giá Trị Cốt Lõi</h4>
            <p class="text-muted small">
              <strong>Y đức hàng đầu</strong> – <strong>Tận tâm phục vụ</strong> – <strong>Đổi mới không ngừng</strong> – <strong>Chuyên nghiệp vượt bậc</strong>.
            </p>
          </div>
        </div>

        <!-- Timeline Section -->
        <h2 class="text-center fw-bold mb-5 section-title-center">Dấu Mốc Quan Trọng</h2>
        <div class="timeline-container">
          <!-- Year 2012 -->
          <div class="timeline-item">
            <div class="timeline-marker">
              <Flag class="marker-icon" />
            </div>
            <h4 class="fw-bold text-warning mb-2">Năm 2012 - Khởi đầu Hành trình Bằng Đam Mê</h4>
            <p class="text-muted lh-lg">
              Vào những ngày đầu năm 2012, MyPetClinic chính thức được thành lập với một phòng khám nhỏ tọa lạc tại trung tâm TP. HCM. Được xây dựng bởi một nhóm các bác sĩ thú y trẻ tuổi, đầy nhiệt huyết và có chung một tình yêu to lớn dành cho động vật. Ban đầu, phòng khám chỉ cung cấp các dịch vụ thú y cơ bản như tiêm phòng, tẩy giun và tư vấn dinh dưỡng. Tuy nhiên, với cam kết cốt lõi "Chăm sóc như người bạn thân thiết nhất", đội ngũ y bác sĩ đã không ngừng nỗ lực túc trực ngày đêm để cứu chữa các ca bệnh khó. Chính sự tận tâm, tỉ mỉ và lòng yêu thương vô điều kiện đã giúp MyPetClinic nhanh chóng chiếm trọn niềm tin và trở thành điểm đến quen thuộc của hàng trăm chủ nuôi trong khu vực.
            </p>
          </div>

          <!-- Year 2016 -->
          <div class="timeline-item">
            <div class="timeline-marker">
              <Heart class="marker-icon" />
            </div>
            <h4 class="fw-bold text-warning mb-2">Năm 2016 - Mở rộng Quy mô & Đầu tư Công nghệ Hiện đại</h4>
            <p class="text-muted lh-lg">
              Nhận thấy nhu cầu chăm sóc sức khỏe thú cưng ngày càng cao và đòi hỏi chuyên môn sâu hơn, năm 2016 đánh dấu bước ngoặt lớn khi MyPetClinic chính thức chuyển đổi mô hình từ phòng khám thành Bệnh viện Thú y đa khoa. Chúng tôi đã mạnh tay đầu tư hàng tỷ đồng để nhập khẩu đồng bộ hệ thống máy móc từ Đức và Nhật Bản, bao gồm: hệ thống máy xét nghiệm máu tự động sinh hóa, máy siêu âm màu 4D chuyên dụng cho thú nhỏ, và hệ thống X-quang kỹ thuật số. Đồng thời, cơ sở thứ 2 tại khu vực Đông Sài Gòn cũng được khai trương với quy mô lớn, thiết kế phân khu riêng biệt cho chó và mèo nhằm giảm căng thẳng, nâng cao đáng kể khả năng tiếp nhận và chất lượng chăm sóc nội trú dài ngày cho các bé cưng.
            </p>
          </div>

          <!-- Year 2020 -->
          <div class="timeline-item">
            <div class="timeline-marker">
              <Award class="marker-icon" />
            </div>
            <h4 class="fw-bold text-warning mb-2">Năm 2020 - Chuẩn hóa Quy trình Theo Tiêu chuẩn Quốc tế</h4>
            <p class="text-muted lh-lg">
              Bất chấp những thách thức của đại dịch toàn cầu, MyPetClinic vẫn kiên định với mục tiêu nâng tầm chất lượng y khoa. Chúng tôi chính thức ký kết liên kết hợp tác chuyên môn với các trường đại học và tổ chức y tế thú y hàng đầu tại Thái Lan và Hàn Quốc. Tại các cơ sở, MyPetClinic đã xây dựng thành công khu vực phẫu thuật vô trùng khép kín với hệ thống lọc không khí áp lực dương, cùng phòng hồi sức tích cực (ICU) trang bị lồng ấp dưỡng khí chất lượng cao. Cơ sở thứ 3 ra đời mang tính đột phá khi tích hợp hệ sinh thái toàn diện: từ bệnh viện khám chữa bệnh chuyên sâu đến khu nghỉ dưỡng cao cấp, khách sạn lưu trú chuồng tĩnh âm và trung tâm Spa - Grooming cắt tỉa làm đẹp chuyên nghiệp.
            </p>
          </div>

          <!-- Year 2026 -->
          <div class="timeline-item">
            <div class="timeline-marker">
              <Star class="marker-icon" />
            </div>
            <h4 class="fw-bold text-warning mb-2">Năm 2026 - Khẳng định Vị thế Premium & Vươn tầm Tương lai</h4>
            <p class="text-muted lh-lg">
              Trải qua hơn một thập kỷ hình thành và phát triển, MyPetClinic tự hào định vị là hệ thống Bệnh viện Thú y Premium hàng đầu Việt Nam. Hiện tại, 100% đội ngũ y bác sĩ chủ chốt đều sở hữu các chứng chỉ đào tạo chuyên sâu quốc tế và thường xuyên tham gia các hội thảo y khoa thế giới. Chúng tôi là một trong những đơn vị tiên phong áp dụng hệ thống hồ sơ y tế điện tử thông minh, đồng bộ dữ liệu trên toàn hệ thống giúp theo dõi sức khỏe thú cưng trọn đời một cách liền mạch và bảo mật. Với khát vọng không ngừng vươn xa, MyPetClinic cam kết sẽ luôn là điểm tựa vững chắc nhất, mang lại dịch vụ chăm sóc hoàn hảo, bằng tất cả trái tim và trách nhiệm nghề nghiệp cao quý nhất.
            </p>
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
import { Clock, Eye, Target, Gem, Flag, Heart, Award, Star } from '@lucide/vue';

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

.icon-clock {
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
  margin-bottom: 3rem;
  text-align: center;
}

/* Vision mission grid */
.mission-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 2rem;
}

.mission-card {
  padding: 2rem;
  text-align: center;
  background-color: white !important;
}

.mission-icon-wrapper {
  background-color: rgba(245, 158, 11, 0.2);
  color: var(--primary-gold);
  width: 70px;
  height: 70px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1.5rem auto;
}

.mission-icon {
  width: 30px;
  height: 30px;
}

/* Timeline */
.timeline-container {
  border-left: 3px solid var(--primary-light);
  margin-left: 30px;
  padding-left: 30px;
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 3.5rem;
}

.timeline-item {
  position: relative;
}

.timeline-marker {
  position: absolute;
  left: -48px;
  top: 0;
  background-color: var(--primary-gold);
  color: white;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 4px solid white;
  box-shadow: var(--shadow-sm);
}

.marker-icon {
  width: 14px;
  height: 14px;
}
</style>
