# 📖 Operator & Developer Documentation - Admin Revenue Reports

Tài liệu hướng dẫn vận hành (dành cho Admin) và tài liệu tích hợp/debug kỹ thuật (dành cho Lập trình viên).

---

## 1. Hướng dẫn Vận hành dành cho Admin (Operator Guide)

### Hướng dẫn Xem và Xuất Báo cáo Tài chính
1. Đăng nhập hệ thống MyPetClinic bằng tài khoản Quản trị viên (`Admin`).
2. Nhấp chọn tab **Báo cáo tài chính** trên thanh điều hướng chính.
3. Sử dụng bộ chọn khoảng ngày (Date Range Picker) ở góc trái để chọn thời gian cần thống kê:
   - Có thể click chọn nhanh các nút: **Tháng này**, **Tháng trước**, hoặc **Năm nay** để hệ thống tự động điền ngày lọc.
4. Chờ 1 giây để các biểu đồ Line Chart (Xu hướng doanh thu) và Donut Chart (Cơ cấu nguồn thu) tự động cập nhật và dựng hình vẽ.
5. Rê chuột qua các điểm mốc trên biểu đồ để xem số tiền doanh thu chi tiết của ngày đó.
6. Muốn tải file dữ liệu thô phục vụ đối soát thuế hoặc kế toán:
   - Click nút **[Xuất báo cáo Excel]** ở góc phải màn hình.
   - Trình duyệt sẽ tải xuống file `.xlsx` chứa chi tiết doanh số tổng hợp, doanh số thuốc, phí khám và danh sách hóa đơn tương ứng.

---

## 2. Hướng dẫn Kỹ thuật dành cho Developer (Developer Guide)

### Hướng dẫn Cài đặt và tích hợp Chart.js trên Vue 3 SPA
Để dựng biểu đồ mờ kính, chúng ta sử dụng thư viện **Chart.js** kết hợp với thư viện wrapper **vue-chartjs**:

1. Cài đặt các thư viện cần thiết thông qua NPM (Lưu vào `dependencies`):
   ```bash
   npm install chart.js vue-chartjs
   ```
2. Mã nguồn Vue 3 Component mẫu tích hợp Donut Chart trong [RevenueStructure.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/dashboard/RevenueStructure.vue):
   ```vue
   <template>
     <div class="chart-container glass-card">
       <h3>Cơ cấu nguồn thu</h3>
       <Doughnut v-if="chartData" :data="chartData" :options="chartOptions" />
       <div v-else class="skeleton-shimmer chart-placeholder"></div>
     </div>
   </template>

   <script lang="ts">
   import { defineComponent, computed } from 'vue';
   import { Doughnut } from 'vue-chartjs';
   import { Chart as ChartJS, Title, Tooltip, Legend, ArcElement, CategoryScale } from 'chart.js';
   import { useReportStore } from '@/stores/useReportStore';

   ChartJS.register(Title, Tooltip, Legend, ArcElement, CategoryScale);

   export default defineComponent({
     name: 'RevenueStructure',
     components: { Doughnut },
     setup() {
       const reportStore = useReportStore();

       const chartData = computed(() => reportStore.donutChartData);

       const chartOptions = {
         responsive: true,
         maintainAspectRatio: false,
         plugins: {
           legend: {
             position: 'bottom' as const,
             labels: { color: 'hsl(210, 20%, 80%)' }
           }
         }
       };

       return { chartData, chartOptions };
     }
   });
   </script>
   ```

### Các lệnh cURL Kiểm thử API Thủ công (API Debugging)

#### 1. Lấy KPIs tài chính tổng hợp
```bash
curl -X GET "https://localhost:5001/api/admin/reports/kpis?startDate=2026-05-01&endDate=2026-05-31" \
     -H "Authorization: Bearer <ADMIN_JWT_TOKEN>"
```

#### 2. Lấy dữ liệu biểu đồ Donut (Cơ cấu doanh thu)
```bash
curl -X GET "https://localhost:5001/api/admin/reports/revenue-structure?startDate=2026-05-01&endDate=2026-05-31" \
     -H "Authorization: Bearer <ADMIN_JWT_TOKEN>"
```

---

## 3. Khắc phục Sự cố Thường gặp (Troubleshooting)

### Sự cố: Lệch số liệu báo cáo do chênh lệch múi giờ (Timezone Offset Gap)
- **Triệu chứng:** Doanh thu của ngày hôm nay bị đẩy sang ngày mai, hoặc doanh thu cuối ngày hôm qua bị mất trong báo cáo.
- **Nguyên nhân:** Database lưu trữ ngày thanh toán (`PaidAt`) theo múi giờ chuẩn **UTC**. Khi Admin lọc theo giờ địa phương Việt Nam (**GMT+7**), nếu không convert múi giờ trước khi GroupBy, các hóa đơn thanh toán trong khoảng từ 17h đến 24h sẽ bị nhảy sang ngày tiếp theo ở múi giờ UTC.
- **Giải pháp khắc phục:**
  - Luôn thực hiện chuyển đổi múi giờ sang GMT+7 tại câu truy vấn PostgreSQL hoặc ép kiểu Date theo múi giờ địa phương của phòng khám trước khi gom nhóm:
    ```csharp
    // C# LINQ convert timezone sang GMT+7 trong DbContext
    var rawData = await _context.Invoices
        .Where(i => i.Status == "Paid" && i.PaidAt >= startDate && i.PaidAt <= endDate)
        .GroupBy(i => EF.Functions.DateTrunc("day", EF.Property<DateTime>(i, "PaidAt").AddHours(7)))
        .Select(...)
        .ToListAsync();
    ```
