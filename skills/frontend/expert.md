# 🔴 Level 4: Expert (Chuyên Gia) - Frontend Skills

Bao gồm các kỹ năng chuyên sâu về Custom Composables, Form handling nâng cao, và tối ưu hóa hiệu năng ứng dụng Vue 3 (Vite).

---

## 📋 Danh Sách Kỹ Năng
1. [FE-E01: Reusable State & Fetch Hooks với Custom Composables](#fe-e01-reusable-state--fetch-hooks-với-custom-composables)
2. [FE-E02: Client-side Validation Models](#fe-e02-client-side-validation-models)
3. [FE-E03: Performance Optimization & Vite Analyzer](#fe-e03-performance-optimization--vite-analyzer)
4. [FE-E04: Frontend Testing với Vitest & E2E (Playwright/Cypress)](#fe-e04-frontend-testing-với-vitest--e2e-playwrightcypress)

---

### FE-E01: Reusable State & Fetch Hooks với Custom Composables

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 5-6 |
| **Tại sao cần?** | Tách biệt logic nghiệp vụ khỏi phần giao diện (template), giúp tái sử dụng và kiểm thử logic dễ dàng hơn. |

<details>
<summary><b>📚 Custom Fetch Composable (Click để mở rộng)</b></summary>

```typescript
// composables/useFetch.ts
import { ref } from 'vue';

export function useFetch<T>(apiCall: () => Promise<{ data: T }>) {
  const data = ref<T | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  const execute = async () => {
    isLoading.value = true;
    error.value = null;
    try {
      const response = await apiCall();
      data.value = response.data;
    } catch (err: any) {
      error.value = err.message || 'Lỗi tải dữ liệu';
    } finally {
      isLoading.value = false;
    }
  };

  return { data, isLoading, error, execute };
}
```

</details>

---

### FE-E02: Client-side Validation Models

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Intermediate |
| **Sprint** | Sprint 4-5 |

<details>
<summary><b>📚 Form validation thủ công tối ưu (Click để mở rộng)</b></summary>

```typescript
import { ref } from 'vue';

export function useFormValidation() {
  const errors = ref<Record<string, string>>({});

  const validatePhone = (phone: string): boolean => {
    const phoneRegex = /^(0[3|5|7|8|9])+([0-8]{8})$/;
    if (!phoneRegex.test(phone)) {
      errors.value.phone = 'Số điện thoại không hợp lệ (định dạng Việt Nam)';
      return false;
    }
    delete errors.value.phone;
    return true;
  };

  const validateEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      errors.value.email = 'Email không hợp lệ';
      return false;
    }
    delete errors.value.email;
    return true;
  };

  return { errors, validatePhone, validateEmail };
}
```

</details>

---

### FE-E03: Performance Optimization & Vite Analyzer

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 5-6 |
| **Tại sao cần?** | Giảm thiểu dung lượng bundle tải xuống trình duyệt, giúp tăng tốc độ tải trang ban đầu (LCP - Largest Contentful Paint). |

<details>
<summary><b>📚 Kỹ thuật tối ưu hóa (Click để mở rộng)</b></summary>

1. **Lazy Loading Route:** Sử dụng tính năng dynamic import của Vue Router:
   ```typescript
   {
     path: '/profile',
     component: () => import('../views/Profile.vue') // Load khi truy cập
   }
   ```
2. **Vite Dynamic Import Chunking:** Cấu hình Rollup options trong [vite.config.ts](file:///e:/DATN/MyPetClinic/frontend/vite.config.ts) để tách nhỏ vendor packages.

</details>

---

### FE-E04: Frontend Testing với Vitest & E2E (Playwright/Cypress)

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 5-6 |
| **Tại sao cần?** | Đảm bảo mã nguồn hoạt động chính xác khi nâng cấp hoặc refactor. Viết unit test cho logic (Composables, Stores) và E2E test cho các flow nghiệp vụ chính của hệ thống. |

<details>
<summary><b>📚 Code mẫu Unit Test với Vitest & Component Testing (Click để mở rộng)</b></summary>

Ví dụ viết Unit Test cho custom hook `useFormValidation` bằng **Vitest**:

```typescript
// tests/composables/useFormValidation.spec.ts
import { describe, it, expect } from 'vitest';
import { useFormValidation } from '../../src/composables/useFormValidation';

describe('useFormValidation Spec', () => {
  it('nên validate số điện thoại Việt Nam hợp lệ', () => {
    const { errors, validatePhone } = useFormValidation();

    const isValid = validatePhone('0912345678');
    expect(isValid).toBe(true);
    expect(errors.value.phone).toBeUndefined();
  });

  it('nên phát hiện số điện thoại sai định dạng', () => {
    const { errors, validatePhone } = useFormValidation();

    const isValid = validatePhone('12345');
    expect(isValid).toBe(false);
    expect(errors.value.phone).toBe('Số điện thoại không hợp lệ (định dạng Việt Nam)');
  });
});
```

Ví dụ E2E Test cơ bản bằng **Playwright** cho luồng Login:

```typescript
// e2e/login.spec.ts
import { test, expect } from '@playwright/test';

test('Luồng đăng nhập thành công', async ({ page }) => {
  await page.goto('/login');

  await page.fill('input[type="email"]', 'admin@petclinic.com');
  await page.fill('input[type="password"]', 'Password123!');
  await page.click('button.btn-premium');

  // Sau khi đăng nhập thành công, URL sẽ chuyển hướng về dashboard
  await expect(page).toHaveURL(/\/dashboard/);
});
```

</details>
