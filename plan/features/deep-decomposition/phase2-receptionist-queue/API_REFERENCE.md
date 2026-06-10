# 📖 API Reference Details - Receptionist Portal & Queue Management

Tài liệu này đặc tả chi tiết giao thức API kết nối (API Contracts) phục vụ các hoạt động duyệt lịch hẹn, quét mã QR check-in, đăng ký khách vãng lai (Walk-in) và điều phối trạng thái hàng chờ của Lễ tân. Tất cả các endpoint đều yêu cầu xác thực JWT của tài khoản thuộc vai trò `receptionist` hoặc `admin`.

---

## 1. GET /api/receptionist/queue
Lấy toàn bộ danh sách hàng đợi khám của ngày hôm nay để đổ vào bảng Kanban.

* **URL:** `/api/receptionist/queue`
* **Method:** `GET`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
* **Response (200 OK) - Thành công:**
  ```json
  [
    {
      "appointmentId": "e2c38d4f-3721-4f18-a664-d3a373ff2010",
      "queueNumber": "Q-012",
      "petId": 12,
      "petName": "Bé Miu",
      "petSpecies": "Cat",
      "customerName": "Nguyễn Thu Hà",
      "doctorName": "BS. Trần Quốc Anh",
      "roomName": "Phòng khám 101",
      "status": "waiting",
      "checkInTime": "2026-06-10T17:15:30Z"
    }
  ]
  ```

---

## 2. POST /api/receptionist/check-in
Xác nhận check-in cho khách hàng có lịch hẹn trước bằng ID lịch hẹn và phân bổ phòng khám.

* **URL:** `/api/receptionist/check-in`
* **Method:** `POST`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
  * `Content-Type: application/json`
* **Request Body JSON:**
  ```json
  {
    "appointmentId": "e2c38d4f-3721-4f18-a664-d3a373ff2010",
    "clinicRoom": "Phòng khám 101"
  }
  ```
* **Response (200 OK) - Thành công:**
  ```json
  {
    "appointmentId": "e2c38d4f-3721-4f18-a664-d3a373ff2010",
    "queueNumber": "Q-012",
    "petName": "Bé Miu",
    "customerName": "Nguyễn Thu Hà",
    "doctorName": "BS. Trần Quốc Anh",
    "roomName": "Phòng khám 101",
    "status": "waiting",
    "checkInTime": "2026-06-10T17:15:30Z"
  }
  ```
* **Response (400 Bad Request) - Lịch hẹn không ở trạng thái được check-in:**
  Ví dụ lịch hẹn đã bị hủy (`cancelled`) hoặc đang được khám (`in_progress`).
  ```json
  {
    "message": "Trạng thái lịch hẹn hiện tại không cho phép check-in."
  }
  ```

---

## 3. POST /api/receptionist/walk-in
Đăng ký nhanh khách vãng lai không đặt lịch trước, tự động cấp số thứ tự và xếp vào hàng đợi trực tiếp.

* **URL:** `/api/receptionist/walk-in`
* **Method:** `POST`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
  * `Content-Type: application/json`
* **Request Body JSON:**
  ```json
  {
    "customerName": "Hoàng Văn Nam",
    "customerPhone": "0987654321",
    "petName": "Bé Cún",
    "species": "Dog",
    "breed": "Poodle",
    "serviceId": 3,
    "assignedDoctorId": "b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8",
    "clinicRoom": "Phòng khám 102",
    "symptom": "Chú cún bị mẩn ngứa da."
  }
  ```
* **Response (200 OK) - Đăng ký & check-in thành công:**
  ```json
  {
    "appointmentId": "d5f6e7a8-3721-4f18-a664-d3a373ff2012",
    "queueNumber": "Q-013",
    "petId": 88,
    "petName": "Bé Cún",
    "petSpecies": "Dog",
    "customerName": "Hoàng Văn Nam",
    "doctorName": "BS. Nguyễn Đức",
    "roomName": "Phòng khám 102",
    "status": "waiting",
    "checkInTime": "2026-06-10T17:20:45Z"
  }
  ```
* **Response (400 Bad Request) - Lỗi Validation đầu vào:**
  ```json
  {
    "errors": {
      "customerPhone": [
        "Số điện thoại không đúng định dạng Việt Nam."
      ]
    }
  }
  ```

---

## 4. PUT /api/receptionist/queue/{appointmentId}/status
Cập nhật trạng thái và phòng khám của một ca trong hàng đợi (sử dụng khi di chuyển các cột Kanban).

* **URL:** `/api/receptionist/queue/e2c38d4f-3721-4f18-a664-d3a373ff2010/status`
* **Method:** `PUT`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
  * `Content-Type: application/json`
* **Request Body JSON:**
  ```json
  {
    "status": "in_progress",
    "clinicRoom": "Phòng khám 101"
  }
  ```
* **Response (200 OK) - Cập nhật thành công:**
  ```json
  {
    "success": true,
    "message": "Cập nhật trạng thái hàng đợi thành công."
  }
  ```
* **Response (409 Conflict) - Tranh chấp cập nhật trạng thái:**
  Xảy ra khi Bác sĩ đã kết thúc ca khám (`completed`) nhưng Lễ tân cố kéo thả ngược lại hàng chờ.
  ```json
  {
    "success": false,
    "message": "Không thể cập nhật trạng thái: Ca khám này đã hoàn thành và khóa dữ liệu y tế."
  }
  ```
