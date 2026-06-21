# 📈 Báo Cáo Tiến Độ Dự Án - Development Progress Tracking Log

Tài liệu này ghi nhận chi tiết tiến trình thực thi, trạng thái chi tiết của từng Sprint và các Task (công việc phân rã) trong dự án MyPetClinic nhằm đảm bảo tính minh bạch và dễ dàng theo dõi cho toàn bộ đội ngũ phát triển.

---

## 📊 Tóm Tắt Dự Án (Project Dashboard)

| Hạng mục | Giá trị thực tế | Trạng thái |
| :--- | :--- | :--- |
| **Tổng số Sprints kế hoạch** | 17 Sprints (34 tuần) | Đang thực thi rà soát & tối ưu hóa |
| **Tài liệu thiết kế (Specs)** | 12/12 đặc tả chi tiết | `✅ DONE` |
| **Tiến độ Code của các Sprint** | Hoàn thành nâng cấp chất lượng & tái cấu trúc Sprint 5 | `✅ CODE DONE` |
| **Mốc Sprint hiện tại** | Sprint 6 (Đặt lịch & Ca trực) | `✅ READY` |

---

## 📅 Nhật Ký Tiến Độ Chi Tiết Theo Từng Sprint

### 🎯 Sprint 1: Khởi Tạo Dự Án & CI/CD
*   **Mục tiêu:** Thiết lập nền tảng kiến trúc Backend, dự án Frontend và quy trình tích hợp liên tục (CI/CD).
*   **Danh sách Task chi tiết:**

| Mã Task | Tên công việc / Nội dung chi tiết | Người thực hiện | Trạng thái | Minh chứng & Ghi chú |
| :--- | :--- | :--- | :--- | :--- |
| **T1** | Thiết lập cấu trúc Solution Backend (.NET 10 Clean Architecture) | Nam | `✅ CODE DONE` | 4 layer: Domain, Application, Infrastructure, WebApi. <br> **Cải tiến:** Thêm `.editorconfig` chuẩn hóa code format C#. |
| **T2** | Thiết lập dự án Frontend (Vue 3 + TypeScript + Vanilla CSS/Tailwind) | Phương | `✅ CODE DONE` | Cấu trúc Vue 3 SPA + Vite + TS. Thiết lập Premium Gold Design System & Glassmorphic CSS. |
| **T3** | Thiết lập CI/CD Pipeline (GitHub Actions) | Lâm | `✅ CODE DONE` | File `dotnet-build-test.yml` và `vue-build.yml`. <br> **Cải tiến:** Tích hợp `dotnet format --verify-no-changes` tự động kiểm tra định dạng code. |

---

### 🎯 Sprint 2: Xác Thực Tài Khoản (Authentication)
*   **Mục tiêu:** Hoàn thiện luồng Đăng ký và Đăng nhập.
*   **Danh sách Task chi tiết:**

| Mã Task | Tên công việc / Nội dung chi tiết | Người thực hiện | Trạng thái | Minh chứng & Ghi chú |
| :--- | :--- | :--- | :--- | :--- |
| **T4** | PB01 - Đăng ký tài khoản (Backend API & mã hóa BCrypt) | Nam | `✅ CODE DONE` | Viết logic băm mật khẩu, lưu User vào DB. <br> **Cải tiến:** Sửa đổi DTO requests thêm `required` giải quyết 100% warnings nullable. |
| **T5** | PB01 - Đăng ký tài khoản (Frontend Form UI & Validation) | Phương | `✅ CODE DONE` | Giao diện đăng ký nhập liệu, validation lỗi phía client. |
| **T6** | PB02 - Đăng nhập hệ thống (Cookie Session Auth) | Nam | `✅ CODE DONE` | Đăng nhập hệ thống qua Cookie session, phân quyền dựa trên Roles (RBAC). <br> **Cải tiến:** Dọn dẹp câu lệnh `using` trùng lặp trong controller. |
| **T7** | PB02 - Đăng nhập hệ thống (Frontend store & Router guard) | Phương | `✅ CODE DONE` | Router guard liên kết kiểm tra session bằng gọi API `/profile` an toàn. |

---

### 🎯 Sprint 3: Quên Mật Khẩu & Hồ Sơ Cá Nhân
*   **Mục tiêu:** Xử lý xác thực OTP và thông tin tài khoản người dùng.
*   **Danh sách Task chi tiết:**

| Mã Task | Tên công việc / Nội dung chi tiết | Người thực hiện | Trạng thái | Minh chứng & Ghi chú |
| :--- | :--- | :--- | :--- | :--- |
| **T8** | PB03 - Quên mật khẩu (Backend sinh OTP & gửi SMTP Email) | Hạnh | `✅ CODE DONE` | Sinh OTP ngẫu nhiên, lưu cache 5 phút và gửi SMTP email html. |
| **T9** | PB03 - Quên mật khẩu (Frontend wizard nhập OTP & reset password) | Phương | `✅ CODE DONE` | Bộ form nhập OTP và đổi mật khẩu an toàn phía client. |
| **T14** | PB07 - Quản lý thông tin cá nhân (Backend Get/Update Profile) | Nam | `✅ CODE DONE` | API cập nhật profile và tải ảnh đại diện lên server. <br> **Cải tiến:** Thêm cơ chế fallback an toàn cho `WebRootPath` chống crash. Thêm `UseStaticFiles()` trong Program.cs phục vụ file tĩnh. |
| **T15** | PB07 - Quản lý thông tin cá nhân (Frontend Form Profile) | Lâm | `✅ CODE DONE` | Giao diện chỉnh sửa thông tin cá nhân và thay đổi avatar trực quan. <br> **Cải tiến:** Thêm nút "Chỉnh sửa hồ sơ" vào Dashboard, sửa lỗi avatar vỡ ở Dashboard. |

---

### 🎯 Sprint 4: Cổng Dịch Vụ & Bác Sĩ Công Khai
*   **Mục tiêu:** Khách vãng lai có thể tra cứu thông tin dịch vụ và bác sĩ tại phòng khám.
*   **Danh sách Task chi tiết:**

| Mã Task | Tên công việc / Nội dung chi tiết | Người thực hiện | Trạng thái | Minh chứng & Ghi chú |
| :--- | :--- | :--- | :--- | :--- |
| **T10** | PB04 - Xem danh sách dịch vụ & bảng giá (Backend API & Seed Data) | Hạnh | `✅ CODE DONE` | Bảng giá và danh mục dịch vụ trong DB. <br> **Cải tiến:** Tạo mới `ServiceController.cs` công khai, tích hợp `IMemoryCache` lưu `"Services_All"` hạn sống 1h. |
| **T11** | PB04 - Xem danh sách dịch vụ & bảng giá (Frontend Cards view & Filter) | Phương | `✅ CODE DONE` | Các trang chi tiết dịch vụ, giao diện tìm kiếm và lọc phân loại dịch vụ. |
| **T12** | PB05 - Xem đội ngũ bác sĩ (Backend API lọc Vet) | Hạnh | `✅ CODE DONE` | Lọc danh sách người dùng có quyền là `doctor`. <br> **Cải tiến:** Tạo mới `DoctorsController.cs` công khai cho khách vãng lai `/api/doctors`. |
| **T13** | PB05 - Xem đội ngũ bác sĩ (Frontend Doctors Grid view) | Phương | `✅ CODE DONE` | Grid hiển thị danh sách bác sĩ thú y trực quan ngoài trang chủ. |

---

### 🎯 Sprint 5: Hồ Sơ Thú Cưng (Tái Cấu Trúc Chất Lượng Cao & Chống IDOR)
*   **Mục tiêu:** Nâng cấp chất lượng code, tối ưu truy vấn, bảo mật IDOR qua ActionFilter và xây dựng giao diện Premium Glassmorphic.
*   **Danh sách Task chi tiết:**

| Mã Task | Tên công việc / Nội dung chi tiết | Người thực hiện | Trạng thái | Minh chứng & Ghi chú |
| :--- | :--- | :--- | :--- | :--- |
| **T16** | PB08 - Quản lý hồ sơ thú cưng (Backend CRUD & Chống IDOR bằng ActionFilter) | Nam | `✅ CODE DONE` | Hoàn thành ActionFilter chặn đứng IDOR, tối ưu LINQ queries và phủ đầy đủ Unit Tests. |
| **T17** | PB08 - Quản lý hồ sơ thú cưng (Frontend Grid & Modal Form Glassmorphic) | Lâm | `✅ CODE DONE` | Tái cấu trúc giao diện danh sách sang Premium Light-Theme Glassmorphism, sửa lỗi hiển thị. <br> **Cải tiến:** Redesign toàn diện trang Chi tiết Hồ sơ Thú cưng (`PetProfile.vue`) sang kiến trúc Clean Modern Dashboard (Nền trắng tinh tế, biểu đồ Chart.js theo dõi cân nặng, hệ thống Cards chi tiết trực quan). |

---

### 🎯 Sprint 6: Ca Làm Việc & Khung Giờ Bác Sĩ
*   **Mục tiêu:** Thiết lập cấu trúc ca trực bác sĩ và tính toán slot trống.
*   **Trạng thái chung:** `✅ CODE DONE` (T18).

---

### 🎯 Sprint 7: Đặt Lịch Khám & Tiêm Chủng Trực Tuyến
*   **Mục tiêu:** Đặt lịch khám và tiêm chủng trực tuyến ngăn double-booking & tối ưu hóa trải nghiệm khách hàng (Realtime Available Slots Picker).
*   **Trạng thái chung:** `✅ CODE DONE`
*   **Chi tiết:** 
    *   Backend Serializable Transaction đặt lịch (T19).
    *   Frontend Wizard đặt lịch động (T20).
    *   API Vaccine & kho (T21).
    *   Frontend chọn Vaccine (T22).
    *   **Cải tiến tối ưu hóa UX & Concurrency:** 
        *   Tích hợp endpoint `GET /api/my-appointments/available-slots` tự động truy vấn lịch trực bác sĩ và tính toán slot trống thời gian thực.
        *   Refactor toàn bộ `BookingModal.vue` từ form truyền thống thành **Multi-step Booking Wizard (4 Bước)** sang trọng (Chọn Thú cưng -> Dịch vụ -> Giờ khám -> Xác nhận) bám sát trải nghiệm người dùng Premium.
        *   Cải tiến Custom Calendar và giao diện Time Slots dạng viên thuốc (Pills) chia sáng/chiều mang lại trải nghiệm mượt mà không cần dùng input date native.
        *   Thay đổi cơ chế check trùng giờ khám của bác sĩ sang toán tử bất đẳng thức nghiêm ngặt (`>` và `<`), cho phép đặt lịch liên tiếp (back-to-back appointments) không bị kẹt biên.
        *   Bổ sung Unit Test xác thực tính toán slot rảnh chính xác.
    *   **Thực thi luồng Nghiệp vụ Khách hàng (Customer Business Rules):**
        *   Tách riêng logic `CustomerAppointmentService` để chặn: Cấp cứu, Quá sát giờ (< 2 tiếng), Ngoài giờ hoạt động (08:00 - 20:00), Spam booking (cùng 1 pet < 2 tiếng).
        *   Khóa chặn đặt lịch online nếu User có >= 3 lần No-show.
        *   Đưa vào danh sách chờ duyệt (`pending_approval`) nếu User có >= 3 lần Cancel trong 30 ngày qua.
        *   **Cập nhật mới:** Gỡ bỏ tính năng tự chọn bác sĩ. Hệ thống **Bắt buộc Tự động phân công** (ẩn dropdown trên UI và đè `DoctorId = Guid.Empty` tại Backend) dựa theo Nhóm dịch vụ (Khám bệnh -> BS. Long/Tuấn; Tiêm phòng -> BS. Chung/Hà).


### 🎯 Sprint 8: Theo Dõi Cuộc Hẹn & Lịch Sử
*   **Mục tiêu:** Theo dõi và xem lại lịch sử y tế qua Dashboard Khách hàng.
*   **Trạng thái chung:** `✅ CODE DONE` (T23, T24, T44).
*   **Chi tiết & Cải tiến triển khai:**
    *   Tái thiết kế toàn bộ **Customer Overview Tab (Trang Tổng Quan Khách Hàng)** sang phong cách Premium Glassmorphism.
    *   Xây dựng hệ thống 3 thẻ thống kê động: Lịch hẹn sắp tới, Số lượng thú cưng, Số lần khám bệnh dựa trên dữ liệu thật.
    *   Phân rã giao diện theo tỷ lệ 60/40: Bên trái là Grid thẻ thú cưng nổi bật, bên phải là Timeline (dòng thời gian) Hoạt động y tế gần đây gọi từ API lịch sử cuộc hẹn.
    *   Tách rời hoàn toàn giao diện giữa Customer và Admin/Staff trong `Dashboard.vue` thành các Component độc lập, gọn gàng dễ bảo trì.

### 🎯 Sprint 9: Tiếp Nhận & Duyệt Lịch Hẹn
*   **Mục tiêu:** Cổng lễ tân duyệt lịch hẹn và check-in.
*   **Trạng thái chung:** `✅ CODE DONE` (T25, T26, T27, T28).

---

### 🎯 Sprint 10: Điều Phối Hàng Đợi
*   **Mục tiêu:** Cấp số thứ tự tự động (định dạng `Q-XXX`) cho hàng khám.
*   **Trạng thái chung:** `✅ CODE DONE`
*   **Chi tiết:** 
    *   Tự động cấp số thứ tự khám (T29).
    *   Thiết kế Bảng điều khiển Lễ tân (T30).
    *   **Cải tiến triển khai:**
        *   Cải tiến API lấy hàng đợi thời gian thực.
        *   Tích hợp bộ máy phát âm thanh giọng nói tiếng Việt (Web Speech API) tự động đọc số thứ tự khi bác sĩ kích hoạt khám ca mới.
        *   Đồng bộ định dạng số thứ tự `Q-XXX` (ví dụ `Q-001`) xuyên suốt các giao diện quản lý.


---

### 🎯 Sprint 11: Cổng Bác Sĩ & Tiếp Nhận Khám
*   **Mục tiêu:** Bác sĩ theo dõi hàng đợi khám và kích hoạt ca khám (định dạng số thứ tự `Q-XXX`).
*   **Trạng thái chung:** `✅ CODE DONE`
*   **Chi tiết:** 
    *   Xem lịch khám của bác sĩ (T31) & Dashboard hàng đợi của Bác sĩ (T32).
    *   Tiếp nhận ca khám lâm sàng - Đổi trạng thái (T33) & Kích hoạt chuyển trang khám (T34).
    *   **Cải tiến bảo mật & UX:**
        *   Tích hợp IDOR Protection vào `DoctorController.cs` (chặn bác sĩ A can thiệp bắt đầu/kết thúc ca khám của bác sĩ B).
        *   Tạo mới component `DoctorQueueTab.vue` làm Dashboard hàng khám chuyên dụng cho bác sĩ đang đăng nhập, hiển thị thông tin chi tiết và hỗ trợ click bắt đầu/tiếp tục khám.
        *   Đồng bộ định dạng `Q-XXX` và nhúng thành công vào Dashboard chính khi activeTab là `doctor-cases`.


---

### 🎯 Sprint 12: Bệnh Án & Kê Đơn Thuốc (Trừ Kho Tự Động)
*   **Mục tiêu:** Khám lâm sàng, kê đơn và trừ kho thuốc an toàn.
*   **Trạng thái chung:** `✅ CODE DONE`
*   **Chi tiết:**
    *   Xem bệnh sử & lịch sử thú cưng (T35 & T36).
    *   Giao dịch trừ kho an toàn rollback khi hết thuốc (T37).
    *   Giao diện nhập bệnh án & kê đơn thuốc động cảnh báo tồn kho (T38).
    *   Cập nhật trạng thái cuộc hẹn thành completed (T39).
    *   **Cải tiến triển khai:**
        *   Tạo mới `MedicalRecordsController.cs` và `MedicinesController.cs` an toàn và phân quyền đầy đủ.
        *   Triển khai `MedicalRecordService.cs` tách biệt logic EF Core ra khỏi Application Layer, truy vấn lồng liên quan đến Prescription và Medicine qua in-memory mapping tối ưu.
        *   Cải tiến toàn diện `MedicalRecordsTab.vue`: Chuyển đổi từ giao diện S.O.A.P Accordion sang thiết kế 2 Sub-tabs (Phiếu Điều Trị & Hồ sơ Thú cưng). Giao diện phẳng phân cột tự động điều chỉnh theo Khám Bệnh/Tiêm Phòng với Form nhập liệu lớn, tiện dụng và mang đậm phong cách Pet Clinic. Tích hợp cảnh báo tồn kho thời gian thực.
        *   **[MỚI CẬP NHẬT]** Nâng cấp Bệnh án Khám bệnh theo tiêu chuẩn **S.O.A.P** (S-Chủ quan, O-Khách quan, A-Đánh giá, P-Kế hoạch) trên giao diện `ConsultationRecordTab.vue`, hỗ trợ hàng chục trường dữ liệu chi tiết và lưu trữ an toàn bằng JSON Serialize không làm phá vỡ DB schema cũ.
        *   Bổ sung Unit Test bao phủ 100% các kịch bản thành công và rollback giao dịch khi hết thuốc.

---

### 🎯 Sprint 13: Tiêm Chủng Vaccine & Thanh Toán Hóa Đơn
*   **Mục tiêu:** Thực hiện tiêm chủng và xuất hóa đơn thanh toán tại quầy.
*   **Trạng thái chung:** `✅ CODE DONE`
*   **Chi tiết:**
    *   Thanh toán hóa đơn - kết xuất tự động dịch vụ + thuốc kê đơn (T40).
    *   In hóa đơn HTML `@media print` và xác nhận thanh toán phía Lễ tân (T41).
    *   Thực hiện tiêm chủng - ghi nhận mũi tiêm & tự động tính NextDueDate dựa trên chu kỳ vắc-xin (T42).
    *   Form nhập vắc-xin & số lô (T43).
    *   **Cải tiến triển khai:**
        *   Sửa lỗi nạp danh sách thuốc kê đơn (`prescription.PrescriptionItems` bị null) trong `InvoiceService.cs` bằng cơ chế tìm kiếm repository phân cấp độc lập.
        *   Tạo mới `VaccinationRecordDto`, `IVaccinationService`, `VaccinationService` và `VaccinationsController` hoàn chỉnh logic tiêm chủng.
        *   **[MỚI CẬP NHẬT]** Nâng cấp toàn diện kiến trúc Bệnh án tiêm chủng theo tiêu chuẩn **S.O.A.P** (S-Chủ quan, O-Khách quan, A-Đánh giá, P-Kế hoạch). Giao diện `VaccinationRecordTab.vue` được thiết kế mới dưới dạng Split-pane siêu trực quan, xử lý tự động Hoãn tiêm (chỉ sinh phí khám) và Trừ kho lô Vắc-xin an toàn.
        *   Bổ sung Unit Test bao phủ 100% logic tính toán ngày tái chủng tự động (`NextDueDate`) và kiểm kho vắc-xin.

---

### 🎯 Sprint 14: Quản Trị Hệ Thống & Cấu Hình Dịch Vụ
*   **Mục tiêu:** Quản trị viên quản lý người dùng, danh mục dịch vụ và khung giờ.
*   **Trạng thái chung:** `✅ CODE DONE` (T45, T46, T49).

---

### 🎯 Sprint 15: Quản Trị Kho Thuốc, Vắc-xin & Ca Trực Bác Sĩ
*   **Mục tiêu:** Quản trị kho dược phẩm, kho vắc-xin và phân ca trực của bác sĩ.
*   **Trạng thái chung:** `✅ CODE DONE` (T47, T48, T48b).
*   **Chi tiết:**
    *   T48b: Quản lý Vắc-xin & Lô nhập ở Admin (`✅ CODE DONE` - Backend DTO, AdminService logic tự động StockQuantity, API Endpoints, Frontend `VaccinesAdminTab.vue` UI chia 2 cấp).

---

### 🎯 Sprint 16: Báo Cáo Doanh Thu & Cổng Tin Tức
*   **Mục tiêu:** Thống kê doanh thu, quản lý bài viết/đánh giá của khách hàng.
*   **Trạng thái chung:** `✅ CODE DONE` (T50, T54, T55, T56).

---

### 🎯 Sprint 17: Trợ Lý Gemini AI, Nhắc Lịch Tự Động & E2E Test
*   **Mục tiêu:** Chatbot AI, background job nhắc lịch và tối ưu hóa hệ thống.
*   **Trạng thái chung:** `✅ CODE DONE` (T51, T52, T53, T57).
