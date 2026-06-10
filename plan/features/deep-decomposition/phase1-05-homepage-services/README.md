# 🏥 DANH MỤC DỊCH VỤ TRANG CHỦ (HOMEPAGE SERVICES)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - 05)

Chào mừng bạn đến với tài liệu kỹ thuật chi tiết nhất về phân hệ **Danh mục Dịch vụ Trang chủ** của ứng dụng phòng khám thú y **MyPetClinic**. Phân hệ này cung cấp giao diện công khai trực quan giới thiệu các gói dịch vụ khám bệnh, tiêm phòng, phẫu thuật, spa thú cưng kèm bảng giá minh bạch, tích hợp cơ chế lưu đệm Memory Cache tăng tốc độ tải trang cực đỉnh và bộ lọc tìm kiếm tức thời ở Client.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU CHI TIẾT

Tài liệu thiết kế chi tiết được phân rã thành các tệp chuyên biệt dưới đây để đảm bảo tính độc lập và quản lý cấu trúc dễ bảo trì:

1.  **[Product Requirements Document (PRD)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/PRD.md)**
    *   Mục tiêu nghiệp vụ, chân dung người dùng (Vy tìm spa mèo, Hùng tìm dịch vụ cấp cứu), User Stories.
    *   Phạm vi phát triển (In-Scope/Out-of-Scope) và các yêu cầu phi chức năng (NFRs) về thời gian và độ co giãn lưới responsive.
2.  **[Technical Specification](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/TECHNICAL_SPEC.md)**
    *   Sơ đồ luồng xác thực tuần tự (Sequence Diagram) tích hợp bộ đệm Memory Cache.
    *   Lược đồ cơ sở dữ liệu `Services` & `ServiceCategories` PostgreSQL, các DTOs validation.
3.  **[Core Business Logic Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/01-core-logic.md)**
    *   Mã nguồn C# Service Service xử lý IMemoryCache, Sliding và Absolute Expiration.
    *   Chiến lược dọn dẹp bộ đệm chủ động (Cache Eviction Policy) đồng bộ bảng giá khi cập nhật.
4.  **[UI/UX Design Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/02-ui-ux.md)**
    *   Sơ đồ cấu trúc lưới và Tab lọc dạng **ASCII Art Mockup**.
    *   Thiết kế thẻ Card Glassmorphism, hiệu ứng Elevate hover, mã nguồn CSS Shimmer Skeleton Loading.
5.  **[State Management (Pinia)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/03-state-management.md)**
    *   Mã nguồn TypeScript Pinia Store (`useServicesStore.ts`) quản lý state.
    *   Bộ lọc Client-side memory filtering tối ưu hóa hiệu năng phản hồi và bảo vệ CPU server.
6.  **[Infrastructure & Security](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/04-infrastructure.md)**
    *   Cấu hình cổng API công khai (Public Access), cấu hình IMemoryCache Registry trong Startup.
    *   Các lá chắn bảo mật: IP Rate Limiting API công khai, giải pháp tích hợp CDN Cloudflare Edge Caching.
7.  **[API Reference Details](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/API_REFERENCE.md)**
    *   Đặc tả API Contract: chi tiết URL, HTTP Method, Request/Response payload mẫu GET services và các mã lỗi.
8.  **[Behavioral Specification (FSM)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/BEHAVIOR_SPEC.md)**
    *   Mô tả máy trạng thái hữu hạn bằng sơ đồ Mermaid và diễn giải các bước nạp/lọc.
    *   Xử lý lỗi biên (không có kết quả phù hợp, cơ chế chặn Auth Shield khi bấm đặt lịch).
9.  **[UX Flow Walks](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/UX_FLOW.md)**
    *   Hành trình trải nghiệm người dùng chi tiết từng bước (User Journey Map) với các hiệu ứng chuyển đổi mượt mà.
10. **[User & Developer Documentation](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/DOCUMENTATION.md)**
    *   Hướng dẫn duyệt bảng giá và đặt lịch nhanh cho người dùng, tài liệu debug đo lường hiệu năng Cache cho Dev.
11. **[Implementation Plan & Testing Strategy](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-05-homepage-services/plan.md)**
    *   Lộ trình 5 giai đoạn phát triển và bộ kiểm thử đơn vị (Unit Tests) thực tế kiểm chứng ghi/đọc Memory Cache.

---

## 🏛️ CƠ CHẾ LỌC LƯU ĐỆM KẾT HỢP (CLIENT-SIDE FILTER & MEMORY CACHE)

Để tối ưu hóa tài nguyên phần cứng hệ thống phòng khám, chúng ta thiết lập mô hình lưu đệm và lọc phân cấp vô cùng thông minh:

```
[Vue 3 Client Browser] ----(1. GET /api/services - Lượt đầu)----> [API Gateway]
       |                                                               |
       | (4. Tải và lưu List Dịch vụ về RAM Client)                    v
       v                                                     [IMemoryCache Server]
[Lọc RAM cục bộ qua computed] <---(3. Trả về List thô) <--- (Cache Hit từ RAM Server)
- Gõ từ khoá tìm kiếm                                                  ^
- Đổi Tab danh mục lọc                                                 | (2. Cache Miss mới đọc DB)
- Phản hồi tức thì <2ms                                         [PostgreSQL Database]
```
Mô hình kết hợp này đảm bảo PostgreSQL chỉ nhận truy vấn lấy dịch vụ đúng 1 lần mỗi giờ, mọi lượt click bộ lọc hay tìm kiếm của hàng ngàn khách hàng duyệt web đồng thời đều được xử lý cục bộ tại Client, đem lại hiệu năng phản hồi thần tốc.
