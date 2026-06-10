# 🌐 Infrastructure & Security Specification - Vets Team (Phase 1)

Tài liệu này đặc tả cấu hình định tuyến API công khai (Public Access), cấu trúc IMemoryCache tại Middleware và các biện pháp bảo mật chống dò quét tải dữ liệu (Scraping Protection).

---

## 1. Cơ chế Truy cập Công khai (Anonymous Public Access policy)

Danh mục bác sĩ là thông tin công khai giúp tiếp thị và cung cấp thông tin minh bạch cho khách hàng vãng lai.
*   **Cấu hình API Controller:** Endpoint `/api/doctors` được thiết lập thuộc tính **`[AllowAnonymous]`** trong ASP.NET Core:
    *   *Hành vi:* Bỏ qua Middleware xác thực JWT Bearer Token, cho phép tất cả các HTTP GET Request truy cập trực tiếp mà không cần Header Authorization.
*   **Cấu hình CORS:** Chỉ cho phép phương thức `GET` từ các Client bên ngoài truy vấn danh mục bác sĩ. Chặn đứng các phương thức thay đổi dữ liệu như `POST`, `PUT`, `DELETE` từ nguồn gốc lạ.

---

## 2. Thiết lập Bộ đệm IMemoryCache trong ASP.NET Core Middleware

Để kích hoạt dịch vụ `IMemoryCache` trong ứng dụng Backend:

### 2.1. Đăng ký Dịch vụ (`Program.cs`)
```csharp
var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ bộ đệm mặc định của .NET Core
builder.Services.AddMemoryCache();

builder.Services.AddControllers();
```

### 2.2. Các thông số cấu hình tối ưu hiệu năng
*   `AbsoluteExpiration`: **60 phút** (sau 1 giờ dữ liệu bắt buộc phải giải phóng và truy vấn lại DB).
*   `SlidingExpiration`: **15 phút** (nếu trong 15 phút không phát sinh bất kỳ lượt truy cập API nào, cache tự động giải phóng bộ nhớ RAM).
*   `CacheItemPriority.High`: Đảm bảo khi máy chủ gặp trạng thái cạn kiệt tài nguyên bộ nhớ (Low Memory), Garbage Collector của .NET sẽ ưu tiên giữ lại danh sách bác sĩ và dọn dẹp các cache kém quan trọng hơn.

---

## 3. Lá chắn chống cào quét dữ liệu hàng loạt (Scraping Protection)

Do API là công khai, các đối thủ cạnh tranh có thể viết robot hoặc script tự động để quét liên tục danh sách nhân sự của phòng khám. Hệ thống áp dụng 2 lá chắn:

### 3.1. Rate Limiting cho API Công khai
*   *Hạn mức:* **Tối đa 60 requests/phút trên mỗi địa chỉ IP**.
*   *Hành vi:* Vượt hạn mức sẽ trả về lỗi `HTTP 429 Too Many Requests`. Hạn mức này dư dả cho người dùng thật duyệt trang web nhưng đủ để chặn đứng các script cào dữ liệu nhanh.

### 3.2. Cấu hình CDN Caching (Cloudflare Integration)
*   Trong môi trường Production thực tế, API `/api/doctors` được cấu hình đi qua CDN Cloudflare với thiết lập **Edge Cache TTL: 2 giờ**.
*   *Mục đích:* Khi khách hàng truy vấn danh sách bác sĩ, yêu cầu sẽ được trả về trực tiếp từ Server CDN gần nhất của Cloudflare mà không cần truyền request tới Server gốc của MyPetClinic, giảm tải 99% áp lực mạng và bảo vệ CPU máy chủ.
