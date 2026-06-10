# 📄 User & Dev Documentation - Clinical Diagnosis & Treatment

## 1. Cấu trúc Database liên quan đến Bệnh án & Đơn thuốc

Dưới đây là thiết kế SQL của các bảng thực tế để lập trình viên backend cấu hình EF Core Entity:

```sql
-- Bảng MedicalRecords
CREATE TABLE "MedicalRecords" (
    "Id" UUID PRIMARY KEY,
    "AppointmentId" UUID NOT NULL FOREIGN KEY REFERENCES "Appointments"("Id"),
    "PetId" UUID NOT NULL FOREIGN KEY REFERENCES "Pets"("Id"),
    "DoctorId" UUID NOT NULL FOREIGN KEY REFERENCES "Doctors"("Id"),
    "Symptoms" TEXT NOT NULL,
    "Diagnosis" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL
);

-- Bảng Prescriptions
CREATE TABLE "Prescriptions" (
    "Id" UUID PRIMARY KEY,
    "MedicalRecordId" UUID NOT NULL FOREIGN KEY REFERENCES "MedicalRecords"("Id"),
    "CreatedAt" TIMESTAMP NOT NULL
);

-- Bảng PrescriptionItems
CREATE TABLE "PrescriptionItems" (
    "Id" UUID PRIMARY KEY,
    "PrescriptionId" UUID NOT NULL FOREIGN KEY REFERENCES "Prescriptions"("Id"),
    "MedicineId" UUID NOT NULL FOREIGN KEY REFERENCES "Medicines"("Id"),
    "Quantity" INT NOT NULL,
    "DosageInstructions" TEXT NOT NULL
);
```

---

## 2. Hướng dẫn Tích hợp Autocomplete Tìm kiếm Thuốc (Frontend)
Sử dụng thư viện `lodash.debounce` để tối ưu hóa hiệu năng tìm kiếm, tránh gửi quá nhiều request lên server khi bác sĩ đang gõ tên thuốc:

```typescript
import debounce from 'lodash.debounce';

const searchMedicinesDebounced = debounce(async (query: string) => {
  if (query.trim().length >= 2) {
    await store.searchMedicines(query);
  }
}, 300);
```
Trực quan hóa danh sách kết quả tìm kiếm với định dạng: `[Tên thuốc] - [Tồn: X] - [Giá: Y VNĐ]`.
