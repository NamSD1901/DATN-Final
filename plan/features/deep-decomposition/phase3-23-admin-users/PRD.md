# 🚀 Product Requirements Document (PRD) - Admin Staff Management

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Phân hệ **Quản trị Nhân sự & Phân quyền (Admin Staff Management & Role Authorization)** là công cụ cốt lõi giúp Quản trị viên (Admin) thiết lập và duy trì trật tự an ninh hệ thống trong phòng khám MyPetClinic. Tính năng này cho phép quản lý vòng đời nhân sự của phòng khám: từ lúc tuyển dụng thêm mới tài khoản, phân chia vai trò chuyên môn (Bác sĩ, Lễ tân, Thu ngân), cập nhật thông tin làm việc, cho đến việc khóa tài khoản khẩn cấp khi nhân viên thôi việc.

Mục tiêu chính là phân ranh giới quyền hạn rõ ràng (RBAC), ngăn chặn tuyệt đối lỗi rò rỉ dữ liệu y khoa hoặc tài chính do nhân viên truy cập vượt quyền, đồng thời bảo vệ hệ thống trước các nguy cơ tấn công leo thang đặc quyền.

---

## 2. Đối tượng sử dụng (Target Personas)

### 🧑‍💼 Quản trị viên hệ thống - Anh Khánh (38 tuổi)
- **Mô tả:** Khánh là chủ chuỗi phòng khám thú y kiêm quản trị viên tối cao của hệ thống. Anh chịu trách nhiệm vận hành chung, tuyển dụng và cấp tài khoản cho các bác sĩ mới ra trường hoặc nhân viên thu ngân mới vào làm.
- **Nỗi đau (Pain points):**
  - Không muốn nhân viên tự ý đổi vai trò hoặc cấp quyền cho nhau.
  - Khi một nhân viên nghỉ việc đột ngột, anh cần khóa tài khoản của họ ngay lập tức để họ không thể đăng nhập từ xa vào xem thông tin khách hàng hoặc đơn thuốc của phòng khám.
  - Sợ cấp mật khẩu mặc định quá đơn giản dẫn đến tài khoản nhân viên bị hacker tấn công dò quét mật khẩu (brute-force).
- **Mong muốn:** Một màn hình quản trị nhân sự tập trung, hiển thị danh sách nhân viên trực quan, cho phép thao tác đổi vai trò, khóa tài khoản tức thì và tự động sinh mật khẩu ngẫu nhiên độ bảo mật cao.

### 🧑‍⚕️ Bác sĩ thú y mới - Anh Đức (26 tuổi)
- **Mô tả:** Đức mới được tuyển vào phòng khám. Anh được cấp một tài khoản nhân viên để bắt đầu ghi chép bệnh án thú cưng.
- **Nỗi đau (Pain points):**
  - Không muốn sử dụng mật khẩu mặc định dễ đoán.
  - Cần hệ thống bắt buộc đổi mật khẩu mới trong lần đầu tiên đăng nhập để đảm bảo tính riêng tư.
- **Mong muốn:** Nhận được email chứa thông tin đăng nhập và mật khẩu tạm thời bảo mật, dễ dàng đổi mật khẩu khi đăng nhập lần đầu.

---

## 3. User Stories & Tiêu chí Nghiệm thu (Acceptance Criteria)

### Story 1: Thêm mới tài khoản nhân viên và sinh mật khẩu tạm thời an toàn
> **Là một** Quản trị viên hệ thống,  
> **Tôi muốn** thêm mới thông tin nhân viên (Họ tên, Email, Số điện thoại, Vai trò) và tự động sinh một mật khẩu mặc định an toàn gửi cho họ,  
> **Để** khởi tạo môi trường làm việc bảo mật cho nhân viên mới.

#### Tiêu chí Nghiệm thu (AC):
- **AC 1.1:** Admin nhập đầy đủ thông tin nhân viên gồm: Họ tên, Email (phải duy nhất), Số điện thoại, Vai trò chuyên môn (`doctor`, `receptionist`, `cashier`, `admin`).
- **AC 1.2:** Hệ thống tự động sinh mật khẩu tạm thời ngẫu nhiên có độ dài tối thiểu 12 ký tự, chứa ít nhất: 1 chữ hoa, 1 chữ thường, 1 chữ số, và 1 ký tự đặc biệt.
- **AC 1.3:** Tài khoản nhân viên mới tạo sẽ có thuộc tính `RequirePasswordChange = true` và trạng thái `PendingActivation`.
- **AC 1.4:** Hệ thống gửi một email chào mừng chứa link đăng nhập và thông tin tài khoản kèm mật khẩu tạm thời đến email nhân viên (sử dụng dịch vụ MailKit/SMTP bảo mật).

### Story 2: Phân quyền vai trò nghiêm ngặt (Role-Based Access Control)
> **Là một** Quản trị viên hệ thống,  
> **Tôi muốn** thay đổi vai trò làm việc của bất kỳ nhân viên nào trong phòng khám,  
> **Để** phân chia đúng trách nhiệm công việc và bảo vệ dữ liệu nhạy cảm.

#### Tiêu chí Nghiệm thu (AC):
- **AC 2.1:** Chỉ tài khoản có vai trò `admin` mới được quyền thay đổi vai trò của tài khoản khác. Bất kỳ vai trò nào khác (`doctor`, `receptionist`, `cashier`, `customer`) cố gắng gọi API thay đổi vai trò phải bị hệ thống từ chối ngay lập tức (Mã lỗi `403 Forbidden`).
- **AC 2.2:** Hệ thống chặn không cho phép Admin tự thay đổi vai trò của chính mình để tránh tình trạng hệ thống không còn tài khoản Admin tối cao nào điều hành (`Self-Role Demotion Prevention`).
- **AC 2.3:** Mọi thao tác thay đổi vai trò nhân viên phải được lưu vết tự động vào bảng nhật ký hệ thống (`AuditLogs`) gồm: Người thực hiện, Người bị tác động, Quyền cũ, Quyền mới, và Thời gian thực thi.

### Story 3: Khóa tài khoản nhân viên khẩn cấp (Emergency Account Lockout)
> **Là một** Quản trị viên hệ thống,  
> **Tôi muốn** tạm thời khóa tài khoản của một nhân viên đã nghỉ việc hoặc nghi ngờ bị hack,  
> **Để** ngăn chặn ngay lập tức quyền truy cập của họ vào hệ thống phòng khám.

#### Tiêu chí Nghiệm thu (AC):
- **AC 3.1:** Khi Admin bấm nút "Khóa tài khoản" (`Suspend Account`), trạng thái tài khoản của nhân viên đó chuyển sang `Suspended`.
- **AC 3.2:** Tài khoản ở trạng thái `Suspended` sẽ bị chặn đăng nhập ngay lập tức.
- **AC 3.3:** Nếu nhân viên đang hoạt động trong phiên làm việc (Session active), hệ thống sẽ vô hiệu hóa Token JWT hiện tại của họ thông qua cơ chế danh sách đen token (Token Blacklist) hoặc kiểm tra trạng thái database trực tiếp trong Middleware kiểm tra phiên đăng nhập, thời gian đáp ứng thu hồi quyền dưới **1 giây**.

---

## 4. Phạm vi dự án (In-Scope & Out-of-Scope)

### ✅ In-Scope (Phase 3 MVP)
- CRUD tài khoản nhân sự phòng khám (Admin thực hiện).
- Phân quyền phân chia vai trò chuyên môn (`doctor`, `receptionist`, `cashier`, `admin`).
- Cơ chế tự động sinh mật khẩu tạm thời an toàn mật mã.
- Khóa (`Suspend`) và Kích hoạt lại (`Activate`) tài khoản nhân viên.
- Bắt buộc đổi mật khẩu trong lần đầu tiên đăng nhập.
- Ghi nhận nhật ký hệ thống `AuditLogs` cho mọi hành động thay đổi quyền/khóa tài khoản.

### ❌ Out-of-Scope (Bàn giao Phase sau)
- Tích hợp đăng nhập một lần (Single Sign-On - SSO) với tài khoản doanh nghiệp Google Workspace / Microsoft Entra ID.
- Quản lý ca trực (Timesheet) và tính lương nhân sự tự động.
- Xác thực hai yếu tố (2FA / MFA via Google Authenticator) cho tài khoản nhân viên.

---

## 5. Yêu cầu phi chức năng (Non-Functional Requirements - NFRs)
- **Bảo mật:** Phòng chống 100% các lỗ hổng privilege escalation y khoa và tài chính.
- **Hiệu năng:** Tốc độ xác thực trạng thái tài khoản khóa/mở khóa tại API Gateway / Middleware dưới 50ms mỗi request.
- **Độ tin cậy:** Hệ thống Audit Log được lưu trữ riêng biệt, không thể sửa đổi hoặc xóa bỏ bởi bất kỳ ai (kể cả Admin) để làm bằng chứng đối soát pháp lý.
- **Độ tương thích:** Giao diện quản trị nhân viên hiển thị tối ưu trên màn hình Desktop lớn (1920x1080) và máy tính bảng cầm tay của quản lý phòng khám.
