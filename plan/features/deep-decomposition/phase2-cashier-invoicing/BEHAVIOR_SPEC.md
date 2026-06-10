# 🎭 Behavioral Specification - Cashier & Invoicing

## 1. Biểu đồ Trạng thái Xử lý Hóa đơn của Thu ngân (Billing State Machine)

```mermaid
stateDiagram-v2
    [*] --> UnpaidList : Thu ngân mở trang Hóa đơn
    UnpaidList --> LoadingDetails : Click chọn xem chi tiết hóa đơn
    LoadingDetails --> ShowingReceipt : Hiển thị chi tiết (Dịch vụ + Thuốc)
    ShowingReceipt --> PaymentProcessing : Click "Xác nhận Thanh toán"
    
    state PaymentProcessing {
        [*] --> SelectingMethod
        SelectingMethod --> PayingCash : Chọn Tiền mặt
        SelectingMethod --> GeneratingVietQR : Chọn Chuyển khoản
        GeneratingVietQR --> WaitingTransfer
        PayingCash --> ConfirmSuccess
        WaitingTransfer --> ConfirmSuccess : Nhận callback/Xác thực thủ công
    }
    
    ConfirmSuccess --> PaidReceipt : Cập nhật UI thành Đã thanh toán
    PaidReceipt --> PrintingTicket : Click "In hóa đơn" (Mở Print Dialog)
    PrintingTicket --> UnpaidList : Trở về danh sách hóa đơn chờ khám
```

---

## 2. Ràng buộc Hành vi & Giao diện (Behavior Constraints)
- **VietQR Generation Dynamic:** Nếu chọn phương thức `Chuyển khoản (BankTransfer)`, hệ thống tự động sinh mã QR động theo định dạng tiêu chuẩn Napas 247, mã hóa đầy đủ số tiền thanh toán thực tế của hóa đơn để tránh khách hàng chuyển khoản sai số tiền.
- **Double Click Prevention:** Vô hiệu hóa nút "Xác nhận thanh toán" ngay khi vừa nhấn để tránh gửi trùng lặp request thanh toán lên server.
