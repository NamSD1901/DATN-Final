# 📝 Implementation Plan & Testing Strategy - Clinical Diagnosis & Treatment

## 1. Kế hoạch Triển khai (Sprint 4)

| Giai đoạn | Task | Skills áp dụng | Est. |
|---|---|---|---|
| 1 | Tạo các Entity `MedicalRecord`, `Prescription`, `PrescriptionItem` và cấu hình liên kết DbContext | BE-C02 (EF Core) | 2h |
| 2 | Code endpoint GET `/api/pets/{id}/medical-history` truy xuất timeline bệnh án cũ | BE-F03, BE-C02 | 2h |
| 3 | Triển khai logic Transaction lưu bệnh án, trừ kho thuốc & cấu hình Lock dữ liệu tránh Race condition | BE-F01, BE-A02 | 4h |
| 4 | Code UI Dashboard của bác sĩ, form kê đơn thuốc động và Autocomplete tích hợp Debounce | FE-C03, FE-F01 | 6h |
| 5 | Tích hợp kiểm thử tích hợp (Integration Tests) kiểm tra việc Rollback khi hết thuốc | BE-T01 (Testing) | 3h |

---

## 2. QA Test Suite (Kiểm thử chức năng & Tính đúng đắn của Kho)

### Case 1: Tạo bệnh án và đơn thuốc thành công (Kho đủ số lượng)
- **Các bước:** Đăng nhập bác sĩ ➡️ Tiếp nhận ca khám thú cưng ➡️ Nhập bệnh án ➡️ Kê đơn thuốc A (số lượng 2, tồn kho hiện tại là 10) ➡️ Lưu bệnh án.
- **Kết quả mong muốn:** API trả về 201 Created. Bảng `MedicalRecords`, `Prescriptions` được thêm bản ghi mới. Số lượng tồn kho của thuốc A giảm từ 10 xuống 8.

### Case 2: Kiểm thử tính Atomic (Rollback) khi một loại thuốc trong đơn bị thiếu kho
- **Các bước:** Kê đơn 2 loại thuốc: Thuốc A (kê 2, tồn kho 10) và Thuốc B (kê 5, tồn kho 3) ➡️ Lưu bệnh án.
- **Kết quả mong muốn:** API trả về 400 Bad Request kèm thông báo lỗi cụ thể loại thuốc thiếu. Giao dịch bị hủy hoàn toàn (Rollback). Không có bản ghi bệnh án hay đơn thuốc nào được lưu vào DB. Tồn kho của Thuốc A vẫn giữ nguyên là 10.

### Case 3: Chặn truy cập trái phép bệnh án thú cưng
- **Các bước:** Đăng nhập bằng tài khoản Khách hàng ➡️ Cố gắng gọi API POST `/api/doctor/medical-records`.
- **Kết quả mong muốn:** API trả về HTTP 403 Forbidden. Yêu cầu tạo bệnh án bị từ chối.
