<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <PhoneCall class="icon-phone" /> Kết Nối Với Chúng Tôi
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">LIÊN HỆ & ĐẶT LỊCH</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Cơ sở chính sẵn sàng hỗ trợ thăm khám, tư vấn chăm sóc sức khỏe cho bé cưng mọi lúc bạn cần.
        </p>
      </div>
    </section>

    <!-- Contact Details -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <div class="contact-grid">
          <!-- Left Column: Location details and Working Hours -->
          <div class="contact-left">
            <h3 class="fw-bold mb-4 block-title">Trụ Sở Chính</h3>
            
            <!-- Branch 1 -->
            <div class="d-flex align-items-start gap-3 mb-4 branch-item">
              <div class="icon-box-gold">
                <MapPin class="branch-icon" />
              </div>
              <div>
                <h6 class="fw-bold mb-1 text-dark">MyPetClinic (Trụ sở chính)</h6>
                <p class="small text-muted mb-1">124A Xuân Thủy, Phường An Khánh, TP. Hồ Chí Minh</p>
                <p class="small mb-0 text-warning-highlight fw-bold"><Phone class="phone-inline-icon" /> Hotline: 0905 090 629</p>
              </div>
            </div>

            <hr class="my-4" />

            <h4 class="fw-bold mb-3 block-title">Giờ Làm Việc</h4>
            <div class="card bg-light border-0 p-3 rounded-4 mb-4 hours-card">
              <div class="hours-row">
                <span>Thứ 2 - Thứ 7:</span>
                <strong class="text-dark">Sáng: 08:00 - 12:00 | Chiều: 14:00 - 19:00</strong>
              </div>
              <div class="hours-row">
                <span>Chủ nhật & Lễ:</span>
                <strong class="text-dark">Sáng: 08:00 - 12:00 (Chiều nghỉ)</strong>
              </div>
            </div>

            <h4 class="fw-bold mb-3 block-title">Bản Đồ</h4>
            <div class="rounded-4 overflow-hidden border shadow-sm map-container">
              <iframe style="border: 0; width: 100%; height: 100%;" src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3920.0531119583807!2d106.70744515079734!3d10.73038709231523!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752f91a2a96ad1%3A0xc19a98cc93ec317e!2sPetcare+Veterinary+Hospital!5e0!3m2!1sen!2s!4v1465265330727" frameborder="0" allowfullscreen="true"></iframe>
            </div>
          </div>

          <!-- Right Column: Contact Channels -->
          <div class="contact-right">
            <div class="card border-0 glass-card p-4 shadow-md booking-card">
              <h3 class="fw-bold mb-3 text-dark">Đặt Lịch Ngay</h3>
              <p class="text-muted small mb-4">
                Quý khách vui lòng chọn một trong các phương thức liên hệ dưới đây để đặt lịch khám nhanh chóng hoặc nhận tư vấn trực tiếp từ các bác sĩ thú y tại MyPetClinic:
              </p>

              <div class="action-buttons-grid">
                <!-- Zalo Option -->
                <button type="button" @click="isZaloLocalOpen = true" class="btn-channel btn-zalo-outline">
                  <MessageSquare class="channel-icon" />
                  <span class="fw-bold">Zalo Tư Vấn (0905 090 629)</span>
                </button>

                <!-- Messenger Option -->
                <a href="https://m.me/mypetclinic" target="_blank" class="btn-channel btn-messenger-outline">
                  <MessageCircle class="channel-icon" />
                  <span class="fw-bold">Facebook Messenger</span>
                </a>

                <!-- Hotline Option -->
                <a href="tel:0905090629" class="btn-channel btn-phone-outline">
                  <PhoneCall class="channel-icon" />
                  <span class="fw-bold">Gọi Hotline: 0905 090 629</span>
                </a>
              </div>
            </div>
          </div>
        </div>

        <!-- FAQ Section -->
        <div class="faq-section mt-5 pt-4">
          <h3 class="fw-bold text-center mb-5 faq-title-center">Câu Hỏi Thường Gặp (FAQs)</h3>
          <div class="faq-list">
            <div v-for="(faq, idx) in faqs" :key="idx" class="faq-item">
              <button class="faq-question" @click="toggleFaq(idx)">
                {{ faq.question }}
                <ChevronDown class="faq-chevron" :class="{ 'rotated': openFaqs.includes(idx) }" />
              </button>
              <div class="faq-answer" v-show="openFaqs.includes(idx)">
                <p>{{ faq.answer }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Zalo Local Modal -->
    <div v-if="isZaloLocalOpen" class="zalo-modal-overlay" @click.self="isZaloLocalOpen = false">
      <div class="zalo-modal-card">
        <div class="zalo-modal-header">
          <h5 class="modal-title"><QrCode class="modal-icon-title" /> Quét QR Zalo MyPetClinic</h5>
          <button class="modal-close" @click="isZaloLocalOpen = false"><X /></button>
        </div>
        <div class="zalo-modal-body">
          <p class="modal-desc">
            Quét mã QR dưới đây bằng ứng dụng Zalo để liên hệ và nhận tư vấn nhanh chóng từ MyPetClinic.
          </p>
          <div class="qr-wrapper">
            <img src="https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=https://zalo.me/0905090629" alt="Zalo QR Code" />
          </div>
          <div class="qr-info-row">Tài khoản Zalo: 0905 090 629</div>
          <div class="qr-info-sub">Chủ tài khoản: MyPetClinic Support</div>
          <div class="modal-actions-list">
            <a href="https://zalo.me/0905090629" target="_blank" class="btn-zalo-action">Mở bằng ứng dụng Zalo</a>
          </div>
        </div>
      </div>
    </div>

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
import { 
  PhoneCall, MapPin, Phone, MessageSquare, 
  MessageCircle, ChevronDown, QrCode, X 
} from '@lucide/vue';

const showBookingModal = ref(false);
const isZaloLocalOpen = ref(false);
const openFaqs = ref<number[]>([]);

const handleBookingSuccess = (msg: string) => {
  alert(msg);
};

const handleBookingError = (msg: string) => {
  alert(msg);
};

const faqs = [
  {
    question: '1. Tôi có cần đặt lịch khám trước khi đưa thú cưng tới không?',
    answer: 'MyPetClinic khuyến khích khách hàng đặt lịch khám trực tuyến hoặc gọi hotline trước khi đến để được ưu tiên sắp xếp bác sĩ và hạn chế thời gian chờ đợi. Tuy nhiên, chúng tôi vẫn tiếp nhận các ca khám trực tiếp trong giờ làm việc.'
  },
  {
    question: '2. Chi phí điều trị nội trú tại bệnh viện được tính thế nào?',
    answer: 'Chi phí chăm sóc nội trú bao gồm phí lưu chuồng theo ngày, tiền thuốc theo phác đồ điều trị, dinh dưỡng đặc thù và công theo dõi sát sao của các điều dưỡng. Bác sĩ sẽ luôn trao đổi chi tiết và đưa ra bảng ước tính chi phí trước khi làm thủ tục nhập viện cho thú cưng.'
  },
  {
    question: '3. Bệnh viện có tiếp nhận cấp cứu ngoài giờ hành chính không?',
    answer: 'Hiện tại, MyPetClinic hoạt động phục vụ theo khung giờ hành chính cố định được niêm yết (Thứ 2 - Thứ 7: 8h00 - 19h00; Chủ nhật: 8h00 - 12h00). Đối với các sự cố khẩn cấp phát sinh ngoài giờ làm việc trên, quý khách vui lòng liên hệ các trung tâm cấp cứu 24/7 chuyên biệt.'
  }
];

const toggleFaq = (idx: number) => {
  if (openFaqs.value.includes(idx)) {
    openFaqs.value = openFaqs.value.filter(i => i !== idx);
  } else {
    openFaqs.value.push(idx);
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

.icon-phone {
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

.contact-grid {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 3.5rem;
}

@media (max-width: 991px) {
  .contact-grid {
    grid-template-columns: 1fr;
  }
}

.block-title {
  font-size: 1.4rem;
  font-weight: 800;
  margin-bottom: 1.5rem;
}

.icon-box-gold {
  background-color: rgba(245, 158, 11, 0.2);
  color: var(--primary-gold);
  padding: 0.6rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.branch-icon {
  width: 24px;
  height: 24px;
}

.text-warning-highlight {
  color: var(--primary-dark) !important;
  font-weight: 700;
}

.phone-inline-icon {
  width: 14px;
  height: 14px;
  display: inline-block;
}

.hours-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 1.25rem !important;
  background-color: #f7f6f2 !important;
}

.hours-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
  color: var(--text-muted);
}

.map-container {
  height: 220px;
  border: 1px solid var(--border-color);
}

.booking-card {
  padding: 2.5rem;
  background-color: white !important;
}

.action-buttons-grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.btn-channel {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 1rem;
  border-radius: 8px;
  font-size: 1rem;
  text-decoration: none;
  cursor: pointer;
  background: transparent;
  transition: transform var(--transition-speed);
}

.btn-channel:hover {
  transform: translateY(-2px);
}

.btn-zalo-outline {
  border: 2px solid #0068ff;
  color: #0068ff;
}

.btn-messenger-outline {
  border: 2px solid #0084ff;
  color: #0084ff;
}

.btn-phone-outline {
  border: 2px solid #198754;
  color: #198754;
}

.channel-icon {
  width: 20px;
  height: 20px;
}

/* FAQ layout */
.faq-title-center {
  font-size: 1.8rem;
  font-weight: 800;
  text-align: center;
}

.faq-list {
  max-width: 800px;
  margin: 0 auto;
}

.faq-item {
  border-bottom: 1px solid var(--border-color);
  padding: 0.75rem 0;
}

.faq-question {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 700;
  font-size: 0.95rem;
  color: var(--text-dark);
  background: none;
  border: none;
  padding: 0.75rem 0;
  cursor: pointer;
  text-align: left;
}

.faq-chevron {
  width: 16px;
  height: 16px;
  transition: transform 0.2s;
}

.faq-chevron.rotated {
  transform: rotate(180deg);
}

.faq-answer {
  padding: 0.5rem 0 1rem 0;
  font-size: 0.85rem;
  color: var(--text-muted);
  line-height: 1.6;
}

/* Zalo Modal */
.zalo-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(5px);
  z-index: 1200;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}

.zalo-modal-card {
  background: white;
  width: 100%;
  max-width: 420px;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
}

.zalo-modal-header {
  background-color: #0068ff;
  color: white;
  padding: 1.2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-icon-title {
  width: 20px;
  height: 20px;
}

.modal-close {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
}

.modal-close svg {
  width: 18px;
  height: 18px;
}

.zalo-modal-body {
  padding: 2rem 1.5rem;
  text-align: center;
}

.modal-desc {
  font-size: 0.85rem;
  color: var(--text-muted);
  margin-bottom: 1.5rem;
}

.qr-wrapper {
  padding: 1rem;
  border: 1px solid var(--border-color);
  border-radius: 12px;
  display: inline-block;
  box-shadow: var(--shadow-sm);
  margin-bottom: 1.5rem;
  background: white;
}

.qr-wrapper img {
  width: 200px;
  height: 200px;
}

.qr-info-row {
  font-weight: 700;
  font-size: 0.95rem;
  margin-bottom: 4px;
}

.qr-info-sub {
  font-size: 0.8rem;
  color: var(--text-muted);
  margin-bottom: 1.5rem;
}

.btn-zalo-action {
  background-color: #0068ff;
  color: white;
  padding: 0.75rem;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.95rem;
  display: block;
  text-decoration: none;
  transition: background-color 0.2s;
}

.btn-zalo-action:hover {
  background-color: #0056d6;
}
</style>
