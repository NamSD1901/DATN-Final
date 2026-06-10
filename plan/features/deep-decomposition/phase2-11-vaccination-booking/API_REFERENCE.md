# 📖 API Reference Details - Online Vaccination Booking

Tài liệu này đặc tả chi tiết giao thức API kết nối (API Contracts) phục vụ các hoạt động tra cứu vắc-xin, xem lịch sử tiêm chủng, kiểm tra phác đồ an toàn và đặt lịch tiêm phòng trực tuyến của khách hàng. Tất cả các endpoint đều yêu cầu xác thực JWT của tài khoản thuộc vai trò `customer`.

---

## 1. GET /api/vaccination/available-vaccines
Lấy danh sách các loại vắc-xin đang được cung cấp và còn hàng tại kho dược của phòng khám.

* **URL:** `/api/vaccination/available-vaccines`
* **Method:** `GET`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
* **Response (200 OK) - Thành công:**
  ```json
  [
    {
      "id": 15,
      "name": "Vắc-xin Dại Rabisin (Chó/Mèo)",
      "targetSpecies": "All",
      "description": "Vắc-xin phòng bệnh dại cho chó và mèo từ 3 tháng tuổi trở lên.",
      "price": 120000.0,
      "stockQuantity": 45,
      "minAgeWeeks": 12,
      "intervalDays": 330
    },
    {
      "id": 16,
      "name": "Vắc-xin 4 Bệnh Mèo (Nobivac Tricat Novum)",
      "targetSpecies": "Cat",
      "description": "Phòng bệnh giảm bạch cầu, viêm mũi khí quản, tịt mũi do Herpesvirus và Calicivirus ở mèo.",
      "price": 350000.0,
      "stockQuantity": 8,
      "minAgeWeeks": 8,
      "intervalDays": 21
    }
  ]
  ```

---

## 2. GET /api/vaccination/pet-history/{petId}
Lấy lịch sử tiêm chủng của một thú cưng cụ thể. Endpoint này tích hợp bộ lọc chống IDOR để đảm bảo khách hàng chỉ xem được thú cưng của mình.

* **URL:** `/api/vaccination/pet-history/12`
* **Method:** `GET`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
* **Response (200 OK) - Thành công:**
  ```json
  [
    {
      "id": 89,
      "petId": 12,
      "vaccineId": 16,
      "vaccineName": "Vắc-xin 4 Bệnh Mèo (Nobivac Tricat Novum)",
      "vaccinatedDate": "2026-05-10",
      "nextDueDate": "2026-05-31",
      "notes": "Tiêm mũi 1, sức khỏe bình thường."
    }
  ]
  ```
* **Response (403 Forbidden) - Vi phạm IDOR:**
  Trả về khi `petId` không thuộc sở hữu của khách hàng đang đăng nhập.
  ```json
  {
    "message": "Thú cưng không hợp lệ hoặc không thuộc quyền sở hữu của bạn."
  }
  ```

---

## 3. POST /api/vaccination/validate-interval
Kiểm tra phác đồ tiêm chủng y tế và tính toán khoảng cách thời gian tiêm an toàn dựa trên lịch sử của thú cưng.

* **URL:** `/api/vaccination/validate-interval`
* **Method:** `POST`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
  * `Content-Type: application/json`
* **Request Body JSON:**
  ```json
  {
    "petId": 12,
    "vaccineId": 16,
    "proposedDate": "2026-06-15T10:00:00Z"
  }
  ```
* **Response (200 OK) - Trường hợp 1: Phác đồ hợp lệ (Đủ khoảng cách thời gian khuyến nghị):**
  ```json
  {
    "isValid": true,
    "lastVaccinatedDate": "2026-05-10",
    "recommendedDate": "2026-05-31",
    "warningMessage": null,
    "requiresDoctorOverride": false
  }
  ```
* **Response (200 OK) - Trường hợp 2: Vi phạm phác đồ (Tiêm quá sớm):**
  Ví dụ, khoảng cách tối thiểu giữa 2 mũi 4 bệnh của mèo là 21 ngày, nhưng khách đặt lịch chỉ cách mũi 1 có 15 ngày.
  ```json
  {
    "isValid": false,
    "lastVaccinatedDate": "2026-06-05",
    "recommendedDate": "2026-06-26",
    "warningMessage": "Cảnh báo: Bé đã tiêm mũi gần nhất vào ngày 05/06/2026. Thời gian tiêm nhắc lại khuyến nghị là sau ngày 26/06/2026 (cách tối thiểu 21 ngày).",
    "requiresDoctorOverride": true
  }
  ```

---

## 4. POST /api/vaccination/book
Gửi yêu cầu đặt lịch hẹn tiêm phòng vắc-xin chính thức.

* **URL:** `/api/vaccination/book`
* **Method:** `POST`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
  * `Content-Type: application/json`
* **Request Body JSON:**
  ```json
  {
    "petId": 12,
    "vaccineId": 16,
    "doctorId": "b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8",
    "appointmentDate": "2026-06-26T09:30:00Z",
    "symptom": "Tiêm phòng vắc-xin định kỳ theo phác đồ",
    "note": "Xin xếp lịch tiêm mũi 2.",
    "bypassWarning": true,
    "requiresOverride": true
  }
  ```
* **Response (200 OK) - Đặt lịch thành công:**
  ```json
  {
    "success": true,
    "message": "Đặt lịch tiêm phòng thành công! Trạng thái lịch hẹn đang chờ xác nhận.",
    "appointmentId": 150
  }
  ```
* **Response (400 Bad Request) - Lỗi hết hàng vắc-xin trong kho dược:**
  ```json
  {
    "success": false,
    "message": "Đặt lịch thất bại: Loại vắc-xin được chọn hiện đã hết hàng tại kho dược."
  }
  ```
* **Response (400 Bad Request) - Không thể tự ý bỏ qua cảnh báo y khoa:**
  Trả về khi vi phạm khoảng cách y khoa (`requiresOverride: true`) nhưng cờ xác nhận `bypassWarning` trong request body được gửi lên là `false`.
  ```json
  {
    "success": false,
    "message": "Không thể tạo lịch hẹn: Đề xuất tiêm phòng vi phạm phác đồ thời gian an toàn. Vui lòng xác nhận đồng ý với cảnh báo y khoa trước khi tiếp tục."
  }
  ```
* **Response (429 Too Many Requests) - Bị giới hạn tần suất gửi yêu cầu:**
  ```json
  {
    "message": "Bạn đã gửi quá nhiều yêu cầu đặt lịch. Vui lòng thử lại sau 1 phút."
  }
  ```
