# 🌐 Infrastructure & Security - Clinical Diagnosis & Treatment

## 1. Phân quyền API và Bảo mật vai trò (RBAC & Access Control)

Hồ sơ bệnh án và đơn thuốc là thông tin y sinh nhạy cảm, đòi hỏi các chính sách bảo mật truy cập nghiêm ngặt tương tự tiêu chuẩn HIPAA:
*   **Chặn truy cập trái phép:** Endpoint `/api/doctor/medical-records` được cấu hình cứng phân quyền `[Authorize(Roles = "doctor,admin")]`.
*   **Xác thực Claims JWT:** Lấy `doctorId` trực tiếp từ JWT Claims. Tuyệt đối không chấp nhận tham số `doctorId` truyền tự do từ Client gửi lên.
*   **Quy tắc giới hạn Bác sĩ điều trị:**
    Chỉ cho phép Bác sĩ được phân bổ khám ca đó (`AssignedDoctorId` trong lịch hẹn) thực hiện ghi chép bệnh án và kê đơn thuốc cho thú cưng:
    ```csharp
    var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
    if (appointment != null && appointment.AssignedDoctorId != currentDoctorId && !User.IsInRole("admin"))
    {
        throw new UnauthorizedAccessException("Bạn không phải bác sĩ được chỉ định cho ca khám này. Không thể ghi chép bệnh án.");
    }
    ```

---

## 2. Phòng chống Tấn công IDOR xem lén Bệnh sử (IDOR & Patient History Scope)

Một bác sĩ tò mò hoặc một tài khoản bị chiếm đoạt có thể tìm cách gọi API xem hồ sơ bệnh án cũ của các thú cưng không thuộc phòng khám điều trị của mình:

*   **Cơ chế phòng ngự phạm vi khám (Scope Guard):**
    Hệ thống chỉ cho phép Bác sĩ truy vấn bệnh sử cũ của thú cưng (`petId`) khi và chỉ khi:
    1. Thú cưng đó có một lịch hẹn đang ở trạng thái `in_progress` hoặc `waiting` được chỉ định cho chính bác sĩ đó trong ngày hôm nay.
    2. Hoặc tài khoản yêu cầu thuộc vai trò quản trị viên hệ thống (`admin`).
    ```csharp
    bool hasActiveVisit = await _context.Appointments
        .AnyAsync(a => a.PetId == petId 
                       && a.AssignedDoctorId == currentDoctorId 
                       && (a.Status == "in_progress" || a.Status == "waiting")
                       && a.AppointmentDate.Date == DateTime.UtcNow.Date);
                       
    if (!hasActiveVisit && !User.IsInRole("admin"))
    {
        throw new UnauthorizedAccessException("Thú cưng không nằm trong danh sách khám hôm nay của bạn. Quyền truy cập bệnh sử bị từ chối.");
    }
    ```

---

## 3. Quản lý Đồng thời và Khóa Bi quan Kho dược (Pessimistic Concurrency Control)

Khi phòng khám đông khách, nhiều phòng khám có thể đồng thời kê cùng một loại thuốc kháng sinh khan hiếm. Để ngăn ngừa xung đột dữ liệu tồn kho bị âm (Race Condition):

*   **PostgreSQL Row Locking (FOR UPDATE):**
    Trong luồng Entity Framework Core, trước khi trừ kho thuốc, hệ thống thực thi câu lệnh SQL thô có kèm từ khóa khóa dòng `FOR UPDATE`. Điều này báo cho PostgreSQL khóa bản ghi thuốc đó lại, buộc các transaction ở phòng khám khác muốn truy cập loại thuốc này phải xếp hàng chờ cho đến khi transaction hiện tại được Commit hoặc Rollback.
    ```sql
    SELECT "StockQuantity", "Price" FROM "Medicines" 
    WHERE "Id" = @MedicineId AND "IsActive" = true 
    FOR UPDATE;
    ```
*   **Transaction Isolation Level:**
    DbContext Transaction được thiết lập với mức độ cô lập dữ liệu là `ReadCommitted` kết hợp với Row Locking để đảm bảo tốc độ xử lý nhanh nhất mà vẫn loại bỏ hoàn toàn hiện tượng thất thoát kho dược phẩm.

---

## 4. Thiết kế Index Cơ sở dữ liệu cho Bệnh sử y khoa

Hoạt động tra cứu timeline bệnh sử của thú cưng quét qua nhiều bảng liên kết. Ta cấu hình các chỉ mục để tăng tốc độ truy vấn:

*   **Composite Index cho Bệnh sử thú cưng:**
    Giúp tăng tốc truy vấn tìm kiếm toàn bộ bệnh án của một thú cưng theo thời gian tạo giảm dần.
    ```sql
    CREATE INDEX IX_MedicalRecords_PetId_CreatedAt 
    ON "MedicalRecords" ("PetId", "CreatedAt" DESC);
    ```

*   **Index cho Chi tiết đơn thuốc:**
    Hỗ trợ kết xuất nhanh danh sách các loại thuốc đã kê trong bệnh án:
    ```sql
    CREATE INDEX IX_PrescriptionItems_PrescriptionId 
    ON "PrescriptionItems" ("PrescriptionId");
    ```

---

## 5. Giới hạn Tần suất gọi API (Rate Limiting)

*   API Kê đơn & Ghi bệnh án: Tối đa **10 requests / 1 phút / 1 tài khoản bác sĩ**.
*   API Tìm kiếm thuốc Autocomplete: Tối đa **120 requests / 1 phút / 1 tài khoản bác sĩ** (để hỗ trợ gõ phím gợi ý liên tục).
*   **Mã cấu hình .NET 8 Rate Limiter:**
    ```csharp
    options.AddFixedWindowLimiter("MedicalRecordSubmitPolicy", limitOptions =>
    {
        limitOptions.PermitLimit = 10;
        limitOptions.Window = TimeSpan.FromMinutes(1);
        limitOptions.QueueLimit = 0;
    });
    ```
