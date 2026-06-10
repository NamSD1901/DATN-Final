# 📄 User & Dev Documentation - Profile Avatar Upload

Tài liệu cung cấp hướng dẫn vận hành chi tiết dành cho người dùng và tài liệu tích hợp kỹ thuật dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng dành cho Người dùng (End-User Guide)

### Bước 1: Mở trình tải ảnh đại diện
1. Truy cập vào trang **"Hồ sơ cá nhân"** của bạn sau khi đăng nhập thành công.
2. Di chuyển con trỏ chuột (hoặc chạm tay trên màn hình điện thoại) vào biểu tượng ảnh đại diện hình tròn hiện tại.
3. Nhấp chọn biểu tượng máy ảnh hiển thị đè lên để mở hộp thoại tải ảnh.

### Bước 2: Chọn hình ảnh của bạn
Bạn có hai phương thức để đưa ảnh vào biểu mẫu:
*   **Cách 1 (Kéo thả):** Kéo tệp ảnh từ thư mục máy tính của bạn và thả trực tiếp vào khung nét đứt trên màn hình.
*   **Cách 2 (Chọn tệp truyền thống):** Click chuột vào bất cứ đâu bên trong khung nét đứt để mở cửa sổ duyệt tệp tin của hệ điều hành, chọn ảnh mong muốn và nhấn **Open**.

### Bước 3: Xem trước và tải lên
1. Một hình ảnh xem trước dạng tròn sẽ xuất hiện trên giao diện. Hãy đảm bảo khuôn mặt hoặc hình ảnh thú cưng của bạn nằm cân đối ở giữa.
2. Nếu muốn đổi ảnh khác, nhấp nút **"Hủy bỏ"** để chọn lại.
3. Nếu đã hài lòng, nhấp nút **"Tải Lên Máy Chủ"**. Giao diện sẽ hiển thị phần trăm tiến độ tải lên. Khi thanh tiến trình đạt 100%, hệ thống tự động cập nhật ảnh đại diện của bạn trên thanh menu và đóng cửa sổ.

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc thư mục liên quan trong dự án
*   **Backend Web API:**
    *   [ProfileController.cs](file:///e:/DATN/MyPetClinic/backend/src/WebApi/Controllers/ProfileController.cs#L82-L126) — Chứa API POST `/api/profile/avatar` tiếp nhận tệp nhị phân, kiểm tra dung lượng và ghi đĩa cứng.
    *   [UserService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/UserService.cs#L81-L92) — Chứa hàm cập nhật đường dẫn `AvatarUrl` vào bảng cơ sở dữ liệu.
*   **Frontend SPA:**
    *   `frontend/src/stores/avatar.ts` — Quản lý trạng thái tải lên, thanh tiến trình % và tích hợp API.
    *   `frontend/src/components/dashboard/AvatarUploadModal.vue` — Component chứa giao diện Drag & Drop vùng kéo thả và Preview.

### B. Kiểm thử nhanh API bằng công cụ Curl
Bạn có thể sử dụng công cụ dòng lệnh `curl` để kiểm tra hoạt động của API tải ảnh (yêu cầu gửi kèm Token JWT và tệp ảnh thực tế):

```bash
curl -X POST "https://localhost:5001/api/profile/avatar" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_TẠI_ĐÂY>" \
     -H "Content-Type: multipart/form-data" \
     -F "avatarFile=@/path/to/your/image.png"
```
*(Thay thế `/path/to/your/image.png` bằng đường dẫn tuyệt đối đến tệp ảnh trên máy tính của bạn).*

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### Sự cố 1: Lỗi `400 Bad Request` - "Kích thước ảnh không được vượt quá 2MB"
*   **Nguyên nhân:** Người dùng tải lên ảnh chụp trực tiếp từ máy ảnh điện thoại chất lượng cao (thường từ 3MB - 8MB).
*   **Giải pháp:**
    *   *Phía User:* Sử dụng công cụ nén ảnh trực tuyến hoặc resize ảnh xuống dưới 2MB trước khi upload.
    *   *Phía Dev:* Ở Phase tiếp theo, tích hợp thư viện nén ảnh Client-side (ví dụ: `browser-image-compression`) để tự động giảm kích thước và nén ảnh về định dạng `.jpeg` chất lượng 80% ngay tại trình duyệt trước khi gửi file lên server, triệt tiêu hoàn toàn lỗi này.

### Sự cố 2: Ảnh cập nhật thành công nhưng UI Navbar vẫn hiển thị ảnh cũ
*   **Nguyên nhân:** Trình duyệt web tự động lưu cache hình ảnh dựa trên đường dẫn URL tĩnh (Image Caching).
*   **Giải pháp khắc phục ở Frontend:** Khi hiển thị thẻ `<img>` chứa ảnh đại diện, bổ sung một tham số query ngẫu nhiên hoặc timestamp vào sau đuôi ảnh để ép trình duyệt tải lại ảnh mới thay vì lấy từ bộ nhớ đệm:
    ```html
    <img :src="profileStore.profile.avatar + '?t=' + Date.now()" alt="Avatar" />
    ```
