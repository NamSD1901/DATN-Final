# 📅 Implementation Plan - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **BE-F02 (Development Plan):** Lên kế hoạch chi tiết tích hợp dịch vụ báo cáo từ việc tối ưu database, phân quyền endpoint đến xây dựng UI hiển thị biểu đồ.

---

## 1. Kế hoạch Phát triển & Tích hợp (Sprint 6 Plan)

Kéo dài trong **4 ngày làm việc**:
- **Ngày 1:**
  - Viết database migration bổ sung index cho bảng `Invoices` và `Appointments`.
  - Cài đặt `RevenueReportService` và `AdminReportsController`.
- **Ngày 2:**
  - Viết unit test cho Service kiểm tra logic tính toán doanh thu và tỷ lệ tăng trưởng.
  - Tích hợp bảo mật API bằng phân quyền `[Authorize(Roles = "admin")]`.
- **Ngày 3:**
  - Cài đặt thư viện `chart.js` và `vue-chartjs`.
  - Xây dựng Pinia store `useRevenueStore` kết nối API.
- **Ngày 4:**
  - Xây dựng layout dashboard giao diện, các KPI cards và vẽ biểu đồ.
  - Kiểm thử đầu cuối (End-to-End Testing) và tối ưu hóa phản hồi.

---

## 2. Kế hoạch Kiểm thử (QA Test Cases)

### Test Case 1: Tính toán doanh thu chính xác
- **Mục tiêu:** Đảm bảo doanh thu hiển thị khớp chính xác với dữ liệu trong database.
- **Các bước:**
  1. Tạo sẵn 3 hóa đơn với trạng thái `Paid` (tổng tiền: 5,000,000đ) và 1 hóa đơn trạng thái `Pending` (3,000,000đ) trong khoảng ngày lọc.
  2. Gửi request lấy báo cáo cho khoảng ngày trên.
- **Kết quả mong đợi:** Tổng doanh thu trả về là đúng `5,000,000đ`. Hóa đơn `Pending` không được cộng vào doanh thu.

### Test Case 2: Kiểm soát truy cập RBAC (Security Validation)
- **Mục tiêu:** Ngăn chặn các tài khoản không có quyền Admin truy cập dữ liệu tài chính.
- **Các bước:**
  1. Sử dụng tài khoản khách hàng hoặc bác sĩ gọi API `GET /api/admin/reports/revenue`.
- **Kết quả mong đợi:** Server trả về mã lỗi `403 Forbidden` và không lộ thông tin tài chính nào.

### Test Case 3: Xác thực khoảng ngày lọc (Validation Range)
- **Mục tiêu:** Đảm bảo hệ thống không bị crash hoặc treo khi tham số ngày không hợp lệ.
- **Các bước:**
  1. Chọn ngày bắt đầu là `2026-06-10` và ngày kết thúc là `2026-06-01` (ngày bắt đầu lớn hơn ngày kết thúc) rồi nhấn áp dụng.
- **Kết quả mong đợi:** Hệ thống hiển thị cảnh báo lỗi validation phía Client và không gửi request lên Server. Nếu cố tình bypass gửi API trực tiếp, Server phản hồi lỗi `400 Bad Request`.
