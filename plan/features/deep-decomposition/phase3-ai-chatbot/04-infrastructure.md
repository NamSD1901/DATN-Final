# 04. Infrastructure & Security - Gemini AI Chatbot

Tài liệu thiết kế hạ tầng, bảo mật thông tin API Key, phân quyền người dùng và chính sách kiểm soát chi phí token Google Cloud.

---

## 1. Bảo mật API Key của Google Gemini ở Tầng Backend

API Key của Google Gemini là tài sản nhạy cảm có tính phí trực tiếp dựa trên lưu lượng token sử dụng. Bất kỳ hành vi để lộ key nào cũng có thể dẫn đến việc hóa đơn Google Cloud bị đội lên hàng ngàn USD hoặc bị kẻ xấu lợi dụng để chạy các mô hình AI khác.

### Các nguyên tắc bảo mật hạ tầng bắt buộc:
1. **Tuyệt đối không lưu key trong mã nguồn (No hardcoded keys):** API Key không được phép xuất hiện dưới dạng chuỗi thô trong bất kỳ file `.cs` hay `.vue` nào.
2. **Sử dụng Cấu hình Biến môi trường (Environment Variables):** Key được lưu trữ trong biến môi trường của hệ thống vận hành hoặc thông qua file bảo mật `appsettings.json` bị loại trừ khỏi Git `.gitignore`.
   ```json
   {
     "GeminiSettings": {
       "ApiKey": "AIzaSyD-Your-Secret-Gemini-Key-Here"
     }
   }
   ```
3. **Cơ chế API Proxy Gateway:** Toàn bộ mã nguồn Javascript của Vue 3 Client không bao giờ gọi trực tiếp sang Google API. Tất cả các yêu cầu chat phải đi qua Backend Web API của MyPetClinic đóng vai trò trung gian xác thực (Proxy). Nhờ đó, API Key được bảo vệ an toàn tuyệt đối bên sau tường lửa Backend.

---

## 2. Phân quyền và Xác thực Yêu cầu Chat (Authentication & Access Control)

Nhằm kiểm soát tài nguyên hệ thống và tránh bị bots tấn công spam tự động:
- **Xác thực JWT:** Chỉ cho phép người dùng đã đăng nhập hệ thống và có Token JWT hợp lệ mới được quyền gọi API chat (`[Authorize]`). Chặn hoàn toàn khách vãng lai chưa đăng ký tài khoản.
- **Xác định thông tin người dùng:** Backend sử dụng `ClaimTypes.NameIdentifier` lấy ra ID người dùng hiện tại để phục vụ tính toán giới hạn Rate Limiting cá nhân.

---

## 3. Chính sách Rate Limiting Kiểm soát Chi phí (Token Cost Control)

Mô hình Gemini tính phí dựa trên số lượng Input Tokens (câu hỏi + lịch sử) và Output Tokens (câu trả lời). Để kiểm soát tài chính của phòng khám:
- **Giới hạn số lần hỏi:** Áp dụng bộ lọc Rate Limiting chỉ cho phép tối đa **15 requests / phút** trên một tài khoản khách hàng. Khi vượt quá, hệ thống trả về mã lỗi `429 Too Many Requests`.
- **Giới hạn độ dài câu hỏi:** Ô nhập tin nhắn phía Client và validator Backend chặn không cho phép gửi câu hỏi dài quá **2000 ký tự**.
- **Giới hạn độ dài lịch sử ngữ cảnh:** Store chỉ gửi kèm tối đa 10 tin nhắn gần nhất làm lịch sử. Các tin nhắn cũ hơn sẽ bị giải phóng khỏi payload để tiết kiệm chi phí token đầu vào.
- **Giới hạn số token đầu ra (maxOutputTokens):** Cấu hình mô hình Gemini giới hạn tối đa **800 tokens** cho mỗi câu trả lời, tránh việc AI trả lời quá lan man dài dòng gây tốn token vô ích.
- **Mức độ sáng tạo (Temperature):** Thiết lập `temperature = 0.2` giúp câu trả lời của AI luôn nhất quán, tập trung vào y học chính xác và giảm thiểu rủi ro AI "ảo tưởng" (hallucination) đưa ra thông tin y tế sai lệch nguy hiểm.
