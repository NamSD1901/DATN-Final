# 📖 Operator & Developer Documentation - Admin Staff Management

Tài liệu hướng dẫn vận hành (dành cho Admin) và tài liệu tích hợp/debug kỹ thuật (dành cho Lập trình viên).

---

## 1. Hướng dẫn Vận hành dành cho Quản trị viên (Operator Guide)

### Cách tạo và thiết lập tài khoản cho Nhân viên mới
1. Đăng nhập vào hệ thống MyPetClinic bằng tài khoản Quản trị viên (`Admin`).
2. Nhấp chọn tab **Quản trị Nhân sự** trên thanh Menu chính.
3. Bấm nút **[+ Thêm nhân viên]** để mở form nhập liệu.
4. Điền đầy đủ thông tin:
   - *Họ và tên:* Tên đầy đủ có dấu (ví dụ: *Nguyễn Văn Đức*).
   - *Email:* Nhập chính xác email cá nhân hoặc email công ty cấp cho nhân viên (đây là email dùng để đăng nhập và nhận mật khẩu).
   - *Số điện thoại:* Định dạng gồm 10 chữ số.
5. Chọn vai trò phù hợp:
   - **Bác sĩ thú y (doctor):** Quyền xem hàng đợi khám, ghi chép bệnh án và kê đơn thuốc.
   - **Lễ tân (receptionist):** Quyền check-in khách hàng, xếp hàng đợi và lập hóa đơn.
   - **Thu ngân (cashier):** Quyền thu tiền hóa đơn và in biên lai nhiệt.
6. Bấm **[Khởi tạo tài khoản]**. Hệ thống sẽ tự động băm mật khẩu ngẫu nhiên và gửi thông báo về email nhân viên.

### Hướng dẫn Khóa tài khoản nhân viên thôi việc
1. Tại bảng danh sách nhân viên, tìm tài khoản nhân viên cần xử lý.
2. Click nút **[Khóa]** màu đỏ ở cột hành động.
3. Hộp thoại cảnh báo xuất hiện, kiểm tra kỹ tên nhân viên và bấm **[Xác nhận khóa]**.
4. Trạng thái nhân viên lập tức chuyển sang màu đỏ `Suspended` và phiên làm việc hiện tại của nhân viên đó bị ngắt kết nối ngay lập tức.

---

## 2. Hướng dẫn Kỹ thuật dành cho Developer (Developer Guide)

### Cơ chế Router Guard phân quyền trên Frontend Vue 3
Để bảo vệ các trang Dashboard quản trị không bị truy cập bất hợp pháp từ Client, cấu hình Router Guard trong Vue Router:

```typescript
// router/index.ts
import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/stores/useAuthStore';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/admin/staff',
      component: () => import('@/views/admin/StaffManagement.vue'),
      meta: { requiresAuth: true, roles: ['admin'] } // Chỉ cho phép admin
    }
  ]
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  const isAuthenticated = authStore.isAuthenticated;
  const userRole = authStore.userRole; // Lấy ra từ JWT Claim

  if (to.meta.requiresAuth && !isAuthenticated) {
    return next({ path: '/auth/login' });
  }

  if (to.meta.roles && !to.meta.roles.includes(userRole)) {
    // Trả về trang 403 Forbidden nếu không đủ quyền hạn
    return next({ path: '/403' });
  }

  next();
});

export default router;
```

### Các lệnh cURL Kiểm thử API Thủ công (API Debugging)

#### 1. Tạo tài khoản nhân viên mới (Gọi bởi Admin)
```bash
curl -X POST "https://localhost:5001/api/admin/staff" \
     -H "Authorization: Bearer <ADMIN_JWT_TOKEN>" \
     -H "Content-Type: application/json" \
     -d "{\"fullName\": \"Nguyễn Văn Đức\", \"email\": \"duc.nv@mypet.vn\", \"phoneNumber\": \"0912345678\", \"role\": \"doctor\"}"
```

#### 2. Thay đổi vai trò nhân viên
```bash
curl -X PUT "https://localhost:5001/api/admin/staff/a993e54b-d72b-42fa-97ab-713217b1897d/role" \
     -H "Authorization: Bearer <ADMIN_JWT_TOKEN>" \
     -H "Content-Type: application/json" \
     -d "{\"newRole\": \"admin\"}"
```

---

## 3. Khắc phục Sự cố Thường gặp (Troubleshooting)

### Sự cố: Khóa nhầm tài khoản Admin gốc duy nhất của hệ thống
- **Triệu chứng:** Không còn tài khoản nào có vai trò `admin` hoạt động để mở khóa hoặc quản trị nhân sự.
- **Giải pháp khắc phục khẩn cấp (Chỉ thực hiện trực tiếp trên Database Server):**
  1. Kết nối vào máy chủ PostgreSQL thông qua công cụ pgAdmin hoặc terminal:
     ```bash
     psql -h localhost -U postgres -d MyPetClinicDb
     ```
  2. Chạy câu lệnh SQL cập nhật cưỡng chế trạng thái và vai trò của tài khoản Admin gốc:
     ```sql
     UPDATE Users 
     SET Status = 'Active', Role = 'admin' 
     WHERE Email = 'admin.root@mypetclinic.vn';
     ```
  3. Kiểm tra lại trạng thái để xác nhận tài khoản đã được phục hồi:
     ```sql
     SELECT Email, Role, Status FROM Users WHERE Email = 'admin.root@mypetclinic.vn';
     ```
