# 🛠️ Technical Specification - Clinical Diagnosis & Treatment

## 🔗 Skills Liên Quan
- **BE-F02 (SOLID - SRP):** Tách biệt logic quản lý bệnh án (`MedicalRecordService`) và logic xử lý trừ kho thuốc (`InventoryStockService`).
- **BE-A02 (Unit of Work / Transaction):** Áp dụng Transaction Scope đảm bảo đơn thuốc và cập nhật kho thuốc là Atomic.
- **BE-C02 (EF Core):** Sử dụng các truy vấn tải trước (Eager Loading) `.Include()` để tối ưu hóa việc đọc lịch sử bệnh án phức tạp.

---

## 1. Bản vẽ Thiết kế Database quan hệ (Data Model)

```mermaid
erDiagram
    Appointments ||--|| MedicalRecords : "creates"
    MedicalRecords ||--|| Prescriptions : "has"
    Prescriptions ||--o{ PrescriptionItems : "contains"
    PrescriptionItems ||--|| Medicines : "references"
    MedicalRecords ||--o{ VaccinationRecords : "records"
    Pets ||--o{ MedicalRecords : "has history"
```

---

## 2. Sequence Diagram: Kê đơn & Trừ kho thuốc Atomic

```mermaid
sequenceDiagram
    actor Doctor as Bác sĩ
    participant FE as Vue Frontend
    participant API as Web API Gateway
    participant DB as PostgreSQL Database

    Doctor->>FE: Nhập triệu chứng, chẩn đoán & thêm thuốc kê đơn
    FE->>API: POST /api/doctor/medical-records (Data)
    Note over API: Bắt đầu DbContext Transaction
    API->>DB: Insert MedicalRecord & Prescription
    
    loop Với mỗi Medicine trong PrescriptionItems
        API->>DB: Select Medicine.StockQuantity (Row Lock)
        alt StockQuantity < Kê đơn
            Note over API: Phát hiện thiếu thuốc trong kho
            API-->>FE: Rollback Transaction & Trả về HTTP 400 (Tên thuốc thiếu)
        else Hợp lệ
            API->>DB: Update Medicine.StockQuantity = StockQuantity - Kê đơn
        end
    end

    API->>DB: Save Changes & Commit Transaction
    DB-->>API: Giao dịch thành công
    API-->>FE: HTTP 201 Created
    FE-->>Doctor: Hiển thị thông báo thành công & In đơn thuốc
```
