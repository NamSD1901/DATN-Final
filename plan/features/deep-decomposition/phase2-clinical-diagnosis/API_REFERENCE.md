# 📖 API Reference Details - Clinical Diagnosis & Treatment

## 1. POST /api/doctor/medical-records
Tạo bệnh án mới, tự động lập đơn thuốc kèm trừ tồn kho thuốc tương ứng.

*   **Auth:** `[Authorize(Roles = "doctor,admin")]`
*   **Request Body:**
    ```json
    {
      "appointmentId": "e2c38d4f-3721-4f18-a664-d3a373ff2010",
      "petId": "cfb42a9b-13a8-443b-bd9d-ef7efef88390",
      "symptoms": "Bỏ ăn 2 ngày, sốt nhẹ",
      "diagnosis": "Viêm đường ruột nhẹ",
      "prescriptionItems": [
        {
          "medicineId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
          "quantity": 5,
          "dosageInstructions": "Uống ngày 2 lần, mỗi lần 1 viên sau ăn"
        }
      ]
    }
    ```
*   **Response (201 Created):**
    Không trả về body.
*   **Response (400 Bad Request - Thiếu thuốc):**
    ```json
    {
      "message": "Thuốc 'Amoxicillin' không đủ tồn kho (Yêu cầu: 5, Tồn: 2)."
    }
    ```

---

## 2. GET /api/pets/{id}/medical-history
Lấy toàn bộ lịch sử bệnh án và nhật ký tiêm phòng của thú cưng để hiển thị lên timeline.

*   **Auth:** `[Authorize(Roles = "doctor,receptionist,admin")]`
*   **Response (200 OK):**
    ```json
    [
      {
        "recordId": "f2c38d4f-3721-4f18-a664-d3a373ff2010",
        "visitDate": "2026-05-10T09:00:00Z",
        "doctorName": "BS. Nguyễn Văn A",
        "symptoms": "Ho khan kéo dài",
        "diagnosis": "Viêm phế quản",
        "medicines": [
          {
            "name": "Siro ho thảo dược",
            "quantity": 1,
            "instructions": "Uống ngày 3 lần, mỗi lần 5ml"
          }
        ]
      }
    ]
    ```
