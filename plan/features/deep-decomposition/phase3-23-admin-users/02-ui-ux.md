# 🎨 UI/UX Design Specification - Admin Staff Management

Tài liệu thiết kế giao diện người dùng, sơ đồ bố cục ASCII Mockup, bảng chỉ màu HSL và các hiệu ứng phản hồi trạng thái tài khoản nhân viên.

---

## 1. Bản vẽ Bố cục giao diện (ASCII Art Mockups)

### Giao diện Quản trị Nhân sự (Admin Staff Directory Workspace)
Màn hình Dashboard tối giản mờ kính dành cho Admin quản lý danh sách toàn bộ nhân viên.

```text
+-------------------------------------------------------------------------------------------------------------------+
|  [Logo] MYPETCLINIC - CỔNG QUẢN TRỊ VIÊN (ADMIN DASHBOARD)                            [NV: Khánh] [Đăng xuất]     |
+-------------------------------------------------------------------------------------------------------------------+
|  [ Tìm kiếm nhân viên...]   ( Lọc vai trò: [Tất cả]  [Bác sĩ]  [Lễ tân]  [Thu ngân]  [Admin] )  [+ THÊM NHÂN VIÊN]   |
+-------------------------------------------------------------------------------------------------------------------+
|  DANH SÁCH NHÂN SỰ PHÒNG KHÁM (8 nhân viên)                                                                        |
|  +-------------------------------------------------------------------------------------------------------------+  |
|  | HỌ VÀ TÊN        | EMAIL                  | SĐT         | VAI TRÒ     | TRẠNG THÁI     | HÀNH ĐỘNG              |  |
|  +-------------------------------------------------------------------------------------------------------------+  |
|  | Nguyễn Văn Minh  | minh.nv@mypet.vn       | 0912345678  | [ Bác sĩ ]  | [Hoạt động]    | [Đổi quyền] [Khóa]     |  |
|  | Lê Thị Hoa       | hoa.lt@mypet.vn        | 0987654321  | [ Thu ngân] | [Hoạt động]    | [Đổi quyền] [Khóa]     |  |
|  | Trần Tuấn Anh    | anh.tt@mypet.vn        | 0909998887  | [ Lễ tân ]  | [Hoạt động]    | [Đổi quyền] [Khóa]     |  |
|  | Đỗ Quốc Huy      | huy.dq@mypet.vn        | 0933445566  | [ Bác sĩ ]  | [Đang khóa]    | [Đổi quyền] [Mở khóa]  |  |
|  | Phạm Hải Yến     | yen.ph@mypet.vn        | 0955667788  | [ Thu ngân] | [Chờ kích hoạt]| [Đổi quyền] [Khóa]     |  |
|  +-------------------------------------------------------------------------------------------------------------+  |
|                                                                                                                   |
+-------------------------------------------------------------------------------------------------------------------+
```

### Popup Thêm mới Nhân viên & Phân vai trò (Add Staff Modal)
```text
+---------------------------------------------------------+
|                  THÊM MỚI TÀI KHOẢN NHÂN VIÊN           |
+---------------------------------------------------------+
|                                                         |
|  Họ và tên:                                             |
|  +---------------------------------------------------+  |
|  | Nguyễn Văn A                                      |  |
|  +---------------------------------------------------+  |
|                                                         |
|  Địa chỉ Email:                                         |
|  +---------------------------------------------------+  |
|  | a.nv@mypet.vn                                     |  |
|  +---------------------------------------------------+  |
|                                                         |
|  Số điện thoại:                                         |
|  +---------------------------------------------------+  |
|  | 0901234567                                        |  |
|  +---------------------------------------------------+  |
|                                                         |
|  Chọn vai trò làm việc chuyên môn:                      |
|  ( ) Bác sĩ thú y (doctor)    (o) Lễ tân (receptionist) |
|  ( ) Thu ngân (cashier)       ( ) Quản trị (admin)      |
|                                                         |
|  * Mật khẩu tạm thời sẽ được tự sinh và gửi về Email.    |
|  +---------------------------------------------------+  |
|  |       [ HỦY BỎ ]          [ KHỞI TẠO TÀI KHOẢN ]      |
|  +---------------------------------------------------+  |
+---------------------------------------------------------+
```

---

## 2. Hệ thống CSS Design Tokens (HSL Color Theme)

Mã màu HSL giúp quản lý vai trò và trạng thái nhân viên trực quan:

```css
:root {
  /* Biến màu Vai trò (Roles) */
  --role-admin: hsl(280, 75%, 60%);          /* Tím quý phái */
  --role-admin-bg: hsla(280, 75%, 60%, 0.12);
  
  --role-doctor: hsl(200, 90%, 50%);         /* Xanh lam y khoa */
  --role-doctor-bg: hsla(200, 90%, 50%, 0.12);
  
  --role-receptionist: hsl(175, 70%, 45%);   /* Ngọc bích */
  --role-receptionist-bg: hsla(175, 70%, 45%, 0.12);
  
  --role-cashier: hsl(45, 95%, 50%);         /* Vàng tài chính */
  --role-cashier-bg: hsla(45, 95%, 50%, 0.12);

  /* Biến màu trạng thái hoạt động */
  --status-active: hsl(145, 65%, 45%);       /* Xanh lá - Active */
  --status-active-bg: hsla(145, 65%, 45%, 0.15);
  
  --status-suspended: hsl(355, 75%, 50%);    /* Đỏ - Suspended */
  --status-suspended-bg: hsla(355, 75%, 50%, 0.15);
  
  --status-pending: hsl(25, 95%, 55%);       /* Cam - Pending Activation */
  --status-pending-bg: hsla(25, 95%, 55%, 0.15);
  
  --backdrop-blur: blur(16px);
  --glass-card: rgba(30, 41, 59, 0.7);       /* Mờ kính tối */
}
```

---

## 3. Form Validation ở phía Client (Vue Form Validation)

Validation các thông tin nhân viên trước khi gửi lên API Backend để tạo tài khoản:

```typescript
// AdminStaffValidation.ts
export interface StaffCreationData {
  fullName: string;
  email: string;
  phoneNumber: string;
  role: 'admin' | 'doctor' | 'receptionist' | 'cashier';
}

export interface ValidationError {
  field: string;
  message: string;
}

export function validateCreateStaff(data: StaffCreationData): { isValid: boolean; errors: ValidationError[] } {
  const errors: ValidationError[] = [];

  // 1. Kiểm tra họ tên
  if (!data.fullName || data.fullName.trim().length < 3) {
    errors.push({ field: 'fullName', message: 'Họ tên nhân viên phải có ít nhất 3 ký tự.' });
  }

  // 2. Kiểm tra định dạng Email
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!data.email || !emailRegex.test(data.email)) {
    errors.push({ field: 'email', message: 'Địa chỉ Email không đúng định dạng.' });
  }

  // 3. Kiểm tra số điện thoại (chuẩn Việt Nam 10 số)
  const phoneRegex = /^(03|05|07|08|09)\d{8}$/;
  if (!data.phoneNumber || !phoneRegex.test(data.phoneNumber)) {
    errors.push({ field: 'phoneNumber', message: 'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng đầu số di động VN.' });
  }

  // 4. Kiểm tra vai trò
  const validRoles = ['admin', 'doctor', 'receptionist', 'cashier'];
  if (!data.role || !validRoles.includes(data.role)) {
    errors.push({ field: 'role', message: 'Vai trò nhân viên đã chọn không hợp lệ.' });
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}
```
