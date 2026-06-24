# PRD: Module Hồ Sơ Khách Hàng & Thú Cưng

## PHẦN 1. MỤC TIÊU NGHIỆP VỤ
### 1.1 Tại sao cần module Hồ sơ khách hàng & thú cưng?
Dữ liệu cốt lõi của phòng khám thú y là **Chủ nuôi (Customer)** và **Thú cưng (Pet)**. Khách hàng Walk-in đến trực tiếp thường trong tình trạng khẩn cấp hoặc không có nhu cầu tạo tài khoản online. Do đó, hệ thống bắt buộc phải quản lý được Hồ sơ Khách hàng độc lập với Tài khoản đăng nhập (Account).

### 1.2 Vai trò
Đây là **trái tim của hệ thống**, mọi nghiệp vụ khác (Khám bệnh, Tiêm chủng, Thu ngân) đều dựa trên Customer và Pet.

## PHẦN 2. PHÂN TÍCH ĐỐI TƯỢNG DỮ LIỆU
### 2.1 Customer (Chủ nuôi)
Thực thể trung tâm chịu trách nhiệm pháp lý, y tế và thanh toán. Tồn tại độc lập với hệ thống Web/App.

### 2.2 Pet (Thú cưng)
Gắn liền với Customer. Chứa lịch sử sinh lý, bệnh lý y khoa.

### 2.3 Account (Tài khoản)
Là định danh (Credentials) để đăng nhập Web/App. Một Customer có thể có 0 hoặc 1 Account.

## PHẦN 3. LUỒNG NGHIỆP VỤ WALK-IN
1. Tiếp đón -> Hỏi SĐT.
2. Tìm SĐT -> Không có -> Lễ tân tạo nhanh Customer & Pet (Không cần mật khẩu).
3. Đăng ký khám -> Sinh Appointment.
4. Bác sĩ khám -> Thanh toán -> Hoàn tất.

## PHẦN 4. AUTO-LINK TÀI KHOẢN (KHÁCH WALK-IN)
1. Khách về tải App, đăng ký bằng SĐT đã khám Walk-in.
2. Hệ thống phát hiện SĐT tồn tại ở `Customers`.
3. Gửi OTP -> Xác thực -> Tạo `Users` (Account) và gán `CustomerId`.
4. Khách đăng nhập thấy lại toàn bộ lịch sử bệnh án cũ.

## PHẦN 5. CHIẾN LƯỢC MIGRATION (ZERO DATA LOSS)
Để tách 3 bảng (Customer, Pet, Account) từ bảng Users cũ mà không mất dữ liệu:
- Tạo bảng `Customers`.
- Copy dữ liệu từ bảng `Users` sang bảng `Customers` bằng lệnh SQL, sử dụng chính xác GUID (Id) của Users làm Id cho Customers (**Đồng bộ ID**).
- Thay đổi khóa ngoại của bảng `Pets` trỏ sang `Customers`. Vì GUID khớp 100%, dữ liệu không bị lệch.
