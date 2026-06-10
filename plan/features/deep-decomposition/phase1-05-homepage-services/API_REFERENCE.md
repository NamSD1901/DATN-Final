# 📄 API Reference - Services Catalog (Phase 1)

Tài liệu này đặc tả chi tiết giao diện lập trình ứng dụng (API Contract) cho danh mục dịch vụ công khai của hệ thống **MyPetClinic**.

---

## 1. API Lấy danh sách Dịch vụ công khai

Truy vấn danh mục dịch vụ, hỗ trợ các bộ lọc tùy chọn. Không yêu cầu đăng nhập.

*   **URL:** `/api/services`
*   **Method:** `GET`
*   **Headers:**
    *   `Accept: application/json`

### 1.1. Request Query Parameters
*   **categoryCode** (string, optional): Lọc theo mã danh mục (ví dụ: `kham-benh`, `tiem-phong`, `phau-thuat`, `spa`).
*   **search** (string, optional): Từ khóa tìm kiếm theo tên hoặc mô tả dịch vụ.

### 1.2. Response Success (HTTP 200 OK)
Trả về danh sách các dịch vụ hợp lệ.
```json
[
  {
    "id": "b12c3d4e-5f6g-7h8i-9j1k-2l3m4n5o6p7q",
    "name": "Tiêm Phòng Dại Cho Mèo",
    "description": "Tiêm vắc-xin phòng bệnh dại định kỳ hàng năm cho mèo trên 3 tháng tuổi. Bao gồm khám sàng lọc trước khi tiêm.",
    "price": 150000.00,
    "imageUrl": "https://mypetclinic.com/images/services/rabies-vaccine.jpg",
    "categoryName": "Tiêm chủng",
    "categoryCode": "tiem-phong"
  },
  {
    "id": "c23d4e5f-6g7h-8i9j-1k2l-3m4n5o6p7q8r",
    "name": "Tắm Sấy Spa Trọn Gói Mèo Cưng",
    "description": "Dịch vụ làm sạch tai, cắt móng, tắm bồn xà bông thảo dược khử mùi và sấy khô tạo kiểu lông.",
    "price": 250000.00,
    "imageUrl": "https://mypetclinic.com/images/services/cat-spa.jpg",
    "categoryName": "Spa & Làm đẹp",
    "categoryCode": "spa"
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
