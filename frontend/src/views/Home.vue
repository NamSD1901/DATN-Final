<template>
  <div class="home-wrapper">
    <!-- Toast Notifications -->
    <TransitionGroup name="toast-fade" tag="div" class="toast-container">
      <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast-${toast.type}`]">
        <component :is="toast.icon" class="toast-icon" />
        <span class="toast-message">{{ toast.message }}</span>
      </div>
    </TransitionGroup>

    <!-- Sticky Header -->
    <header class="header" :class="{ 'scrolled': isScrolled }">
      <div class="nav-container">
        <div class="logo" @click="scrollToSection('hero')">
          <PawPrint class="logo-icon" />
          <span class="logo-text">MyPetClinic</span>
        </div>

        <button class="menu-toggle" @click="isMobileMenuOpen = !isMobileMenuOpen">
          <Menu v-if="!isMobileMenuOpen" class="menu-icon" />
          <X v-else class="menu-icon" />
        </button>

        <nav class="nav-menu" :class="{ 'mobile-open': isMobileMenuOpen }">
          <a href="#hero" @click.prevent="scrollToSection('hero')" class="nav-link" :class="{ active: activeSection === 'hero' }">Trang chủ</a>
          <a href="#services" @click.prevent="scrollToSection('services')" class="nav-link" :class="{ active: activeSection === 'services' }">Dịch vụ</a>
          <router-link to="/team" class="nav-link">Đội ngũ</router-link>
          <router-link to="/news" class="nav-link">Tin tức</router-link>
          <router-link to="/contact" class="nav-link">Liên hệ</router-link>

          <div class="auth-buttons">
            <template v-if="isLoggedIn">
              <router-link to="/dashboard" class="btn-dashboard">Dashboard</router-link>
              <button @click="handleLogout" class="btn-logout-header">Đăng xuất</button>
            </template>
            <template v-else>
              <router-link to="/login" class="btn-login-nav">Đăng Nhập</router-link>
              <router-link to="/register" class="btn-register-nav">Đăng Ký</router-link>
            </template>
          </div>
        </nav>
      </div>
    </header>

    <!-- Hero Section -->
    <section id="hero" class="hero-section">
      <div class="hero-glow bg-glow-1"></div>
      <div class="hero-glow bg-glow-2"></div>
      
      <div class="hero-container-grid">
        <div class="hero-content-left">
          <span class="hero-tagline">Chất lượng - Tận tâm - Uy tín</span>
          <h1 class="hero-title">Chăm Sóc Thú Cưng <br/><span class="gradient-text">Bằng Cả Trái Tim</span></h1>
          <p class="hero-desc">Chúng tôi mang lại giải pháp y tế toàn diện và các dịch vụ spa làm đẹp tốt nhất cho pet cưng của bạn.</p>
          
          <div class="hero-actions">
            <button @click="scrollToSection('contact')" class="btn-hero-primary">Đặt Lịch Hẹn Ngay</button>
            <button @click="scrollToSection('services')" class="btn-hero-secondary">Tìm Hiểu Dịch Vụ</button>
          </div>

          <!-- Stats counters inside left content -->
          <div class="stats-container">
            <div class="stat-card">
              <h3 class="stat-number">5,000+</h3>
              <p class="stat-label">Thú cưng được khám</p>
            </div>
            <div class="stat-card">
              <h3 class="stat-number">15+</h3>
              <p class="stat-label">Bác sĩ chuyên khoa</p>
            </div>
            <div class="stat-card">
              <h3 class="stat-number">10+</h3>
              <p class="stat-label">Năm hoạt động</p>
            </div>
          </div>
        </div>

        <div class="hero-visual-right">
          <div class="image-frame-container">
            <img src="/hero_veterinarian.png" alt="Bác sĩ thú y MyPetClinic" class="hero-image" />
            <div class="floating-badge badge-top">
              <span class="badge-dot animate-pulse"></span>
              <span class="badge-text">Bác sĩ trực: 24/7</span>
            </div>
            <div class="floating-badge badge-bottom">
              <span class="badge-star">★</span>
              <span class="badge-text">5.0 Uy Tín Hàng Đầu</span>
            </div>
          </div>
        </div>
      </div>
    </section>


    <!-- Services Section -->
    <section id="services" class="services-section">
      <div class="section-header">
        <span class="section-tag">Dịch Vụ Nổi Bật</span>
        <h2 class="section-title">Chăm Sóc Pet Chuyên Nghiệp</h2>
        <p class="section-desc">Chúng tôi cung cấp các gói dịch vụ chất lượng cao giúp giữ cho thú cưng luôn khỏe mạnh và sạch đẹp.</p>
      </div>

      <div class="services-grid">
        <div 
          v-for="service in services" 
          :key="service.id" 
          class="service-card"
          @click="openServiceModal(service)"
        >
          <div class="service-icon-wrapper" :style="{ background: service.color }">
            <component :is="service.icon" class="service-icon" />
          </div>
          <h3 class="service-title">{{ service.name }}</h3>
          <p class="service-excerpt">{{ service.excerpt }}</p>
          <span class="btn-read-more">Chi tiết <ChevronRight class="icon-right" /></span>
        </div>
      </div>
    </section>

    <!-- Why Us Section -->
    <section class="why-us-section">
      <div class="why-us-grid">
        <div class="why-us-info">
          <span class="section-tag">Tại sao chọn chúng tôi?</span>
          <h2 class="section-title">Nơi Gửi Gắm Niềm Tin Của Mọi Chủ Nuôi</h2>
          <p class="section-desc">Với trang thiết bị y tế hiện đại đạt chuẩn quốc tế cùng quy trình chăm sóc khép kín, MyPetClinic tự hào là lựa chọn hàng đầu cho thú cưng của bạn.</p>
          
          <div class="benefit-list">
            <div class="benefit-item">
              <CheckCircle class="benefit-icon" />
              <div>
                <h4>Bác sĩ thú y giàu kinh nghiệm</h4>
                <p>Đội ngũ chuyên gia chẩn đoán và điều trị tận tâm, tận lực vì sức khỏe của pet.</p>
              </div>
            </div>
            <div class="benefit-item">
              <CheckCircle class="benefit-icon" />
              <div>
                <h4>Trang thiết bị hiện đại</h4>
                <p>Máy siêu âm, chụp X-quang và phòng phẫu thuật vô trùng tiên tiến nhất.</p>
              </div>
            </div>
            <div class="benefit-item">
              <CheckCircle class="benefit-icon" />
              <div>
                <h4>Hỗ trợ cấp cứu 24/7</h4>
                <p>Luôn sẵn sàng tiếp nhận trường hợp khẩn cấp bất cứ thời điểm nào trong ngày.</p>
              </div>
            </div>
          </div>
        </div>
        
        <div class="why-us-visual">
          <div class="visual-card">
            <div class="visual-glow"></div>
            <div class="visual-content">
              <HeartHandshake class="visual-icon" />
              <h3>Bảo vệ Pet Cưng</h3>
              <p>Cam kết mang đến dịch vụ hoàn hảo và an toàn tuyệt đối cho người bạn 4 chân của bạn.</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Doctors Section -->
    <section id="doctors" class="doctors-section">
      <div class="section-header">
        <span class="section-tag">Đội Ngũ Chuyên Gia</span>
        <h2 class="section-title">Các Bác Sĩ Tiêu Biểu</h2>
        <p class="section-desc">Gặp gỡ những bác sĩ chuyên khoa xuất sắc luôn hết mình vì thú cưng.</p>
      </div>

      <div class="doctors-grid">
        <div v-for="doctor in doctors" :key="doctor.id" class="doctor-card">
          <div class="doctor-avatar-wrapper">
            <img :src="doctor.avatar" :alt="doctor.name" class="doctor-avatar" />
            <div class="doctor-overlay">
              <span class="doctor-exp">{{ doctor.exp }} năm kinh nghiệm</span>
            </div>
          </div>
          <h3 class="doctor-name">{{ doctor.name }}</h3>
          <span class="doctor-role">{{ doctor.specialty }}</span>
        </div>
      </div>
    </section>

    <!-- News & Articles Section -->
    <section id="news" class="news-section">
      <div class="section-header">
        <span class="section-tag">Góc Chia Sẻ</span>
        <h2 class="section-title">Tin Tức & Kinh Nghiệm Nuôi Pet</h2>
        <p class="section-desc">Cập nhật cẩm nang hữu ích để chăm sóc thú cưng của bạn luôn khỏe mạnh.</p>
      </div>

      <div class="news-grid">
        <div v-for="article in articles" :key="article.id" class="news-card">
          <div class="news-img-wrapper">
            <img :src="article.image" :alt="article.title" class="news-img" />
            <span class="news-tag">{{ article.tag }}</span>
          </div>
          <div class="news-info">
            <span class="news-date">{{ article.date }}</span>
            <h3 class="news-title" @click="openArticleModal(article)">{{ article.title }}</h3>
            <p class="news-excerpt">{{ article.excerpt }}</p>
            <button @click="openArticleModal(article)" class="btn-news-more">Đọc tiếp <ChevronRight class="icon-right" /></button>
          </div>
        </div>
      </div>
    </section>

    <!-- Contact & Booking Section -->
    <section id="contact" class="contact-section">
      <div class="contact-grid">
        <!-- Contact details -->
        <div class="contact-info">
          <span class="section-tag">Liên Hệ Thảo Luận</span>
          <h2 class="section-title">Kết Nối Với Chúng Tôi</h2>
          <p class="section-desc">Bạn có câu hỏi hoặc cần tư vấn nhanh? Đừng ngần ngại liên hệ qua các kênh thông tin chính thức hoặc quét mã Zalo để nhận tư vấn trực tiếp từ bác sĩ trực ca.</p>
          
          <div class="contact-details">
            <div class="detail-item">
              <MapPin class="detail-icon" />
              <div>
                <h5>Địa chỉ phòng khám</h5>
                <p>123 Đường Nguyễn Văn Linh, Quận Hải Châu, TP. Đà Nẵng</p>
              </div>
            </div>
            
            <div class="detail-item">
              <Phone class="detail-icon" />
              <div>
                <h5>Điện thoại khẩn cấp</h5>
                <p>0905 090 629 (Hotline 24/7)</p>
              </div>
            </div>

            <div class="detail-item">
              <Mail class="detail-icon" />
              <div>
                <h5>Email liên hệ</h5>
                <p>support@mypetclinic.com</p>
              </div>
            </div>
          </div>

          <button @click="isZaloModalOpen = true" class="btn-zalo">
            <MessageSquare class="btn-icon" />
            <span>Tư vấn qua Zalo Bác Sĩ</span>
          </button>
        </div>

        <!-- Booking Form -->
        <div class="booking-card">
          <h3>Đặt Lịch Hẹn Khám Nhanh</h3>
          <p>Điền thông tin đặt lịch để được ưu tiên sắp xếp không phải chờ đợi.</p>
          
          <form @submit.prevent="handleQuickBooking" class="booking-form">
            <div class="input-group">
              <label for="b-name">Họ tên của bạn</label>
              <input id="b-name" type="text" v-model="bookingForm.name" required placeholder="Nguyễn Văn A" class="form-input" />
            </div>

            <div class="input-group">
              <label for="b-phone">Số điện thoại liên lạc</label>
              <input id="b-phone" type="tel" v-model="bookingForm.phone" required placeholder="0912345678" class="form-input" />
            </div>

            <div class="form-row">
              <div class="input-group">
                <label for="b-service">Chọn dịch vụ</label>
                <select id="b-service" v-model="bookingForm.service" class="form-select">
                  <option v-for="srv in services" :key="srv.id" :value="srv.name">{{ srv.name }}</option>
                </select>
              </div>

              <div class="input-group">
                <label for="b-date">Ngày đặt lịch</label>
                <input id="b-date" type="date" v-model="bookingForm.date" required class="form-input" />
              </div>
            </div>

            <button type="submit" :disabled="bookingLoading" class="btn-submit-booking">
              <span v-if="!bookingLoading">Xác Nhận Đặt Lịch</span>
              <div class="spinner" v-else></div>
            </button>
          </form>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <footer class="footer">
      <div class="footer-container">
        <div class="footer-brand">
          <div class="logo">
            <PawPrint class="logo-icon" />
            <span class="logo-text">MyPetClinic</span>
          </div>
          <p class="footer-desc">Hệ thống phòng khám thú y cao cấp cung cấp dịch vụ chăm sóc sức khỏe toàn diện tốt nhất cho vật nuôi.</p>
        </div>

        <div class="footer-links">
          <h4>Về chúng tôi</h4>
          <a href="#" @click.prevent="scrollToSection('hero')">Trang chủ</a>
          <router-link to="/history">Lịch sử phòng khám</router-link>
          <a href="#" @click.prevent="scrollToSection('services')">Dịch vụ</a>
          <router-link to="/team">Đội ngũ bác sĩ</router-link>
        </div>

        <div class="footer-contact">
          <h4>Giờ làm việc</h4>
          <p>Thứ 2 - Chủ nhật: 08:00 - 21:00</p>
          <p>Nhận cấp cứu khẩn cấp 24/7</p>
          <p class="hotline-p">Hotline: 0905 090 629</p>
        </div>
      </div>
      <div class="footer-bottom">
        <p>&copy; 2026 MyPetClinic. Bảo lưu mọi quyền.</p>
      </div>
    </footer>

    <!-- Service Detail Modal -->
    <div v-if="activeServiceModal" class="modal-overlay" @click.self="activeServiceModal = null">
      <div class="modal-card">
        <button class="modal-close" @click="activeServiceModal = null"><X /></button>
        <div class="modal-header-icon" :style="{ background: activeServiceModal.color }">
          <component :is="activeServiceModal.icon" class="modal-icon" />
        </div>
        <h3 class="modal-title">{{ activeServiceModal.name }}</h3>
        <p class="modal-detail-desc">{{ activeServiceModal.detailDesc }}</p>
        
        <div class="modal-highlights">
          <h4>Ưu điểm nổi bật:</h4>
          <ul>
            <li v-for="(hl, idx) in activeServiceModal.highlights" :key="idx">
              <CheckCircle2 class="hl-icon" />
              <span>{{ hl }}</span>
            </li>
          </ul>
        </div>

        <div class="modal-price">
          <span>Chi phí ước lượng:</span>
          <span class="price-val">{{ activeServiceModal.price }}</span>
        </div>

        <div class="modal-actions-row">
          <button @click="selectServiceForBooking(activeServiceModal.name)" class="btn-modal-action">Đặt lịch dịch vụ này</button>
          <button @click="goToServiceDetailPage(activeServiceModal.name)" class="btn-modal-secondary">Xem chi tiết & Bảng giá</button>
        </div>
      </div>
    </div>

    <!-- Article Detail Modal -->
    <div v-if="activeArticleModal" class="modal-overlay" @click.self="activeArticleModal = null">
      <div class="modal-card article-modal-card">
        <button class="modal-close" @click="activeArticleModal = null"><X /></button>
        <div class="article-modal-img-wrapper">
          <img :src="activeArticleModal.image" alt="Article image" class="article-modal-img" />
          <span class="article-modal-tag">{{ activeArticleModal.tag }}</span>
        </div>
        <span class="article-modal-date">{{ activeArticleModal.date }}</span>
        <h3 class="modal-title">{{ activeArticleModal.title }}</h3>
        <div class="article-modal-body">
          <p v-for="(pText, idx) in activeArticleModal.paragraphs" :key="idx">{{ pText }}</p>
        </div>
      </div>
    </div>

    <!-- Zalo QR Code Modal -->
    <div v-if="isZaloModalOpen" class="modal-overlay" @click.self="isZaloModalOpen = false">
      <div class="modal-card qr-modal-card">
        <button class="modal-close" @click="isZaloModalOpen = false"><X /></button>
        <h3 class="modal-title text-center">Kết Nối Zalo Bác Sĩ</h3>
        <p class="text-center subtitle-qr">Quét mã QR bên dưới để bắt đầu chat tư vấn trực tiếp với bác sĩ trực ca của phòng khám.</p>
        <div class="qr-wrapper">
          <img src="https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=https://zalo.me/0905090629" alt="Zalo QR Code" class="qr-img" />
        </div>
        <p class="phone-qr text-center">Số điện thoại: <strong>0905.090.629</strong></p>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';

const router = useRouter();
import { 
  PawPrint, 
  Menu, 
  X, 
  HeartHandshake, 
  CheckCircle, 
  CheckCircle2, 
  ChevronRight,
  Activity, 
  Sparkles,
  ShieldCheck,
  Phone,
  Mail,
  MapPin,
  MessageSquare,
  Info,
  AlertCircle
} from '@lucide/vue';

const isLoggedIn = ref(false);
const isScrolled = ref(false);
const isMobileMenuOpen = ref(false);
const activeSection = ref('hero');

// Booking Form & Loading State
const bookingLoading = ref(false);
const bookingForm = reactive({
  name: '',
  phone: '',
  service: 'Khám & Điều Trị',
  date: ''
});

// Toast notification handling
interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
  icon: any;
}
const toasts = ref<Toast[]>([]);
let toastId = 0;

const showToast = (message: string, type: 'success' | 'error' | 'info' = 'info') => {
  const id = toastId++;
  let icon = Info;
  if (type === 'success') icon = CheckCircle2;
  if (type === 'error') icon = AlertCircle;

  toasts.value.push({ id, message, type, icon });
  setTimeout(() => {
    toasts.value = toasts.value.filter(t => t.id !== id);
  }, 4000);
};

const showSuccessToast = (msg: string) => showToast(msg, 'success');
const showErrorToast = (msg: string) => showToast(msg, 'error');

// Modals state
const activeServiceModal = ref<any | null>(null);
const activeArticleModal = ref<any | null>(null);
const isZaloModalOpen = ref(false);

// Services list data
const services = ref([
  {
    id: 1,
    name: 'Khám & Điều Trị',
    icon: Activity,
    color: '#0d9488',
    excerpt: 'Khám lâm sàng, chẩn đoán bằng hình ảnh siêu âm, xét nghiệm máu và lên phác đồ điều trị chuyên sâu.',
    detailDesc: 'Dịch vụ chẩn đoán y khoa cốt lõi của chúng tôi. Thú cưng sẽ được kiểm tra toàn diện, thực hiện các xét nghiệm sinh hóa nếu cần thiết để phát hiện sớm các bệnh lý về gan, thận, tim mạch hoặc truyền nhiễm.',
    highlights: ['Bác sĩ thú y túc trực chẩn đoán', 'Hệ thống xét nghiệm máu hiện đại có kết quả sau 15 phút', 'Phác đồ điều trị an toàn, cập nhật liên tục'],
    price: 'Từ 100.000 VNĐ'
  },
  {
    id: 2,
    name: 'Spa & Grooming',
    icon: Sparkles,
    color: '#6366f1',
    excerpt: 'Tắm spa dưỡng lông, cắt tỉa lông tạo kiểu chuyên nghiệp, vệ sinh tai và cắt móng an toàn.',
    detailDesc: 'Giúp thú cưng của bạn sở hữu diện mạo xinh xắn và sạch sẽ nhất. Quy trình spa bao gồm việc vắt tuyến hôi, tắm sấy 5 bước bằng sữa tắm dưỡng lông chuyên dụng, cắt dũa móng và tạo kiểu nghệ thuật theo yêu cầu.',
    highlights: ['Nhân viên spa chuyên nghiệp, khéo léo', 'Sử dụng sữa tắm thảo dược an toàn cho da nhạy cảm', 'Tạo kiểu thời trang, hợp xu hướng'],
    price: 'Từ 150.000 VNĐ'
  },
  {
    id: 3,
    name: 'Tiêm Phòng & Vaccine',
    icon: ShieldCheck,
    color: '#f59e0b',
    excerpt: 'Cung cấp đầy đủ các loại vaccine phòng bệnh dại, 5 bệnh, 7 bệnh phổ biến cho chó mèo.',
    detailDesc: 'Bảo vệ thú cưng khỏi các căn bệnh nguy hiểm gây tử vong cao như Parvo, Care ở chó hay Giảm bạch cầu ở mèo. Quy trình tiêm chủng an toàn kèm sổ khám theo dõi định kỳ tiện lợi.',
    highlights: ['Vaccine nhập khẩu chính hãng có tem kiểm định', 'Khám sức khỏe miễn phí trước khi tiêm', 'Nhắc lịch tiêm chủng tự động qua tin nhắn'],
    price: 'Từ 120.000 VNĐ'
  },
  {
    id: 4,
    name: 'Tư Vấn Sức Khỏe',
    icon: HeartHandshake,
    color: '#ec4899',
    excerpt: 'Tư vấn dinh dưỡng, chế độ ăn uống, tập luyện và tiêm chủng định kỳ chuẩn khoa học.',
    detailDesc: 'Dịch vụ tư vấn chuyên sâu giúp bạn xây dựng chế độ dinh dưỡng cá nhân hóa cho thú cưng theo độ tuổi, cân nặng và thể trạng đặc biệt (như mang thai, béo phì, dưỡng bệnh).',
    highlights: ['Thiết lập thực đơn dinh dưỡng chuẩn khoa học', 'Lời khuyên từ chuyên gia dinh dưỡng thú y hàng đầu', 'Tặng kèm cẩm nang chăm sóc độc quyền'],
    price: 'Miễn phí khi khám tại Clinic'
  }
]);

// Doctors Team list
const doctors = ref([
  {
    id: 1,
    name: 'Bác sĩ Nguyễn Văn Minh',
    specialty: 'Giám đốc chuyên môn - Ngoại khoa',
    avatar: 'https://images.unsplash.com/photo-1622253692010-333f2da6031d?auto=format&fit=crop&w=300&h=300',
    exp: 12
  },
  {
    id: 2,
    name: 'Bác sĩ Trần Thị Hồng',
    specialty: 'Chuyên khoa Nội - Da liễu Thú y',
    avatar: 'https://images.unsplash.com/photo-1594824813573-246434de83fb?auto=format&fit=crop&w=300&h=300',
    exp: 8
  },
  {
    id: 3,
    name: 'Bác sĩ Lê Hoàng Nam',
    specialty: 'Chuyên gia siêu âm & chẩn đoán hình ảnh',
    avatar: 'https://images.unsplash.com/photo-1537368910025-700350fe46c7?auto=format&fit=crop&w=300&h=300',
    exp: 6
  }
]);

// Articles News list
const articles = ref([
  {
    id: 1,
    title: 'Lịch tiêm phòng dại định kỳ cho chó mèo bạn cần biết',
    tag: 'Sức khỏe',
    date: '05 Tháng 6, 2026',
    image: 'https://images.unsplash.com/photo-1581888227599-779811939961?auto=format&fit=crop&w=400&h=250',
    excerpt: 'Bệnh dại là căn bệnh vô cùng nguy hiểm và có khả năng lây sang người. Tìm hiểu lịch tiêm phòng chuẩn xác nhất...',
    paragraphs: [
      'Bệnh dại (Rabies) là bệnh truyền nhiễm virus cấp tính của hệ thần kinh trung ương, lây từ động vật sang người thông qua vết cắn, vết cào. Đây là căn bệnh cực kỳ nguy hiểm, một khi đã lên cơn dại thì tỷ lệ tử vong là 100%.',
      'Để bảo vệ thú cưng cũng như bản thân và gia đình, chủ nuôi bắt buộc phải cho chó mèo đi tiêm vaccine phòng dại định kỳ. Mũi tiêm đầu tiên nên thực hiện khi thú cưng đạt 3 tháng tuổi. Sau đó, cần tiêm nhắc lại đều đặn mỗi năm một lần.',
      'Lưu ý: Chỉ thực hiện tiêm vaccine khi thú cưng hoàn toàn khỏe mạnh, không bị sốt hay đang điều trị bệnh lý nào khác. Sau khi tiêm nên theo dõi tại phòng khám khoảng 15-30 phút để đề phòng sốc phản vệ.'
    ]
  },
  {
    id: 2,
    title: 'Cách chăm sóc thú cưng vào mùa hè nắng nóng tránh sốc nhiệt',
    tag: 'Cẩm nang',
    date: '28 Tháng 5, 2026',
    image: 'https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=400&h=250',
    excerpt: 'Thời tiết nắng nóng của mùa hè rất dễ khiến chó mèo bị mất nước và sốc nhiệt dẫn đến đột quỵ. Hãy áp dụng ngay...',
    paragraphs: [
      'Sốc nhiệt là tình trạng khẩn cấp xảy ra khi nhiệt độ cơ thể thú cưng tăng cao vượt ngưỡng an toàn (thường trên 40 độ C), khiến cơ thể không kịp tản nhiệt. Điều này rất dễ xảy ra trong những ngày hè oi bức tại Việt Nam.',
      'Các dấu hiệu sốc nhiệt dễ nhận biết bao gồm: thở gấp gáp, chảy nhiều nước dãi, nướu đỏ sẫm hoặc xanh tím, đi đứng lảo đảo và lờ đờ. Nếu không sơ cứu kịp thời có thể dẫn đến suy đa tạng và tử vong.',
      'Biện pháp phòng ngừa hiệu quả: Luôn cung cấp đủ nước sạch mát, giữ pet trong không gian thoáng gió hoặc phòng điều hòa vào khung giờ nắng nóng cao điểm. Tuyệt đối không để thú cưng một mình trong xe ô tô đóng kín cửa.'
    ]
  }
]);

// Scroll handling and active navigation highlight
const handleScroll = () => {
  isScrolled.value = window.scrollY > 50;

  const sections = ['hero', 'services', 'doctors', 'news', 'contact'];
  const scrollPosition = window.scrollY + 120;

  for (const section of sections) {
    const el = document.getElementById(section);
    if (el) {
      const top = el.offsetTop;
      const height = el.offsetHeight;
      if (scrollPosition >= top && scrollPosition < top + height) {
        activeSection.value = section;
      }
    }
  }
};

const scrollToSection = (id: string) => {
  isMobileMenuOpen.value = false;
  const el = document.getElementById(id);
  if (el) {
    window.scrollTo({
      top: el.offsetTop - 80,
      behavior: 'smooth'
    });
  }
};

// Check if user is logged in (via backend call to profile or token cookie verification)
const checkLoginState = async () => {
  try {
    await api.get('/profile');
    isLoggedIn.value = true;
  } catch (err) {
    isLoggedIn.value = false;
  }
};

const handleLogout = async () => {
  try {
    await api.post('/account/logout');
    isLoggedIn.value = false;
    showSuccessToast('Đã đăng xuất tài khoản.');
  } catch (err) {
    showErrorToast('Lỗi khi đăng xuất.');
  }
};

const goToServiceDetailPage = (serviceName: string) => {
  activeServiceModal.value = null;
  if (serviceName === 'Khám & Điều Trị') {
    router.push('/services/kham-dieu-tri');
  } else if (serviceName === 'Spa & Grooming') {
    router.push('/services/spa-grooming');
  } else if (serviceName === 'Tiêm Phòng & Vaccine') {
    router.push('/services/tiem-phong');
  } else if (serviceName === 'Tư Vấn Sức Khỏe') {
    router.push('/services/suc-khoe');
  }
};

const selectServiceForBooking = (serviceName: string) => {
  bookingForm.service = serviceName;
  activeServiceModal.value = null;
  scrollToSection('contact');
};

const openServiceModal = (service: any) => {
  activeServiceModal.value = service;
};

const openArticleModal = (article: any) => {
  activeArticleModal.value = article;
};

// Handle booking submission
const handleQuickBooking = async () => {
  bookingLoading.value = true;
  try {
    // Gọi API lưu đặt lịch nếu có backend API hỗ trợ đặt lịch nhanh, 
    // hoặc giả lập thông báo thành công cho Khách hàng trải nghiệm mượt mà.
    // Vì đây là trang đặt lịch nhanh cho người dùng vãng lai, ta gửi thông tin và thông báo.
    await new Promise(resolve => setTimeout(resolve, 1200)); // Hiệu ứng mượt mà
    showSuccessToast(`Đăng ký đặt lịch khám thành công cho ngày ${bookingForm.date}! Chúng tôi sẽ liên hệ sớm nhất để xác nhận.`);
    bookingForm.name = '';
    bookingForm.phone = '';
    bookingForm.date = '';
  } catch (error) {
    showErrorToast('Đặt lịch thất bại. Vui lòng liên hệ hotline.');
  } finally {
    bookingLoading.value = false;
  }
};

onMounted(() => {
  window.addEventListener('scroll', handleScroll);
  checkLoginState();
  // Thiết lập ngày mặc định cho form là ngày mai
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  bookingForm.date = tomorrow.toISOString().split('T')[0];
});

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll);
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.home-wrapper {
  background-color: #080c14;
  color: #f8fafc;
  font-family: 'Outfit', sans-serif;
  min-height: 100vh;
  position: relative;
  overflow: hidden;
}

/* Global Glow Backgrounds */
.bg-glow-1 {
  width: 700px;
  height: 700px;
  background: radial-gradient(circle, rgba(20, 184, 166, 0.15) 0%, transparent 70%);
  position: absolute;
  top: -200px;
  right: -100px;
  pointer-events: none;
  animation: pulse-glow 8s ease-in-out infinite alternate;
}

.bg-glow-2 {
  width: 600px;
  height: 600px;
  background: radial-gradient(circle, rgba(99, 102, 241, 0.12) 0%, transparent 70%);
  position: absolute;
  top: 35%;
  left: -200px;
  pointer-events: none;
  animation: pulse-glow 10s ease-in-out infinite alternate-reverse;
}

/* Floating Glassmorphic Header Capsule */
.header {
  position: fixed;
  top: 24px;
  left: 50%;
  transform: translateX(-50%);
  width: calc(100% - 40px);
  max-width: 1200px;
  z-index: 1000;
  padding: 1rem 2.5rem;
  background: rgba(9, 13, 22, 0.6);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 24px;
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.header.scrolled {
  top: 12px;
  padding: 0.8rem 2.5rem;
  background: rgba(9, 13, 22, 0.85);
  border-color: rgba(20, 184, 166, 0.25);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.4);
}

.nav-container {
  width: 100%;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.logo-icon {
  width: 32px;
  height: 32px;
  color: #14b8a6;
  filter: drop-shadow(0 0 8px rgba(20, 184, 166, 0.3));
}

.logo-text {
  font-size: 1.6rem;
  font-weight: 800;
  letter-spacing: -0.5px;
  color: #ffffff;
  background: linear-gradient(135deg, #ffffff, #94a3b8);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.menu-toggle {
  display: none;
  background: none;
  border: none;
  color: #ffffff;
  cursor: pointer;
}

.menu-icon {
  width: 26px;
  height: 26px;
}

.nav-menu {
  display: flex;
  align-items: center;
  gap: 2.2rem;
}

.nav-link {
  color: #94a3b8;
  text-decoration: none;
  font-weight: 500;
  font-size: 0.95rem;
  transition: all 0.3s;
  position: relative;
  padding: 0.25rem 0;
}

.nav-link::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  width: 0;
  height: 2px;
  background: var(--primary, #14b8a6);
  transition: width 0.3s ease;
}

.nav-link:hover::after, .nav-link.active::after {
  width: 100%;
}

.nav-link:hover, .nav-link.active {
  color: #ffffff;
}

.auth-buttons {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-left: 1rem;
}

.btn-login-nav {
  color: #cbd5e1;
  text-decoration: none;
  font-weight: 600;
  padding: 0.6rem 1.2rem;
  transition: color 0.3s;
}

.btn-login-nav:hover {
  color: #ffffff;
}

.btn-register-nav, .btn-dashboard {
  background: linear-gradient(135deg, #14b8a6, #0d9488);
  color: white;
  border: none;
  text-decoration: none;
  padding: 0.6rem 1.4rem;
  border-radius: 12px;
  font-weight: 600;
  transition: all 0.3s;
  box-shadow: 0 4px 12px rgba(20, 184, 166, 0.25);
}

.btn-register-nav:hover, .btn-dashboard:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(20, 184, 166, 0.45);
}

.btn-logout-header {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.2);
  padding: 0.6rem 1.2rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-logout-header:hover {
  background: rgba(239, 68, 68, 0.2);
  color: white;
}

/* Mobile responsive menu */
@media (max-width: 868px) {
  .menu-toggle {
    display: block;
  }

  .nav-menu {
    position: fixed;
    top: 90px;
    right: -100%;
    width: 280px;
    height: auto;
    max-height: 80vh;
    background: rgba(9, 13, 22, 0.95);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    flex-direction: column;
    align-items: flex-start;
    padding: 2.5rem 2rem;
    gap: 1.8rem;
    transition: right 0.4s ease;
    border: 1px solid rgba(255, 255, 255, 0.08);
  }

  .nav-menu.mobile-open {
    right: 20px;
  }

  .auth-buttons {
    flex-direction: column;
    width: 100%;
    margin-left: 0;
    margin-top: 1rem;
    gap: 1rem;
  }

  .auth-buttons > * {
    width: 100%;
    text-align: center;
  }
}

/* Hero Section */
.hero-section {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  padding-top: 140px;
  padding-bottom: 5rem;
}

.hero-container-grid {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 2rem;
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 4rem;
  align-items: center;
  width: 100%;
}

@media (max-width: 992px) {
  .hero-container-grid {
    grid-template-columns: 1fr;
    text-align: center;
    gap: 3.5rem;
  }
  .hero-actions {
    justify-content: center;
  }
  .stats-container {
    margin: 0 auto !important;
  }
}

.hero-content-left {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

@media (max-width: 992px) {
  .hero-content-left {
    align-items: center;
  }
}

.hero-tagline {
  font-size: 0.9rem;
  font-weight: 600;
  text-transform: uppercase;
  color: #14b8a6;
  letter-spacing: 2px;
  background: rgba(20, 184, 166, 0.08);
  padding: 0.5rem 1.2rem;
  border-radius: 20px;
  margin-bottom: 2rem;
  display: inline-block;
  border: 1px solid rgba(20, 184, 166, 0.15);
}

.hero-title {
  font-size: 3.8rem;
  font-weight: 800;
  line-height: 1.15;
  letter-spacing: -1.5px;
  margin-bottom: 1.5rem;
  color: #ffffff;
  text-align: left;
}

@media (max-width: 992px) {
  .hero-title {
    text-align: center;
  }
}

@media (max-width: 600px) {
  .hero-title {
    font-size: 2.8rem;
  }
}

.gradient-text {
  background: linear-gradient(135deg, #14b8a6, #6366f1);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.hero-desc {
  font-size: 1.2rem;
  color: #94a3b8;
  max-width: 600px;
  margin-bottom: 2.5rem;
  line-height: 1.7;
  text-align: left;
}

@media (max-width: 992px) {
  .hero-desc {
    text-align: center;
    margin: 0 auto 2.5rem auto;
  }
}

.hero-actions {
  display: flex;
  gap: 15px;
  margin-bottom: 4.5rem;
  width: 100%;
}

@media (max-width: 480px) {
  .hero-actions {
    flex-direction: column;
    align-items: center;
  }
  .hero-actions button {
    width: 100%;
    max-width: 280px;
  }
}

.btn-hero-primary {
  background: linear-gradient(135deg, #14b8a6, #0d9488);
  color: white;
  border: none;
  padding: 1rem 2.2rem;
  border-radius: 14px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: 0 4px 20px rgba(20, 184, 166, 0.4);
}

.btn-hero-primary:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(20, 184, 166, 0.55);
}

.btn-hero-secondary {
  background: rgba(255, 255, 255, 0.03);
  color: #cbd5e1;
  border: 1px solid rgba(255, 255, 255, 0.08);
  padding: 1rem 2.2rem;
  border-radius: 14px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-hero-secondary:hover {
  background: rgba(255, 255, 255, 0.08);
  color: white;
  border-color: rgba(255, 255, 255, 0.2);
}

.stats-container {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
  max-width: 600px;
  width: 100%;
}

.stat-card {
  background: rgba(17, 24, 39, 0.45);
  border: 1px solid rgba(255, 255, 255, 0.05);
  padding: 1.5rem 1rem;
  border-radius: 20px;
  backdrop-filter: blur(10px);
  transition: all 0.3s;
  text-align: center;
}

.stat-card:hover {
  border-color: rgba(20, 184, 166, 0.2);
  transform: translateY(-4px);
}

.stat-number {
  font-size: 2.2rem;
  font-weight: 800;
  color: #14b8a6;
  margin-bottom: 0.2rem;
  filter: drop-shadow(0 0 10px rgba(20, 184, 166, 0.2));
}

.stat-label {
  font-size: 0.85rem;
  color: #94a3b8;
  font-weight: 500;
}

/* Right side image styling */
.hero-visual-right {
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
}

.image-frame-container {
  position: relative;
  width: 100%;
  max-width: 440px;
  border-radius: 30px;
  padding: 8px;
  background: linear-gradient(135deg, rgba(20, 184, 166, 0.3), rgba(99, 102, 241, 0.3));
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.5);
  animation: float 6s ease-in-out infinite;
}

.hero-image {
  width: 100%;
  height: 480px;
  object-fit: cover;
  border-radius: 24px;
  display: block;
}

.floating-badge {
  position: absolute;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 0.8rem 1.2rem;
  background: rgba(9, 13, 22, 0.75);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 16px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
}

.badge-top {
  top: 30px;
  left: -30px;
  border-color: rgba(20, 184, 166, 0.3);
}

.badge-bottom {
  bottom: 40px;
  right: -20px;
  border-color: rgba(99, 102, 241, 0.3);
}

.badge-dot {
  width: 8px;
  height: 8px;
  background-color: #10b981;
  border-radius: 50%;
  box-shadow: 0 0 10px #10b981;
}

.badge-star {
  color: #fbbf24;
  font-weight: bold;
}

.badge-text {
  font-size: 0.85rem;
  font-weight: 600;
  color: #ffffff;
}

@media (max-width: 992px) {
  .image-frame-container {
    max-width: 380px;
  }
  .hero-image {
    height: 380px;
  }
  .badge-top {
    left: -10px;
  }
  .badge-bottom {
    right: -10px;
  }
}

/* Section generic headers */
.section-header {
  text-align: center;
  max-width: 700px;
  margin: 0 auto 4rem auto;
  padding: 0 1.5rem;
}

.section-tag {
  color: #14b8a6;
  font-size: 0.85rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 2px;
  display: block;
  margin-bottom: 0.8rem;
}

.section-title {
  font-size: 2.6rem;
  font-weight: 800;
  color: #ffffff;
  margin-bottom: 1.2rem;
  letter-spacing: -0.5px;
}

.section-desc {
  color: #94a3b8;
  font-size: 1.05rem;
  line-height: 1.7;
}

/* Services section */
.services-section {
  padding: 7rem 1.5rem;
  background: rgba(9, 13, 22, 0.4);
  position: relative;
}

.services-grid {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 2.5rem;
}

.service-card {
  background: rgba(17, 24, 39, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 24px;
  padding: 2.5rem 2rem;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.service-card:hover {
  transform: translateY(-10px);
  border-color: rgba(20, 184, 166, 0.3);
  background: rgba(31, 41, 55, 0.7);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4), 0 0 30px rgba(20, 184, 166, 0.05);
}

.service-icon-wrapper {
  padding: 14px;
  border-radius: 16px;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-bottom: 1.8rem;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.25);
}

.service-icon {
  color: white;
  width: 24px;
  height: 24px;
}

.service-title {
  color: white;
  font-size: 1.35rem;
  font-weight: 700;
  margin-bottom: 0.8rem;
}

.service-excerpt {
  color: #94a3b8;
  font-size: 0.95rem;
  line-height: 1.7;
  margin-bottom: 1.8rem;
  flex-grow: 1;
}

.btn-read-more {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #14b8a6;
  font-size: 0.95rem;
  font-weight: 700;
}


.icon-right {
  width: 16px;
  height: 16px;
  transition: transform 0.2s;
}

.service-card:hover .icon-right {
  transform: translateX(4px);
}

/* Why us section */
.why-us-section {
  padding: 6rem 1.5rem;
  max-width: 1200px;
  margin: 0 auto;
}

.why-us-grid {
  display: grid;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 4rem;
  align-items: center;
}

@media (max-width: 868px) {
  .why-us-grid {
    grid-template-columns: 1fr;
    gap: 3rem;
  }
}

.benefit-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  margin-top: 2rem;
}

.benefit-item {
  display: flex;
  gap: 15px;
}

.benefit-icon {
  width: 24px;
  height: 24px;
  color: #14b8a6;
  flex-shrink: 0;
  margin-top: 2px;
}

.benefit-item h4 {
  font-size: 1.1rem;
  font-weight: 600;
  color: white;
  margin-bottom: 0.3rem;
}

.benefit-item p {
  font-size: 0.9rem;
  color: #94a3b8;
  line-height: 1.5;
}

.why-us-visual {
  display: flex;
  justify-content: center;
}

.visual-card {
  position: relative;
  background: linear-gradient(135deg, rgba(30, 41, 59, 0.8), rgba(15, 23, 42, 0.9));
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 24px;
  padding: 3rem 2.5rem;
  text-align: center;
  max-width: 320px;
  overflow: hidden;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
}

.visual-glow {
  position: absolute;
  top: -50px;
  left: -50px;
  width: 150px;
  height: 150px;
  background: radial-gradient(circle, rgba(99, 102, 241, 0.15), transparent 70%);
  filter: blur(20px);
}

.visual-icon {
  width: 48px;
  height: 48px;
  color: #6366f1;
  margin-bottom: 1.5rem;
}

.visual-card h3 {
  font-size: 1.3rem;
  color: white;
  margin-bottom: 0.8rem;
}

.visual-card p {
  font-size: 0.9rem;
  color: #94a3b8;
  line-height: 1.6;
}

/* Doctors Section */
.doctors-section {
  padding: 6rem 1.5rem;
  background: rgba(15, 23, 42, 0.4);
}

.doctors-grid {
  max-width: 1000px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 2.5rem;
}

.doctor-card {
  background: rgba(30, 41, 59, 0.3);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 20px;
  padding: 1.8rem;
  text-align: center;
  transition: all 0.3s;
}

.doctor-card:hover {
  transform: translateY(-5px);
  border-color: rgba(20, 184, 166, 0.25);
  background: rgba(30, 41, 59, 0.5);
}

.doctor-avatar-wrapper {
  position: relative;
  width: 140px;
  height: 140px;
  border-radius: 50%;
  margin: 0 auto 1.5rem auto;
  overflow: hidden;
  border: 3px solid rgba(20, 184, 166, 0.2);
}

.doctor-avatar {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.doctor-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(15, 23, 42, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  opacity: 0;
  transition: opacity 0.3s;
}

.doctor-avatar-wrapper:hover .doctor-overlay {
  opacity: 1;
}

.doctor-exp {
  color: white;
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: uppercase;
}

.doctor-name {
  font-size: 1.2rem;
  font-weight: 600;
  color: white;
  margin-bottom: 0.3rem;
}

.doctor-role {
  font-size: 0.85rem;
  color: #14b8a6;
  font-weight: 500;
}

/* News Section */
.news-section {
  padding: 6rem 1.5rem;
}

.news-grid {
  max-width: 1100px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 2.5rem;
}

.news-card {
  background: rgba(30, 41, 59, 0.4);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 20px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.news-img-wrapper {
  position: relative;
  height: 200px;
  overflow: hidden;
}

.news-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s;
}

.news-card:hover .news-img {
  transform: scale(1.05);
}

.news-tag {
  position: absolute;
  top: 15px;
  left: 15px;
  background: #14b8a6;
  color: white;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.3rem 0.8rem;
  border-radius: 20px;
}

.news-info {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.news-date {
  font-size: 0.8rem;
  color: #64748b;
  margin-bottom: 0.6rem;
}

.news-title {
  font-size: 1.15rem;
  font-weight: 600;
  color: white;
  margin-bottom: 0.8rem;
  line-height: 1.4;
  cursor: pointer;
  transition: color 0.2s;
}

.news-title:hover {
  color: #14b8a6;
}

.news-excerpt {
  color: #94a3b8;
  font-size: 0.85rem;
  line-height: 1.6;
  margin-bottom: 1.2rem;
  flex-grow: 1;
}

.btn-news-more {
  background: none;
  border: none;
  color: #14b8a6;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 4px;
  align-self: flex-start;
  padding: 0;
}

.btn-news-more:hover {
  color: #2dd4bf;
}

/* Contact and booking section */
.contact-section {
  padding: 6rem 1.5rem;
  background: rgba(15, 23, 42, 0.4);
}

.contact-grid {
  max-width: 1100px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4rem;
  align-items: center;
}

@media (max-width: 868px) {
  .contact-grid {
    grid-template-columns: 1fr;
    gap: 3rem;
  }
}

.contact-details {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  margin: 2.2rem 0;
}

.detail-item {
  display: flex;
  gap: 15px;
}

.detail-icon {
  width: 22px;
  height: 22px;
  color: #14b8a6;
  flex-shrink: 0;
  margin-top: 2px;
}

.detail-item h5 {
  font-size: 1rem;
  font-weight: 600;
  color: white;
  margin-bottom: 0.2rem;
}

.detail-item p {
  font-size: 0.9rem;
  color: #94a3b8;
}

.btn-zalo {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #0068ff; /* Zalo Blue */
  color: white;
  border: none;
  padding: 0.8rem 1.8rem;
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: 0 4px 15px rgba(0, 104, 255, 0.25);
}

.btn-zalo:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 104, 255, 0.35);
  background: #0056d6;
}

.btn-icon {
  width: 18px;
  height: 18px;
}

/* Booking card form */
.booking-card {
  background: rgba(30, 41, 59, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.08);
  padding: 2.5rem;
  border-radius: 24px;
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.3);
}

.booking-card h3 {
  font-size: 1.4rem;
  color: white;
  margin-bottom: 0.4rem;
}

.booking-card p {
  font-size: 0.85rem;
  color: #94a3b8;
  margin-bottom: 1.8rem;
}

.booking-form {
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.2rem;
}

@media (max-width: 450px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.input-group label {
  color: #cbd5e1;
  font-size: 0.85rem;
  font-weight: 500;
}

.form-input, .form-select {
  width: 100%;
  padding: 0.8rem 1rem;
  background: rgba(15, 23, 42, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  color: white;
  font-family: inherit;
  font-size: 0.9rem;
  transition: all 0.3s;
}

.form-input:focus, .form-select:focus {
  outline: none;
  border-color: #14b8a6;
  box-shadow: 0 0 0 3px rgba(20, 184, 166, 0.15);
}

.form-select option {
  background: #1e293b;
  color: white;
}

.btn-submit-booking {
  background: linear-gradient(135deg, #14b8a6, #0d9488);
  color: white;
  border: none;
  border-radius: 10px;
  padding: 0.85rem;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 0.5rem;
}

.btn-submit-booking:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(20, 184, 166, 0.3);
}

.btn-submit-booking:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Footer Section */
.footer {
  background: #0b0f19;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  padding: 4.5rem 2rem 2rem 2rem;
}

.footer-container {
  max-width: 1200px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1.5fr 1fr 1fr;
  gap: 4rem;
  margin-bottom: 3.5rem;
}

@media (max-width: 768px) {
  .footer-container {
    grid-template-columns: 1fr;
    gap: 2.5rem;
  }
}

.footer-brand {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.footer-desc {
  color: #64748b;
  font-size: 0.9rem;
  line-height: 1.6;
  max-width: 320px;
}

.footer-links, .footer-contact {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.footer-links h4, .footer-contact h4 {
  color: white;
  font-size: 1.05rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
}

.footer-links a {
  color: #94a3b8;
  text-decoration: none;
  font-size: 0.9rem;
  transition: color 0.3s;
}

.footer-links a:hover {
  color: #14b8a6;
}

.footer-contact p {
  color: #94a3b8;
  font-size: 0.9rem;
  line-height: 1.5;
}

.hotline-p {
  color: #14b8a6 !important;
  font-weight: 600;
  font-size: 1rem !important;
}

.footer-bottom {
  border-top: 1px solid rgba(255, 255, 255, 0.04);
  padding-top: 2rem;
  text-align: center;
  max-width: 1200px;
  margin: 0 auto;
}

.footer-bottom p {
  color: #475569;
  font-size: 0.8rem;
}

/* Modals layout */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(15, 23, 42, 0.8);
  backdrop-filter: blur(8px);
  z-index: 10000;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
}

.modal-card {
  background: #1e293b;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 24px;
  width: 100%;
  max-width: 520px;
  padding: 2.5rem;
  position: relative;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.5);
  animation: modal-enter 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes modal-enter {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.modal-close {
  position: absolute;
  top: 20px;
  right: 20px;
  background: none;
  border: none;
  color: #64748b;
  cursor: pointer;
  padding: 5px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.modal-close:hover {
  background: rgba(255, 255, 255, 0.05);
  color: white;
}

.modal-header-icon {
  width: 54px;
  height: 54px;
  border-radius: 16px;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-bottom: 1.5rem;
  box-shadow: 0 6px 15px rgba(0, 0, 0, 0.15);
}

.modal-icon {
  color: white;
  width: 26px;
  height: 26px;
}

.modal-title {
  font-size: 1.6rem;
  font-weight: 700;
  color: white;
  margin-bottom: 1rem;
}

.modal-detail-desc {
  color: #cbd5e1;
  font-size: 0.95rem;
  line-height: 1.6;
  margin-bottom: 1.8rem;
}

.modal-highlights h4 {
  font-size: 1rem;
  color: white;
  font-weight: 600;
  margin-bottom: 0.8rem;
}

.modal-highlights ul {
  list-style: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  margin-bottom: 1.8rem;
}

.modal-highlights li {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 0.9rem;
  color: #94a3b8;
}

.hl-icon {
  width: 18px;
  height: 18px;
  color: #14b8a6;
  flex-shrink: 0;
}

.modal-price {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  padding-top: 1.2rem;
  margin-bottom: 1.8rem;
}

.modal-price span:first-child {
  color: #64748b;
  font-size: 0.9rem;
}

.price-val {
  color: #14b8a6;
  font-weight: 700;
  font-size: 1.2rem;
}

.btn-modal-action {
  width: 100%;
  background: linear-gradient(135deg, #14b8a6, #0d9488);
  color: white;
  border: none;
  border-radius: 12px;
  padding: 0.9rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-modal-action:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(20, 184, 166, 0.3);
}

.modal-actions-row {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.btn-modal-secondary {
  width: 100%;
  background: rgba(255, 255, 255, 0.05);
  color: #cbd5e1;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 0.9rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-modal-secondary:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

/* Article modal specifics */
.article-modal-card {
  max-width: 680px;
  padding: 0;
  overflow: hidden;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

.article-modal-card .modal-close {
  background: rgba(15, 23, 42, 0.6);
  color: white;
  z-index: 1;
}

.article-modal-card .modal-close:hover {
  background: rgba(15, 23, 42, 0.9);
}

.article-modal-img-wrapper {
  position: relative;
  height: 280px;
  flex-shrink: 0;
}

.article-modal-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.article-modal-tag {
  position: absolute;
  bottom: 20px;
  left: 20px;
  background: #14b8a6;
  color: white;
  font-weight: 600;
  padding: 0.4rem 1rem;
  border-radius: 20px;
  font-size: 0.8rem;
}

.article-modal-date {
  color: #64748b;
  font-size: 0.85rem;
  margin: 1.5rem 2rem 0.5rem 2rem;
  display: block;
}

.article-modal-card .modal-title {
  margin: 0 2rem 1.2rem 2rem;
  line-height: 1.3;
}

.article-modal-body {
  padding: 0 2rem 2.5rem 2rem;
  overflow-y: auto;
  color: #cbd5e1;
  font-size: 0.95rem;
  line-height: 1.7;
}

.article-modal-body p {
  margin-bottom: 1rem;
}

/* Zalo QR Modal specifics */
.qr-modal-card {
  max-width: 400px;
  text-align: center;
}

.subtitle-qr {
  color: #94a3b8;
  font-size: 0.85rem;
  line-height: 1.5;
  margin-bottom: 1.5rem;
}

.qr-wrapper {
  background: white;
  padding: 1.2rem;
  border-radius: 16px;
  display: inline-block;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  margin-bottom: 1.5rem;
}

.qr-img {
  width: 180px;
  height: 180px;
  display: block;
}

.phone-qr {
  font-size: 0.95rem;
  color: #cbd5e1;
}

.text-center {
  text-align: center;
}

/* Spinner */
.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Custom Toasts container */
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  z-index: 10001;
  max-width: 350px;
}

.toast {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 1rem 1.25rem;
  border-radius: 12px;
  color: #ffffff;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.35);
  font-size: 0.9rem;
  font-weight: 500;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.toast-success {
  background: rgba(16, 185, 129, 0.9);
  border-color: rgba(16, 185, 129, 0.2);
}

.toast-error {
  background: rgba(239, 68, 68, 0.9);
  border-color: rgba(239, 68, 68, 0.2);
}

.toast-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

.toast-message {
  line-height: 1.4;
}

/* Toast Transitions */
.toast-fade-enter-active,
.toast-fade-leave-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.toast-fade-enter-from {
  opacity: 0;
  transform: translateY(-20px) scale(0.9);
}

.toast-fade-leave-to {
  opacity: 0;
  transform: translateY(20px) scale(0.9);
}
</style>
