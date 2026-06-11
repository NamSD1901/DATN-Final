# 🎨 UI/UX Design Specification - Cashier & Invoicing

Tài liệu thiết kế giao diện người dùng, sơ đồ bố cục ASCII Mockup, đặc tả biến màu CSS HSL, CSS in nhiệt K80, và logic validation ở phía Client.

---

## 1. Bản vẽ Bố cục giao diện (ASCII Art Mockups)

### Màn hình Quản lý Hóa đơn của Thu ngân (Invoices Workspace)
Màn hình thiết kế theo phong cách Glassmorphism (mờ kính), chia làm Grid hai cột linh hoạt: Danh sách hóa đơn chờ bên trái và chi tiết hóa đơn được chọn bên phải.

```text
+-------------------------------------------------------------------------------------------------------------------+
|  [Logo] MYPETCLINIC - CỔNG THU NGÂN & THANH TOÁN (CASHIER DASHBOARD)                      [NV: Hoa] [Đăng xuất]    |
+-------------------------------------------------------------------------------------------------------------------+
|  [Tìm kiếm số HD / SĐT...]   ( Lọc: [Tất cả]  [(*) Chờ thanh toán]  [Đã thanh toán]  [Đã hủy] )                   |
+-------------------------------------------------------------------------------------------------------------------+
|  DANH SÁCH HÓA ĐƠN CHỜ THANH TOÁN (12)      |  CHI TIẾT HÓA ĐƠN: INV-20260611-0043                                |
|  +---------------------------------------+  |  +---------------------------------------------------------------+  |
|  | #INV-0043 - Mèo LuLu - Nam            |  |  | Chủ nuôi: Nguyễn Văn Nam          Thú cưng: Mèo LuLu          |  |
|  | Số tiền: 420.000đ  [Chờ]  10 phút trước|  |  | Ngày lập: 11/06/2026 09:30 UTC     Trạng thái: CHỜ THANH TOÁN  |  |
|  +---------------------------------------+  |  +---------------------------------------------------------------+  |
|  | #INV-0042 - Cún Kiki - Vy             |  |  | CHI TIẾT CÁC KHOẢN PHÍ:                                       |  |
|  | Số tiền: 1.250.000đ [Chờ] 15 phút trước|  |  | 1. Phí khám bệnh lâm sàng........................... 100.000 đ |  |
|  +---------------------------------------+  |  | 2. Thuốc: Amoxicillin 500mg x 10 viên............... 150.000 đ |  |
|  | #INV-0041 - Thỏ Bông - Trang          |  |  | 3. Thuốc: Dexafort kháng viêm x 1 lọ................ 150.000 đ |  |
|  | Số tiền: 280.000đ  [Chờ]  22 phút trước|  |  | 4. Thuế VAT 10% (Chỉ áp dụng cho thuốc)..............  30.000 đ |  |
|  +---------------------------------------+  |  |---------------------------------------------------------------|  |
|  | #INV-0040 - Mèo Miu - Tuấn            |  |  | TỔNG TIỀN THANH TOÁN:                              430.000 đ  |  |
|  | Số tiền: 550.000đ  [Đã thanh toán]    |  |  +---------------------------------------------------------------+  |
|  +---------------------------------------+  |  | Chọn phương thức thanh toán:                                  |  |
|                                             |  | (o) Tiền mặt (Cash)      ( ) Chuyển khoản VietQR              |  |
|                                             |  +---------------------------------------------------------------+  |
|                                             |  |    [ HỦY HÓA ĐƠN ]       [ IN HÓA ĐƠN NHÁP ]      [ XÁC NHẬN PAY ]  |  |
|                                             |  +---------------------------------------------------------------+  |
+-------------------------------------------------------------------------------------------------------------------+
```

### Popup Quét QR Code động thanh toán chuyển khoản (VietQR Dynamically Generated Modal)
```text
+---------------------------------------------------------+
|                  XÁC NHẬN THANH TOÁN QR                 |
+---------------------------------------------------------+
|  Vui lòng hướng dẫn khách hàng quét mã QR Code dưới đây |
|  bằng ứng dụng ngân hàng di động (SmartBanking).        |
|                                                         |
|                     +---------------+                   |
|                     |  ###########  |                   |
|                     |  ## QR CODE ##  |                   |
|                     |  ##  DYNAMIC ## |                   |
|                     |  ###########  |                   |
|                     +---------------+                   |
|                                                         |
|  Ngân hàng: MB Bank (Ngân hàng Quân Đội)                |
|  Số tài khoản: 1902 8888 9999                           |
|  Tên thụ hưởng: PHONG KHAM MYPETCLINIC                  |
|  Số tiền: 430.000 đ                                     |
|  Nội dung: MYPETCLINIC INVOICE INV-20260611-0043         |
|                                                         |
|  [x] Đang chờ hệ thống ghi nhận giao dịch chuyển khoản... |
|  +---------------------------------------------------+  |
|  |       [ QUAY LẠI ]        [ XÁC NHẬN ĐÃ THU TIỀN ]        |
|  +---------------------------------------------------+  |
+---------------------------------------------------------+
```

---

## 2. Hệ thống CSS Design Tokens (HSL Color Theme)

Bảng màu mờ kính sang trọng tối ưu cho giao diện cổng Lễ tân/Thu ngân:

```css
:root {
  /* Bảng màu Glassmorphism HSL */
  --bg-workspace: hsl(220, 20%, 10%);       /* Nền tối sâu */
  --glass-bg: hsla(220, 20%, 18%, 0.45);    /* Lớp mờ kính nền */
  --glass-border: hsla(220, 10%, 100%, 0.08);/* Viền kính mỏng */
  --text-primary: hsl(210, 20%, 98%);       /* Chữ trắng sáng */
  --text-secondary: hsl(215, 15%, 75%);     /* Chữ xám phụ */

  /* Trạng thái hóa đơn */
  --status-pending: hsl(38, 95%, 55%);      /* Cam - Chờ thanh toán */
  --status-pending-bg: hsla(38, 95%, 55%, 0.15);
  
  --status-paid: hsl(145, 65%, 45%);         /* Xanh lá - Đã thanh toán */
  --status-paid-bg: hsla(145, 65%, 45%, 0.15);
  
  --status-cancelled: hsl(355, 75%, 50%);    /* Đỏ - Đã hủy */
  --status-cancelled-bg: hsla(355, 75%, 50%, 0.15);

  --accent-blue: hsl(200, 90%, 50%);         /* Xanh neon bổ trợ */
  --card-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
  --backdrop-blur: blur(12px);
}
```

---

## 3. Đặc tả Bố cục In ấn K80 (`@media print` CSS)

Sử dụng CSS sau để bảo đảm hóa đơn in nhiệt khổ 80mm không bị lỗi layout:

```css
@media print {
  /* Ẩn toàn bộ giao diện Web, Sidebar, Header, Buttons */
  body * {
    visibility: hidden;
  }
  
  /* Chỉ hiển thị phần thẻ có id="k80-print-receipt" */
  #k80-print-receipt, #k80-print-receipt * {
    visibility: visible;
  }
  
  #k80-print-receipt {
    position: absolute;
    left: 0;
    top: 0;
    width: 80mm;            /* Khổ giấy K80 */
    padding: 2mm;
    margin: 0;
    font-family: 'Courier New', Courier, monospace; /* Font in nhiệt chuẩn */
    font-size: 11px;
    color: #000;
    background: #fff;
  }

  /* Định dạng dòng kẻ gạch đứt */
  .receipt-divider {
    border-top: 1px dashed #000;
    margin: 5px 0;
  }

  .receipt-title {
    font-size: 14px;
    font-weight: bold;
    text-align: center;
    margin-bottom: 5px;
  }

  .receipt-header-info {
    text-align: center;
    font-size: 10px;
    margin-bottom: 10px;
  }

  .receipt-table {
    width: 100%;
    border-collapse: collapse;
  }

  .receipt-table th {
    border-bottom: 1px solid #000;
    text-align: left;
  }

  .text-right {
    text-align: right;
  }
  
  /* Chặn việc in background colors và gộp page */
  @page {
    size: auto;
    margin: 0mm;
  }
}
```

---

## 4. Logic Client-side Validation

Đảm bảo dữ liệu phương thức thanh toán phải hợp lệ trước khi gọi API thanh toán.

```typescript
// InvoicePaymentValidation.ts
export interface PaymentConfirmation {
  paymentMethod: 'Cash' | 'BankTransfer';
}

export function validatePayment(data: PaymentConfirmation): { isValid: boolean; error?: string } {
  if (!data.paymentMethod) {
    return {
      isValid: false,
      error: 'Vui lòng chọn phương thức thanh toán (Tiền mặt hoặc Chuyển khoản).'
    };
  }

  const validMethods = ['Cash', 'BankTransfer'];
  if (!validMethods.includes(data.paymentMethod)) {
    return {
      isValid: false,
      error: 'Phương thức thanh toán đã chọn không được hỗ trợ.'
    };
  }

  return { isValid: true };
}
```
