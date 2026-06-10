# 🔴 Level 4: Expert (Chuyên Gia - Tuần 11-12)

Cấp độ Chuyên gia đòi hỏi năng lực xây dựng chiến lược sản phẩm dài hạn, điều hành các đợt phát hành (Release) quy mô và tự động hóa kiểm thử nâng cao để duy trì sự ổn định của hệ thống.

---

## 🎯 PRD-18: Automation Testing Basics (Playwright)

Khi hệ thống MyPetClinic ngày càng mở rộng, việc hồi quy bằng tay (Manual Regression) rất mất thời gian. QA cần viết các mã kiểm thử tự động (Automation Test) chạy định kỳ cho các luồng cốt lõi.

### Ví dụ Mã Kiểm Thử Luồng Đăng Nhập & Đặt Lịch Bằng Playwright
```typescript
import { test, expect } from '@playwright/test';

test('Luồng đặt lịch khám bệnh trực tuyến thành công', async ({ page }) => {
  // 1. Điều hướng và Đăng nhập
  await page.goto('http://localhost:5173/login');
  await page.fill('#email', 'minhanh@gmail.com');
  await page.fill('#password', 'SecurePassword123');
  await page.click('button[type="submit"]');
  await expect(page).toHaveURL('http://localhost:5173/dashboard');

  // 2. Vào trang Đặt lịch
  await page.click('text=Đặt lịch khám');
  await page.selectOption('#select-pet', { label: 'Mun' });
  await page.selectOption('#select-service', { label: 'Khám sức khỏe định kỳ' });
  await page.fill('#appointment-date', '2026-06-15');
  await page.selectOption('#select-timeslot', '09:00 - 09:30');
  
  // 3. Xác nhận
  await page.click('button#confirm-booking');
  
  // 4. Kiểm chứng kết quả hiển thị trên UI
  const toastMessage = page.locator('.toast-success');
  await expect(toastMessage).toBeVisible();
  await expect(toastMessage).toHaveText('Đặt lịch khám thành công!');
});
```

---

## 🎯 PRD-19: Release Management & Go-Live

PO/QA điều hành chu kỳ phát hành sản phẩm để đưa các tính năng mới tới tay người dùng an toàn.

### 📋 Mẫu Nhật Ký Phát Hành (Release Notes Template)
```markdown
# 🚀 Release Version 1.1.0 - MyPetClinic (10/06/2026)

## 🌟 Tính Năng Mới (New Features)
* **[PB-09] Đặt lịch khám trực tuyến:** Cho phép chủ nuôi chọn dịch vụ, ngày giờ trống và bác sĩ khám nhanh trong 2 phút.
* **[PB-14] Chatbot AI Hỗ Trợ:** Tích hợp Gemini API phục vụ tư vấn dinh dưỡng và chăm sóc thú cưng sơ bộ.

## 🛠️ Cải Tiến & Vá Lỗi (Improvements & Bug Fixes)
* Sửa lỗi nút "Xác nhận đặt lịch" bị nhân đôi request khi nhấn liên tục trên các thiết bị Safari Mobile.
* Tự động trừ kho thuốc ngay khi bác sĩ hoàn thành ca khám.

## ⚠️ Lưu ý Triển khai (Deployment Instructions)
* Cần chạy script SQL cập nhật bảng giá dịch vụ mới (`db_migration_v1.1.sql`) trước khi khởi chạy Backend.
```

---

## 🎯 PRD-20: User Documentation & Training

BA chịu trách nhiệm biên soạn các tài liệu hướng dẫn và tổ chức đào tạo cho nhân viên phòng khám sử dụng phần mềm.
* **Cẩm nang sử dụng (User Manual):** Tài liệu ngắn gọn, nhiều hình ảnh minh họa hướng dẫn Lễ tân cách check-in khách đến, tạo hóa đơn tính tiền; hướng dẫn Bác sĩ cách kê đơn thuốc.
* **Buổi chạy thử nghiệm (Dry-run session):** Cho nhân viên phòng khám tự thao tác trên môi trường Staging trước khi chính thức go-live để tìm ra các điểm bất cập trong thiết kế UX.

---

## 🎯 PRD-21: Product Roadmap & Strategy

Xây dựng lộ trình phát triển MyPetClinic dài hạn sau phiên bản Web MVP:
* **Quý 1:** Tập trung hoàn thiện quy trình lõi: Quản lý hàng chờ (Queue), Lịch hẹn (Booking), Bệnh án (Medical Records) và Dược phẩm (Inventory).
* **Quý 2:** Tích hợp sâu AI Chatbot để phân loại triệu chứng của Pet trước khi đến phòng khám, đưa ra khuyến nghị phòng khám có chuyên khoa tương ứng.
* **Quý 3:** Phát triển phiên bản Mobile App dạng Progressive Web App (PWA) để hỗ trợ Push Notification nhắc lịch tái chủng, nhắc giờ uống thuốc của Pet.
