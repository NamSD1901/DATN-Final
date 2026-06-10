# ⚙️ Behavior Specification - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **FE-F01 (Form/Input Validation):** Kiểm tra tính hợp lệ của input ngày trên giao diện trước khi gửi request lên Server.
- **FE-F02 (Dynamic UI State):** Quản lý các trạng thái hiển thị của biểu đồ (Loading skeleton, Empty chart, Error fallback).

---

## 1. Trạng thái giao diện và các điều kiện kích hoạt (UI State Machine)

### 1.1. Trạng thái Loading (Đang tải dữ liệu)
- **Hành vi:** Khi bắt đầu gọi API (`isLoading = true`), hiển thị hiệu ứng Skeleton Loader cho các thẻ KPI Cards và hiệu ứng xoay (spinner) phủ mờ trên vùng hiển thị biểu đồ.
- **Mục tiêu:** Ngăn chặn người dùng tương tác liên tục vào nút "Áp dụng" trong khi request cũ đang chạy.

### 1.2. Trạng thái Trống (Empty State - Không có dữ liệu)
- **Hành vi:** Nếu API trả về mảng `dailyChart` rỗng (không có doanh thu phát sinh trong kỳ):
  - Biểu đồ Line Chart hiển thị một đường thẳng nằm ngang ở mức `0` kèm nhãn thông báo mờ ở giữa: "Không có dữ liệu doanh thu trong khoảng thời gian này".
  - Biểu đồ Donut hiển thị hình tròn màu xám nhạt duy nhất đại diện cho 100% rỗng.
  - KPI Cards hiển thị số `0 đ` hoặc `0` cuộc hẹn.

### 1.3. Trạng thái Lỗi (Error State)
- **Hành vi:** Khi API trả về lỗi (400, 401, 500), ẩn vùng biểu đồ và hiển thị Banner lỗi màu đỏ (`--alert-critical`) kèm nút "Tải lại" (Retry) để kích hoạt hàm `fetchDashboard()`.

---

## 2. Ràng buộc Validation Phía Frontend (Client-side Validation)

### 2.1. Ràng buộc Khoảng ngày
- **Quy tắc 1:** Ngày bắt đầu (`startDate`) không được lớn hơn ngày kết thúc (`endDate`).
- **Quy tắc 2 (Tối ưu hóa hiệu năng):** Giới hạn tối đa khoảng lọc ngày là **365 ngày (1 năm)**. Nếu vượt quá, vô hiệu hóa nút "Áp dụng" và hiển thị Tooltip/Toast nhắc nhở: "Khoảng thời gian báo cáo tối đa là 1 năm để đảm bảo tốc độ phản hồi".
- **Hành động khi vi phạm:** Giao diện hiển thị viền đỏ quanh ô chọn ngày vi phạm và hiển thị text cảnh báo dưới ô input.
