# 📖 API Reference Details - Cashier & Invoicing

## 1. POST /api/receptionist/invoices
Tạo hóa đơn nháp từ mã lịch hẹn đã hoàn thành khám bệnh.

*   **Auth:** `[Authorize(Roles = "receptionist,cashier,admin")]`
*   **Request Body:**
    ```json
    {
      "appointmentId": "e2c38d4f-3721-4f18-a664-d3a373ff2010"
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "id": "7b8f9e0d-1234-5678-abcd-ef0123456789",
      "invoiceNumber": "HD-20260610-001",
      "appointmentId": "e2c38d4f-3721-4f18-a664-d3a373ff2010",
      "serviceAmount": 150000.0,
      "medicineAmount": 220000.0,
      "totalAmount": 370000.0,
      "status": "Unpaid",
      "createdAt": "2026-06-10T12:00:00Z"
    }
    ```

---

## 2. PUT /api/receptionist/invoices/{id}/pay
Xác nhận hóa đơn đã được thanh toán và lưu trữ phương thức thanh toán.

*   **Auth:** `[Authorize(Roles = "receptionist,cashier,admin")]`
*   **Request URL Param:** `id` (Guid) - ID của hóa đơn.
*   **Request Body:**
    ```json
    {
      "paymentMethod": "BankTransfer" // Cash, BankTransfer
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "message": "Xác nhận thanh toán hóa đơn thành công"
    }
    ```
*   **Response (400 Bad Request):**
    ```json
    {
      "message": "Hóa đơn này đã được thanh toán trước đó"
    }
    ```
