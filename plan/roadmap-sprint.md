# 🗓️ Lộ Trình Phân Chia Sprint & Sprint Backlog - MyPetClinic

Tài liệu này đặc tả chi tiết **Sprint Backlog** gồm 6 Sprints phát triển và hướng dẫn vận hành cho dự án MyPetClinic.

---

## 🎯 Quy Ước Chung & Định Nghĩa Hoàn Thành (DoD)
*   **Độ dài mỗi Sprint:** 2 tuần (10 ngày làm việc thực tế).
*   **Tổng số Sprint:** 6 Sprint (12 tuần).
*   **Quy trình quản lý:** Agile / Scrum với Daily Stand-up 15 phút đầu giờ.
*   **Định nghĩa Hoàn thành (Definition of Done - DoD):**
    *   Code đã được viết Clean Code, review chéo và merge thành công vào nhánh `develop`.
    *   Đã viết và chạy pass Unit Tests cho các API endpoints và dịch vụ nghiệp vụ cốt lõi.
    *   Đã deploy bản dựng lên môi trường Staging/Development cục bộ và kiểm thử liên kết (Integration Test) thành công.
    *   Không còn lỗi nghiêm trọng thuộc nhóm `Critical` hay `High` tồn đọng.
    *   Acceptance Criteria (AC) của từng User Story được nhóm trưởng/giảng viên nghiệm thu.

---

## 🎯 Sprint 1: Khởi Tạo Dự Án & Xác Thực Người Dùng
**Mục tiêu:** Thiết lập nền tảng kỹ thuật, CI/CD, và hoàn thành luồng Đăng ký/Đăng nhập cơ bản.

| ID | Epic | User Story | Priority | Assigned To | Est. (giờ) | Task Breakdown |
|:---|:---|:---|:---|:---|:---|:---|
| **T1** | Technical | Thiết lập cấu trúc Solution Backend (.NET 8 Clean Architecture) | Critical | Nam | 8 | Tạo các project: Domain, Application, Infrastructure, WebApi. Cấu hình EF Core + PostgreSQL. |
| **T2** | Technical | Thiết lập dự án Frontend (Vue 3 + TypeScript + Tailwind/CSS) | Critical | Phương | 8 | Cài đặt Vite, Vue Router, Pinia, Axios, cấu trúc thư mục chuẩn. |
| **T3** | Technical | Thiết lập CI/CD Pipeline (GitHub Actions) | High | Lâm | 6 | Tự động build & test khi push lên `main`/`develop`. |
| **T4** | Authentication | **PB01 - Đăng ký tài khoản (Backend)** | High | Nam | 8 | Tạo Entity User, API Register, mã hóa BCrypt. |
| **T5** | Authentication | **PB01 - Đăng ký tài khoản (Frontend)** | High | Phương | 6 | Giao diện form đăng ký, validate input, gọi API. |
| **T6** | Authentication | **PB02 - Đăng nhập hệ thống (Backend)** | High | Nam | 6 | API Login, sinh JWT Token, trả về thông tin user + role. |
| **T7** | Authentication | **PB02 - Đăng nhập hệ thống (Frontend)** | High | Phương | 6 | Giao diện form đăng nhập, lưu JWT vào LocalStorage, điều hướng sau login. |
| **T8** | Authentication | **PB03 - Quên mật khẩu (Backend)** | Medium | Hạnh | 8 | API Forgot Password, sinh OTP (6 số), gửi qua Email (SMTP). |
| **T9** | Authentication | **PB03 - Quên mật khẩu (Frontend)** | Medium | Phương | 4 | Giao diện nhập email -> nhập OTP -> đặt mật khẩu mới. |

**Tổng thời gian dự kiến Sprint 1:** ~60 giờ

---

## 🎯 Sprint 2: Trang Chủ & Hồ Sơ Người Dùng - Thú Cưng
**Mục tiêu:** Người dùng có thể xem thông tin phòng khám và quản lý hồ sơ cá nhân, hồ sơ thú cưng.

| ID | Epic | User Story | Priority | Assigned To | Est. (giờ) | Task Breakdown |
|:---|:---|:---|:---|:---|:---|:---|
| **T10** | Homepage | **PB04 - Xem danh sách dịch vụ & bảng giá (Backend)** | High | Hạnh | 6 | API lấy danh sách Service (bao gồm Category). Seed dữ liệu mẫu. |
| **T11** | Homepage | **PB04 - Xem danh sách dịch vụ & bảng giá (Frontend)** | High | Phương | 8 | Giao diện hiển thị dịch vụ dạng card/table, filter theo danh mục, responsive. |
| **T12** | Homepage | **PB05 - Xem đội ngũ bác sĩ (Backend)** | Medium | Hạnh | 4 | API lấy danh sách bác sĩ (từ bảng User với role='Vet'), seed dữ liệu mẫu. |
| **T13** | Homepage | **PB05 - Xem đội ngũ bác sĩ (Frontend)** | Medium | Phương | 6 | Giao diện hiển thị thông tin bác sĩ: ảnh, chuyên môn, kinh nghiệm. |
| **T14** | Customer Portal | **PB07 - Quản lý thông tin cá nhân (Backend)** | High | Nam | 6 | API Get/Update Profile, upload ảnh đại diện (Cloudinary/Local). |
| **T15** | Customer Portal | **PB07 - Quản lý thông tin cá nhân (Frontend)** | High | Lâm | 6 | Giao diện trang Profile, form chỉnh sửa thông tin. |
| **T16** | Customer Portal | **PB08 - Quản lý hồ sơ thú cưng (Backend)** | High | Nam | 10 | CRUD API cho Pet entity (gắn với UserId), validate dữ liệu. |
| **T17** | Customer Portal | **PB08 - Quản lý hồ sơ thú cưng (Frontend)** | High | Lâm | 10 | Giao diện danh sách thú cưng, form thêm/sửa thú cưng (có upload ảnh). |

**Tổng thời gian dự kiến Sprint 2:** ~56 giờ

---

## 🎯 Sprint 3: Đặt Lịch & Quản Lý Lịch Hẹn (Core Feature)
**Mục tiêu:** Hoàn thiện luồng đặt lịch khám/tiêm chủng từ phía khách hàng. Đây là tính năng cốt lõi quan trọng nhất của MVP.

| ID | Epic | User Story | Priority | Assigned To | Est. (giờ) | Task Breakdown |
|:---|:---|:---|:---|:---|:---|:---|
| **T18** | Technical | Thiết kế logic Slot & Ca làm việc (Backend) | Critical | Nam | 12 | Tạo entity Schedule, Slot (khung giờ). API cho Admin cấu hình slot (liên quan PB31). Logic kiểm tra slot còn trống. |
| **T19** | Booking | **PB09 - Đặt lịch khám bệnh (Backend)** | Very High | Nam | 12 | API Create Booking: chọn pet, chọn slot, chọn vet. Validate trùng lịch, quá giới hạn slot. Trạng thái mặc định "Chờ xác nhận". |
| **T20** | Booking | **PB09 - Đặt lịch khám bệnh (Frontend)** | Very High | Phương | 14 | Giao diện multi-step: Chọn thú cưng -> Chọn dịch vụ -> Chọn ngày -> Chọn giờ -> Chọn bác sĩ -> Xác nhận. |
| **T21** | Booking | **PB10 - Đặt lịch tiêm chủng (Backend)** | Very High | Nam | 8 | Tương tự T19, nhưng gắn thêm loại vaccine. Kiểm tra phác đồ tiêm. |
| **T22** | Booking | **PB10 - Đặt lịch tiêm chủng (Frontend)** | Very High | Phương | 10 | Tương tự T20, bổ sung bước chọn vaccine. |
| **T23** | Customer Portal | **PB11 - Quản lý lịch hẹn (Backend)** | High | Hạnh | 6 | API lấy danh sách Booking của user, filter theo trạng thái, phân trang. |
| **T24** | Customer Portal | **PB11 - Quản lý lịch hẹn (Frontend)** | High | Lâm | 8 | Giao diện danh sách lịch hẹn dạng timeline/card, hiển thị trạng thái bằng màu sắc. |

**Tổng thời gian dự kiến Sprint 3:** ~70 giờ

---

## 🎯 Sprint 4: Nghiệp Vụ Lễ Tân & Bác Sĩ (Phần 1)
**Mục tiêu:** Hoàn thiện giao diện và nghiệp vụ cho Lễ tân (Check-in, xác nhận lịch) và Bác sĩ (Xem lịch trực, bắt đầu tiếp nhận ca khám).

| ID | Epic | User Story | Priority | Assigned To | Est. (giờ) | Task Breakdown |
|:---|:---|:---|:---|:---|:---|:---|
| **T25** | Receptionist | **PB15 - Tiếp nhận khách hàng (Backend)** | High | Nam | 8 | API Check-in: tạo Walk-in Booking hoặc cập nhật trạng thái Booking đã đặt thành "Đã Check-in". |
| **T26** | Receptionist | **PB15 - Tiếp nhận khách hàng (Frontend)** | High | Lâm | 10 | Giao diện Lễ tân: tìm kiếm khách hàng bằng SĐT, hiển thị lịch hẹn hôm nay, nút Check-in. |
| **T27** | Receptionist | **PB17 - Xác nhận/Hủy lịch hẹn (Backend)** | Very High | Nam | 8 | API Update Booking Status (Confirmed/Cancelled). Nếu hủy, gửi email thông báo cho khách. |
| **T28** | Receptionist | **PB17 - Xác nhận/Hủy lịch hẹn (Frontend)** | Very High | Lâm | 10 | Giao diện dashboard lịch hẹn chờ duyệt, nút "Xác nhận"/"Từ chối", modal lý do hủy. |
| **T29** | Receptionist | **PB18 - Quản lý hàng đợi (Backend)** | High | Hạnh | 10 | Logic Queue tự động: khi Check-in -> cấp số thứ tự, phân bổ vào phòng khám theo vet. |
| **T30** | Receptionist | **PB18 - Quản lý hàng đợi (Frontend)** | High | Lâm | 10 | Giao diện màn hình lớn hiển thị số thứ tự, phòng khám, trạng thái chờ. Giao diện lễ tân để điều chỉnh thủ công. |
| **T31** | Clinical | **PB20 - Xem lịch khám của bác sĩ (Backend)** | High | Nam | 6 | API lấy danh sách Booking của vet trong ngày, kèm số thứ tự và trạng thái. |
| **T32** | Clinical | **PB20 - Xem lịch khám của bác sĩ (Frontend)** | High | Phương | 8 | Giao diện bác sĩ: dashboard danh sách bệnh nhân chờ, đang khám, đã khám. |
| **T33** | Clinical | **PB22 - Tiếp nhận ca khám (Backend)** | High | Nam | 4 | API Start Examination: chuyển trạng thái Booking thành "Đang khám". |
| **T34** | Clinical | **PB22 - Tiếp nhận ca khám (Frontend)** | High | Phương | 6 | Nút "Bắt đầu khám", chuyển giao diện sang màn hình khám. |

**Tổng thời gian dự kiến Sprint 4:** ~80 giờ

---

## 🎯 Sprint 5: Nghiệp Vụ Bác Sĩ (Phần 2) & Thanh Toán
**Mục tiêu:** Hoàn thiện quy trình khám bệnh, kê đơn thuốc, tiêm chủng và thanh toán hóa đơn.

| ID | Epic | User Story | Priority | Assigned To | Est. (giờ) | Task Breakdown |
|:---|:---|:---|:---|:---|:---|:---|
| **T35** | Clinical | **PB21 - Xem hồ sơ & lịch sử thú cưng (Backend)** | High | Hạnh | 6 | API tổng hợp: thông tin pet, lịch sử bệnh án, mũi tiêm. |
| **T36** | Clinical | **PB21 - Xem hồ sơ & lịch sử thú cưng (Frontend)** | High | Phương | 8 | Giao diện hiển thị timeline lịch sử bệnh án, thông tin sinh hiệu. |
| **T37** | Clinical | **PB23 - Quản lý bệnh án & Kê đơn (Backend)** | Very High | Nam | 14 | Tạo MedicalRecord entity. API lưu chẩn đoán, triệu chứng. API kê đơn: chọn thuốc từ kho -> tự động trừ tồn kho. Validate số lượng tồn. |
| **T38** | Clinical | **PB23 - Quản lý bệnh án & Kê đơn (Frontend)** | Very High | Phương | 14 | Giao diện form khám: nhập triệu chứng/chẩn đoán, autocomplete chọn thuốc, bảng kê đơn, hiển thị tồn kho real-time. |
| **T39** | Clinical | **PB25 - Cập nhật trạng thái lịch hẹn (Backend)** | Medium | Hạnh | 4 | API hoàn thành ca khám -> chuyển Booking thành "Đã hoàn thành", chuyển sang trạng thái chờ thanh toán. |
| **T40** | Receptionist | **PB19 - Thanh toán hóa đơn (Backend)** | Very High | Nam | 10 | API tổng hợp hóa đơn: tiền khám + tiền thuốc + dịch vụ phát sinh. API xác nhận thanh toán (tiền mặt/chuyển khoản). |
| **T41** | Receptionist | **PB19 - Thanh toán hóa đơn (Frontend)** | Very High | Lâm | 10 | Giao diện hóa đơn cho Lễ tân: chi tiết dịch vụ, nút in hóa đơn, xác nhận thanh toán. |
| **T42** | Clinical | **PB24 - Thực hiện tiêm chủng (Backend)** | High | Hạnh | 8 | API ghi nhận mũi tiêm (vaccine, số lô, ngày tiêm, vet thực hiện). Cập nhật lịch sử tiêm của pet. |
| **T43** | Clinical | **PB24 - Thực hiện tiêm chủng (Frontend)** | High | Phương | 6 | Giao diện chọn vaccine, nhập số lô, xác nhận đã tiêm. |
| **T44** | Customer Portal | **PB12 - Xem lịch sử dịch vụ & bệnh án (Frontend)** | Medium | Lâm | 8 | Giao diện khách hàng xem lại toàn bộ lịch sử khám, đơn thuốc cũ (dữ liệu lấy từ T35). |

**Tổng thời gian dự kiến Sprint 5:** ~88 giờ

---

## 🎯 Sprint 6: Admin Dashboard, AI & Các Tính Năng Hoàn Thiện
**Mục tiêu:** Xây dựng trang quản trị, tích hợp AI Chatbot tư vấn, tự động hóa nhắc lịch và tối ưu hóa hệ thống.

| ID | Epic | User Story | Priority | Assigned To | Est. (giờ) | Task Breakdown |
|:---|:---|:---|:---|:---|:---|:---|
| **T45** | Admin | **PB28 - Quản lý người dùng (Backend + Frontend)** | High | Nam & Lâm | 12 | Backend: CRUD user, phân quyền, khóa/mở khóa. Frontend: bảng quản lý user. |
| **T46** | Admin | **PB26, 27 - Quản lý dịch vụ & danh mục (Backend + Frontend)** | High | Hạnh & Phương | 10 | CRUD Service & Category. Giao diện quản lý với form modal. |
| **T47** | Admin | **PB29 - Quản lý thuốc & Vật tư (Backend + Frontend)** | High | Nam & Lâm | 14 | CRUD thuốc, chức năng nhập kho. Cảnh báo tồn kho thấp & hết hạn. Frontend: bảng quản lý + form nhập kho. |
| **T48** | Admin | **PB30 - Quản lý lịch làm việc bác sĩ (Backend + Frontend)** | High | Hạnh & Phương | 12 | Backend: CRUD Schedule cho vet. Frontend: giao diện lịch trực tuần/tháng. |
| **T49** | Admin | **PB31 - Cấu hình khung giờ đặt lịch (Backend + Frontend)** | High | Hạnh & Phương | 8 | API cấu hình Slot (số lượng tối đa/khung giờ). Frontend: form cài đặt. |
| **T50** | Admin | **PB33 - Báo cáo doanh thu (Backend + Frontend)** | Very High | Nam & Lâm | 16 | Backend: API thống kê doanh thu (theo ngày/tháng/vet/dịch vụ). Frontend: biểu đồ trực quan. |
| **T51** | AI | **PB14 - Tích hợp Gemini AI Chatbot (Backend)** | Medium | Hạnh | 10 | Cấu hình Gemini API, tạo Chatbot Service, xây dựng prompt tư vấn y tế thú y. |
| **T52** | AI | **PB14 - Tích hợp Gemini AI Chatbot (Frontend)** | Medium | Phương | 8 | Giao diện Chat Widget (góc phải màn hình), hiển thị hội thoại với AI. |
| **T53** | Notification | **PB34 - Tự động nhắc lịch tái chủng (Backend)** | Medium | Hạnh | 8 | Background Service (Hangfire/Quartz.NET) quét lịch tiêm, gửi email nhắc 3-5 ngày trước hạn. |
| **T54** | Homepage | **PB06 - Xem bài viết/Blog (Backend + Frontend)** | Low | Hạnh & Lâm | 10 | CRUD bài viết. Frontend: danh sách & chi tiết bài viết. |
| **T55** | Admin | **PB32 - Quản lý bài viết & đánh giá (Backend + Frontend)** | Medium | Hạnh & Phương | 8 | Admin duyệt/ẩn bài viết, đánh giá. Frontend: bảng quản lý. |
| **T56** | Customer | **PB13 - Đánh giá dịch vụ (Backend + Frontend)** | Medium | Nam & Lâm | 8 | API gửi đánh giá sau ca khám. Frontend: form đánh giá số sao + nhận xét. |
| **T57** | Technical | Kiểm thử tích hợp & Sửa lỗi tổng thể | Critical | Cả nhóm | 16 | Kiểm thử End-to-End toàn bộ hệ thống, vá lỗi bảo mật, tối ưu hóa database trước khi báo cáo. |

**Tổng thời gian dự kiến Sprint 6:** ~122 giờ
