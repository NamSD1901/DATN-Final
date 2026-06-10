# 📖 API Reference Details - Customer Appointment Management

Tài liệu này đặc tả chi tiết giao thức API kết nối (API Contracts) phục vụ các hoạt động tải danh sách lịch hẹn có phân trang, xem chi tiết, và hủy lịch hẹn trực tuyến của khách hàng. Tất cả các endpoint đều yêu cầu xác thực JWT của tài khoản thuộc vai trò `customer`.

---

## 1. GET /api/my-appointments
Lấy danh sách các lịch hẹn khám và tiêm chủng của khách hàng đang đăng nhập hiện tại, hỗ trợ lọc theo trạng thái và phân trang.

* **URL:** `/api/my-appointments`
* **Method:** `GET`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
* **Query Parameters:**
  * `status` (string, optional): Trạng thái cần lọc (`pending`, `confirmed`, `waiting`, `in_progress`, `completed`, `cancelled`). Nếu bỏ trống sẽ lấy tất cả.
  * `page` (int, optional): Chỉ số trang hiện tại. Mặc định là `1`.
  * `pageSize` (int, optional): Số bản ghi trên mỗi trang. Mặc định là `10`.
* **Response (200 OK) - Thành công:**
  ```json
  {
    "items": [
      {
        "id": "e2c38d4f-3721-4f18-a664-d3a373ff2010",
        "petId": 12,
        "petName": "Bé LuLu",
        "petSpecies": "Dog",
        "doctorId": "b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8",
        "doctorName": "BS. Trần Quốc Anh",
        "serviceId": 3,
        "serviceName": "Khám tổng quát chó mèo",
        "vaccineId": null,
        "vaccineName": null,
        "appointmentDate": "2026-06-15T10:00:00Z",
        "status": "pending",
        "symptom": "Chú cún bị nôn mửa liên tục.",
        "note": "Xin xếp bác sĩ có kinh nghiệm.",
        "cancelledReason": null,
        "cancelledAt": null,
        "qrToken": "QR-LH00472"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  }
  ```

---

## 2. GET /api/my-appointments/{id}
Lấy chi tiết thông tin của một lịch hẹn y tế cụ thể. Có kiểm tra IDOR bảo mật chéo.

* **URL:** `/api/my-appointments/e2c38d4f-3721-4f18-a664-d3a373ff2010`
* **Method:** `GET`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
* **Response (200 OK) - Thành công:**
  ```json
  {
    "id": "e2c38d4f-3721-4f18-a664-d3a373ff2010",
    "petId": 12,
    "petName": "Bé LuLu",
    "petSpecies": "Dog",
    "doctorId": "b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8",
    "doctorName": "BS. Trần Quốc Anh",
    "serviceId": 3,
    "serviceName": "Khám tổng quát chó mèo",
    "vaccineId": null,
    "vaccineName": null,
    "appointmentDate": "2026-06-15T10:00:00Z",
    "status": "pending",
    "symptom": "Chú cún bị nôn mửa liên tục.",
    "note": "Xin xếp bác sĩ có kinh nghiệm.",
    "cancelledReason": null,
    "cancelledAt": null,
    "qrToken": "QR-LH00472"
  }
  ```
* **Response (403 Forbidden / 404 Not Found) - Vi phạm IDOR:**
  Trả về khi ID lịch hẹn không thuộc về bất kỳ thú cưng nào của khách hàng đang yêu cầu.
  ```json
  {
    "message": "Lịch hẹn không tồn tại hoặc bạn không có quyền truy cập."
  }
  ```

---

## 3. PUT /api/my-appointments/{id}/cancel
Yêu cầu hủy lịch hẹn đã đặt. Hệ thống chỉ cho phép hủy khi trạng thái hiện tại là `pending` hoặc `confirmed` và đối chiếu khoảng cách thời gian hủy an toàn.

* **URL:** `/api/my-appointments/e2c38d4f-3721-4f18-a664-d3a373ff2010/cancel`
* **Method:** `PUT`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
  * `Content-Type: application/json`
* **Request Body JSON:**
  ```json
  {
    "reason": "Bận lịch công tác đột xuất không thể mang bé đi khám."
  }
  ```
* **Response (200 OK) - Hủy thành công:**
  ```json
  {
    "success": true,
    "message": "Hủy lịch hẹn thành công! Ca trực của bác sĩ đã được giải phóng."
  }
  ```
* **Response (400 Bad Request) - Lý do quá ngắn (Lỗi Validation):**
  ```json
  {
    "errors": {
      "reason": [
        "Lý do hủy lịch phải chứa tối thiểu 10 ký tự."
      ]
    }
  }
  ```
* **Response (400 Bad Request) - Vi phạm ràng buộc trạng thái y tế hoặc thời gian:**
  Ví dụ lịch hẹn đã chuyển sang `in_progress` hoặc đã `confirmed` nhưng chỉ còn 45 phút nữa là đến giờ hẹn.
  ```json
  {
    "success": false,
    "message": "Không thể hủy lịch hẹn: Lịch hẹn đã được xác nhận chỉ có thể hủy trực tuyến trước giờ hẹn tối thiểu 2 tiếng."
  }
  ```
* **Response (403 Forbidden) - Hủy lịch của người khác (IDOR):**
  ```json
  {
    "message": "Bạn không có quyền thực hiện thao tác trên lịch hẹn này."
  }
  ```
* **Response (429 Too Many Requests) - Bị giới hạn tần suất:**
  ```json
  {
    "message": "Bạn đã gửi quá nhiều yêu cầu hủy lịch. Vui lòng thử lại sau 1 phút."
  }
  ```
