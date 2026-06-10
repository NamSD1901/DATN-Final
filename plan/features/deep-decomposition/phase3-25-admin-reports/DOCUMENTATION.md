# 📖 Technical Documentation - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **FE-F01 (Library Integration):** Tích hợp và cấu hình thư viện biểu đồ bên thứ ba (`chart.js`, `vue-chartjs`).
- **BE-C01 (EF Core Performance):** Đảm bảo dịch câu lệnh LINQ xuống SQL chạy trực tiếp tại DB Server thay vì kéo dữ liệu về bộ nhớ Client (In-memory execution).

---

## 1. Cài đặt thư viện Biểu đồ phía Frontend

Để vẽ các biểu đồ doanh thu trực quan, chúng ta sử dụng thư viện `chart.js` cùng lớp bọc Vue là `vue-chartjs`.

### 1.1. Lệnh cài đặt
Chạy lệnh sau tại thư mục `frontend`:
```bash
npm install chart.js vue-chartjs
```

### 1.2. Đăng ký các thành phần biểu đồ (Chart.js Registration)
Để giảm kích thước bundle (Tree shaking), cần đăng ký đúng các module cần thiết của Chart.js tại Vue Component:

```typescript
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale,
  ArcElement
} from 'chart.js';

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale,
  ArcElement
);
```

---

## 2. Các điểm lưu ý đặc biệt về Hiệu năng Backend (EF Core Warnings)

> [!WARNING]
> **Tránh In-memory Collection Materialization**
> Khi thực hiện các phép tính tổng hợp (Aggregate functions) như `.Sum()`, `.Count()`, hoặc nhóm `.GroupBy()` trên hàng triệu dòng dữ liệu:
>
> - **KHÔNG ĐƯỢC** gọi `.ToList()` hoặc `.ToListAsync()` trước khi áp dụng `.Sum()` hoặc `.GroupBy()`. Nếu gọi `.ToList()` trước, EF Core sẽ tải toàn bộ danh sách hóa đơn từ Database vào RAM của Web Server rồi mới tính tổng, gây ra lỗi **Out Of Memory (OOM)** khi lượng dữ liệu lớn.
> - **ĐÚNG:** Luôn thực hiện `.SumAsync(i => i.TotalAmount)` hoặc `.GroupBy()` trực tiếp trên đối tượng `IQueryable` để câu lệnh biên dịch trực tiếp sang hàm `SUM()` và `GROUP BY` trong SQL, thực thi trực tiếp tại SQL Server và chỉ trả về kết quả cuối cùng.
