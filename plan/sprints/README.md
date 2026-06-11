# 📂 Chi Tiết Kế Hoạch Thực Thi Từng Sprint - Sprint Execution Specifications

Thư mục này chứa đặc tả chi tiết các bước thực hiện, phân công công việc, API endpoints, thực thể cơ sở dữ liệu liên quan và tiêu chí nghiệm thu cho từng Sprint của dự án **MyPetClinic** theo mô hình 17 Sprints tinh gọn.

---

## 📅 Danh Sách Các Sprint

| Sprint | Chủ Đề / Mục Tiêu Chính | Tài Liệu Chi Tiết |
| :--- | :--- | :--- |
| **Sprint 1** | **Hạ tầng & CI/CD:** Solution Clean Architecture Backend, Vue 3 SPA và GitHub Actions. | [Xem chi tiết Sprint 1](file:///e:/DATN/MyPetClinic/plan/sprints/sprint-1/feature-plan.md) |
| **Sprint 2** | **Xác thực tài khoản:** Đăng ký tài khoản (PB01) và Đăng nhập JWT (PB02). | `sprint-2/feature-plan.md` |
| **Sprint 3** | **Mật khẩu & Cá nhân:** Quên mật khẩu OTP (PB03) và Hồ sơ cá nhân (PB07). | `sprint-3/feature-plan.md` |
| **Sprint 4** | **Cổng Dịch vụ & Bác sĩ:** Danh mục dịch vụ (PB04) và Đội ngũ bác sĩ (PB05) công khai. | `sprint-4/feature-plan.md` |
| **Sprint 5** | **Hồ sơ Thú cưng:** CRUD quản lý thú cưng, bảo mật chống tấn công IDOR (PB08). | `sprint-5/feature-plan.md` |
| **Sprint 6** | **Lịch trực & Khung giờ:** Thiết lập lịch làm việc của Bác sĩ và tính toán slot trống khả dụng. | `sprint-6/feature-plan.md` |
| **Sprint 7** | **Đặt lịch trực tuyến:** Khách đặt lịch khám (PB09) và đặt tiêm phòng (PB10) trực tuyến. | `sprint-7/feature-plan.md` |
| **Sprint 8** | **Quản lý & Lịch sử cuộc hẹn:** Xem/Theo dõi lịch hẹn (PB11) và Lịch sử y khoa (PB12). | `sprint-8/feature-plan.md` |
| **Sprint 9** | **Tiếp nhận Lễ tân:** Lễ tân check-in/walk-in (PB15) và duyệt/hủy lịch hẹn gửi mail (PB17). | `sprint-9/feature-plan.md` |
| **Sprint 10** | **Điều phối Hàng đợi:** Cấp số thứ tự tự động (PB18) và màn hình TV Queue Board ngoài sảnh. | `sprint-10/feature-plan.md` |
| **Sprint 11** | **Cổng Bác sĩ:** Bác sĩ xem hàng chờ bệnh nhân (PB20) và Bắt đầu ca khám (PB22). | `sprint-11/feature-plan.md` |
| **Sprint 12** | **Bệnh án lâm sàng:** Xem bệnh sử thú cưng (PB21) và Bệnh án kê đơn thuốc (PB23 - Trừ kho). | `sprint-12/feature-plan.md` |
| **Sprint 13** | **Tiêm chủng & Thanh toán:** Thực hiện tiêm chủng (PB24) và Lễ tân in xuất hóa đơn (PB19). | `sprint-13/feature-plan.md` |
| **Sprint 14** | **Quản trị hệ thống:** Admin quản lý người dùng (PB28) và quản lý dịch vụ (PB26, 27). | `sprint-14/feature-plan.md` |
| **Sprint 15** | **Quản trị kho & Lịch trực:** Admin quản lý kho thuốc (PB29) và quản lý ca trực bác sĩ (PB30). | `sprint-15/feature-plan.md` |
| **Sprint 16** | **Thống kê & Blog:** Biểu đồ doanh thu Admin (PB33), Blog cẩm nang (PB06, 32) và Reviews (PB13). | `sprint-16/feature-plan.md` |
| **Sprint 17** | **AI & Background Job:** Gemini AI Chatbot (PB14), Job nhắc lịch tiêm (PB34) và E2E Testing. | `sprint-17/feature-plan.md` |

---

## 🛠️ Hướng Dẫn Thực Thi
1. **Trước mỗi Sprint:** Đội ngũ phát triển mở tài liệu chi tiết của Sprint tương ứng để thống nhất các task breakdown và giao diện API.
2. **Trong quá trình Code:** Các thành viên phát triển tính năng trên nhánh Git riêng biệt (`feature/sprint-[id]-[name]`).
3. **Cuối mỗi Sprint:** Thực hiện kiểm thử tích hợp, chạy pass unit test và cập nhật nhật ký tiến độ tại [progress.md](file:///e:/DATN/MyPetClinic/plan/tracking/progress.md).
