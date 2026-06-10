# 🔁 UX Flow - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **FE-F01 (Navigation Flow):** Quản lý điều hướng người dùng giữa bảng điều khiển quản trị (Admin Dashboard) và trang chi tiết báo cáo.

---

## 1. Biểu đồ luồng trải nghiệm người dùng (UX Diagram)

Dưới đây là sơ đồ Mermaid thể hiện cách một Admin tương tác với trang báo cáo doanh thu:

```mermaid
graph TD
    A[Admin đăng nhập thành công] --> B[Mở thanh Sidebar menu]
    B --> C[Click 'Báo cáo doanh thu']
    C --> D{Kiểm tra quyền Admin?}
    
    D -- Không có quyền --> E[Hiển thị trang lỗi 403 Forbidden]
    D -- Có quyền hợp lệ --> F[Truy cập trang Revenue Reports]
    
    F --> G[Tự động lấy khoảng ngày mặc định: 30 ngày qua]
    G --> H[Gọi API GET /api/admin/reports/revenue]
    H --> I[Hiển thị KPI Cards, Line Chart, Donut Chart, Top 5 Services]
    
    I --> J[Admin muốn thay đổi khoảng ngày lọc]
    J --> K[Chọn ngày bắt đầu & ngày kết thúc mới]
    K --> L[Click nút 'Áp dụng']
    L --> M{Ngày hợp lệ? <br>StartDate <= EndDate}
    
    M -- Không hợp lệ --> N[Hiển thị thông báo Toast lỗi validation]
    M -- Hợp lệ --> O[Cập nhật Pinia store state]
    O --> H
```
