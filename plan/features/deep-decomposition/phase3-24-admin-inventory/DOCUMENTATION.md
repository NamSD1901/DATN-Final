# 📖 Operator & Developer Documentation - Admin Drug Inventory

Tài liệu hướng dẫn vận hành (dành cho Thủ kho / Admin) và tài liệu tích hợp/debug kỹ thuật (dành cho Lập trình viên).

---

## 1. Hướng dẫn Vận hành dành cho Thủ kho (Operator Guide)

### Quy trình Kiểm kho và Đối soát Định kỳ Cuối tháng
Để đảm bảo số lượng thuốc thực tế trên kệ tủ thuốc khớp hoàn toàn với số dư trên phần mềm MyPetClinic, Thủ kho thực hiện theo các bước sau vào ngày 30 hàng tháng:

1. Đăng nhập hệ thống với quyền **Admin**.
2. Chọn mục **Quản lý Kho thuốc** -> Xuất báo cáo tồn kho hiện tại ra file Excel bằng nút **[Xuất báo cáo]**.
3. Tiến hành kiểm đếm thủ công từng hộp thuốc, lọ vắc-xin trên kệ vật lý, ghi chép lại:
   - Số lượng thực tế.
   - Số lô sản xuất (`BatchNumber`) và Hạn sử dụng của từng hộp.
4. Đối soát chéo:
   - Nếu phát hiện **lệch số dư** (ví dụ: phần mềm báo còn 10 hộp nhưng thực tế chỉ còn 9 hộp do hao hụt hoặc đổ vỡ):
     - Không được tự ý sửa trực tiếp danh mục.
     - Lập phiếu điều chỉnh kho (Stock Adjustment) chọn lý do: *Hao hụt y tế, Đổ vỡ vật lý, hoặc Sai sót kiểm đếm*.
   - Nếu phát hiện **lô thuốc cận hạn dùng** (hạn dùng dưới 30 ngày):
     - Dán nhãn cận hạn lên hộp thuốc vật lý.
     - Kiểm tra phần mềm xem hệ thống đã phát cảnh báo màu cam `Expiry Warning` chưa để ưu tiên xuất trước.

---

## 2. Hướng dẫn Kỹ thuật dành cho Developer (Developer Guide)

### Các lệnh cURL Kiểm thử API Thủ công (API Debugging)

#### 1. Lấy danh sách toàn bộ danh mục thuốc kèm tồn kho
```bash
curl -X GET "https://localhost:5001/api/admin/inventory/medicines" \
     -H "Authorization: Bearer <JWT_TOKEN>"
```

#### 2. Nhập lô hàng thuốc mới (Admin thực hiện)
```bash
curl -X POST "https://localhost:5001/api/admin/inventory/batches" \
     -H "Authorization: Bearer <ADMIN_JWT_TOKEN>" \
     -H "Content-Type: application/json" \
     -d "{\"medicineId\": \"d290f1ee-6c54-4b01-90e6-d701748f0851\", \"batchNumber\": \"LOT-202606-02\", \"quantity\": 100, \"manufacturingDate\": \"2026-05-01\", \"expiryDate\": \"2028-05-01\"}"
```

#### 3. Truy xuất nhật ký biến động giao dịch kho
```bash
curl -X GET "https://localhost:5001/api/admin/inventory/transactions" \
     -H "Authorization: Bearer <ADMIN_JWT_TOKEN>"
```

---

## 3. Khắc phục Sự cố Thường gặp (Troubleshooting)

### Sự cố: Lệch số dư kho do lỗi Race-condition trong giờ cao điểm
- **Triệu chứng:** Số dư tổng của thuốc (`CurrentStock`) bị lệch, lệch nhỏ hơn hoặc lớn hơn tổng số lượng thực tế cộng dồn của các lô hàng `InventoryBatches`.
- **Nguyên nhân:** Do hai luồng xử lý xuất kho đồng thời của bác sĩ chạy qua điều kiện kiểm tra tồn tổng nhưng không được khóa dòng PostgreSQL (`FOR UPDATE`), dẫn đến việc cả hai giao dịch cùng đọc một giá trị tồn cũ và ghi đè giá trị sai.
- **Giải pháp khắc phục:**
  1. Kiểm tra log lỗi để tìm các giao dịch bị xung đột ghi đè.
  2. Đồng bộ lại số lượng tồn kho tổng bằng câu lệnh SQL sửa sai trực tiếp trên Database:
     ```sql
     -- Cập nhật lại tồn tổng bằng tổng tồn các lô đang hoạt động
     UPDATE Medicines m
     SET CurrentStock = COALESCE(
         (SELECT SUM(CurrentQuantity) 
          FROM InventoryBatches b 
          WHERE b.MedicineId = m.Id AND b.Status = 'InStock'), 0
     );
     ```
  3. Đảm bảo mã nguồn Service đã kích hoạt chỉ thị khóa dòng bi quan `FOR UPDATE` trong giao dịch xuất kho.
