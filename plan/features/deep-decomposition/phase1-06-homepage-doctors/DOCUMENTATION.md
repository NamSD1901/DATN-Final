# 📖 Product & Developer Documentation - Vets Team (Phase 1)

Tài liệu này cung cấp hướng dẫn vận hành cho người dùng và tài liệu phát triển/gỡ lỗi (Debugging Guide) dành cho các lập trình viên làm việc trên danh mục bác sĩ trang chủ của hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng cho Khách hàng (User Guide)

### Duyệt thông tin bác sĩ và đặt lịch khám nhanh:
1. Cuộn xuống mục **Đội ngũ bác sĩ thú y** tại trang chủ.
2. Sử dụng các thẻ Tab lọc: Nhấp chọn chuyên khoa mong muốn (Ví dụ: click tab **Da liễu** để lọc riêng các bác sĩ da liễu thú y).
3. Xem thông tin bằng cấp, kinh nghiệm và mô tả tiểu sử hiển thị trên thẻ bác sĩ.
4. Kiểm tra trạng thái trực ca của bác sĩ:
   - *🟢 Đang trực ca:* Bạn có thể đặt lịch hẹn khám được ngay bằng cách click **Đặt lịch khám**.
   - *🔴 Nghỉ phép:* Bác sĩ hiện không khả dụng để nhận lịch mới.
5. Khi tìm thấy bác sĩ phù hợp, nhấp nút **Đặt lịch khám**.
   - *Nếu bạn đã đăng nhập:* Hệ thống mở cửa sổ đặt lịch và chọn sẵn bác sĩ này.
   - *Nếu bạn chưa đăng nhập:* Hệ thống sẽ yêu cầu bạn đăng nhập trước khi tiến hành đặt lịch.

---

## 2. Tài liệu dành cho Lập trình viên (Developer Guide)

### 2.1. Cấu trúc các File liên quan trong Codebase
*   **Backend Web API:**
    *   [DoctorController.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.WebApi/Controllers/DoctorController.cs) — API công khai `/api/doctors`.
    *   [DoctorService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/DoctorService.cs) — Logic lấy dữ liệu, cấu hình `IMemoryCache` lưu đệm và dọn dẹp cache.
*   **Frontend Vue 3:**
    *   [VetsTeamCatalog.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/home/VetsTeamCatalog.vue) — Component danh mục bác sĩ, bộ lọc tab.
    *   [useDoctorsStore.ts](file:///e:/DATN/MyPetClinic/frontend/src/store/useDoctorsStore.ts) — Store Pinia quản lý tải dữ liệu bác sĩ và lưu cache cục bộ ở Client.

### 2.2. Hướng dẫn Gỡ lỗi & Kiểm tra Cache (Debugging Cache)
Để xác nhận xem API `/api/doctors` có đang chạy đúng cơ chế lưu đệm (Memory Cache) hay không:
1. Chạy ứng dụng Backend Web API.
2. Thực hiện gọi API lần đầu qua Postman hoặc trình duyệt: `GET http://localhost:5000/api/doctors`.
   - *Xem Console Terminal của Backend:* Sẽ xuất hiện log câu lệnh SQL truy vấn JOIN bảng `Doctors` và `Users`.
3. Thực hiện gọi lại API lần thứ 2, 3 liên tiếp:
   - *Kiểm tra Console Terminal:* Không phát sinh thêm bất kỳ câu lệnh SQL nào.
   - *Thời gian phản hồi (Response Time):* Giảm đột ngột từ 80ms xuống còn <5ms.

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### Trạng thái trực ca của Bác sĩ bị sai lệch (Stale Status)
*   *Triệu chứng:* Bác sĩ đã được chuyển sang trạng thái nghỉ phép ở trang quản trị, nhưng khách hàng truy cập trang chủ vẫn thấy hiển thị trạng thái đang trực ca.
*   *Nguyên nhân:* Hàm sửa trạng thái trực ca của Bác sĩ ở Backend chưa gọi lệnh dọn dẹp cache `ClearDoctorsCache()`, dẫn đến Client tiếp tục nạp dữ liệu cũ từ bộ đệm của RAM.
*   *Giải pháp:* Đảm bảo Controller của Lễ tân/Admin kế thừa đúng Service và kích hoạt hàm xóa Cache mỗi khi cập nhật dữ liệu thành công. Hoặc chờ 60 phút để Cache tự động hết hạn tuyệt đối.
