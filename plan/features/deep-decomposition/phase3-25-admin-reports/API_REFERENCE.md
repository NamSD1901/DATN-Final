# 🔌 API Reference - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **BE-F02 (REST API Principles):** Thiết kế API Endpoint chuẩn RESTful: sử dụng đúng HTTP method (`GET`), phân định rõ Query Parameter và kiểu dữ liệu chuẩn ISO 8601.

---

## 1. Lấy dữ liệu báo cáo doanh thu (Revenue Dashboard Data)

Tải dữ liệu tổng hợp về doanh thu, số cuộc hẹn, biểu đồ doanh thu theo ngày và danh sách các dịch vụ bán chạy nhất.

- **Endpoint:** `GET /api/admin/reports/revenue`
- **Authentication:** Required (Bearer Token)
- **Role Allowed:** `admin`

### 1.1. Query Parameters
| Tham số | Kiểu dữ liệu | Bắt buộc | Mô tả | Ví dụ |
| :--- | :--- | :--- | :--- | :--- |
| `startDate` | `string (date)` | Có | Ngày bắt đầu lọc (định dạng YYYY-MM-DD) | `2026-05-01` |
| `endDate` | `string (date)` | Có | Ngày kết thúc lọc (định dạng YYYY-MM-DD) | `2026-05-31` |

---

### 1.2. Responses

#### 🟢 200 OK
Trả về thông tin phân tích tài chính chi tiết trong khoảng ngày đã chọn.

```json
{
  "totalRevenue": 150230000.00,
  "growthRate": 12.50,
  "totalAppointments": 420,
  "serviceRevenue": 110230000.00,
  "medicineRevenue": 40000000.00,
  "dailyChart": [
    {
      "date": "2026-05-01",
      "revenue": 5200000.00
    },
    {
      "date": "2026-05-02",
      "revenue": 4800000.00
    }
  ],
  "topServices": [
    {
      "name": "Tiêm vaccine ngừa dại",
      "count": 120,
      "value": 24000000.00
    },
    {
      "name": "Khám sức khỏe tổng quát chó/mèo",
      "count": 95,
      "value": 19000000.00
    }
  ]
}
```

#### 🔴 400 Bad Request
Khi khoảng ngày không hợp lệ (Ví dụ: `startDate` sau `endDate`).
```json
{
  "message": "Ngày bắt đầu không được lớn hơn ngày kết thúc."
}
```

#### 🔴 401 Unauthorized
Khi Token không hợp lệ hoặc đã hết hạn.

#### 🔴 403 Forbidden
Khi tài khoản đăng nhập không có Role `admin` (ví dụ: `customer`, `doctor`, `receptionist`).
