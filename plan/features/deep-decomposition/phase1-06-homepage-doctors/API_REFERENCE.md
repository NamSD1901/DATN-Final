# 📄 API Reference - Vets Team (Phase 1)

Tài liệu này đặc tả chi tiết giao diện lập trình ứng dụng (API Contract) cho danh sách bác sĩ công khai của hệ thống **MyPetClinic**.

---

## 1. API Lấy danh sách Bác sĩ công khai

Truy vấn danh sách bác sĩ thú y, hỗ trợ các bộ lọc tùy chọn. Không yêu cầu đăng nhập.

*   **URL:** `/api/doctors`
*   **Method:** `GET`
*   **Headers:**
    *   `Accept: application/json`

### 1.1. Request Query Parameters
*   **specialtyCode** (string, optional): Lọc theo mã chuyên khoa (ví dụ: `noi-khoa`, `da-lieu`, `ngoai-khoa`, `tiem-phong`).

### 1.2. Response Success (HTTP 200 OK)
Trả về danh sách các bác sĩ hợp lệ.
```json
[
  {
    "id": "d3b07384-d113-4ec6-a5d6-c8c3e8a6a123",
    "userId": "e4c18495-e224-5fd7-b6e7-d9d4f9b7b234",
    "fullName": "ThS. BS. Nguyễn Văn Minh",
    "email": "doctor.minh@mypetclinic.com",
    "specialty": "DaLieu",
    "specialtyCode": "da-lieu",
    "experienceYears": 8,
    "qualifications": "Thạc sĩ Da liễu thú y Đại học Nông Lâm",
    "biography": "Chuyên gia với hơn 8 năm nghiên cứu điều trị các bệnh dị ứng da, rụng lông và phục hồi cấu trúc biểu bì thú cưng.",
    "avatarUrl": "https://mypetclinic.com/images/avatars/doctor-minh.jpg",
    "isOnDuty": true
  },
  {
    "id": "f5d295a6-f335-6ae8-c7f8-e0e5a0c8c345",
    "userId": "f6d3a6b7-f446-7bf9-d8fa-f1f6b1d9d456",
    "fullName": "BS. Nguyễn Thị Vy",
    "email": "doctor.vy@mypetclinic.com",
    "specialty": "NoiKhoa",
    "specialtyCode": "noi-khoa",
    "experienceYears": 6,
    "qualifications": "Bác sĩ thú y tốt nghiệp loại ưu Đại học Nông Nghiệp",
    "biography": "Kinh nghiệm chuyên sâu trong chẩn đoán lâm sàng nội khoa, siêu âm chẩn đoán hình ảnh điều trị các bệnh tiêu hoá động vật.",
    "avatarUrl": "https://mypetclinic.com/images/avatars/doctor-vy.jpg",
    "isOnDuty": true
  }
]
```

### 1.3. Response Fail - Rate Limit Exceeded (HTTP 429 Too Many Requests)
Trả về khi địa chỉ IP gửi quá 60 yêu cầu trong vòng 1 phút.
```json
{
  "success": false,
  "errorType": "RATE_LIMIT_EXCEEDED",
  "message": "Bạn đã gửi yêu cầu quá nhanh. Vui lòng thử lại sau."
}
```

### 1.4. Response Fail - Server Error (HTTP 500 Internal Server Error)
```json
{
  "success": false,
  "errorType": "INTERNAL_SERVER_ERROR",
  "message": "Đã xảy ra lỗi không xác định tại máy chủ. Vui lòng liên hệ bộ phận hỗ trợ kỹ thuật."
}
```
