# TÀI LIỆU ĐẶC TẢ YÊU CẦU PHẦN MỀM (SRS/PRD)
## MODULE: CẤU HÌNH KHUNG GIỜ HOẠT ĐỘNG PHÒNG KHÁM

---

## PHẦN 1 - MỤC TIÊU NGHIỆP VỤ

### 1.1 Mục tiêu và Ý nghĩa
Khung giờ hoạt động là xương sống của toàn bộ hệ thống vận hành phòng khám thú y. Việc số hóa và cho phép Admin cấu hình linh hoạt thời gian hoạt động giúp:
- **Tự động hóa lịch hẹn:** Hệ thống dựa vào khung giờ này để sinh ra các "slot" (khoảng thời gian trống) cho phép khách hàng đặt lịch khám bệnh và tiêm chủng.
- **Quản lý nguồn lực:** Tránh tình trạng quá tải (overbooking) hoặc đặt lịch vào ngoài giờ làm việc, ngày nghỉ lễ.
- **Tăng trải nghiệm khách hàng:** Khách hàng thấy rõ khung giờ nào phòng khám đang mở cửa để chủ động sắp xếp thời gian.

### 1.2 Tác động đến các đối tượng
- **Khách hàng:** Dễ dàng xem được các khung giờ còn trống trong ngày để đặt lịch trực tuyến mà không cần gọi điện hỏi. Không thể chọn các khung giờ phòng khám nghỉ.
- **Lễ tân:** Dựa vào cấu hình này để phân bổ lịch hẹn đột xuất (walk-in), từ chối khách hoặc tư vấn chuyển giờ. Dễ dàng nắm bắt khi nào phòng khám nghỉ lễ để thông báo.
- **Bác sĩ:** Biết chính xác thời gian bắt đầu và kết thúc ca làm việc mỗi ngày, từ đó chuẩn bị tinh thần và sức khỏe. Lịch làm việc cá nhân của bác sĩ phải nằm gọn trong hoặc bằng với khung giờ hoạt động chung.
- **Admin:** Có toàn quyền kiểm soát, thay đổi linh hoạt theo tình hình thực tế (ví dụ: ngày lễ Tết, ngày nghỉ đột xuất, hoặc thay đổi giờ làm việc theo mùa).

### 1.3 Quy trình vận hành thực tế
1. **Đầu kỳ/Hàng năm:** Admin thiết lập lịch làm việc cố định cho các ngày trong tuần (Thứ 2 - Chủ Nhật) và thiết lập sẵn các ngày nghỉ lễ lớn (Tết, Quốc khánh...).
2. **Hàng ngày:** Hệ thống tự động sinh slot đặt lịch dựa trên cấu hình ngày hôm đó. Khách hàng/Lễ tân thao tác book lịch vào các slot này.
3. **Đột xuất:** Khi có sự kiện phát sinh (cúp điện, đi du lịch toàn phòng khám), Admin vào hệ thống cấu hình "Ngày nghỉ" đột xuất. Lễ tân sẽ phải dời các lịch hẹn đã có trong ngày đó sang ngày khác.

---

## PHẦN 2 - PHÂN TÍCH NGHIỆP VỤ

### 2.1 Cấu hình ngày hoạt động trong tuần
- Hệ thống liệt kê 7 ngày: Thứ 2, Thứ 3, Thứ 4, Thứ 5, Thứ 6, Thứ 7, Chủ Nhật.
- Trạng thái: Bật (Hoạt động) hoặc Tắt (Không hoạt động). Nếu Tắt, toàn bộ hệ thống không sinh slot cho ngày thứ đó.

### 2.2 Cấu hình nhiều khoảng thời gian trong ngày
- Với mỗi ngày "Bật" hoạt động, Admin được phép thêm 1 hoặc nhiều khoảng thời gian làm việc (Ca làm việc).
- **Ví dụ chuẩn:**
  - Ca sáng: 08:00 - 12:00
  - Ca chiều: 13:30 - 17:30
  - Ca tối (nếu có): 18:30 - 20:00
- Thời gian giữa các ca (12:00 - 13:30) tự động được hiểu là giờ nghỉ trưa, hệ thống KHÔNG sinh slot cho giờ này.

### 2.3 Cấu hình ngày nghỉ, ngày lễ
- **Định nghĩa:** Ngày nghỉ/Lễ là một (hoặc một dải) ngày cụ thể trong năm mà phòng khám không hoạt động, ghi đè (override) lên cấu hình ngày trong tuần.
- **Ví dụ:** Thứ 2 (Bình thường là có làm việc), nhưng nếu Thứ 2 đó rơi vào 01/05 (Tạo cấu hình Ngày lễ 01/05), thì hệ thống sẽ coi Thứ 2 đó là ngày nghỉ.

---

## PHẦN 3 - THIẾT KẾ MÀN HÌNH

**Tên màn hình:** Cấu hình khung giờ hoạt động (Clinic Working Hours Settings)

**Layout:** 
Màn hình chia làm 2 Tab chính:
1. **Tab 1: Khung giờ tiêu chuẩn (Weekly Schedule)**
2. **Tab 2: Ngày nghỉ & Ngày lễ (Holidays & Time-offs)**

### Các khu vực chi tiết trên UI:

**Khu vực A. Header & Thông tin chung (Mọi Tab)**
- Tiêu đề "Cấu hình Khung giờ & Ngày nghỉ".
- Dòng trạng thái (Status): Lần cập nhật cuối bởi ai, lúc nào.
- Nút "Lưu tất cả" và "Hủy thay đổi" (Sticky ở góc phải trên).

**Khu vực B. Danh sách ngày trong tuần (Tab 1)**
- Một danh sách dọc (List view) hoặc các thẻ (Cards) hiển thị từ Thứ 2 đến Chủ nhật.
- Mỗi dòng (Thứ) có một Toggle Button (Switch) để Bật/Tắt hoạt động.

**Khu vực C. Danh sách khoảng thời gian (Nằm trong Khu vực B)**
- Khi Toggle "Bật", sổ xuống danh sách các "Khoảng thời gian" của ngày đó.
- Mỗi khoảng thời gian gồm 2 TimePicker (Giờ bắt đầu - Giờ kết thúc).
- Bên cạnh mỗi khoảng thời gian có icon 🗑️ (Thùng rác) để xóa.
- Nút "+ Thêm khoảng thời gian" ở dưới cùng của mỗi ngày.

**Khu vực D. Danh sách Ngày nghỉ/Lễ (Tab 2)**
- Nút "+ Thêm ngày nghỉ".
- Bảng (Table) danh sách các ngày nghỉ sắp tới.
  - Các cột: Tên dịp lễ/Nghỉ, Từ ngày, Đến ngày, Trạng thái (Đang áp dụng/Đã qua), Hành động (Edit/Delete).

**Khu vực E. Nút chức năng chung**
- Nút "Lưu cấu hình", Nút "Khôi phục mặc định".

---

## PHẦN 4 - CÁC FIELD TRÊN MÀN HÌNH

### Tab 1: Khung giờ tiêu chuẩn

| Tên Field | Kiểu dữ liệu | Required | Validation | Default | Error Message |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Trạng thái (Thứ 2 -> CN)** | Boolean (Toggle) | Có | Không có ràng buộc. | `true` (Trừ CN `false`) | - |
| **Giờ bắt đầu** | Time (HH:mm) | Có (nếu Bật) | Phải < Giờ kết thúc; Khác Rỗng | `08:00` | "Vui lòng chọn giờ bắt đầu." / "Giờ bắt đầu phải nhỏ hơn giờ kết thúc." |
| **Giờ kết thúc** | Time (HH:mm) | Có (nếu Bật) | Phải > Giờ bắt đầu; Khác Rỗng | `17:00` | "Vui lòng chọn giờ kết thúc." / "Giờ kết thúc không hợp lệ." |

### Tab 2: Ngày nghỉ & Lễ

| Tên Field | Kiểu dữ liệu | Required | Validation | Default | Error Message |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Tên dịp nghỉ** | Text | Có | Max 100 char, không rỗng | Rỗng | "Vui lòng nhập tên ngày nghỉ." |
| **Từ ngày** | Date (DD/MM/YYYY) | Có | >= Ngày hiện tại | Ngày hiện tại | "Ngày bắt đầu nghỉ không được là quá khứ." |
| **Đến ngày** | Date (DD/MM/YYYY) | Có | >= Từ ngày | Từ ngày | "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu." |

---

## PHẦN 5 - CÁC NÚT CHỨC NĂNG

### 1. Thêm khoảng thời gian (Add Time Slot)
- **Mục đích:** Thêm một ca làm việc mới cho 1 ngày cụ thể trong tuần.
- **Điều kiện:** Toggle ngày đó phải đang "Bật". Số khoảng thời gian hiện tại < 5.
- **Kết quả:** Thêm một dòng TimePicker mới ngay dưới khoảng thời gian cuối cùng. Giờ mặc định trống.

### 2. Xóa khoảng thời gian (Delete Time Slot)
- **Mục đích:** Loại bỏ một ca làm việc.
- **Điều kiện:** Ngày đó đang "Bật". Phải để lại ít nhất 1 khoảng thời gian (không cho xóa dòng cuối cùng nếu đang Bật).
- **Kết quả:** Dòng bị xóa khỏi UI.

### 3. Thêm ngày nghỉ (Add Holiday)
- **Mục đích:** Tạo một dịp nghỉ.
- **Điều kiện:** Không.
- **Kết quả:** Mở Modal/Drawer để nhập "Tên dịp nghỉ, Từ ngày, Đến ngày". Bấm xác nhận sẽ thêm vào bảng.

### 4. Lưu (Save / Apply)
- **Mục đích:** Lưu toàn bộ thay đổi vào Database.
- **Điều kiện:** Toàn bộ Validation Rule phải PASS.
- **Kết quả:** Gửi API POST/PUT. Hiển thị Toast "Lưu cấu hình thành công". Trigger tạo log audit.

### 5. Khôi phục mặc định (Reset to Default)
- **Mục đích:** Đưa thiết lập về chuẩn chung 8h-12h, 13h-17h, nghỉ CN.
- **Điều kiện:** Không.
- **Kết quả:** Mở Popup Confirm. Đồng ý thì fill lại UI theo default. Cần bấm "Lưu" để thực sự lưu vào DB.

---

## PHẦN 6 - VALIDATION RULES (30 Rules)

**Kiểm tra tính hợp lệ dữ liệu (Dữ liệu vào):**
1. **VR-01:** Giờ bắt đầu không được để trống (nếu bật ngày hoạt động).
2. **VR-02:** Giờ kết thúc không được để trống.
3. **VR-03:** Giờ bắt đầu phải đúng định dạng `HH:mm` (24h).
4. **VR-04:** Giờ kết thúc phải đúng định dạng `HH:mm` (24h).
5. **VR-05:** Tên ngày nghỉ không được chứa ký tự đặc biệt như `<`, `>`, `&`.
6. **VR-06:** Tên ngày nghỉ có độ dài tối đa 100 ký tự.
7. **VR-07:** Tên ngày nghỉ không được chỉ chứa khoảng trắng.
8. **VR-08:** "Từ ngày" của ngày nghỉ phải là ngày hợp lệ (Real date).
9. **VR-09:** "Đến ngày" của ngày nghỉ phải là ngày hợp lệ.

**Kiểm tra logic thời gian (Time Logic):**
10. **VR-10:** Giờ bắt đầu của một khoảng thời gian phải nhỏ hơn (<) giờ kết thúc của khoảng thời gian đó.
11. **VR-11:** Khoảng cách giữa giờ bắt đầu và giờ kết thúc ít nhất phải bằng độ dài 1 slot khám (ví dụ: 30 phút).
12. **VR-12:** Hai khoảng thời gian trong CÙNG MỘT NGÀY không được trùng nhau (ví dụ: Slot 1: 08:00-10:00, Slot 2: 09:00-11:00 là lỗi).
13. **VR-13:** Hai khoảng thời gian trong cùng một ngày không được liền kề sát nhau mà không gộp (Cảnh báo: Nếu 08:00-10:00 và 10:00-12:00 thì khuyên gộp thành 08:00-12:00).
14. **VR-14:** Khoảng thời gian không được vượt quá 23:59 (Không hỗ trợ ca làm việc xuyên ngày trong phiên bản này).
15. **VR-15:** Giờ bắt đầu nhỏ nhất cho phép là 00:00.
16. **VR-16:** Giờ kết thúc lớn nhất cho phép là 23:59.
17. **VR-17:** Nếu ngày "Bật" hoạt động, phải có ít nhất 1 khoảng thời gian hợp lệ.
18. **VR-18:** Tối đa 5 khoảng thời gian được tạo trong 1 ngày (tránh spam/chia nhỏ quá mức).

**Kiểm tra logic ngày nghỉ (Holiday Logic):**
19. **VR-19:** "Từ ngày" phải lớn hơn hoặc bằng ngày hiện tại (không được cấu hình ngày nghỉ cho quá khứ).
20. **VR-20:** "Đến ngày" phải lớn hơn hoặc bằng "Từ ngày".
21. **VR-21:** Khoảng thời gian từ "Từ ngày" đến "Đến ngày" không vượt quá 30 ngày (Tránh chọn nhầm nghỉ cả năm).
22. **VR-22:** Hai dịp nghỉ lễ khác nhau không được trùng lặp khoảng thời gian.
23. **VR-23:** Hai dịp nghỉ lễ khác nhau không được chồng lấn thời gian một phần.
24. **VR-24:** Tên dịp lễ không được trùng lặp với một dịp lễ đang tồn tại (có cùng năm).

**Kiểm tra hệ thống/API Level:**
25. **VR-25:** Trạng thái Bật/Tắt ngày trong tuần chỉ nhận giá trị boolean (true/false).
26. **VR-26:** ID của cấu hình ngày nghỉ phải tồn tại khi thực hiện thao tác Edit/Delete.
27. **VR-27:** ID của các ngày trong tuần truyền vào API phải hợp lệ (từ 0 đến 6 đại diện cho CN đến T7).
28. **VR-28:** Không truyền mã độc (XSS payload) vào tên dịp lễ.
29. **VR-29:** Request Payload size không được vượt quá giới hạn hệ thống (e.g., 1MB).
30. **VR-30:** Bắt buộc có Bearer Token trong header khi gọi API lưu.

---

## PHẦN 7 - BUSINESS RULES (50 Rules)

**Quyền và Phân quyền (Auth & RBAC):**
1. **BR-01:** Chỉ Role Admin mới có quyền truy cập vào màn hình Cấu hình Khung giờ hoạt động.
2. **BR-02:** Role Admin được quyền xem (Read), Thêm mới (Create), Sửa (Update) cấu hình khung giờ.
3. **BR-03:** Role Admin được quyền Thêm, Sửa, Xóa ngày nghỉ lễ.
4. **BR-04:** Lễ tân chỉ được quyền XEM (Read-only) khung giờ hoạt động tại màn hình Booking, không được cấu hình.
5. **BR-05:** Bác sĩ chỉ được quyền XEM (Read-only) khung giờ tại màn hình lịch làm việc cá nhân.
6. **BR-06:** Khách hàng ẩn danh hoặc khách hàng đã đăng nhập không có quyền xem cấu hình này dưới dạng raw, chỉ thấy slot trống được sinh ra.
7. **BR-07:** API cập nhật cấu hình phải kiểm tra claims `Role=Admin` từ JWT.
8. **BR-08:** Mọi thao tác lưu cấu hình phải được ghi nhận vào Audit Log.

**Quy tắc sinh Slot (Slot Generation):**
9. **BR-09:** Dựa vào cấu hình thời gian, hệ thống tự động sinh các slot lịch khám (ví dụ mỗi slot 30 phút).
10. **BR-10:** Hệ thống chỉ sinh slot cho các ngày được "Bật" hoạt động.
11. **BR-11:** Nếu ngày trong tuần được "Bật", nhưng lại rơi vào khoảng thời gian của một "Ngày nghỉ lễ", hệ thống SẼ KHÔNG sinh slot cho ngày đó. (Ngày lễ có độ ưu tiên cao hơn Ngày trong tuần).
12. **BR-12:** Slot đầu tiên của ca bắt đầu đúng bằng Giờ bắt đầu.
13. **BR-13:** Slot cuối cùng của ca phải kết thúc bằng hoặc trước Giờ kết thúc của ca đó. (Ví dụ ca từ 08:00-11:45, slot 30p, thì slot cuối là 11:00-11:30, 15 phút thừa không sinh slot).
14. **BR-14:** Khi Admin THAY ĐỔI khung giờ của 1 ngày (VD từ 08h đổi thành 09h), các slot mới sẽ được sinh ra (hoặc thu hẹp) ngay lập tức cho các ngày tương lai.
15. **BR-15:** Lịch hẹn đã đặt (Booked) trong quá khứ không bị ảnh hưởng bởi thay đổi cấu hình hiện tại.

**Ràng buộc Dữ liệu Xung đột (Conflict Handling):**
16. **BR-16:** Nếu Admin xóa một khoảng thời gian (VD xóa ca chiều), hệ thống phải kiểm tra xem có "Lịch hẹn" nào của khách (trạng thái Pending, Confirmed) đang tồn tại trong khoảng thời gian bị xóa ở TƯƠNG LAI hay không.
17. **BR-17:** Nếu CÓ lịch hẹn tương lai, hệ thống phải chặn lại và cảnh báo: "Không thể xóa ca làm việc do đã có lịch hẹn đặt trước từ ngày [X] đến [Y]. Vui lòng hủy/dời lịch hẹn trước khi thay đổi cấu hình".
18. **BR-18:** Tương tự, nếu Admin "Tắt" hoạt động của Thứ 3, nhưng tương lai có hàng loạt lịch khám Thứ 3 đã đặt, hệ thống phải cảnh báo và CẤM lưu.
19. **BR-19:** Nếu Admin thêm một Ngày nghỉ lễ (VD 01/05), mà ngày 01/05 đã có khách đặt lịch, hệ thống phải CẤM thêm ngày nghỉ này và yêu cầu Admin xử lý các lịch hẹn ngày 01/05 trước.
20. **BR-20:** Quản trị viên có thể "ÉP LƯU" (Force Save) qua một popup xác nhận quyền lực cao, trong đó hệ thống sẽ tự động chuyển tất cả các lịch hẹn bị xung đột sang trạng thái "Bị Hủy Do Phòng Khám" và tự động gửi Email/SMS xin lỗi khách. (Lựa chọn tùy yêu cầu nghiệp vụ, mặc định nên Cấm).
21. **BR-21:** Nếu một bác sĩ đã được phân ca làm việc từ 08:00-12:00, Admin không thể sửa khung giờ phòng khám thành 09:00-11:00 nếu không đổi lại lịch của bác sĩ đó trước. Lịch phòng khám phải BAO PHỦ lịch bác sĩ.

**Quy tắc cập nhật & Lịch sử (Update Rules):**
22. **BR-22:** Mỗi khi lưu cấu hình, `UpdatedDate` và `UpdatedBy` phải được cập nhật thời gian thực.
23. **BR-23:** Lưu cấu hình là lưu dạng Snapshot hoặc ghi đè (Overwrite) toàn bộ thiết lập tuần.
24. **BR-24:** Không cho phép cập nhật cấu hình nếu không có bất kỳ thay đổi nào so với dữ liệu gốc (nút Lưu bị disable).
25. **BR-25:** Ngày nghỉ lễ đã đi qua (Trong quá khứ) thì KHÔNG được phép sửa (Edit) hoặc xóa (Delete). Chỉ lưu cho mục đích báo cáo.
26. **BR-26:** Có thể xóa Ngày nghỉ lễ đang diễn ra (hôm nay), có hiệu lực ngay lập tức, hệ thống mở lại slot.
27. **BR-27:** Cấu hình mới sẽ có hiệu lực ngay lập tức sau khi lưu thành công (`Status 200 OK`).

**Quy định về Giao diện & Hiển thị:**
28. **BR-28:** Khung giờ phải được format theo kiểu 24h (HH:mm) trên UI để tránh nhầm lẫn AM/PM.
29. **BR-29:** Ngày tháng phải format chuẩn `DD/MM/YYYY`.
30. **BR-30:** Dữ liệu kéo từ DB lên phải được sort theo thứ tự: Thứ 2 -> Chủ nhật.
31. **BR-31:** Khoảng thời gian trong 1 ngày phải được sort theo thời gian tăng dần (08:00 nằm trên, 13:00 nằm dưới).
32. **BR-32:** Danh sách Ngày nghỉ lễ sort theo `Từ ngày` giảm dần (mới nhất/sắp tới lên đầu).
33. **BR-33:** Trạng thái Ngày lễ hiển thị: "Sắp tới" (Màu xanh), "Đang diễn ra" (Màu vàng), "Đã qua" (Màu xám).
34. **BR-34:** Nút Lưu chỉ được enable khi form Validates form is valid (IsDirty = true & IsValid = true).
35. **BR-35:** Nếu có lỗi ở Tab nào, xuất hiện chấm đỏ cảnh báo trên Header của Tab đó.

**Ngoại lệ và Các ràng buộc sâu hơn (Deep Constraints):**
36. **BR-36:** Giới hạn dữ liệu Ngày nghỉ: Tối đa tạo 50 ngày nghỉ trong 1 năm lịch (Calendar Year).
37. **BR-37:** Nếu thay đổi cấu hình, Cache (Redis/Memory) lưu các Slot khả dụng trên trang chủ phải được Invalidate (xóa) ngay lập tức.
38. **BR-38:** Nếu hệ thống đang trong phiên bảo trì chạy ngầm (Cron job đang quét slot), tạm khóa quyền cấu hình (Lock).
39. **BR-39:** Nếu khoảng thời gian của Dịch vụ Khám và Tiêm chủng có sự khác biệt (trong tương lai mở rộng), module này sẽ cung cấp cấu hình ở cấp độ Phòng Khám (Global). Các dịch vụ không được phép vượt qua Global.
40. **BR-40:** Ngày trong tuần không được định nghĩa bằng ID ngẫu nhiên mà dùng chuẩn System.DayOfWeek của .NET (0 = Sunday, 1 = Monday...).
41. **BR-41:** TimeZone của phòng khám được fix cứng là UTC+7 (Asia/Ho_Chi_Minh). Mọi DateTime xử lý ở Backend phải theo chuẩn này.
42. **BR-42:** Khi khách hàng vào xem lịch lúc Admin đang lưu cấu hình, hệ thống ưu tiên giữ Slot theo cấu hình cũ cho đến khi hoàn tất Request đặt lịch đó (Concurrency control).
43. **BR-43:** Ngày nghỉ lễ có thể trùng lặp với ngày Chủ nhật nghỉ định kỳ (Không lỗi, hệ thống vẫn hiểu là ngày nghỉ).
44. **BR-44:** Không cho phép chọn "Đến ngày" của ngày nghỉ lễ qua năm tiếp theo (Nghỉ Tết dương, Tết âm phải chia thành các Block trong từng năm để dễ query).
45. **BR-45:** Admin không được thiết lập khoảng thời gian kết thúc lúc `00:00` của ngày hôm sau. Phải là `23:59`.
46. **BR-46:** Lịch sử thay đổi cấu hình (Audit Log) phải lưu lại chi tiết JSON của cấu hình cũ và JSON cấu hình mới.
47. **BR-47:** Tính năng "Khôi phục mặc định" sẽ lấy cấu hình cứng từ tệp `appsettings.json` hoặc Environment Variables làm tham chiếu.
48. **BR-48:** Các bảng liên quan trong Database (ví dụ: `ClinicSchedules`, `Holidays`) sử dụng Soft Delete thay vì Hard Delete khi xóa ngày nghỉ.
49. **BR-49:** Không giới hạn số lần Admin được phép cấu hình trong 1 ngày, nhưng sẽ Rate Limit (30 request/phút) để chống spam API.
50. **BR-50:** Khi lưu thất bại do lỗi Timeout Database, hệ thống không làm hỏng dữ liệu hiện tại (Sử dụng DbTransaction).

---

## PHẦN 8 - LUỒNG XỬ LÝ (PROCESSING FLOW)

### 8.1 Luồng chính (Happy Path)
1. **Admin** đăng nhập thành công vào hệ thống.
2. Từ Sidebar, Admin chọn **Cấu hình -> Khung giờ hoạt động**.
3. **Frontend** gọi API `GET /api/v1/clinic-settings/working-hours` và `GET /api/v1/holidays`.
4. **Backend** trả về dữ liệu cấu hình hiện tại.
5. **Frontend** render hiển thị dữ liệu lên 2 tab.
6. Admin thao tác: "Bật" hoạt động ngày Thứ 2, thêm khoảng `08:00 - 12:00` và `13:30 - 18:00`.
7. Admin bấm **Lưu cấu hình**.
8. **Frontend** chạy Validation tại client (pass 100%).
9. **Frontend** gọi API `PUT /api/v1/clinic-settings/working-hours` kèm Payload JSON.
10. **Backend** chạy Validation, kiểm tra các BR (Không có lịch hẹn xung đột).
11. **Backend** thực hiện update Db, lưu Audit Log, invalidate Redis Cache.
12. **Backend** trả về `200 OK`.
13. **Frontend** hiển thị Toast "Lưu thành công", lấy lại dữ liệu mới nhất.

### 8.2 Luồng thay thế (Alternative Flow) - Thêm ngày nghỉ lễ
1. Tại Tab 2, Admin bấm "Thêm ngày nghỉ".
2. Nhập "Nghỉ lễ Quốc Khánh", chọn `02/09/2026` đến `03/09/2026`.
3. Bấm **Lưu**.
4. Quá trình kiểm tra và lưu diễn ra bình thường, bảng danh sách ngày lễ xuất hiện thêm dòng mới. Hệ thống xóa mọi slot khả dụng của ngày 2/9 và 3/9.

### 8.3 Luồng lỗi (Exception Flow) - Lỗi xung đột lịch hẹn
1. Tại bước 10 của Luồng chính, khi Backend kiểm tra sự kiện thay đổi.
2. **Backend** phát hiện: Việc Admin rút ngắn ca làm việc Thứ 2 từ `18:00` xuống `17:00` làm ảnh hưởng đến 2 lịch hẹn khám đã đặt lúc `17:30` của tuần sau.
3. **Backend** trả về `409 Conflict`, kèm mã lỗi `ERR_SCHEDULE_CONFLICT` và mảng danh sách thông tin 2 lịch hẹn đó.
4. **Frontend** hiển thị Modal báo lỗi: "Không thể thay đổi. Đang có lịch hẹn của Khách hàng [A], [B] vào giờ bị loại bỏ. Vui lòng xử lý lịch hẹn trước."
5. Admin bắt buộc phải bấm "Đóng" và chưa thể cấu hình xong.

### 8.4 Luồng khôi phục (Recovery Flow) - Mất kết nối
1. Admin đang thao tác trên form nhưng bị rớt mạng.
2. Bấm "Lưu", Frontend gọi API nhưng gặp lỗi `Network Error`.
3. Frontend hiển thị: "Mất kết nối mạng. Vui lòng kiểm tra lại đường truyền."
4. Dữ liệu trên Form vẫn được giữ nguyên không mất (lưu trữ tạm ở State/Pinia).
5. Khi có mạng, Admin bấm "Lưu" lại, quá trình tiếp diễn.

---

## PHẦN 9 - USE CASE SPECIFICATION

### UC01 - Cấu hình giờ hoạt động

- **Mục tiêu:** Cho phép Quản trị viên thiết lập lịch làm việc cố định và ngày nghỉ của phòng khám để hệ thống điều phối lịch khám bệnh, tiêm chủng.
- **Actor:** Administrator (Quản trị viên)
- **Pre-conditions (Điều kiện tiên quyết):** 
  - Actor đã đăng nhập thành công.
  - Actor có role `Admin`.
- **Main Flow (Luồng chính):**
  1. Actor chọn menu "Cấu hình giờ hoạt động".
  2. Hệ thống hiển thị giao diện gồm các thứ trong tuần và các ngày nghỉ lễ đã tạo.
  3. Actor bật/tắt ngày hoạt động, thêm/sửa khoảng thời gian (giờ bắt đầu - kết thúc).
  4. Actor chọn "Lưu cấu hình".
  5. Hệ thống kiểm tra tính hợp lệ dữ liệu.
  6. Hệ thống kiểm tra xung đột với lịch hẹn và lịch làm việc của bác sĩ.
  7. Hệ thống cập nhật dữ liệu vào Database, tạo log thay đổi.
  8. Hệ thống thông báo "Cập nhật thành công".
- **Alternative Flow (Luồng thay thế): Khôi phục mặc định**
  1. Tại bước 3, Actor chọn "Khôi phục mặc định".
  2. Hệ thống hỏi xác nhận.
  3. Actor chọn "Đồng ý".
  4. Hệ thống nạp dữ liệu chuẩn (8h-12h, 13h-17h) lên form. Actor trở lại bước 4 của luồng chính.
- **Exception Flow (Luồng lỗi): Xung đột lịch**
  1. Tại bước 6 của luồng chính, hệ thống phát hiện có lịch hẹn tương lai nằm ngoài khung giờ vừa thu hẹp.
  2. Hệ thống báo lỗi "Xung đột lịch hẹn", hiển thị danh sách lịch hẹn cần xử lý.
  3. Quá trình lưu bị hủy bỏ.
- **Post-conditions (Điều kiện hậu quyết):**
  - Khung giờ mới được áp dụng lập tức.
  - Các slot cho phép đặt lịch ngoài frontend (customer site) tự động thay đổi theo cấu hình mới.

---

## PHẦN 11 - AUDIT LOG (LỊCH SỬ THAY ĐỔI)

### Mục đích
Do đây là tính năng lõi (Core Settings), bất kỳ sự thay đổi nào cũng có nguy cơ làm rối loạn vận hành. Do đó, cần có Audit Log cực kỳ chi tiết.

### Các hành động được log lại (Action Type)
1. `UPDATE_WEEKLY_SCHEDULE`: Thay đổi giờ làm việc trong tuần.
2. `ADD_HOLIDAY`: Thêm một ngày nghỉ lễ.
3. `UPDATE_HOLIDAY`: Sửa ngày nghỉ lễ.
4. `DELETE_HOLIDAY`: Xóa ngày nghỉ lễ.

### Cấu trúc dữ liệu Audit Log (Lưu trong bảng `AuditLogs`)
- **LogID:** Guid (PK)
- **ActionType:** (Như trên)
- **EntityName:** `ClinicSchedules` / `Holidays`
- **EntityID:** (ID của bản ghi)
- **ActorID:** `UserID` của người thao tác
- **ActorName:** Tên người thao tác (ví dụ: `Admin Nguyễn Văn A`)
- **Timestamp:** Thời gian thực hiện (UTC)
- **IPAddress:** IP của người thực hiện
- **OldValues (JSON):** Nội dung cấu hình trước khi sửa (Lưu dạng JSON string để đối chiếu).
- **NewValues (JSON):** Nội dung cấu hình sau khi sửa.

### Phân tích
- **Ai thay đổi giờ hoạt động:** Có thể truy vết ngay lập tức nhờ `ActorName` và `Timestamp`.
- **Ai xóa ngày nghỉ:** Nếu nhân viên (có quyền Admin) vô tình xóa ngày lễ dẫn đến khách ồ ạt đặt lịch, người quản lý cấp cao có thể vào Audit Log tìm lại `OldValues` để khôi phục và quy trách nhiệm.

---

## PHẦN 12 - SCREEN UNIT TEST CHECKLIST (50 Test Cases)

### A. Test Hợp lệ (Happy Cases)
1. **TC_01:** Mở màn hình, kiểm tra dữ liệu load đúng cấu hình hiện tại từ API.
2. **TC_02:** Bật ngày Thứ 2, nhập 1 ca (08:00 - 12:00), Lưu thành công.
3. **TC_03:** Bật ngày Thứ 3, nhập 2 ca (08:00 - 12:00, 13:00 - 17:00), Lưu thành công.
4. **TC_04:** Tắt 1 ngày (VD Chủ Nhật), Lưu thành công, hệ thống không sinh slot ngày CN.
5. **TC_05:** Thêm 1 ngày nghỉ lễ hợp lệ (trong tương lai), Lưu thành công.
6. **TC_06:** Thêm nhiều ngày nghỉ lễ hợp lệ (không trùng nhau), Lưu thành công.
7. **TC_07:** Sửa tên của 1 ngày nghỉ lễ đang có, Lưu thành công.
8. **TC_08:** Xóa 1 ngày nghỉ lễ trong tương lai, Lưu thành công.
9. **TC_09:** Bấm nút "Khôi phục mặc định", check UI hiển thị lại cấu hình chuẩn, Lưu thành công.
10. **TC_10:** Bật 7/7 ngày trong tuần, thêm ca làm việc cho tất cả, Lưu thành công.

### B. Test Không Hợp Lệ (Invalid / Validation Cases)
11. **TC_11:** Để trống giờ bắt đầu của ca 1, bấm Lưu -> Lỗi.
12. **TC_12:** Để trống giờ kết thúc của ca 1, bấm Lưu -> Lỗi.
13. **TC_13:** Nhập giờ bắt đầu LỚN HƠN giờ kết thúc (VD: 12:00 - 08:00) -> Lỗi VR-10.
14. **TC_14:** Nhập giờ bắt đầu BẰNG giờ kết thúc (VD: 08:00 - 08:00) -> Lỗi VR-10.
15. **TC_15:** Thêm ca thứ 2 có thời gian trùng với ca 1 (VD Ca1 08-10, Ca2 09-11) -> Lỗi chồng lấn VR-12.
16. **TC_16:** Để trống tên ngày nghỉ lễ -> Lỗi VR-05.
17. **TC_17:** Tên ngày nghỉ lễ chứa ký tự XSS (VD `<script>`) -> Lỗi VR-05 / Backend chối từ.
18. **TC_18:** Tên ngày nghỉ lễ quá 100 ký tự -> Lỗi VR-06.
19. **TC_19:** Ngày bắt đầu nghỉ lễ LỚN HƠN Ngày kết thúc nghỉ lễ -> Lỗi VR-20.
20. **TC_20:** Để trống Ngày bắt đầu nghỉ lễ -> Lỗi validation.
21. **TC_21:** Để trống Ngày kết thúc nghỉ lễ -> Lỗi validation.
22. **TC_22:** Bật ngày hoạt động nhưng không thêm bất kỳ khoảng thời gian nào -> Lỗi VR-17.
23. **TC_23:** Nhập giờ có định dạng sai (VD: `25:00` hoặc `12:60`) -> Lỗi format time.
24. **TC_24:** Bấm thêm khoảng thời gian vượt quá giới hạn (VD bấm 6 lần cho 1 ngày) -> Nút Add disable hoặc báo lỗi VR-18.
25. **TC_25:** Cố gắng xóa khoảng thời gian duy nhất của 1 ngày đang "Bật" -> Không cho phép xóa (Nút Xóa bị disable).

### C. Test Biên (Boundary Cases)
26. **TC_26:** Chọn giờ bắt đầu nhỏ nhất `00:00`, kết thúc `23:59` -> Hợp lệ.
27. **TC_27:** Độ dài của ca làm việc chỉ đúng 1 slot (VD hệ thống slot 30p, cấu hình 08:00 - 08:30) -> Hợp lệ.
28. **TC_28:** Độ dài ca làm việc nhỏ hơn 1 slot (VD: 08:00 - 08:15) -> Lỗi VR-11.
29. **TC_29:** Chọn ngày nghỉ lễ BẮT ĐẦU từ chính ngày hôm nay (Tương lai gần nhất) -> Hợp lệ.
30. **TC_30:** Chọn ngày nghỉ lễ là ngày hôm qua (Quá khứ) -> Lỗi VR-19.
31. **TC_31:** Khoảng thời gian nghỉ lễ kéo dài đúng 30 ngày -> Hợp lệ.
32. **TC_32:** Khoảng thời gian nghỉ lễ kéo dài 31 ngày -> Lỗi VR-21 (Vượt giới hạn độ dài).

### D. Test Dữ Liệu Trùng (Duplication Cases)
33. **TC_33:** Tạo 2 ca làm việc có giờ hoàn toàn giống nhau (VD 08:00-12:00 và 08:00-12:00) -> Lỗi.
34. **TC_34:** Tạo ngày nghỉ lễ MỚI trùng hoàn toàn thời gian với ngày nghỉ ĐÃ CÓ -> Lỗi VR-22.
35. **TC_35:** Tạo ngày nghỉ lễ MỚI chồng lấn 1 phần thời gian với ngày nghỉ ĐÃ CÓ (VD Đã có 1/5-3/5, tạo mới 2/5-4/5) -> Lỗi VR-23.
36. **TC_36:** Tạo ngày nghỉ lễ có TÊN giống hệt ngày nghỉ đã có trong năm -> Lỗi VR-24.

### E. Test Dữ Liệu Rỗng (Empty/No Data Cases)
37. **TC_37:** Tắt toàn bộ 7 ngày trong tuần, bấm Lưu -> Hệ thống hỏi "Cảnh báo đóng cửa phòng khám toàn diện", xác nhận -> Hợp lệ.
38. **TC_38:** Hệ thống lần đầu khởi tạo (Database trống) -> Trả về cấu hình mặc định (Hoặc không lỗi khi render).
39. **TC_39:** Bảng ngày nghỉ không có data nào -> Hiển thị "Không có ngày nghỉ nào sắp tới".

### F. Test Phân Quyền Truy Cập (Access Control)
40. **TC_40:** Đăng nhập bằng tài khoản Lễ tân truy cập route `/admin/settings/hours` -> Chuyển hướng 403 Forbidden.
41. **TC_41:** Đăng nhập bằng tài khoản Bác sĩ truy cập route -> 403 Forbidden.
42. **TC_42:** Gọi trực tiếp API POST với Token của Lễ tân -> Backend trả về 403.
43. **TC_43:** Gọi API mà không có Token (Unauthenticated) -> Backend trả về 401.

### G. Test Xung Đột Nghiệp Vụ (Business Logic/Conflict Cases)
44. **TC_44:** Thay đổi ca chiều Thứ 3 (Xóa đi), nhưng Thứ 3 có 1 lịch khám CHƯA DUYỆT (Pending) -> Lỗi xung đột BR-17.
45. **TC_45:** Xóa ca chiều Thứ 3, lịch khám Thứ 3 đã bị HỦY (Cancelled/Completed) -> Lệnh Lưu thành công (Không xung đột với quá khứ/lịch đã hủy).
46. **TC_46:** Tạo ngày nghỉ 01/05, nhưng 01/05 đã có lịch hẹn -> Lỗi báo xung đột lịch.
47. **TC_47:** Thu hẹp ca làm việc (từ 18h xuống 17h) khi bác sĩ X đang được phân ca trực đến 18h -> Lỗi xung đột ca trực của bác sĩ.

### H. Test Khác
48. **TC_48:** Sửa xóa ngày nghỉ lễ đã đi qua (ví dụ Tết năm ngoái) -> Nút Edit/Delete bị disable trên UI, gọi API thì backend báo lỗi.
49. **TC_49:** Đang sửa cấu hình thì F5 (Reload trang) -> Data quay về như cũ (chưa lưu vào DB).
50. **TC_50:** Bấm liên tục (Double click) vào nút "Lưu" -> Button hiển thị spinner Loading, disable click, chỉ gửi đúng 1 API request.

---

## PHẦN 13 - TÁC ĐỘNG ĐẾN CÁC MODULE KHÁC

Sự thay đổi của Module "Cấu hình khung giờ hoạt động" là **Global Effect**, ảnh hưởng như hiệu ứng domino đến các module sau:

### 1. Đặt lịch khám (Customer Booking Module)
- **Tác động:** Widget chọn giờ trên Frontend của Khách hàng phụ thuộc trực tiếp vào API sinh slot dựa trên cấu hình này.
- **Ảnh hưởng:** Nếu Admin tạo "Ngày nghỉ", ngày đó trên lịch của khách sẽ bị disable (màu xám). Nếu thay đổi giờ làm việc, các slot sẽ hiển thị mới ngay lập tức. Cần phải xóa Cache (Invalidate) sau khi Admin bấm Lưu.

### 2. Duyệt lịch hẹn (Receptionist Booking Management)
- **Tác động:** Khi Lễ tân thao tác "Tạo lịch hẹn mới (Walk-in)" cho khách hoặc dời lịch.
- **Ảnh hưởng:** Lễ tân chỉ được phép chọn các khung giờ và ngày nằm trong cấu hình. Không thể dời khách vào lúc phòng khám đã cấu hình đóng cửa.

### 3. Lịch làm việc cá nhân / Phân công bác sĩ (Doctor Schedule Roster)
- **Tác động:** Admin/Quản lý nhân sự khi chia ca cho bác sĩ (Module Roster).
- **Ảnh hưởng:** Hệ thống phân ca phải Validate dựa trên khung giờ tổng này. (Bác sĩ không thể được phân ca từ 18:00 - 20:00 nếu phòng khám thiết lập đóng cửa lúc 17:30).

### 4. Hệ thống thông báo / Cron Job (Notification System)
- **Tác động:** Các worker ngầm gửi tin nhắn SMS nhắc lịch khách hàng.
- **Ảnh hưởng:** Ngày nghỉ lễ có thể sẽ được dùng để lọc và bỏ qua các luồng thông báo tự động bị sai lệch. 

### 5. Báo cáo & Thống kê (Dashboard & Analytics)
- **Tác động:** Báo cáo công suất hoạt động (Utilization Rate).
- **Ảnh hưởng:** Công thức tính công suất = (Số slot đã book / Tổng số slot khả dụng trong ngày). Khi Admin thay đổi giờ (tăng/giảm tổng số slot), thì mẫu số thay đổi, dẫn đến % công suất của phòng khám bị tính lại. Cần logic chốt số liệu quá khứ, chỉ áp dụng cấu hình mới cho báo cáo tương lai.

---
*(Hết tài liệu)*
