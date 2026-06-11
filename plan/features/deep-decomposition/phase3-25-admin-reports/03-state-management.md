# 03. State Management (Pinia Store) - Admin Revenue Reports

Tài liệu thiết kế Vue 3 Pinia Store sử dụng TypeScript cho phân hệ Báo cáo Doanh thu & Hiệu suất.

---

## 1. Vai trò của Pinia Store trong Phân hệ

Pinia Store `useReportStore` chịu trách nhiệm điều phối toàn bộ dữ liệu phục vụ biểu đồ và các thẻ KPI. Store này giữ trạng thái khoảng thời gian lọc hiện hành (`startDate`, `endDate`), quản lý việc gọi đồng thời 3 API báo cáo (KPIs, Xu hướng, Cơ cấu) và định dạng dữ liệu thô nhận được từ Backend thành các cấu trúc mảng mà Chart.js yêu cầu để vẽ biểu đồ trực tiếp trên giao diện Client.

---

## 2. Mã nguồn TypeScript hoàn chỉnh cho Pinia Store

Dưới đây là cài đặt chi tiết của file `useReportStore.ts` triển khai tại thư mục `frontend/src/stores/useReportStore.ts`:

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';

// Định nghĩa kiểu dữ liệu KPIs báo cáo
export interface KpiData {
  totalRevenue: number;
  growthRate: number;
  totalAppointments: number;
  activeCustomersCount: number;
  averageOrderValue: number;
}

// Định nghĩa kiểu dữ liệu biểu đồ xu hướng doanh thu
export interface RevenueTrend {
  labels: string[];
  dataPoints: number[];
  invoiceCounts: number[];
}

// Định nghĩa kiểu dữ liệu cơ cấu doanh thu
export interface RevenueStructure {
  serviceRevenue: number;
  medicineRevenue: number;
  vaccineRevenue: number;
}

interface ReportState {
  startDate: string;
  endDate: string;
  kpis: KpiData | null;
  trend: RevenueTrend | null;
  structure: RevenueStructure | null;
  isLoading: boolean;
  error: string | null;
}

export const useReportStore = defineStore('report', {
  state: (): ReportState => {
    // Mặc định khoảng ngày lọc là từ đầu tháng hiện tại đến ngày hôm nay
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    
    return {
      startDate: firstDay.toISOString().split('T')[0],
      endDate: today.toISOString().split('T')[0],
      kpis: null,
      trend: null,
      structure: null,
      isLoading: false,
      error: null
    };
  },

  getters: {
    // Định dạng dữ liệu thô phục vụ trực tiếp cho Chart.js Line Dataset
    lineChartData(state) {
      if (!state.trend) return null;
      return {
        labels: state.trend.labels,
        datasets: [
          {
            label: 'Doanh thu theo ngày (VND)',
            data: state.trend.dataPoints,
            borderColor: 'hsl(200, 90%, 55%)',
            backgroundColor: 'hsla(200, 90%, 55%, 0.1)',
            tension: 0.3,
            fill: true
          }
        ]
      };
    },

    // Định dạng dữ liệu thô phục vụ trực tiếp cho Chart.js Donut/Pie Dataset
    donutChartData(state) {
      if (!state.structure) return null;
      return {
        labels: ['Phí dịch vụ khám', 'Tiền bán thuốc kê đơn', 'Tiêm phòng vắc-xin'],
        datasets: [
          {
            data: [
              state.structure.serviceRevenue,
              state.structure.medicineRevenue,
              state.structure.vaccineRevenue
            ],
            backgroundColor: [
              'hsl(200, 90%, 55%)', // Xanh lam
              'hsl(45, 95%, 50%)',  // Vàng
              'hsl(280, 75%, 60%)'  // Tím
            ],
            borderWidth: 1,
            borderColor: 'rgba(255, 255, 255, 0.08)'
          }
        ]
      };
    }
  },

  actions: {
    // Cập nhật khoảng ngày lọc nhanh
    setDateRange(start: string, end: string) {
      this.startDate = start;
      this.endDate = end;
      // Sau khi đổi ngày, tự động gọi tải lại báo cáo
      this.fetchAllReports();
    },

    // Gọi đồng thời tất cả các API báo cáo để vẽ biểu đồ và hiển thị KPIs
    async fetchAllReports() {
      this.isLoading = true;
      this.error = null;
      
      const params = {
        startDate: this.startDate,
        endDate: this.endDate
      };

      try {
        const [kpiRes, trendRes, structRes] = await Promise.all([
          axios.get<KpiData>('/api/admin/reports/kpis', { params }),
          axios.get<RevenueTrend>('/api/admin/reports/revenue-trend', { params }),
          axios.get<RevenueStructure>('/api/admin/reports/revenue-structure', { params })
        ]);

        this.kpis = kpiRes.data;
        this.trend = trendRes.data;
        this.structure = structRes.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Lỗi xảy ra khi tải dữ liệu báo cáo.';
        console.error('Error fetching report dashboard data:', err);
      } finally {
        this.isLoading = false;
      }
    },

    // Xuất báo cáo Excel
    async exportReportToExcel() {
      try {
        const response = await axios.get('/api/admin/reports/export', {
          params: { startDate: this.startDate, endDate: this.endDate },
          responseType: 'blob' // Nhận file nhị phân Excel
        });
        
        // Tải file tự động về máy khách
        const blob = new Blob([response.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = `MyPetClinic_Report_${this.startDate}_to_${this.endDate}.xlsx`;
        link.click();
      } catch (err: any) {
        console.error('Không thể xuất báo cáo Excel:', err);
        throw err;
      }
    }
  }
});
```
