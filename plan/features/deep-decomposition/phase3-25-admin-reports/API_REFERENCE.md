# 📄 API Reference - Admin Revenue Reports

Tài liệu đặc tả chi tiết các cổng kết nối (RESTful API Contracts) dành cho phân hệ Báo cáo Doanh thu & Hiệu suất.

---

## 1. Danh sách các API Endpoints hỗ trợ

| Phương thức | Đường dẫn API | Phân quyền | Mô tả |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/reports/kpis` | `admin` | Lấy các chỉ số KPI tài chính tổng hợp nhanh. |
| **GET** | `/api/admin/reports/revenue-trend` | `admin` | Lấy dữ liệu doanh thu xu hướng theo ngày trong kỳ. |
| **GET** | `/api/admin/reports/revenue-structure` | `admin` | Lấy cơ cấu doanh thu theo loại dòng tiền. |
| **GET** | `/api/admin/reports/export` | `admin` | Xuất báo cáo tài chính ra file Excel. |

---

## 2. Đặc tả Chi tiết từng Endpoint

### 1. Lấy chỉ số KPI tài chính tổng hợp nhanh
- **Endpoint:** `GET /api/admin/reports/kpis`
- **Headers:** `Authorization: Bearer <ADMIN_JWT_TOKEN>`
- **Query Parameters:**
  - `startDate` (Kiểu Date, định dạng `yyyy-MM-dd`): Ngày bắt đầu lọc.
  - `endDate` (Kiểu Date, định dạng `yyyy-MM-dd`): Ngày kết thúc lọc.

#### Response (200 OK)
```json
{
  "totalRevenue": 275000000.00,
  "growthRate": 12.42,
  "totalAppointments": 420,
  "activeCustomersCount": 185,
  "averageOrderValue": 654761.90
}
```

#### Response (400 Bad Request) - Thiếu tham số ngày lọc hoặc ngày bắt đầu lớn hơn ngày kết thúc
```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Ngày bắt đầu lọc không được lớn hơn ngày kết thúc."
}
```

#### Response (403 Forbidden) - Bác sĩ cố truy cập doanh thu phòng khám
```json
{
  "status": 403,
  "title": "Forbidden Action",
  "detail": "Tài khoản của bạn không được phân quyền truy cập thông tin tài chính."
}
```

---

### 2. Lấy dữ liệu doanh thu xu hướng theo ngày (Line Chart Data Source)
- **Endpoint:** `GET /api/admin/reports/revenue-trend`
- **Headers:** `Authorization: Bearer <ADMIN_JWT_TOKEN>`
- **Query Parameters:** `startDate=2026-05-01&endDate=2026-05-07`

#### Response (200 OK)
```json
{
  "labels": [
    "2026-05-01",
    "2026-05-02",
    "2026-05-03",
    "2026-05-04",
    "2026-05-05",
    "2026-05-06",
    "2026-05-07"
  ],
  "dataPoints": [
    12500000.00,
    15000000.00,
    8500000.00,
    22000000.00,
    19500000.00,
    14000000.00,
    30500000.00
  ],
  "invoiceCounts": [
    18,
    22,
    11,
    30,
    27,
    19,
    45
  ]
}
```

---

### 3. Lấy cơ cấu doanh thu theo loại dòng tiền (Donut Chart Data Source)
- **Endpoint:** `GET /api/admin/reports/revenue-structure`
- **Headers:** `Authorization: Bearer <ADMIN_JWT_TOKEN>`
- **Query Parameters:** `startDate=2026-05-01&endDate=2026-05-31`

#### Response (200 OK)
```json
{
  "serviceRevenue": 150000000.00,
  "medicineRevenue": 85000000.00,
  "vaccineRevenue": 40000000.00
}
```
*Giao diện Vue 3 sẽ tự động map 3 trường này thành mảng `[150000000, 85000000, 40000000]` để cấp cho cấu hình dataset của Chart.js.*
