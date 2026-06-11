# 📖 API Reference Details - Clinical Diagnosis & Treatment

Tài liệu này đặc tả chi tiết giao thức API kết nối (API Contracts) phục vụ các hoạt động tiếp nhận khám, tra cứu bệnh sử cũ của thú cưng, tìm kiếm thuốc gợi ý và lưu bệnh án kê đơn của Bác sĩ. Tất cả các endpoint đều yêu cầu xác thực JWT của tài khoản thuộc vai trò `doctor` hoặc `admin`.

---

## 1. PUT /api/doctor/appointments/{id}/start
Bác sĩ xác nhận bắt đầu thực hiện khám lâm sàng cho thú cưng.

* **URL:** `/api/doctor/appointments/e2c38d4f-3721-4f18-a664-d3a373ff2010/start`
* **Method:** `PUT`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
* **Response (200 OK) - Thành công:**
  ```json
  {
    "success": true,
    "message": "Ca khám đã chính thức bắt đầu.",
    "startExamTime": "2026-06-10T17:30:15Z"
  }
  ```
* **Response (400 Bad Request) - Lỗi trạng thái:**
  Nếu lịch hẹn chưa check-in sảnh (`waiting`) hoặc đã xong.
  ```json
  {
    "message": "Không thể bắt đầu ca khám: Lịch hẹn không ở trạng thái chờ khám."
  }
  ```

---

## 2. GET /api/doctor/pets/{petId}/medical-history
Tải toàn bộ lịch sử khám chữa bệnh và đơn thuốc cũ của thú cưng để phục vụ bác sĩ nghiên cứu bệnh sử.

* **URL:** `/api/doctor/pets/12/medical-history`
* **Method:** `GET`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
* **Response (200 OK) - Thành công:**
  ```json
  [
    {
      "id": 85,
      "diagnosis": "Viêm phế quản cấp tính",
      "treatmentPlan": "Giữ ấm cổ, hạn chế tắm nước lạnh.",
      "createdAt": "2026-05-10T09:00:00Z",
      "prescriptionItems": [
        {
          "medicineName": "Siro ho thảo dược Astex",
          "quantity": 1,
          "dosageInstructions": "Uống ngày 3 lần, mỗi lần 5ml sau ăn."
        }
      ]
    }
  ]
  ```
* **Response (403 Forbidden) - Vi phạm IDOR:**
  Trả về khi bác sĩ cố tình truy quét bệnh sử của thú cưng không nằm trong danh sách khám hôm nay.
  ```json
  {
    "message": "Thú cưng không nằm trong danh sách khám hôm nay của bạn. Quyền truy cập bệnh sử bị từ chối."
  }
  ```

---

## 3. GET /api/medicines/autocomplete
Gợi ý danh mục thuốc hoạt chất từ kho dược phục vụ bộ gõ Autocomplete.

* **URL:** `/api/medicines/autocomplete`
* **Method:** `GET`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
* **Query Parameters:**
  * `query` (string, required): Từ khóa tìm kiếm theo tên thuốc hoặc hoạt chất (Ví dụ: `amox`).
* **Response (200 OK) - Thành công:**
  ```json
  [
    {
      "id": 16,
      "name": "Amoxicillin 500mg (Kháng sinh)",
      "activeIngredient": "Amoxicillin Trihydrate",
      "stockQuantity": 150,
      "price": 8500.0,
      "unit": "Viên"
    },
    {
      "id": 17,
      "name": "Amoxicillin Siro 250mg/5ml",
      "activeIngredient": "Amoxicillin",
      "stockQuantity": 0,
      "price": 45000.0,
      "unit": "Chai"
    }
  ]
  ```

---

## 4. POST /api/doctor/medical-records
Lưu bệnh án lâm sàng, tạo đơn thuốc, tự động trừ tồn kho khả dụng của thuốc và sinh hóa đơn nháp.

* **URL:** `/api/doctor/medical-records`
* **Method:** `POST`
* **Headers:**
  * `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
  * `Content-Type: application/json`
* **Request Body JSON:**
  ```json
  {
    "appointmentId": 147,
    "petId": 12,
    "diagnosis": "Viêm dạ dày ruột cấp tính, có biểu hiện mất nước nhẹ.",
    "treatmentPlan": "Kiêng ăn thức ăn dầu mỡ trong 3 ngày. Uống nhiều nước ấm.",
    "prescriptionItems": [
      {
        "medicineId": 16,
        "quantity": 10,
        "dosageInstructions": "Uống ngày 2 lần, mỗi lần 1 viên sau khi ăn."
      }
    ]
  }
  ```
* **Response (201 Created) - Lưu thành công:**
  ```json
  {
    "success": true,
    "medicalRecordId": 189,
    "message": "Ghi nhận bệnh án và kê đơn thuốc thành công. Đã sinh hóa đơn nháp chuyển quầy thu ngân."
  }
  ```
* **Response (400 Bad Request) - Lỗi tồn kho dược không đủ:**
  ```json
  {
    "success": false,
    "message": "Lưu bệnh án thất bại: Thuốc 'Amoxicillin 500mg' trong kho hiện chỉ còn 8 liều, không đủ số lượng yêu cầu: 10."
  }
  ```
* **Response (400 Bad Request) - Lỗi y khoa trùng lặp hoạt chất:**
  ```json
  {
    "success": false,
    "message": "CẢNH BÁO Y TẾ: Đơn thuốc chứa hoạt chất trùng lặp có thể gây quá liều: Hoạt chất 'amoxicillin' có trong các thuốc: Amoxicillin 500mg, Amoxicillin Siro."
  }
  ```
