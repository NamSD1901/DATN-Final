# 🗺️ LỘ TRÌNH VÀ CHI TIẾT SPRINT 1 - KHỎI TẠO DỰ ÁN & CI/CD
## 📝 TÀI LIỆU KẾ HOẠCH TRIỂN KHAI VÀ PHÂN CHIA TÍNH NĂNG (SPRINT 1 MASTER PLAN)

Tài liệu này đặc tả chi tiết kế hoạch triển khai cho **Sprint 1: Khởi Tạo Dự Án & CI/CD**. Phân hệ chịu trách nhiệm xây dựng toàn bộ giải pháp hạ tầng Backend (.NET 8 Clean Architecture), khung ứng dụng Client-side SPA (Vue 3, Vite, TypeScript) và tích hợp hệ thống tự động kiểm thử liên tục (CI/CD Pipeline) bằng GitHub Actions.

---

## 📌 BẢNG LỘ TRÌNH 17 SPRINT PHÁT TRIỂN (MASTER ROADMAP)

Dưới đây là sơ đồ phân chia tính năng hệ thống thành 17 Sprints tinh gọn chạy liên tục bám sát thiết kế tệp tin đặc tả:

| Sprint | Phân hệ (Phase) | Tính năng trọng tâm (Core Features) | Tài liệu sản phẩm / Spec |
| :--- | :--- | :--- | :--- |
| **Sprint 1** | **Phase 1** | **Khởi tạo hạ tầng & CI/CD:** Cấu trúc solution .NET 8 Clean Architecture, dự án Vue 3 Vite SPA và setup GitHub Actions. | [phase1-core-setup.md](file:///e:/DATN/MyPetClinic/plan/sprints/sprint-1/phase1-core-setup.md) |
| **Sprint 2** | **Phase 1** | **Xác thực tài khoản:** Đăng ký tài khoản (PB01) và Đăng nhập JWT (PB02). | `sprint-2/phase1-auth.md` |
| **Sprint 3** | **Phase 1** | **Mật khẩu & Cá nhân:** Quên mật khẩu OTP (PB03) và Hồ sơ cá nhân (PB07). | `sprint-3/phase1-profile.md` |
| **Sprint 4** | **Phase 1** | **Cổng Dịch vụ & Bác sĩ:** Danh mục dịch vụ (PB04) và Đội ngũ bác sĩ (PB05) công khai. | `sprint-4/phase1-catalog.md` |
| **Sprint 5** | **Phase 1** | **Hồ sơ Thú cưng:** CRUD quản lý thú cưng, bảo mật chống tấn công IDOR (PB08). | `sprint-5/phase1-pets.md` |
| **Sprint 6** | **Phase 1** | **Lịch trực & Khung giờ:** Thiết lập lịch làm việc của Bác sĩ và tính toán slot trống khả dụng. | `sprint-6/phase1-slots.md` |
| **Sprint 7** | **Phase 1** | **Đặt lịch trực tuyến:** Khách đặt lịch khám (PB09) và đặt tiêm phòng (PB10) trực tuyến. | `sprint-7/phase1-booking.md` |
| **Sprint 8** | **Phase 1** | **Quản lý & Lịch sử cuộc hẹn:** Xem/Theo dõi lịch hẹn (PB11) và Lịch sử y khoa (PB12). | `sprint-8/phase1-appointments.md` |
| **Sprint 9** | **Phase 2** | **Tiếp nhận Lễ tân:** Lễ tân check-in/walk-in (PB15) và duyệt/hủy lịch hẹn gửi mail (PB17). | `sprint-9/phase2-receptionist.md` |
| **Sprint 10** | **Phase 2** | **Điều phối Hàng đợi:** Cấp số thứ tự tự động (PB18) và màn hình TV Queue Board ngoài sảnh. | `sprint-10/phase2-queue.md` |
| **Sprint 11** | **Phase 2** | **Cổng Bác sĩ:** Bác sĩ xem hàng chờ bệnh nhân (PB20) và Bắt đầu ca khám (PB22). | `sprint-11/phase2-doctor.md` |
| **Sprint 12** | **Phase 2** | **Bệnh án lâm sàng:** Xem bệnh sử thú cưng (PB21) và Bệnh án kê đơn thuốc (PB23 - Trừ kho). | `sprint-12/phase2-clinical.md` |
| **Sprint 13** | **Phase 2** | **Tiêm chủng & Thanh toán:** Thực hiện tiêm chủng (PB24) và Lễ tân in xuất hóa đơn (PB19). | `sprint-13/phase2-billing.md` |
| **Sprint 14** | **Phase 2** | **Quản trị hệ thống:** Admin quản lý người dùng (PB28) và quản lý dịch vụ (PB26, 27). | `sprint-14/phase2-admin-users-services.md` |
| **Sprint 15** | **Phase 2** | **Quản trị kho & Lịch trực:** Admin quản lý kho thuốc (PB29) và quản lý ca trực bác sĩ (PB30). | `sprint-15/phase2-admin-medicines-schedules.md` |
| **Sprint 16** | **Phase 2** | **Thống kê & Blog:** Biểu đồ doanh thu Admin (PB33), Blog cẩm nang (PB06, 32) và Reviews (PB13). | `sprint-16/phase2-admin-reports-blog.md` |
| **Sprint 17** | **Phase 2** | **AI & Background Job:** Gemini AI Chatbot (PB14), Job nhắc lịch tiêm (PB34) và E2E Testing. | `sprint-17/phase2-advanced-ai.md` |

---

## 🛠 SPRINT 1: CORE ARCHITECTURE & CI/CD SETUP

### 1.1. Mục tiêu Sprint (Sprint Goal)
Thiết lập nền móng kỹ thuật vững chắc và hệ thống tích hợp liên tục (CI/CD) cho toàn bộ dự án:
*   **Backend:** Khởi tạo Solution .NET 8 Clean Architecture chuẩn 4 lớp (`Domain`, `Application`, `Infrastructure`, `WebApi`), tạo các liên kết dự án đúng quy tắc Dependency Inversion.
*   **Frontend:** Cấu hình khung ứng dụng Vue 3 SPA sử dụng Vite và TypeScript, cài đặt các thư viện thiết yếu ban đầu.
*   **CI/CD:** Xây dựng workflow tự động kiểm thử và xây dựng mã nguồn (Build & Test verification) để hỗ trợ phản hồi nhanh.

### 1.2. Danh sách công việc (Task Backlog)
1.  `[ ]` **T1:** Khởi tạo Solution `MyPetClinic` và cấu hình liên kết tham chiếu 4 dự án thành viên.
2.  `[ ]` **T2:** Khởi tạo dự án Vue 3 với Vite, TypeScript, cấu trúc mục lục và cài đặt `vue-router`, `pinia`, `axios`.
3.  `[ ]` **T3:** Thiết lập GitHub Actions tự động hóa quy trình build và test backend.

### 1.3. Tiêu chí nghiệm thu (DoD)
*   Solution Backend và dự án Frontend build thành công trên môi trường cục bộ không lỗi.
*   Cấu trúc các lớp Clean Architecture được phân chia rõ ràng.
*   Tệp YAML GitHub Actions chạy đúng cú pháp và kích hoạt tự động thành công khi mở PR vào nhánh `develop`.
