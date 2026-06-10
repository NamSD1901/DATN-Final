# 📖 Product & Developer Documentation - Services Catalog (Phase 1)

Tài liệu này cung cấp hướng dẫn vận hành cho người dùng và tài liệu phát triển/gỡ lỗi (Debugging Guide) dành cho các lập trình viên làm việc trên danh mục dịch vụ trang chủ của hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng cho Khách hàng (User Guide)

### Duyệt bảng giá và đặt lịch nhanh:
1. Cuộn xuống mục **Dịch vụ của chúng tôi** tại trang chủ.
2. Sử dụng thanh tìm kiếm: Gõ từ khóa liên quan đến nhu cầu (Ví dụ: `Dại`, `Khám`, `Tắm`).
3. Sử dụng các thẻ Tab lọc: Nhấp chọn danh mục mong muốn (Ví dụ: click tab **Spa/Làm đẹp** để lọc riêng các dịch vụ cắt tỉa lông).
4. Xem thông tin mô tả chi tiết và giá tiền công khai hiển thị trên thẻ dịch vụ.
5. Khi tìm thấy dịch vụ phù hợp, nhấp nút **Đặt lịch ngay**.
   - *Nếu bạn đã đăng nhập:* Hệ thống mở cửa sổ đặt lịch và chọn sẵn dịch vụ này.
   - *Nếu bạn chưa đăng nhập:* Hệ thống sẽ yêu cầu bạn đăng nhập trước khi tiến hành đặt lịch.

---

## 2. Tài liệu dành cho Lập trình viên (Developer Guide)

### 2.1. Cấu trúc các File liên quan trong Codebase
*   **Backend Web API:**
    *   [ServiceController.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.WebApi/Controllers/ServiceController.cs) — API công khai `/api/services`.
    *   [ServiceService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/ServiceService.cs) — Logic lấy dữ liệu, cấu hình `IMemoryCache` lưu đệm và dọn dẹp cache.
*   **Frontend Vue 3:**
    *   [ServicesCatalog.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/home/ServicesCatalog.vue) — Component danh mục dịch vụ, bộ lọc tab và tìm kiếm.
    *   [useServicesStore.ts](file:///e:/DATN/MyPetClinic/frontend/src/store/useServicesStore.ts) — Store Pinia quản lý tải dữ liệu dịch vụ và lưu cache cục bộ ở Client.

### 2.2. Hướng dẫn Gỡ lỗi & Kiểm tra Cache (Debugging Cache)
Để xác nhận xem API `/api/services` có đang chạy đúng cơ chế lưu đệm (Memory Cache) hay không:
1. Chạy ứng dụng Backend Web API.
2. Thực hiện gọi API lần đầu qua Postman hoặc trình duyệt: `GET http://localhost:5000/api/services`.
   - *Xem Console Terminal của Backend:* Sẽ xuất hiện log câu lệnh SQL truy vấn tới bảng `Services` trong database PostgreSQL.
3. Thực hiện gọi lại API lần thứ 2, 3 liên tiếp:
   - *Kiểm tra Console Terminal:* Không phát sinh thêm bất kỳ câu lệnh SQL nào truy vấn tới bảng `Services`. Điều này chứng tỏ dữ liệu đang được trả về trực tiếp từ RAM của server.
   - *Thời gian phản hồi (Response Time):* Giảm đột ngột từ 80ms xuống còn <5ms.

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### Giá dịch vụ bị sai lệch (Stale Price Issue)
*   *Triệu chứng:* Admin đã cập nhật giá dịch vụ tiêm vắc-xin dại từ 150k lên 180k ở trang quản trị, nhưng khách hàng truy cập trang chủ vẫn thấy hiển thị giá cũ là 150k.
*   *Nguyên nhân:* Hàm sửa dịch vụ của Admin ở Backend chưa gọi lệnh dọn dẹp cache `ClearServicesCache()`, dẫn đến Client tiếp tục nạp dữ liệu cũ từ bộ đệm của RAM.
*   *Giải pháp:* Đảm bảo Controller của Admin kế thừa đúng Service và kích hoạt hàm xóa Cache mỗi khi cập nhật dữ liệu thành công. Hoặc chờ 60 phút để Cache tự động hết hạn tuyệt đối.
