# 🧠 Core Business Logic - Pet Portfolio Management

## 1. Cơ chế Bảo mật Chống IDOR (Ownership Validation)
Để bảo mật dữ liệu y tế của thú cưng, hệ thống MyPetClinic bắt buộc phải đối chiếu quyền sở hữu đối với mọi thao tác Đọc chi tiết, Sửa đổi hoặc Xóa hồ sơ thú cưng.

*   **Nguyên tắc cốt lõi:** Khi người dùng gửi yêu cầu tác động lên ID thú cưng cụ thể (ví dụ: `id = 125`), API controller không tin tưởng mù quáng mà bắt buộc truyền kèm tham số `ownerId` giải mã từ Token JWT của tài khoản đang thực hiện request.
*   **Mã nguồn đối chiếu logic (Service Layer):**
```csharp
public async Task UpdatePetAsync(UpdatePetDto dto, Guid ownerId)
{
    // 1. Truy vấn thực thể thú cưng từ Database bằng khóa chính ID
    var pet = await _petRepository.GetPetByIdAsync(dto.Id);
    
    // 2. Kiểm tra sự tồn tại và cờ xóa mềm
    if (pet == null)
    {
        throw new KeyNotFoundException("Không tìm thấy thông tin thú cưng yêu cầu.");
    }

    // 3. CHẶN ĐỨNG IDOR: Đối chiếu chủ sở hữu
    if (pet.OwnerId != ownerId)
    {
        // Quăng ngoại lệ từ chối quyền truy cập trái phép
        throw new UnauthorizedAccessException("Không tìm thấy thú cưng hoặc bạn không có quyền sửa.");
    }

    // 4. Tiến hành ghi đè thông tin mới
    pet.Name = dto.Name;
    pet.Species = dto.Species;
    pet.Breed = dto.Breed;
    pet.Gender = dto.Gender;
    pet.BirthDate = dto.BirthDate;
    pet.Weight = dto.Weight;
    pet.Color = dto.Color;
    pet.BloodType = dto.BloodType;
    pet.Sterilized = dto.Sterilized;
    pet.MicrochipCode = dto.MicrochipCode;
    pet.AllergyNote = dto.AllergyNote;

    // 5. Lưu thay đổi
    await _petRepository.UpdatePetAsync(pet);
    await _petRepository.SaveChangesAsync();
}
```

---

## 2. Logic Xóa mềm (Soft Delete Policy)
*   **Tại sao không dùng Xóa cứng (Hard Delete)?** Trong nghiệp vụ thú y, hồ sơ thú cưng liên kết trực tiếp với dữ liệu lịch sử các ca khám chữa bệnh cũ (Bệnh án lâm sàng), các hóa đơn tài chính đã xuất, và danh sách tiêm phòng vắc-xin. Việc xóa cứng vật lý dòng thú cưng khỏi DB sẽ gây lỗi phá vỡ tính toàn vẹn tham chiếu khóa ngoại (Foreign Key Integrity Cascade Error) hoặc làm mất thông tin kế toán y tế.
*   **Cách thức vận hành:**
    *   Thêm cột `IsDeleted` (boolean) vào bảng `Pets`.
    *   Khi xóa, Repo chỉ cập nhật: `pet.IsDeleted = true;`.
    *   Tất cả các truy vấn xem danh sách ở phía khách hàng sẽ tự động lọc bỏ các bé đã bị xóa:
        `SELECT * FROM Pets WHERE OwnerId = @OwnerId AND IsDeleted = false`
    *   Đối với giao diện của Bác sĩ hoặc Lễ tân tra cứu bệnh sử cũ, thông tin thú cưng vẫn được truy xuất ra bình thường nhưng có nhãn cảnh báo ẩn để phục vụ chuyên môn điều trị.

---

## 3. Logic Tính tuổi Thú cưng tự động (Dynamic Age Calculation Helper)
*   **Đặc điểm:** Tuổi của thú cưng nhỏ (chó mèo con) thường được bác sĩ tính theo tháng để kê liều lượng thuốc chính xác. Do đó, hiển thị tuổi dạng năm thuần túy sẽ không tối ưu.
*   **Logic tính toán ở Client/API Helper:**
```typescript
export function calculatePetAge(birthDateString: string | null): string {
  if (!birthDateString) return 'Không rõ ngày sinh';
  
  const birthDate = new Date(birthDateString);
  const today = new Date();
  
  let years = today.getFullYear() - birthDate.getFullYear();
  let months = today.getMonth() - birthDate.getMonth();
  
  if (months < 0 || (months === 0 && today.getDate() < birthDate.getDate())) {
    years--;
    months += 12;
  }
  
  if (years === 0) {
    if (months === 0) {
      // Nhỏ hơn 1 tháng tuổi (tính theo ngày)
      const diffTime = Math.abs(today.getTime() - birthDate.getTime());
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      return `${diffDays} ngày tuổi`;
    }
    return `${months} tháng tuổi`;
  }
  
  if (months === 0) {
    return `${years} tuổi`;
  }
  
  return `${years} tuổi ${months} tháng`;
}
```
*   **Hiển thị trực quan:** Sử dụng hàm helper này trên UI để hiển thị tuổi sinh động trên các thẻ Grid thú cưng.
