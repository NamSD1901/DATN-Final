<template>
  <div class="dashboard-layout">
    <!-- Sidebar -->
    <nav id="sidebar" :class="{ 'active': isSidebarActive }">
      <div class="sidebar-header text-center py-4 border-bottom">
        <router-link to="/" class="navbar-brand text-dark fs-3 text-decoration-none justify-content-center d-flex align-items-center gap-2">
          <i class="bi bi-heart-pulse-fill brand-accent"></i>
          MyPet<span class="brand-accent">Clinic</span>
        </router-link>
      </div>

      <ul class="list-unstyled components px-2 py-3">
        <li v-if="role === 'customer'" :class="{ 'active': activeTab === 'overview' }">
          <a href="#" @click.prevent="activeTab = 'overview'">
            <i class="bi bi-grid-1x2-fill text-warning"></i> {{ $t('sidebar.overview') }}
          </a>
        </li>

        <!-- Staff/Admin specific routes -->
        <template v-if="role === 'receptionist'">
          <li class="mt-4 mb-2 px-3 text-muted sidebar-section-title">Quản lý chuyên môn</li>
          <li :class="{ 'active': activeTab === 'queue' }">
            <a href="#" @click.prevent="activeTab = 'queue'"><i class="bi bi-kanban text-warning opacity-75"></i> Hàng khám</a>
          </li>
          <li :class="{ 'active': activeTab === 'customers' }">
            <a href="#" @click.prevent="activeTab = 'customers'"><i class="bi bi-people-fill text-warning opacity-75"></i> Khách hàng & Thú cưng</a>
          </li>
          <li :class="{ 'active': activeTab === 'appointments' }">
            <a href="#" @click.prevent="activeTab = 'appointments'"><i class="bi bi-calendar-check-fill text-warning opacity-75"></i> Quản lý Lịch hẹn</a>
          </li>
        </template>

        <template v-if="role === 'doctor'">
          <li class="mt-4 mb-2 px-3 text-muted sidebar-section-title text-nowrap text-truncate">Quản lý chuyên môn</li>
          <li :class="{ 'active': activeTab === 'doctor-cases' || activeTab === 'medical-records' }">
            <a href="#" class="text-nowrap text-truncate" @click.prevent="activeTab = 'doctor-cases'" title="Ca khám & Bệnh án">
              <i class="bi bi-person-lines-fill text-warning opacity-75"></i> Ca khám & Bệnh án
            </a>
          </li>
        </template>

        <template v-if="false">
          <li class="mt-4 mb-2 px-3 text-muted sidebar-section-title text-nowrap text-truncate">Quản lý lâm sàng</li>
          <li :class="{ 'active': activeTab === 'doctor-cases' || activeTab === 'medical-records' }">
            <a href="#" class="text-nowrap text-truncate" @click.prevent="activeTab = 'doctor-cases'" title="Ca khám & Bệnh án">
              <i class="bi bi-heart-pulse-fill text-warning opacity-75"></i> Ca khám & Bệnh án
            </a>
          </li>
        </template>

        <template v-if="role === 'receptionist'">
          <li :class="{ 'active': activeTab === 'invoices' }">
            <a href="#" @click.prevent="activeTab = 'invoices'"><i class="bi bi-receipt-cutoff text-warning opacity-75"></i> Quản lý Hóa đơn</a>
          </li>
        </template>

        <template v-if="role === 'admin'">
          <li class="mt-4 mb-2 px-3 text-muted sidebar-section-title">Quản trị hệ thống</li>
          <li :class="{ 'active': activeTab === 'staff' }">
            <a href="#" @click.prevent="activeTab = 'staff'"><i class="bi bi-person-badge-fill text-warning opacity-75"></i> Nhân sự</a>
          </li>
          <li :class="{ 'active': activeTab === 'services-admin' }">
            <a href="#" @click.prevent="activeTab = 'services-admin'"><i class="bi bi-box-seam-fill text-warning opacity-75"></i> Quản lý Dịch vụ</a>
          </li>
          <li :class="{ 'active': activeTab === 'settings-admin' }">
            <a href="#" @click.prevent="activeTab = 'settings-admin'"><i class="bi bi-gear-wide-connected text-warning opacity-75"></i> Cấu hình Phòng khám</a>
          </li>
          <li :class="{ 'active': activeTab === 'schedules-admin' }">
            <a href="#" @click.prevent="activeTab = 'schedules-admin'"><i class="bi bi-clock-fill text-warning opacity-75"></i> Lịch trực Bác sĩ</a>
          </li>
          <li :class="{ 'active': activeTab === 'medicines-admin' }">
            <a href="#" @click.prevent="activeTab = 'medicines-admin'"><i class="bi bi-capsule text-warning opacity-75"></i> Quản lý Kho thuốc</a>
          </li>
          <li :class="{ 'active': activeTab === 'vaccines-admin' }">
            <a href="#" @click.prevent="activeTab = 'vaccines-admin'"><i class="bi bi-droplet-half text-warning opacity-75"></i> Quản lý Vắc-xin</a>
          </li>

          <li :class="{ 'active': activeTab === 'reports-admin' }">
            <a href="#" @click.prevent="activeTab = 'reports-admin'"><i class="bi bi-graph-up-arrow text-warning opacity-75"></i> Báo cáo doanh thu</a>
          </li>
          <li :class="{ 'active': activeTab === 'reviews-admin' }">
            <a href="#" @click.prevent="activeTab = 'reviews-admin'"><i class="bi bi-star-fill text-warning opacity-75"></i> Quản lý Đánh giá</a>
          </li>
          <li :class="{ 'active': activeTab === 'offers-admin' }">
            <a href="#" @click.prevent="activeTab = 'offers-admin'"><i class="bi bi-ticket-perforated-fill text-warning opacity-75"></i> Quản lý Khuyến mãi</a>
          </li>
          <li :class="{ 'active': activeTab === 'blog-admin' }">
            <a href="#" @click.prevent="activeTab = 'blog-admin'"><i class="bi bi-journal-text text-warning opacity-75"></i> Quản lý bài viết</a>
          </li>

        </template>

        <!-- Customer specific routes -->
        <template v-if="role === 'customer'">
          <li class="mt-4 mb-2 px-3 text-muted sidebar-section-title">{{ $t('sidebar.myServices') }}</li>
          <li :class="{ 'active': activeTab === 'my-pets' }">
            <a href="#" @click.prevent="activeTab = 'my-pets'"><i class="bi bi-heptagon-fill text-warning opacity-75"></i> {{ $t('sidebar.myPets') }}</a>
          </li>
          <li :class="{ 'active': activeTab === 'my-appointments' }">
            <a href="#" @click.prevent="activeTab = 'my-appointments'"><i class="bi bi-calendar-check-fill text-warning opacity-75"></i> {{ $t('sidebar.myAppointments') }}</a>
          </li>
          <li :class="{ 'active': activeTab === 'my-history' }">
            <a href="#" @click.prevent="activeTab = 'my-history'"><i class="bi bi-clock-history text-warning opacity-75"></i> {{ $t('sidebar.myHistory') }}</a>
          </li>
          <li :class="{ 'active': activeTab === 'my-services-invoices' }">
            <a href="#" @click.prevent="activeTab = 'my-services-invoices'"><i class="bi bi-receipt text-warning opacity-75"></i> {{ $t('sidebar.myInvoices') }}</a>
          </li>

        </template>
      </ul>

      <!-- Sidebar Footer Actions -->
      <div class="sidebar-footer p-3 border-top mt-auto">
        <button v-if="role !== 'doctor' && role !== 'admin'" class="btn btn-premium w-100 mb-3 py-2 fw-bold shadow-sm rounded-4" @click="handleSidebarBookNew">
          <i class="bi bi-plus-circle-fill me-1"></i> {{ role === 'customer' ? $t('sidebar.bookNew') : 'Đặt Lịch Mới' }}
        </button>

        <button v-if="role === 'customer'" @click="activeTab = 'settings'" class="btn btn-outline-secondary w-100 mb-2 border-0 text-start ps-4 rounded-4 hover-text-warning py-2 fw-bold" :class="{'bg-light text-warning': activeTab === 'settings'}" style="font-size: 0.95em;">
          <i class="bi bi-gear-fill text-warning opacity-75 me-2 fs-5 align-middle"></i> {{ $t('common.settings') }}
        </button>

        <router-link to="/" class="btn btn-outline-secondary w-100 mb-2 border-0 text-start ps-4 rounded-4 hover-text-warning py-2 fw-bold" style="font-size: 0.95em;">
          <i class="bi bi-house-door-fill text-warning opacity-75 me-2 fs-5 align-middle"></i> {{ role === 'customer' ? $t('sidebar.home') : 'Trang chủ' }}
        </router-link>
        <button @click="handleLogout" class="btn btn-outline-danger w-100 border-0 text-start ps-4 rounded-4 py-2 fw-bold" style="font-size: 0.95em;">
          <i class="bi bi-door-closed-fill text-danger opacity-75 me-2 fs-5 align-middle"></i> {{ role === 'customer' ? $t('sidebar.logout') : 'Đăng xuất' }}
        </button>
      </div>
    </nav>

    <!-- Page Content Container -->
    <div id="content" class="flex-grow-1 d-flex flex-column">
      
      <!-- Topbar Header -->
      <div class="topbar d-flex justify-content-between align-items-center p-3 mb-4 bg-white shadow-sm rounded-4">
        <div>
          <h5 class="mb-0 fw-bold text-dark d-none d-md-block">{{ getTitle }}</h5>
          <button type="button" @click="isSidebarActive = !isSidebarActive" class="btn btn-light d-md-none border-0 rounded-circle shadow-sm">
            <i class="bi bi-list fs-4"></i>
          </button>
        </div>

        <div class="d-flex align-items-center gap-4">
          <!-- Notifications Dropdown -->
          <div class="nav-icon">
            <NotificationBell />
          </div>

          <!-- User Profile Details -->
          <div class="d-flex align-items-center gap-2">
            <img :src="avatarUrl" @error="handleAvatarError" alt="Avatar" class="rounded-circle shadow-sm border" width="45" height="45" style="object-fit: cover;">
            <div class="d-none d-md-block text-dark text-start">
              <span class="d-block fw-bold small">{{ userName }}</span>
              <span class="d-block text-muted" style="font-size: 0.75rem;">{{ getRoleLabel }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Tab Content -->
      <div class="flex-grow-1">
        <Transition name="fade" mode="out-in">
          
          <!-- tab: Overview Tab -->
          <div v-if="activeTab === 'overview'" class="container-fluid p-0">
            <!-- For Customer Role -->
            <template v-if="role === 'customer'">
              <CustomerOverviewTab @switch-tab="activeTab = $event" />
            </template>
            
            <!-- For Admin/Staff Roles -->
            <template v-else>
              <div class="row g-4">
                <!-- Admin Quick Info (Admin Only) -->
                <div class="col-lg-4 col-md-6">
                  <div class="card border-0 shadow-sm rounded-4 h-100 p-4 bg-white">
                    <h5 class="fw-bold mb-3 text-dark"><i class="bi bi-shield-check text-warning me-2"></i>Trạng Thái Hệ Thống</h5>
                    <p class="text-muted small mb-3">Tất cả các cổng dịch vụ hiện đang hoạt động bình thường ở môi trường Development.</p>
                    <div class="mt-auto">
                      <span class="badge bg-success px-3 py-2 rounded-pill fw-bold text-uppercase shadow-sm">
                        <i class="bi bi-cpu me-1"></i> Online
                      </span>
                    </div>
                  </div>
                </div>

                <!-- Quick Action Card -->
                <div class="col-lg-4 col-md-6">
                  <div class="card border-0 shadow-sm rounded-4 h-100 p-4 bg-gold-gradient text-white position-relative overflow-hidden">
                    <div class="position-absolute top-0 end-0 p-3 opacity-25">
                      <i class="bi bi-calendar-plus-fill" style="font-size: 8rem;"></i>
                    </div>
                    <h4 class="fw-bold mb-3 position-relative z-index-1">Đặt Lịch Hẹn Mới</h4>
                    <p class="mb-4 position-relative z-index-1 opacity-75">Tiết kiệm thời gian chờ đợi. Đăng ký trước lịch khám, tiêm phòng hoặc spa cho thú cưng của bạn ngay hôm nay.</p>
                    <div class="mt-auto position-relative z-index-1">
                      <button @click="handleSidebarBookNew" class="btn btn-light text-warning fw-bold px-4 py-2 rounded-pill shadow-sm">
                        Đặt Lịch Ngay <i class="bi bi-arrow-right ms-1"></i>
                      </button>
                    </div>
                  </div>
                </div>

                <!-- Stats Summary Cards -->
                <div class="col-lg-4 col-md-12">
                  <div class="row g-3 h-100">
                    <div class="col-md-6 col-lg-12">
                      <div class="card border-0 shadow-sm rounded-4 p-3 d-flex flex-row align-items-center h-100 bg-white">
                        <div class="bg-warning bg-opacity-10 text-warning p-3 rounded-circle me-3">
                          <i class="bi bi-heptagon-fill fs-3"></i>
                        </div>
                        <div>
                          <h6 class="text-muted mb-1 small">Tổng số thú cưng</h6>
                          <h4 class="fw-bold text-dark mb-0">-- <span class="fs-6 fw-normal text-muted">bé</span></h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-6 col-lg-12">
                      <div class="card border-0 shadow-sm rounded-4 p-3 d-flex flex-row align-items-center h-100 bg-white">
                        <div class="bg-success bg-opacity-10 text-success p-3 rounded-circle me-3">
                          <i class="bi bi-calendar-check-fill fs-3"></i>
                        </div>
                        <div>
                          <h6 class="text-muted mb-1 small">Lịch hẹn hôm nay</h6>
                          <h4 class="fw-bold text-dark mb-0">-- <span class="fs-6 fw-normal text-muted">lịch</span></h4>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Recent Appointments Table List -->
              <div class="row mt-4 g-4">
                <div class="col-12">
                  <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
                    <div class="d-flex justify-content-between align-items-center mb-4">
                      <h5 class="fw-bold text-dark mb-0">Lịch sử khám gần đây</h5>
                      <a href="#" class="text-warning text-decoration-none small fw-bold hover-underline">Xem tất cả <i class="bi bi-arrow-right"></i></a>
                    </div>
                    <div class="table-responsive">
                      <table class="table align-middle border-bottom mb-0">
                        <thead class="table-light">
                          <tr>
                            <th class="border-0 text-muted small py-3">Ngày khám</th>
                            <th class="border-0 text-muted small py-3">Thú cưng</th>
                            <th class="border-0 text-muted small py-3">Dịch vụ</th>
                            <th class="border-0 text-muted small py-3">Bác sĩ</th>
                            <th class="border-0 text-muted small py-3">Trạng thái</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td colspan="5" class="text-center py-5 text-muted">
                              <i class="bi bi-folder2-open fs-1 d-block mb-2 text-black-50 opacity-50"></i>
                              Chưa có dữ liệu lịch sử khám
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </div>
                </div>
              </div>
            </template>
          </div>



          <!-- tab: Settings Tab (Customer) -->
          <div v-else-if="activeTab === 'settings'" class="container-fluid p-0">
            <SettingsTab @profile-updated="fetchDashboardData" />
          </div>

          <!-- tab: Queue Tab -->
          <div v-else-if="activeTab === 'queue'" class="container-fluid p-0">
            <QueueTab @switch-tab="activeTab = $event" @select-invoice="handleSelectInvoice" />
          </div>

          <!-- tab: Customers Tab -->
          <div v-else-if="activeTab === 'customers'" class="container-fluid p-0">
            <CustomersTab />
          </div>

          <!-- tab: Appointments Tab -->
          <div v-else-if="activeTab === 'appointments'" class="container-fluid p-0">
            <AppointmentsTab ref="appointmentsTabRef" />
          </div>

          <!-- tab: Invoices Tab -->
          <div v-else-if="activeTab === 'invoices'" class="container-fluid p-0">
            <InvoicesTab :initial-appointment-id="selectedInvoiceId" />
          </div>

          <!-- tab: My Pets Tab (Customer) -->
          <div v-else-if="activeTab === 'my-pets'" class="container-fluid p-0">
            <MyPetsTab @view-pet="handleViewPetProfile" />
          </div>

          <!-- tab: Pet Profile (Customer) -->
          <div v-else-if="activeTab === 'pet-profile'" class="container-fluid p-0">
            <PetProfile 
              :pet-id="viewingPetId || undefined" 
              @go-back="activeTab = 'my-pets'" 
              @book-appointment="handleBookFromProfile"
            />
          </div>

          <!-- tab: My Appointments Tab (Customer) -->
          <div v-else-if="activeTab === 'my-appointments'" class="container-fluid p-0">
            <MyAppointmentsTab ref="myAppointmentsTabRef" @switch-tab="activeTab = $event" />
          </div>

          <!-- tab: My History Tab (Customer) -->
          <div v-else-if="activeTab === 'my-history'" class="container-fluid p-0">
            <MyHistoryTab />
          </div>

          <!-- tab: My Services & Invoices Tab (Customer) -->
          <div v-else-if="activeTab === 'my-services-invoices'" class="container-fluid p-0">
            <MyServicesInvoicesTab />
          </div>

          <!-- tab: Customer Offers Tab -->
          <div v-else-if="activeTab === 'customer-offers'" class="container-fluid p-0">
            <CustomerOffersTab />
          </div>

          <!-- tab: Doctor Cases Tab -->
          <div v-else-if="activeTab === 'doctor-cases'" class="container-fluid p-0">
            <DoctorQueueTab @switch-tab="activeTab = $event" />
          </div>

          <!-- tab: Medical Records Tab -->
          <div v-else-if="activeTab === 'medical-records'" class="container-fluid p-0">
            <MedicalRecordsTab @switch-tab="activeTab = $event" />
          </div>

          <!-- tab: Staff Management Tab -->
          <div v-else-if="activeTab === 'staff'" class="container-fluid p-0">
            <StaffTab />
          </div>

          <!-- tab: Services Admin Tab -->
          <div v-else-if="activeTab === 'services-admin'" class="container-fluid p-0">
            <ServicesAdminTab />
          </div>

          <!-- tab: Settings Admin Tab -->
          <div v-else-if="activeTab === 'settings-admin'" class="container-fluid p-0">
            <SettingsAdminTab />
          </div>

          <!-- tab: Medicines Admin Tab -->
          <div v-else-if="activeTab === 'medicines-admin'" class="container-fluid p-0">
            <MedicinesAdminTab />
          </div>

          <!-- tab: Vaccines Admin Tab -->
          <div v-else-if="activeTab === 'vaccines-admin'" class="container-fluid p-0">
            <VaccinesAdminTab />
          </div>

          <!-- tab: Schedules Admin Tab -->
          <div v-else-if="activeTab === 'schedules-admin'" class="container-fluid p-0">
            <SchedulesAdminTab />
          </div>

          <!-- tab: Reports Admin Tab -->
          <div v-else-if="activeTab === 'reports-admin'" class="container-fluid p-0">
            <ReportsAdminTab />
          </div>

          <!-- tab: Blog Admin Tab -->
          <div v-else-if="activeTab === 'blog-admin'" class="container-fluid p-0">
            <BlogAdminTab />
          </div>

          <!-- tab: Reviews Admin Tab -->
          <div v-else-if="activeTab === 'reviews-admin'" class="container-fluid p-0">
            <ReviewsAdminTab />
          </div>

          <!-- tab: Offers Admin Tab -->
          <div v-else-if="activeTab === 'offers-admin'" class="container-fluid p-0">
            <OffersAdminTab />
          </div>

        </Transition>
      </div>

      <!-- Footer copyright message -->
      <div class="text-center text-muted small mt-5 pt-3 border-top py-3">
        &copy; 2026 MyPetClinic.vn Dashboard
      </div>
    </div>

    <!-- QR Code Modal Dialog -->
    <div v-if="showQrModal" class="zalo-modal-overlay" @click.self="showQrModal = false">
      <div class="zalo-modal-card">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title"><i class="bi bi-qr-code me-2"></i> Thẻ QR Check-in</h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showQrModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body">
          <p class="modal-desc">
            Đưa mã này cho lễ tân khi bạn đến bệnh viện để check-in nhanh.
          </p>
          <div class="qr-wrapper mb-4">
            <img :src="`https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${userId}`" alt="Check-in QR Code" />
          </div>
          <h5 class="fw-bold text-dark mb-1">{{ userName }}</h5>
          <p class="text-muted small mb-0">{{ getRoleLabel }}</p>
        </div>
      </div>
    </div>

    <!-- Shared Booking Modal Component -->
    <BookingModal 
      :show="showBookingModal" 
      @close="showBookingModal = false" 
      @success="handleBookingSuccess" 
      @error="handleBookingError" 
    />

    <!-- Auto Review Modal -->
    <ReviewModal
      :is-open="showAutoReviewModal"
      :initial-data="{ appointmentId: autoReviewApptId }"
      @close="onAutoReviewClosed"
      @submit="onAutoReviewSubmitted"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import api from '../services/api';
import BookingModal from '../components/shared/BookingModal.vue';
import ReviewModal from '../components/shared/ReviewModal.vue';
import NotificationBell from '../components/layout/NotificationBell.vue';
import { useReviewStore } from '../stores/review.store';
import confetti from 'canvas-confetti';
import CustomerOverviewTab from '../components/dashboard/CustomerOverviewTab.vue';
import QueueTab from '../components/dashboard/QueueTab.vue';
import CustomersTab from '../components/dashboard/CustomersTab.vue';
import AppointmentsTab from '../components/dashboard/AppointmentsTab.vue';
import InvoicesTab from '../components/dashboard/InvoicesTab.vue';
import MyPetsTab from '../components/dashboard/MyPetsTab.vue';
import PetProfile from '../views/PetProfile.vue';
import MyAppointmentsTab from '../components/dashboard/MyAppointmentsTab.vue';
import MyHistoryTab from '../components/dashboard/MyHistoryTab.vue';
import MyServicesInvoicesTab from '../components/dashboard/MyServicesInvoicesTab.vue';
import DoctorQueueTab from '../components/dashboard/DoctorQueueTab.vue';
import MedicalRecordsTab from '../components/dashboard/MedicalRecordsTab.vue';
import StaffTab from '../components/dashboard/StaffTab.vue';
import ServicesAdminTab from '../components/dashboard/ServicesAdminTab.vue';
import SettingsAdminTab from '../components/dashboard/SettingsAdminTab.vue';
import MedicinesAdminTab from '../components/dashboard/MedicinesAdminTab.vue';
import VaccinesAdminTab from '../components/dashboard/VaccinesAdminTab.vue';
import SchedulesAdminTab from '../components/dashboard/SchedulesAdminTab.vue';
import ReportsAdminTab from '../components/dashboard/ReportsAdminTab.vue';
import BlogAdminTab from '../components/dashboard/BlogAdminTab.vue';
import ReviewsAdminTab from '../components/dashboard/ReviewsAdminTab.vue';
import OffersAdminTab from '../components/dashboard/OffersAdminTab.vue';
import CustomerOffersTab from '../components/dashboard/CustomerOffersTab.vue';
import SettingsTab from '../components/dashboard/SettingsTab.vue';

const router = useRouter();
const route = useRoute();

const { t, locale } = useI18n();

// Tab state: 'overview' | 'profile' | 'queue' | 'customers' | 'appointments' | 'invoices'
const activeTab = ref<string>('overview');
const selectedInvoiceId = ref<number | undefined>(undefined);
const viewingPetId = ref<number | string | null>(null);

const handleViewPetProfile = (petId: string | number) => {
  viewingPetId.value = petId;
  activeTab.value = 'pet-profile';
};

const handleBookFromProfile = (data: { petId: number, serviceName: string }) => {
  activeTab.value = 'my-appointments';
  let attempts = 0;
  const checkInterval = setInterval(() => {
    if (myAppointmentsTabRef.value && typeof myAppointmentsTabRef.value.openBookModal === 'function') {
      myAppointmentsTabRef.value.openBookModal(data.petId, data.serviceName);
      clearInterval(checkInterval);
    }
    attempts++;
    if (attempts > 20) {
      clearInterval(checkInterval);
    }
  }, 100);
};

const handleSelectInvoice = (id: number) => {
  selectedInvoiceId.value = id;
};

const isSidebarActive = ref(false);
const showBookingModal = ref(false);
const showQrModal = ref(false);
const myAppointmentsTabRef = ref<any>(null);

const reviewStore = useReviewStore();
const showAutoReviewModal = ref(false);
const autoReviewApptId = ref(0);
const appointmentsTabRef = ref<any>(null);

const handleSidebarBookNew = () => {
  if (role.value.toLowerCase() === 'customer') {
    activeTab.value = 'my-appointments';
    let attempts = 0;
    const checkInterval = setInterval(() => {
      if (myAppointmentsTabRef.value && typeof myAppointmentsTabRef.value.openBookModal === 'function') {
        myAppointmentsTabRef.value.openBookModal();
        clearInterval(checkInterval);
      }
      attempts++;
      if (attempts > 20) {
        clearInterval(checkInterval);
      }
    }, 100);
  } else if (role.value.toLowerCase() === 'receptionist' || role.value.toLowerCase() === 'admin') {
    activeTab.value = 'appointments';
    let attempts = 0;
    const checkInterval = setInterval(() => {
      if (appointmentsTabRef.value && typeof appointmentsTabRef.value.openCreateModal === 'function') {
        appointmentsTabRef.value.openCreateModal();
        clearInterval(checkInterval);
      }
      attempts++;
      if (attempts > 20) {
        clearInterval(checkInterval);
      }
    }, 100);
  } else {
    showBookingModal.value = true;
  }
};

// User context
const userId = ref('');
const userName = ref('User');
const email = ref('');
const role = ref('customer');
const phone = ref('');
const address = ref('');
const avatarUrl = ref('');

const getTitle = computed(() => {
  if (role.value === 'customer') {
    if (activeTab.value === 'overview') return t('pageTitle.overview');
    if (activeTab.value === 'profile') return t('pageTitle.profile');
    if (activeTab.value === 'my-pets') return t('pageTitle.myPets');
    if (activeTab.value === 'pet-profile') return t('pageTitle.petProfile');
    if (activeTab.value === 'my-appointments') return t('pageTitle.myAppointments');
    if (activeTab.value === 'my-history') return t('pageTitle.myHistory');
    if (activeTab.value === 'my-services-invoices') return t('pageTitle.myServicesInvoices');
    if (activeTab.value === 'customer-offers') return 'Kho Voucher của tôi';
    if (activeTab.value === 'settings') return t('common.settings');
  } else {
    if (activeTab.value === 'overview') return 'Tổng quan hệ thống';
    if (activeTab.value === 'profile') return 'Thông tin cá nhân';
  }

  if (activeTab.value === 'queue') return 'Hàng khám - Digital Whiteboard';
  if (activeTab.value === 'customers') return 'Quản lý Khách hàng & Thú cưng';
  if (activeTab.value === 'appointments') return 'Quản lý Lịch hẹn & Điều phối';
  if (activeTab.value === 'invoices') return 'Quản lý Hóa đơn & Thu ngân';
  if (activeTab.value === 'doctor-cases') return 'Hàng khám của tôi';
  if (activeTab.value === 'medical-records') return 'Hồ sơ bệnh án & Khám bệnh';
  if (activeTab.value === 'staff') return 'Quản lý Nhân sự';
  if (activeTab.value === 'services-admin') return 'Quản lý Dịch vụ & Giá cả';
  if (activeTab.value === 'settings-admin') return 'Cấu hình Khung giờ làm việc';
  if (activeTab.value === 'medicines-admin') return 'Quản lý Kho thuốc & Dược phẩm';
  if (activeTab.value === 'vaccines-admin') return 'Quản lý Vắc-xin & Lô nhập';
  if (activeTab.value === 'schedules-admin') return 'Quản lý Ca trực Bác sĩ';
  if (activeTab.value === 'reports-admin') return 'Báo cáo Doanh thu & Hiệu suất';
  if (activeTab.value === 'reviews-admin') return 'Quản lý Đánh giá';
  if (activeTab.value === 'offers-admin') return 'Quản lý Khuyến mãi (Voucher)';
  if (activeTab.value === 'blog-admin') return 'Quản trị Bài viết & Tin tức';
  return role.value === 'customer' ? t('pageTitle.dashboard') : 'Bảng điều khiển';
});

const getRoleLabel = computed(() => {
  const currentRole = role.value.toLowerCase();
  if (currentRole === 'admin') return 'Quản trị viên';
  if (currentRole === 'doctor') return 'Bác sĩ thú y';
  if (currentRole === 'receptionist') return 'Lễ tân';
  return t('role.customer');
});

const fetchDashboardData = async () => {
  try {
    const res = await api.get('/dashboard');
    userId.value = res.data.userId || '';
    userName.value = res.data.userName || 'Người dùng';
    email.value = res.data.email || '';
    
    // Normalize doctor roles for UI
    let fetchedRole = res.data.role || 'customer';
    if (fetchedRole === 'clinical_doctor' || fetchedRole === 'vaccination_doctor') {
      fetchedRole = 'doctor';
    }
    role.value = fetchedRole;
    localStorage.setItem('user_role', role.value);
    
    if (role.value === 'customer') {
      checkPendingReviews();
    }
    
    if (role.value === 'receptionist' && activeTab.value === 'overview') {
      activeTab.value = 'queue';
    } else if (role.value === 'doctor' && activeTab.value === 'overview') {
      activeTab.value = 'doctor-cases';
    } else if (role.value === 'admin' && activeTab.value === 'overview') {
      activeTab.value = 'staff';
    }

    // Fetch details to get avatar, phone, address
    const profileRes = await api.get('/profile');
    phone.value = profileRes.data.phone || '';
    address.value = profileRes.data.address || '';
    
    const avatarPath = profileRes.data.avatar;
    const backendUrl = 'https://localhost:7284';
    if (avatarPath) {
      avatarUrl.value = avatarPath.startsWith('http') ? avatarPath : `${backendUrl}${avatarPath}`;
    } else {
      avatarUrl.value = `https://ui-avatars.com/api/?name=${encodeURIComponent(userName.value)}&background=f59e0b&color=fff&rounded=true`;
    }

    if (route.query.action === 'book' && role.value === 'customer') {
      handleSidebarBookNew();
    }
  } catch (err) {
    console.error('Lỗi khi lấy dữ liệu dashboard:', err);
    router.push('/login');
  }
};

const handleAvatarError = () => {
  avatarUrl.value = `https://ui-avatars.com/api/?name=${encodeURIComponent(userName.value || 'User')}&background=f59e0b&color=fff&rounded=true`;
};

const handleLogout = async () => {
  try {
    await api.post('/account/logout');
    router.push('/login');
  } catch (error) {
    console.error('Lỗi khi đăng xuất:', error);
  }
};

const handleBookingSuccess = (msg: string) => {
  alert(msg);
};

const handleBookingError = (msg: string) => {
  alert(msg);
};

const triggerConfetti = () => {
  const duration = 2.5 * 1000;
  const animationEnd = Date.now() + duration;
  const defaults = { startVelocity: 30, spread: 360, ticks: 60, zIndex: 1200 };

  function randomInRange(min: number, max: number) {
    return Math.random() * (max - min) + min;
  }

  const interval: any = setInterval(function() {
    const timeLeft = animationEnd - Date.now();
    if (timeLeft <= 0) {
      return clearInterval(interval);
    }
    const particleCount = 40 * (timeLeft / duration);
    confetti(Object.assign({}, defaults, { particleCount, origin: { x: randomInRange(0.1, 0.3), y: Math.random() - 0.2 } }));
    confetti(Object.assign({}, defaults, { particleCount, origin: { x: randomInRange(0.7, 0.9), y: Math.random() - 0.2 } }));
  }, 250);
};

const onAutoReviewClosed = () => {
  showAutoReviewModal.value = false;
  try {
    const dismissedReviewsStr = localStorage.getItem('dismissedReviews');
    const dismissedReviews: number[] = dismissedReviewsStr ? JSON.parse(dismissedReviewsStr) : [];
    if (!dismissedReviews.includes(autoReviewApptId.value)) {
      dismissedReviews.push(autoReviewApptId.value);
      localStorage.setItem('dismissedReviews', JSON.stringify(dismissedReviews));
    }
  } catch (err) {
    console.error("Lỗi lưu trạng thái dismiss review", err);
  }
};

const onAutoReviewSubmitted = async (data: any) => {
  try {
    await reviewStore.submitReview({
      appointmentId: data.appointmentId,
      rating: data.rating,
      comment: data.comment,
      imageUrls: data.imageUrls
    });
    showAutoReviewModal.value = false;
    
    try {
      const dismissedReviewsStr = localStorage.getItem('dismissedReviews');
      const dismissedReviews: number[] = dismissedReviewsStr ? JSON.parse(dismissedReviewsStr) : [];
      if (!dismissedReviews.includes(data.appointmentId)) {
        dismissedReviews.push(data.appointmentId);
        localStorage.setItem('dismissedReviews', JSON.stringify(dismissedReviews));
      }
    } catch (e) {}

    confetti({
      particleCount: 150,
      spread: 70,
      origin: { y: 0.6 },
      zIndex: 1200
    });
  } catch (err: any) {
    console.error('Lỗi khi gửi đánh giá', err);
    if (err.response?.data?.message?.includes("đã được đánh giá")) {
      showAutoReviewModal.value = false;
      try {
        const dismissedReviewsStr = localStorage.getItem('dismissedReviews');
        const dismissedReviews: number[] = dismissedReviewsStr ? JSON.parse(dismissedReviewsStr) : [];
        if (!dismissedReviews.includes(data.appointmentId)) {
          dismissedReviews.push(data.appointmentId);
          localStorage.setItem('dismissedReviews', JSON.stringify(dismissedReviews));
        }
      } catch (e) {}
    } else {
      alert(err.response?.data?.message || "Lỗi khi gửi đánh giá. Vui lòng thử lại!");
    }
  }
};

const checkPendingReviews = async () => {
  try {
    const res = await api.get('/my-appointments?page=1&pageSize=50');
    const appointments = res.data.items || res.data || [];
    
    await reviewStore.fetchMyReviews(1);
    
    const dismissedReviewsStr = localStorage.getItem('dismissedReviews');
    const dismissedReviews: number[] = dismissedReviewsStr ? JSON.parse(dismissedReviewsStr) : [];

    const completedAppts = appointments
      .filter((a: any) => a.status === 'completed')
      .sort((a: any, b: any) => new Date(b.appointmentDate).getTime() - new Date(a.appointmentDate).getTime());
      
    if (completedAppts.length > 0) {
      const latestAppt = completedAppts[0];
      const hasReviewed = reviewStore.myReviews.some(r => r.appointmentId == latestAppt.id);
      
      if (!hasReviewed && !dismissedReviews.includes(latestAppt.id)) {
        autoReviewApptId.value = latestAppt.id;
        
        setTimeout(() => {
          showAutoReviewModal.value = true;
          triggerConfetti();
        }, 800);
      }
    }
  } catch (e) {
    console.error("Lỗi khi tải lịch sử đánh giá auto:", e);
  }
};

onMounted(() => {
  fetchDashboardData();
});
</script>

<style scoped>
.dashboard-layout {
  display: flex;
  width: 100%;
  align-items: stretch;
  min-height: 100vh;
  background-color: #f4f6f9;
}

#sidebar {
  min-width: 260px;
  max-width: 260px;
  background: #ffffff;
  color: #333;
  transition: all 0.3s;
  box-shadow: 2px 0 10px rgba(0,0,0,0.05);
  display: flex;
  flex-direction: column;
  position: sticky;
  top: 0;
  height: 100vh;
  align-self: flex-start;
  overflow-y: auto;
  z-index: 1000;
}

#sidebar .sidebar-header {
  border-bottom: 1px solid #f0f0f0;
}

#sidebar ul.components {
  flex-grow: 1;
}

#sidebar ul li {
  margin-bottom: 5px;
}

#sidebar ul li a {
  padding: 10px 20px;
  margin: 4px 15px;
  font-size: 0.95em;
  font-weight: 500;
  display: block;
  color: #4a4a4a;
  text-decoration: none;
  transition: 0.3s;
  border-radius: 12px;
}

#sidebar ul li a:hover, #sidebar ul li.active > a {
  color: var(--primary-gold);
  background: #fdfaf0;
  font-weight: 700;
}

#sidebar ul li a i {
  margin-right: 12px;
  font-size: 1.15em;
  display: inline-block;
  width: 22px;
  text-align: center;
  transition: 0.3s;
}

#sidebar ul li a:hover i, #sidebar ul li.active > a i {
  transform: scale(1.1);
}

.sidebar-section-title {
  font-size: 0.8rem;
  font-weight: bold;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.brand-accent {
  color: var(--primary-gold);
}

#content {
  width: 100%;
  padding: 25px;
  min-height: 100vh;
}

.topbar {
  border-radius: 12px;
}

.nav-icon {
  font-size: 1.35rem;
  cursor: pointer;
  position: relative;
}

.nav-icon:hover {
  color: var(--primary-gold);
}

/* responsive classes matching site.css */
.border-end-md {
  border-right: 1px solid var(--border-color);
}

@media (max-width: 768px) {
  #sidebar {
    margin-left: -260px;
    position: fixed;
  }
  #sidebar.active {
    margin-left: 0;
  }
  .border-end-md {
    border-right: none;
    border-bottom: 1px solid var(--border-color);
    padding-bottom: 2rem;
  }
}

/* Zalo QR Modal overlay */
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
  max-width: 400px;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
}

.zalo-modal-header {
  padding: 1.2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
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
  border: 2px dashed #f59e0b;
  border-radius: 12px;
  display: inline-block;
  box-shadow: var(--shadow-sm);
  background: #fdfaf0;
}

.qr-wrapper img {
  width: 180px;
  height: 180px;
}

/* transitions */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.25s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
