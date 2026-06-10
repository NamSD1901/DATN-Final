# 🎭 Behavioral Specification - Automatic Notification Service

## 1. Biểu đồ Trạng thái Vue Notification Component

```mermaid
stateDiagram-v2
    [*] --> Closed : Mặc định dropdown đóng
    Closed --> FetchingNotifications : Nhấp chuông mở dropdown
    FetchingNotifications --> EmptyList : Thành công & Không có tin
    FetchingNotifications --> RenderList : Thành công & Có tin nhắn
    RenderList --> MarkingItemRead : Click vào 1 tin chưa đọc
    MarkingItemRead --> RenderList : API 200 & Đổi class CSS sang đã đọc
    RenderList --> Closed : Nhấp vùng ngoài dropdown
```

---

## 2. Ràng buộc Hành vi & Quy tắc Giao tiếp thời gian thực
- **SignalR Real-time Push:** Khi SignalR Hub phát đi tin nhắn `ReceiveNotification`, client ngay lập tức đẩy thông báo mới vào đầu mảng `list` trong Store, tăng số lượng badge chuông lên 1 và phát tiếng chuông thông báo (sound alert) tùy chọn.
- **Empty State UI:** Nếu danh sách trống, hiển thị hình vẽ minh họa (illustration) tinh tế kèm dòng chữ *"Bạn hiện không có thông báo nào mới."*.
