# 📦 Nhật Ký Quản Lý Thư Viện & Phụ Thuộc - Dependency Log

## Backend

### MyPetClinic.Application
- `Microsoft.EntityFrameworkCore` v10.0.0: Thêm vào để sử dụng `IQueryable` async extensions (`ToListAsync`, `SumAsync`, `CountAsync`) trong các Application Service (`PrescriptionService`, `ReportService`). Đây là pattern phổ biến trong Clean Architecture — Application layer không phụ thuộc EF Core provider (Npgsql/SqlServer), chỉ dùng core abstractions của EF. *(Thêm: ADR-08)*

## Frontend
- `@microsoft/signalr`: Thư viện kết nối WebSocket/SignalR để nhận thông báo realtime từ backend.
- `pinia`: Quản lý state cho Vue 3, được dùng để lưu trữ danh sách thông báo và trạng thái kết nối SignalR.
- `sweetalert2`: Hiển thị toast message khi có thông báo mới (thay thế cho vue-toastification).
