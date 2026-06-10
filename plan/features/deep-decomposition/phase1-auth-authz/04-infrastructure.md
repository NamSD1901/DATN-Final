# 🌐 Infrastructure & Security - Authentication & Authorization

## 1. Cấu hình CORS & HTTPS (Cross-Origin Resource Sharing)

Hạ tầng bảo vệ truyền tải của **MyPetClinic** được cấu hình để chống tấn công nghe lén và giả mạo trên đường truyền mạng:

*   **Bắt buộc HTTPS:** Kênh truyền thông giữa SPA Client và Web API phải được mã hóa bằng chuẩn TLS 1.3. 
    *   Sử dụng lệnh `app.UseHttpsRedirection()` trong ASP.NET Core để tự động chuyển hướng mọi luồng request từ HTTP (cổng 80) sang HTTPS (cổng 443).
    *   Kích hoạt chính sách **HSTS (HTTP Strict Transport Security)** trong môi trường Production để ép trình duyệt chỉ sử dụng HTTPS cho các kết nối trong tương lai.
*   **Chính sách CORS nghiêm ngặt:** Chỉ cho phép Client SPA đáng tin cậy truy xuất tài nguyên hệ thống, ngăn chặn các script độc hại chạy từ domain khác.
    *   **Môi trường Development:** Chấp nhận cổng của Vue (`http://localhost:5173`).
    *   **Môi trường Production:** Chỉ chấp nhận domain chính thức của phòng khám (Ví dụ: `https://mypetclinic.com`).
    *   **Cấu hình C# CORS Policy:**
        ```csharp
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:5173", "https://mypetclinic.com")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Hỗ trợ gửi Cookie HttpOnly
            });
        });
        ```

---

## 2. Bảo mật Cookies & Tokens (Token Security Settings)

Để chống lại các đòn tấn công phổ biến như **XSS (Cross-Site Scripting)** và **CSRF (Cross-Site Request Forgery)**, hạ tầng Web API và SPA phối hợp cấu hình:

*   **Cookie Security Flags:** Nếu hệ thống sử dụng Refresh Token dạng Cookie, các cờ bảo mật bắt buộc phải được kích hoạt:
    *   `HttpOnly`: Chặn đứng các đoạn mã Javascript (`document.cookie`) đọc giá trị token, bảo vệ phiên đăng nhập khỏi lỗ hổng XSS.
    *   `Secure`: Chỉ truyền tải cookie qua kết nối mã hóa HTTPS.
    *   `SameSite = SameSiteMode.Strict` (hoặc `Lax`): Ngăn chặn trình duyệt tự động đính kèm cookie vào các request xuất phát từ website bên ngoài, ngăn chặn hoàn toàn tấn công CSRF.
*   **Cấu hình JWT Bearer Middleware (.NET 8+):**
    ```csharp
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // Kiểm tra ngày exp của token
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecurityKey)),
            ClockSkew = TimeSpan.Zero // Chặn hoàn toàn thời gian trễ đồng hồ
        };
    });
    ```

---

## 3. Chính sách giới hạn tần suất (Rate Limiting & Lockout Policy)

Để đối phó với cuộc tấn công dò quét mật khẩu hàng loạt (Brute-Force) và từ chối dịch vụ (DoS) tại cổng đăng nhập, hệ thống thiết lập hai rào cản:

### A. Rate Limiting trên IP cho API Xác thực
*   **Giới hạn:** Tối đa **5 lượt đăng nhập hoặc đăng ký / 1 phút / 1 địa chỉ IP**.
*   **Hình phạt:** Trả về mã lỗi `429 Too Many Requests` ngay lập tức nếu vượt hạn mức.

### B. Cơ chế Khóa tài khoản tạm thời (Lockout Policy)
*   Nếu một tài khoản email bị đăng nhập sai mật khẩu quá **5 lần liên tiếp**:
    *   Hệ thống tự động chuyển trạng thái tài khoản thành tạm khóa.
    *   Lưu thời điểm mở khóa `LockoutEnd = DateTime.UtcNow.AddMinutes(15)`.
    *   Mọi yêu cầu đăng nhập vào tài khoản này trong thời gian 15 phút khóa đều nhận phản hồi `400 Bad Request` kèm thông báo *"Tài khoản đã bị tạm khóa do nhập sai nhiều lần. Vui lòng thử lại sau 15 phút."*.
