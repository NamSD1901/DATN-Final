# 🗓️ Sprint 13: Tiêm Chủng Vaccine & Thanh Toán Hóa Đơn
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Xây dựng hệ thống ghi nhận mũi tiêm chủng cho thú cưng kèm tính toán ngày tái chủng tự động (`NextDueDate`) để phục vụ chức năng nhắc lịch tự động ở Sprint 17 (PB24). Đồng thời hoàn thiện hệ thống thanh toán hóa đơn tại quầy lễ tân: tự động kết xuất hóa đơn tổng hợp phí dịch vụ khám + phí thuốc đã kê, cho phép in hóa đơn HTML và xác nhận thanh toán (PB19).

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T40] Thanh toán hóa đơn - Kết xuất tự động (Backend)
*   **Người thực hiện:** Nam (10 giờ)
*   **Nội dung công việc:**
    *   Thiết kế thực thể `Invoice` (hóa đơn tổng) và `InvoiceDetail` (dòng chi tiết: dịch vụ khám, từng loại thuốc đã kê).
    *   Xây dựng logic tự động kết xuất hóa đơn khi ca khám hoàn thành: Tổng hợp phí dịch vụ (từ `ClinicService.Price`) + phí thuốc (từ `Medicine.UnitPrice * Prescription.Quantity`).
    *   API xác nhận thanh toán (`PUT /invoices/{id}/pay`).

### 2. [T41] In hóa đơn & Xác nhận thanh toán (Frontend)
*   **Người thực hiện:** Lâm (10 giờ)
*   **Nội dung công việc:**
    *   Thiết kế giao diện xem hóa đơn với bố cục in ấn (print-friendly CSS `@media print`).
    *   Nút "In hóa đơn" gọi `window.print()` và nút "Xác nhận thanh toán" cập nhật trạng thái hóa đơn.

### 3. [T42] Thực hiện tiêm chủng - Ghi nhận mũi tiêm & NextDueDate (Backend)
*   **Người thực hiện:** Hạnh (8 giờ)
*   **Nội dung công việc:**
    *   Thiết kế thực thể `VaccinationRecord` lưu thông tin mũi tiêm: Thú cưng, Vaccine, Số lô, Ngày tiêm, và `NextDueDate` (tính tự động dựa trên chu kỳ tiêm phòng của loại vaccine).
    *   API `POST /vaccinations` ghi nhận mũi tiêm.

### 4. [T43] Form nhập vaccine & Số lô (Frontend)
*   **Người thực hiện:** Phương (6 giờ)
*   **Nội dung công việc:**
    *   Xây dựng form nhập thông tin tiêm chủng: Chọn vaccine từ danh mục, nhập số lô (Batch Number), hiển thị ngày tái chủng tự động tính từ Backend.

---

## 🔍 Tiêu Chí Nghiệm Thu (Definition of Done - DoD)
1.  **Hóa đơn chính xác:** Tổng tiền hóa đơn = Phí dịch vụ + Σ(Giá thuốc × Số lượng kê). Không được tính sai hoặc thiếu bất kỳ dòng nào.
2.  **Trạng thái thanh toán:** Hóa đơn có 2 trạng thái: `Unpaid` (chưa thanh toán) và `Paid` (đã thanh toán).
3.  **NextDueDate chính xác:** Ngày tái chủng được tính tự động từ `VaccinationDate + Vaccine.IntervalDays`.
4.  **In ấn chuẩn:** Trang hóa đơn in ra sạch sẽ, ẩn header/sidebar/nút bấm khi in.
