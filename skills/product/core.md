# 🟡 Level 2: Core (Cốt Lõi - Tuần 3-6)

Cấp độ Cốt lõi tập trung vào việc làm chủ các công cụ mô hình hóa quy trình nghiệp vụ, thiết kế test case nâng cao dựa trên phân tích logic hệ thống, quản lý lỗi hiệu quả và quy trình tổ chức backlog chuyên nghiệp.

---

## 🎯 PRD-06: Business Process Modeling (BPMN)

BA cần biết cách vẽ sơ đồ phân làn (**Swimlanes**) để mô tả luồng di chuyển dữ liệu và sự tương tác giữa các bộ phận của phòng khám thú y.

```
┌────────────────────────────────────────────────────────────────────────┐
│               SƠ ĐỒ PHÂN LÀN QUY TRÌNH CHECK-IN & KHÁM BỆNH            │
├────────────────────────────────────────────────────────────────────────┤
│ CHỦ NUÔI  │  [Đến phòng khám] ──────────────────────────┐              │
│ (Customer)│                                              │              │
├───────────┼──────────────────────────────────────────────┼──────────────┤
│ LỄ TÂN    │  [Kiểm tra lịch đặt] ──> [Đóng tiền khám]    │              │
│ (Recep)   │          │                                   │              │
│           │          v                                   v              │
│           │  [Cấp số hàng chờ (Queue)] <─────── [Quét mã Check-in]      │
├───────────┼─────────────────────────────────────────────────────────────┤
│ BÁC SĨ    │  [Gọi số thứ tự] ──> [Khám & Kê đơn] ──> [Xác nhận kết thúc]│
│ (Vet)     │                                                             │
├───────────┼─────────────────────────────────────────────────────────────┤
│ HỆ THỐNG  │  [Tự động trừ kho thuốc] ──> [Tính toán hóa đơn viện phí]  │
│ (System)  │                                                             │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 PRD-07: Domain Knowledge - Veterinary Clinic Operations

Để đưa ra yêu cầu sản phẩm chuẩn xác, BA phải nắm rõ thuật ngữ nghiệp vụ phòng khám thú y:

### 📖 Từ Điển Thuật Ngữ Nghiệp Vụ (Domain Glossary)
* **Pet Owner (Chủ nuôi):** Khách hàng đăng ký tài khoản, chịu trách nhiệm pháp lý và tài chính cho thú cưng.
* **Pet (Thú cưng):** Đối tượng được chăm sóc, liên kết trực tiếp với 1 chủ nuôi (một chủ nuôi có thể có nhiều thú cưng).
* **Booking / Appointment (Lịch hẹn):** Đăng ký khung giờ trước khi đến phòng khám.
* **Queue Management (Quản lý hàng chờ):** Hệ thống phân phối số thứ tự thông minh. Các ca cấp cứu được ưu tiên hàng đầu, sau đó đến ca đặt lịch trước, cuối cùng là khách vãng lai (walk-in).
* **Medical Record (Bệnh án):** Nhật ký điều trị chứa cân nặng, nhiệt độ, triệu chứng, chẩn đoán của bác sĩ, hình ảnh chụp X-Quang/Siêu âm (nếu có).
* **Prescription (Đơn thuốc):** Danh sách dược phẩm được bác sĩ kê toa, tự động liên kết với kho thuốc để tính giá và trừ tồn kho.
* **Inventory (Tồn kho dược phẩm):** Quản lý thuốc theo lô, ngày hết hạn và cảnh báo khi số lượng chạm ngưỡng tối thiểu.
* **Booster Dose (Mũi tiêm nhắc lại):** Lịch hẹn tiêm vaccine định kỳ (ví dụ: vaccine dại tiêm nhắc lại hàng năm).

---

## 🎯 PRD-08: Test Case Design Techniques

### 1. Phân Vùng Tương Đương (Equivalence Partitioning)
Áp dụng cho nghiệp vụ tính toán cân nặng của thú cưng để đưa ra liều lượng thuốc tiêm:
* **Yêu cầu:** Cân nặng hợp lệ phải nằm trong khoảng từ `0.1 kg` đến `100 kg`.
* **Phân vùng:**
  - Phân vùng không hợp lệ (Dưới biên): `< 0.1` (Ví dụ: `0` hoặc `-5`) -> *Kết quả mong đợi: Hệ thống báo lỗi.*
  - Phân vùng hợp lệ: `0.1` đến `100.0` (Ví dụ: `5.5`, `50.0`) -> *Kết quả mong đợi: Lưu thành công.*
  - Phân vùng không hợp lệ (Trên biên): `> 100` (Ví dụ: `101`) -> *Kết quả mong đợi: Hệ thống báo lỗi.*

### 2. Phân Tích Giá Trị Biên (Boundary Value Analysis)
Áp dụng cho giới hạn số lượng thú cưng tối đa được đăng ký dưới một tài khoản khách hàng (Giới hạn: 5 pets).
* **Các giá trị cần test:**
  - Biên dưới: `0` pet (Không thể đặt lịch khám) -> *Hệ thống yêu cầu thêm pet trước.*
  - Cận biên: `1` pet -> *Cho phép thêm.*
  - Biên trên: `4` pets, `5` pets -> *Cho phép thêm.*
  - Vượt biên: `6` pets -> *Hệ thống chặn và báo: "Bạn đã đạt số lượng thú cưng tối đa".*

### 3. Bảng Quyết Định (Decision Table)
Áp dụng cho biểu phí dịch vụ khám bệnh tùy theo loại Pet và ngày khám (Ngày thường vs Ngày lễ):

| Điều kiện | Quy tắc 1 | Quy tắc 2 | Quy tắc 3 | Quy tắc 4 |
| :--- | :---: | :---: | :---: | :---: |
| **Loại thú cưng** | Chó / Mèo | Chó / Mèo | Thú lạ (Chim/Bò sát) | Thú lạ (Chim/Bò sát) |
| **Ngày khám** | Ngày thường | Ngày lễ | Ngày thường | Ngày lễ |
| **Phí khám áp dụng** | **150,000đ** | **200,000đ** | **250,000đ** | **350,000đ** |

### 4. Biểu Đồ Chuyển Đổi Trạng Thái (State Transition)
Áp dụng cho vòng đời của một Lịch hẹn khám bệnh (**Booking State**):
* Trạng thái hợp lệ: `Pending` -> `Confirmed` -> `CheckedIn` -> `InProgress` -> `Completed`.
* Trạng thái hủy: Từ `Pending` hoặc `Confirmed` có thể chuyển sang `Cancelled`.
* **Quy tắc chặn:** Không cho phép chuyển đổi từ `Completed` sang `Cancelled`.

---

## 🎯 PRD-09: Bug Reporting & Defect Management

Một bug report tốt giúp lập trình viên hiểu ngay vấn đề và tái dựng được lỗi mà không cần hỏi lại.

### 📋 Mẫu Bug Report Chuẩn (Bug Template)
* **Bug ID:** `BUG-PB09-003`
* **Tiêu đề:** Nút "Đặt lịch" bị đơ (không phản hồi) khi click liên tục nhiều lần trên Mobile Safari.
* **Mức độ nghiêm trọng (Severity):** 🔴 Major (Tính năng chính bị gián đoạn nhưng có giải pháp thay thế).
* **Độ ưu tiên xử lý (Priority):** High (Cần fix trong Sprint hiện tại).
* **Môi trường:** Staging - iOS 17.2 - Safari Mobile.
* **Các bước tái hiện (Steps to Reproduce):**
  1. Đăng nhập tài khoản khách hàng bằng iPhone.
  2. Chọn pet "Mun", chọn Bác sĩ Hùng, chọn ngày 15/12/2026.
  3. Nhấn nhanh liên tiếp 3 lần vào nút "Xác nhận đặt lịch".
* **Kết quả thực tế (Actual Result):** Hệ thống gửi đi 3 API request trùng lặp, tạo ra 3 lịch hẹn giống hệt nhau trong database và đơ giao diện.
* **Kết quả mong đợi (Expected Result):** Khi nhấn "Xác nhận đặt lịch" lần đầu, nút bấm phải chuyển sang trạng thái disabled và hiển thị icon loading (xoay tròn) cho đến khi API trả về kết quả thành công, tránh trùng lặp request.
* **Ảnh chụp màn hình / Log đính kèm:** (Đính kèm file ảnh màn hình lỗi hoặc API payload trùng).

---

## 🎯 PRD-10: Backlog Refinement & Prioritization

### 1. Phân Tích Độ Ưu Tiên MoSCoW
* **M (Must Have):** Khách đặt lịch, Bác sĩ khám & kê đơn, Lễ tân check-in, Tính tiền xuất hóa đơn.
* **S (Should Have):** Quản lý hồ sơ chi tiết thú cưng, Cập nhật thông tin thuốc tồn kho tự động.
* **C (Could Have):** Chatbot AI hỗ trợ giải đáp nhanh, Gửi tin nhắn SMS nhắc lịch.
* **W (Won't Have):** Đa ngôn ngữ (chỉ hỗ trợ Tiếng Việt ở bản đầu tiên), Thanh toán ví điện tử Momo/VNPAY (ở MVP chỉ nhận tiền mặt/chuyển khoản ngân hàng đối chiếu thủ công).

### 2. Mô Hình Định Lượng RICE Scoring
Tính độ ưu tiên dựa trên công thức: $RICE = \frac{Reach \times Impact \times Confidence}{Effort}$
* **Reach:** Số lượng người dùng/giao dịch bị tác động trong 1 tháng.
* **Impact:** Mức độ ảnh hưởng (3: Rất lớn, 2: Lớn, 1: Vừa, 0.5: Thấp).
* **Confidence:** Độ tự tin của PO về các ước lượng (100%: Rất tự tin, 80%: Khá tự tin, 50%: Mơ hồ).
* **Effort:** Số giờ/ngày công phát triển (Man-days).

---

## 🎯 PRD-11: Sprint Planning & Sizing

* **Story Points (Điểm phức tạp):** Sử dụng dãy Fibonacci cải tiến (`1, 2, 3, 5, 8, 13, 21`) để đánh giá độ phức tạp thay vì tính theo số giờ làm việc. Việc sizing này được thực hiện thông qua trò chơi Planning Poker giữa Dev và QA.
* **Sprint Velocity (Vận tốc Sprint):** PO theo dõi tổng số Story Points mà đội ngũ hoàn thành qua từng Sprint để tính toán năng lực thực tế, từ đó cam kết số lượng công việc hợp lý trong các Sprint tiếp theo.
