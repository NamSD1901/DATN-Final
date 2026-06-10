# 🌐 Infrastructure & Security - Online Vaccination Booking

## 1. Phân quyền API và Bảo mật Vai trò (RBAC & Endpoint Security)

Quy trình đặt lịch tiêm phòng vắc-xin trực tuyến được kiểm soát phân quyền chặt chẽ trên Backend để ngăn chặn truy cập trái phép:
* **Ràng buộc Endpoint:** Endpoint API `/api/vaccination/book` và `/api/vaccination/validate-interval` được cấu hình chặt chẽ với Annotation `[Authorize(Roles = "customer")]`. Chỉ những tài khoản khách hàng đã đăng nhập và được cấp JWT hợp lệ mới có thể tương tác.
* **Xác thực JWT token:** Backend phân giải Claims trong JWT để lấy `currentUserId` (hoặc `NameIdentifier`). Tuyệt đối không chấp nhận tham số `customerId` truyền thủ công dưới dạng Query string hoặc Request body để ngăn ngừa giả danh ID người dùng khác.
* **Tách biệt Portal:** Luồng xem lịch tiêm và duyệt tiêm của Bác sĩ/Lễ tân nằm ở các endpoint nội bộ khác với các chính sách phân quyền riêng biệt (`[Authorize(Roles = "receptionist,doctor")]`).

---

## 2. Phòng chống Tấn công IDOR chéo (Pet Ownership & Vaccine Verification)

Tấn công IDOR (Insecure Direct Object Reference) là lỗ hổng bảo mật nghiêm trọng trong việc thao tác dữ liệu thú cưng hoặc tiêm chủng không đúng chủ sở hữu:

* **Xác thực quyền sở hữu thú cưng (Pet Ownership Validator):**
  Trước khi kiểm tra khoảng cách tiêm hoặc lưu lịch hẹn tiêm phòng, Backend bắt buộc thực thi kiểm tra tính hợp lệ của thú cưng:
  ```csharp
  var pet = await _dbContext.Pets
      .FirstOrDefaultAsync(p => p.Id == dto.PetId && p.OwnerId == currentUserId);
  if (pet == null)
  {
      throw new UnauthorizedAccessException("Thú cưng không hợp lệ hoặc không thuộc quyền sở hữu của bạn.");
  }
  ```
  Cơ chế này ngăn chặn hoàn toàn việc một tài khoản khách hàng gửi ID thú cưng của người khác để dò quét lịch sử tiêm phòng hoặc tạo lịch hẹn tiêm giả.

* **Xác thực trạng thái vắc-xin y tế:**
  Hệ thống kiểm tra ID vắc-xin yêu cầu (`dto.VaccineId`) nhằm đảm bảo:
  * Vắc-xin đó tồn tại và đang được phân phối tại phòng khám (`IsActive = true`).
  * Vắc-xin khớp với loài của thú cưng (ví dụ: Không thể chọn vắc-xin chó cho mèo).

---

## 3. Thiết kế Index Cơ sở dữ liệu và Tối ưu hóa Truy vấn

Tần suất tra cứu lịch sử tiêm chủng và đối chiếu khoảng cách tiêm giữa các mũi rất lớn, đặc biệt khi khách hàng thao tác trên giao diện đặt lịch. Ta cần thiết lập các Database Index tối ưu trên hệ cơ sở dữ liệu:

* **Index tra cứu lịch sử tiêm phòng:**
  Giúp tăng tốc truy quét lịch sử tiêm phòng của thú cưng theo thời gian và theo loại vắc-xin để phục vụ logic nghiệp vụ tính khoảng cách tái chủng.
  ```sql
  CREATE INDEX IX_VaccinationRecords_PetId_VaccineId_Date 
  ON "VaccinationRecords" ("PetId", "VaccineId", "VaccinatedDate" DESC);
  ```

* **Index lọc tồn kho vắc-xin khả dụng:**
  Hỗ trợ tải danh sách vắc-xin còn hàng nhanh chóng cho giao diện lựa chọn bước 2.
  ```sql
  CREATE INDEX IX_Medicines_Vaccine_StockQuantity 
  ON "Medicines" ("Id", "Name", "TargetSpecies") 
  WHERE "Category" = 'Vaccine' AND "StockQuantity" > 0 AND "IsActive" = true;
  ```

---

## 4. Quản lý Đồng thời và Đồng bộ Tồn kho Dược (Concurrency & Inventory Validation)

Việc đặt lịch hẹn tiêm phòng có tính chất đặc thù là yêu cầu kiểm kho vắc-xin thời gian thực (Real-time Inventory Check). Tránh tình trạng hai khách hàng đặt lịch cùng lúc cho 1 liều vắc-xin cuối cùng trong kho (Race Condition):

* **Transaction Isolation Level:**
  Luồng kiểm tra tồn kho dược và xác nhận đặt lịch tiêm phòng được thực thi trong một Database Transaction với mức độ cô lập `Serializable` hoặc áp dụng cơ chế khóa bi quan (Pessimistic Locking / `SELECT FOR UPDATE`) đối với bản ghi của Vắc-xin trong bảng dược phẩm `Medicines`:
  ```sql
  SELECT StockQuantity FROM Medicines 
  WHERE Id = @VaccineId AND Category = 'Vaccine' 
  FOR UPDATE;
  ```
* **Quy trình trừ tồn kho y tế:**
  * **Tại bước Đặt lịch trực tuyến:** Hệ thống chỉ kiểm tra tồn kho ảo (`StockQuantity > 0`) để cho phép đặt lịch mà **chưa trừ tồn kho thực tế**. Điều này ngăn việc khách hàng đặt lịch ảo giữ chỗ làm khóa vắc-xin của khách hàng khác.
  * **Tại bước Check-in / Tiêm thực tế:** Khi thú cưng đến phòng khám và Bác sĩ xác nhận bắt đầu tiêm vắc-xin, hệ thống mới chính thức thực hiện trừ tồn kho dược (`StockQuantity = StockQuantity - 1`) thông qua transaction nội bộ của phần mềm quản lý kho.

---

## 5. Giới hạn Tần suất gọi API (Rate Limiting)

Đặt lịch tiêm phòng sinh ra các truy vấn phức tạp kết nối nhiều bảng (`Pets`, `VaccinationRecords`, `Medicines`, `Appointments`). Để tránh các cuộc tấn công từ chối dịch vụ (DDoS) hoặc công cụ spam lịch ảo:

* **Cấu hình Rate Limit:**
  * Endpoint xác thực khoảng cách tiêm (`/validate-interval`): Tối đa **10 requests / 1 phút / 1 IP**.
  * Endpoint đặt lịch tiêm (`/book`): Tối đa **2 requests / 1 phút / 1 tài khoản**.
* **Mã cấu hình Rate Limiting Middleware C#:**
  ```csharp
  options.AddTokenBucketLimiter(policyName: "VaccinationBookingLimit", tokenBucketOptions =>
  {
      tokenBucketOptions.TokenLimit = 2;
      tokenBucketOptions.QueueLimit = 0;
      tokenBucketOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
      tokenBucketOptions.TokensPerPeriod = 2;
  });
  ```
