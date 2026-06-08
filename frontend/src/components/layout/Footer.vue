<template>
  <footer class="footer-premium">
    <div class="footer-container">
      <div class="footer-grid">
        <!-- Col 1: Brand Info -->
        <div class="footer-col col-brand">
          <h5 class="brand-title">
            <HeartPulse class="footer-brand-icon" />
            MyPet<span class="text-accent">Clinic</span>
          </h5>
          <p class="brand-desc">
            Bệnh viện thú y chất lượng hàng đầu Việt Nam. Nơi chăm sóc toàn diện, chẩn đoán chính xác và điều trị tận tâm bằng tình yêu thương lớn nhất dành cho thú cưng của bạn.
          </p>
          <div class="social-icons">
            <a href="https://facebook.com" target="_blank" class="social-btn">
              <svg class="social-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z"/></svg>
            </a>
            <a href="https://youtube.com" target="_blank" class="social-btn">
              <svg class="social-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22.54 6.42a2.78 2.78 0 0 0-1.94-2C18.88 4 12 4 12 4s-6.88 0-8.6.46a2.78 2.78 0 0 0-1.94 2A29 29 0 0 0 1 11.75a29 29 0 0 0 .46 5.33A2.78 2.78 0 0 0 3.4 19c1.72.46 8.6.46 8.6.46s6.88 0 8.6-.46a2.78 2.78 0 0 0 1.94-2 29 29 0 0 0 .46-5.25 29 29 0 0 0-.46-5.33z"/><polygon points="9.75 15.02 15.5 11.75 9.75 8.48 9.75 15.02"/></svg>
            </a>
            <a href="https://instagram.com" target="_blank" class="social-btn">
              <svg class="social-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="2" width="20" height="20" rx="5" ry="5"/><path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z"/><line x1="17.5" y1="6.5" x2="17.51" y2="6.5"/></svg>
            </a>
          </div>
        </div>

        <!-- Col 2: Locations & Hotlines -->
        <div class="footer-col col-contact">
          <h6 class="footer-title">Địa Chỉ Liên Hệ</h6>
          <div class="contact-details">
            <p>
              <strong>MyPetClinic (Trụ sở chính):</strong><br />
              124A Xuân Thủy, P. An Khánh, TP. HCM<br />
              <span class="d-flex align-items-center gap-1 mt-1 text-highlight">
                <Phone class="contact-icon" /> Hotline: 0905 090 629
              </span>
            </p>
          </div>
        </div>

        <!-- Col 3: Working Hours -->
        <div class="footer-col col-hours">
          <h6 class="footer-title">Giờ Làm Việc</h6>
          <div class="hours-details">
            <p class="mb-2">
              <strong>Thứ 2 - Thứ 7:</strong><br />
              Sáng: 08:00 - 12:00 | Chiều: 14:00 - 19:00
            </p>
            <p>
              <strong>Chủ nhật & Ngày Lễ:</strong><br />
              Sáng: 08:00 - 12:00 (Chiều nghỉ)
            </p>
          </div>
        </div>

        <!-- Col 4: Quick Map -->
        <div class="footer-col col-map">
          <h6 class="footer-title">Bản Đồ Chỉ Đường</h6>
          <div class="map-wrapper">
            <iframe 
              style="border: 0; width: 100%; height: 100%;" 
              src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3920.0531119583807!2d106.70744515079734!3d10.73038709231523!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752f91a2a96ad1%3A0xc19a98cc93ec317e!2sPetcare+Veterinary+Hospital!5e0!3m2!1sen!2s!4v1465265330727" 
              frameborder="0" 
              allowfullscreen="true">
            </iframe>
          </div>
          <p class="email-row">
            <a href="mailto:info@petcare.vn" class="email-link">
              <Mail class="contact-icon" /> info@petcare.vn
            </a>
          </p>
        </div>
      </div>

      <hr class="footer-divider" />
      
      <div class="footer-bottom">
        <div class="copyright">
          &copy; 2026 - <strong>MyPetClinic.vn</strong>. Tất cả quyền được bảo lưu.
        </div>
        <div class="policy-links">
          <a href="#" class="policy-link">Quy định sử dụng</a> | <a href="#" class="policy-link">Chính sách bảo mật</a>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { HeartPulse, Phone, Mail } from '@lucide/vue';
</script>

<style scoped>
.footer-premium {
  position: relative;
  width: 100%;
  margin-top: auto;
  background: linear-gradient(135deg, #fffdf2 0%, #fef3c7 100%);
  border-top: 1px solid rgba(0, 0, 0, 0.08);
  color: var(--text-dark);
  padding: 4rem 1.5rem 2rem 1.5rem;
}

.footer-container {
  max-width: 1200px;
  margin: 0 auto;
}

.footer-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 2.5rem;
}

.footer-col {
  display: flex;
  flex-direction: column;
}

.brand-title {
  font-size: 1.5rem;
  font-weight: 800;
  color: #1f2937 !important;
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 1rem;
}

.footer-brand-icon {
  width: 28px;
  height: 28px;
  color: var(--primary-gold);
}

.text-accent {
  color: var(--primary-gold);
}

.brand-desc {
  font-size: 0.9rem;
  line-height: 1.6;
  color: #4b5563 !important;
  margin-bottom: 1.5rem;
}

.social-icons {
  display: flex;
  gap: 8px;
}

.social-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: 1px solid #9ca3af;
  color: #4b5563 !important;
  transition: var(--transition-smooth);
}

.social-btn:hover {
  background-color: var(--primary-gold);
  border-color: var(--primary-gold);
  color: #fff !important;
}

.social-icon {
  width: 18px;
  height: 18px;
}

.footer-title {
  color: #1f2937 !important;
  font-weight: 700;
  font-size: 1.1rem;
  margin-bottom: 1.5rem;
  position: relative;
  padding-bottom: 8px;
}

.footer-title::after {
  content: '';
  position: absolute;
  left: 0;
  bottom: 0;
  width: 40px;
  height: 3px;
  background-color: var(--primary-gold);
  border-radius: 2px;
}

.contact-details p, .hours-details p {
  font-size: 0.9rem;
  line-height: 1.6;
  color: #4b5563 !important;
}

.text-highlight {
  color: #78350f !important;
  font-weight: 700;
}

.contact-icon {
  width: 16px;
  height: 16px;
  display: inline-block;
}

.map-wrapper {
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(0, 0, 0, 0.08);
  height: 150px;
  margin-bottom: 0.5rem;
}

.email-row {
  margin-top: 0.5rem;
  text-align: right;
}

.email-link {
  color: #78350f !important;
  font-weight: 600;
  text-decoration: none;
  font-size: 0.9rem;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.email-link:hover {
  color: var(--primary-gold) !important;
}

.footer-divider {
  border: 0;
  border-top: 1px solid rgba(0, 0, 0, 0.08);
  margin: 2rem 0;
}

.footer-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
  font-size: 0.85rem;
  color: #4b5563 !important;
}

.policy-links {
  display: flex;
  gap: 1rem;
}

.policy-link {
  color: #4b5563 !important;
  text-decoration: none;
}

.policy-link:hover {
  color: var(--primary-gold) !important;
}

@media (max-width: 768px) {
  .footer-bottom {
    flex-direction: column;
    text-align: center;
  }
  
  .email-row {
    text-align: left;
  }
}
</style>
