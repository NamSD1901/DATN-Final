# 🏥 ĐỘI NGŨ BÁC SĨ TRANG CHỦ (HOMEPAGE VETS TEAM)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - 06)

Chào mừng bạn đến với tài liệu kỹ thuật chi tiết nhất về phân hệ **Đội ngũ Bác sĩ Trang chủ** của ứng dụng phòng khám thú y **MyPetClinic**. Phân hệ này cung cấp giao diện công khai trực quan giới thiệu các bác sĩ chuyên khoa y tế thú y kèm số năm kinh nghiệm, tiểu sử chuyên môn, tích hợp cơ chế lưu đệm Memory Cache và bộ lọc chuyên khoa tức thời ở Client.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU CHI TIẾT

Tài liệu thiết kế chi tiết được phân rã thành các tệp chuyên biệt dưới đây để đảm bảo tính độc lập và dễ bảo trì:

1.  **[Product Requirements Document (PRD)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/PRD.md)**
    *   Mục tiêu nghiệp vụ, chân dung người dùng (Vy tìm bác sĩ da liễu cho mèo, Chú Tuấn tìm phẫu thuật viên), User Stories.
    *   Phạm vi phát triển (In-Scope/Out-of-Scope) và các yêu cầu phi chức năng (NFRs) về độ phân giải hình ảnh chân dung.
2.  **[Technical Specification](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/TECHNICAL_SPEC.md)**
    *   Sơ đồ luồng xác thực tuần tự (Sequence Diagram) tích hợp bộ đệm Memory Cache.
    *   Lược đồ cơ sở dữ liệu `Doctors` PostgreSQL liên kết 1-1 với bảng `Users`, các DTOs validation.
3.  **[Core Business Logic Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/01-core-logic.md)**
    *   Mã nguồn C# Doctor Service xử lý IMemoryCache, Sliding và Absolute Expiration.
    *   Chiến lược dọn dẹp bộ đệm chủ động (Cache Eviction Policy) khi bác sĩ đổi ca trực.
4.  **[UI/UX Design Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/02-ui-ux.md)**
    *   Sơ đồ cấu trúc lưới thẻ bác sĩ dạng **ASCII Art Mockup** có pulse badge nhấp nháy chỉ thị ca trực.
    *   Thiết kế thẻ Card Glassmorphism, hiệu ứng Zoom hover chân dung, mã nguồn CSS Pulse nhấp nháy.
5.  **[State Management (Pinia)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/03-state-management.md)**
    *   Mã nguồn TypeScript Pinia Store (`useDoctorsStore.ts`) quản lý state.
    *   Bộ lọc Client-side memory filtering tối ưu hóa hiệu năng phản hồi và đồng bộ đặt lịch khám nhanh.
6.  **[Infrastructure & Security](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/04-infrastructure.md)**
    *   Chính sách truy cập công khai (AllowAnonymous), cấu hình cache và giải pháp chống cào quét dữ liệu (Rate Limiting, CDN Cloudflare Caching).
7.  **[API Reference Details](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/API_REFERENCE.md)**
    *   Đặc tả API Contract: chi tiết URL, HTTP Method, Request/Response payload mẫu GET doctors và các mã lỗi.
8.  **[Behavioral Specification (FSM)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/BEHAVIOR_SPEC.md)**
    *   Mô tả máy trạng thái hữu hạn bằng sơ đồ Mermaid và diễn giải các bước nạp/lọc.
    *   Xử lý lỗi biên (bác sĩ nghỉ phép off-duty, cơ chế chặn Auth Shield khi bấm đặt lịch).
9.  **[UX Flow Walks](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/UX_FLOW.md)**
    *   Hành trình trải nghiệm người dùng chi tiết từng bước (User Journey Map) với các hiệu ứng chuyển đổi mượt mà.
10. **[User & Developer Documentation](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/DOCUMENTATION.md)**
    *   Hướng dẫn duyệt thông tin bác sĩ cho người dùng, tài liệu debug đo lường hiệu năng Cache cho Dev.
11. **[Implementation Plan & Testing Strategy](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-06-homepage-doctors/plan.md)**
    *   Lộ trình 5 giai đoạn phát triển và bộ kiểm thử đơn vị (Unit Tests) thực tế kiểm chứng ghi/đọc Memory Cache.

---

## 🏛️ CƠ CHẾ ĐỒNG BỘ ĐẶT LỊCH NHANH (FAST-BOOKING SYNC)

Khi người dùng click đặt lịch trực tiếp với một bác sĩ cụ thể từ danh sách trang chủ, hệ thống cung cấp một luồng trải nghiệm vô cùng trơn tru:

```
[Click Đặt lịch BS. Vy] ---> [Mở Booking Modal] ---> [Tự động điền DoctorId = Vy]
                                                               |
                                                  (Khoá trường lựa chọn Bác sĩ)
                                                               |
                                     +-------------------------+-------------------------+
                                     |                                                   |
                     [Tự động điền Chuyên khoa]                                [Mở khoá chọn ngày/giờ]
                     - Lọc trước Dịch vụ khám                                  - Người dùng chọn lịch
                     - Đảm bảo đúng chuyên môn                                  - Đặt lịch thành công
```
Cơ chế đồng bộ này giúp loại bỏ hoàn toàn các bước thao tác thừa, dẫn dắt khách hàng hoàn tất đặt lịch nhanh gọn nhất.
