# 🐾 CS-04: PostgreSQL & EF Core Basics (Quản trị & Truy vấn Database)

Dự án **MyPetClinic** sử dụng hệ quản trị cơ sở dữ liệu **PostgreSQL** đặt trên nền tảng điện toán đám mây **Supabase Cloud**. Tài liệu này hướng dẫn cách kết nối, truy vấn dữ liệu trực quan và cách tương tác với database thông qua Entity Framework Core (EF Core).

---

## 1. Hướng dẫn Kết nối Database bằng Công cụ trực quan

### Các thông tin kết nối (Connection String)
Thông tin kết nối nằm trong file [appsettings.json](file:///e:/DATN/MyPetClinic/backend/src/WebApi/appsettings.json) của dự án WebApi:
- **Host:** `aws-1-ap-south-1.pooler.supabase.com`
- **Port:** `5432`
- **Database:** `postgres`
- **Username:** `postgres.yzbacphijkzkjahlsbdz`
- **Password:** `cogangviphuonglamhanh`

### Hướng dẫn sử dụng DBeaver / pgAdmin để kết nối:
1. Tải và cài đặt **DBeaver Community** hoặc **pgAdmin**.
2. Chọn tạo kết nối mới, chọn kiểu database là **PostgreSQL**.
3. Điền thông tin Host, Port, Database, Username, Password như trên.
4. Ở tab SSL/Connection, đảm bảo kích hoạt chế độ **Require SSL** nếu cần. Nhấn **Test Connection** để kiểm tra, sau đó lưu lại.
5. Giờ bạn có thể mở cây thư mục bên trái: `postgres` -> `Schemas` -> `public` -> `Tables` để xem toàn bộ danh sách các bảng như `Pets`, `Bookings`, `Users`, `Medicines`, v.v.

---

## 2. Các Câu lệnh SQL Cơ bản cần dùng khi Test Dự án

QA (Lâm, Hạnh) hoặc Frontend (Phương) có thể mở **Query Tool** trong DBeaver để chạy các câu lệnh kiểm tra dữ liệu thực tế nhằm xác nhận tính năng chạy đúng:

### Đọc dữ liệu (SELECT & JOIN)
```sql
-- Lấy tất cả thú cưng của một chủ nuôi có tên là 'Nguyễn Văn A'
SELECT p."Id", p."Name", p."Species", p."Age", u."FullName"
FROM "Pets" p
JOIN "Users" u ON p."OwnerId" = u."Id"
WHERE u."FullName" = 'Nguyễn Văn A';

-- Đếm số lượng đặt lịch (Bookings) theo từng trạng thái (Status)
SELECT "Status", COUNT(*) as "Total"
FROM "Bookings"
GROUP BY "Status";

-- Xem doanh thu hóa đơn đã thanh toán của ngày hôm nay
SELECT SUM("TotalAmount") as "DoanhThuHocNay"
FROM "Invoices"
WHERE DATE("CreatedAt") = CURRENT_DATE AND "Status" = 'Paid';
```

### Thêm dữ liệu mẫu (INSERT)
```sql
-- Thêm một loại thuốc mới để test kê đơn
INSERT INTO "Medicines" ("Id", "Name", "Unit", "Price", "StockQuantity", "CreatedAt")
VALUES (
    gen_random_uuid(), 
    'Thuốc Tẩy Giun NexGard', 
    'Viên', 
    120000, 
    50, 
    NOW()
);
```

---

## 3. Quản lý thay đổi cấu trúc Database với EF Core Migrations

Hệ thống sử dụng cơ chế **Code-First** của EF Core. Mọi thay đổi về cấu trúc bảng (thêm cột, tạo bảng mới) sẽ được viết bằng code C# trước, sau đó đồng bộ hóa với Database PostgreSQL thông qua các lệnh Migration.

### Khi Nam cập nhật Database, bạn cần làm gì?
Khi bạn kéo code mới từ nhánh `develop` về và thấy có các file Migration mới, bạn cần cập nhật cấu trúc database ở máy mình bằng cách chạy lệnh sau trong terminal tại thư mục [backend/src](file:///e:/DATN/MyPetClinic/backend/src) (hoặc thư mục gốc backend):

```bash
# Cập nhật database local (hoặc database dùng chung) khớp với code mới nhất
dotnet ef database update --project MyPetClinic.Infrastructure --startup-project WebApi
```

*(Lưu ý: Bạn cần cài đặt công cụ `dotnet-ef` bằng lệnh: `dotnet tool install --global dotnet-ef` nếu terminal báo lỗi không nhận lệnh).*

---

## 4. Bài tập Thực Hành Đạt Yêu Cầu CS-04

- [ ] Kết nối thành công công cụ DBeaver/pgAdmin vào database Supabase của dự án.
- [ ] Thực hiện thành công câu lệnh SELECT JOIN 3 bảng: `Bookings`, `Pets`, và `Users` để xem thông tin chi tiết một lịch khám.
- [ ] Chạy lệnh `dotnet ef database update` thành công để đồng bộ cơ sở dữ liệu khi có thay đổi code.
- [ ] Viết câu lệnh SQL cập nhật trạng thái của một hóa đơn từ `Unpaid` sang `Paid` để phục vụ việc test giao diện.
