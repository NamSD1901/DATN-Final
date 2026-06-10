# 🔴 Level 4: QA Expert (Sprint 6+)

Cấp độ Chuyên gia tập trung vào việc tự động hóa toàn bộ quy trình kiểm thử giao diện người dùng (E2E Testing), tích hợp vào đường ống CI/CD tự động và áp dụng các mô hình kỹ thuật nâng cao để cải tiến chất lượng phần mềm liên tục.

---

## 🎯 QA-18: Automation Testing (Playwright)

**Playwright** là công cụ kiểm thử tự động E2E hàng đầu cho các ứng dụng Vue 3 / Vite hiện nay nhờ tốc độ chạy nhanh, hỗ trợ đa trình duyệt và cơ chế tự động chờ (Auto-wait) thông minh.

### 💻 Kịch Bản Kiểm Thử Tự Động Đầu-Cuối (End-to-End Test Script)

```typescript
import { test, expect } from '@playwright/test';

test.describe('Kiểm thử luồng đặt lịch khám bệnh cốt lõi', () => {
    
    test.beforeEach(async ({ page }) => {
        // Thực hiện đăng nhập trước mỗi ca test
        await page.goto('http://localhost:5173/login');
        await page.fill('input[type="email"]', 'customer_test@petclinic.com');
        await page.fill('input[type="password"]', 'SecurePassword123');
        await page.click('button[type="submit"]');
        
        // Xác minh đăng nhập thành công và được chuyển hướng về trang chủ
        await expect(page).toHaveURL('http://localhost:5173/dashboard');
    });

    test('Đặt lịch khám bệnh thành công', async ({ page }) => {
        // 1. Nhấn nút mở form đặt lịch
        await page.click('a[href="/booking"]');
        
        // 2. Điền thông tin vào form
        await page.selectOption('select#pet-select', { label: 'Milo' });
        await page.selectOption('select#service-select', { label: 'Khám tổng quát' });
        await page.fill('input#appointment-date', '2026-06-15');
        await page.selectOption('select#slot-select', '09:00 - 09:30');
        
        // 3. Submit form đặt lịch
        await page.click('button#submit-booking-btn');
        
        // 4. Xác nhận modal popup
        await page.click('button#modal-confirm-btn');
        
        // 5. Kiểm tra thông báo thành công hiển thị trên UI
        const successToast = page.locator('.toast-success');
        await expect(successToast).toBeVisible();
        await expect(successToast).toContainText('Đặt lịch khám thành công!');
    });
});
```

---

## 🎯 QA-19: CI/CD Test Integration

QA cần biết cách tích hợp bộ chạy test tự động vào quy trình Git. Mỗi khi lập trình viên tạo Pull Request mới hoặc push code lên nhánh chính `main`, hệ thống tự động chạy toàn bộ script test để phát hiện lỗi sớm.

### 🔧 Cấu Hình GitHub Actions Chạy Playwright Tests Tự Động (`.github/workflows/playwright.yml`)

```yaml
name: Playwright Tests
on:
  push:
    branches: [ main, master ]
  pull_request:
    branches: [ main, master ]
jobs:
  test:
    timeout-minutes: 60
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - uses: actions/setup-node@v4
      with:
        node-version: 18
        
    - name: Install dependencies
      run: npm ci
      working-directory: ./frontend
      
    - name: Install Playwright Browsers
      run: npx playwright install --with-deps
      working-directory: ./frontend
      
    - name: Run Playwright tests
      run: npx playwright test
      working-directory: ./frontend
      
    - uses: actions/upload-artifact@v4
      if: always()
      with:
        name: playwright-report
        path: frontend/playwright-report/
        retention-days: 30
```

---

## 🎯 QA-20: Chaos Engineering Basics

Kiểm thử hỗn loạn (Chaos Engineering) là hình thức cố tình tiêm lỗi vào hệ thống để kiểm tra khả năng phục hồi của hệ thống.
* **Fault Injection (Tiêm lỗi):** Cố tình ngắt kết nối mạng database PostgreSQL trong 5 giây trong lúc khách hàng đang thanh toán hóa đơn.
* **Mục tiêu kiểm thử:** Hệ thống .NET 8 Backend có kích hoạt chế độ tự động thử lại (Retry policy via Polly library) để kết nối lại database và đảm bảo giao dịch không bị đứt quãng hay không, hoặc Frontend hiển thị đúng thông báo lỗi thân thiện thay vì hiển thị màn hình trắng (White screen of death).

---

## 🎯 QA-21: Quality Process Improvement

QA Expert đóng vai trò cải tiến liên tục quy trình sản xuất phần mềm:
* Áp dụng phương pháp kiểm thử sớm (**Shift-Left Testing**): Tham gia cùng BA ngay từ khi thiết kế nghiệp vụ và vẽ UI mockup để chỉ ra các kịch bản thiếu logic trước khi Dev gõ dòng code đầu tiên.
* Thiết lập hệ thống giám sát cảnh báo lỗi (Real-time monitoring & Alerting) trên môi trường Production bằng các công cụ như Sentry, Grafana để tự động thông báo lỗi phát sinh cho team trước khi khách hàng phàn nàn.
