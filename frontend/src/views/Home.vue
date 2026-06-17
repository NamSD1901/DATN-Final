<template>
  <div class="home-wrapper">
    <!-- Toast Notifications -->
    <TransitionGroup name="toast-fade" tag="div" class="toast-container">
      <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast-${toast.type}`]">
        <component :is="toast.icon" class="toast-icon" />
        <span class="toast-message">{{ toast.message }}</span>
      </div>
    </TransitionGroup>

    <!-- Shared Header Layout -->
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section with Floating Shapes -->
    <section class="hero-section bg-gold-gradient position-relative" id="hero">
      <div class="hero-shape-1"></div>
      <div class="hero-shape-2"></div>
      <div class="container">
        <div class="hero-row">
          <div class="hero-content-left reveal-left active">
            <span class="hero-tag animate-pulse">
              <Heart class="icon-heart-fill" /> Bệnh Viện Thú Y Uy Tín Hàng Đầu
            </span>
            <h1 class="hero-title gradient-text-gold">
              CHĂM SÓC THÚ CƯNG<br />NHƯ BẠN THÂN
            </h1>
            <p class="hero-desc">
              Tại <strong>MyPetClinic</strong>, chúng tôi hiểu rằng thú cưng là thành viên vô giá trong gia đình bạn. Với đội ngũ bác sĩ thú y giàu y đức, tay nghề cao cùng trang thiết bị hiện đại chuẩn quốc tế, chúng tôi cam kết mang lại dịch vụ chăm sóc sức khỏe tốt nhất.
            </p>
            <div class="hero-actions">
              <button class="btn-premium btn-lg shadow" @click="handleBookingBtnClick">
                <CalendarDays class="btn-icon-left" /> Đặt Lịch Ngay
              </button>
              <a href="#services" class="btn-premium-outline btn-lg" @click.prevent="scrollToSection('services')">
                Tìm Hiểu Dịch Vụ <ArrowDown class="btn-icon-right animate-bounce-y" />
              </a>
            </div>
          </div>
          
          <div class="hero-visual-right reveal-right active">
            <div class="position-relative d-inline-block">
              <div class="chat-bubble-floating animate-bounce">
                <HeartHandshake class="chat-bubble-icon" />
              </div>
              <img src="https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?q=80&w=600&auto=format&fit=crop" class="hero-image" alt="Pet Clinic Welcome" />
              <div class="badge-hours-floating">
                <h6><Clock class="badge-icon" /> Giờ Hành Chính</h6>
                <p>Đội ngũ bác sĩ luôn sẵn sàng phục vụ bé cưng trong khung giờ làm việc.</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- About Section -->
    <section class="py-5 bg-white border-bottom" id="intro">
      <div class="container my-4">
        <div class="about-row">
          <div class="about-visual reveal-left active">
            <img src="https://images.unsplash.com/photo-1576091160550-2173dba999ef?q=80&w=600&auto=format&fit=crop" class="about-image" alt="Veterinarian Examining Dog" />
          </div>
          <div class="about-content reveal-right active">
            <span class="section-tag-gold">Về Chúng Tôi</span>
            <h2 class="section-title">Nơi Gửi Gắm Niềm Tin Của Hàng Triệu Chủ Nuôi</h2>
            <p class="section-desc">
              Thành lập từ khát khao nâng cao chất lượng phúc lợi cho động vật tại Việt Nam, <strong>MyPetClinic</strong> đã không ngừng đổi mới và nâng cao năng lực y tế. Chúng tôi tin rằng mỗi thú cưng xứng đáng nhận được dịch vụ chăm sóc y tế chuyên nghiệp, nhân văn nhất.
            </p>
            <div class="benefits-grid">
              <div class="benefit-item">
                <div class="benefit-icon-wrapper">
                  <Award class="benefit-icon" />
                </div>
                <div>
                  <h6>Bác sĩ chuyên khoa</h6>
                  <p>Hơn 15 năm kinh nghiệm thực chiến y khoa.</p>
                </div>
              </div>
              <div class="benefit-item">
                <div class="benefit-icon-wrapper">
                  <HeartPulse class="benefit-icon" />
                </div>
                <div>
                  <h6>Thiết bị hiện đại</h6>
                  <p>Hệ thống chẩn đoán hình ảnh cao cấp nhập khẩu.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Core Services Section -->
    <section class="py-5" id="services">
      <div class="container my-4 text-center">
        <span class="section-tag-gold">Dịch Vụ Của Chúng Tôi</span>
        <h2 class="section-title text-center">Giải Pháp Chăm Sóc Toàn Diện Cho Thú Cưng</h2>
        <p class="section-subtitle max-w-2xl mx-auto">
          Chúng tôi cung cấp đầy đủ các danh mục dịch vụ từ y tế dự phòng, xét nghiệm chẩn đoán nâng cao cho đến các liệu trình phẫu thuật chuyên sâu và làm đẹp thẩm mỹ.
        </p>

        <div class="services-grid">
          <!-- Service Cards loop -->
          <div v-for="service in services" :key="service.id" class="service-card glass-card">
            <div class="service-icon-wrapper" :style="{ color: service.color, backgroundColor: service.bgColor }">
              <component :is="service.icon" class="service-card-icon" />
            </div>
            <h5 class="service-card-title">{{ service.name }}</h5>
            <p class="service-card-desc">{{ service.excerpt }}</p>
            <button class="btn-read-more" @click="goToServiceDetailPage(service.name)">
              Xem Chi Tiết <ChevronRight class="icon-right" />
            </button>
          </div>
        </div>
      </div>
    </section>

    <!-- Useful Knowledge & Blog Section -->
    <section class="py-5 bg-white border-top border-bottom" id="news">
      <div class="container my-4">
        <div class="text-center mb-5">
          <span class="section-tag-gold">Góc Chia Sẻ</span>
          <h2 class="section-title text-center">Cẩm Nang Sức Khỏe & Tin Tức Thú Cưng</h2>
          <p class="section-subtitle max-w-2xl mx-auto">
            Cập nhật những thông tin quan trọng, kiến thức chăm sóc khoa học từ đội ngũ bác sĩ y khoa MyPetClinic.
          </p>
        </div>

        <div class="articles-grid">
          <!-- Article Item -->
          <div v-for="article in articles" :key="article.id" class="article-card glass-card">
            <div class="article-img-wrapper">
              <img :src="article.image" class="article-img" :alt="article.title" />
              <span class="article-badge-tag">{{ article.tag }}</span>
            </div>
            <div class="article-body">
              <div class="article-meta">
                <span><Clock class="meta-icon" /> {{ article.date }}</span>
                <span>•</span>
                <span><User class="meta-icon" /> Bác sĩ MyPetClinic</span>
              </div>
              <h5 class="article-title" @click="openArticleModal(article)">
                {{ article.title }}
              </h5>
              <div class="article-tags">
                <span v-for="t in article.tags" :key="t" class="tag-badge"><Tag class="tag-badge-icon" />{{ t }}</span>
              </div>
              <p class="article-desc">{{ article.excerpt }}</p>
              <button class="btn-news-more" @click="openArticleModal(article)">
                Đọc thêm <ArrowRight class="icon-right" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </section>


    <!-- Shared Footer Layout -->
    <Footer />

    <!-- Shared Booking Modal -->
    <BookingModal 
      :show="showBookingModal" 
      @close="showBookingModal = false" 
      @success="handleBookingSuccess" 
      @error="handleBookingError" 
    />

    <!-- Article Detail Modal -->
    <div v-if="activeArticleModal" class="modal-overlay" @click.self="activeArticleModal = null">
      <div class="modal-card article-modal-card">
        <button class="modal-close" @click="activeArticleModal = null"><X /></button>
        <div class="article-modal-img-wrapper">
          <img :src="activeArticleModal.image" alt="Article image" class="article-modal-img" />
          <span class="article-modal-tag">{{ activeArticleModal.tag }}</span>
        </div>
        <div class="article-modal-content">
          <span class="article-modal-date"><Clock class="meta-icon" /> {{ activeArticleModal.date }}</span>
          <h3 class="modal-title">{{ activeArticleModal.title }}</h3>
          <div class="article-modal-body">
            <p v-for="(pText, idx) in activeArticleModal.paragraphs" :key="idx">{{ pText }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Zalo QR Code Modal -->
    <div v-if="isZaloModalOpen" class="modal-overlay" @click.self="isZaloModalOpen = false">
      <div class="modal-card qr-modal-card">
        <div class="qr-modal-header">
          <h3 class="modal-title text-center">Kết Nối Zalo Bác Sĩ</h3>
          <button class="modal-close" @click="isZaloModalOpen = false"><X /></button>
        </div>
        <div class="qr-modal-body text-center">
          <p class="subtitle-qr">Quét mã QR bên dưới để bắt đầu chat tư vấn trực tiếp với bác sĩ trực ca của phòng khám.</p>
          <div class="qr-wrapper">
            <img src="https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=https://zalo.me/0905090629" alt="Zalo QR Code" class="qr-img" />
          </div>
          <p class="phone-qr">Số điện thoại: <strong>0905.090.629</strong></p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';
import api from '../services/api';
import Header from '../components/layout/Header.vue';
import Footer from '../components/layout/Footer.vue';
import BookingModal from '../components/shared/BookingModal.vue';
import { 
  Heart, HeartPulse, HeartHandshake, ArrowDown, Clock, 
  Award, Stethoscope, Scissors, ShieldCheck, Tag, Info, 
  CheckCircle2, AlertCircle, ChevronRight, ArrowRight, X,
  CalendarDays, User
} from '@lucide/vue';

const router = useRouter();

const isLoggedIn = ref(false);
const showBookingModal = ref(false);
const activeArticleModal = ref<any | null>(null);
const isZaloModalOpen = ref(false);
const todayDate = ref('');

// Toasts notifications
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

const handleBookingSuccess = (msg: string) => {
  showToast(msg, 'success');
};

const handleBookingError = (msg: string) => {
  showToast(msg, 'error');
};

// Quick Booking
// (Removed unused quick booking handlers to avoid TS6133 compiler warnings)

const handleBookingBtnClick = () => {
  if (isLoggedIn.value) {
    showBookingModal.value = true;
  } else {
    Swal.fire({
      title: 'Chưa đăng nhập!',
      text: 'Để đặt lịch khám cho bé cưng, bạn vui lòng đăng nhập vào hệ thống nhé.',
      icon: 'info',
      iconColor: '#f59e0b',
      showCancelButton: true,
      confirmButtonText: 'Đăng nhập ngay',
      cancelButtonText: 'Để sau',
      confirmButtonColor: '#f59e0b',
      cancelButtonColor: '#f3f4f6',
      background: '#ffffff',
      customClass: {
        popup: 'rounded-4 shadow-lg border-0',
        title: 'fw-bold text-dark fs-4 mb-2',
        htmlContainer: 'text-muted mb-4',
        confirmButton: 'btn btn-warning rounded-pill px-4 fw-bold shadow-sm me-2',
        cancelButton: 'btn btn-light rounded-pill px-4 fw-bold shadow-sm border text-muted'
      },
      buttonsStyling: false
    }).then((result) => {
      if (result.isConfirmed) {
        router.push('/login');
      }
    });
  }
};

const scrollToSection = (id: string) => {
  const el = document.getElementById(id);
  if (el) {
    window.scrollTo({
      top: el.offsetTop - 80,
      behavior: 'smooth'
    });
  }
};

const goToServiceDetailPage = (serviceName: string) => {
  if (serviceName === 'Khám & Điều Trị') {
    router.push('/services/kham-dieu-tri');
  } else if (serviceName === 'Spa & Làm Đẹp') {
    router.push('/services/spa-grooming');
  } else if (serviceName === 'Tiêm Phòng Bệnh') {
    router.push('/services/tiem-phong');
  } else if (serviceName === 'Phẫu Thuật Ngoại') {
    router.push('/services/kham-dieu-tri#phau-thuat');
  }
};

const checkLoginState = async () => {
  try {
    await api.get('/profile');
    isLoggedIn.value = true;
  } catch (err) {
    isLoggedIn.value = false;
  }
};

// Core service list
const services = ref([
  {
    id: 1,
    name: 'Tiêm Phòng Bệnh',
    icon: ShieldCheck,
    color: '#0ea5e9',
    bgColor: 'rgba(14, 165, 233, 0.1)',
    excerpt: 'Tiêm phòng vaccine và phòng chống ký sinh trùng định kỳ theo phác đồ khoa học giúp phòng ngừa tối đa bệnh tật.'
  },
  {
    id: 2,
    name: 'Khám & Điều Trị',
    icon: Stethoscope,
    color: '#f59e0b',
    bgColor: 'rgba(245, 158, 11, 0.15)',
    excerpt: 'Thăm khám tổng quát, tư vấn chế độ dinh dưỡng, chẩn đoán lâm sàng chuẩn xác các bệnh nội khoa phức tạp.'
  },
  {
    id: 3,
    name: 'Phẫu Thuật Ngoại',
    icon: HeartPulse,
    color: '#10b981',
    bgColor: 'rgba(16, 185, 129, 0.1)',
    excerpt: 'Phòng mổ vô trùng áp lực dương tuyệt đối. Thực hiện các ca phẫu thuật triệt sản, mổ đẻ hay nối xương phức tạp.'
  },
  {
    id: 4,
    name: 'Spa & Làm Đẹp',
    icon: Scissors,
    color: '#6366f1',
    bgColor: 'rgba(99, 102, 241, 0.1)',
    excerpt: 'Cắt tỉa lông tạo kiểu nghệ thuật, tắm sấy khử mùi, vắt tuyến hôi và vệ sinh tai móng chuyên nghiệp từ các chuyên gia Grooming.'
  }
]);

// Cẩm nang & blog
const articles = ref([
  {
    id: 1,
    title: 'Nấm da ở chó mèo có lây sang người không?',
    tag: 'Sức khỏe',
    date: '25/05/2026',
    image: 'https://images.unsplash.com/photo-1596492784531-6e6eb5ea9993?q=80&w=400&auto=format&fit=crop',
    tags: ['Nấm da', 'Lây nhiễm', 'Bệnh da liễu'],
    excerpt: 'Bạn thấy thú cưng rụng lông từng mảng tròn, ngứa liên tục... vài ngày sau chính bạn cũng xuất hiện vết đỏ. Tìm hiểu nguyên nhân và phác đồ điều trị dứt điểm...',
    paragraphs: [
      'Bệnh nấm da (Microsporum canis) là căn bệnh da liễu phổ biến nhất ở chó mèo, đặc biệt phát triển mạnh trong điều kiện thời tiết nóng ẩm tại Việt Nam.',
      'Bệnh này HOÀN TOÀN CÓ THỂ lây trực tiếp từ chó mèo sang người thông qua ôm ấp, tiếp xúc da thịt hoặc qua môi trường sống trung gian như nệm, lược chải lông. Ở người, nấm da tạo thành các vệt tròn đỏ hình đồng xu gây ngứa ngáy dữ dội (hắc lào).',
      'Phác đồ điều trị dứt điểm bao gồm: cạo lông quanh vùng tổn thương của thú cưng, tắm bằng dầu tắm sát khuẩn chuyên dụng kháng nấm, và bôi thuốc mỡ. Đồng thời, vệ sinh khử trùng toàn bộ nhà cửa, xịt cồn y tế lên nệm nằm để diệt sạch bào tử nấm.'
    ]
  },
  {
    id: 2,
    title: 'Tại sao chó bị rụng lông và cách điều trị hiệu quả',
    tag: 'Kinh nghiệm',
    date: '24/05/2026',
    image: 'https://images.unsplash.com/photo-1581888227599-779811939961?q=80&w=400&auto=format&fit=crop',
    tags: ['Rụng lông', 'Kinh nghiệm', 'Chăm sóc chó'],
    excerpt: 'Chó rụng lông là hiện tượng sinh lý bình thường nhưng rụng quá nhiều kèm theo ngứa, lở loét có thể là biểu hiện của viêm da, ghẻ Demodex hoặc dị ứng...',
    paragraphs: [
      'Hiện tượng rụng lông ở chó có hai loại chính: rụng lông sinh lý (thay lông định kỳ) và rụng lông bệnh lý. Rụng lông sinh lý thường đều khắp cơ thể và da chó vẫn hồng hào, mịn màng.',
      'Nếu chó rụng lông từng mảng, lộ da đỏ ửng, ngứa gãi liên tục, có vảy gàu hoặc mủ thì đây chắc chắn là rụng lông bệnh lý. Nguyên nhân có thể do ký sinh trùng (ve, rận, ghẻ Sarcoptes/Demodex), nấm da hoặc dị ứng thức ăn/sữa tắm.',
      'Cách khắc phục hiệu quả: Đưa bé đi cạo da làm xét nghiệm kính hiển vi để tìm đúng nguyên nhân. Sử dụng các thuốc nhỏ gáy hoặc uống diệt ngoại ký sinh trùng thế hệ mới, tắm bằng xà phòng y tế hỗ trợ và cải thiện khẩu phần ăn giàu Omega-3, Omega-6 giúp lông khỏe mượt.'
    ]
  },
  {
    id: 3,
    title: 'Chó bị táo bón: Biểu hiện và cách điều trị tại nhà',
    tag: 'Dinh dưỡng',
    date: '20/05/2026',
    image: 'https://images.unsplash.com/photo-1544568100-847a948585b9?q=80&w=400&auto=format&fit=crop',
    tags: ['Táo bón', 'Tiêu hóa', 'Dinh dưỡng'],
    excerpt: 'Táo bón lâu ngày có thể gây ra phình đại tràng, nhiễm độc ngược dòng rất nguy hiểm cho bé. Tìm hiểu ngay biểu hiện và mẹo xử lý tại nhà...',
    paragraphs: [
      'Chó bị táo bón là tình trạng phân khô cứng, chó đi rặn khó khăn, kêu rên khi rặn hoặc không thể đi tiêu trong vòng 2-3 ngày liên tiếp.',
      'Nguyên nhân phổ biến nhất là do khẩu phần ăn thiếu chất xơ, chó uống ít nước, nuốt phải dị vật như đất đá, xương gà, hoặc lười vận động dẫn đến nhu động ruột kém.',
      'Cách điều trị tại nhà: Bổ sung ngay chất xơ từ bí đỏ hấp chín, khoai lang xay nhuyễn vào bữa ăn. Khuyến khích chó uống nhiều nước, cho đi bộ vận động nhiều hơn. Trong trường hợp nặng, không được tự ý bơm thuốc thụt đại tràng mà cần đưa đến bác sĩ thú y để thụt rửa an toàn tránh vỡ ruột.'
    ]
  }
]);

const openArticleModal = (article: any) => {
  activeArticleModal.value = article;
};

onMounted(() => {
  checkLoginState();
  
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const tomorrowStr = tomorrow.toISOString().split('T')[0];
  todayDate.value = tomorrowStr;
});
</script>

<style scoped>
.home-wrapper {
  background-color: var(--bg-light);
  color: var(--text-dark);
  min-height: 100vh;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1.5rem;
}

/* Toast Notifications styling */
.toast-container {
  position: fixed;
  top: 90px;
  right: 20px;
  z-index: 1200;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.toast {
  background: white;
  border-radius: var(--radius-md);
  padding: 1rem 1.5rem;
  box-shadow: var(--shadow-lg);
  display: flex;
  align-items: center;
  gap: 12px;
  border-left: 5px solid #cbd5e1;
  min-width: 300px;
}

.toast-success { border-left-color: #10b981; }
.toast-error { border-left-color: #ef4444; }
.toast-info { border-left-color: #f59e0b; }

.toast-icon {
  width: 20px;
  height: 20px;
}
.toast-success .toast-icon { color: #10b981; }
.toast-error .toast-icon { color: #ef4444; }
.toast-info .toast-icon { color: #f59e0b; }

.toast-message {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--text-dark);
}

.toast-fade-enter-active, .toast-fade-leave-active {
  transition: all 0.3s ease;
}
.toast-fade-enter-from {
  opacity: 0;
  transform: translateX(50px);
}
.toast-fade-leave-to {
  opacity: 0;
  transform: translateX(50px);
}

/* Hero Section */
.hero-section {
  padding: 5rem 0;
  overflow: hidden;
}

.hero-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2.5rem;
  align-items: center;
}

@media (max-width: 991px) {
  .hero-row {
    grid-template-columns: 1fr;
    text-align: center;
  }
  .hero-actions {
    justify-content: center;
  }
}

.hero-shape-1 {
  position: absolute;
  top: -20%;
  right: -10%;
  width: 600px;
  height: 600px;
  background: radial-gradient(circle, rgba(254, 243, 199, 0.7) 0%, rgba(254, 243, 199, 0) 70%);
  z-index: 1;
  animation: floatShape1 20s ease-in-out infinite;
  pointer-events: none;
}

.hero-shape-2 {
  position: absolute;
  bottom: -10%;
  left: -10%;
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, rgba(254, 243, 199, 0.5) 0%, rgba(254, 243, 199, 0) 70%);
  z-index: 1;
  animation: floatShape2 16s ease-in-out infinite;
  pointer-events: none;
}

@keyframes floatShape1 {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(-30px, 40px) scale(1.1); }
  100% { transform: translate(0, 0) scale(1); }
}

@keyframes floatShape2 {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(40px, -30px) scale(0.95); }
  100% { transform: translate(0, 0) scale(1); }
}

.hero-content-left {
  position: relative;
  z-index: 2;
}

.hero-tag {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: var(--primary-gold);
  color: var(--text-dark);
  font-weight: 700;
  font-size: 0.8rem;
  padding: 0.5rem 1rem;
  border-radius: 50px;
  text-transform: uppercase;
  box-shadow: var(--shadow-sm);
  margin-bottom: 1.5rem;
}

.icon-heart-fill {
  width: 14px;
  height: 14px;
  fill: #ef4444;
  color: #ef4444;
}

.hero-title {
  font-size: 3rem;
  font-weight: 900;
  line-height: 1.2;
  margin-bottom: 1.5rem;
}

.hero-desc {
  font-size: 1.05rem;
  color: var(--text-muted);
  line-height: 1.7;
  margin-bottom: 2.5rem;
}

.hero-actions {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.btn-icon-left {
  width: 18px;
  height: 18px;
}

.btn-icon-right {
  width: 16px;
  height: 16px;
}

.animate-bounce-y {
  animation: bounceY 1.5s infinite;
}

@keyframes bounceY {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(4px); }
}

.hero-visual-right {
  display: flex;
  justify-content: center;
  position: relative;
  z-index: 2;
}

.hero-image {
  width: 420px;
  height: 420px;
  object-fit: cover;
  border-radius: 50%;
  border: 6px solid white;
  box-shadow: var(--shadow-lg);
  transition: transform 0.5s ease;
}

.hero-image:hover {
  transform: scale(1.02);
}

.chat-bubble-floating {
  position: absolute;
  left: 0;
  top: 10%;
  width: 80px;
  height: 80px;
  background: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: var(--shadow-lg);
  z-index: 5;
}

.chat-bubble-icon {
  width: 40px;
  height: 40px;
  color: var(--primary-gold);
}

.animate-bounce {
  animation: bounce 3s infinite ease-in-out;
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-12px); }
}

.badge-hours-floating {
  position: absolute;
  right: -20px;
  bottom: 10px;
  width: 220px;
  background: var(--primary-gold);
  color: var(--text-dark);
  padding: 1rem;
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  border-left: 5px solid var(--primary-dark);
  text-align: left;
}

.badge-hours-floating h6 {
  font-weight: 700;
  margin-bottom: 4px;
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 0.9rem;
}

.badge-hours-floating p {
  font-size: 0.75rem;
  color: rgba(41, 37, 36, 0.8) !important;
  margin: 0;
  line-height: 1.4;
}

.badge-icon {
  width: 14px;
  height: 14px;
}

/* About Section */
.about-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 3.5rem;
  align-items: center;
}

@media (max-width: 991px) {
  .about-row {
    grid-template-columns: 1fr;
  }
}

.about-image {
  width: 100%;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
}

.section-tag-gold {
  color: var(--primary-dark);
  font-weight: 700;
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 2px;
  display: block;
  margin-bottom: 0.5rem;
}

.section-title {
  font-size: 2rem;
  font-weight: 800;
  margin-bottom: 1.5rem;
  color: var(--text-dark);
  text-align: left;
}

.section-desc {
  font-size: 1rem;
  color: var(--text-muted);
  line-height: 1.7;
  margin-bottom: 2rem;
}

.benefits-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 576px) {
  .benefits-grid {
    grid-template-columns: 1fr;
  }
}

.benefit-item {
  display: flex;
  align-items: start;
  gap: 12px;
}

.benefit-icon-wrapper {
  background: var(--primary-cream);
  color: var(--primary-dark);
  padding: 0.6rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.benefit-icon {
  width: 24px;
  height: 24px;
}

.benefit-item h6 {
  font-size: 0.95rem;
  font-weight: 700;
  margin-bottom: 4px;
}

.benefit-item p {
  font-size: 0.8rem;
  margin: 0;
  color: var(--text-muted);
}

/* Services section */
.section-subtitle {
  font-size: 1rem;
  color: var(--text-muted);
  margin-bottom: 3rem;
}

.services-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
  text-align: left;
}

.service-card {
  padding: 2.5rem 2rem;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: start;
}

.service-icon-wrapper {
  width: 60px;
  height: 60px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 1.5rem;
  box-shadow: var(--shadow-sm);
}

.service-card-icon {
  width: 30px;
  height: 30px;
}

.service-card-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin-bottom: 0.75rem;
}

.service-card-desc {
  font-size: 0.85rem;
  line-height: 1.6;
  color: var(--text-muted);
  margin-bottom: 1.5rem;
  flex-grow: 1;
}

.btn-read-more {
  display: flex;
  align-items: center;
  gap: 4px;
  background: none;
  border: none;
  color: var(--primary-dark);
  font-weight: 700;
  font-size: 0.85rem;
  cursor: pointer;
  padding: 0;
  transition: transform 0.2s;
}

.btn-read-more:hover {
  transform: translateX(4px);
}

.icon-right {
  width: 14px;
  height: 14px;
}

/* Articles Section */
.articles-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 2.5rem;
}

.article-card {
  border-radius: var(--radius-md);
  overflow: hidden;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.article-img-wrapper {
  position: relative;
  height: 220px;
  overflow: hidden;
}

.article-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.5s ease;
}

.article-card:hover .article-img {
  transform: scale(1.05);
}

.article-badge-tag {
  position: absolute;
  top: 15px;
  left: 15px;
  background: #ef4444;
  color: white;
  font-weight: 700;
  font-size: 0.75rem;
  padding: 0.3rem 0.8rem;
  border-radius: 5px;
  text-transform: uppercase;
}

.article-card:nth-child(2) .article-badge-tag {
  background: var(--primary-gold);
  color: var(--text-dark);
}

.article-card:nth-child(3) .article-badge-tag {
  background: #0ea5e9;
}

.article-body {
  padding: 2rem 1.5rem;
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.article-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.8rem;
  color: var(--text-muted);
  margin-bottom: 0.75rem;
}

.meta-icon {
  width: 14px;
  height: 14px;
  display: inline-block;
}

.article-title {
  font-size: 1.15rem;
  font-weight: 750;
  line-height: 1.4;
  margin-bottom: 0.75rem;
  cursor: pointer;
  transition: color 0.2s;
}

.article-title:hover {
  color: var(--primary-gold);
}

.article-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 1rem;
}

.tag-badge {
  font-size: 0.75rem;
  color: var(--text-muted);
  background: var(--bg-light);
  border: 1px solid var(--border-color);
  padding: 0.2rem 0.6rem;
  border-radius: 4px;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.tag-badge-icon {
  width: 10px;
  height: 10px;
}

.article-desc {
  font-size: 0.85rem;
  line-height: 1.6;
  color: var(--text-muted);
  margin-bottom: 1.5rem;
  flex-grow: 1;
}

.btn-news-more {
  background: none;
  border: none;
  color: var(--primary-dark);
  font-weight: 700;
  font-size: 0.85rem;
  cursor: pointer;
  padding: 0;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  transition: transform 0.2s;
}

.btn-news-more:hover {
  transform: translateX(4px);
}

/* Contact and Quick Booking */
.contact-section-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 3.5rem;
  align-items: center;
}

@media (max-width: 991px) {
  .contact-section-grid {
    grid-template-columns: 1fr;
  }
}

.contact-info-left h2 {
  font-size: 2rem;
  font-weight: 800;
  margin-bottom: 1rem;
}

.contact-desc {
  font-size: 0.95rem;
  line-height: 1.7;
  color: var(--text-muted);
  margin-bottom: 2rem;
}

.contact-details-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  margin-bottom: 2.5rem;
}

.contact-detail-item {
  display: flex;
  gap: 12px;
  align-items: start;
}

.contact-detail-icon {
  width: 24px;
  height: 24px;
  color: var(--primary-dark);
  margin-top: 2px;
}

.contact-detail-item h5 {
  font-size: 0.95rem;
  font-weight: 700;
  margin-bottom: 2px;
}

.contact-detail-item p {
  font-size: 0.85rem;
  color: var(--text-muted);
  margin: 0;
}

.btn-zalo {
  background: #0068ff;
  color: white;
  border: none;
  border-radius: var(--radius-md);
  padding: 0.8rem 2rem;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  box-shadow: 0 4px 14px rgba(0, 104, 255, 0.35);
  transition: var(--transition-smooth);
}

.btn-zalo:hover {
  background: #0056d6;
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0, 104, 255, 0.45);
}

.btn-zalo-icon {
  width: 20px;
  height: 20px;
}

.booking-card {
  padding: 2.5rem;
  background: white !important;
}

.booking-card h3 {
  font-size: 1.4rem;
  font-weight: 800;
  margin-bottom: 0.5rem;
}

.booking-card p {
  font-size: 0.85rem;
  color: var(--text-muted);
  margin-bottom: 1.5rem;
}

.quick-booking-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-group-booking {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.form-group-booking label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-muted);
}

.form-row-booking {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

@media (max-width: 576px) {
  .form-row-booking {
    grid-template-columns: 1fr;
  }
}

.form-input, .form-select {
  padding: 0.75rem 1rem;
  border-radius: var(--radius-sm);
  border: 1px solid rgba(0, 0, 0, 0.08);
  font-family: inherit;
  font-size: 0.9rem;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s infinite linear;
  display: inline-block;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Modals global layouts */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(5px);
  z-index: 1100;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}

.modal-card {
  background: white;
  width: 100%;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  animation: modal-enter 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-close {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-dark);
  display: flex;
  align-items: center;
}

.modal-close svg {
  width: 20px;
  height: 20px;
}

.modal-title {
  font-size: 1.2rem;
  font-weight: 800;
  margin: 0;
}

/* Article Modal specifically */
.article-modal-card {
  max-width: 700px;
  max-height: 90vh;
  position: relative;
}

.article-modal-card .modal-close {
  position: absolute;
  top: 15px;
  right: 15px;
  background: rgba(255, 255, 255, 0.8);
  border-radius: 50%;
  padding: 0.4rem;
  box-shadow: var(--shadow-sm);
  z-index: 10;
}

.article-modal-img-wrapper {
  height: 280px;
  position: relative;
}

.article-modal-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.article-modal-tag {
  position: absolute;
  bottom: 15px;
  left: 15px;
  background: var(--primary-gold);
  color: var(--text-dark);
  font-weight: 700;
  font-size: 0.8rem;
  padding: 0.4rem 1rem;
  border-radius: 5px;
  text-transform: uppercase;
}

.article-modal-content {
  padding: 2rem;
  overflow-y: auto;
}

.article-modal-date {
  font-size: 0.8rem;
  color: var(--text-muted);
  display: block;
  margin-bottom: 0.5rem;
}

.article-modal-body {
  margin-top: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  font-size: 0.9rem;
  line-height: 1.6;
  color: var(--text-muted);
}

/* QR Code Modal specifically */
.qr-modal-card {
  max-width: 420px;
}

.qr-modal-header {
  padding: 1.2rem 1.5rem;
  border-bottom: 1px solid var(--border-color);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.qr-modal-body {
  padding: 2rem 1.5rem;
}

.subtitle-qr {
  font-size: 0.85rem;
  color: var(--text-muted);
  margin-bottom: 1.5rem;
}

.qr-wrapper {
  background: white;
  padding: 1rem;
  border: 1px solid var(--border-color);
  border-radius: 12px;
  display: inline-block;
  box-shadow: var(--shadow-sm);
  margin-bottom: 1.5rem;
}

.qr-img {
  width: 180px;
  height: 180px;
}

.phone-qr {
  font-size: 0.95rem;
  color: var(--text-dark);
}
</style>
