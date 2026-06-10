# 🚀 Product Requirements Document (PRD) - Clinical Diagnosis & Treatment

## 1. Tổng quan & Tầm nhìn
Màn hình **Bác sĩ thú y (Clinical Diagnosis & Treatment)** cung cấp không gian làm việc chuyên sâu cho bác sĩ. Tại đây, bác sĩ có thể quản lý danh sách bệnh nhân chờ khám của mình, truy xuất nhanh hồ sơ lịch sử bệnh án đầy đủ của thú cưng, nhập triệu chứng, chẩn đoán, kê đơn thuốc và ghi nhận chi tiết lịch sử tiêm chủng vaccine. Hệ thống tự động trừ kho thuốc thực tế và đưa ra cảnh báo tức thời khi số lượng kê đơn vượt quá lượng tồn kho hiện tại.

---

## 2. Đối tượng sử dụng (Target Persona)
- **Bác sĩ thú y (Veterinarian):** Cần giao diện trực quan, hỗ trợ gõ nhanh (autocomplete), tìm kiếm thuốc thông minh, hiển thị cảnh báo tồn kho và truy cập lịch sử y tế của thú cưng chỉ với 1 click.

---

## 3. Yêu cầu Nghiệp vụ Chi tiết
- **Hàng chờ của bác sĩ (PB20):**
  - Hiển thị danh sách thú cưng được chỉ định khám cho bác sĩ đó theo số thứ tự (QueueNumber) từ lễ tân truyền sang.
  - Bác sĩ bấm "Tiếp nhận" để bắt đầu ca khám, hệ thống đổi trạng thái lịch hẹn sang `InProgress`.
- **Xem bệnh sử chi tiết (PB21):**
  - Hiển thị timeline lịch sử khám bệnh cũ, các đơn thuốc đã kê, kết quả xét nghiệm, và nhật ký tiêm phòng cũ của thú cưng đang khám.
- **Kê đơn thuốc & Kiểm tra Tồn kho (PB23):**
  - Form kê đơn thuốc động cho phép thêm nhiều dòng thuốc, hỗ trợ tìm kiếm thuốc tự động (Autocomplete) theo tên hoặc hoạt chất.
  - Hiển thị số lượng tồn kho thực tế (Stock) của từng loại thuốc ngay trên form kê đơn.
  - **Quy tắc nguyên tử (Atomicity):** Khi lưu bệnh án và đơn thuốc, hệ thống thực hiện trừ số lượng tồn kho của từng loại thuốc tương ứng. Nếu bất kỳ loại thuốc nào không đủ số lượng tồn kho, toàn bộ giao dịch phải được hủy bỏ (Rollback) và thông báo lỗi cụ thể cho bác sĩ.
- **Ghi nhận thông tin Tiêm chủng (PB24):**
  - Khi thực hiện tiêm phòng, bác sĩ ghi nhận: Loại vaccine (từ danh mục thuốc), Số lô sản xuất (BatchNumber), Liều lượng, Ngày tiêm và Ngày hẹn tái chủng dự kiến (NextDoseDate).
  - Tự động đồng bộ sang hồ sơ tiêm chủng của thú cưng để phục vụ gửi cảnh báo email nhắc lịch tự động sau này.
