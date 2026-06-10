# 📄 User & Dev Documentation - Cashier & Invoicing

## 1. DB Schema cho Hóa đơn & Chi tiết Hóa đơn

Lập trình viên backend cần chạy Migration khởi tạo cấu trúc các bảng dữ liệu sau:

```sql
-- Bảng Invoices
CREATE TABLE "Invoices" (
    "Id" UUID PRIMARY KEY,
    "InvoiceNumber" VARCHAR(50) NOT NULL UNIQUE,
    "AppointmentId" UUID NOT NULL FOREIGN KEY REFERENCES "Appointments"("Id"),
    "ServiceAmount" DECIMAL(18,2) NOT NULL,
    "MedicineAmount" DECIMAL(18,2) NOT NULL,
    "TotalAmount" DECIMAL(18,2) NOT NULL,
    "Status" VARCHAR(20) NOT NULL, -- Unpaid, Paid, Cancelled
    "PaymentMethod" VARCHAR(20) NULL, -- Cash, BankTransfer
    "CreatedAt" TIMESTAMP NOT NULL
);

-- Bảng InvoiceItems
CREATE TABLE "InvoiceItems" (
    "Id" UUID PRIMARY KEY,
    "InvoiceId" UUID NOT NULL FOREIGN KEY REFERENCES "Invoices"("Id"),
    "ItemName" VARCHAR(255) NOT NULL,
    "Quantity" INT NOT NULL,
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "TotalPrice" DECIMAL(18,2) NOT NULL
);
```

---

## 2. Hướng dẫn Tích hợp VietQR (Frontend & Backend)
- **Tạo Link VietQR nhanh:**
  Tận dụng dịch vụ API miễn phí của VietQR để hiển thị ảnh QR chuyển khoản nhanh trên màn hình thanh toán cho lễ tân đưa khách hàng quét:
  `https://img.vietqr.io/image/<BANK_ID>-<ACCOUNT_NO>-qr_only.png?amount=<AMOUNT>&addInfo=<MEMO>&accountName=<ACCOUNT_NAME>`
- **Các tham số truyền động:**
  - `<BANK_ID>`: Mã ngân hàng (ví dụ: `vcb`, `tcb`).
  - `<ACCOUNT_NO>`: Số tài khoản của phòng khám thú y.
  - `<AMOUNT>`: Tổng số tiền hóa đơn (`invoice.totalAmount`).
  - `<MEMO>`: Mã nội dung thanh toán (ví dụ: `THANH TOAN HOA DON HD-20260610-001`).
