# 🚀 Product Requirements Document (PRD) - Cashier & Invoicing

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Phân hệ **Thu ngân & Lập hóa đơn (Cashier & Invoicing)** là điểm chạm cuối cùng trong luồng vận hành khám chữa bệnh tại phòng khám thú y MyPetClinic. Sau khi bác sĩ hoàn thành ca chẩn đoán lâm sàng, kê đơn thuốc hoặc chỉ định tiêm phòng vắc-xin, hệ thống cần tự động tính toán tổng số tiền dựa trên dữ liệu bệnh án hiện tại để xuất hóa đơn nháp ngay lập tức.

Mục tiêu chính là tối ưu hóa tốc độ thanh toán, loại bỏ rủi ro sai lệch dữ liệu thủ công, cung cấp phương thức thanh toán linh hoạt (tiền mặt hoặc chuyển khoản QR Code động) và in biên lai nhiệt chuẩn hóa, mang lại trải nghiệm chuyên nghiệp cho cả nhân viên và khách hàng.

---

## 2. Đối tượng sử dụng (Target Personas)

### 🧑‍💼 Lễ tân kiêm Thu ngân - Chị Hoa (32 tuổi)
- **Mô tả:** Hoa làm việc tại bàn lễ tân phòng khám. Vào giờ cao điểm (17h - 20h), cô phải vừa đón khách mới, vừa làm thủ tục thanh toán cho 5-7 khách hàng đang chờ ra về cùng lúc. Thú cưng thường hiếu động hoặc sủa lớn làm cô mất tập trung.
- **Nỗi đau (Pain points):**
  - Việc cộng tay đơn thuốc và phí khám dễ nhầm lẫn, mất thời gian dò sổ.
  - Khách muốn chuyển khoản nhưng cô phải đọc số tài khoản và số tiền rồi bảo khách nhập tay, dẫn đến khách nhập sai số tiền hoặc sai cú pháp chuyển khoản, khiến việc đối soát ngân hàng cuối ngày cực kỳ mệt mỏi.
  - Muốn in hóa đơn nhanh nhưng bố cục trang in bị lệch, mất thông tin thuốc khiến khách phàn nàn.
- **Mong muốn:** Hệ thống tự động tạo hóa đơn nháp, quét mã QR thanh toán nhanh và in ấn biên lai tự động trong 1 click.

### 🧑‍🌾 Khách hàng nuôi thú cưng - Anh Nam (28 tuổi)
- **Mô tả:** Nam đem chú mèo Anh lông ngắn đi khám tiêu hóa. Sau khi khám xong, anh muốn thanh toán chuyển khoản thật nhanh để đưa mèo về vì nó đang stress và kêu liên tục.
- **Nỗi đau (Pain points):**
  - Chờ đợi thu ngân tính tiền quá lâu.
  - Phải nhập tay số tài khoản ngân hàng và nội dung chuyển khoản rườm rà.
  - Không có biên lai chi tiết giá tiền từng loại thuốc để về theo dõi chi phí.
- **Mong muốn:** Thanh toán chuyển khoản không chạm bằng QR Code động hiển thị ngay trên máy tính thu ngân, nhận hóa đơn in rõ ràng.

---

## 3. User Stories & Tiêu chí Nghiệm thu (Acceptance Criteria)

### Story 1: Tự động tổng hợp dữ liệu ca khám thành hóa đơn nháp (Draft Invoice Auto-Generation)
> **Là một** Thu ngân phòng khám,  
> **Tôi muốn** hệ thống tự động tổng hợp chi phí ngay khi bác sĩ hoàn thành ca khám của thú cưng,  
> **Để tôi** không cần phải nhập thủ công hay tính toán lại đơn giá thuốc và phí dịch vụ.

#### Tiêu chí Nghiệm thu (AC):
- **AC 1.1:** Khi một Lịch hẹn khám (`Appointment`) chuyển sang trạng thái `Completed` bởi bác sĩ, hệ thống tự động tạo một thực thể Hóa đơn (`Invoice`) ở trạng thái `Pending` (Chờ thanh toán).
- **AC 1.2:** Hóa đơn phải tự động gom tất cả các dòng chi phí bao gồm:
  - Phí dịch vụ khám gốc của lịch hẹn.
  - Các loại thuốc được kê trong bệnh án (Số lượng $\times$ Đơn giá bán lẻ tại thời điểm xuất hóa đơn).
  - Vắc-xin được sử dụng trong ca khám (nếu có).
- **AC 1.3:** Số tiền tổng cộng (`TotalAmount`) phải bằng tổng tiền dịch vụ cộng tiền thuốc/vắc-xin đã cộng thuế VAT mặc định (10%). Công thức:
  $$\text{TotalAmount} = \sum (\text{ServiceFee}) + \sum (\text{MedicinePrice} \times \text{Quantity}) \times 1.1$$
- **AC 1.4:** Hệ thống không cho phép sửa đổi thủ công đơn giá thuốc hoặc số lượng thuốc đã được bác sĩ kê trên hóa đơn nhằm bảo toàn tính toàn vẹn y khoa.

### Story 2: Thanh toán linh hoạt bằng QR động hoặc Tiền mặt
> **Là một** Thu ngân phòng khám,  
> **Tôi muốn** lựa chọn phương thức thanh toán và hiển thị QR Code động của hóa đơn để khách hàng thanh toán chuyển khoản nhanh,  
> **Để** giảm thiểu thao tác thủ công và lỗi đối soát tài chính.

#### Tiêu chí Nghiệm thu (AC):
- **AC 2.1:** Khi thu ngân chọn phương thức thanh toán là `Bank Transfer` (Chuyển khoản), hệ thống lập tức gọi API để sinh mã VietQR động chứa:
  - Số tài khoản ngân hàng thụ hưởng cấu hình của phòng khám.
  - Số tiền chính xác cần thanh toán (`TotalAmount`).
  - Nội dung chuyển khoản duy nhất định dạng: `MYPETCLINIC INVOICE <InvoiceNumber>`.
- **AC 2.2:** Khi thu ngân bấm xác nhận "Thành công" (`Confirm Paid`), hệ thống phải cập nhật trạng thái hóa đơn thành `Paid`, phương thức thanh toán (`PaymentMethod`), thời gian thanh toán thực tế (`PaidAt` theo UTC).
- **AC 2.3:** Hệ thống tự động chuyển trạng thái thanh toán của Lịch hẹn tương ứng thành `Paid`. Hành động này phải được thực thi trong một Database Transaction an toàn.

### Story 3: In biên lai nhiệt K80 tối giản
> **Là một** Thu ngân phòng khám,  
> **Tôi muốn** in hóa đơn nhiệt khổ K80 trực tiếp từ trình duyệt,  
> **Để** đưa cho khách hàng kiểm tra chi tiết đơn thuốc và chi phí dịch vụ.

#### Tiêu chí Nghiệm thu (AC):
- **AC 3.1:** Trang in hóa đơn phải sử dụng CSS `@media print` ẩn đi toàn bộ thanh điều hướng, sidebar, các nút hành động, và chỉ hiển thị phần biên lai hóa đơn nhiệt.
- **AC 3.2:** Bố cục biên lai phải vừa vặn khổ giấy nhiệt 80mm (K80) không bị tràn viền, font chữ to rõ ràng, có đầy đủ tên phòng khám, địa chỉ, số điện thoại, tên thú cưng, tên chủ nuôi, chi tiết từng dịch vụ/thuốc và chữ ký thu ngân.

---

## 4. Phạm vi dự án (In-Scope & Out-of-Scope)

### ✅ In-Scope (Phase 2 MVP)
- Tạo hóa đơn tự động từ các ca khám đã hoàn thành.
- Quản lý danh sách hóa đơn của thu ngân (Bộ lọc: Chờ thanh toán, Đã thanh toán, Đã hủy).
- Tích hợp sinh mã VietQR động trực quan trên giao diện popup.
- In hóa đơn K80 trực tiếp qua CSS Print.
- Cập nhật đồng bộ trạng thái Lịch hẹn và Hóa đơn sang `Paid` thông qua Database Transaction.

### ❌ Out-of-Scope (Bàn giao Phase sau)
- Tự động quét biến động số dư ngân hàng (Webhook VietQR/Lưu lượng tài khoản Bank API) để tự động cập nhật hóa đơn thành `Paid` (Sẽ làm ở Phase 3).
- Áp dụng mã giảm giá (Voucher/Coupon) hoặc chương trình điểm thưởng khách hàng thân thiết.
- Hỗ trợ thanh toán một phần (Trả trước / Trả sau nhiều đợt).

---

## 5. Yêu cầu phi chức năng (Non-Functional Requirements - NFRs)
- **Hiệu năng:** Thời gian tính toán và tạo hóa đơn nháp từ ca khám dưới 150ms.
- **Độ tin cậy:** Giao dịch thanh toán phải bảo đảm tính toàn vẹn dữ liệu (không thể có tình trạng hóa đơn báo `Paid` nhưng lịch hẹn vẫn báo chưa thanh toán).
- **Trải nghiệm in ấn:** Tỷ lệ in lỗi (lệch dòng, tràn trang K80) phải bằng 0% trên các trình duyệt hiện đại (Chrome, Edge, Safari).
- **Bảo mật:** Chặn đứng IDOR, khách hàng chỉ được xem hóa đơn do chính mình sở hữu thông qua JWT Token.
