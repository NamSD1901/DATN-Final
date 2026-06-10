# 🎨 UI/UX Design Spec - Clinical Diagnosis & Treatment

## 🔗 Skills Liên Quan
- **FE-F02 (CSS Variables):** Thiết kế giao diện đậm chất Y khoa với tone màu xanh ngọc chủ đạo (`--primary-cyan`: `#0EA5E9`), tạo sự tin tưởng và dễ chịu cho mắt bác sĩ khi làm việc lâu.
- **FE-F01 (HTML Semantic):** Sử dụng các khu vực `<aside>` cho thanh điều hướng bên bệnh sử thú cưng và `<main>` cho form ghi nhận triệu chứng.

---

## 1. Bố cục Màn hình Khám bệnh (Doctor Workspace Layout)

```
+------------------------------------------------------------+
|  👨‍⚕️ Bác sĩ: BS. Nguyễn Văn A               📅 10/06/2026  |
+-----------------------------+------------------------------+
| 📋 HÀNG CHỜ KHÁM (QUEUE)    | 🩺 MÀN HÌNH KHÁM BỆNH CHÍNH  |
| 1. Q-001 | Bông (🐶 Cún)    | Bệnh nhân đang khám: Bông    |
| 2. Q-002 | Miu  (🐱 Mèo)    | +--------------------------+ |
|                             | | Triệu chứng: [         ] | |
|                             | | Chẩn đoán:   [         ] | |
|                             | +--------------------------+ |
|                             | 💊 KÊ ĐƠN THUỐC              |
|                             | - Paracetamol  [ 10 ]  (X) |
|                             | - [ Nhập tên thuốc... ]      |
+-----------------------------+------------------------------+
```

## 2. Dynamic Autocomplete & Stock Indicator
- Khi bác sĩ gõ chữ vào ô thêm thuốc, hệ thống hiển thị danh sách thả xuống (dropdown) gợi ý tự động.
- Kế bên tên mỗi loại thuốc trong dropdown phải hiển thị nhãn số lượng tồn kho (ví dụ: `Amoxicillin - Tồn: 150 hộp`). Nếu tồn kho bằng 0, chữ sẽ chuyển sang màu đỏ và bị disabled không cho chọn.
- Sử dụng hiệu ứng chuyển động trượt mượt mà khi bác sĩ thêm hoặc xóa một dòng thuốc khỏi đơn thuốc.
