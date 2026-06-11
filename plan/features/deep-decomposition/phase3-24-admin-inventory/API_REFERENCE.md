# 📄 API Reference - Admin Drug Inventory

Tài liệu đặc tả chi tiết các cổng kết nối (RESTful API Contracts) dành cho phân hệ Quản lý Kho thuốc & Vật tư.

---

## 1. Danh sách các API Endpoints hỗ trợ

| Phương thức | Đường dẫn API | Phân quyền | Mô tả |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/inventory/medicines` | `doctor`, `admin` | Lấy danh mục toàn bộ biệt dược kèm số lượng tồn và lô. |
| **POST** | `/api/admin/inventory/batches` | `admin` | Nhập một lô thuốc mới vào kho. |
| **GET** | `/api/admin/inventory/transactions` | `admin` | Truy xuất lịch sử giao dịch nhập/xuất kho. |
| **GET** | `/api/admin/inventory/alerts/low-stock` | `admin` | Lấy danh sách thuốc sắp hết hàng (dưới định mức). |

---

## 2. Đặc tả Chi tiết từng Endpoint

### 1. Nhập một lô thuốc mới vào kho
- **Endpoint:** `POST /api/admin/inventory/batches`
- **Headers:** `Authorization: Bearer <ADMIN_JWT_TOKEN>`

#### Request Body Schema (Application/JSON)
```json
{
  "medicineId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "batchNumber": "LOT-202606-02",
  "quantity": 100,
  "manufacturingDate": "2026-05-01",
  "expiryDate": "2028-05-01"
}
```

#### Response (201 Created)
```json
{
  "id": "b883e54b-d72b-42fa-97ab-713217b1897d",
  "batchNumber": "LOT-202606-02",
  "currentQuantity": 100,
  "expiryDate": "2028-05-01T00:00:00.000Z",
  "batchStatus": "InStock"
}
```

#### Response (400 Bad Request) - Sai hạn sử dụng (Hạn dùng ở quá khứ)
```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": {
    "ExpiryDate": ["Hạn sử dụng của lô thuốc mới nhập phải là ngày trong tương lai."]
  }
}
```

---

### 2. Lấy danh mục biệt dược kèm số lượng tồn và lô hàng
- **Endpoint:** `GET /api/admin/inventory/medicines`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Response (200 OK)
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Amoxicillin 500mg",
    "activeIngredient": "Amoxicillin",
    "price": 15000.00,
    "minStockThreshold": 100,
    "currentStock": 450,
    "stockStatus": "InStock",
    "batches": [
      {
        "id": "b883e54b-d72b-42fa-97ab-713217b1897d",
        "batchNumber": "LOT-202606-02",
        "currentQuantity": 100,
        "expiryDate": "2028-05-01T00:00:00.000Z",
        "batchStatus": "InStock"
      },
      {
        "id": "c994f65c-e83c-53fb-08bc-824328c2908e",
        "batchNumber": "LOT-202601-01",
        "currentQuantity": 350,
        "expiryDate": "2027-01-15T00:00:00.000Z",
        "batchStatus": "InStock"
      }
    ]
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "name": "Dexafort 50ml",
    "activeIngredient": "Dexamethasone",
    "price": 150000.00,
    "minStockThreshold": 15,
    "currentStock": 8,
    "stockStatus": "LowStock",
    "batches": [
      {
        "id": "d005g76d-f94d-64fc-19cd-935439d3019f",
        "batchNumber": "LOT-2605-A",
        "currentQuantity": 8,
        "expiryDate": "2026-09-30T00:00:00.000Z",
        "batchStatus": "InStock"
      }
    ]
  }
]
```

---

### 3. Lấy danh sách giao dịch kho gần nhất (Audit Trail)
- **Endpoint:** `GET /api/admin/inventory/transactions`
- **Headers:** `Authorization: Bearer <ADMIN_JWT_TOKEN>`

#### Response (200 OK)
```json
[
  {
    "id": "e007h87e-a05e-75fd-20de-046540e4120a",
    "medicineName": "Amoxicillin 500mg",
    "batchNumber": "LOT-202606-02",
    "transactionType": "IMPORT",
    "quantity": 100,
    "actorName": "Nguyễn Văn Khánh",
    "timestamp": "2026-06-11T09:30:00.000Z"
  },
  {
    "id": "f118i98f-b16f-86fe-31ef-157651f5231b",
    "medicineName": "Dexafort 50ml",
    "batchNumber": "LOT-2605-A",
    "transactionType": "EXPORT",
    "quantity": 2,
    "actorName": "Bác sĩ Đỗ Quốc Huy",
    "timestamp": "2026-06-11T09:15:22.000Z"
  }
]
```
