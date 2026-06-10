# 🗺️ Product Backlog - Lộ Trình Phát Triển Dự Án MyPetClinic

Tài liệu này định nghĩa toàn bộ **Product Backlog** của hệ thống MyPetClinic, liệt kê các yêu cầu chức năng (User Story) kèm theo mức độ ưu tiên, vai trò sử dụng và mô tả sơ bộ.

---

## 📋 Danh Sách Product Backlog (PB)

| Mã PB | Phân Hệ (Epic) | Tên Yêu Cầu / User Story | Người dùng (Actor) | Độ Ưu Tiên | Mô Tả Tóm Tắt |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **PB01** | Authentication | Đăng ký tài khoản mới | Khách hàng | High | Đăng ký tài khoản chủ nuôi bằng email, số điện thoại và mật khẩu. |
| **PB02** | Authentication | Đăng nhập hệ thống | Tất cả | High | Xác thực tài khoản bằng email/mật khẩu, trả về JWT Token và phân quyền (Role). |
| **PB03** | Authentication | Khôi phục / Quên mật khẩu | Tất cả | Medium | Yêu cầu đổi mật khẩu mới qua mã OTP gửi tới email. |
| **PB04** | Homepage | Xem danh sách dịch vụ & bảng giá | Khách hàng/Khách vãng lai | High | Xem thông tin chi tiết các dịch vụ phòng khám cung cấp kèm mức giá tương ứng. |
| **PB05** | Homepage | Xem đội ngũ bác sĩ | Khách hàng/Khách vãng lai | Medium | Xem danh sách bác sĩ thú y, chuyên môn và kinh nghiệm làm việc. |
| **PB06** | Homepage | Xem bài viết / Cẩm nang sức khỏe | Khách hàng/Khách vãng lai | Low | Đọc các bài chia sẻ kinh nghiệm chăm sóc sức khỏe cho thú cưng. |
| **PB07** | Customer Portal | Quản lý thông tin cá nhân | Khách hàng | High | Cập nhật thông tin liên hệ, họ tên, số điện thoại, ảnh đại diện. |
| **PB08** | Customer Portal | Quản lý hồ sơ thú cưng | Khách hàng | High | CRUD thông tin các thú cưng của mình (tên, giống, tuổi, cân nặng, hình ảnh). |
| **PB09** | Booking | Đặt lịch khám bệnh trực tuyến | Khách hàng | Very High | Chọn thú cưng, chọn dịch vụ khám, chọn ngày/giờ và bác sĩ mong muốn để đặt hẹn. |
| **PB10** | Booking | Đặt lịch tiêm chủng trực tuyến | Khách hàng | Very High | Đặt hẹn lịch tiêm phòng kèm theo lựa chọn loại vaccine cần thiết. |
| **PB11** | Customer Portal | Quản lý & Theo dõi lịch hẹn | Khách hàng | High | Xem danh sách lịch hẹn đã đặt, trạng thái (Chờ duyệt, Xác nhận, Đã hủy). |
| **PB12** | Customer Portal | Xem lịch sử dịch vụ & bệnh án | Khách hàng | Medium | Xem lịch sử các ca khám trước đó của thú cưng, bao gồm chẩn đoán và đơn thuốc. |
| **PB13** | Customer Portal | Đánh giá dịch vụ phòng khám | Khách hàng | Medium | Gửi đánh giá số sao và nhận xét sau khi hoàn thành ca khám chữa bệnh. |
| **PB14** | AI Consultation | Trợ lý ảo AI Chatbot tư vấn | Khách hàng | Medium | Tích hợp Gemini AI tư vấn kiến thức y tế thú y cơ bản và sơ cứu khẩn cấp. |
| **PB15** | Receptionist Portal | Tiếp nhận khách hàng (Check-in) | Lễ tân | High | Xác nhận check-in cho khách có lịch hẹn trước hoặc thêm nhanh khách vãng lai (Walk-in). |
| **PB17** | Receptionist Portal | Xác nhận hoặc Hủy lịch hẹn | Lễ tân | Very High | Phê duyệt hoặc từ chối lịch hẹn trực tuyến của khách hàng (có ghi chú lý do hủy). |
| **PB18** | Receptionist Portal | Quản lý hàng đợi phòng khám | Lễ tân | High | Điều phối số thứ tự khám (Queue), phân bổ thú cưng vào các phòng khám của bác sĩ. |
| **PB19** | Receptionist Portal | Thanh toán hóa đơn | Lễ tân | Very High | Xuất hóa đơn tổng hợp tiền khám + tiền thuốc, xác nhận trạng thái thanh toán. |
| **PB20** | Clinical Portal | Xem danh sách & lịch trực bác sĩ | Bác sĩ | High | Xem danh sách thú cưng đang xếp hàng đợi khám theo số thứ tự của mình. |
| **PB21** | Clinical Portal | Xem hồ sơ & lịch sử thú cưng | Bác sĩ | High | Truy xuất nhanh thông tin chi tiết thú cưng và các bệnh án/mũi tiêm cũ. |
| **PB22** | Clinical Portal | Tiếp nhận ca khám bệnh | Bác sĩ | High | Nút chuyển trạng thái từ chờ khám sang "Đang khám" để ghi nhận ca điều trị. |
| **PB23** | Clinical Portal | Ghi nhận bệnh án & Kê đơn thuốc | Bác sĩ | Very High | Ghi triệu chứng, chẩn đoán bệnh và chọn thuốc kê toa từ kho (tự động trừ tồn kho). |
| **PB24** | Clinical Portal | Thực hiện tiêm phòng vaccine | Bác sĩ | High | Ghi nhận thông tin tiêm chủng: loại vaccine, số lô tiêm và ngày hoàn thành. |
| **PB25** | Clinical Portal | Cập nhật hoàn thành ca khám | Bác sĩ | Medium | Chuyển trạng thái ca khám thành hoàn thành để chuyển sang cổng thanh toán. |
| **PB26** | Admin Portal | Quản lý dịch vụ phòng khám | Admin | High | CRUD các dịch vụ khám chữa bệnh của phòng khám. |
| **PB27** | Admin Portal | Quản lý danh mục dịch vụ | Admin | High | CRUD các nhóm danh mục dịch vụ lớn (Khám bệnh, Spa, Tiêm phòng...). |
| **PB28** | Admin Portal | Quản lý tài khoản & Phân quyền | Admin | High | Quản lý danh sách tài khoản người dùng, phân quyền vai trò nhân viên. |
| **PB29** | Admin Portal | Quản lý kho thuốc & Vật tư | Admin | High | CRUD thông tin thuốc, số lượng tồn kho, cảnh báo thuốc sắp hết hạn/hết hàng. |
| **PB30** | Admin Portal | Quản lý lịch làm việc của Bác sĩ | Admin | High | Thiết lập lịch trực tuần/tháng cho từng bác sĩ thú y. |
| **PB31** | Admin Portal | Cấu hình khung giờ đặt lịch (Slot)| Admin | High | Thiết lập số lượng lịch hẹn tối đa cho mỗi khung giờ khám bệnh. |
| **PB32** | Admin Portal | Quản lý bài viết & Đánh giá | Admin | Medium | Phê duyệt/ẩn bài viết cẩm nang hoặc phản hồi/ẩn các đánh giá xấu của khách hàng. |
| **PB33** | Admin Portal | Báo cáo & Thống kê doanh thu | Admin | Very High | Xem doanh thu chi tiết theo ngày, tháng, năm, thống kê dịch vụ ưa chuộng nhất. |
| **PB34** | Notification | Tự động gửi nhắc nhở tiêm chủng | Hệ thống | Medium | Background job tự động gửi email nhắc lịch tái chủng vaccine trước 3-5 ngày. |

---

## 📈 Lộ Trình Phát Triển & Bản Phát Hành (Releases)

Dự án được phân chia thành **3 Giai đoạn Phát hành (Release)** chính để đảm bảo tính sẵn sàng cao:

1. **Release 1: Nền tảng & Xác thực (Sprint 1 - 2):**
   * Hoàn thành khung kiến trúc, đăng ký/đăng nhập và hồ sơ thú cưng.
2. **Release 2: Nghiệp Vụ Cốt Lõi MVP (Sprint 3 - 5):**
   * Đặt lịch trực tuyến, tiếp nhận hàng đợi, bác sĩ chẩn đoán/kê đơn và lễ tân thanh toán hóa đơn.
3. **Release 3: Trí Tuệ Nhân Tạo & Quản Trị Nâng Cao (Sprint 6):**
   * Tích hợp AI chatbot tư vấn sức khỏe, các báo cáo thống kê doanh thu trực quan cho Admin và tối ưu hệ thống.
