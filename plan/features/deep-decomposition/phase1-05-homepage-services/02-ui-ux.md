# 🎨 UI & UX Specifications - Services Catalog (Vue 3)

Tài liệu này đặc tả chi tiết giao diện danh mục dịch vụ công khai trên trang chủ, sơ đồ cấu trúc lưới (ASCII Mockup) và các hiệu ứng tương tác nâng cao của thẻ dịch vụ (Cards).

---

## 1. Thiết kế Giao diện Danh mục (Services Grid Layout)

Giao diện danh mục dịch vụ được tích hợp dưới dạng một khu vực cuộn trực quan, sử dụng bộ lọc Tabs và thanh tìm kiếm tích hợp:

```
+-----------------------------------------------------------------------------------+
|                                🏥 CÁC DỊCH VỤ CỦA CHÚNG TÔI                       |
|          Chăm sóc bé cưng chuyên nghiệp với trang thiết bị y tế hiện đại          |
|                                                                                   |
|  [ Tất cả ]   [ Khám bệnh ]   [ Tiêm chủng 🎯 ]   [ Phẫu thuật ]   [ Spa/Làm đẹp ]  | (Tabs lọc)
|  Search: +---------------------------------------------------------------------+  |
|          | Tìm dịch vụ... (ví dụ: Tắm, Tiêm phòng...)                          |  | (Thanh tìm kiếm)
|          +---------------------------------------------------------------------+  |
|                                                                                   |
|  +--------------------+   +--------------------+   +--------------------+         |
|  | [ Ảnh dịch vụ ]    |   | [ Ảnh dịch vụ ]    |   | [ Ảnh dịch vụ ]    |         |
|  | Tiêm Phòng Dại     |   | Khám Sức Khoẻ Định |   | Tắm Sấy Vệ Sinh    |         | (Dạng lưới Cards)
|  | Phòng bệnh dại...  |   | Kiểm tra tổng quát|   | Dịch vụ làm sạch...|         |
|  | Giá: 150.000đ      |   | Giá: 200.000đ      |   | Giá: 180.000đ      |         |
|  |   [⚡ Đặt Lịch ]   |   |   [⚡ Đặt Lịch ]   |   |   [⚡ Đặt Lịch ]   |         |
|  +--------------------+   +--------------------+   +--------------------+         |
|                                                                                   |
+-----------------------------------------------------------------------------------+
```

### 1.1. Thẻ dịch vụ kính mờ (Glassmorphism Service Card)
Mỗi dịch vụ được biểu diễn bằng một thẻ Card có cấu trúc CSS tinh tế:
*   `background: rgba(255, 255, 255, 0.05);`
*   `backdrop-filter: blur(16px);`
*   `border: 1px solid rgba(255, 255, 255, 0.1);`
*   `border-radius: 12px;`
*   `overflow: hidden;`

---

## 2. Xác thực Trực quan & Hiệu ứng Chuyển động (Micro-Animations)

*   **Hiệu ứng Hover trên Card (Card Elevate):**
    *   Khi rê chuột vào thẻ Card, thẻ sẽ dịch chuyển nhẹ lên trên theo trục Y (`transform: translateY(-8px)`) và độ mờ viền sáng lên (`border-color: var(--color-primary)`).
    *   Bóng đổ chuyển sang màu Neon nhẹ tỏa ra xung quanh: `box-shadow: 0 10px 20px rgba(13, 148, 136, 0.15)`.
    *   *Thời gian chuyển tiếp:* Cấu hình `transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1)` để tạo chuyển động vô cùng mượt mà.
*   **Bộ xương Đang tải (Skeleton Loading Screen):**
    *   Khi đang tải dữ liệu dịch vụ từ API, hệ thống không hiển thị màn hình trống. Thay vào đó, hiển thị 3-4 thẻ Card rỗng với hiệu ứng phát sáng mờ chạy qua lại (Shimmer loading effect) để người dùng có cảm giác trang đang tải nhanh hơn.
*   **Hiệu ứng Đọc thêm (Read More Overlay):**
    *   Phần mô tả dịch vụ dài quá 3 dòng sẽ tự động cắt ngắn bằng dấu ba chấm (`line-clamp: 3`). Hover chuột vào sẽ trượt hiển thị tooltip hoặc nút đọc thêm.

---

## 3. Khai báo CSS Skeleton Animation (Hiệu ứng Shimmer)

```css
@keyframes shimmer {
  0% {
    background-position: -468px 0;
  }
  100% {
    background-position: 468px 0;
  }
}

.skeleton-box {
  background: linear-gradient(to right, #242936 8%, #2d3345 18%, #242936 33%);
  background-size: 800px 104px;
  animation: shimmer 1.2s ease-in-out infinite;
  border-radius: 4px;
}
```
*Tất cả các thẻ Skeleton Card trong component Vue sẽ sử dụng lớp `.skeleton-box` này khi biến `isLoading` của store bằng `true`.*
