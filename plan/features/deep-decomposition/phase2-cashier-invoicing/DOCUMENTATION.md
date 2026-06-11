# 📖 Operator & Developer Documentation - Cashier & Invoicing

Tài liệu hướng dẫn vận hành (dành cho Thu ngân) và tài liệu tích hợp/debug kỹ thuật (dành cho Lập trình viên).

---

## 1. Hướng dẫn Vận hành dành cho Thu ngân (Operator Guide)

### Hướng dẫn Cấu hình in Hóa đơn nhiệt K80 trên Google Chrome / Microsoft Edge
Để hóa đơn nhiệt được in ra đẹp mắt, vừa vặn trên khổ giấy K80 (80mm) và không có tiêu đề/chân trang web mặc định, thu ngân cần thiết lập máy in theo các bước sau trong lần đầu vận hành:

1. Khi Popup in hóa đơn xuất hiện, chọn máy in hóa đơn nhiệt của bạn (ví dụ: *Xprinter XP-Q200, Canon...*).
2. Tại phần cấu hình in của trình duyệt (Print Settings):
   - **Khổ giấy (Paper size):** Chọn `Roll Paper 80x297mm` hoặc `80mm x Receipt`.
   - **Lề (Margins):** Chọn `None` (Không lề) hoặc `Minimum`.
   - **Tỷ lệ (Scale):** Chọn `100%` hoặc `Fit to page width`.
   - **Tùy chọn khác (Options):** Bỏ tích ô *Headers and footers* (Tiêu đề và chân trang) và tích chọn *Background graphics* (Đồ họa nền) để biên lai sạch sẽ và chuyên nghiệp.
3. Bấm **Print (In)** để hoàn tất. Trình duyệt sẽ tự động nhớ các tùy chọn này cho lần in tiếp theo.

---

## 2. Hướng dẫn Kỹ thuật dành cho Developer (Developer Guide)

### Cấu hình Tích hợp API VietQR (Appsettings.json)
Để cấu hình thông tin ngân hàng thụ hưởng của phòng khám, cập nhật các tham số sau trong tệp cấu hình `appsettings.json` của Backend:

```json
{
  "VietQrSettings": {
    "BankBin": "970422", 
    "AccountNumber": "190288889999",
    "AccountName": "PHONG KHAM MYPETCLINIC",
    "ApiUrl": "https://api.vietqr.io/v2/generate"
  }
}
```

### Các lệnh cURL Kiểm thử API Thủ công (API Debugging)

#### 1. Gọi Lập Hóa đơn mới từ Lịch hẹn
```bash
curl -X POST "https://localhost:5001/api/receptionist/invoices" \
     -H "Authorization: Bearer <JWT_TOKEN>" \
     -H "Content-Type: application/json" \
     -d "{\"appointmentId\": \"d290f1ee-6c54-4b01-90e6-d701748f0851\"}"
```

#### 2. Lấy mã QR Code VietQR động
```bash
curl -X GET "https://localhost:5001/api/receptionist/invoices/e883e54b-d72b-42fa-97ab-713217b1897d/qr" \
     -H "Authorization: Bearer <JWT_TOKEN>"
```

#### 3. Xác nhận Thanh toán hóa đơn (Chuyển khoản)
```bash
curl -X PUT "https://localhost:5001/api/receptionist/invoices/e883e54b-d72b-42fa-97ab-713217b1897d/pay" \
     -H "Authorization: Bearer <JWT_TOKEN>" \
     -H "Content-Type: application/json" \
     -d "{\"paymentMethod\": \"BankTransfer\"}"
```

---

## 3. Khắc phục Sự cố Thường gặp (Troubleshooting)

### Sự cố 1: Sai lệch/Lệch số tiền do làm tròn dấu phẩy động (Floating point error)
- **Triệu chứng:** Tổng tiền hiển thị trên hóa đơn bị lẻ thập phân hoặc chênh lệch vài trăm đồng so với tổng tiền thực tế.
- **Nguyên nhân:** Do sử dụng kiểu dữ liệu `double` hoặc `float` trong C# và Javascript để tính tiền y khoa dẫn đến sai số nhị phân.
- **Giải pháp khắc phục:**
  - Luôn sử dụng kiểu dữ liệu **`decimal`** trong C# (Database dùng kiểu `NUMERIC(18,2)`) để thực hiện các phép nhân chia tính tiền.
  - Phía Frontend Vue 3, sử dụng thư viện làm tròn chuẩn tiền tệ trước khi hiển thị:
    ```typescript
    export function formatVnd(amount: number): string {
      const rounded = Math.round(amount);
      return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(rounded);
    }
    ```

### Sự cố 2: Lỗi 403 Forbidden khi Thu ngân truy cập
- **Nguyên nhân:** Token JWT của thu ngân bị thiếu Claim vai trò `role` hoặc vai trò trong DB không khớp với chuỗi `"receptionist"` hoặc `"cashier"`.
- **Giải pháp khắc phục:**
  - Giải mã token JWT tại trang [jwt.io](https://jwt.io) để kiểm tra claim `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`.
  - Đảm bảo cơ sở dữ liệu đã gán đúng vai trò cho tài khoản nhân viên.
