# 📖 API Reference Details - Online Examination Booking

Tài liệu này đặc tả chi tiết giao thức API kết nối (API Contracts) phục vụ các hoạt động xem, đặt và hủy lịch khám bệnh y tế trực tuyến của khách hàng. Tất cả các endpoint đều yêu cầu xác thực JWT của tài khoản thuộc vai trò `customer`.

---

## 1. POST /api/my-appointments
Khách hàng thực hiện đăng ký đặt lịch khám y tế cho thú cưng của mình.

*   **URL:** `/api/my-appointments`
*   **Method:** `POST`
*   **Headers:**
    *   `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
    *   `Content-Type: application/json`
*   **Request Body JSON:**
    ```json
    {
      "petId": 12,
      "doctorId": "b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8",
      "serviceId": 3,
      "appointmentDate": "2026-06-15T10:00:00Z",
      "symptom": "Chú cún bị nôn mửa liên tục từ tối qua và bỏ ăn.",
      "note": "Xin vui lòng xếp lịch cho bác sĩ có kinh nghiệm ngoại khoa."
    }
    ```
    *Lưu ý: Nếu không chỉ định doctorId, có thể gửi `null` hoặc bỏ trống để hệ thống tự phân bổ bác sĩ trực.*
*   **Response (200 OK) - Thành công:**
    Trả về id lịch hẹn vừa tạo để theo dõi trạng thái.
    ```json
    {
      "success": true,
      "message": "Đặt lịch hẹn thành công! Chúng tôi sẽ xác nhận sớm.",
      "id": 147
    }
    ```
*   **Response (400 Bad Request) - Lỗi trùng lịch (Double-booked):**
    ```json
    {
      "success": false,
      "message": "Đặt lịch thất bại: Bác sĩ đã có lịch hẹn trong khoảng thời gian này."
    }
    ```
*   **Response (400 Bad Request) - Thú cưng không hợp lệ (IDOR Block):**
    ```json
    {
      "message": "Thú cưng không hợp lệ hoặc không thuộc về bạn."
    }
    ```
*   **Response (400 Bad Request) - Lỗi Validation đầu vào:**
    ```json
    {
      "errors": {
        "symptom": [
          "Vui lòng mô tả triệu chứng hoặc lý do khám bệnh."
        ],
        "appointmentDate": [
          "Thời gian hẹn khám phải lớn hơn thời gian hiện tại."
        ]
      }
    }
    ```

---

## 2. GET /api/my-appointments
Lấy toàn bộ danh sách lịch hẹn khám của khách hàng đang đăng nhập.

*   **URL:** `/api/my-appointments`
*   **Method:** `GET`
*   **Response (200 OK):**
    Trả về danh sách lịch hẹn sắp xếp theo thứ tự thời gian mới nhất ở trên.
    ```json
    [
      {
        "id": 147,
        "petId": 12,
        "petName": "Bé Leo",
        "species": "dog",
        "breed": "Poodle",
        "customerId": "a3b2c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d",
        "customerName": "Nguyễn Khách Hàng",
        "customerPhone": "0987654321",
        "serviceId": 3,
        "serviceName": "Khám tổng quát chó mèo",
        "servicePrice": 150000.0,
        "doctorId": "b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8",
        "doctorName": "Bác sĩ Trần Quốc Anh",
        "appointmentDate": "2026-06-15T10:00:00Z",
        "symptom": "Chú cún bị nôn mửa liên tục...",
        "note": "Xin vui lòng xếp lịch...",
        "status": "pending",
        "qrToken": "QR-C4D5E6F7",
        "invoiceId": null,
        "invoiceStatus": null,
        "invoiceTotalAmount": null
      }
    ]
    ```

---

## 3. PUT /api/my-appointments/{id}/cancel
Khách hàng tự hủy lịch hẹn đã đặt (Chỉ cho phép khi trạng thái lịch là `pending` hoặc `confirmed`, không cho phép hủy khi đã check-in `waiting` hoặc đang khám `in_progress`).

*   **URL:** `/api/my-appointments/147/cancel`
*   **Method:** `PUT`
*   **Response (200 OK):**
    ```json
    {
      "success": true,
      "message": "Đã huỷ lịch hẹn thành công."
    }
    ```
*   **Response (400 Bad Request) - Không thể hủy lịch:**
    ```json
    {
      "message": "Không thể huỷ lịch hẹn ở trạng thái 'in_progress'."
    }
    ```
*   **Response (403 Forbidden) - Hủy lịch của người khác:**
    Trả về khi ID lịch hẹn không thuộc sở hữu của tài khoản đang gửi request.
