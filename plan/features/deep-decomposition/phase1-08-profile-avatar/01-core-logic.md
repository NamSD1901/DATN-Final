# 🧠 Core Business Logic - Profile Avatar Upload

## 1. Phòng chống Tấn công Tải tệp độc hại (Upload Vulnerability Mitigation)
Tải ảnh đại diện là một trong những hành vi có rủi ro bảo mật cao nhất trong các ứng dụng web nếu không được cấu hình logic chặt chẽ. Hệ thống **MyPetClinic** áp dụng các biện pháp phòng vệ sau tại tầng Core Logic:

### A. Ngăn chặn tấn công Path Traversal (Dịch chuyển đường dẫn)
*   **Rủi ro:** Kẻ tấn công gửi tên file có dạng `../../etc/passwd` hoặc `..\..\WebApi\appsettings.json` nhằm ghi đè các tệp tin cấu hình nhạy cảm của hệ thống.
*   **Giải pháp xử lý:**
    1.  Không sử dụng trực tiếp tên tệp tin (`FileName`) do client gửi lên làm tên lưu trữ trên đĩa.
    2.  Bắt buộc tách phần đuôi mở rộng (Extension) bằng `Path.GetExtension(fileName)` và làm sạch bằng `ToLowerInvariant()`.
    3.  Tạo ra tên tệp ngẫu nhiên thông qua UUID:
        `string uniqueFileName = $"{Guid.NewGuid()}{extension}";`
    4.  Điều này cô lập hoàn toàn tên tệp gốc, triệt tiêu khả năng chèn ký tự điều hướng thư mục `..` hoặc `/`.

### B. Kiểm tra chữ ký tệp tin (Magic Numbers / File Signatures Verification)
*   **Rủi ro:** Kẻ tấn công đổi đuôi file mã độc từ `webshell.asp` hoặc `malware.exe` thành `webshell.jpg` hòng qua mặt bộ lọc kiểm tra đuôi mở rộng (Extension Bypass).
*   **Giải pháp xử lý nâng cao:** Ở các hệ thống lớn hoặc khi cần bảo mật tối đa, ta kiểm tra các byte đầu tiên (Header Bytes) của tệp tin. Dưới đây là bảng Magic Numbers được áp dụng:

| Định dạng ảnh | Chuỗi Byte Header (Hex) | Diễn giải |
| :--- | :--- | :--- |
| **PNG** | `89 50 4E 47 0D 0A 1A 0A` | Header chuẩn của Portable Network Graphics |
| **JPEG / JPG** | `FF D8 FF` | Header chuẩn của Joint Photographic Experts Group |
| **GIF** | `47 49 46 38` (GIF8) | Header chuẩn của Graphics Interchange Format |

#### Mã nguồn C# kiểm tra chữ ký ảnh (Ví dụ tích hợp kiểm thử):
```csharp
public static bool ValidateFileSignature(Stream fileStream)
{
    using (var reader = new BinaryReader(fileStream, System.Text.Encoding.UTF8, true))
    {
        if (fileStream.Length < 8) return false;
        
        var bytes = reader.ReadBytes(8);
        fileStream.Position = 0; // Reset lại con trỏ stream

        // Kiểm tra PNG
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;
        // Kiểm tra JPEG
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return true;
        // Kiểm tra GIF
        if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38) return true;
        
        return false;
    }
}
```

---

## 2. Logic Cập nhật thông tin trong Database (UserService Integration)

Sau khi tệp hình ảnh được ghi vật lý lên đĩa lưu trữ của server thành công, đường dẫn tương đối (ví dụ: `/uploads/avatars/5f4e3d2c_my-cat.png`) sẽ được cập nhật vào trường `Avatar` của người dùng.

### C# Code Reference: `UserService.cs` (Cập nhật Avatar)
```csharp
public async Task<bool> UpdateAvatarAsync(Guid userId, string avatarPath)
{
    // 1. Lấy thông tin người dùng từ Repository
    var user = await _userRepository.GetUserByIdAsync(userId);
    
    // 2. Kiểm tra tính tồn tại (Không rỗng & chưa bị xóa mềm)
    if (user == null) 
    {
        return false;
    }

    // 3. Ghi nhận đường dẫn ảnh đại diện mới
    user.Avatar = avatarPath;

    // 4. Lưu thay đổi
    await _userRepository.UpdateUserAsync(user);
    await _userRepository.SaveChangesAsync();

    return true;
}
```

---

## 3. Tối ưu hóa dung lượng lưu trữ (Storage Clean Up Strategy)
*   **Vấn đề:** Khi người dùng thay đổi ảnh đại diện liên tục, các tệp ảnh cũ không còn được sử dụng sẽ tồn tại vô thời hạn trong thư mục `/uploads/avatars/`, gây lãng phí tài nguyên ổ đĩa cứng của máy chủ.
*   **Giải pháp đề xuất:**
    *   Hệ thống khuyến nghị lưu giữ lịch sử hoặc sử dụng một Background Worker (Quartz.NET hoặc HostedService) quét cơ sở dữ liệu và thư mục vật lý định kỳ (ví dụ: 0h00 Chủ nhật hàng tuần).
    *   Mọi tệp tin trong thư mục `/uploads/avatars/` không có liên kết tương ứng với trường `Avatar` của bất cứ dòng nào trong bảng `Users` sẽ được tự động xóa bỏ hoàn toàn nhằm giải phóng bộ nhớ lưu trữ.
