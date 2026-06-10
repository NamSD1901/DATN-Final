# 🟡 Level 2: QA Core (Sprint 2-4)

Cấp độ Cốt lõi trang bị cho QA năng lực kiểm thử API chuyên sâu bằng Postman, truy vấn SQL để xác thực dữ liệu cơ sở dữ liệu PostgreSQL và kiểm tra độ tương thích giao diện (Responsive/Cross-browser).

---

## 🎯 QA-06: API Testing with Postman

### 1. Cấu Trúc Postman Collection
```
MyPetClinic API
├── 🟢 Auth (Register, Login, Refresh Token)
├── 🟢 Pets (GET My Pets, POST Create Pet, PUT Update, DELETE)
├── 🟢 Bookings (GET My Bookings, POST Create, PUT Status, GET Available Slots)
├── 🟢 Medical Records (GET History, POST Create Record, POST Prescribe)
└── 🟢 Admin (GET Dashboard Stats, CRUD Users/Medicines)
```

### 2. Kịch Bản Kiểm Thử API (Mẫu)
* **Endpoint:** `POST /api/v1/bookings`
* **Xác thực:** Bearer Token (JWT Cookie)
* **Body Request:**
  ```json
  {
    "petId": "abc-123-def",
    "serviceId": "svc-001",
    "slotId": "slot-2024-12-15-0900",
    "vetId": null,
    "notes": "Milo bị ho"
  }
  ```
* **Kịch bản âm tính (Negative):**
  - Thiếu `petId` -> Trả về `400 Bad Request` kèm thông điệp báo lỗi.
  - Lịch hẹn quá khứ -> Trả về `400 Bad Request`.
  - Khách hàng khác gọi API sửa booking -> Trả về `403 Forbidden`.

### 3. Viết Test Scripts Tự Động Trong Postman (JavaScript)

```javascript
// ============ PRE-REQUEST SCRIPT (Lấy Token Đăng Nhập Tự Động) ============
const loginRequest = {
    url: pm.environment.get("base_url") + "/api/auth/login",
    method: 'POST',
    header: { 'Content-Type': 'application/json' },
    body: {
        mode: 'raw',
        raw: JSON.stringify({
            email: pm.environment.get("test_customer_email"),
            password: pm.environment.get("test_customer_password")
        })
    }
};
pm.sendRequest(loginRequest, (err, response) => {
    if (!err) {
        pm.environment.set("access_token", response.json().data.accessToken);
    }
});

// ============ TEST SCRIPT (Kiểm thử phản hồi) ============
pm.test("Status code is 201 Created", () => {
    pm.response.to.have.status(201);
});

pm.test("Response time is under 1000ms", () => {
    pm.expect(pm.response.responseTime).to.be.below(1000);
});

pm.test("Check data format", () => {
    const jsonData = pm.response.json();
    pm.expect(jsonData.success).to.be.true;
    pm.expect(jsonData.data).to.have.property('id');
    pm.expect(jsonData.data.status).to.equal('Pending');
    
    // Lưu ID booking để test API Get/Update ở các request sau
    pm.environment.set("last_booking_id", jsonData.data.id);
});
```

---

## 🎯 QA-07: Database Testing with SQL

Để đảm bảo dữ liệu lưu trữ chính xác trong PostgreSQL DB, QA cần thực hiện các truy vấn xác minh.

### 1. Truy Vấn Kiểm Tra Tính Toàn Vẹn Dữ Liệu (Data Integrity)
```sql
-- Check 1: Tìm kiếm các lịch đặt bị mồ côi (Orphan) không có Pet
SELECT "Id", "PetId" FROM "Bookings" b
WHERE NOT EXISTS (SELECT 1 FROM "Pets" p WHERE p."Id" = b."PetId");
-- Kết quả mong đợi: 0 rows

-- Check 2: Kiểm tra tồn kho dược phẩm không được âm
SELECT "Id", "Name", "StockQuantity" FROM "Medicines"
WHERE "StockQuantity" < 0;
-- Kết quả mong đợi: 0 rows

-- Check 3: Phát hiện thú cưng bị trùng lịch khám trong cùng khung giờ
SELECT "PetId", "AppointmentDate", COUNT(*) 
FROM "Bookings" 
WHERE "Status" != 'Cancelled'
GROUP BY "PetId", "AppointmentDate"
HAVING COUNT(*) > 1;
-- Kết quả mong đợi: 0 rows
```

### 2. Dọn Dẹp Dữ Liệu Rác Sau Khi Test (Cleanup Script)
```sql
-- Reset lại kho thuốc sau khi test kê đơn
UPDATE "Medicines" SET "StockQuantity" = 100 WHERE "Id" = 'test-medicine-id';

-- Xóa các hóa đơn và lịch hẹn của User Test
DELETE FROM "Invoices" WHERE "BookingId" IN (SELECT "Id" FROM "Bookings" WHERE "PetId" IN (SELECT "Id" FROM "Pets" WHERE "OwnerId" = 'test-user-id'));
DELETE FROM "Bookings" WHERE "PetId" IN (SELECT "Id" FROM "Pets" WHERE "OwnerId" = 'test-user-id');
DELETE FROM "Pets" WHERE "OwnerId" = 'test-user-id';
```

---

## 🎯 QA-08: UI/UX Testing & Responsive Testing

### 1. Ma Trận Thiết Bị Phổ Biến (Responsive Matrix)
QA thực hiện test giao diện trên các kích thước màn hình tương ứng:
* **Mobile (375px - 430px):** iPhone SE, iPhone 12/13, Samsung Galaxy S23. 
  - *Yêu cầu:* Menu điều hướng rút gọn thành nút Hamburger, bảng (Table) chuyển sang dạng thẻ (Card list), touch targets tối thiểu $44 \times 44$ pixels.
* **Tablet (768px - 1024px):** iPad Mini, iPad Air.
* **Desktop (1366px - 1920px):** Màn hình máy tính thông thường.
  - *Yêu cầu:* Sidebar hiển thị đầy đủ, bảng hiển thị tất cả các cột thông tin mà không bị che khuất.

### 2. Danh Sách Kiểm Tra Tính Nhất Quán Giao Diện (UI Consistency Checklist)
* **Typography:** Kích thước font chữ nhất quán (H1: 32px, Body: 16px, Caption: 14px), font Poppins/Inter hiển thị đúng.
* **Colors:** Màu sắc tuân thủ đúng bảng màu thương hiệu Premium Gold (màu vàng kim ánh gold làm điểm nhấn trên nền tối hoặc sáng sang trọng).
* **Interactive States:** Buttons có đầy đủ hiệu ứng Hover, Active, Focus Ring, và chuyển màu xám khi Disabled.
