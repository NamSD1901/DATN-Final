<template>
  <div class="contact-wrapper">
    <!-- Background elements -->
    <div class="bg-glow bg-glow-1"></div>
    <div class="bg-glow bg-glow-2"></div>

    <!-- Toast Notifications -->
    <TransitionGroup name="toast-fade" tag="div" class="toast-container">
      <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast-${toast.type}`]">
        <component :is="toast.icon" class="toast-icon" />
        <span class="toast-message">{{ toast.message }}</span>
      </div>
    </TransitionGroup>

    <div class="contact-container">
      <!-- Back button -->
      <div class="action-bar">
        <router-link to="/" class="btn-back">
          <ArrowLeft class="icon-btn" />
          <span>Quay lại Trang chủ</span>
        </router-link>
      </div>

      <!-- Header Section -->
      <div class="contact-header">
        <span class="section-tag">Liên hệ hỗ trợ</span>
        <h1 class="gradient-text">Kết Nối Với Chúng Tôi</h1>
        <p class="contact-subtitle">
          Ý kiến đóng góp và phản hồi của bạn là động lực to lớn giúp MyPetClinic cải thiện dịch vụ mỗi ngày. Hãy gửi tin nhắn cho chúng tôi hoặc kết nối Zalo để được phản hồi ngay lập tức.
        </p>
      </div>

      <div class="contact-grid">
        <!-- Contact Information & Zalo QR -->
        <div class="info-column">
          <div class="info-card">
            <h3>Thông Tin Liên Hệ</h3>
            
            <div class="details-list">
              <div class="detail-item">
                <MapPin class="detail-icon" />
                <div>
                  <h4>Địa chỉ phòng khám</h4>
                  <p>123 Đường Nguyễn Văn Linh, Quận Hải Châu, TP. Đà Nẵng</p>
                </div>
              </div>

              <div class="detail-item">
                <Phone class="detail-icon" />
                <div>
                  <h4>Hotline hỗ trợ & Cấp cứu 24/7</h4>
                  <p class="highlight-text">0905 090 629</p>
                </div>
              </div>

              <div class="detail-item">
                <Mail class="detail-icon" />
                <div>
                  <h4>Email tiếp nhận đóng góp</h4>
                  <p>support@mypetclinic.com</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Zalo QR Code card -->
          <div class="zalo-card">
            <div class="zalo-icon-box">
              <MessageSquare class="zalo-icon" />
            </div>
            <h3>Kênh Tư Vấn Zalo</h3>
            <p>Quét mã QR dưới đây để chat trực tuyến cùng bác sĩ trực ca của MyPetClinic.</p>
            <div class="qr-wrapper">
              <img src="https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=https://zalo.me/0905090629" alt="Zalo QR Code" class="qr-img" />
            </div>
            <span class="zalo-phone">Zalo Hotline: 0905.090.629</span>
          </div>
        </div>

        <!-- Feedback & Suggestions Form -->
        <div class="form-column">
          <div class="form-card">
            <h3>Gửi Ý Kiến Đóng Góp</h3>
            <p>Chúng tôi luôn lắng nghe ý kiến phản hồi về chất lượng dịch vụ từ bạn.</p>

            <form @submit.prevent="handleFeedbackSubmit" class="feedback-form">
              <div class="input-group">
                <label for="fullName">Họ và Tên</label>
                <input 
                  id="fullName" 
                  type="text" 
                  v-model="feedbackForm.fullName" 
                  placeholder="Nguyễn Văn A" 
                  required 
                  class="form-input"
                />
              </div>

              <div class="input-group">
                <label for="email">Địa chỉ Email</label>
                <input 
                  id="email" 
                  type="email" 
                  v-model="feedbackForm.email" 
                  placeholder="name@example.com" 
                  required 
                  class="form-input"
                />
              </div>

              <div class="input-group">
                <label for="subject">Tiêu đề phản hồi</label>
                <input 
                  id="subject" 
                  type="text" 
                  v-model="feedbackForm.subject" 
                  placeholder="Góp ý về dịch vụ spa, thái độ phục vụ..." 
                  required 
                  class="form-input"
                />
              </div>

              <div class="input-group">
                <label for="message">Nội dung chi tiết</label>
                <textarea 
                  id="message" 
                  v-model="feedbackForm.message" 
                  rows="5" 
                  placeholder="Hãy viết ý kiến đóng góp chi tiết của bạn tại đây..." 
                  required 
                  class="form-textarea"
                ></textarea>
              </div>

              <button type="submit" :disabled="submitting" class="btn-submit">
                <span v-if="!submitting">Gửi Phản Hồi</span>
                <div class="spinner" v-else></div>
              </button>
            </form>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { 
  ArrowLeft, 
  MapPin, 
  Phone, 
  Mail, 
  MessageSquare,
  CheckCircle2,
  AlertCircle,
  Info
} from '@lucide/vue';

const submitting = ref(false);

const feedbackForm = reactive({
  fullName: '',
  email: '',
  subject: '',
  message: ''
});

// Toast notification State
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

const handleFeedbackSubmit = async () => {
  submitting.value = true;
  try {
    // Giả lập gửi feedback đóng góp ý kiến
    await new Promise(resolve => setTimeout(resolve, 1500));
    showSuccessToast('Cảm ơn bạn đã đóng góp ý kiến! Ý kiến của bạn đã được gửi tới Ban giám đốc phòng khám.');
    feedbackForm.fullName = '';
    feedbackForm.email = '';
    feedbackForm.subject = '';
    feedbackForm.message = '';
  } catch (err) {
    showErrorToast('Không thể gửi phản hồi lúc này. Vui lòng liên hệ hotline.');
  } finally {
    submitting.value = false;
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.contact-wrapper {
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

.contact-container {
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

.contact-header {
  text-align: center;
  margin-bottom: 4rem;
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

.contact-subtitle {
  color: #94a3b8;
  font-size: 1.05rem;
  line-height: 1.7;
  max-width: 700px;
  margin: 0 auto;
}

/* Grid layout for two columns */
.contact-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2.5rem;
  margin-bottom: 4rem;
}

@media (max-width: 868px) {
  .contact-grid {
    grid-template-columns: 1fr;
  }
}

.info-column {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.info-card, .zalo-card, .form-card {
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 24px;
  padding: 2.5rem;
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.25);
}

.info-card h3, .zalo-card h3, .form-card h3 {
  font-size: 1.35rem;
  font-weight: 700;
  color: white;
  margin-bottom: 1.5rem;
}

.details-list {
  display: flex;
  flex-direction: column;
  gap: 1.8rem;
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

.detail-item h4 {
  font-size: 1rem;
  font-weight: 600;
  color: white;
  margin-bottom: 0.3rem;
}

.detail-item p {
  font-size: 0.9rem;
  color: #94a3b8;
  line-height: 1.5;
}

.highlight-text {
  color: #14b8a6 !important;
  font-weight: 600;
  font-size: 1.05rem !important;
}

/* Zalo QR Code card specs */
.zalo-card {
  text-align: center;
}

.zalo-icon-box {
  width: 50px;
  height: 50px;
  border-radius: 14px;
  background: rgba(0, 104, 255, 0.15);
  color: #0068ff;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0 auto 1.2rem auto;
}

.zalo-icon {
  width: 24px;
  height: 24px;
}

.zalo-card p {
  font-size: 0.9rem;
  color: #94a3b8;
  line-height: 1.5;
  margin-bottom: 1.5rem;
}

.qr-wrapper {
  background: white;
  padding: 1rem;
  border-radius: 16px;
  display: inline-block;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  margin-bottom: 1.2rem;
}

.qr-img {
  width: 150px;
  height: 150px;
  display: block;
}

.zalo-phone {
  display: block;
  font-size: 0.95rem;
  font-weight: 600;
  color: #14b8a6;
}

/* Form layout specs */
.form-card p {
  font-size: 0.9rem;
  color: #94a3b8;
  margin-bottom: 2rem;
}

.feedback-form {
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.input-group label {
  color: #cbd5e1;
  font-size: 0.85rem;
  font-weight: 500;
}

.form-input, .form-textarea {
  width: 100%;
  padding: 0.85rem 1rem;
  background: rgba(15, 23, 42, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  color: white;
  font-family: inherit;
  font-size: 0.95rem;
  transition: all 0.3s;
}

.form-input:focus, .form-textarea:focus {
  outline: none;
  border-color: #14b8a6;
  box-shadow: 0 0 0 3px rgba(20, 184, 166, 0.15);
  background: rgba(15, 23, 42, 0.8);
}

.form-textarea {
  resize: vertical;
}

.btn-submit {
  background: linear-gradient(135deg, #14b8a6, #0d9488);
  color: white;
  border: none;
  border-radius: 12px;
  padding: 0.9rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 0.5rem;
}

.btn-submit:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(20, 184, 166, 0.3);
}

.btn-submit:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

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
