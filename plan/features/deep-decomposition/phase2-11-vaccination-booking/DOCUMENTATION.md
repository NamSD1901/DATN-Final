# 📄 User & Dev Documentation - Online Vaccination Booking

Tài liệu này cung cấp hướng dẫn vận hành tiêm chủng dành cho nhân viên phòng khám và tài liệu tích hợp kỹ thuật chi tiết dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng & Quy trình vận hành (Operator & End-User Guide)

### Quy trình dành cho Khách hàng:
1. Đăng nhập hệ thống, nhấn nút **"Đặt lịch tiêm phòng"** từ thanh menu hoặc Dashboard.
2. **Bước 1 (Chọn thú cưng):** Click chọn chú thú cưng cần tiêm chủng. Hệ thống sẽ tự động ghi nhận loài của bé để chuẩn bị bộ lọc thuốc.
3. **Bước 2 (Chọn Vắc-xin):** Lựa chọn loại vắc-xin phù hợp từ Grid hiển thị vắc-xin còn hàng. Lịch sử tiêm phòng của bé sẽ hiển thị bên cạnh để tham khảo.
4. **Bước 3 (Chọn Giờ & Bác sĩ):** Chọn ngày tiêm phòng trên lịch và chọn khung giờ rảnh mong muốn.
5. **Bước 4 (Xác nhận & Cảnh báo):** Đọc kỹ tóm tắt lịch hẹn. Nếu có cảnh báo y tế về việc tiêm chủng quá sớm, bạn bắt buộc phải đánh dấu xác nhận đồng ý để kích hoạt nút gửi lịch.
6. **QR Code Check-in:** Lưu mã QR check-in được cấp sau khi đặt lịch thành công để quét tại quầy lễ tân khi đưa bé đến phòng khám.

### Quy trình tiếp nhận dành cho Lễ tân & Bác sĩ (Clinic Staff Operation):
1. **Tại Quầy Tiếp Nhận:** Khách hàng xuất trình QR code đặt lịch tiêm phòng. Lễ tân thực hiện quét mã để chuyển trạng thái lịch hẹn sang `waiting`.
2. **Xử lý Cờ Cảnh báo (Requires Override):** Nếu lịch hẹn này vi phạm phác đồ (hệ thống đánh dấu cờ warning đỏ trên màn hình tiếp nhận), Lễ tân cần thông báo cho Bác sĩ thú y trực ca để khám sàng lọc kỹ càng hơn trước khi tiêm.
3. **Thực thi Tiêm chủng & Cập nhật Kho:** Bác sĩ thực hiện tiêm phòng, sau đó ghi nhận thông tin tiêm chủng (mã lô vắc-xin, phản ứng sau tiêm) vào phần mềm. Khi Bác sĩ nhấn nút "Hoàn thành tiêm chủng", hệ thống sẽ:
   * Trừ 1 đơn vị tồn kho trong bảng dược phẩm `Medicines` (`StockQuantity = StockQuantity - 1`).
   * Tự động thêm một bản ghi mới vào bảng lịch sử tiêm chủng `VaccinationRecords` để cập nhật ngày tiêm mới nhất và tính toán ngày hẹn nhắc lại cho bé.

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc Database liên quan
Để triển khai tính năng đặt lịch tiêm chủng, cơ sở dữ liệu sử dụng bảng dược phẩm `Medicines` hiện có để lưu trữ vắc-xin (phân biệt bằng cột `Category = 'Vaccine'`), bổ sung thêm các thuộc tính y tế và tạo mới bảng `VaccinationRecords` để lưu lịch sử tiêm:

```sql
-- 1. Bổ sung các cột y sinh học vào bảng Medicines (nếu chưa có)
ALTER TABLE "Medicines" ADD COLUMN "TargetSpecies" VARCHAR(20) DEFAULT 'All'; -- 'Dog', 'Cat', 'All'
ALTER TABLE "Medicines" ADD COLUMN "MinAgeWeeks" INT DEFAULT 8;
ALTER TABLE "Medicines" ADD COLUMN "IntervalDays" INT DEFAULT 21;
ALTER TABLE "Medicines" ADD COLUMN "IsActive" BOOLEAN DEFAULT TRUE;

-- 2. Tạo bảng lưu trữ lịch sử tiêm chủng y khoa VaccinationRecords
CREATE TABLE "VaccinationRecords" (
    "Id"                     BIGSERIAL PRIMARY KEY,
    "PetId"                  BIGINT NOT NULL,
    "AppointmentId"          BIGINT NULL, -- Có thể null nếu tiêm walk-in trực tiếp hoặc nhập thủ công
    "VaccineId"              BIGINT NOT NULL,
    "VaccinatedDate"         DATE NOT NULL,
    "NextDueDate"            DATE NOT NULL, -- Ngày đề xuất tiêm nhắc lại
    "Notes"                  TEXT,
    "CreatedAt"              TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY ("PetId") REFERENCES "Pets" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("VaccineId") REFERENCES "Medicines" ("Id") ON DELETE RESTRICT
);
```

### B. Seed Dữ liệu mẫu y khoa (Seed Data Script)
Chạy script SQL sau để chèn dữ liệu vắc-xin mẫu vào cơ sở dữ liệu phục vụ phát triển và kiểm thử:

```sql
-- Làm sạch dữ liệu cũ
DELETE FROM "Medicines" WHERE "Category" = 'Vaccine';

-- Thêm vắc-xin cho Chó
INSERT INTO "Medicines" ("Name", "Category", "Unit", "StockQuantity", "Price", "TargetSpecies", "MinAgeWeeks", "IntervalDays", "IsActive")
VALUES 
('Vắc-xin 5 bệnh cho Chó (Nobivac DHPPi)', 'Vaccine', 'Liều', 40, 280000.00, 'Dog', 8, 21, true),
('Vắc-xin 7 bệnh cho Chó (Nobivac L4)', 'Vaccine', 'Liều', 15, 380000.00, 'Dog', 12, 28, true);

-- Thêm vắc-xin cho Mèo
INSERT INTO "Medicines" ("Name", "Category", "Unit", "StockQuantity", "Price", "TargetSpecies", "MinAgeWeeks", "IntervalDays", "IsActive")
VALUES 
('Vắc-xin 4 bệnh cho Mèo (Nobivac Tricat)', 'Vaccine', 'Liều', 35, 340000.00, 'Cat', 8, 21, true);

-- Thêm vắc-xin dùng chung (Dại)
INSERT INTO "Medicines" ("Name", "Category", "Unit", "StockQuantity", "Price", "TargetSpecies", "MinAgeWeeks", "IntervalDays", "IsActive")
VALUES 
('Vắc-xin phòng Dại Rabisin', 'Vaccine', 'Liều', 100, 150000.00, 'All', 12, 330, true);
```

### C. Kiểm thử API bằng Curl
Các lập trình viên có thể sử dụng các lệnh sau để kiểm thử nhanh API trong terminal:

#### 1. Kiểm tra khoảng cách tiêm phòng y tế (Validate Interval)
```bash
curl -X POST "https://localhost:5001/api/vaccination/validate-interval" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_JWT_CUSTOMER_TOKEN>" \
     -d "{\"petId\":12,\"vaccineId\":16,\"proposedDate\":\"2026-06-25T10:00:00Z\"}"
```

#### 2. Thực hiện đặt lịch tiêm vắc-xin
```bash
curl -X POST "https://localhost:5001/api/vaccination/book" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_JWT_CUSTOMER_TOKEN>" \
     -d "{\"petId\":12,\"vaccineId\":16,\"doctorId\":\"b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8\",\"appointmentDate\":\"2026-06-26T09:30:00Z\",\"symptom\":\"Tiêm phòng vắc-xin định kỳ theo phác đồ\",\"note\":\"Tiêm mũi 2\",\"bypassWarning\":true}"
```

---

## 3. Khắc phục sự cố thường gặp (Troubleshooting)

### Sự cố 1: Lỗi chênh lệch Múi giờ khi so sánh khoảng cách tiêm (Timezone Issues)
* **Nguyên nhân:** Lịch tiêm lịch sử lưu kiểu `DATE` trong DB (không chứa múi giờ, ví dụ: `2026-05-10`), còn ngày hẹn mới gửi lên dạng UTC ISO 8601 (`2026-06-15T10:00:00Z`). Việc chuyển đổi múi giờ không đồng nhất giữa Backend và Client có thể dẫn đến việc lệch 1 ngày, gây tính toán sai khoảng cách tối thiểu.
* **Khắc phục:** Tại Backend, khi so sánh, bắt buộc phải ép kiểu ngày đề xuất và ngày tiêm lịch sử về định dạng ngày thuần (`DateOnly` trong C# / .NET 8) trước khi tính khoảng cách:
  ```csharp
  DateOnly proposedDateOnly = DateOnly.FromDateTime(proposedDateTimeUtc.ToLocalTime());
  int daysDifference = proposedDateOnly.DayNumber - lastVaccinatedDateOnly.DayNumber;
  ```

### Sự cố 2: Vắc-xin hết hàng nhưng vẫn cho đặt lịch
* **Nguyên nhân:** Dữ liệu vắc-xin hiển thị trên Client là dữ liệu cũ đã được cache trong store, hoặc nhiều khách hàng submit đồng thời.
* **Khắc phục:** Đảm bảo hàm `fetchVaccines()` được gọi mỗi khi mở modal đặt lịch và tại Service Backend, logic đặt lịch bắt buộc phải truy vấn trực tiếp bảng `Medicines` kiểm tra lại `StockQuantity > 0` trong một Transaction độc lập trước khi lưu lịch hẹn.
