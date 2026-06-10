# 🚀 TẢI LÊN ẢNH ĐẠI DIỆN (PROFILE AVATAR UPLOAD)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - FEATURE 08)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Tải lên ảnh đại diện cá nhân (Avatar Upload)** của người dùng trong hệ thống quản lý phòng khám thú y **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Đặc tả mục tiêu kinh doanh, chân dung người dùng (User Personas), câu chuyện người dùng (User Stories) kèm tiêu chí nghiệm thu (Acceptance Criteria), phạm vi MVP và các yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Đặc tả kiến trúc kỹ thuật bao gồm sơ đồ tuần tự (Sequence Diagram) tương tác tải file nhị phân, cấu trúc thư mục lưu trữ tĩnh và bộ lọc dữ liệu đầu vào. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Logic Reference** | Logic phòng chống tấn công tải file mã độc (webshell), cơ chế chống ghi đè Path Traversal bằng UUID, và giải pháp dọn dẹp dung lượng lưu trữ trên server. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế vùng kéo thả Dropzone, sơ đồ ASCII Mockup cho trạng thái chờ & tải lên, bảng màu CSS HSL Tokens và hoạt động của thanh phần trăm tiến trình upload. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store bằng TypeScript (`useAvatarStore`) đo lường tiến trình tải lên Axios `onUploadProgress` thời gian thực và đồng bộ dữ liệu. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cấu hình máy chủ phục vụ tệp tĩnh, cấu hình Kestrel/IIS giới hạn cứng dung lượng request tối đa 2MB, và Rate Limiting chống spam tải tệp. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Tài liệu đặc tả API Contracts cho endpoint POST upload avatar kèm mẫu dữ liệu JSON thành công/thất bại chi tiết (400, 401, 429, 500). | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid mô tả chu kỳ tương tác kéo thả tệp tin và cơ chế giải phóng Object URL tránh rò rỉ RAM. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm tương tác từ rê chuột hiển thị biểu tượng Camera đến đóng cửa sổ tự động. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn sử dụng nhanh dành cho khách hàng/nhân viên, lệnh kiểm thử nhanh `curl` API và giải pháp sửa lỗi cache ảnh của trình duyệt. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai nhỏ (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) phục vụ QA kiểm thử biên dung lượng, bảo mật Webshell và Path Traversal. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Tính năng tải lên ảnh đại diện sau khi nâng cấp tài liệu và mã nguồn phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1.  **Bảo mật tải file tuyệt đối:** Cấm hoàn toàn đuôi tệp thực thi nguy hiểm, bắt buộc đổi tên file sang UUID để chống ghi đè tệp tin hệ thống.
2.  **Trực quan hóa tiến trình:** Hiển thị thanh tiến trình phần trăm thực tế dựa trên luồng truyền tải dữ liệu của Axios để nâng cao trải nghiệm người dùng.
3.  **Tối ưu tài nguyên:** Giải phóng Blob Object URL cục bộ ngay khi kết thúc vòng đời tương tác, kiểm soát chặt chẽ dung lượng file tối đa dưới 2MB.
