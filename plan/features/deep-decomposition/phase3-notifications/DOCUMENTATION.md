# 📖 Operator & Developer Documentation - Notifications

Tài liệu hướng dẫn vận hành (dành cho Lễ tân / Quản trị viên) và tài liệu tích hợp/debug kỹ thuật (dành cho Lập trình viên) cho phân hệ Thông báo & Nhắc lịch tiêm chủng tự động.

---

## 1. Hướng dẫn Vận hành dành cho Admin (Operator Guide)

### Hướng dẫn Chỉnh sửa Mẫu Email Nhắc lịch Tiêm phòng (Email Template Customization)
Để chỉnh sửa nội dung thư gửi khách hàng hoặc thay đổi chương trình khuyến mãi đi kèm mũi tiêm nhắc lại, Admin thực hiện theo các bước sau:

1. Truy cập máy chủ lưu trữ Backend của phòng khám MyPetClinic.
2. Tìm thư mục lưu trữ mẫu email tĩnh: `backend/src/MyPetClinic.Application/Templates/`.
3. Mở file `VaccineReminderTemplate.html` bằng phần mềm chỉnh sửa văn bản.
4. Có thể chỉnh sửa nội dung thẻ HTML hoặc CSS inline để thay đổi màu sắc nút đặt lịch khám, điều chỉnh font chữ phòng khám.
5. **LƯU Ý:** Không được xóa hoặc chỉnh sửa các từ khóa placeholders dạng biến thay thế tự động của hệ thống:
   - `{ownerName}`: Tên khách hàng.
   - `{petName}`: Tên bé cún/mèo.
   - `{vaccineName}`: Tên loại vắc-xin cần tái chủng.
   - `{nextDoseDate}`: Ngày tái chủng dự kiến.
   - `{bookingLink}`: Link đặt lịch tự động điền sẵn thông tin.
6. Lưu file và reload lại dịch vụ Backend để cập nhật mẫu email mới.

---

## 2. Hướng dẫn Kỹ thuật dành cho Developer (Developer Guide)

### Cấu hình Cron Trigger cho Quartz.NET Job (Appsettings.json)
Để tùy chỉnh thời gian quét lịch tiêm chủng hàng ngày (ví dụ: quét lúc 08:00 sáng hàng ngày hoặc chạy thử nghiệm mỗi 5 phút trong môi trường test), chỉnh sửa cấu hình Cron Expression:

```json
{
  "Quartz": {
    "VaccinationReminderJob": {
      "CronExpression": "0 0 8 * * ?" // Chạy lúc 08:00:00 sáng mỗi ngày
      // Cấu hình test: "0 */5 * * * ?" (Chạy 5 phút một lần)
    }
  }
}
```

### Các lệnh cURL Kiểm thử API Thủ công (API Debugging)

#### 1. Lấy danh sách thông báo của khách hàng đang đăng nhập
```bash
curl -X GET "https://localhost:5001/api/customer/notifications" \
     -H "Authorization: Bearer <CUSTOMER_JWT_TOKEN>"
```

#### 2. Đánh dấu đã đọc một tin thông báo
```bash
curl -X PUT "https://localhost:5001/api/customer/notifications/e883e54b-d72b-42fa-97ab-713217b1897d/read" \
     -H "Authorization: Bearer <CUSTOMER_JWT_TOKEN>"
```

---

## 3. Khắc phục Sự cố Thường gặp (Troubleshooting)

### Sự cố 1: Lỗi SignalR không thể kết nối WebSockets hoặc báo lỗi CORS
- **Triệu chứng:** Client console hiển thị lỗi đỏ liên tục: *"Error: Failed to start the connection: Error: WebSockets failed to connect..."* hoặc *"Access to XMLHttpRequest at... from origin... has been blocked by CORS policy..."*.
- **Nguyên nhân:** Do chưa cho phép gửi thông tin xác thực (`AllowCredentials`) tại cấu hình CORS ở file `Program.cs` Backend, hoặc phía Client chưa cấu hình SignalR Client bỏ qua đàm phán phương thức và ép kiểu WebSockets.
- **Giải pháp khắc phục:**
  1. Kiểm tra lại cấu hình CORS ở Backend (đã mô tả chi tiết tại file `04-infrastructure.md`). Đảm bảo có dòng `.AllowCredentials()` và không dùng dấu sao `*` cho `WithOrigins`.
  2. Phía Client Vue 3, cấu hình khởi tạo kết nối SignalR chi tiết như sau:
     ```typescript
     const connection = new signalR.HubConnectionBuilder()
       .withUrl('/hubs/notifications', {
         skipNegotiation: true,
         transport: signalR.HttpTransportType.WebSockets // Ép sử dụng kết nối WebSockets trực tiếp
       })
       .build();
     ```

### Sự cố 2: Quartz.NET Job bị bỏ lỡ lịch quét (Misfire Instruction)
- **Nguyên nhân:** Do server tắt nguồn hoặc khởi động lại lúc 08:00 sáng, Quartz.NET không thể chạy Job đúng giờ cấu hình.
- **Giải pháp khắc phục:**
  - Cấu hình chính sách xử lý Job bị lỡ lịch (Misfire Instruction) thành **`WithMisfireInstructionFireNow`** để Quartz tự động chạy bù ngay khi máy chủ hoạt động trở lại:
    ```csharp
    // Trong Program.cs lúc đăng ký Quartz
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("VaccinationReminderJob-Trigger")
        .WithCronSchedule("0 0 8 * * ?", s => s.WithMisfireInstructionFireNow())
    );
    ```
