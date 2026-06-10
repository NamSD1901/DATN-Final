# 📄 User & Dev Documentation - Receptionist Portal & Queue Management

Tài liệu này cung cấp hướng dẫn vận hành sảnh tiếp tiếp đón dành cho nhân viên lễ tân và cẩm nang tích hợp kỹ thuật chi tiết dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng & Quy trình vận hành (Operator & End-User Guide)

### Cấu hình Thiết bị ngoại vi (Barcode Scanner Setup):
Để tối ưu hóa luồng tiếp nhận check-in "không chạm", phòng khám sử dụng máy quét mã vạch USB HID cầm tay kết nối trực tiếp với máy tính của lễ tân:
1. Cắm máy quét mã vạch qua cổng USB. Máy tính tự động nhận diện thiết bị như một bàn phím phụ (Keyboard Emulator).
2. Quét mã vạch cấu hình đi kèm sách hướng dẫn của máy quét để bật chế độ **"Automatic Carriage Return (CR)"** hoặc **"Enter Suffix"**.
3. *Kết quả:* Mỗi khi máy quét nhận dạng được QR Code, nó sẽ tự động chèn ký tự `Enter` ở cuối chuỗi token, kích hoạt hàm submit check-in trên giao diện mà lễ tân không cần bấm phím thủ công.

### Quy trình duyệt lịch & tiếp đón vật lý:
1. **Duyệt lịch đặt trực tuyến:** Lễ tân mở Tab "Chờ duyệt". Kiểm tra thông tin ca đặt hẹn của khách hàng, nhấp "Duyệt" (Confirm) để xác nhận ca trực cho bác sĩ, hoặc "Từ chối" (Reject) nhập lý do.
2. **Check-in khách đặt trước:** Hướng máy quét vào mã QR trên điện thoại của chủ nuôi. Hệ thống tự động điền Token check-in, gọi API cấp Số thứ tự y tế, và in phiếu khám ra máy in nhiệt tại quầy.
3. **Tiếp nhận khách vãng lai (Walk-in):** Nhấp nút "Đăng ký Walk-in", điền thông tin chủ nuôi + thú cưng vào Quick Form, chọn phòng khám chỉ định và nhấn "Xếp hàng khám".

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc thư mục liên quan trong dự án
* **Backend .NET Core App:**
  * [ReceptionistService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/ReceptionistService.cs) — Logic xử lý hàng đợi, sinh số thứ tự và phân bổ phòng khám.
  * `MyPetClinic.Application/Hubs/QueueHub.cs` — SignalR Hub Gateway đẩy sự kiện thay đổi hàng chờ.
  * `MyPetClinic.Domain/Entities/Appointment.cs` — Chứa các cột dữ liệu hàng đợi `QueueNumber`, `CheckInTime`, `ClinicRoom`.
* **Frontend Vue 3 SPA:**
  * `frontend/src/stores/receptionistQueue.ts` — Pinia Store quản lý state Kanban và kết nối SignalR Client.
  * `frontend/src/views/receptionist/QueueBoard.vue` — Component Vue hiển thị bảng Kanban điều phối hàng đợi.
  * `frontend/src/views/receptionist/PendingApprovals.vue` — Giao diện duyệt lịch hẹn trực tuyến.

### B. Kiểm thử API bằng Curl
Các nhà phát triển sử dụng các lệnh curl sau để kiểm thử nhanh API tại Terminal (yêu cầu gửi kèm Token JWT Receptionist hoặc Admin hợp lệ):

#### 1. Lấy danh sách hàng đợi khám hôm nay
```bash
curl -X GET "https://localhost:5001/api/receptionist/queue" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_RECEPTIONIST_TẠI_ĐÂY>"
```

#### 2. Thực hiện Check-in cấp số thứ tự
```bash
curl -X POST "https://localhost:5001/api/receptionist/check-in" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_RECEPTIONIST_TẠI_ĐÂY>" \
     -d "{\"appointmentId\":\"e2c38d4f-3721-4f18-a664-d3a373ff2010\",\"clinicRoom\":\"Phòng khám 101\"}"
```

#### 3. Tạo nhanh ca khám vãng lai (Walk-in)
```bash
curl -X POST "https://localhost:5001/api/receptionist/walk-in" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_RECEPTIONIST_TẠI_ĐÂY>" \
     -d "{\"customerName\":\"Hoàng Văn Nam\",\"customerPhone\":\"0987654321\",\"petName\":\"Bé Cún\",\"species\":\"Dog\",\"breed\":\"Poodle\",\"serviceId\":3,\"clinicRoom\":\"Phòng khám 102\",\"symptom\":\"Ngứa da\"}"
```

---

## 3. Khắc phục sự cố thường gặp (Troubleshooting)

### Sự cố 1: Lỗi kết nối SignalR (WebSocket Handshake Failed)
* **Nguyên nhân:** Lỗi xảy ra khi IIS/Kestrel proxy hoặc CDN (như Cloudflare) chưa bật cấu hình cho phép WebSockets, hoặc do thiếu token JWT trong Header kết nối.
* **Khắc phục:**
  1. Đảm bảo cấu hình SignalR Client Vue truyền đúng token JWT qua query string khi bắt đầu kết nối:
     ```typescript
     accessTokenFactory: () => localStorage.getItem('token') || ''
     ```
  2. Bật cổng WebSockets trong mục cài đặt Network của Cloudflare hoặc file cấu hình Nginx:
     ```nginx
     proxy_set_header Upgrade $http_upgrade;
     proxy_set_header Connection "upgrade";
     ```

### Sự cố 2: Giờ check-in hiển thị lệch múi giờ trên phiếu in nhiệt
* **Nguyên nhân:** Máy in nhiệt đọc giờ hệ thống trực tiếp từ API gửi về. Nếu API gửi chuỗi UTC (`2026-06-10T10:30:00Z`), máy in (hoặc thư viện in ấn) có thể in thô chuỗi đó ra phiếu khám khiến khách hàng hoang mang vì bị lùi đi 7 tiếng so với thực tế (GMT+7).
* **Khắc phục:** Trước khi gửi lệnh in nhiệt hoặc truyền DTO ra máy in, tại Client bắt buộc phải format ngày giờ về chuỗi Local String đã cộng múi giờ địa phương:
  ```javascript
  const printTime = new Date(checkInTimeUtc).toLocaleString('vi-VN', { timeZone: 'Asia/Ho-Chi-Minh' });
  ```
