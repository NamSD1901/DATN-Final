# 📝 Implementation Plan & Testing Strategy - Admin Drug Inventory

## 1. Kế hoạch Triển khai (Sprint 6)

| Giai đoạn | Task | Skills áp dụng | Est. |
|---|---|---|---|
| 1 | Thêm cột `MinStockLimit` vào Entity `Medicine`, tạo bảng `StockLogs`, chạy Migration | BE-C02 (EF Core) | 1h |
| 2 | Code `InventoryAlertService` với logic phát hiện thuốc sắp hết tồn / sắp hết hạn | BE-F01, BE-F03 | 2h |
| 3 | Code API CRUD thuốc + API nhập kho `restock` với guard `[Authorize(Roles = "admin")]` | BE-A03 | 2h |
| 4 | Xây dựng giao diện bảng danh mục thuốc kèm badge cảnh báo và Modal nhập kho | FE-F01, FE-C03 | 4h |
| 5 | Viết unit test kiểm tra logic cảnh báo ngưỡng tồn kho | BE-T01 (Testing) | 2h |

---

## 2. QA Test Suite (Kiểm thử chức năng & Tính đúng đắn cảnh báo)

### Case 1: Cảnh báo sắp hết hàng chính xác
- **Các bước:** Tạo thuốc A có `StockQuantity = 5`, `MinStockLimit = 10` ➡️ Gọi GET `/api/admin/medicines/alerts`.
- **Kết quả mong muốn:** Thuốc A xuất hiện trong danh sách cảnh báo với `isLowStock = true`.

### Case 2: Nhập kho thành công
- **Các bước:** Gọi POST `/api/admin/medicines/{id}/restock` với `adjustQuantity = 50`.
- **Kết quả mong muốn:** `StockQuantity` tăng lên đúng 50 đơn vị. Bảng `StockLogs` ghi 1 dòng mới với số lượng và ghi chú tương ứng.

### Case 3: Cảnh báo thuốc sắp hết hạn
- **Các bước:** Tạo thuốc B có `ExpiryDate = Today + 10 ngày` ➡️ Gọi GET `/api/admin/medicines/alerts`.
- **Kết quả mong muốn:** Thuốc B xuất hiện trong danh sách với `isNearExpiry = true` và `daysUntilExpiry = 10`.
