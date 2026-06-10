# 🟢 Level 1: QA Foundation (Sprint 1-2)

Mục tiêu của cấp độ này là xây dựng nền tảng tư duy kiểm thử vững chắc, thiết kế kịch bản kiểm thử hộp đen chuẩn xác và hiểu rõ cấu trúc của một Kế hoạch kiểm thử tổng thể.

---

## 🎯 QA-01: Testing Fundamentals & Mindset

### 1. Kế Hoạch Kiểm Thử Tổng Thể (Master Test Plan)
Một Kế hoạch kiểm thử chuẩn cho dự án **MyPetClinic** cần bao gồm các thành phần sau:
* **Mục tiêu:** Đảm bảo hệ thống hoạt động chính xác, ổn định, bảo mật và phản hồi nhanh ($< 3s$ cho 95% requests).
* **Phạm vi kiểm thử (Scope):**
  - *In-Scope:* Kiểm thử chức năng, API, Database, UI/UX Responsive, tích hợp luồng E2E, Regression và UAT.
  - *Out-of-Scope:* Kiểm thử tải cực hạn ($> 1000$ users đồng thời), kiểm thử bảo mật chuyên sâu (Penetration Test), và test native app trên iOS/Android (chỉ tập trung test PWA và Responsive).
* **Định nghĩa mức độ nghiêm trọng của lỗi (Severity Classification):**
  - 🔴 **Critical:** Lỗi sập hệ thống (Crash), mất dữ liệu, lộ thông tin bảo mật, không thể đăng nhập.
  - 🟠 **High:** Lỗi vỡ luồng nghiệp vụ cốt lõi mà không có cách giải quyết tạm thời (ví dụ: không thể đặt lịch khám).
  - 🟡 **Medium:** Lỗi tính năng nhưng có giải pháp thay thế tạm thời (ví dụ: tính toán sai hiển thị giá, nhưng khi thanh toán DB vẫn đúng).
  - 🟢 **Low:** Lỗi chính tả, lệch giao diện nhẹ, sai màu sắc nút bấm.

### 2. Ma Trận Kiểm Thử Dựa Trên Rủi Ro (Risk-based Testing)
QA cần phân phối nỗ lực kiểm thử dựa trên công thức: $Risk = Xác\ suất\ lỗi \times Mức\ độ\ ảnh\ hưởng$.
* **Mức độ Cao (60% nỗ lực):** Module Đăng nhập/Xác thực (PB01-03), Đặt lịch khám (PB09-10), Thanh toán hóa đơn (PB19).
* **Mức độ Trung bình (25% nỗ lực):** Quản lý bệnh án & Kê đơn (PB23), Quản lý hàng chờ (PB18).
* **Mức độ Thấp (15% nỗ lực):** Dashboard Admin (PB33), Blog (PB06), Trợ lý ảo AI (PB14).

---

## 🎯 QA-02: Manual Testing Techniques

Nắm vững các phương pháp chạy test thủ công:
* **Smoke Testing (Kiểm thử khói):** Chạy các luồng nghiệp vụ cơ bản nhất (Đăng nhập -> Xem Dashboard -> Đăng xuất) sau mỗi bản build để đảm bảo hệ thống không bị crash lập tức.
* **Sanity Testing (Kiểm thử độ ổn định):** Kiểm thử tập trung vào một vùng chức năng cụ thể vừa được thay đổi hoặc sửa lỗi để xem nó có hoạt động ổn định không.
* **Regression Testing (Kiểm thử hồi quy):** Chạy lại toàn bộ test suite cốt lõi để đảm bảo code mới viết không làm hỏng các tính năng cũ đã chạy tốt.

---

## 🎯 QA-03: Test Case Design (Black-box Techniques)

Dưới đây là bộ Test Suite mẫu thiết kế cho **PB09 (Đặt lịch khám trực tuyến)** sử dụng các kỹ thuật phân tích giá trị biên và phân vùng tương đương:

### 📋 TEST SUITE: PB09 - ĐẶT LỊCH KHÁM BỆNH (Mẫu Chi Tiết)

#### 🔴 KỊCH BẢN UY TIÊN CAO (CRITICAL)
* **TC-PB09-001: Happy Path - Đặt lịch thành công**
  - *Precondition:* Khách hàng đã đăng nhập, có thú cưng Milo, slot 09:00 ngày 15/12/2026 trống.
  - *Steps:* 1. Chọn pet "Milo" -> 2. Chọn dịch vụ "Khám tổng quát" -> 3. Chọn ngày 15/12/2026 -> 4. Chọn slot 09:00 -> 5. Nhấn "Xác nhận".
  - *Expected:* Lịch hẹn được tạo ở trạng thái "Pending", slot trống giảm đi 1, hiển thị Toast thành công.
* **TC-PB09-002: Đặt lịch khi slot cuối cùng (Giá trị biên)**
  - *Precondition:* Slot 09:00 ngày 15/12 chỉ còn đúng 1 slot trống.
  - *Steps:* Thực hiện đặt lịch tương tự Happy Path.
  - *Expected:* Đặt thành công, slot chuyển sang trạng thái "Hết chỗ" (0 slots).
* **TC-PB09-003: Đặt lịch khi slot đã hết**
  - *Precondition:* Slot 09:00 đã hết chỗ (0 slots).
  - *Steps:* Cố tình chọn slot 09:00 và xác nhận.
  - *Expected:* Hệ thống hiển thị "Hết chỗ" ở ô chọn, không cho phép nhấn Xác nhận.
* **TC-PB09-004: Đặt lịch trùng giờ cho cùng một thú cưng**
  - *Precondition:* Pet "Milo" đã có lịch hẹn lúc 09:00 ngày 15/12.
  - *Steps:* Thực hiện đặt thêm 1 lịch khác cho Milo vào đúng ngày giờ này.
  - *Expected:* Hệ thống báo lỗi: "Thú cưng này đã có lịch hẹn vào khung giờ đã chọn".
* **TC-PB09-005: Đặt lịch với ngày trong quá khứ**
  - *Steps:* Chọn ngày hôm qua trên lịch biểu.
  - *Expected:* Lịch biểu chuyển màu xám và vô hiệu hóa (disabled) các ngày trong quá khứ.

#### 🟠 KỊCH BẢN MỨC ĐỘ CAO (HIGH)
* **TC-PB09-006: Đặt lịch không chỉ định Bác sĩ (Bác sĩ bất kỳ)**
  - *Expected:* Đặt thành công, hệ thống tự gán (assign) ngẫu nhiên cho một bác sĩ trống lịch.
* **TC-PB09-007: Đặt lịch vào ngày nghỉ của phòng khám (Chủ nhật)**
  - *Expected:* Lịch biểu không cho phép chọn ngày Chủ nhật.
* **TC-PB09-009: Đặt lịch khi tài khoản chưa khai báo thú cưng**
  - *Expected:* Giao diện hiển thị nút "Thêm thú cưng ngay" thay vì form chọn lịch.

---

## 🎯 QA-05: Browser DevTools for Testing

QA cần thành thạo sử dụng Chrome DevTools (F12) để hỗ trợ tìm lỗi:
* **Elements Tab:** Kiểm tra mã HTML, CSS xem giao diện có bị đè layout, kiểm tra thuộc tính ID/Class của các nút bấm phục vụ viết Automation Test.
* **Console Tab:** Xem các lỗi JavaScript runtime (màu đỏ) khi thao tác trên giao diện.
* **Network Tab:** Theo dõi các request API đi và về:
  - Xem Request Payload gửi đi có đúng dữ liệu nhập từ UI không.
  - Xem Response JSON trả về từ Backend .NET để bắt lỗi API mà không cần đoán mò.
  - Kiểm tra Header xem Cookie Session/Token JWT có được đính kèm chính xác không.
