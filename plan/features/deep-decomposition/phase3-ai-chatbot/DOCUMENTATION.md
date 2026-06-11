# 📖 Operator & Developer Documentation - Gemini AI Chatbot

Tài liệu hướng dẫn vận hành (dành cho Quản trị viên) và tài liệu tích hợp/debug kỹ thuật (dành cho Lập trình viên) cho phân hệ Trợ lý ảo AI.

---

## 1. Hướng dẫn Vận hành dành cho Quản trị viên (Operator Guide)

### Hướng dẫn Đăng ký và Cấu hình Google Gemini API Key
Để kích hoạt trí tuệ nhân tạo cho phòng khám MyPetClinic, Quản trị viên cần thực hiện lấy API Key miễn phí hoặc có phí từ Google AI Studio theo các bước sau:

1. Truy cập vào trang quản trị Google AI Studio: [https://aistudio.google.com/](https://aistudio.google.com/)
2. Đăng nhập bằng tài khoản Google của phòng khám.
3. Click chọn nút **Get API Key** ở góc trái.
4. Click **Create API Key** -> Chọn dự án Google Cloud tương ứng -> Tạo khóa.
5. Sao chép chuỗi mã khóa an toàn (dạng `AIzaSyD...`).
6. Truy cập vào thư mục mã nguồn Backend Web API của phòng khám. Mở file cấu hình mật `appsettings.json` (hoặc file cấu hình môi trường sản xuất) và điền khóa vào mục:
   ```json
   {
     "GeminiSettings": {
       "ApiKey": "PASTE_YOUR_COPIED_KEY_HERE"
     }
   }
   ```
7. Khởi động lại ứng dụng Web API để cập nhật key mới.

---

## 2. Hướng dẫn Kỹ thuật dành cho Developer (Developer Guide)

### Các lệnh cURL Kiểm thử API Thủ công (API Debugging)

#### 1. Gửi tin nhắn chat thông thường
```bash
curl -X POST "https://localhost:5001/api/ai/chat" \
     -H "Authorization: Bearer <JWT_TOKEN>" \
     -H "Content-Type: application/json" \
     -d "{\"message\": \"Mèo con 2 tháng tuổi ăn gì tốt nhất?\", \"history\": []}"
```

#### 2. Gửi tin nhắn chứa triệu chứng nguy kịch (Kiểm tra xem hệ thống có trả về cờ requiresAppointment = true và Warning hay không)
```bash
curl -X POST "https://localhost:5001/api/ai/chat" \
     -H "Authorization: Bearer <JWT_TOKEN>" \
     -H "Content-Type: application/json" \
     -d "{\"message\": \"Cún của tôi bị nôn ra máu đen nằm im một chỗ, tôi lo quá.\", \"history\": []}"
```

---

## 3. Khắc phục Sự cố Thường gặp (Troubleshooting)

### Sự cố 1: Lỗi 429 Too Many Requests hoặc Vượt hạn mức sử dụng (Quota Exceeded)
- **Triệu chứng:** Người dùng chat và hệ thống trả về thông báo lỗi đỏ: *"Trợ lý AI đang quá tải lượt truy vấn (Rate Limit)..."* hoặc trong log server ghi nhận mã lỗi API Google `RESOURCE_EXHAUSTED`.
- **Nguyên nhân:** Do sử dụng gói API Key miễn phí của Google Gemini (mức giới hạn là 15 requests / phút) và có nhiều người cùng chat cùng lúc, hoặc key đã dùng hết số tiền khuyến mãi định mức của tài khoản Google Cloud.
- **Giải pháp khắc phục:**
  1. Đăng nhập vào Google Cloud Console và nâng cấp tài khoản thanh toán (Billing Account) cho dự án AI. Chuyển sang gói trả tiền theo lưu lượng thực tế (Pay-as-you-go).
  2. Bật cơ chế Cache ngắn hạn (Memory Cache) ở Backend cho cùng một câu hỏi phổ biến để giảm số lần gọi trực tiếp sang API Google.

### Sự cố 2: AI trả lời lan man hoặc đưa ra đơn thuốc trái phép
- **Nguyên nhân:** Do tham số `temperature` (độ sáng tạo) cấu hình quá cao (ví dụ: > 0.7) khiến mô hình tự ý suy diễn vượt qua chỉ thị bảo mật System Instruction.
- **Giải pháp khắc phục:**
  - Truy cập mã nguồn Backend `GeminiChatService.cs`. Đảm bảo thuộc tính `temperature` được set bằng **`0.2`** hoặc thấp hơn để ép mô hình tuân thủ tuyệt đối chỉ thị bảo mật.
  - Cập nhật thêm các từ khóa thuốc cấm vào mảng kiểm duyệt `ForbiddenKeywords` ở Backend để lọc cứng kết quả trước khi trả về cho khách hàng.
