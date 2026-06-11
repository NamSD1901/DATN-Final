# 📄 API Reference - Gemini AI Chatbot

Tài liệu đặc tả chi tiết cổng kết nối (RESTful API Contract) dành cho phân hệ Trợ lý ảo Tư vấn Sức khỏe AI.

---

## 1. Endpoint Tương tác Trò chuyện AI

- **Đường dẫn API:** `/api/ai/chat`
- **Phương thức:** `POST`
- **Phân quyền:** `customer`, `doctor`, `receptionist`, `cashier`, `admin` (Yêu cầu đăng nhập JWT)
- **Headers:** 
  - `Authorization: Bearer <JWT_TOKEN>`
  - `Content-Type: application/json`

---

## 2. Đặc tả Cấu trúc Payload gửi nhận

### Request Body Schema (Application/JSON)
```json
{
  "message": "Chú chó của tôi bỗng nhiên nôn ra máu đen, tôi phải làm sao?",
  "history": [
    {
      "role": "user",
      "text": "Chào trợ lý, tôi mới nuôi một chú chó Poodle nặng 5kg."
    },
    {
      "role": "model",
      "text": "Chào bạn! Tôi có thể giúp gì cho chú chó Poodle 5kg của bạn?"
    }
  ]
}
```

### Response (200 OK) - Trường hợp phát hiện triệu chứng nguy hiểm cần đặt lịch khám
```json
{
  "textResponse": "Nôn ra máu đen là dấu hiệu xuất huyết tiêu hóa cực kỳ nguy hiểm ở chó. Bạn hãy thực hiện các bước sơ cứu sau:\n1. Ngừng cho chó ăn uống ngay lập tức để tránh kích ứng dạ dày thêm.\n2. Giữ ấm cho chó và đặt nằm nghiêng ở nơi yên tĩnh.\n\nLƯU Ý: Đây là tình trạng khẩn cấp đe dọa tính mạng. Vui lòng đưa ngay chú chó của bạn đến phòng khám thú y gần nhất hoặc đặt lịch khám trực tiếp với bác sĩ chuyên khoa của chúng tôi bằng nút bên dưới.",
  "requiresAppointment": true,
  "systemWarning": "CẢNH BÁO Y KHOA: Vui lòng đưa thú cưng tới phòng khám hoặc liên hệ Bác sĩ ngay nếu có dấu hiệu nặng."
}
```

### Response (200 OK) - Câu hỏi chăm sóc thông thường
```json
{
  "textResponse": "Mèo con 2 tháng tuổi nên được cho ăn hạt ngâm nước ấm cho mềm hoặc pate chuyên dụng cho mèo con. Chia nhỏ bữa ăn thành 4-5 cữ/ngày để hệ tiêu hóa dễ hấp thu bạn nhé.",
  "requiresAppointment": false,
  "systemWarning": null
}
```

---

## 3. Đặc tả Mã lỗi Phản hồi (Error Codes)

### Response (400 Bad Request) - Câu hỏi quá dài
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": {
    "Message": ["Câu hỏi không được vượt quá 2000 ký tự."]
  }
}
```

### Response (401 Unauthorized) - Chưa đăng nhập
```json
{
  "status": 401,
  "title": "Unauthorized",
  "detail": "Vui lòng đăng nhập để sử dụng tính năng Trợ lý ảo AI."
}
```

### Response (429 Too Many Requests) - Vượt giới hạn rate limit
```json
{
  "status": 429,
  "title": "Rate Limit Exceeded",
  "detail": "Trợ lý AI đang quá tải lượt truy vấn (Rate Limit). Vui lòng thử lại sau ít phút."
}
```

### Response (500 Internal Server Error) - Lỗi kết nối Google API hoặc hết quota
```json
{
  "status": 500,
  "title": "External Service Error",
  "detail": "Không thể kết nối đến Trợ lý AI lúc này. Hệ thống đang tiến hành khắc phục sự cố."
}
```
