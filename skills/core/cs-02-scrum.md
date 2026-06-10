# 🐾 CS-02: Agile & Scrum Mindset (Mô hình Phát triển Agile/Scrum)

Tài liệu này hướng dẫn cách nhóm **MyPetClinic** (Nam, Phương, Lâm, Hạnh) vận hành theo mô hình Agile/Scrum rút gọn để đảm bảo tiến độ dự án bàn giao đúng hạn sau mỗi 2 tuần.

---

## 1. Các Sự Kiện Scrum (Scrum Events) Trong Dự Án

Chúng ta tổ chức dự án theo chu kỳ phát triển **Sprint kéo dài 2 tuần**. Mỗi Sprint sẽ có 4 sự kiện chính mà tất cả thành viên bắt buộc phải tham gia:

```
┌─────────────────────────────────────────────────────────────┐
│  DAY 1: SPRINT PLANNING (Lập kế hoạch Sprint - 1.5 giờ)       │
│  • Lâm (PO) trình bày danh sách tính năng cần làm.          │
│  • Nhóm thảo luận, ước lượng độ khó và cam kết khối lượng.   │
├─────────────────────────────────────────────────────────────┤
│  DAY 1-10: DAILY STANDUP (Họp nhanh hàng ngày - 15 phút)      │
│  • Diễn ra vào lúc 8:30 AM hàng ngày qua kênh chat/meet.     │
│  • Trả lời nhanh 3 câu hỏi (Xem template bên dưới).          │
├─────────────────────────────────────────────────────────────┤
│  DAY 10: SPRINT REVIEW (Đánh giá Sprint - 1 giờ)              │
│  • Demo sản phẩm chạy thực tế cho cả nhóm và giảng viên xem. │
│  • Nhận phản hồi để điều chỉnh trong các bước tiếp theo.    │
├─────────────────────────────────────────────────────────────┤
│  DAY 10: SPRINT RETROSPECTIVE (Họp cải tiến - 1 giờ)           │
│  • Cả nhóm ngồi lại chia sẻ: Điều gì tốt? Điều gì chưa tốt? │
│  • Đề xuất hành động cụ thể để cải tiến cho Sprint sau.      │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Bản Chuẩn Bị Daily Standup (Mẫu báo cáo)

Để tránh họp lan man, trước 8:30 AM mỗi ngày, từng thành viên hãy tự trả lời ngắn gọn theo mẫu sau:

```markdown
# Daily Standup - [Tên của bạn] - [Ngày hôm nay]

### 1. Hôm qua tôi đã làm gì?
- [x] Thiết kế xong API đăng ký thú cưng (Nam)
- [x] Code xong giao diện Form nhập liệu (Phương)

### 2. Hôm nay tôi sẽ làm gì?
- [ ] Tích hợp API của Nam vào form giao diện (Phương & Nam)
- [ ] Viết test case cho tính năng này (Lâm/Hạnh)

### 3. Tôi có gặp khó khăn (Blocker) gì không?
- ⚠️ Chưa có (Hoặc: Có lỗi kết nối database PostgreSQL trên Supabase từ tối qua chưa giải quyết được).
```

---

## 3. Định Nghĩa Hoàn Thành (Definition of Done - DoD)

Một công việc (User Story/Task) trên bảng quản lý (Jira/Trello) **chỉ được kéo sang cột DONE** khi đạt đủ các tiêu chí dưới đây:

### Tiêu chí về Code & Giao diện:
- [ ] Code không lỗi biên dịch (`dotnet build` thành công, `npm run build` không báo lỗi TypeScript).
- [ ] Không chứa mã cứng (hardcoded colors) - bắt buộc sử dụng CSS variables trong `style.css`.
- [ ] Responsive hoạt động tốt trên cả điện thoại di động và máy tính (desktop).

### Tiêu chí về Kiểm thử & Chất lượng:
- [ ] Đã kiểm thử thủ công đạt hết các điều kiện chấp nhận (Acceptance Criteria).
- [ ] Code đã qua review chéo và được Approve trên GitHub bởi ít nhất 1 thành viên khác.

### Tiêu chí về Bàn giao & Triển khai:
- [ ] Đã cập nhật tài liệu hướng dẫn hoặc cập nhật Swagger API (nếu có API mới).
- [ ] Đã merge code thành công vào nhánh `develop`.

---

## 4. Bài tập Thực Hành Đạt Yêu Cầu CS-02

- [ ] Tham gia đầy đủ các buổi họp Sprint Planning, Review, Retrospective.
- [ ] Cập nhật trạng thái Task cá nhân trên bảng công việc hàng ngày.
- [ ] Báo cáo Daily Standup ngắn gọn, rõ ràng, nêu đúng blocker nếu có.
- [ ] Tuân thủ nghiêm ngặt DoD trước khi bàn giao task.
