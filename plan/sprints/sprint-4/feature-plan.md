# 🗺️ LỘ TRÌNH VÀ CHI TIẾT SPRINT 4 - CỔNG DỊCH VỤ & BÁC SĨ CÔNG KHAI
## 📝 TÀI LIỆU KẾ HOẠCH TRIỂN KHAI VÀ PHÂN CHIA TÍNH NĂNG (SPRINT 4 MASTER PLAN)

Tài liệu này đặc tả chi tiết kế hoạch triển khai cho **Sprint 4: Cổng Dịch Vụ & Bác Sĩ Công Khai (Clinic Public Services & Vets View)**. Phân hệ chịu trách nhiệm xây dựng các màn hình công khai cho phép khách vãng lai và khách hàng tra cứu bảng giá dịch vụ điều trị/làm đẹp, lọc theo danh mục tương ứng, đồng thời xem danh sách đội ngũ bác sĩ thú y có ảnh đại diện và chi tiết chuyên môn tại phòng khám.

---

## 🛠 SPRINT 4: CLINIC PUBLIC SERVICES & VETS VIEW

### 4.1. Mục tiêu Sprint (Sprint Goal)
Xây dựng và tối ưu các cổng thông tin công khai giúp tiếp cận khách hàng:
*   **Xem danh sách dịch vụ & bảng giá (PB04):** Hiển thị trực quan các gói dịch vụ (Khám chữa bệnh, tiêm phòng, spa) kèm giá tiền và thời lượng thực hiện, hỗ trợ tìm kiếm và lọc theo danh mục.
*   **Xem đội ngũ bác sĩ (PB05):** Giới thiệu hồ sơ chuyên môn công khai của các bác sĩ để khách hàng tham khảo trước khi đặt lịch khám trực tuyến.

### 4.2. Danh sách công việc (Task Backlog)
1.  `[ ]` **T10:** PB04 - Hiện thực Backend API lấy danh sách Dịch vụ (có kèm Category), hỗ trợ lọc và tìm kiếm tối ưu.
2.  `[ ]` **T11:** PB04 - Xây dựng giao diện danh sách dịch vụ dạng Cards/Table, phân chia danh mục và có responsive.
3.  `[ ]` **T12:** PB05 - Hiện thực Backend API lấy danh sách Bác sĩ (lọc người dùng có vai trò là Bác sĩ) chỉ trả về thông tin công khai.
4.  `[ ]` **T13:** PB05 - Xây dựng giao diện hiển thị danh sách đội ngũ bác sĩ, trình bày đẹp mắt kèm chuyên môn/kinh nghiệm.

### 4.3. Tiêu chí nghiệm thu (DoD)
*   Truy vấn danh sách dịch vụ và bác sĩ sử dụng `AsNoTracking` để tối ưu hóa hiệu năng cơ sở dữ liệu.
*   Giao diện hiển thị dịch vụ và bác sĩ phải mượt mà, hỗ trợ tìm kiếm không bị gián đoạn và tương thích hoàn toàn với các kích thước màn hình di động/tablet.
*   API bác sĩ bảo mật, chỉ xuất ra các thông tin cần thiết (FullName, Email, Avatar, Bio, Gender), không để lộ hash mật khẩu hay dữ liệu cá nhân khác.
