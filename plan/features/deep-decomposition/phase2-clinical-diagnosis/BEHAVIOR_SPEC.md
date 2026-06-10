# 🎭 Behavioral Specification - Clinical Diagnosis & Treatment

## 1. Biểu đồ Trạng thái Màn hình làm việc của Bác sĩ (Doctor Clinic Flow)

```mermaid
stateDiagram-v2
    [*] --> ViewingQueue : Truy cập workspace bác sĩ
    ViewingQueue --> LoadingHistory : Chọn thú cưng tiếp theo & bấm Tiếp nhận
    LoadingHistory --> DiagnosticForm : Xem xong bệnh sử cũ & mở form khám
    DiagnosticForm --> MedicineSearching : Gõ tìm kiếm thuốc để kê đơn
    MedicineSearching --> DiagnosticForm : Chọn thuốc & điền liều lượng
    DiagnosticForm --> SubmittingRecord : Bấm "Lưu Bệnh Án"
    
    state SubmittingRecord {
        [*] --> CheckStock
        CheckStock --> DeductStock : Tồn kho đủ
        CheckStock --> ShowStockError : Thiếu hàng tồn kho (Abort & Rollback)
        DeductStock --> SaveSuccess
    }

    ShowStockError --> DiagnosticForm : Bác sĩ điều chỉnh lại đơn thuốc
    SaveSuccess --> ViewingQueue : Hoàn thành ca khám, quay lại hàng chờ
```

---

## 2. Các Ràng buộc Phản ứng Giao diện (Interactive Constraints)
- **Real-time Input Validation:** Ô số lượng thuốc kê đơn không được phép nhận số âm hoặc số 0.
- **Stock Limit Alert:** Ngay khi bác sĩ nhập số lượng lớn hơn tồn kho hiện hữu, viền của dòng thuốc đó phải lập tức chuyển sang màu đỏ kèm tooltip: `"Vượt quá số lượng tồn kho còn lại!"`.
