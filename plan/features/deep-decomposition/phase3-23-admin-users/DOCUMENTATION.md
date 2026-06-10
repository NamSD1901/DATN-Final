# 📄 User & Dev Documentation - Admin Staff Management

## 1. Cơ chế Hoạt động của Lockout trong ASP.NET Core Identity

Khi Admin gọi API khóa người dùng, trường `LockoutEnd` (kiểu dữ liệu `timestamp with time zone`) trong bảng `AspNetUsers` sẽ được gán giá trị thời gian kết thúc tương lai xa. Khi đó:
- Cơ chế của Identity Framework khi xác thực đăng nhập sẽ tự động từ chối yêu cầu và trả về kết quả `SignInResult.IsLockedOut`.
- Khi mở khóa, trường `LockoutEnd` được cập nhật về `NULL` (hoặc thời gian trong quá khứ).

---

## 2. Dev Guidelines khi cập nhật Role
Mỗi người dùng trong hệ thống chỉ được sở hữu duy nhất 1 vai trò tại một thời điểm để đơn giản hóa quá trình phân quyền (Single Role Constraint). Khi cập nhật Role:
1. Xóa toàn bộ Role cũ của User bằng cách gọi `await _userManager.RemoveFromRolesAsync(user, currentRoles)`.
2. Thêm Role mới bằng cách gọi `await _userManager.AddToRoleAsync(user, newRole)`.
