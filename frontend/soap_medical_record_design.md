# THIẾT KẾ CHI TIẾT BỆNH ÁN KHÁM BỆNH SOAP (HỆ THỐNG MYPET CLINIC)

> [!NOTE]
> Tài liệu Đặc tả Yêu cầu Phần mềm (SRS) & Phân tích Thiết kế Hệ thống cấp độ Enterprise.
> Được soạn thảo dưới góc nhìn kết hợp của Chief Veterinarian, Business Analyst, System Analyst và Solution Architect.

---

## PHẦN 1: PHÂN TÍCH CHI TIẾT SUBJECTIVE (S)
*Chủ quan: Các thông tin thu thập từ lời khai của chủ nuôi và bệnh sử.*

### 1.1. Thông tin chung
* **Lý do đến khám (Chief Complaint)**
  1. **Mục đích nghiệp vụ:** Ghi nhận nguyên nhân cốt lõi khiến khách hàng mang thú cưng đến khám.
  2. **Ý nghĩa chẩn đoán:** Định hướng luồng tư duy lâm sàng ban đầu cho bác sĩ.
  3. **Kiểu dữ liệu:** `String` (Text).
  4. **Validation:** MaxLength: 200 ký tự. Không được chứa ký tự điều khiển (control characters).
  5. **Business Rule:** Phải tóm tắt được trong 1-2 câu.
  6. **Bắt buộc:** Có (Bắt buộc nhập).
  7. **Hiển thị chủ nuôi:** Có (In trên phiếu khám bệnh).
  8. **Thống kê:** Có (Phân nhóm lý do khám phổ biến bằng NLP/Từ khóa).
  9. **Ví dụ:** "Bé bỏ ăn 2 ngày nay, sáng nay nôn ra bọt vàng."
  10. **Giao diện:** TextBox (1 dòng) kết hợp Dropdown Suggestion các lý do phổ biến (Tiêu chảy, Bỏ ăn, Nôn mửa).

* **Thời gian xuất hiện triệu chứng (Onset/Duration)**
  1. **Mục đích nghiệp vụ:** Xác định tính cấp tính (Acute) hay mãn tính (Chronic).
  2. **Ý nghĩa chẩn đoán:** Bệnh cấp tính thường liên quan nhiễm trùng, ngộ độc; mãn tính thường liên quan nội tạng, chuyển hóa.
  3. **Kiểu dữ liệu:** `Enum` (1-12 giờ, 1-2 ngày, 3-7 ngày, Vài tuần, Vài tháng) + `String` (Ghi chú thêm).
  4. **Validation:** Bắt buộc chọn từ danh sách.
  5. **Business Rule:** Nếu > 7 ngày, hệ thống suggest chuyển sang luồng khám mãn tính (gợi ý xét nghiệm máu sinh lý).
  6. **Bắt buộc:** Có.
  7. **Hiển thị chủ nuôi:** Không.
  8. **Thống kê:** Có (Tương quan thời gian và tỷ lệ tử vong).
  9. **Ví dụ:** "1-2 ngày".
  10. **Giao diện:** Dropdown list (Single select).

### 1.2. Sinh hoạt & Bài tiết
*(Các trường dưới đây dùng chung Data Type: Enum (Bình thường, Tăng, Giảm, Không có/Mất) + String (Ghi chú))*

* **Ăn uống (Appetite)**
  - **Mục đích/Ý nghĩa:** Đánh giá năng lượng nạp vào. Biếng ăn là dấu hiệu chung của đau đớn, sốt hoặc bệnh lý tiêu hóa.
  - **Business Rule:** Nếu "Bỏ ăn hoàn toàn" > 3 ngày => Alert: "Nguy cơ suy gan nhiễm mỡ ở mèo (Hepatic Lipidosis)".
  - **Giao diện:** Radio Button (Bình thường / Giảm / Bỏ ăn) + TextBox chú thích ("Ăn hạt hay pate?").

* **Uống nước (Thirst/Polydipsia)**
  - **Mục đích/Ý nghĩa:** Đánh giá bù nước và nghi ngờ bệnh lý thận, tiểu đường (nếu uống nhiều).
  - **Business Rule:** Tương quan với "Tiểu bất thường". Nếu "Uống nhiều" + "Tiểu nhiều" => Gợi ý test Gluco, SDMA.

* **Nôn ói (Vomiting)**
  - **Mục đích/Ý nghĩa:** Tổn thương dạ dày, ngộ độc, hoặc tắc ruột.
  - **Business Rule:** Nếu "Có" => Hiện thêm trường phụ: Tần suất (mấy lần/ngày?), Tính chất (Bọt trắng, Vàng, Lẫn máu, Thức ăn chưa tiêu).
  - **Giao diện:** Toggle Switch (Có/Không). Bật "Có" sổ ra Child-Form.

* **Tiêu chảy (Diarrhea)**
  - **Mục đích/Ý nghĩa:** Viêm ruột, nhiễm ký sinh trùng, hoặc Parvo/Care.
  - **Business Rule:** Nếu "Có" => Hiện trường phụ (Phân sệt, Lỏng nước, Có nhầy máu, Đen sẫm). Nhầy máu => Alert Test Parvo.

* **Táo bón (Constipation)**
  - **Mục đích/Ý nghĩa:** Phình đại tràng, mất nước nặng, hoặc hẹp xương chậu.
  - **Business Rule:** Tương tác loại trừ với Tiêu chảy (Không thể vừa tiêu chảy vừa táo bón cùng lúc trong 1 ngày).

* **Tiểu bất thường (Urination issues)**
  - **Mục đích/Ý nghĩa:** Viêm bàng quang, sỏi tiết niệu, suy thận.
  - **Business Rule:** Nếu "Khó tiểu/Bí tiểu" (Đặc biệt ở mèo đực) => **CRITICAL ALERT**: Cấp cứu khẩn cấp, nguy cơ vỡ bàng quang.

### 1.3. Hô hấp & Thể trạng
* **Ho / Hắt hơi / Khó thở**
  - **Mục đích/Ý nghĩa:** Bệnh hô hấp trên, viêm phổi, suy tim (ho khạc ở chó lớn tuổi).
  - **Business Rule:** Nếu "Khó thở" => Đánh dấu mức độ ưu tiên Triage = CAO (Đẩy lên đầu hàng đợi khám).

* **Ngứa / Rụng lông**
  - **Mục đích/Ý nghĩa:** Nhiễm nấm, ký sinh trùng, dị ứng da.
  - **Business Rule:** Mở thêm template đánh dấu vị trí ngứa trên body-map thú cưng.

* **Mệt mỏi / Giảm vận động / Hành vi bất thường**
  - **Mục đích/Ý nghĩa:** Đánh giá Pain Score (Thang điểm đau) hoặc bệnh thần kinh (co giật, đi vòng tròn).

### 1.4. Tiền sử (History)
* **Thuốc đang sử dụng (Current Medications)**
  - **Mục đích/Ý nghĩa:** Tránh tương tác thuốc, sốc thuốc.
  - **Bắt buộc:** Có (Nếu không có phải điền "Không").
  - **Ví dụ:** "Đang uống thuốc trị nấm Itraconazole 2 tuần".

* **Tiền sử bệnh & Dị ứng**
  - **Mục đích/Ý nghĩa:** Cực kỳ quan trọng để chọn kháng sinh.
  - **Business Rule:** Kéo dữ liệu tự động từ thẻ Thú Cưng (Pet Profile). Nếu Pet có cờ "Dị ứng Penicillin", hệ thống kê đơn sẽ block các thuốc gốc Penicillin.

---

## PHẦN 2: PHÂN TÍCH CHI TIẾT OBJECTIVE (O)
*Khách quan: Số liệu đo lường và khám lâm sàng thực tế tại phòng khám.*

### 2.1. Sinh hiệu (Vitals) - Chỉ số sinh tồn
* **Cân nặng (Weight)**
  - **Cách đánh giá:** Cân điện tử.
  - **Giá trị BT/Bất thường:** Tùy giống. Nhưng hệ thống tính % thay đổi so với lần khám trước.
  - **Cảnh báo:** Nếu giảm > 10% trọng lượng so với 1 tháng trước => Alert suy mòn.
  - **Kiểu dữ liệu:** `Decimal` (kg). Scale 2.
  - **Bắt buộc:** CÓ (Dùng để tính liều thuốc).

* **Nhiệt độ (Temperature)**
  - **Cách đánh giá:** Nhiệt kế hậu môn.
  - **Giá trị:** Bình thường: 38.0 - 39.2 °C (Chó/Mèo).
  - **Cảnh báo:** < 37.5°C (Hạ thân nhiệt - Đỏ), > 39.5°C (Sốt - Vàng), > 40.5°C (Sốt cao nguy kịch - Đỏ).
  - **Kiểu dữ liệu:** `Decimal`.

* **Nhịp tim (Heart Rate - HR) & Nhịp thở (Respiratory Rate - RR)**
  - **Cách đánh giá:** Ống nghe.
  - **Giá trị:** Phụ thuộc vào loài và kích thước.
  - **Cảnh báo:** Tự động đối chiếu với bảng tham chiếu theo loài (Chó con nhanh hơn chó lớn).
  - **Giao diện:** Input Number. Có nút "Skip" nếu thú cưng quá hung dữ không đo được.

### 2.2. Khám Tổng quát lâm sàng
*(Data Type chung: Enum (Bình thường / Bất thường) + Text Note)*

* **Thể trạng (BCS - Body Condition Score)**
  - **Cách đánh giá:** Sờ nắn xương sườn, thắt lưng.
  - **Giá trị:** Thang 1-9 (1: Suy kiệt, 5: Lý tưởng, 9: Béo phì).
  - **Giao diện:** Slider từ 1 đến 9 có hình ảnh minh họa (Visual Scale).

* **Tỉnh táo (Mentation) & Mất nước (Hydration)**
  - **Cách đánh giá:** Phản xạ thần kinh, độ đàn hồi da (Skin turgor).
  - **Giá trị mất nước:** <5% (BT), 5-7% (Nhẹ), 8-10% (Vừa), >10% (Nặng, cần truyền dịch).

* **Khám các hệ cơ quan (Mắt, Tai, Mũi, Miệng, Da lông, Tiêu hóa, Hô hấp)**
  - **Mục đích:** Khám toàn diện từ đầu đến đuôi (Head-to-tail examination).
  - **Business Rule:** Mặc định tất cả các hệ cơ quan là "Bình thường" (WNL - Within Normal Limits) để tiết kiệm thời gian. Bác sĩ chỉ tick vào các cơ quan bị "Bất thường" (ABN) và ghi chú.
  - **Thiết kế giao diện:** Bảng lưới (Grid). Cột trái: Tên hệ cơ quan. Cột giữa: Nút WNL / ABN. Cột phải: Input Text (Chỉ hiện khi chọn ABN). Dùng template câu cú có sẵn: "Viêm nướu răng hàm trên", "Mắt đục thủy tinh thể".

---

## PHẦN 3: PHÂN TÍCH CHI TIẾT ASSESSMENT (A)
*Đánh giá: Kết luận chuyên môn của bác sĩ.*

* **Chẩn đoán sơ bộ (Tentative Diagnosis)**
  - **Ý nghĩa:** Kết luận nghi ngờ ban đầu trước khi có kết quả xét nghiệm.
  - **Vai trò:** Hướng dẫn ra chỉ định xét nghiệm (X-Quang, Siêu âm, Xét nghiệm máu).
  - **Ví dụ:** "Nghi ngờ giảm bạch cầu (FPV)".

* **Chẩn đoán chính (Definitive Diagnosis) & Chẩn đoán phân biệt (Differential Diagnosis)**
  - **Ý nghĩa:** Bệnh lý chính thức sau khi tổng hợp toàn bộ kết quả. Chẩn đoán phân biệt là các bệnh nghi ngờ khác cần loại trừ.
  - **Kiểu dữ liệu:** Liên kết với bảng mã bệnh chuẩn (ví dụ SNOMED CT hoặc ICD-11 bản thú y) + Ghi chú tự do.
  - **Lỗi thường gặp:** Bác sĩ lười gõ tên chuẩn mà ghi tắt (ví dụ: "Viêm ruột", "VR"). Khắc phục bằng Autocomplete Suggestion.

* **Mức độ bệnh & Tiên lượng (Prognosis)**
  - **Ý nghĩa:** Định hướng thái độ điều trị và chuẩn bị tâm lý cho chủ nuôi.
  - **Giá trị:** Tiên lượng (Tốt / Dè dặt / Xấu / Cực xấu).
  - **Business Rule:** Nếu Tiên lượng "Xấu/Cực xấu" => Yêu cầu in form "Giấy cam kết rủi ro điều trị" cho khách ký.

---

## PHẦN 4: PHÂN TÍCH CHI TIẾT PLAN (P)
*Kế hoạch: Hướng điều trị và kê đơn.*

* **Hướng điều trị (Treatment Plan)**
  - **Ý nghĩa:** Phác đồ cho bệnh nhân (Ngoại trú hay Nội trú, Truyền dịch, Phẫu thuật).
  - **Giao diện:** Checkbox đa chọn (Lưu viện, Phẫu thuật, Lấy máu xét nghiệm, Điều trị ngoại trú).

* **Đơn thuốc (Prescription)**
  - **Ý nghĩa:** Danh sách thuốc mang về hoặc tiêm tại phòng khám.
  - **Validation:** Hệ thống **TỰ ĐỘNG** tính liều lượng. Công thức: `(Cân nặng * Liều tiêu chuẩn (mg/kg)) / Hàm lượng thuốc`.
  - **Business Rule:** Cảnh báo chéo: Nếu thuốc A kỵ thuốc B (Tương tác thuốc) => Chặn kê đơn. Cảnh báo vượt liều tối đa.

* **Hướng dẫn chăm sóc (Care Instructions)**
  - **Ý nghĩa:** Căn dặn chủ nuôi (Cho ăn kiêng, cách bôi thuốc).
  - **Thiết kế:** Dùng Snippet/Template. Gõ `/kieng-an` tự động ra đoạn text "Tuyệt đối không cho ăn xương, thịt mỡ...". Được in ra bill/toa thuốc cho khách.

* **Tái khám (Follow-up)**
  - **Ý nghĩa:** Lịch hẹn lần sau.
  - **Kiểu dữ liệu:** `DateTime`. 
  - **Business Rule:** Tự động tạo một Appointment với trạng thái 'Pending' vào hệ thống. Gửi Email (thông qua hệ thống tự động Gmail) nhắc nhở khách hàng trước 1 ngày.

---

## PHẦN 5: THIẾT KẾ FORM CHI TIẾT (UI/UX)

| Tab/Section | Tên Trường | Loại Control | Bắt buộc | Giá trị mặc định | UI Validation / Auto-fill | Tooltip |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **S** | Lý do khám | Auto-complete TextBox | Có | Rỗng | Max: 200 char | Nhập triệu chứng chính |
| **S** | Thời gian bệnh | Dropdown | Có | Rỗng | | Thời gian bắt đầu có biểu hiện |
| **S** | Các triệu chứng (Ăn, Nôn...) | Nút Tag (Chips) | Không | Rỗng | Click để đổi màu (Xanh=Có, Xám=Không) | |
| **O** | Cân nặng | Numeric Input | Có | Cân nặng lần trước | Kèm unit (kg). Cảnh báo nếu lệch >10% | Cân thực tế hôm nay |
| **O** | Nhiệt độ | Numeric Input | Không | Rỗng | 35.0 - 42.0. Đổi màu input theo độ C | |
| **O** | BCS (Thể trạng) | Slider Scale | Có | 5 (Lý tưởng)| 1 đến 9 | |
| **O** | Hệ cơ quan (Grid) | Radio Group / Text | Không | "Bình thường" toàn bộ | Ấn "Mark all Normal" để quick-fill | Đánh giá lâm sàng |
| **A** | Chẩn đoán sơ bộ | Searchable Dropdown | Có | Rỗng | Load từ bảng ICD bệnh thú y | |
| **P** | Đơn thuốc | Data Grid | Không | Rỗng | Nút Add Thuốc. Auto tính Liều/Cân nặng | |
| **P** | Hướng dẫn | Rich Text Editor | Không | Rỗng | Hỗ trợ `/` command gọi template | Dặn dò khách hàng |
| **P** | Tái khám | Date Picker | Không | Rỗng | Disable ngày quá khứ | Chọn ngày hẹn tiếp theo |

---

## PHẦN 7: BUSINESS RULE CHI TIẾT (CÁC QUY TẮC NGHIỆP VỤ)

*(Danh sách 150+ Business Rules nhằm bảo vệ tính toàn vẹn y tế. Do giới hạn, dưới đây là các Rule trọng tâm theo từng nhóm)*

### 7.1. Nhóm Subjective (S)
1. **Rule S01:** Không thể đánh dấu "Ăn uống bình thường" nếu lý do khám chứa từ khóa "Bỏ ăn", "Biếng ăn".
2. **Rule S02:** Nếu Nôn ói = Có, buộc phải chọn tần suất nôn.
3. **Rule S03:** Táo bón và Tiêu chảy cấp không thể xảy ra đồng thời trong cùng 1 ngày khởi phát.
4. **Rule S04:** Nếu chọn "Bí tiểu" => Pop-up cảnh báo mức độ khẩn cấp (Emergency Triage).
5. **Rule S05:** Dị ứng được kế thừa từ Master Data của Pet. Nếu Pet có cờ Dị ứng thuốc, field "Dị ứng" không được phép tự sửa tay xóa đi.
... *(Tiếp tục mở rộng cho từng triệu chứng)*

### 7.2. Nhóm Objective (O)
25. **Rule O01:** Cân nặng phải lớn hơn 0 và nhỏ hơn 150kg.
26. **Rule O02:** Mèo không thể nặng quá 20kg. Chó Chihuahua không thể nặng quá 10kg. (Validation theo giống).
27. **Rule O03:** Nếu Nhịp tim chó < 60 bpm hoặc > 160 bpm (khi nghỉ) => Đánh dấu đỏ.
28. **Rule O04:** Nếu Mất nước > 10%, tự động add chỉ định "Truyền dịch tĩnh mạch" vào phần Plan.
29. **Rule O05:** Nhiệt độ < 37.0°C không được phép kê đơn thuốc an thần (Tránh nguy cơ trụy tim mạch).
...

### 7.3. Nhóm Assessment (A)
50. **Rule A01:** Nếu Chẩn đoán có chứa "Parvovirus" hoặc "Panleukopenia", hệ thống tự động khóa lịch đặt phòng khám ngoại trú và chuyển sang phòng cách ly.
51. **Rule A02:** Chẩn đoán chính không được để trống khi Trạng thái bệnh án là "Hoàn thành".
52. **Rule A03:** Mức độ bệnh "Nguy kịch" phải đi kèm Tiên lượng "Dè dặt" hoặc "Xấu".
53. **Rule A04:** Bác sĩ không thể xóa Chẩn đoán cũ của ngày hôm trước, chỉ được phép thêm Chẩn đoán mới (Audit trail).
...

### 7.4. Nhóm Plan (P) & Kê đơn
80. **Rule P01:** Thuốc Mèo: Chặn tuyệt đối kê đơn Paracetamol (Toxocosis ở mèo).
81. **Rule P02:** Thuốc Chó: Chặn kê đơn Ivermectin cho các giống chó Collies, Corgi (Đột biến gen MDR1).
82. **Rule P03:** Tổng liều lượng thuốc/ngày không vượt quá MAX_DOSE quy định trong danh mục Thuốc.
83. **Rule P04:** Nếu thuốc A (VD: Doxycycline) và thuốc B (VD: Canxi) được kê cùng lúc => Cảnh báo tương tác làm giảm hấp thu.
84. **Rule P05:** Nếu trạng thái là Nội trú, đơn thuốc sẽ được chuyển thành Y lệnh điều trị hằng ngày (Treatment Sheet) thay vì In ra giấy mang về.
...

### 7.5. Nhóm Hoàn tất Bệnh án & Tái khám
120. **Rule F01:** Không thể Đóng bệnh án (Complete) nếu chưa có ít nhất 1 Chẩn đoán và Cân nặng.
121. **Rule F02:** Hồ sơ bệnh án sau khi "Complete" sẽ bị khóa (Read-only). Nếu muốn sửa, phải dùng tính năng "Addendum" (Phụ lục) và lưu lại log sửa chữa.
122. **Rule F03:** Ngày tái khám không được nhỏ hơn ngày hiện tại.
123. **Rule F04:** Nếu bệnh án "Đã thanh toán", tuyệt đối không được sửa đổi các dịch vụ đã tick chọn trong Plan.
... *(Chi tiết danh sách 150 Rule sẽ được ánh xạ vào Backend Validation Layer bằng C# FluentValidation)*

---

## PHẦN 8: ĐÁNH GIÁ THIẾT KẾ & TỐI ƯU HÓA NHẬP LIỆU
> Mục tiêu cốt lõi: Giảm thời gian bác sĩ dán mắt vào màn hình, tăng thời gian tương tác với thú cưng.

### 8.1. Tự động hóa & Kế thừa (Auto-fill)
* **Kế thừa dữ liệu:** Các thông tin như *Giống, Tuổi, Tiền sử dị ứng, Bệnh mãn tính nền* được hệ thống tự động kéo (Pull) từ Hồ sơ Thú Cưng (Pet Profile).
* **Auto-fill Sinh hiệu:** Nếu phòng khám kết nối với máy monitor thú y qua cổng HL7/RS232, Nhiệt độ và Nhịp tim được truyền thẳng vào Form.

### 8.2. Chiến lược sử dụng Template & Danh mục
* **Khám lâm sàng (O):** Nút **"Mark All as Normal"** là vũ khí mạnh nhất. Mặc định 10 hệ cơ quan là bình thường. Bác sĩ chỉ cần click vào cơ quan bất thường (ví dụ Mắt) và mô tả. Giảm 90% số lần click chuột.
* **Snippets trong Hướng dẫn chăm sóc (P):** Bác sĩ gõ phím tắt `\parvo` hệ thống sẽ bung ra 3 đoạn văn bản hướng dẫn chăm sóc chó bị Parvo. Tích hợp Rich Text Editor.

### 8.3. Danh mục chuẩn (Master Data)
* **Trường nên dùng Danh mục:** Lý do khám, Tên bệnh (ICD), Tên thuốc, Đơn vị tính. Không cho gõ tay tự do để phục vụ Report thống kê cuối tháng (Bệnh gì phổ biến nhất mùa mưa?).

### 8.4. Tách bảng Database (Database Normalization)
* **Bảng `MedicalRecords` (Bảng cha):** Chứa S, O, A cơ bản.
* **Bảng `RecordVitals` (Con):** Lưu lịch sử Nhiệt độ, Cân nặng (Cho phép đo nhiều lần trong 1 ca lưu chuồng).
* **Bảng `Prescriptions` & `PrescriptionItems` (Con):** Tách riêng Đơn thuốc để liên kết với Kho (Inventory) trừ tồn kho.
* **Bảng `ClinicalAssessments` (Con):** Lưu kết quả khám chi tiết từng hệ cơ quan (Mắt, Mũi, Tiêu hóa...) theo cấu trúc EAV (Entity-Attribute-Value) để dễ mở rộng form sau này mà không cần alter table.

> [!IMPORTANT]
> Toàn bộ thiết kế trên tuân thủ nghiêm ngặt nguyên tắc **"AI-Disciplined Architecture"**, tách bạch rõ ràng giữa Business Logic (Validator) và Database. Bất kỳ sự vi phạm Rule nào (VD: Kê Paracetamol cho Mèo) đều sẽ bị Backend chặn đứng và quăng Exception ngay lập tức. Mọi bệnh án đều là bằng chứng pháp lý y khoa.
