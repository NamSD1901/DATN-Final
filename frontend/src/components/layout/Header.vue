<template>
  <header class="sticky-top">
    <nav class="navbar-premium">
      <div class="nav-container">
        <!-- Brand Brand -->
        <router-link to="/" class="navbar-brand">
          <HeartPulse class="brand-icon" />
          MyPet<span class="brand-accent">Clinic</span>
        </router-link>

        <!-- Toggle Mobile Menu Button -->
        <button class="navbar-toggler" @click="isMobileMenuOpen = !isMobileMenuOpen">
          <Menu v-if="!isMobileMenuOpen" class="toggler-icon" />
          <X v-else class="toggler-icon" />
        </button>

        <!-- Navigation Links Menu -->
        <div class="navbar-collapse" :class="{ 'show': isMobileMenuOpen }">
          <ul class="navbar-nav">
            <li class="nav-item">
              <router-link to="/" class="nav-link" exact-active-class="active">Trang chủ</router-link>
            </li>

            <!-- Dropdown Giới thiệu -->
            <li class="nav-item dropdown" @mouseenter="openDropdown('intro')" @mouseleave="closeDropdown('intro')">
              <a class="nav-link dropdown-toggle" href="#" @click.prevent="toggleDropdownMobile('intro')">
                Giới thiệu
                <ChevronDown class="dropdown-chevron" />
              </a>
              <ul class="dropdown-menu" :class="{ 'show-mobile': activeDropdowns.intro }">
                <li>
                  <router-link to="/history" class="dropdown-item" @click="closeMobileMenu">
                    <HistoryIcon class="dropdown-icon" /> Lịch sử phát triển
                  </router-link>
                </li>
                <li>
                  <router-link to="/team" class="dropdown-item" @click="closeMobileMenu">
                    <Users class="dropdown-icon" /> Đội ngũ nhân viên
                  </router-link>
                </li>
              </ul>
            </li>

            <!-- Dropdown Dịch Vụ -->
            <li class="nav-item dropdown" @mouseenter="openDropdown('services')" @mouseleave="closeDropdown('services')">
              <a class="nav-link dropdown-toggle" href="#" @click.prevent="toggleDropdownMobile('services')">
                Dịch vụ thú y
                <ChevronDown class="dropdown-chevron" />
              </a>
              <ul class="dropdown-menu" :class="{ 'show-mobile': activeDropdowns.services }">
                <li>
                  <router-link to="/services/kham-dieu-tri" class="dropdown-item" @click="closeMobileMenu">
                    <Stethoscope class="dropdown-icon text-primary" /> Khám và điều trị
                  </router-link>
                </li>
                <li>
                  <router-link to="/services/tiem-phong" class="dropdown-item" @click="closeMobileMenu">
                    <ShieldCheck class="dropdown-icon text-info" /> Tiêm phòng bệnh
                  </router-link>
                </li>
                <li>
                  <hr class="dropdown-divider">
                </li>
                <li>
                  <router-link to="/services/spa-grooming" class="dropdown-item" @click="closeMobileMenu">
                    <Scissors class="dropdown-icon text-secondary" /> Spa & Thẩm mỹ cắt tỉa
                  </router-link>
                </li>
              </ul>
            </li>

            <!-- Dropdown Kiến thức -->
            <li class="nav-item dropdown" @mouseenter="openDropdown('knowledge')" @mouseleave="closeDropdown('knowledge')">
              <a class="nav-link dropdown-toggle" href="#" @click.prevent="toggleDropdownMobile('knowledge')">
                Kiến thức
                <ChevronDown class="dropdown-chevron" />
              </a>
              <ul class="dropdown-menu" :class="{ 'show-mobile': activeDropdowns.knowledge }">
                <li>
                  <router-link to="/news" class="dropdown-item" @click="closeMobileMenu">
                    <Newspaper class="dropdown-icon" /> Tin tức sự kiện
                  </router-link>
                </li>
                <li>
                  <router-link to="/services/suc-khoe" class="dropdown-item" @click="closeMobileMenu">
                    <BookOpen class="dropdown-icon" /> Sức khỏe thú cưng
                  </router-link>
                </li>
              </ul>
            </li>

            <li class="nav-item">
              <router-link to="/contact" class="nav-link" exact-active-class="active">Liên hệ</router-link>
            </li>
          </ul>

          <!-- User Actions Panel -->
          <div class="user-actions">
            <template v-if="isLoggedIn">
              <NotificationBell class="me-2 d-none d-sm-block" />
              <div class="profile-dropdown" @mouseenter="openDropdown('profile')" @mouseleave="closeDropdown('profile')">
                <button class="btn-premium-outline dropdown-toggle d-flex align-items-center gap-2 shadow-sm py-2 px-3 border-2" @click="toggleDropdownMobile('profile')">
                  <UserCircle class="profile-icon text-warning" />
                  <span class="username-text">{{ username }}</span>
                  <ChevronDown class="dropdown-chevron" />
                </button>
                <ul class="dropdown-menu dropdown-menu-end border-0 shadow" :class="{ 'show-mobile': activeDropdowns.profile }">
                  <li>
                    <router-link to="/dashboard" class="dropdown-item py-2" @click="closeMobileMenu">
                      <LayoutDashboard class="dropdown-icon text-warning" /> Dashboard
                    </router-link>
                  </li>
                  <li>
                    <hr class="dropdown-divider">
                  </li>
                  <li>
                    <button @click="handleLogoutClick" class="dropdown-item text-danger border-0 bg-transparent w-100 text-start d-flex align-items-center gap-2">
                      <LogOut class="dropdown-icon" /> Đăng xuất
                    </button>
                  </li>
                </ul>
              </div>
            </template>
            <template v-else>
              <router-link to="/login" class="btn-premium-outline py-2 px-3">
                <LogIn class="action-btn-icon" /> Đăng Nhập
              </router-link>
              <router-link to="/register" class="btn-premium-outline py-2 px-3 d-none d-sm-inline-flex">
                <UserPlus class="action-btn-icon" /> Đăng Ký
              </router-link>
            </template>

            <button class="btn-premium shadow-sm ms-2" @click="triggerBookingModal">
              <CalendarDays class="action-btn-icon" /> Đặt Lịch Ngay
            </button>
          </div>

        </div>
      </div>
    </nav>
  </header>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';
import api from '../../services/api';
import NotificationBell from './NotificationBell.vue';
import { 
  HeartPulse, Menu, X, ChevronDown, 
  History as HistoryIcon, Users, Stethoscope, 
  ShieldCheck, Scissors, Newspaper, BookOpen, 
  UserCircle, LayoutDashboard, LogOut, LogIn, 
  UserPlus, CalendarDays 
} from '@lucide/vue';

const emit = defineEmits(['open-booking']);
const router = useRouter();

const isLoggedIn = ref(false);
const username = ref('');
const isMobileMenuOpen = ref(false);

const activeDropdowns = reactive({
  intro: false,
  services: false,
  knowledge: false,
  profile: false
});

const openDropdown = (key: keyof typeof activeDropdowns) => {
  if (window.innerWidth > 991) {
    activeDropdowns[key] = true;
  }
};

const closeDropdown = (key: keyof typeof activeDropdowns) => {
  if (window.innerWidth > 991) {
    activeDropdowns[key] = false;
  }
};

const toggleDropdownMobile = (key: keyof typeof activeDropdowns) => {
  if (window.innerWidth <= 991) {
    activeDropdowns[key] = !activeDropdowns[key];
  }
};

const closeMobileMenu = () => {
  isMobileMenuOpen.value = false;
  Object.keys(activeDropdowns).forEach(key => {
    activeDropdowns[key as keyof typeof activeDropdowns] = false;
  });
};

const triggerBookingModal = () => {
  closeMobileMenu();
  if (isLoggedIn.value) {
    emit('open-booking');
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

const checkLoginState = async () => {
  try {
    const res = await api.get('/profile');
    isLoggedIn.value = true;
    username.value = res.data.fullName || res.data.userName || 'User';
  } catch (err) {
    isLoggedIn.value = false;
  }
};

const handleLogoutClick = async () => {
  try {
    await api.post('/account/logout');
    isLoggedIn.value = false;
    username.value = '';
    closeMobileMenu();
    router.push('/');
    window.location.reload(); // Reload to clear session state fully
  } catch (err) {
    console.error('Logout error:', err);
  }
};

onMounted(() => {
  checkLoginState();
});
</script>

<style scoped>
.sticky-top {
  position: sticky;
  top: 0;
  z-index: 1020;
}

.navbar-premium {
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(15px);
  -webkit-backdrop-filter: blur(15px);
  border-bottom: 1px solid var(--border-color);
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-speed) ease;
  padding: 0.8rem 1rem;
}

.nav-container {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  position: relative;
}

.navbar-brand {
  font-weight: 800;
  font-size: 1.4rem;
  color: var(--text-dark) !important;
  letter-spacing: -0.5px;
  display: flex;
  align-items: center;
  gap: 8px;
  text-decoration: none;
}

.brand-icon {
  width: 28px;
  height: 28px;
  color: var(--primary-gold);
}

.brand-accent {
  color: var(--primary-gold);
}

.navbar-toggler {
  display: none;
  background: none;
  border: none;
  cursor: pointer;
  color: var(--text-dark);
}

.toggler-icon {
  width: 24px;
  height: 24px;
}

.navbar-collapse {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-grow: 1;
  margin-left: 2rem;
}

.navbar-nav {
  display: flex !important;
  flex-direction: row !important;
  list-style: none;
  gap: 0.5rem;
  align-items: center;
  margin: 0;
  padding: 0;
  flex-shrink: 0; /* Không bị co lại khi user-actions rộng */
}

.nav-link {
  font-weight: 600 !important;
  color: var(--text-dark) !important;
  padding: 0.5rem 1rem !important;
  border-radius: var(--radius-sm);
  transition: all var(--transition-speed) ease;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.95rem;
  white-space: nowrap; /* Không cho wrap xuống dòng */
}

.nav-link:hover {
  color: var(--primary-dark) !important;
  background-color: var(--primary-cream);
}

.nav-link.active {
  color: var(--primary-dark) !important;
  background-color: transparent !important;
}

/* Dropdown specific */
.dropdown {
  position: relative;
}

.dropdown-chevron {
  width: 14px;
  height: 14px;
  transition: transform var(--transition-speed) ease;
}

.dropdown:hover .dropdown-chevron {
  transform: rotate(180px);
}

.dropdown-menu {
  position: absolute;
  top: 100%;
  left: 0;
  z-index: 1000;
  display: none;
  min-width: 13rem;
  padding: 0.8rem;
  margin: 0.125rem 0 0;
  list-style: none;
  background-color: rgba(255, 255, 255, 0.98);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
  backdrop-filter: blur(10px);
}

.dropdown:hover .dropdown-menu {
  display: block;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  padding: 0.6rem 1rem;
  border-radius: var(--radius-sm);
  color: var(--text-dark);
  text-decoration: none;
  transition: all var(--transition-speed) ease;
  font-size: 0.9rem;
}

.dropdown-item:hover {
  background-color: var(--primary-cream);
  color: var(--primary-dark);
  transform: translateX(4px);
}

.dropdown-icon {
  width: 16px;
  height: 16px;
}

.dropdown-divider {
  height: 0;
  margin: 0.5rem 0;
  overflow: hidden;
  border-top: 1px solid var(--border-color);
}

.profile-dropdown {
  position: relative;
}

.profile-dropdown:hover .dropdown-menu {
  display: block;
}

.profile-icon {
  width: 20px;
  height: 20px;
}

.user-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0; /* Không bị co lại */
  min-width: 0; /* Cho phép truncate bên trong */
}

.user-actions .btn-premium,
.user-actions .btn-premium-outline {
  padding: 0.5rem 1rem !important;
  white-space: nowrap !important;
  font-size: 0.85rem !important;
  letter-spacing: 0.2px;
}

.action-btn-icon {
  width: 16px;
  height: 16px;
}

/* Username truncation */
.username-text {
  font-weight: 700;
  color: var(--text-dark);
  max-width: 130px;        /* Giới hạn độ rộng tối đa */
  overflow: hidden;
  text-overflow: ellipsis; /* Hiện "..." khi quá dài */
  white-space: nowrap;
  display: inline-block;
}

/* Responsive Menu */
@media (max-width: 991px) {
  .navbar-toggler {
    display: block;
  }

  .navbar-collapse {
    display: none;
    position: absolute;
    top: 100%;
    left: -1rem;
    right: -1rem;
    background: white;
    flex-direction: column;
    padding: 1.5rem;
    box-shadow: var(--shadow-md);
    border-bottom: 1px solid var(--border-color);
    margin-left: 0;
    gap: 1.5rem;
  }

  .navbar-collapse.show {
    display: flex;
  }

  .navbar-nav {
    flex-direction: column !important;
    width: 100%;
    align-items: stretch;
  }

  .nav-link {
    justify-content: space-between;
    width: 100%;
  }

  .dropdown-menu {
    position: static;
    display: none;
    box-shadow: none;
    border: none;
    background: var(--primary-cream);
    margin-top: 0.5rem;
    padding-left: 1rem;
  }

  .dropdown-menu.show-mobile {
    display: block;
  }

  .user-actions {
    flex-direction: column;
    width: 100%;
    align-items: stretch;
    gap: 0.75rem;
  }

  .profile-dropdown {
    width: 100%;
  }

  .profile-dropdown button {
    width: 100%;
    justify-content: center;
  }
}
</style>
