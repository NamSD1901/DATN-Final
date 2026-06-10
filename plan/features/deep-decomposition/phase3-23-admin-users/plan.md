# 📝 Implementation Plan & Testing Strategy - Admin Staff Management

## 1. Kế hoạch Triển khai (Sprint 6)

| Giai đoạn | Task | Skills áp dụng | Est. |
|---|---|---|---|
| 1 | Cấu hình tham số Identity Options trong Program.cs bảo mật | BE-C01 (Config) | 1h |
| 2 | Code lớp dịch vụ quản lý tài khoản nhân viên `AdminStaffService` | BE-F01, BE-F03 | 2h |
| 3 | Tạo các API Controller quản trị nhân sự gắn phân quyền RBAC | BE-A03, BE-F03 | 2h |
| 4 | Xây dựng màn hình danh sách nhân viên kèm các modal tạo mới/sửa đổi | FE-F01, FE-C03 | 4h |
| 5 | Tích hợp kiểm thử tự động (Unit/Integration Tests) cho logic Lock/Unlock | BE-T01 (Testing) | 2h |

---

## 2. QA Test Suite (Kiểm thử chức năng & Bảo mật nhân sự)

### Case 1: Thêm mới nhân viên thành công
- **Các bước:** Đăng nhập tài khoản Admin ➡️ Mở trang nhân sự ➡️ Chọn "Thêm nhân viên" ➡️ Nhập đầy đủ thông tin hợp lệ (chọn Role = `doctor`) ➡️ Nhấn Lưu.
- **Kết quả mong muốn:** API trả về 200 OK. Kiểm tra trong DB bảng `AspNetUsers` có bản ghi mới, đồng thời bảng liên kết Role gắn chính xác quyền `doctor`.

### Case 2: Chặn tự khóa chính mình
- **Các bước:** Đăng nhập Admin A ➡️ Click nút "Khóa" chính tài khoản Admin A đang hoạt động.
- **Kết quả mong muốn:** Hệ thống chặn ngay từ frontend (nút khóa bị ẩn/vô hiệu hóa). Nếu gọi API thủ công bằng công cụ ngoài, API trả về 400 Bad Request kèm thông báo lỗi "Bạn không thể tự khóa tài khoản của chính mình."

### Case 3: Chặn tài khoản không có quyền Admin chỉnh sửa nhân viên
- **Các bước:** Đăng nhập tài khoản Bác sĩ hoặc Lễ tân ➡️ Gửi request POST tới `/api/admin/users/create`.
- **Kết quả mong muốn:** API từ chối xử lý, trả về HTTP 403 Forbidden.
