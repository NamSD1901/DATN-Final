# 🗓️ Sprint 12: Bệnh Án & Kê Đơn Thuốc (Trừ Kho Tự Động)
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Xây dựng hệ thống ghi nhận bệnh án lâm sàng (PB21, PB23) cho phép bác sĩ thú y nhập chẩn đoán, phương pháp điều trị và kê đơn thuốc chi tiết. Khi kê đơn, hệ thống tự động trừ số lượng thuốc tồn kho trong cùng một giao dịch (Transaction) an toàn — nếu bất kỳ thuốc nào hết hàng, toàn bộ thao tác sẽ bị rollback ngay lập tức. Đồng thời cập nhật trạng thái cuộc hẹn sang `Completed` khi bác sĩ kết thúc ca khám (PB25).

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T35] Xem hồ sơ & lịch sử thú cưng (Backend)
*   **Người thực hiện:** Hạnh (6 giờ)
*   **Nội dung công việc:**
    *   Xây dựng API tổng hợp thông tin thú cưng kèm danh sách bệnh án cũ (lịch sử khám, chẩn đoán trước đó, đơn thuốc đã kê).
    *   Sắp xếp theo ngày khám mới nhất lên đầu để bác sĩ dễ theo dõi.

### 2. [T36] Giao diện Doctor Timeline xem bệnh sử (Frontend)
*   **Người thực hiện:** Phương (8 giờ)
*   **Nội dung công việc:**
    *   Thiết kế Timeline trục thời gian bệnh sử cho bác sĩ xem trước khi bắt đầu ghi nhận bệnh án mới.
    *   Hiển thị chi tiết: Ngày khám, Bác sĩ điều trị trước đó, Chẩn đoán, Thuốc đã kê.

### 3. [T37] Quản lý bệnh án & Kê đơn thuốc - Transaction trừ kho rollback (Backend)
*   **Người thực hiện:** Nam (14 giờ)
*   **Nội dung công việc:**
    *   Thiết kế thực thể `MedicalRecord` (Bệnh án lâm sàng) và `Prescription` (Dòng chi tiết đơn thuốc).
    *   Xây dựng logic tạo bệnh án + kê đơn + trừ kho thuốc trong 1 Transaction duy nhất.
    *   Nếu bất kỳ thuốc nào có `QuantityAvailable < QuantityPrescribed`, rollback toàn bộ và trả về lỗi cụ thể.

### 4. [T38] Dynamic Prescription Form & Stock Warning (Frontend)
*   **Người thực hiện:** Phương (14 giờ)
*   **Nội dung công việc:**
    *   Xây dựng form kê đơn thuốc động: Bác sĩ có thể thêm/xóa nhiều dòng thuốc, mỗi dòng chọn tên thuốc từ danh mục và nhập số lượng.
    *   Hiển thị cảnh báo trực quan khi số lượng kê vượt quá tồn kho hiện tại.

### 5. [T39] Cập nhật trạng thái lịch hẹn sang Completed (Backend)
*   **Người thực hiện:** Hạnh (4 giờ)
*   **Nội dung công việc:**
    *   Khi bệnh án được ghi nhận thành công, tự động đổi trạng thái cuộc hẹn sang `Completed` và đóng `QueueEntry` tương ứng.

---

## 🔍 Tiêu Chí Nghiệm Thu (Definition of Done - DoD)
1.  **Atomicity:** Tạo bệnh án, kê đơn và trừ kho phải xảy ra trong 1 Transaction duy nhất. Không được phép xảy ra trạng thái trung gian (thuốc đã trừ nhưng bệnh án chưa được lưu).
2.  **Rollback an toàn:** Khi kê thuốc A (còn hàng) và thuốc B (hết hàng), hệ thống phải rollback cả thuốc A đã trừ trước đó.
3.  **Cảnh báo tồn kho:** Frontend hiển thị cảnh báo đỏ khi thuốc còn ≤ 5 đơn vị trong kho.
4.  **Hoàn thành ca khám:** Sau khi ghi nhận bệnh án, trạng thái cuộc hẹn và hàng đợi đồng bộ chuyển sang `Completed`.
