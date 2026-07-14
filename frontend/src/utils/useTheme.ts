/**
 * useTheme.ts — Composable quản lý chủ đề giao diện (Appearance Theme)
 *
 * Chiến lược: Zero-DB (LocalStorage only), không cần Backend
 * Cơ chế:     Gắn attribute `data-theme="dark"` lên thẻ <html>
 *             CSS trong style.css override biến CSS theo selector [data-theme="dark"]
 *
 * 3 chế độ hỗ trợ:
 *   - 'light'  : Luôn sáng
 *   - 'dark'   : Luôn tối
 *   - 'system' : Tự động theo cài đặt hệ điều hành (prefers-color-scheme)
 */

import { ref, watch, onMounted, onUnmounted } from 'vue';

// ─── Constants ────────────────────────────────────────────────────────────────
const STORAGE_KEY = 'user_theme';
type ThemeMode = 'light' | 'dark' | 'system';

// ─── Singleton: trạng thái theme dùng chung toàn app ─────────────────────────
// (module-level ref — chia sẻ giữa các component không cần Pinia)
const currentTheme = ref<ThemeMode>('system');

// ─── Helper: Đọc theme đã lưu từ localStorage ────────────────────────────────
const getSavedTheme = (): ThemeMode => {
  const saved = localStorage.getItem(STORAGE_KEY);
  if (saved === 'light' || saved === 'dark' || saved === 'system') return saved;
  return 'system'; // default
};

// ─── Helper: Áp dụng theme lên <html> element ────────────────────────────────
const applyThemeToDocument = (mode: ThemeMode): void => {
  const html = document.documentElement;

  let resolved: 'light' | 'dark';

  if (mode === 'system') {
    // Lấy preference từ OS
    resolved = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  } else {
    resolved = mode;
  }

  // Gắn/gỡ attribute data-theme
  if (resolved === 'dark') {
    html.setAttribute('data-theme', 'dark');
  } else {
    html.removeAttribute('data-theme');
  }
};

// ─── Khởi tạo theme ngay khi app load (gọi từ main.ts) ──────────────────────
export const initTheme = (): void => {
  const saved = getSavedTheme();
  currentTheme.value = saved;
  applyThemeToDocument(saved);
};

// ─── Composable chính ─────────────────────────────────────────────────────────
export const useTheme = () => {
  // MediaQuery listener để handle 'system' mode khi OS thay đổi
  let mediaQuery: MediaQueryList | null = null;
  const handleSystemThemeChange = () => {
    if (currentTheme.value === 'system') {
      applyThemeToDocument('system');
    }
  };

  onMounted(() => {
    // Đăng ký listener cho system theme change
    mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    mediaQuery.addEventListener('change', handleSystemThemeChange);
  });

  onUnmounted(() => {
    // Dọn dẹp listener khi component unmount
    mediaQuery?.removeEventListener('change', handleSystemThemeChange);
  });

  // Watch: khi currentTheme thay đổi → apply lên document
  watch(currentTheme, (newMode) => {
    applyThemeToDocument(newMode);
  });

  /**
   * Đổi theme — Auto Save vào localStorage
   * Áp dụng ngay lập tức lên toàn bộ UI
   */
  const setTheme = (mode: ThemeMode): void => {
    if (currentTheme.value === mode) return;
    currentTheme.value = mode;
    localStorage.setItem(STORAGE_KEY, mode);
    applyThemeToDocument(mode);
  };

  /**
   * Tính toán: theme thực tế đang áp dụng (resolved) để hiển thị preview
   */
  const resolvedTheme = (): 'light' | 'dark' => {
    if (currentTheme.value === 'system') {
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    return currentTheme.value;
  };

  return {
    currentTheme,   // ThemeMode: 'light' | 'dark' | 'system'
    setTheme,       // (mode: ThemeMode) => void
    resolvedTheme,  // () => 'light' | 'dark'
  };
};
