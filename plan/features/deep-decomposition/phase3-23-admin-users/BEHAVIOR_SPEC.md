# 🎭 Behavioral Specification - Admin Staff Management

## 1. Biểu đồ Trạng thái Vue Component User Table

```mermaid
stateDiagram-v2
    [*] --> LoadingStaff : Component Mounted
    LoadingStaff --> RenderTable : Tải thành công danh sách
    RenderTable --> SearchFiltering : Người dùng gõ tên tìm kiếm
    SearchFiltering --> RenderTable
    RenderTable --> ConfirmingLock : Click Khóa tài khoản
    ConfirmingLock --> LockedState : Xác nhận và gửi API thành công
    LockedState --> RenderTable : Đổi trạng thái Badge thành đỏ
```

---

## 2. Ràng buộc Hành vi & Quy tắc Giao diện
- **Delete Prevention for Admin:** Nút "Xóa" hoặc "Khóa" phải bị ẩn/disabled hoàn toàn đối với chính tài khoản của Admin đang đăng nhập hiện tại để tránh việc tự khóa mình ra ngoài.
- **Form Validation:** Form thêm nhân viên bắt buộc kiểm tra định dạng Email chuẩn và Số điện thoại chỉ chứa chữ số (từ 10 đến 11 số).
