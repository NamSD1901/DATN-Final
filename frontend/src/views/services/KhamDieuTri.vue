<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <HeartPulse class="icon-pulse" /> Khám & Điều Trị Chuyên Sâu
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">DỊCH VỤ KHÁM & ĐIỀU TRỊ</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Quy trình chuẩn hóa quốc tế, trang thiết bị chẩn đoán công nghệ cao cùng đội ngũ y bác sĩ đầu ngành bảo vệ sức khỏe bé cưng.
        </p>
      </div>
    </section>

    <!-- Services Body -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <div class="grid-layout">
          <!-- Sidebar Navigation Links -->
          <aside class="sidebar-wrapper">
            <div class="sticky-sidebar">
              <div class="card border-0 glass-card p-3">
                <h5 class="fw-bold mb-3 pb-2 border-bottom text-warning d-flex align-items-center gap-2">
                  <ListRestart class="sidebar-title-icon" /> Các Dịch Vụ
                </h5>
                <div class="nav flex-column gap-1">
                  <a class="sidebar-link" href="#tong-quat" @click.prevent="scrollToSection('tong-quat')">
                    <span class="dot"></span> Khám Tổng Quát
                  </a>
                  <a class="sidebar-link" href="#chuyen-khoa" @click.prevent="scrollToSection('chuyen-khoa')">
                    <span class="dot"></span> Khám Chuyên Khoa
                  </a>
                  <a class="sidebar-link" href="#chan-doan-hinh-anh" @click.prevent="scrollToSection('chan-doan-hinh-anh')">
                    <span class="dot"></span> Chẩn Đoán Hình Ảnh
                  </a>
                  <a class="sidebar-link" href="#dieu-tri" @click.prevent="scrollToSection('dieu-tri')">
                    <span class="dot"></span> Điều Trị Bệnh
                  </a>
                  <a class="sidebar-link" href="#cham-soc" @click.prevent="scrollToSection('cham-soc')">
                    <span class="dot"></span> Chăm Sóc Sau Điều Trị
                  </a>
                  <a class="sidebar-link" href="#phau-thuat" @click.prevent="scrollToSection('phau-thuat')">
                    <span class="dot"></span> Phẫu Thuật Ngoại Khoa
                  </a>
                  <a class="sidebar-link" href="#xet-nghiem" @click.prevent="scrollToSection('xet-nghiem')">
                    <span class="dot"></span> Xét Nghiệm & Siêu Âm
                  </a>
                </div>
              </div>
            </div>
          </aside>

          <!-- Content Area -->
          <main class="main-content">
            <!-- Section 1: Khám Tổng Quát -->
            <div id="tong-quat" class="content-block pb-5 border-bottom">
              <div class="d-flex align-items-center gap-3 mb-3 block-header">
                <div class="icon-wrapper">
                  <Stethoscope class="block-icon" />
                </div>
                <h3 class="fw-bold mb-0">1. Khám Tổng Quát</h3>
              </div>
              <p class="text-muted block-desc">
                Khám tổng quát định kỳ là biện pháp phòng ngừa hữu hiệu nhất giúp phát hiện sớm các dấu hiệu bất thường của cơ thể, kiểm soát cân nặng, răng miệng và hệ tiêu hóa của thú cưng.
              </p>
              <div class="card bg-light border-0 p-4 rounded-4 mb-3 info-card">
                <h6 class="fw-bold text-dark mb-2">Quy trình khám bao gồm:</h6>
                <ul class="text-muted small ps-3 mb-0">
                  <li class="mb-1">Kiểm tra chỉ số sinh tồn (thân nhiệt, nhịp tim, nhịp thở).</li>
                  <li class="mb-1">Khám lâm sàng tai, mắt, mũi, họng, kiểm tra răng miệng.</li>
                  <li class="mb-1">Sờ nắn ổ bụng để kiểm tra gan, thận và hệ đường ruột.</li>
                  <li class="mb-1">Đánh giá tình trạng da lông, phát hiện các bệnh ký sinh trùng ngoài da.</li>
                  <li>Tư vấn chế độ dinh dưỡng và các phác đồ tiêm phòng định kỳ.</li>
                </ul>
              </div>
              <p class="small text-muted font-bold"><strong>Khuyên dùng:</strong> 6 tháng một lần đối với chó mèo trưởng thành.</p>
            </div>

            <!-- Section 2: Khám Chuyên Khoa -->
            <div id="chuyen-khoa" class="content-block py-5 border-bottom">
              <div class="d-flex align-items-center gap-3 mb-3 block-header">
                <div class="icon-wrapper">
                  <ShieldCheck class="block-icon" />
                </div>
                <h3 class="fw-bold mb-0">2. Khám Chuyên Khoa</h3>
              </div>
              <p class="text-muted block-desc">
                Khi thú cưng của bạn gặp các triệu chứng phức tạp liên quan đến hệ tim mạch, tiết niệu, cơ xương khớp hoặc thần kinh, các bác sĩ chuyên khoa của chúng tôi sẽ tiếp nhận và tiến hành các phương pháp chẩn đoán chuyên sâu.
              </p>
              <div class="row-cards">
                <div class="col-card">
                  <div class="card border-0 glass-card p-3 h-100 sub-card">
                    <h6 class="fw-bold text-dark d-flex align-items-center gap-2">
                      <Activity class="sub-card-icon" /> Chuyên khoa Tim mạch
                    </h6>
                    <p class="text-muted small mb-0">Đo điện tâm đồ (ECG), siêu âm tim Doppler phát hiện hở van tim, cơ tim phì đại.</p>
                  </div>
                </div>
                <div class="col-card">
                  <div class="card border-0 glass-card p-3 h-100 sub-card">
                    <h6 class="fw-bold text-dark d-flex align-items-center gap-2">
                      <Droplets class="sub-card-icon" /> Chuyên khoa Thận - Tiết niệu
                    </h6>
                    <p class="text-muted small mb-0">Hỗ trợ chẩn đoán và điều trị sỏi bàng quang, suy thận cấp, bí tiểu ở mèo đực.</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Section 3: Chẩn Đoán Hình Ảnh -->
            <div id="chan-doan-hinh-anh" class="content-block py-5 border-bottom">
              <div class="d-flex align-items-center gap-3 mb-3 block-header">
                <div class="icon-wrapper">
                  <Camera class="block-icon" />
                </div>
                <h3 class="fw-bold mb-0">3. Chẩn Đoán Hình Ảnh</h3>
              </div>
              <p class="text-muted block-desc">
                MyPetClinic sở hữu các trang thiết bị chẩn đoán hình ảnh tiên tiến bậc nhất hiện nay, giúp các bác sĩ nhìn rõ bên trong cơ thể vật nuôi một cách chính xác mà không gây đau đớn cho vật nuôi.
              </p>
              <ul class="text-muted small ps-3 bullet-list">
                <li class="mb-2"><strong>Hệ thống X-Quang Kỹ thuật số (Digital X-Ray):</strong> Cho hình ảnh độ phân giải cao trong 3 giây, hỗ trợ phát hiện gãy xương, dị vật dạ dày, viêm phổi.</li>
                <li><strong>Siêu âm màu 4D (Color Ultrasound):</strong> Đánh giá chi tiết tình trạng gan, thận, tử cung, thai sản và khối u ổ bụng.</li>
              </ul>
            </div>

            <!-- Section 4: Điều Trị Bệnh -->
            <div id="dieu-tri" class="content-block py-5 border-bottom">
              <div class="d-flex align-items-center gap-3 mb-3 block-header">
                <div class="icon-wrapper">
                  <Thermometer class="block-icon" />
                </div>
                <h3 class="fw-bold mb-0">4. Điều Trị Bệnh</h3>
              </div>
              <p class="text-muted block-desc">
                Với phác đồ điều trị được thiết kế cá nhân hóa dựa trên thể trạng cụ thể của từng bé cưng, chúng tôi cam kết mang lại hiệu quả điều trị cao nhất đối với các bệnh truyền nhiễm (Care, Parvo, giảm bạch cầu), bệnh gan thận mãn tính, truyền dịch, và giải độc cấp tính.
              </p>
            </div>

            <!-- Section 5: Chăm Sóc Sau Điều Trị -->
            <div id="cham-soc" class="content-block py-5 border-bottom">
              <div class="d-flex align-items-center gap-3 mb-3 block-header">
                <div class="icon-wrapper">
                  <Heart class="block-icon" />
                </div>
                <h3 class="fw-bold mb-0">5. Chăm Sóc Sau Điều Trị</h3>
              </div>
              <p class="text-muted block-desc">
                Khu vực chăm sóc nội trú và điều dưỡng sau phẫu thuật luôn sạch sẽ, thoáng mát và ấm áp. Thú cưng được chăm sóc liên tục, cho ăn theo chế độ dinh dưỡng phục hồi chuyên biệt và được theo dõi sát sao nhiệt độ, huyết áp.
              </p>
            </div>

            <!-- Section 6: Phẫu Thuật Ngoại Khoa -->
            <div id="phau-thuat" class="content-block py-5 border-bottom">
              <div class="d-flex align-items-center gap-3 mb-3 block-header">
                <div class="icon-wrapper">
                  <Hospital class="block-icon" />
                </div>
                <h3 class="fw-bold mb-0">6. Phẫu Thuật Ngoại Khoa</h3>
              </div>
              <p class="text-muted block-desc">
                Phòng mổ vô trùng tại MyPetClinic đảm bảo an toàn tối đa cho vật nuôi trong quá trình can thiệp ngoại khoa.
              </p>
              <div class="row-cards">
                <div class="col-card">
                  <div class="card border-0 bg-light p-3 info-card">
                    <h6 class="fw-bold text-dark">Phẫu thuật thông thường</h6>
                    <p class="text-muted small mb-0">Triệt sản thẩm mỹ (chó mèo đực/cái), mổ đẻ khẩn cấp, cắt u lành tính.</p>
                  </div>
                </div>
                <div class="col-card">
                  <div class="card border-0 bg-light p-3 info-card">
                    <h6 class="fw-bold text-dark">Phẫu thuật chấn thương</h6>
                    <p class="text-muted small mb-0">Nối xương, đóng đinh nội tủy, vá cơ hoành, khâu vết rách sâu.</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Section 7: Xét Nghiệm & Siêu Âm -->
            <div id="xet-nghiem" class="content-block py-5">
              <div class="d-flex align-items-center gap-3 mb-3 block-header">
                <div class="icon-wrapper">
                  <FlaskConical class="block-icon" />
                </div>
                <h3 class="fw-bold mb-0">7. Xét Nghiệm & Siêu Âm</h3>
              </div>
              <p class="text-muted block-desc">
                Lab xét nghiệm tại chỗ giúp trả kết quả nhanh chóng trong vòng 15-30 phút, tạo điều kiện cho bác sĩ đưa ra phác đồ cấp cứu kịp thời.
              </p>
              <div class="table-container">
                <table class="table-premium">
                  <thead>
                    <tr>
                      <th scope="col">Loại Xét Nghiệm</th>
                      <th scope="col">Thời gian nhận kết quả</th>
                      <th scope="col">Ứng dụng lâm sàng</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td class="fw-semibold text-dark">Công thức máu toàn bộ (CBC)</td>
                      <td>15 phút</td>
                      <td>Kiểm tra nhiễm trùng, thiếu máu, mất máu cấp.</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">Sinh hóa máu (Gan, Thận, Đường huyết)</td>
                      <td>20 phút</td>
                      <td>Đánh giá chức năng phủ tạng trước khi gây mê phẫu thuật.</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">Xét nghiệm nhanh virus (Parvo, Care, FIV/FeLV)</td>
                      <td>10 phút</td>
                      <td>Chẩn đoán nhanh các bệnh dịch truyền nhiễm nguy hiểm.</td>
                    </tr>
                    <tr>
                      <td class="fw-semibold text-dark">Soi kính hiển vi (Ký sinh trùng tai, da lông)</td>
                      <td>15 phút</td>
                      <td>Xác định nguyên nhân gây ngứa, rụng lông, viêm tai.</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </main>
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
import { 
  HeartPulse, ListRestart, Stethoscope, Droplets, ShieldCheck, 
  Activity, Camera, Thermometer, Heart, Hospital, FlaskConical 
} from '@lucide/vue';

const showBookingModal = ref(false);

const handleBookingSuccess = (msg: string) => {
  alert(msg);
};

const handleBookingError = (msg: string) => {
  alert(msg);
};

const scrollToSection = (id: string) => {
  const el = document.getElementById(id);
  if (el) {
    window.scrollTo({
      top: el.offsetTop - 100,
      behavior: 'smooth'
    });
  }
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

.icon-pulse {
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

.grid-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 3rem;
}

@media (max-width: 991px) {
  .grid-layout {
    grid-template-columns: 1fr;
  }
  .sidebar-wrapper {
    display: none;
  }
}

.sticky-sidebar {
  position: sticky;
  top: 100px;
  z-index: 100;
}

.sidebar-title-icon {
  width: 20px;
  height: 20px;
}

.sidebar-link {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0.6rem 0.8rem;
  border-radius: var(--radius-sm);
  color: var(--text-dark) !important;
  font-weight: 600;
  font-size: 0.85rem;
  text-decoration: none;
  transition: all var(--transition-speed);
}

.sidebar-link .dot {
  width: 6px;
  height: 6px;
  background-color: var(--primary-gold);
  border-radius: 50%;
}

.sidebar-link:hover {
  background-color: var(--primary-cream);
  color: var(--primary-dark) !important;
  transform: translateX(4px);
}

.icon-wrapper {
  background-color: rgba(245, 158, 11, 0.2);
  color: var(--primary-gold);
  padding: 0.8rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.block-icon {
  width: 28px;
  height: 28px;
}

.block-desc {
  font-size: 0.95rem;
  line-height: 1.7;
  margin-bottom: 1.5rem;
}

.info-card {
  border-radius: 16px;
  background-color: #f7f6f2 !important;
}

.row-cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 576px) {
  .row-cards {
    grid-template-columns: 1fr;
  }
}

.sub-card {
  padding: 1.5rem;
  background: white !important;
}

.sub-card-icon {
  width: 18px;
  height: 18px;
  color: var(--primary-gold);
}

.bullet-list {
  line-height: 1.7;
}

/* Table styling */
.table-container {
  overflow-x: auto;
  border-radius: 12px;
  border: 1px solid var(--border-color);
  margin-top: 1.5rem;
}

.table-premium {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
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
</style>
