# 🐾 CS-07: Documentation & QA Mindset (Tài liệu & Tư duy Chất lượng)

Tài liệu này chi tiết hóa cách viết tài liệu kỹ thuật bằng Markdown và quy chuẩn báo cáo lỗi (Bug Report) nhằm nâng cao chất lượng sản phẩm cho dự án **MyPetClinic**. Đây là kỹ năng tối quan trọng dành cho QA (Lâm, Hạnh) cũng như toàn bộ các thành viên tham gia phát triển.

---

## 1. Hướng dẫn viết tài liệu bằng Markdown

Tất cả các tài liệu trong thư mục `plan/` và `skills/` đều được viết bằng định dạng **Markdown (.md)**. Khi tạo hoặc chỉnh sửa file tài liệu, hãy tuân thủ các quy tắc sau:

- **Phân cấp Tiêu đề hợp lý:** Sử dụng một dấu `#` cho tiêu đề trang lớn duy nhất ở đầu file, `##` cho các phần chính, và `###` cho các phần phụ.
- **Sử dụng Bảng so sánh:** Khi liệt kê các thuộc tính hoặc so sánh công nghệ, hãy vẽ bảng để tối ưu trực quan.
- **Sử dụng Khối mã (Code Blocks):** Ghi rõ ngôn ngữ tương ứng để IDE tự động highlight mã nguồn.
  * Ví dụ: ` ```csharp ` cho code C#, ` ```vue ` cho Vue, ` ```bash ` cho câu lệnh terminal.
- **Sử dụng Hộp thoại Cảnh báo (GitHub Alerts):**
  > [!NOTE]
  > Sử dụng cho các lưu ý ngoài lề hoặc giải thích bổ sung.
  
  > [!IMPORTANT]
  > Sử dụng cho các quy tắc quan trọng bắt buộc phải tuân theo.

---

## 2. Quy chuẩn Báo Cáo Lỗi (Bug Report) tiêu chuẩn

Khi kiểm thử hệ thống và phát hiện hành vi sai lệch, QA hoặc các thành viên cần viết Bug Report rõ ràng để Dev (Nam/Phương) dễ dàng tái hiện lỗi và sửa chữa nhanh chóng.

### Cấu trúc một Bug Report tiêu chuẩn:

```markdown
# 🐛 BUG-XXX: Lỗi không hiển thị khung giờ đặt khám khi đổi bác sĩ

### 1. Môi trường xảy ra lỗi
- **URL:** `http://localhost:5173/bookings` (hoặc Staging link)
- **Trình duyệt:** Google Chrome - Phiên bản 125.0
- **Tài khoản test:** `owner_milo@petclinic.com`

### 2. Các bước tái hiện lỗi (Steps to Reproduce)
1. Truy cập vào trang đặt lịch khám thú cưng.
2. Chọn thú cưng là "Milo".
3. Chọn bác sĩ khám là "BS. Nguyễn Văn A".
4. Chọn ngày khám là ngày mai.
5. Thay đổi bác sĩ khám sang "BS. Trần Thị B".

### 3. Kết quả Thực tế (Actual Result)
- Khung giờ khám bị trống hoàn toàn, console log của trình duyệt báo lỗi: `TypeError: Cannot read properties of undefined (reading 'map')` tại file `BookingSlots.vue:line 35`.

### 4. Kết quả Mong đợi (Expected Result)
- Danh sách khung giờ rảnh của "BS. Trần Thị B" phải được tải lại và hiển thị lên giao diện để người dùng chọn.

### 5. Hình ảnh / Video minh họa
- [Đính kèm ảnh chụp màn hình console log bị đỏ tại đây]
```

---

## 3. Tư duy Phòng ngừa lỗi (Shift-Left Testing)

Tất cả thành viên cần nâng cao ý thức về chất lượng sớm hơn trong chu kỳ phát triển:
- **Tự test trên máy của mình trước:** Trước khi tạo PR, dev phải tự chạy kịch bản thông thường để đảm bảo tính năng không lỗi.
- **QA tham gia từ khâu phân tích:** Lâm và Hạnh cùng tham gia viết Acceptance Criteria (AC) ngay khi thảo luận lập kế hoạch Sprint. Điều này giúp Nam và Phương hiểu rõ tiêu chuẩn bàn giao trước khi bắt đầu viết dòng code đầu tiên.

---

## 4. Bài tập Thực Hành Đạt Yêu Cầu CS-07

- [ ] Tạo mới thành công một file Markdown sạch sẽ, sử dụng đúng phân cấp tiêu đề, bảng biểu và khối code.
- [ ] Viết thành công một Bug Report chi tiết trên GitHub Issues (hoặc công cụ của nhóm) đáp ứng đầy đủ 5 tiêu chí trên.
- [ ] Tham gia xây dựng tiêu chí nghiệm thu (Acceptance Criteria) cho ít nhất 2 User Story trong Sprint Planning.
