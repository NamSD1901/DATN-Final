# 🟠 Level 3: Advanced (Nâng Cao - Tuần 7-10)

Cấp độ Nâng cao đòi hỏi khả năng làm việc trực tiếp với kỹ thuật (API, Database), xây dựng kế hoạch nghiệm thu người dùng thực tế và quản lý các rủi ro phát triển phần mềm.

---

## 🎯 PRD-12: UAT Planning & Execution

Kiểm thử chấp nhận người dùng (UAT) là chặng kiểm thử cuối cùng trước khi bàn giao sản phẩm. QA/PO cần thiết kế các kịch bản nghiệm thu (UAT Scenarios) sát với vai trò thực tế.

### Kế Hoạch Nghiệm Thu Theo Vai Trò (Role-based UAT Plan)
* **Kịch bản Lễ tân (Receptionist Role):**
  - Thực hiện tiếp nhận 1 thú cưng chưa đặt trước (Walk-in) -> Đăng ký thông tin nhanh -> Cấp số thứ tự.
  - Check-in cho 1 khách hàng đã đặt lịch trực tuyến trước -> Xác thực số thứ tự hiển thị đúng hàng chờ.
  - Nhập thông tin thanh toán cho hóa đơn điều trị -> In hóa đơn định dạng PDF -> Xác nhận doanh thu hiển thị trên Dashboard.
* **Kịch bản Bác sĩ (Vet Role):**
  - Mở danh sách hàng chờ trên Tablet -> Chọn ca khám tiếp theo -> Xem lịch sử khám cũ của pet.
  - Viết chẩn đoán, kê 3 loại thuốc kháng sinh -> Nhấn "Hoàn thành ca khám" -> Kiểm tra thông tin gửi sang màn hình thanh toán của Lễ tân.

---

## 🎯 PRD-13: API Testing (Swagger / Postman)

QA/BA cần biết kiểm thử trực tiếp các API do Backend .NET 8 cung cấp mà không cần đợi giao diện hoàn thiện.

### Các Endpoint Quan Trọng Cần Kiểm Thử
1. **Đăng nhập (`POST /api/auth/login`):**
   - Kiểm tra xem cookie session có được thiết lập tự động (`HttpOnly`, `Secure`) hay không.
2. **Tạo Booking (`POST /api/bookings`):**
   - *Payload mẫu:*
     ```json
     {
       "petId": "d3b07384-d113-49be-a5d8-232d0012fcd3",
       "serviceId": "1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
       "appointmentDate": "2026-06-15",
       "timeSlot": "09:00 - 09:30",
       "vetId": "8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"
     }
     ```
3. **Mã Phản Hồi HTTP (HTTP Status Codes) cần nắm:**
   - `200 OK` hoặc `201 Created`: Thao tác thành công.
   - `400 Bad Request`: Payload sai cấu trúc hoặc vi phạm luật nghiệp vụ (ví dụ: đặt lịch vào khung giờ đã hết slot).
   - `401 Unauthorized`: Chưa đăng nhập hoặc session đã hết hạn.
   - `403 Forbidden`: Người dùng không có quyền (Ví dụ: Khách hàng gọi API xem báo cáo doanh thu của Admin).
   - `500 Internal Server Error`: Lỗi logic code từ Backend .NET Core.

---

## 🎯 PRD-14: SQL for Test Data & Verification

QA cần viết được các câu lệnh SQL cơ bản để truy vấn trực tiếp vào PostgreSQL database, đối chiếu kết quả hiển thị trên Frontend nhằm loại trừ lỗi "giao diện hiển thị đúng nhưng lưu database sai".

### 💻 Các Câu Lệnh SQL Tiêu Biểu Cho MyPetClinic

1. **Kiểm tra lịch hẹn mới tạo:**
   ```sql
   SELECT b.id, p.name AS pet_name, b.appointment_date, b.time_slot, b.status 
   FROM bookings b
   JOIN pets p ON b.pet_id = p.id
   ORDER BY b.created_at DESC
   LIMIT 1;
   ```

2. **Kiểm tra tự động trừ tồn kho dược phẩm sau khi bác sĩ kê đơn:**
   ```sql
   SELECT name, stock_quantity, updated_at 
   FROM medicines 
   WHERE name = 'Amoxicillin 250mg';
   ```

3. **Kiểm tra tổng doanh thu trong ngày:**
   ```sql
   SELECT SUM(total_amount) AS total_revenue 
   FROM invoices 
   WHERE payment_status = 'Paid' 
     AND DATE(payment_date) = CURRENT_DATE;
   ```

---

## 🎯 PRD-15: Product Metrics & KPI Tracking

Để đo lường độ hiệu quả của sản phẩm sau khi đưa vào vận hành thực tế:
* **Booking Conversion Rate (Tỷ lệ chuyển đổi đặt lịch):** Số lượt đặt lịch thành công / Tổng số lượt truy cập trang web.
* **Clinic Wait Time (Thời gian chờ tại phòng khám):** Thời gian từ lúc Lễ tân check-in cho khách đến khi Bác sĩ bắt đầu gọi vào khám (Mục tiêu: $< 15$ phút).
* **Retention Rate (Tỷ lệ khách hàng quay lại):** Số phần trăm chủ nuôi quay lại phòng khám trong vòng 3 tháng cho các dịch vụ spa hoặc tái chủng vaccine.
* **Monthly Active Users (MAU):** Đo lường lượng người dùng tương tác đều đặn hàng tháng.

---

## 🎯 PRD-16: Risk Management

Một số rủi ro phổ biến trong dự án MyPetClinic và cách giảm thiểu:
* **Scope Creep (Phình to phạm vi):** PO muốn thêm quá nhiều tính năng nhỏ lẻ giữa Sprint. *Giải pháp:* Đưa các yêu cầu mới vào Product Backlog và thực hiện phân tích mức độ ưu tiên ở kỳ Refinement tiếp theo, tuyệt đối không chèn trực tiếp vào Sprint đang chạy.
* **Technical Debt (Nợ kỹ thuật):** Các lỗi vặt chất đống làm chậm hệ thống. *Giải pháp:* Dành ra 10 - 20% dung lượng mỗi Sprint (Sprint Capacity) để dọn dẹp refactor code và sửa các bug cũ.

---

## 🎯 PRD-17: Stakeholder Communication

* **Làm việc với Dev Lead (Nam):** Trao đổi về tính khả thi kỹ thuật (Technical feasibility), API contract trước khi dev bắt đầu code.
* **Làm việc với UX/UI Designer (Phương):** Đảm bảo giao diện tối ưu hóa trải nghiệm người dùng, sử dụng đúng bộ Design System Premium Gold.
* **Làm việc với Khách hàng / Chủ phòng khám:** Lắng nghe phản hồi thực tế từ họ để điều chỉnh thứ tự ưu tiên của Product Backlog.
