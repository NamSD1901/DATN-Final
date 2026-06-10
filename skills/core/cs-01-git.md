# 🐾 CS-01: Git & GitHub Collaboration (Quy trình Hợp tác Git)

Tài liệu này chi tiết hóa cách thức sử dụng Git và GitHub cho toàn bộ thành viên trong dự án **MyPetClinic** (Nam, Phương, Lâm, Hạnh) nhằm giảm thiểu xung đột mã nguồn (conflict) và đồng bộ hóa hiệu quả.

---

## 1. Các lệnh Git Cơ bản bắt buộc thuộc lòng

Mỗi khi bắt đầu làm việc, hãy mở terminal trong thư mục dự án và thực hiện theo đúng thứ tự:

### Bước 1: Cập nhật code mới nhất từ nhánh chung
Luôn lấy code mới nhất từ nhánh `develop` về máy của bạn trước khi làm bất cứ việc gì:
```bash
git checkout develop
git pull origin develop
```

### Bước 2: Tạo nhánh tính năng mới (Feature Branch)
Nhánh tính năng phải tuân thủ quy tắc đặt tên: `feature/tên-tính-năng` hoặc `feature/mã-user-story` (ví dụ: `PB09` cho User Story đặt lịch).
```bash
git checkout -b feature/PB09-booking-ui
```

### Bước 3: Lưu lại tiến trình làm việc (Commit)
Trong quá trình code, hãy commit thường xuyên sau mỗi phần việc nhỏ hoàn thành. **Đừng để code cả ngày rồi commit 1 lần duy nhất.**
```bash
# Xem các file thay đổi
git status

# Thêm file vào vùng chờ commit
git add .

# Tạo commit với message chuẩn (Xem phần quy tắc bên dưới)
git commit -m "feat(booking): add date-time selection with visual slots"
```

### Bước 4: Đẩy nhánh lên GitHub
```bash
git push origin feature/PB09-booking-ui
```

---

## 2. Quy tắc Đặt tên Commit (Commit Message Convention)

Nhóm thống nhất sử dụng chuẩn **Conventional Commits** để dễ dàng tra cứu lịch sử thay đổi:

```text
<type>(<scope>): <mô tả ngắn bằng tiếng Việt hoặc tiếng Anh không dấu>
```

| Type | Ý nghĩa | Ví dụ thực tế |
| :--- | :--- | :--- |
| **`feat`** | Thêm tính năng mới cho giao diện hoặc API | `feat(booking): add pet selection dropdown` |
| **`fix`** | Sửa một lỗi (bug) | `fix(auth): fix token expiration redirect` |
| **`style`** | Chỉ chỉnh sửa CSS, format code (không đổi logic) | `style(theme): adjust gold gradient button padding` |
| **`refactor`**| Tối ưu cấu trúc code, dọn dẹp code cũ | `refactor(api): extract axios instance configuration` |
| **`docs`** | Cập nhật tài liệu, README, hướng dẫn cài đặt | `docs(readme): update environment setup guide` |
| **`test`** | Thêm unit test hoặc test script | `test(booking): add unit test for BookingService` |
| **`chore`** | Cấu hình dự án, cài package mới | `chore(deps): add lucide-vue-next dependency` |

---

## 3. Quy trình Tránh và Giải Quyết Conflict (Merge/Rebase)

### Cách Rebase để tránh "Merge Hell"
Khi bạn làm việc trên nhánh của mình lâu ngày, nhánh `develop` trên GitHub có thể đã có nhiều code mới của người khác. Trước khi tạo Pull Request, hãy tiến hành **Rebase** để gộp code của mình lên trên code mới nhất của develop:

```bash
# 1. Lấy code mới nhất từ develop về local
git checkout develop
git pull origin develop

# 2. Quay lại nhánh của bạn và rebase
git checkout feature/PB09-booking-ui
git rebase develop
```

### Cách xử lý khi gặp Conflict
Nếu xuất hiện thông báo: `CONFLICT (content): Merge conflict in ...`, hãy bình tĩnh thực hiện:

1. Mở IDE (VS Code) lên, tìm đến file bị báo đỏ.
2. Bạn sẽ thấy các kí tự đánh dấu conflict của Git:
   ```javascript
   <<<<<<< HEAD (Current Change - Code của bạn)
   const API_URL = 'http://localhost:5000/api/v1';
   =======
   const API_URL = 'https://mypetclinic-api.supabase.co/api/v1';
   >>>>>>> develop (Incoming Change - Code mới trên develop)
   ```
3. **Giải quyết:** Thảo luận với người sửa file đó (nếu cần) và chọn giữ lại phiên bản đúng (hoặc gộp cả hai). Xóa các ký tự đánh dấu `<<<<<<<`, `=======`, `>>>>>>>`.
4. Đánh dấu file đã sửa xong và tiếp tục tiến trình:
   ```bash
   git add <đường-dẫn-file-vừa-sửa>
   git rebase --continue
   ```
5. Đẩy đè lên GitHub (vì lịch sử commit đã được rebase làm sạch):
   ```bash
   git push origin feature/PB09-booking-ui --force-with-lease
   ```

---

## 4. Quy trình Tạo Pull Request (PR) & Code Review

1. Sau khi đẩy code lên, truy cập GitHub của dự án và chọn **Compare & pull request**.
2. **Template Pull Request bắt buộc điền:**
   * **Mô tả:** Tóm tắt ngắn gọn những gì bạn đã làm.
   * **Người review (Reviewers):** Tag tối thiểu 1-2 thành viên liên quan (ví dụ: Phương tag Hạnh review giao diện, Nam tag Phương review luồng API).
3. **Quy tắc Review:**
   * Không được tự ý Merge PR của chính mình trừ khi khẩn cấp và đã được đồng thuận.
   * Người Review cần mở code ra kiểm tra kỹ, để lại comment cụ thể nếu cần chỉnh sửa trước khi nhấn **Approve**.

---

## 5. Bài tập Thực Hành Đạt Yêu Cầu CS-01

- [ ] Thực hiện Clone dự án về máy thành công.
- [ ] Tạo nhánh cá nhân đúng chuẩn tên đặt ra.
- [ ] Thực hiện tối thiểu 3 commits với message chuẩn conventional commit.
- [ ] Gặp và tự giải quyết thành công ít nhất 1 lần xung đột code (conflict) giả lập với thành viên khác.
- [ ] Tạo thành công 1 Pull Request lên develop và nhận Approve từ đồng nghiệp.
