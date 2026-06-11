# 📄 API Reference - Cashier & Invoicing

Tài liệu đặc tả chi tiết các cổng kết nối (RESTful API Contracts) dành cho phân hệ Thu ngân & Lập hóa đơn phòng khám.

---

## 1. Danh sách các API Endpoints hỗ trợ

| Phương thức | Đường dẫn API | Phân quyền | Mô tả |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/receptionist/invoices/pending` | `receptionist`, `cashier`, `admin` | Lấy danh sách hóa đơn trạng thái chờ (`Pending`). |
| **POST** | `/api/receptionist/invoices` | `receptionist`, `cashier`, `admin` | Lập hóa đơn mới từ mã ca khám lịch hẹn. |
| **GET** | `/api/receptionist/invoices/{id}` | `receptionist`, `cashier`, `admin` | Xem chi tiết hóa đơn (bao gồm danh mục vật tư/thuốc). |
| **GET** | `/api/receptionist/invoices/{id}/qr` | `receptionist`, `cashier`, `admin` | Sinh và lấy mã QR Code VietQR động để thanh toán. |
| **PUT** | `/api/receptionist/invoices/{id}/pay` | `receptionist`, `cashier`, `admin` | Xác nhận hóa đơn đã thanh toán thành công. |
| **PUT** | `/api/receptionist/invoices/{id}/cancel`| `receptionist`, `cashier`, `admin` | Hủy hóa đơn lập sai. |

---

## 2. Đặc tả Chi tiết từng Endpoint

### 1. Lập hóa đơn mới từ mã ca khám lịch hẹn
- **Endpoint:** `POST /api/receptionist/invoices`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Request Body Schema (Application/JSON)
```json
{
  "appointmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Response (201 Created)
```json
{
  "id": "e883e54b-d72b-42fa-97ab-713217b1897d",
  "appointmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "invoiceNumber": "INV-20260611-0001",
  "totalAmount": 430000.00,
  "paymentMethod": null,
  "status": "Pending",
  "createdAt": "2026-06-11T09:30:00.000Z",
  "paidAt": null,
  "customerName": "Nguyễn Văn Nam",
  "petName": "Mèo LuLu",
  "items": [
    {
      "id": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
      "itemName": "Phí khám bệnh lâm sàng",
      "itemType": "Service",
      "quantity": 1,
      "unitPrice": 100000.00,
      "subTotal": 100000.00
    },
    {
      "id": "2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e",
      "itemName": "Thuốc: Amoxicillin 500mg (Kháng sinh)",
      "itemType": "Medicine",
      "quantity": 10,
      "unitPrice": 15000.00,
      "subTotal": 165000.00
    },
    {
      "id": "3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f",
      "itemName": "Thuốc: Dexafort kháng viêm x 1 lọ",
      "itemType": "Medicine",
      "quantity": 1,
      "unitPrice": 150000.00,
      "subTotal": 165000.00
    }
  ]
}
```

#### Response (422 Unprocessable Entity) - Ca khám chưa hoàn thành
```json
{
  "status": 422,
  "title": "Business Logic Error",
  "detail": "Chỉ có thể lập hóa đơn cho ca khám đã hoàn thành (Completed)."
}
```

---

### 2. Sinh và lấy mã QR Code VietQR động
- **Endpoint:** `GET /api/receptionist/invoices/{id}/qr`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Response (200 OK)
```json
{
  "qrCodeUrl": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAJYAAACWCAYAAAA...",
  "accountNo": "190288889999",
  "accountName": "PHONG KHAM MYPETCLINIC",
  "amount": 430000.00,
  "description": "MYPETCLINIC INVOICE INV-20260611-0001"
}
```

#### Response (404 Not Found)
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Không tìm thấy hóa đơn được yêu cầu."
}
```

---

### 3. Xác nhận hóa đơn đã thanh toán thành công
- **Endpoint:** `PUT /api/receptionist/invoices/{id}/pay`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Request Body Schema (Application/JSON)
```json
{
  "paymentMethod": "BankTransfer"
}
```

#### Response (200 OK)
```json
{
  "id": "e883e54b-d72b-42fa-97ab-713217b1897d",
  "invoiceNumber": "INV-20260611-0001",
  "totalAmount": 430000.00,
  "paymentMethod": "BankTransfer",
  "status": "Paid",
  "createdAt": "2026-06-11T09:30:00.000Z",
  "paidAt": "2026-06-11T09:40:15.120Z",
  "customerName": "Nguyễn Văn Nam",
  "petName": "Mèo LuLu"
}
```

#### Response (400 Bad Request) - Sai phương thức thanh toán hoặc hóa đơn đã thanh toán rồi
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Hóa đơn đã được thanh toán trước đó."
}
```

#### Response (403 Forbidden IDOR Blocked) - Khách hàng cố tình xác nhận thanh toán
```json
{
  "status": 403,
  "title": "Forbidden",
  "detail": "Tài khoản của bạn không có quyền thực thi hành động này."
}
```
