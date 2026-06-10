# 📅 Lộ Trình Học Tập & Phát Triển Frontend (MyPetClinic)

Lộ trình học tập chi tiết giúp các thành viên phát triển ứng dụng Single Page Application (SPA) của MyPetClinic dựa trên Vue 3, Vite, và TypeScript.

---

## 📅 Timeline Chi Tiết

### Tuần -2 đến 0 (Chuẩn bị trước Sprint 1)
* **Ngày 1-3:** Làm quen với CSS variables, cấu trúc layout Premium và Flexbox/Grid của dự án tại [style.css](file:///e:/DATN/MyPetClinic/frontend/src/style.css).
* **Ngày 4-7:** Ôn tập và thực hành Vue 3 Composition API với `<script setup>` (ref, reactive, computed, watch).
* **Ngày 8-10:** Đọc hiểu hệ thống định tuyến (Routing) và cơ chế quản lý State cơ bản (Pinia stores).

### Sprint 1-2
* Thiết kế và xây dựng các view tĩnh sử dụng các class `.glass-card`, `.btn-premium`, `.input-premium`.
* Kết nối các API cơ bản thông qua client Axios có kèm `withCredentials: true`.
* Khai báo Store cơ bản lưu thông tin Authentication toàn cục để sử dụng cho Route Guards.

### Sprint 3-4
* Thực hiện tái cấu trúc các model TypeScript tập trung vào thư mục `src/shared/types/`.
* Tối ưu hóa Axios Interceptor để tự động chuyển hướng người dùng khi nhận phản hồi 401 Unauthorized.

### Sprint 5-6
* Triển khai Unit Test cho các Composables và Stores sử dụng Vitest.
* Thiết lập các kịch bản kiểm thử tự động E2E (Playwright hoặc Cypress) cho luồng nghiệp vụ quan trọng.

---

## 📝 Bài Tập Thực Hành Đề Xuất

### Bài tập 1: Thiết kế Trang Chi Tiết Thú Cưng (Pet Detail)
* **Mục tiêu:** Áp dụng Semantic HTML, CSS variables, và Glassmorphic CSS.
* **Yêu cầu:** Tạo một trang đẹp hiển thị avatar thú cưng (dùng hiệu ứng hover đổi màu border Gold), bảng thông tin bệnh án, và nút đặt lịch nổi bật (.btn-premium).

### Bài tập 2: Navigation Guard xác thực Cookie Session
* **Mục tiêu:** Áp dụng Vue Router và Axios Session Credentials.
* **Yêu cầu:** Thiết lập một route mới `/admin/settings` yêu cầu bảo mật. Sử dụng Router Guard để kiểm tra Cookie Session thông qua API `/profile`. Nếu không hợp lệ, redirect về `/login`.

---

## 🏆 Tiêu Chí Đánh Giá Hoàn Thành

| Cấp độ | Tiêu chí kỹ thuật bắt buộc | Phương thức đánh giá |
|:---|:---|:---|
| **Foundation (Nền tảng)** | - Áp dụng đúng các CSS variables của dự án.<br>- Viết HTML5 semantic chuẩn SEO và A11y. | Code Review 1-1 |
| **Core (Cốt lõi)** | - Chuyển đổi thành thạo Options API sang Composition API.<br>- Quản lý Route Guards xác thực chính xác.<br>- Sử dụng thành thạo Pinia Store cơ bản để lưu trữ state. | Pull Request review & Run demo |
| **Advanced (Nâng cao)** | - Cấu hình Axios gửi nhận Cookie Session đúng chuẩn.<br>- Tự động bắt lỗi 401 để điều hướng người dùng thông qua Interceptor.<br>- Quản lý models TypeScript tập trung và tích hợp Pinia Store với Route Guards. | Hệ thống chạy thử nghiệm |
| **Expert (Chuyên gia)** | - Viết Custom Composables quản lý state và fetching độc lập.<br>- Thực hiện Lazy loading các bundle tối ưu.<br>- Đạt tối thiểu 80% coverage cho core logic bằng Vitest và chạy thành công E2E test. | Performance profiling & Test execution logs |
