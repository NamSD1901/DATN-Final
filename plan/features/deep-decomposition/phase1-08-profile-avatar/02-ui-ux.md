# 🎨 UI/UX Design Spec - Profile Avatar Upload

## 1. Bố cục Giao diện & Trực quan hóa (Responsive Layout)

Giao diện tải lên ảnh đại diện được thiết kế tích hợp trực tiếp vào trang thông tin cá nhân hoặc dưới dạng một hộp thoại con (Modal) mờ kính Glassmorphism sang trọng.

### A. Sơ đồ ASCII Mockup - Trạng thái sẵn sàng (Idle & Dragover States)
```text
+-----------------------------------------------------------------------------+
|   UPDATE AVATAR (CẬP NHẬT ẢNH ĐẠI DIỆN)                                 [X] |
+-----------------------------------------------------------------------------+
|                                                                             |
|   +---------------------------------------------------------------------+   |
|   |  [DRAG & DROP ZONE - VÙNG KÉO THẢ]                                  |   |
|   |                                                                     |   |
|   |                      /-----------\                                  |   |
|   |                     /             \                                 |   |
|   |                    |    ( ^_^ )    |  <-- Xem trước ảnh tròn         |   |
|   |                     \             /      (Default or Old Avatar)    |   |
|   |                      \-----------/                                  |   |
|   |                                                                     |   |
|   |            Kéo thả tệp ảnh của bạn vào đây hoặc click               |   |
|   |                     để duyệt tệp từ thiết bị                        |   |
|   |                                                                     |   |
|   |        (Hỗ trợ định dạng: PNG, JPG, GIF | Dung lượng tối đa: 2MB)   |   |
|   +---------------------------------------------------------------------+   |
|                                                                             |
|   [ Hủy bỏ ]                                        [ Tải Lên Máy Chủ ]     |
+-----------------------------------------------------------------------------+
```

### B. Sơ đồ ASCII Mockup - Trạng thái đang tải lên (Uploading & Success States)
```text
+-----------------------------------------------------------------------------+
|   UPDATE AVATAR (CẬP NHẬT ẢNH ĐẠI DIỆN)                                 [X] |
+-----------------------------------------------------------------------------+
|                                                                             |
|   +---------------------------------------------------------------------+   |
|   |  [UPLOADING STATE - ĐANG TẢI LÊN...]                                |   |
|   |                                                                     |   |
|   |                      /-----------\                                  |   |
|   |                     /   (O_O)     \                                 |   |
|   |                    |  Uploading... |  <-- Ảnh mờ + Spinner ở trung tâm|   |
|   |                     \             /                                 |   |
|   |                      \-----------/                                  |   |
|   |                                                                     |   |
|   |    Đang tải ảnh lên máy chủ phòng khám...                           |   |
|   |    +-----------------------------------------------------------+    |   |
|   |    |██████████████████████████████████████                     | 75%|   |
|   |    +-----------------------------------------------------------+    |   |
|   +---------------------------------------------------------------------+   |
|                                                                             |
|   [ Hủy bỏ ](disabled)                             [ Tải Lên... ](disabled) |
+-----------------------------------------------------------------------------+
```

---

## 2. Thiết kế Hệ thống Màu sắc & Trạng thái CSS (HSL Variables)

Vùng kéo thả (Dropzone) thay đổi trạng thái trực quan sinh động khi người dùng rê tệp tin vào (`dragover`):

```css
:root {
  /* Màu sắc Dropzone */
  --dropzone-bg: HSL(223, 47%, 7%, 0.5);          /* Tối mờ kính */
  --dropzone-border: HSL(217, 30%, 25%);          /* Viền nét đứt mặc định */
  
  /* Trạng thái Dragover (Đang rê ảnh lên vùng kéo thả) */
  --dropzone-bg-active: HSL(239, 84%, 67%, 0.15); /* Hơi xanh neon nhạt */
  --dropzone-border-active: HSL(235, 100%, 75%);  /* Viền phát sáng neon */
  
  /* Cấu hình Progress Bar */
  --progress-bg: HSL(223, 47%, 15%);
  --progress-fill: linear-gradient(90deg, HSL(239, 84%, 67%), HSL(280, 80%, 65%));
}

/* Áp dụng kiểu dáng */
.dropzone-container {
  background: var(--dropzone-bg);
  border: 2px dashed var(--dropzone-border);
  backdrop-filter: blur(8px);
  border-radius: 12px;
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.dropzone-container.is-dragover {
  background: var(--dropzone-bg-active);
  border-color: var(--dropzone-border-active);
  box-shadow: 0 0 20px HSL(235, 100%, 75%, 0.25);
  transform: scale(1.01); /* Hiệu ứng phồng nhẹ */
}
```

---

## 3. Hoạt ảnh & Trực quan hóa Tiến trình (Animations & Upload Progress)

### A. Hoạt ảnh Tải lên (Pulse Loading Animation)
Khi ảnh đang được tải lên, hình ảnh xem trước dạng tròn sẽ có lớp phủ mờ kính màu xám tối kèm hiệu ứng xoay spinner hoặc hiệu ứng đập mạch (pulse):
```css
@keyframes pulse-upload {
  0% { opacity: 0.6; }
  50% { opacity: 0.3; }
  100% { opacity: 0.6; }
}

.avatar-preview-uploading {
  animation: pulse-upload 1.5s infinite ease-in-out;
  filter: brightness(0.5) blur(1px);
}
```

### B. Thanh tiến trình (Progress Bar CSS)
Thành phần thanh tiến trình hiển thị mức độ tải lên thực tế tính theo phần trăm giúp người dùng an tâm hệ thống đang chạy:
```css
.progress-bar-container {
  width: 100%;
  height: 8px;
  background-color: var(--progress-bg);
  border-radius: 4px;
  overflow: hidden;
  margin-top: 15px;
}

.progress-bar-fill {
  height: 100%;
  background: var(--progress-fill);
  width: 0%; /* Sẽ cập nhật động bằng Javascript qua biến reactive */
  transition: width 0.1s linear;
}
```

### C. Client-Side Constraints Feedback (Cảnh báo lỗi nhanh)
Nếu người dùng kéo thả file không phải ảnh hoặc file quá lớn, dropzone sẽ chuyển sang viền đỏ đứt nét (`border-color: var(--color-error);`) kèm âm thanh cảnh báo ngắn hoặc hiệu ứng rung lắc (shake animation) để báo hiệu tệp tin bị từ chối:
```css
@keyframes shake-dropzone {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-6px); }
  75% { transform: translateX(6px); }
}

.dropzone-container.has-error {
  animation: shake-dropzone 0.4s ease-in-out;
  border-color: var(--color-error);
  box-shadow: 0 0 15px HSL(0, 84%, 60%, 0.2);
}
```
