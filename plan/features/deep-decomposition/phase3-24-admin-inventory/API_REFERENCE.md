# 📖 API Reference Details - Admin Drug Inventory

## 1. GET /api/admin/medicines
Lấy toàn bộ danh mục thuốc hiện có trong kho.

*   **Auth:** `[Authorize(Roles = "admin")]`
*   **Response (200 OK):** Danh sách `MedicineDto[]`.

---

## 2. GET /api/admin/medicines/alerts
Lấy danh sách thuốc có cảnh báo (sắp hết tồn kho hoặc sắp hết hạn sử dụng).

*   **Auth:** `[Authorize(Roles = "admin")]`
*   **Response (200 OK):**
    ```json
    [
      {
        "id": "abc123",
        "name": "Amoxicillin 250mg",
        "stockQuantity": 5,
        "minStockLimit": 10,
        "expiryDate": "2026-07-01",
        "isLowStock": true,
        "isNearExpiry": false,
        "daysUntilExpiry": 21
      }
    ]
    ```

---

## 3. POST /api/admin/medicines/{id}/restock
Nhập thêm hàng vào kho cho loại thuốc cụ thể.

*   **Auth:** `[Authorize(Roles = "admin")]`
*   **Request Body:**
    ```json
    {
      "adjustQuantity": 50,
      "notes": "Nhập hàng từ nhà phân phối ABC tháng 06/2026"
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "message": "Đã cập nhật tồn kho thành công"
    }
    ```
*   **Response (400 Bad Request):**
    ```json
    {
      "message": "Số lượng nhập phải lớn hơn 0"
    }
    ```
