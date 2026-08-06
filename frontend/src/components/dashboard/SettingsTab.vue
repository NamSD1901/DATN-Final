<template>
  <div class="settings-tab-container p-0 p-md-3">



    <!-- ═══════════════════════════════════════════
         MAIN LAYOUT: Sidebar (Nav) + Content
         Desktop: 2 cột | Mobile: Stack (1 cột)
    ══════════════════════════════════════════════ -->
    <div class="settings-main-layout">

      <!-- ─── SIDEBAR NAVIGATION (Left Panel) ─── -->
      <nav class="settings-nav glass-card p-2 mb-4 mb-lg-0" aria-label="Settings navigation">
        <div class="settings-nav-group mb-1">
          <div class="settings-nav-group-label">{{ $t('settings.nav.personal') }}</div>
          <button
            v-for="item in navItems"
            :key="item.key"
            class="settings-nav-item"
            :class="{ 'active': activeSection === item.key }"
            :aria-current="activeSection === item.key ? 'page' : undefined"
            @click="activeSection = item.key"
          >
            <span class="settings-nav-icon">
              <i :class="item.icon"></i>
            </span>
            <span class="settings-nav-label">{{ $t(item.labelKey) }}</span>
            <i class="bi bi-chevron-right settings-nav-chevron ms-auto"></i>
          </button>
        </div>
      </nav>

      <!-- ─── CONTENT PANEL (Right Panel) ─── -->
      <div class="settings-content-panel">
        <div class="glass-card p-4 p-md-5">

          <!-- Section Header (chung cho tất cả sections) -->
          <div class="d-flex align-items-center gap-3 mb-4 pb-3 section-header-divider">
            <div class="section-icon-wrapper">
              <i :class="currentNavItem?.icon"></i>
            </div>
            <div>
              <h5 class="fw-bold text-dark mb-0">{{ $t(currentNavItem?.labelKey ?? '') }}</h5>
              <p class="text-muted small mb-0">{{ $t(currentNavItem?.descKey ?? '') }}</p>
            </div>
          </div>

          <!-- ═══════════════════════════════════════════════════
               PHASE 2: LANGUAGE SECTION
          ════════════════════════════════════════════════════ -->
          <div v-if="activeSection === 'language'" class="section-language">

            <!-- Info callout -->
            <div class="info-callout mb-4">
              <i class="bi bi-info-circle-fill me-2 text-warning"></i>
              <span>{{ $t('settings.language.autoApplyNote') }}</span>
            </div>

            <!-- Danh sách Radio ngôn ngữ -->
            <div class="language-list" role="radiogroup" :aria-label="$t('settings.nav.language')">
              <label
                v-for="lang in availableLanguages"
                :key="lang.code"
                class="language-option"
                :class="{ 'selected': currentLocale === lang.code }"
                :for="`lang-radio-${lang.code}`"
              >
                <!-- Hidden radio input (accessible) -->
                <input
                  :id="`lang-radio-${lang.code}`"
                  type="radio"
                  name="settings-language"
                  class="language-radio-input visually-hidden"
                  :value="lang.code"
                  :checked="currentLocale === lang.code"
                  @change="handleLanguageChange(lang.code)"
                />

                <!-- Custom radio visual -->
                <div class="language-option-inner">
                  <div class="language-radio-dot">
                    <span class="radio-dot-inner"></span>
                  </div>
                  <div class="language-info">
                    <span class="language-native-name">{{ lang.nativeName }}</span>
                    <span class="language-english-name">{{ lang.englishName }}</span>
                  </div>
                  <div class="language-badge" v-if="currentLocale === lang.code">
                    <i class="bi bi-check-lg"></i>
                    {{ $t('settings.language.active') }}
                  </div>
                </div>
              </label>
            </div>

            <!-- Toast Notification (auto-dismiss) -->
            <Transition name="toast-slide">
              <div v-if="showToast" class="settings-toast" :class="toastType" role="alert" aria-live="polite">
                <i :class="toastIcon" class="me-2"></i>
                <span>{{ toastMessage }}</span>
              </div>
            </Transition>

          </div>

          <!-- ═══════════════════════════════════════════════════
               PHASE 3: APPEARANCE SECTION
          ════════════════════════════════════════════════════ -->
          <div v-else-if="activeSection === 'appearance'" class="section-appearance">

            <!-- Info callout -->
            <div class="info-callout mb-4">
              <i class="bi bi-info-circle-fill me-2 text-warning"></i>
              <span>{{ $t('settings.appearance.autoApplyNote') }}</span>
            </div>

            <!-- Theme Cards -->
            <div class="theme-grid" role="radiogroup" :aria-label="$t('settings.nav.appearance')">
              <label
                v-for="themeOption in themeOptions"
                :key="themeOption.mode"
                class="theme-card"
                :class="{ 'selected': currentTheme === themeOption.mode }"
                :for="`theme-radio-${themeOption.mode}`"
              >
                <input
                  :id="`theme-radio-${themeOption.mode}`"
                  type="radio"
                  name="settings-theme"
                  class="visually-hidden"
                  :value="themeOption.mode"
                  :checked="currentTheme === themeOption.mode"
                  @change="handleThemeChange(themeOption.mode)"
                />

                <!-- Preview miniature -->
                <div class="theme-preview" :class="`preview-${themeOption.mode}`">
                  <div class="preview-topbar"></div>
                  <div class="preview-body">
                    <div class="preview-sidebar"></div>
                    <div class="preview-content">
                      <div class="preview-line" style="width: 60%;"></div>
                      <div class="preview-line" style="width: 40%;"></div>
                      <div class="preview-line" style="width: 80%;"></div>
                    </div>
                  </div>
                </div>

                <!-- Label -->
                <div class="theme-card-label">
                  <div class="d-flex align-items-center gap-2 mb-1">
                    <div class="theme-radio-dot">
                      <span class="radio-dot-inner"></span>
                    </div>
                    <span class="theme-name">{{ $t(themeOption.labelKey) }}</span>
                    <span v-if="currentTheme === themeOption.mode" class="language-badge ms-auto">
                      <i class="bi bi-check-lg"></i>
                      {{ $t('settings.language.active') }}
                    </span>
                  </div>
                  <p class="theme-desc mb-0">{{ $t(themeOption.descKey) }}</p>
                </div>
              </label>
            </div>

            <!-- Toast Notification -->
            <Transition name="toast-slide">
              <div v-if="showToast" class="settings-toast" :class="toastType" role="alert" aria-live="polite">
                <i :class="toastIcon" class="me-2"></i>
                <span>{{ toastMessage }}</span>
              </div>
            </Transition>

          </div>

          <!-- ═══════════════════════════════════════════════════
               PHASE 4: NOTIFICATIONS SECTION
          ════════════════════════════════════════════════════ -->
          <div v-else-if="activeSection === 'notifications'" class="section-notifications">
            <!-- Info callout -->
            <div class="info-callout mb-4">
              <i class="bi bi-info-circle-fill me-2 text-warning"></i>
              <span>{{ $t('settings.notifications.autoApplyNote') }}</span>
            </div>

            <div class="settings-list-container">
              <SettingListItem
                :title="$t('settings.notifications.appointmentTitle')"
                :description="$t('settings.notifications.appointmentDesc')"
                icon="bi bi-calendar-check"
              >
                <UiSwitch v-model="notificationSettings.appointment" @change="handleNotificationChange" />
              </SettingListItem>

              <SettingListItem
                :title="$t('settings.notifications.promoTitle')"
                :description="$t('settings.notifications.promoDesc')"
                icon="bi bi-megaphone"
              >
                <UiSwitch v-model="notificationSettings.promo" @change="handleNotificationChange" />
              </SettingListItem>

              <SettingListItem
                :title="$t('settings.notifications.emailTitle')"
                :description="$t('settings.notifications.emailDesc')"
                icon="bi bi-envelope"
                :divider="false"
              >
                <UiSwitch v-model="notificationSettings.email" @change="handleNotificationChange" />
              </SettingListItem>
            </div>

            <!-- Toast Notification -->
            <Transition name="toast-slide">
              <div v-if="showToast" class="settings-toast" :class="toastType" role="alert" aria-live="polite">
                <i :class="toastIcon" class="me-2"></i>
                <span>{{ toastMessage }}</span>
              </div>
            </Transition>
          </div>

          <!-- ACCOUNT SECTION -->
          <div v-else-if="activeSection === 'account'" class="section-account p-0">
            <ProfileTab @profile-updated="emit('profile-updated')" />
          </div>

          <!-- PLACEHOLDER cho các section chưa implement (Phase 5-6) -->
          <div v-else class="settings-section-placeholder">
            <div class="placeholder-icon">
              <i :class="currentNavItem?.icon"></i>
            </div>
            <p class="text-muted mt-3 mb-0 fw-medium">{{ $t('settings.comingSoon') }}</p>
            <p class="text-muted small mt-1 mb-0">{{ $t('settings.comingSoonDesc') }}</p>
          </div>

        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useTheme } from '../../utils/useTheme';
import SettingListItem from '../shared/SettingListItem.vue';
import UiSwitch from '../shared/UiSwitch.vue';
import ProfileTab from './ProfileTab.vue';

const { t, locale } = useI18n();
const { currentTheme, setTheme } = useTheme();

const emit = defineEmits(['profile-updated']);

// ─── Trạng thái: Section nào đang active ────────────────────────────────────
const activeSection = ref<string>('language');

// ─── Định nghĩa cấu trúc Navigation ─────────────────────────────────────────
interface NavItem {
  key: string;
  icon: string;
  labelKey: string;
  descKey: string;
}

const navItems: NavItem[] = [
  {
    key: 'language',
    icon: 'bi bi-globe',
    labelKey: 'settings.nav.language',
    descKey: 'settings.nav.languageDesc',
  },
  {
    key: 'appearance',
    icon: 'bi bi-palette',
    labelKey: 'settings.nav.appearance',
    descKey: 'settings.nav.appearanceDesc',
  },
  {
    key: 'notifications',
    icon: 'bi bi-bell',
    labelKey: 'settings.nav.notifications',
    descKey: 'settings.nav.notificationsDesc',
  },
  {
    key: 'account',
    icon: 'bi bi-person-circle',
    labelKey: 'settings.nav.account',
    descKey: 'settings.nav.accountDesc',
  },
  {
    key: 'security',
    icon: 'bi bi-shield-lock',
    labelKey: 'settings.nav.security',
    descKey: 'settings.nav.securityDesc',
  },
];

const currentNavItem = computed(() =>
  navItems.find((item) => item.key === activeSection.value)
);

// ═════════════════════════════════════════════════════════════════════════════
// PHASE 2: LANGUAGE MODULE
// ═════════════════════════════════════════════════════════════════════════════

// ─── Danh sách ngôn ngữ được hỗ trợ ─────────────────────────────────────────
interface Language {
  code: string;
  nativeName: string;   // Tên ngôn ngữ bằng chính nó (VD: Tiếng Việt)
  englishName: string;  // Tên ngôn ngữ bằng tiếng Anh (VD: Vietnamese)
}

const availableLanguages: Language[] = [
  {
    code: 'vi',
    nativeName: 'Tiếng Việt',
    englishName: 'Vietnamese',
  },
  {
    code: 'en',
    nativeName: 'English',
    englishName: 'English (US)',
  },
];

// ─── Locale hiện tại (đọc từ vue-i18n, được ghi lại từ localStorage) ────────
const currentLocale = computed(() => locale.value);

// ─── Toast Notification State ──────────────────────────────────────────────
const showToast = ref(false);
const toastMessage = ref('');
const toastType = ref<'toast-success' | 'toast-error'>('toast-success');
let toastTimer: ReturnType<typeof setTimeout> | null = null;

const toastIcon = computed(() =>
  toastType.value === 'toast-success' ? 'bi bi-check-circle-fill' : 'bi bi-exclamation-circle-fill'
);

const triggerToast = (message: string, type: 'toast-success' | 'toast-error' = 'toast-success') => {
  // Hủy timer cũ nếu đang còn chờ (prevent overlap)
  if (toastTimer) clearTimeout(toastTimer);

  toastMessage.value = message;
  toastType.value = type;
  showToast.value = true;

  // Tự ẩn sau 3 giây
  toastTimer = setTimeout(() => {
    showToast.value = false;
  }, 3000);
};

// ─── Xử lý đổi ngôn ngữ (Auto Save — không cần nút Save) ────────────────────
const handleLanguageChange = (langCode: string) => {
  if (locale.value === langCode) return;

  try {
    locale.value = langCode;
    localStorage.setItem('user_locale', langCode);
    triggerToast(t('settings.language.savedSuccess'), 'toast-success');
  } catch {
    triggerToast(t('settings.language.savedError'), 'toast-error');
  }
};

// ═════════════════════════════════════════════════════════════════════════════
// PHASE 3: APPEARANCE MODULE
// ═════════════════════════════════════════════════════════════════════════════

type ThemeMode = 'light' | 'dark' | 'system';

interface ThemeOption {
  mode: ThemeMode;
  labelKey: string;
  descKey: string;
}

const themeOptions: ThemeOption[] = [
  {
    mode: 'light',
    labelKey: 'settings.appearance.lightLabel',
    descKey:  'settings.appearance.lightDesc',
  },
  {
    mode: 'dark',
    labelKey: 'settings.appearance.darkLabel',
    descKey:  'settings.appearance.darkDesc',
  },
  {
    mode: 'system',
    labelKey: 'settings.appearance.systemLabel',
    descKey:  'settings.appearance.systemDesc',
  },
];

// ─── Xử lý đổi theme (Auto Save — không cần nút Save) ────────────────────────
const handleThemeChange = (mode: ThemeMode) => {
  if (currentTheme.value === mode) return;

  try {
    // setTheme trong composable đã xử lý: apply vào DOM + ghi localStorage
    setTheme(mode);
    triggerToast(t('settings.appearance.savedSuccess'), 'toast-success');
  } catch {
    triggerToast(t('settings.appearance.savedError'), 'toast-error');
  }
};

// ═════════════════════════════════════════════════════════════════════════════
// PHASE 4: NOTIFICATIONS MODULE
// ═════════════════════════════════════════════════════════════════════════════
interface NotificationSettings {
  appointment: boolean;
  promo: boolean;
  email: boolean;
}

const notificationSettings = reactive<NotificationSettings>({
  appointment: true,
  promo: false,
  email: true
});

onMounted(() => {
  try {
    const saved = localStorage.getItem('user_notifications');
    if (saved) {
      Object.assign(notificationSettings, JSON.parse(saved));
    }
  } catch {
    // Ignore error
  }
});

const handleNotificationChange = () => {
  try {
    localStorage.setItem('user_notifications', JSON.stringify(notificationSettings));
    triggerToast(t('settings.notifications.savedSuccess'), 'toast-success');
  } catch {
    triggerToast(t('settings.notifications.savedError'), 'toast-error');
  }
};
</script>


<style scoped>
/* ════════════════════════════════════════════════════════════
   SETTINGS TAB - Design System
   Responsive: Desktop (2 cột) | Mobile (Stack 1 cột)
════════════════════════════════════════════════════════════ */

/* ── Container ──────────────────────────────────────────────── */
.settings-tab-container {
  min-height: 100%;
}

/* ── Hero Header ─────────────────────────────────────────────── */
.settings-hero {
  background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 60%, #fde68a 100%);
  border-left: 5px solid #f59e0b;
  border-radius: 20px;
  box-shadow: 0 4px 24px rgba(245, 158, 11, 0.08);
}

.settings-hero-icon {
  width: 42px;
  height: 42px;
  background: linear-gradient(135deg, #f59e0b, #d97706);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 1.1rem;
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.35);
  flex-shrink: 0;
}

.gradient-text-gold {
  background: linear-gradient(to right, #b45309, #d97706);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

/* ── Glass Card ──────────────────────────────────────────────── */
.glass-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 20px;
  box-shadow: none;
}

/* ── Main Layout: 2 cột Desktop, 1 cột Mobile ───────────────── */
.settings-main-layout {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 991.98px) {
  .settings-main-layout {
    grid-template-columns: 1fr;
  }
}

/* ── Sidebar Navigation ──────────────────────────────────────── */
.settings-nav {
  position: sticky;
  top: 1rem;
}

.settings-nav-group-label {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: #94a3b8;
  padding: 0.5rem 0.75rem 0.35rem;
}

.settings-nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
  padding: 0.7rem 0.75rem;
  border: none;
  background: transparent;
  border-radius: 12px;
  text-align: left;
  cursor: pointer;
  transition: all 0.18s ease;
  color: #475569;
  font-size: 0.9rem;
  font-weight: 500;
  min-height: 44px;
}

.settings-nav-item:hover {
  background: rgba(245, 158, 11, 0.08);
  color: #b45309;
}

.settings-nav-item:focus-visible {
  outline: 2px solid #f59e0b;
  outline-offset: 2px;
}

.settings-nav-item.active {
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.15), rgba(217, 119, 6, 0.1));
  color: #b45309;
  font-weight: 600;
}

.settings-nav-item.active .settings-nav-icon {
  color: #f59e0b;
}

.settings-nav-item.active .settings-nav-chevron {
  opacity: 1;
  color: #f59e0b;
}

.settings-nav-icon {
  font-size: 1.05rem;
  width: 24px;
  text-align: center;
  color: #94a3b8;
  transition: color 0.18s ease;
  flex-shrink: 0;
}

.settings-nav-label {
  flex: 1;
}

.settings-nav-chevron {
  font-size: 0.65rem;
  opacity: 0;
  transition: opacity 0.18s ease;
}

.settings-nav-item:hover .settings-nav-chevron {
  opacity: 0.5;
}

/* ── Content Panel ───────────────────────────────────────────── */
.settings-content-panel {
  min-height: 300px;
}

/* ── Section Header Divider ──────────────────────────────────── */
.section-header-divider {
  border-bottom: 1px solid rgba(245, 158, 11, 0.12);
}

.section-icon-wrapper {
  width: 44px;
  height: 44px;
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.12), rgba(217, 119, 6, 0.08));
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.15rem;
  color: #d97706;
  flex-shrink: 0;
}

/* ── Placeholder (sections chưa implement) ───────────────────── */
.settings-section-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 1rem;
  color: #cbd5e1;
}

.placeholder-icon {
  font-size: 3rem;
  opacity: 0.3;
}

/* ════════════════════════════════════════════════════════════
   PHASE 2: LANGUAGE SECTION STYLES
════════════════════════════════════════════════════════════ */

/* ── Info Callout ─────────────────────────────────────────────── */
.info-callout {
  background: rgba(245, 158, 11, 0.06);
  border: 1px solid rgba(245, 158, 11, 0.2);
  border-radius: 12px;
  padding: 0.75rem 1rem;
  font-size: 0.825rem;
  color: #92400e;
  display: flex;
  align-items: center;
}

/* ── Language List ────────────────────────────────────────────── */
.language-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

/* ── Language Option (label bọc ngoài, clickable) ────────────── */
.language-option {
  display: block;
  border: 2px solid #e2e8f0;
  border-radius: 14px;
  padding: 1rem 1.25rem;
  cursor: pointer;
  transition: all 0.2s ease;
  background: #ffffff;
}

.language-option:hover {
  border-color: rgba(245, 158, 11, 0.4);
  background: rgba(245, 158, 11, 0.02);
  transform: translateY(-1px);
  box-shadow: 0 4px 16px rgba(245, 158, 11, 0.08);
}

.language-option.selected {
  border-color: #f59e0b;
  background: linear-gradient(135deg, rgba(255, 251, 235, 0.8), rgba(254, 243, 199, 0.5));
  box-shadow: 0 4px 20px rgba(245, 158, 11, 0.12);
}

/* Focus visible khi dùng bàn phím (Accessibility) */
.language-radio-input:focus-visible + .language-option-inner,
.language-option:has(.language-radio-input:focus-visible) {
  outline: 2px solid #f59e0b;
  outline-offset: 3px;
  border-radius: 14px;
}

/* ── Language Option Inner (flex layout) ─────────────────────── */
.language-option-inner {
  display: flex;
  align-items: center;
  gap: 1rem;
}

/* ── Custom Radio Dot ─────────────────────────────────────────── */
.language-radio-dot {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: 2px solid #cbd5e1;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color 0.2s ease;
  flex-shrink: 0;
}

.language-option.selected .language-radio-dot {
  border-color: #f59e0b;
}

.radio-dot-inner {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: #f59e0b;
  transform: scale(0);
  transition: transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.language-option.selected .radio-dot-inner {
  transform: scale(1);
}

/* ── Language Info Text ───────────────────────────────────────── */
.language-info {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  flex: 1;
}

.language-native-name {
  font-size: 0.95rem;
  font-weight: 600;
  color: #1e293b;
}

.language-english-name {
  font-size: 0.775rem;
  color: #64748b;
}

/* ── Active Badge ─────────────────────────────────────────────── */
.language-badge {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  font-size: 0.72rem;
  font-weight: 600;
  color: #d97706;
  background: rgba(245, 158, 11, 0.12);
  border: 1px solid rgba(245, 158, 11, 0.25);
  border-radius: 20px;
  padding: 0.25rem 0.65rem;
  white-space: nowrap;
  flex-shrink: 0;
}

/* ════════════════════════════════════════════════════════════
   TOAST NOTIFICATION
════════════════════════════════════════════════════════════ */
.settings-toast {
  display: flex;
  align-items: center;
  padding: 0.85rem 1.25rem;
  border-radius: 12px;
  font-size: 0.875rem;
  font-weight: 500;
  margin-top: 1.5rem;
}

.settings-toast.toast-success {
  background: rgba(16, 185, 129, 0.08);
  border: 1px solid rgba(16, 185, 129, 0.2);
  color: #065f46;
}

.settings-toast.toast-success i {
  color: #10b981;
}

.settings-toast.toast-error {
  background: rgba(239, 68, 68, 0.08);
  border: 1px solid rgba(239, 68, 68, 0.2);
  color: #991b1b;
}

.settings-toast.toast-error i {
  color: #ef4444;
}

/* ── Toast Animation ──────────────────────────────────────────── */
.toast-slide-enter-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.toast-slide-leave-active {
  transition: all 0.2s ease-in;
}

.toast-slide-enter-from {
  opacity: 0;
  transform: translateY(8px);
}

.toast-slide-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}

/* ════════════════════════════════════════════════════════════
   PHASE 3: APPEARANCE SECTION STYLES
════════════════════════════════════════════════════════════ */

/* ── Theme Grid: 3 cột Desktop, 1 cột Mobile ──────────────── */
.theme-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
}

@media (max-width: 767.98px) {
  .theme-grid {
    grid-template-columns: 1fr;
  }
}

/* ── Theme Card ───────────────────────────────────────────── */
.theme-card {
  border: 2px solid #e2e8f0;
  border-radius: 16px;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s ease;
  background: #ffffff;
  display: block;
}

.theme-card:hover {
  border-color: rgba(245, 158, 11, 0.4);
  box-shadow: 0 4px 16px rgba(245, 158, 11, 0.08);
  transform: translateY(-2px);
}

.theme-card.selected {
  border-color: #f59e0b;
  background: linear-gradient(135deg, rgba(255,251,235,0.8), rgba(254,243,199,0.5));
  box-shadow: 0 4px 20px rgba(245, 158, 11, 0.14);
}

.theme-card:focus-within {
  outline: 2px solid #f59e0b;
  outline-offset: 3px;
}

/* ── Preview Miniature ────────────────────────────────────── */
.theme-preview {
  border-radius: 10px;
  overflow: hidden;
  margin-bottom: 0.85rem;
  height: 80px;
  border: 1px solid rgba(0,0,0,0.06);
  display: flex;
  flex-direction: column;
}

.preview-body {
  flex: 1;
  display: flex;
}

.preview-content {
  flex: 1;
  padding: 8px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

/* Light */
.preview-light { background: #f8fafc; }
.preview-light .preview-topbar { height: 14px; background: #fff; border-bottom: 1px solid #e2e8f0; }
.preview-light .preview-sidebar { width: 28%; background: #fff; border-right: 1px solid #e2e8f0; }
.preview-light .preview-line { height: 6px; background: #e2e8f0; border-radius: 3px; margin-bottom: 6px; }

/* Dark */
.preview-dark { background: #0f172a; }
.preview-dark .preview-topbar { height: 14px; background: #1e293b; border-bottom: 1px solid rgba(148,163,184,0.1); }
.preview-dark .preview-sidebar { width: 28%; background: #1e293b; border-right: 1px solid rgba(148,163,184,0.1); }
.preview-dark .preview-line { height: 6px; background: #334155; border-radius: 3px; margin-bottom: 6px; }

/* System: split nửa sáng nửa tối */
.preview-system { background: linear-gradient(to right, #f8fafc 50%, #0f172a 50%); }
.preview-system .preview-topbar { height: 14px; background: linear-gradient(to right, #fff 50%, #1e293b 50%); border-bottom: 1px solid rgba(0,0,0,0.08); }
.preview-system .preview-sidebar { width: 28%; background: linear-gradient(to bottom, #fff 50%, #1e293b 50%); border-right: 1px solid rgba(0,0,0,0.06); }
.preview-system .preview-line { height: 6px; background: linear-gradient(to right, #e2e8f0 50%, #334155 50%); border-radius: 3px; margin-bottom: 6px; }

/* ── Theme Label ──────────────────────────────────────────── */
.theme-name {
  font-size: 0.875rem;
  font-weight: 600;
  color: #1e293b;
}

.theme-desc {
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 0.15rem;
  line-height: 1.4;
}

/* ── Reuse Radio Dot ──────────────────────────────────────── */
.theme-radio-dot {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 2px solid #cbd5e1;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color 0.2s ease;
  flex-shrink: 0;
}

.theme-card.selected .theme-radio-dot {
  border-color: #f59e0b;
}

.theme-card.selected .radio-dot-inner {
  transform: scale(1);
}
</style>
