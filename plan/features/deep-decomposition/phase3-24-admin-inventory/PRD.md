# 🚀 Product Requirements Document (PRD) - Admin Drug Inventory

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Phân hệ **Quản lý Kho thuốc & Vật tư (Admin Drug Inventory)** cung cấp giao diện và lõi logic cho Quản trị viên (Admin) kiểm soát danh mục thuốc điều trị, cập nhật số lượng tồn kho thực tế, điều chỉnh đơn giá nhập/bán, và đưa ra các cảnh báo trực quan khi thuốc trong kho sắp cạn kiệt hoặc sắp hết hạn sử dụng. 

Mục tiêu chính là tối ưu hóa quản lý chuỗi cung ứng y bạ phòng khám, loại bỏ rủi ro gián đoạn ca điều trị do hết thuốc đột ngột và tối thiểu hóa thiệt hại tài chính do thuốc bị hết hạn sử dụng không kịp tiêu thụ.

---

## 2. Đối tượng sử dụng (Target Personas)

### 🧑‍💼 Quản trị viên hệ thống - Anh Khánh (38 tuổi)
- **Mô tả:** Khánh cần theo dõi tình trạng kho thuốc hàng tuần. Anh trực tiếp đặt hàng từ các hãng dược và tiến hành nhập kho khi hàng về.
- **Nỗi đau (Pain points):**
  - Không biết loại thuốc nào đang sắp hết để chủ động nhập thêm trước khi bác sĩ phàn nàn.
  - Khó kiểm soát ngày hết hạn của các lô thuốc, dẫn đến việc nhiều lô thuốc hết hạn phải vứt bỏ gây lãng phí hàng chục triệu đồng mỗi tháng.
  - Sợ nhân viên nhập kho nhập sai số lượng, làm lệch dữ liệu thực tế tại quầy bác sĩ.
- **Mong muốn:** Hệ thống tự động báo cáo thuốc sắp hết hàng (`Low Stock`) và cảnh báo hạn dùng trước 30 ngày. Luồng nhập kho đơn giản, rõ ràng.

### 🧑‍⚕️ Bác sĩ thú y trực ca - Anh Đức (26 tuổi)
- **Mô tả:** Đức đang kê đơn thuốc cho một chú chó bị viêm phổi nặng.
- **Nỗi đau (Pain points):**
  - Kê đơn xong, khách ra quầy thanh toán mới phát hiện thuốc đó trong tủ đã hết, buộc anh phải gọi khách lại để đổi đơn thuốc khác, gây mất uy tín và tốn thời gian.
- **Mong muốn:** Số lượng thuốc tồn kho khả dụng hiển thị chính xác theo thời gian thực ngay trên màn hình kê đơn thuốc của anh.

---

## 3. User Stories & Tiêu chí Nghiệm thu (Acceptance Criteria)

### Story 1: Quản lý danh mục thuốc điều trị và định mức tồn kho tối thiểu
> **Là một** Quản trị viên hệ thống,  
> **Tôi muốn** quản lý danh mục thuốc (thêm, sửa thông tin) và thiết lập định mức tồn kho tối thiểu cho từng loại thuốc,  
> **Để** tôi có thể chủ động theo dõi kế hoạch mua sắm thuốc dự phòng.

#### Tiêu chí Nghiệm thu (AC):
- **AC 1.1:** Admin có quyền thêm mới biệt dược vào danh mục thuốc gồm các trường thông tin: Tên thuốc, Hoạt chất, Hàm lượng, Đơn vị tính, Đơn giá nhập, Đơn giá bán lẻ, Nhà sản xuất, và Định mức tồn tối thiểu (`MinStockThreshold`).
- **AC 1.2:** Hệ thống tự động tính toán số lượng tồn kho thực tế (`CurrentStock`) bằng tổng số lượng khả dụng của tất cả các lô hàng đang hoạt động trong kho.
- **AC 1.3:** Khi sửa đổi thông tin thuốc, Admin không được phép thay đổi trực tiếp trường `CurrentStock` mà phải thông qua phiếu nhập/xuất kho chính thống để bảo đảm tính chính xác của sổ sách kế toán.

### Story 2: Hệ thống cảnh báo tự động tồn kho yếu (Low Stock Alert) & Sắp hết hạn (Expiry Warning)
> **Là một** Quản trị viên hệ thống,  
> **Tôi muốn** hệ thống tự động cảnh báo các thuốc sắp hết hoặc sắp hết hạn sử dụng bằng nhãn hiển thị nổi bật,  
> **Để** tôi thực hiện các biện pháp bổ sung hàng hoặc trả hàng kịp thời.

#### Tiêu chí Nghiệm thu (AC):
- **AC 2.1:** Khi số lượng tồn thực tế của một loại thuốc nhỏ hơn hoặc bằng định mức tồn tối thiểu (`CurrentStock <= MinStockThreshold`), hệ thống lập tức hiển thị nhãn cảnh báo đỏ `Low Stock` trên danh sách thuốc của Admin và Bác sĩ.
- **AC 2.2:** Hệ thống tự động quét và đánh dấu nhãn cảnh báo `Expiry Warning` màu cam đối với các lô thuốc có ngày hết hạn (`ExpiryDate`) nằm trong vòng **30 ngày** tới so với thời điểm hiện tại.
- **AC 2.3:** Bất kỳ lô thuốc nào có ngày hết hạn nhỏ hơn ngày hiện tại sẽ được đánh dấu trạng thái `Expired` (Đã hết hạn), tự động khóa không cho bác sĩ chọn kê đơn y bạ nữa.

### Story 3: Ghi nhận lịch sử nhập kho theo lô hàng (Batch-based Inventory Logging)
> **Là một** Quản trị viên hệ thống,  
> **Tôi muốn** ghi nhận thông tin nhập kho theo từng lô hàng cụ thể (số lô, hạn sử dụng, số lượng),  
> **Để** kiểm soát nguồn gốc và quản lý hạn sử dụng hiệu quả.

#### Tiêu chí Nghiệm thu (AC):
- **AC 3.1:** Khi nhập hàng mới, Admin phải tạo một phiếu nhập kho gồm các trường thông tin: Chọn thuốc, Số lô sản xuất (`BatchNumber`), Số lượng nhập, Đơn giá nhập thực tế, Ngày sản xuất, Hạn sử dụng (`ExpiryDate`).
- **AC 3.2:** Sau khi xác nhận nhập kho, hệ thống tự động cộng dồn số lượng vào `CurrentStock` của biệt dược tương ứng.
- **AC 3.3:** Mọi giao dịch nhập/xuất kho phải tạo bản ghi lịch sử `InventoryTransactions` không thể xóa sửa, lưu trữ: Mã Admin thực hiện, Loại giao dịch (Nhập/Xuất), Số lượng, Số lô, và Thời gian thực thi.

---

## 4. Phạm vi dự án (In-Scope & Out-of-Scope)

### ✅ In-Scope (Phase 3 MVP)
- CRUD danh mục thuốc điều trị.
- Thiết lập định mức tồn kho tối thiểu (`MinStockThreshold`).
- Quản lý kho theo từng lô hàng (`InventoryBatches`) có hạn sử dụng.
- Cảnh báo tồn kho thấp (`Low Stock`) và cảnh báo hạn dùng dưới 30 ngày (`Expiry Warning`).
- Ghi nhận lịch sử giao dịch kho (`InventoryTransactions`).

### ❌ Out-of-Scope (Bàn giao Phase sau)
- Tự động gửi email cảnh báo hết hàng cho nhà phân phối thuốc đối tác.
- Quản lý vị trí tủ thuốc vật lý trong phòng khám (Kệ A, Kệ B, Ngăn 1).
- Quét mã vạch Barcode/RFID trên hộp thuốc để tự động nhập kho.

---

## 5. Yêu cầu phi chức năng (Non-Functional Requirements - NFRs)
- **Hiệu năng:** Thời gian quét dữ liệu và cập nhật trạng thái Low Stock/Expired của toàn bộ danh mục thuốc dưới 200ms.
- **Bảo mật:** Chặn đứng mọi quyền truy cập sửa đổi danh mục thuốc của khách hàng (`Customer`) hoặc nhân viên lễ tân (`Receptionist`).
- **Độ tin cậy:** Trừ kho đồng thời phải bảo đảm tính nhất quán cơ sở dữ liệu cao, không bị race-condition.
- **Trải nghiệm:** Màu sắc cảnh báo phải trực quan, dễ nhìn thấy từ xa và hỗ trợ bộ lọc nhanh các thuốc đang có cảnh báo.
