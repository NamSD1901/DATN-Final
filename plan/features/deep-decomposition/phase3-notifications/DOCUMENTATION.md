# 📄 User & Dev Documentation - Automatic Notification Service

## 1. Cấu trúc Database SQL cho Thông báo (Notifications Table)

Lập trình viên chạy tập lệnh SQL sau để khởi tạo bảng thông báo hệ thống:

```sql
CREATE TABLE "Notifications" (
    "Id" UUID PRIMARY KEY,
    "CustomerId" UUID NOT NULL FOREIGN KEY REFERENCES "Customers"("Id"),
    "Title" VARCHAR(255) NOT NULL,
    "Message" TEXT NOT NULL,
    "IsRead" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL
);
```

---

## 2. Hướng dẫn Cấu hình SMTP Gmail làm Mailer Server
1. Truy cập tài khoản Google cá nhân.
2. Bật xác thực 2 lớp (2-Step Verification) nếu chưa bật.
3. Vào phần **App Passwords** (Mật khẩu ứng dụng).
4. Tạo mật khẩu ứng dụng mới với tên ứng dụng là `MyPetClinic`.
5. Sao chép chuỗi mật khẩu 16 ký tự được cấp và điền vào tham số `Smtp:Password` trong tệp biến cấu hình hệ thống:
   - `Smtp:Host = smtp.gmail.com`
   - `Smtp:Port = 587`
   - `Smtp:Username = email-cua-ban@gmail.com`
   - `Smtp:Password = mật-khẩu-ứng-dụng-16-ký-tự`
